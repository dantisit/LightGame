using System.Collections.Generic;
using Light_and_controller.Scripts.Components;
using UnityEngine;

/// <summary>
/// SIMPLER APPROACH: Uses multiple dissolve amounts for different regions.
/// Each physics block controls one float value in the shader.
/// </summary>
public class SimpleBlockToSpriteSync : MonoBehaviour
{
    [System.Serializable]
    public class BlockRegion
    {
        public ObjectHider objectHider;
        public Rect uvRegion; // Which part of sprite (0-1 coordinates)
        [HideInInspector] public float currentDissolve = 0f;
    }

    [Header("References")]
    [SerializeField] private SpriteRenderer wallSprite;
    [SerializeField] private List<BlockRegion> blockRegions = new List<BlockRegion>();

    [Header("Dissolve Settings")]
    [SerializeField] private float dissolveSpeed = 2f;

    private Material _spriteMaterial;
    private Vector4[] _regionData; // x,y,width,height for each region

    private void Start()
    {
        if (wallSprite == null)
        {
            Debug.LogWarning("Wall sprite not assigned on " + gameObject.name);
            return;
        }

        _spriteMaterial = wallSprite.material;

        if (blockRegions.Count == 0)
        {
            Debug.LogWarning("No block regions configured on " + gameObject.name);
            return;
        }

        // Setup arrays
        _regionData = new Vector4[blockRegions.Count];

        // Initialize regions
        for (int i = 0; i < blockRegions.Count; i++)
        {
            var region = blockRegions[i];
            _regionData[i] = new Vector4(
                region.uvRegion.x,
                region.uvRegion.y,
                region.uvRegion.width,
                region.uvRegion.height
            );
            region.currentDissolve = 0f;

            // Subscribe to block events
            if (region.objectHider != null)
            {
                var detector = region.objectHider.GetComponent<LightDetector>();
                if (detector != null)
                {
                    int index = i; // Capture for closure
                    detector.OnChangeState.AddListener((isInLight) =>
                    {
                        OnBlockChanged(index, !isInLight); // Inverted: in light = hidden
                    });
                }
            }
        }

        // Send to shader
        UpdateShader();
    }

    private void OnBlockChanged(int blockIndex, bool isVisible)
    {
        // Start coroutine to animate dissolve
        StartCoroutine(AnimateBlockDissolve(blockIndex, isVisible ? 0f : 1f));
    }

    private System.Collections.IEnumerator AnimateBlockDissolve(int blockIndex, float targetDissolve)
    {
        float startDissolve = blockRegions[blockIndex].currentDissolve;

        float elapsed = 0f;
        float duration = 1f / dissolveSpeed;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            blockRegions[blockIndex].currentDissolve = Mathf.Lerp(startDissolve, targetDissolve, t);

            UpdateShader();

            yield return null;
        }

        blockRegions[blockIndex].currentDissolve = targetDissolve;
        UpdateShader();
    }

    private void UpdateShader()
    {
        if (_spriteMaterial == null || blockRegions.Count == 0) return;

        // Build dissolve amounts array from current block states
        float[] dissolveAmounts = new float[blockRegions.Count];
        for (int i = 0; i < blockRegions.Count; i++)
        {
            dissolveAmounts[i] = blockRegions[i].currentDissolve;
        }

        // Send region data to shader
        _spriteMaterial.SetVectorArray("_RegionData", _regionData);
        _spriteMaterial.SetFloatArray("_RegionDissolve", dissolveAmounts);
        _spriteMaterial.SetInt("_RegionCount", blockRegions.Count);
    }

    private void OnDestroy()
    {
        // Only destroy if we created an instance material
        if (_spriteMaterial != null && wallSprite != null && _spriteMaterial != wallSprite.sharedMaterial)
        {
            Destroy(_spriteMaterial);
        }
    }

    /// <summary>
    /// Get the average dissolve amount across all blocks
    /// </summary>
    public float GetAverageDissolveAmount()
    {
        if (blockRegions == null || blockRegions.Count == 0) return 0f;

        float total = 0f;
        foreach (var region in blockRegions)
        {
            total += region.currentDissolve;
        }

        return total / blockRegions.Count;
    }

    /// <summary>
    /// Manually set dissolve amount for all blocks
    /// </summary>
    public void SetAllBlocksDissolve(float amount)
    {
        amount = Mathf.Clamp01(amount);

        for (int i = 0; i < blockRegions.Count; i++)
        {
            blockRegions[i].currentDissolve = amount;
        }

        UpdateShader();
    }

