using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Simple EditMode tests for SimpleBlockToSpriteSync grid-based approach.
/// For integration tests, see SimpleBlockToSpriteSyncPlayModeTests.
/// </summary>
public class SimpleBlockToSpriteSyncTests
{
    private SimpleBlockToSpriteSync _sync;

    [SetUp]
    public void SetUp()
    {
        var gameObject = new GameObject("TestSync");
        _sync = gameObject.AddComponent<SimpleBlockToSpriteSync>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_sync.gameObject);
    }

    [Test]
    public void GridRegions_2x2Grid_CreatesCorrectRegions()
    {
        // Arrange - Set up a 2x2 grid
        SetPrivateField(_sync, "gridColumns", 2);
        SetPrivateField(_sync, "gridRows", 2);

        // Act - Call private CalculateGridRegions method
        var regionData = new Vector4[4];
        SetPrivateField(_sync, "_regionData", regionData);
        InvokePrivateMethod(_sync, "CalculateGridRegions");

        // Assert - Verify grid regions are correct
        // Region 0 (top-left): x=0, y=0, w=0.5, h=0.5
        Assert.AreEqual(0f, regionData[0].x, 0.001f);
        Assert.AreEqual(0f, regionData[0].y, 0.001f);
        Assert.AreEqual(0.5f, regionData[0].z, 0.001f);
        Assert.AreEqual(0.5f, regionData[0].w, 0.001f);

        // Region 1 (top-right): x=0.5, y=0, w=0.5, h=0.5
        Assert.AreEqual(0.5f, regionData[1].x, 0.001f);
        Assert.AreEqual(0f, regionData[1].y, 0.001f);

        // Region 2 (bottom-left): x=0, y=0.5, w=0.5, h=0.5
        Assert.AreEqual(0f, regionData[2].x, 0.001f);
        Assert.AreEqual(0.5f, regionData[2].y, 0.001f);

        // Region 3 (bottom-right): x=0.5, y=0.5, w=0.5, h=0.5
        Assert.AreEqual(0.5f, regionData[3].x, 0.001f);
        Assert.AreEqual(0.5f, regionData[3].y, 0.001f);
    }

    [Test]
    public void GridRegions_3x2Grid_CreatesCorrectRegions()
    {
        // Arrange - Set up a 3x2 grid (3 columns, 2 rows)
        SetPrivateField(_sync, "gridColumns", 3);
        SetPrivateField(_sync, "gridRows", 2);

        // Act
        var regionData = new Vector4[6];
        SetPrivateField(_sync, "_regionData", regionData);
        InvokePrivateMethod(_sync, "CalculateGridRegions");

        // Assert - Each cell should be 1/3 wide and 1/2 tall
        float expectedWidth = 1f / 3f;
        float expectedHeight = 0.5f;

        // Region 0: x=0, y=0
        Assert.AreEqual(0f, regionData[0].x, 0.001f);
        Assert.AreEqual(0f, regionData[0].y, 0.001f);
        Assert.AreEqual(expectedWidth, regionData[0].z, 0.001f);
        Assert.AreEqual(expectedHeight, regionData[0].w, 0.001f);

        // Region 1: x=1/3, y=0
        Assert.AreEqual(expectedWidth, regionData[1].x, 0.001f);
        Assert.AreEqual(0f, regionData[1].y, 0.001f);

        // Region 2: x=2/3, y=0
        Assert.AreEqual(expectedWidth * 2, regionData[2].x, 0.001f);
        Assert.AreEqual(0f, regionData[2].y, 0.001f);
    }

    // Helper methods
    private void SetPrivateField<T>(object obj, string fieldName, T value)
    {
        var field = obj.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field.SetValue(obj, value);
    }

    private void InvokePrivateMethod(object obj, string methodName)
    {
        var method = obj.GetType().GetMethod(methodName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(obj, null);
    }
}
