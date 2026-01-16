using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;
using Light_and_controller.Scripts.Components;

/// <summary>
/// PlayMode integration tests for SimpleBlockToSpriteSync.
/// Tests event subscription and shader data updates (pure logic, no animation timing).
/// </summary>
public class SimpleBlockToSpriteSyncPlayModeTests
{
    private SimpleBlockToSpriteSync _sync;
    private LightDetector _lightDetector;
    private Material _material;

    [SetUp]
    public void SetUp()
    {
        // Create wall sprite with component
        var wallObject = new GameObject("Wall");
        var spriteRenderer = wallObject.AddComponent<SpriteRenderer>();
        var texture = new Texture2D(100, 100);
        spriteRenderer.sprite = Sprite.Create(texture, new Rect(0, 0, 100, 100), Vector2.one * 0.5f, 10f);

        // Use mock shader that supports our custom properties
        var shader = Shader.Find("Test/MockDissolveShader");
        Assert.IsNotNull(shader, "Mock shader 'Test/MockDissolveShader' not found. Make sure MockDissolveShader.shader is in the project.");
        _material = new Material(shader);
        spriteRenderer.material = _material;

        _sync = wallObject.AddComponent<SimpleBlockToSpriteSync>();
        SetPrivateField(_sync, "wallSprite", spriteRenderer);
        SetPrivateField(_sync, "gridColumns", 2);
        SetPrivateField(_sync, "gridRows", 2);

        // Create block with LightDetector
        var blockObject = new GameObject("Block");
        blockObject.AddComponent<BoxCollider2D>().size = Vector2.one;
        _lightDetector = blockObject.AddComponent<LightDetector>();
        var objectHider = blockObject.AddComponent<ObjectHider>();

        // Setup block region
        var region = new SimpleBlockToSpriteSync.BlockRegion
        {
            objectHider = objectHider,
            regionIndex = 0,
            currentDissolve = 0f
        };
        SetPrivateField(_sync, "blockRegions", new List<SimpleBlockToSpriteSync.BlockRegion> { region });
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_sync.gameObject);
        Object.DestroyImmediate(_lightDetector.gameObject);
    }

    [UnityTest]
    public IEnumerator Start_InitializesComponent()
    {
        // Act - Trigger Start()
        _sync.enabled = false;
        yield return null;
        _sync.enabled = true;
        yield return null;

        // Assert - Verify component initialized properly through public API
        // The component should have initialized with dissolve = 0 (fully visible)
        float averageDissolve = _sync.GetAverageDissolveAmount();
        Assert.AreEqual(0f, averageDissolve, 0.001f, "Initial dissolve should be 0 (fully visible)");

        // Verify the component is functional by setting a value
        _sync.SetAllBlocksDissolve(0.5f);
        averageDissolve = _sync.GetAverageDissolveAmount();
        Assert.AreEqual(0.5f, averageDissolve, 0.001f, "Component should accept and store dissolve values");
    }

    [UnityTest]
    public IEnumerator SetAllBlocksDissolve_UpdatesAverage()
    {
        // Arrange
        _sync.enabled = false;
        yield return null;
        _sync.enabled = true;
        yield return null;

        // Act - Set all blocks to 0.75
        _sync.SetAllBlocksDissolve(0.75f);

        // Assert - Verify public API returns correct average
        float average = _sync.GetAverageDissolveAmount();
        Assert.AreEqual(0.75f, average, 0.001f, "Average should match set value");

        // Test clamping behavior
        _sync.SetAllBlocksDissolve(1.5f); // Should clamp to 1.0
        average = _sync.GetAverageDissolveAmount();
        Assert.AreEqual(1.0f, average, 0.001f, "Should clamp to 1.0");

        _sync.SetAllBlocksDissolve(-0.5f); // Should clamp to 0.0
        average = _sync.GetAverageDissolveAmount();
        Assert.AreEqual(0.0f, average, 0.001f, "Should clamp to 0.0");
    }

    [UnityTest]
    public IEnumerator LightDetectorEvent_TriggersDissolveAnimation()
    {
        // Arrange
        _sync.enabled = false;
        yield return null;
        _sync.enabled = true;
        yield return null;

        // Verify initial state (visible, dissolve = 0)
        float initialDissolve = _sync.GetAverageDissolveAmount();
        Assert.AreEqual(0f, initialDissolve, 0.001f, "Should start visible");

        // Act - Simulate light detector event (block goes into light = should hide)
        _lightDetector.OnChangeState.Invoke(true); // true = in light

        // Wait for animation to complete (dissolveSpeed = 2, so duration = 0.5s)
        yield return new WaitForSeconds(0.6f);

        // Assert - Block should be dissolved (hidden)
        float finalDissolve = _sync.GetAverageDissolveAmount();
        Assert.AreEqual(1f, finalDissolve, 0.05f, "Block should be dissolved when in light");
    }

    [UnityTest]
    public IEnumerator LightDetectorEvent_ReverseAnimation()
    {
        // Arrange - Start with block dissolved
        _sync.enabled = false;
        yield return null;
        _sync.enabled = true;
        yield return null;

        // Set block to dissolved state
        _sync.SetAllBlocksDissolve(1f);

        // Verify starting state immediately (don't yield before checking)
        float initialDissolve = _sync.GetAverageDissolveAmount();
        Assert.AreEqual(1f, initialDissolve, 0.001f, "Should start dissolved");

        // Act - Simulate light detector event (block leaves light = should show)
        _lightDetector.OnChangeState.Invoke(false); // false = not in light

        // Wait for animation to complete
        yield return new WaitForSeconds(0.6f);

        // Assert - Block should be visible
        float finalDissolve = _sync.GetAverageDissolveAmount();
        Assert.AreEqual(0f, finalDissolve, 0.05f, "Block should be visible when not in light");
    }

    // Helper methods
    private void SetPrivateField<T>(object obj, string fieldName, T value)
    {
        var field = obj.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field.SetValue(obj, value);
    }
}
