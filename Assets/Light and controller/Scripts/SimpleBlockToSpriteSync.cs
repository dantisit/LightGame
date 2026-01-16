using System.Collections.Generic;
using Light_and_controller.Scripts.Components;
using UnityEngine;

/// <summary>
/// SIMPLIFIED GRID APPROACH: Divides sprite into equal grid regions.
/// Each physics block controls one region in the grid.
/// </summary>
public class SimpleBlockToSpriteSync : MonoBehaviour
{
    [System.Serializable]
    public class BlockRegion
    {
        public ObjectHider objectHider;
        [HideInInspector] public int regionIndex;
        [HideInInspector] public float currentDissolve = 0f;
    }

    [Header("References")]
    [SerializeField] private SpriteRenderer wallSprite;
    [SerializeField] private List<BlockRegion> blockRegions = new List<BlockRegion>();

    [Header("Grid Settings")]
    [SerializeField] private int gridColumns = 2;
    [SerializeField] private int gridRows = 2;

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

        // Calculate grid-based UV regions
        int totalRegions = gridColumns * gridRows;
        _regionData = new Vector4[totalRegions];
        
        CalculateGridRegions();

        // Initialize regions
        for (int i = 0; i < blockRegions.Count; i++)
        {
            var region = blockRegions[i];
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

    private void CalculateGridRegions()
    {
        float cellWidth = 1f / gridColumns;
        float cellHeight = 1f / gridRows;

        int index = 0;
        for (int row = 0; row < gridRows; row++)
        {
            for (int col = 0; col < gridColumns; col++)
            {
                float x = col * cellWidth;
                float y = row * cellHeight;
                
                _regionData[index] = new Vector4(x, y, cellWidth, cellHeight);
                index++;
            }
        }
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
        if (_spriteMaterial == null || _regionData == null) return;

        int totalRegions = gridColumns * gridRows;
        float[] dissolveAmounts = new float[totalRegions];
        
        // Initialize all regions to 0 (visible)
        for (int i = 0; i < totalRegions; i++)
        {
            dissolveAmounts[i] = 0f;
        }
        
        // Set dissolve for assigned blocks
        for (int i = 0; i < blockRegions.Count; i++)
        {
            int regionIdx = blockRegions[i].regionIndex;
            if (regionIdx >= 0 && regionIdx < totalRegions)
            {
                dissolveAmounts[regionIdx] = blockRegions[i].currentDissolve;
            }
        }

        // Send region data to shader
        _spriteMaterial.SetVectorArray("_RegionData", _regionData);
        _spriteMaterial.SetFloatArray("_RegionDissolve", dissolveAmounts);
        _spriteMaterial.SetInt("_RegionCount", totalRegions);
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
    [ContextMenu("Auto Setup - Find All Blocks and Assign Grid Regions")]
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

        // Create new regions and assign sequential grid indices
        for (int i = 0; i < foundBlocks.Length; i++)
        {
            BlockRegion newRegion = new BlockRegion
            {
                objectHider = foundBlocks[i],
                regionIndex = i,
                currentDissolve = 0f
            };

            blockRegions.Add(newRegion);
        }

        // Auto-adjust grid size to fit all blocks
        int totalBlocks = blockRegions.Count;
        gridColumns = Mathf.CeilToInt(Mathf.Sqrt(totalBlocks));
        gridRows = Mathf.CeilToInt((float)totalBlocks / gridColumns);

        Debug.Log($"Auto-setup complete! Found {blockRegions.Count} blocks. Grid set to {gridColumns}x{gridRows}.");
    }
#endif
}