#if UNITY_EDITOR
    // Helper to auto-find all ObjectHider children and calculate UV regions
    [ContextMenu("Auto Setup - Find All Blocks and Calculate UV Regions")]
    private void AutoSetupBlockRegions()
    {
        if (wallSprite == null)
        {
            Debug.LogError("Wall sprite not assigned!");
            return;
        }

        // Find all ObjectHider components in children
        ObjectHider[] foundBlocks = GetComponentsInChildren<ObjectHider>();

        if (foundBlocks.Length == 0)
        {
            Debug.LogWarning("No ObjectHider components found in children!");
            return;
        }

        // Clear existing regions
        blockRegions.Clear();

        Bounds spriteBounds = wallSprite.bounds;

        // Create new regions for each found block
        foreach (var block in foundBlocks)
        {
            Collider2D blockCollider = block.GetComponent<Collider2D>();
            if (blockCollider == null)
            {
                Debug.LogWarning($"ObjectHider on {block.gameObject.name} has no Collider2D!");
                continue;
            }

            Bounds blockBounds = blockCollider.bounds;
            Rect uvRegion = CalculateUVRegionFromBounds(spriteBounds, blockBounds);

            // Create new region
            BlockRegion newRegion = new BlockRegion
            {
                objectHider = block,
                uvRegion = uvRegion,
                currentDissolve = 0f
            };

            blockRegions.Add(newRegion);
        }

        Debug.Log($"Auto-setup complete! Found and configured {blockRegions.Count} ObjectHider blocks.");
    }

    // Helper to only recalculate UV regions (if blocks are already assigned)
    [ContextMenu("Recalculate UV Regions Only")]
    private void RecalculateUVRegions()
    {
        if (wallSprite == null)
        {
            Debug.LogError("Wall sprite not assigned!");
            return;
        }

        Bounds spriteBounds = wallSprite.bounds;
        int calculatedCount = 0;

        foreach (var region in blockRegions)
        {
            if (region.objectHider == null) continue;

            Collider2D blockCollider = region.objectHider.GetComponent<Collider2D>();
            if (blockCollider == null) continue;

            Bounds blockBounds = blockCollider.bounds;
            region.uvRegion = CalculateUVRegionFromBounds(spriteBounds, blockBounds);
            calculatedCount++;
        }

        Debug.Log($"UV regions recalculated for {calculatedCount} blocks!");
    }
#endif

    /// <summary>
    /// Public helper method for testing: Calculate UV region from world bounds
    /// </summary>
    public Rect CalculateUVRegionFromBounds(Bounds spriteBounds, Bounds blockBounds)
    {
        // Get sprite reference
        Sprite sprite = wallSprite.sprite;
        if (sprite == null)
        {
            Debug.LogError("WallSprite has no sprite assigned!");
            return new Rect(0, 0, 1, 1);
        }

        // Get the sprite's transform
        Transform spriteTransform = wallSprite.transform;

        // Convert all 8 corners of the block bounds to local space to handle rotation properly
        Vector3[] worldCorners = new Vector3[8];
        worldCorners[0] = new Vector3(blockBounds.min.x, blockBounds.min.y, blockBounds.min.z);
        worldCorners[1] = new Vector3(blockBounds.min.x, blockBounds.min.y, blockBounds.max.z);
        worldCorners[2] = new Vector3(blockBounds.min.x, blockBounds.max.y, blockBounds.min.z);
        worldCorners[3] = new Vector3(blockBounds.min.x, blockBounds.max.y, blockBounds.max.z);
        worldCorners[4] = new Vector3(blockBounds.max.x, blockBounds.min.y, blockBounds.min.z);
        worldCorners[5] = new Vector3(blockBounds.max.x, blockBounds.min.y, blockBounds.max.z);
        worldCorners[6] = new Vector3(blockBounds.max.x, blockBounds.max.y, blockBounds.min.z);
        worldCorners[7] = new Vector3(blockBounds.max.x, blockBounds.max.y, blockBounds.max.z);

        // Transform all corners to local space and find actual min/max
        Vector3 blockMinLocal = spriteTransform.InverseTransformPoint(worldCorners[0]);
        Vector3 blockMaxLocal = blockMinLocal;

        for (int i = 1; i < 8; i++)
        {
            Vector3 localCorner = spriteTransform.InverseTransformPoint(worldCorners[i]);
            blockMinLocal = Vector3.Min(blockMinLocal, localCorner);
            blockMaxLocal = Vector3.Max(blockMaxLocal, localCorner);
        }

        // Get sprite dimensions in unscaled local space
        Rect spriteRect = sprite.textureRect;
        float pixelsPerUnit = sprite.pixelsPerUnit;
        Vector2 spritePivot = sprite.pivot;

        // Calculate sprite size in unscaled local units
        float spriteWidthUnits = spriteRect.width / pixelsPerUnit;
        float spriteHeightUnits = spriteRect.height / pixelsPerUnit;

        // Calculate sprite bounds in unscaled local space (accounting for pivot)
        float spriteMinX = -spritePivot.x / pixelsPerUnit;
        float spriteMinY = -spritePivot.y / pixelsPerUnit;
        float spriteMaxX = spriteMinX + spriteWidthUnits;
        float spriteMaxY = spriteMinY + spriteHeightUnits;

        // Convert block local bounds to UV coordinates (0-1 range within sprite)
        // The block bounds are already in the sprite's local space, accounting for scale
        float uvX = (blockMinLocal.x - spriteMinX) / spriteWidthUnits;
        float uvY = (blockMinLocal.y - spriteMinY) / spriteHeightUnits;
        float uvWidth = (blockMaxLocal.x - blockMinLocal.x) / spriteWidthUnits;
        float uvHeight = (blockMaxLocal.y - blockMinLocal.y) / spriteHeightUnits;

        // Don't clamp - allow blocks to extend slightly outside sprite bounds
        // This handles cases where colliders are positioned with small offsets
        // The shader will handle UV coordinates outside 0-1 range properly

        Debug.Log($"Block UV Region: x={uvX:F3}, y={uvY:F3}, w={uvWidth:F3}, h={uvHeight:F3} " +
                  $"(BlockLocal: {blockMinLocal} to {blockMaxLocal}, SpriteLocal: ({spriteMinX:F3},{spriteMinY:F3}) to ({spriteMaxX:F3},{spriteMaxY:F3}))");

        return new Rect(uvX, uvY, uvWidth, uvHeight);
    }
}
