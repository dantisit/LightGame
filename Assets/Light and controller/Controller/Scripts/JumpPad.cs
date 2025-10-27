using System;
using UnityEngine;

/// <summary>
/// Jump pad component that triggers a jump when the player lands on it.
/// Works with the state-based player controller system by initiating jump state transitions.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class JumpPad : MonoBehaviour
{
    [Header("Jump Pad Settings")]
    [SerializeField, Tooltip("Which jump to trigger (1 = first jump, 2 = double jump, 3 = triple jump)")]
    [Range(1, 3)]
    private int jumpIndex = 1;
    
    [SerializeField, Tooltip("Multiplier for the jump height (1.0 = normal jump height)")]
    [Range(0.5f, 10f)]
    private float jumpHeightMultiplier = 1.5f;
    
    [SerializeField, Tooltip("Optional horizontal velocity boost multiplier")]
    [Range(0f, 2f)]
    private float horizontalBoostMultiplier = 1.2f;
    
    [SerializeField, Tooltip("Cooldown time before the same player can use this pad again")]
    private float cooldownTime = 0.5f;
    
    [Header("Layer Settings")]
    [SerializeField, Tooltip("Layer mask for objects that can use the jump pad")]
    private LayerMask targetLayers = -1;
    
    [Header("Events")]
    [Tooltip("Called when a player lands on the jump pad (before bounce)")]
    public Action<PlayerMain> OnPlayerLand;
    
    [Tooltip("Called when the jump force is applied to the player")]
    public Action<PlayerMain> OnPlayerBounce;
    
    [Tooltip("Called when the cooldown period completes")]
    public Action OnCooldownComplete;
    
    [Tooltip("Called for visual/audio feedback (same as OnPlayerBounce but without player parameter)")]
    public Action OnBounce;
    
    // Internal state
    private float lastBounceTime = -999f;
    private PlayerMain lastPlayer;
    private bool cooldownCompleted = true;
    
    /// <summary>
    /// Returns true if the jump pad is ready to be used (not on cooldown)
    /// </summary>
    public bool IsActive => cooldownCompleted;
    
    private void OnValidate()
    {
        // Ensure we have a collider and it's NOT set as a trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col != null && col.isTrigger)
        {
            col.isTrigger = false;
            Debug.LogWarning($"JumpPad on {gameObject.name}: Collider2D was set as trigger. Auto-corrected to non-trigger.");
        }
    }
    
    private void Update()
    {
        // Check if cooldown has completed
        if (!cooldownCompleted && Time.time - lastBounceTime >= cooldownTime)
        {
            cooldownCompleted = true;
            OnCooldownComplete?.Invoke();
        }
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if enough time has passed since last bounce
        if (Time.time - lastBounceTime < cooldownTime)
            return;
        
        // Check if the colliding object is on the target layer
        if (((1 << collision.gameObject.layer) & targetLayers) == 0)
            return;
        
        // Try to get the PlayerMain component
        PlayerMain player = collision.gameObject.GetComponent<PlayerMain>();
        if (player == null)
            return;
        
        // Check if player is coming from above (landing on the pad)
        Vector2 contactNormal = collision.contacts[0].normal;
        if (Vector2.Dot(contactNormal, Vector2.down) < 0.5f)
            return; // Player didn't land on top of the pad
        
        // Invoke land event
        OnPlayerLand?.Invoke(player);
        
        ApplyJumpForce(player, collision.rigidbody);
    }
    
    
    private void ApplyJumpForce(PlayerMain player, Rigidbody2D rb)
    {
        if (player == null || rb == null)
            return;
        
        // Validate jump index
        int clampedJumpIndex = Mathf.Clamp(jumpIndex, 1, player.PlayerData.Jump.Jumps.Count);
        if (clampedJumpIndex != jumpIndex)
        {
            Debug.LogWarning($"JumpPad on {gameObject.name}: Invalid jump index {jumpIndex}. Using index {clampedJumpIndex}.");
        }
        
        // Set up jump parameters for the state machine
        player.PlayerData.Jump.NextJumpInt = clampedJumpIndex;
        player.PlayerData.Jump.NewJump = true;
        
        // Reset coyote time to ensure jump triggers
        player.PlayerData.Jump.CoyoteTimeTimer = player.PlayerData.Jump.CoyoteTimeMaxTime;
        
        // Transition to jump state
        player._stateMachine.ChangeState(player.JumpState);
        
        // Set the velocity multiplier AFTER transition and apply velocity directly
        var jumpState = player.JumpState as PlayerJumpState;
        if (jumpState != null)
        {
            jumpState.VelocityMultiplier = jumpHeightMultiplier;
        }
        
        // Get jump info and apply boosted velocity directly
        var jumpInfo = player.PlayerData.Jump.Jumps[clampedJumpIndex - 1];
        float targetJumpVelocity = jumpInfo.MaxHeight * jumpHeightMultiplier;
        
        // Apply velocity with horizontal boost
        float horizontalVelocity = rb.linearVelocity.x * horizontalBoostMultiplier;
        rb.linearVelocity = new Vector2(horizontalVelocity, targetJumpVelocity);
        
        // Update cooldown
        lastBounceTime = Time.time;
        lastPlayer = player;
        cooldownCompleted = false;
        
        // Invoke bounce events
        OnPlayerBounce?.Invoke(player);
        OnBounce?.Invoke();
    }
    
    private void OnDrawGizmos()
    {
        // Draw a visual indicator in the editor
        Gizmos.color = Color.yellow;
        Vector3 position = transform.position;
        
        // Draw arrow showing jump direction
        float visualHeight = jumpHeightMultiplier * 2f;
        Gizmos.DrawLine(position, position + Vector3.up * visualHeight);
        Gizmos.DrawLine(position + Vector3.up * visualHeight, 
                       position + Vector3.up * visualHeight + Vector3.left * 0.2f);
        Gizmos.DrawLine(position + Vector3.up * visualHeight, 
                       position + Vector3.up * visualHeight + Vector3.right * 0.2f);
    }
    
    private void OnDrawGizmosSelected()
    {
        // Draw more detailed gizmo when selected
        Gizmos.color = Color.green;
        Vector3 position = transform.position;
        
        // Draw force visualization
        float visualHeight = jumpHeightMultiplier * 2f;
        Gizmos.DrawWireSphere(position + Vector3.up * visualHeight, 0.3f);
    }
}
