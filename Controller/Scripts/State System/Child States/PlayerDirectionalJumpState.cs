using UnityEngine;

public class PlayerDirectionalJumpState : MainState
{
    private Vector2 launchDirection;
    private float launchForce;
    private float jumpDuration;
    private bool preservePerpendicularVelocity;
    
    public PlayerDirectionalJumpState(PlayerMain player, PlayerStateMachine stateMachine, PlayerMain.AnimName animEnum, PlayerData playerData) : base(player, stateMachine, animEnum, playerData)
    {
    }

    public void SetLaunchParameters(Vector2 direction, float force, float duration = 0.3f, bool preservePerpendicular = true)
    {
        launchDirection = direction.normalized;
        launchForce = force;
        jumpDuration = duration;
        preservePerpendicularVelocity = preservePerpendicular;
    }

    public override void Enter()
    {
        base.Enter();
        
        player.Animator.SetBool(_animEnum.ToString(), true);
        
        Vector2 launchVelocity = launchDirection * launchForce;
        
        if (preservePerpendicularVelocity)
        {
            Vector2 perpendicular = new Vector2(-launchDirection.y, launchDirection.x);
            float perpendicularSpeed = Vector2.Dot(rigidbody.linearVelocity, perpendicular);
            launchVelocity += perpendicular * perpendicularSpeed;
        }
        
        rigidbody.linearVelocity = launchVelocity;
        rigidbody.gravityScale = playerData.Jump.Physics2DGravityScale;
        
        playerData.Jump.JumpBufferTimer = 0;
        playerData.Jump.NewJump = false;
    }

    public override void Update()
    {
        base.Update();
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        rigidbody.linearVelocity += playerData.Physics.Platform.DampedVelocity;
    }

    public override void Exit()
    {
        base.Exit();
        player.Animator.SetBool(_animEnum.ToString(), false);
    }

    public override void PhysicsCheck()
    {
        base.PhysicsCheck();
    }

    public override void SwitchStateLogic()
    {
        base.SwitchStateLogic();

        if (localTime >= jumpDuration || (playerData.Physics.IsOnHeadBump && playerData.Physics.CanBumpHead))
        {
            stateMachine.ChangeState(player.LandState);
        }
        else if (inputManager.Input_Dash && playerData.Dash.DashCooldownTimer <= 0f)
        {
            stateMachine.ChangeState(player.DashState);
        }
        else if (playerData.Physics.IsNextToWall)
        {
            if (inputManager.Input_WallGrab)
            {
                stateMachine.ChangeState(player.WallGrabState);
            }
            else if (localTime > playerData.Jump.IgnoreWallSlideTime && inputManager.Input_Walk != 0 && Mathf.Sign(inputManager.Input_Walk) == Mathf.Sign(playerData.Physics.WallDirection))
            {
                stateMachine.ChangeState(player.WallSlideState);
            }
        }
    }
}
