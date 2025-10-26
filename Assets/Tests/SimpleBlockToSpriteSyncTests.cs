using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Simple EditMode tests for pure calculation methods in SimpleBlockToSpriteSync.
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
    public void CalculateUVRegion_BlockAtSpriteOrigin_ReturnsCorrectUV()
    {
        // Arrange - Sprite at (0,0) with size 10x10, Block at (0,0) with size 2x2
        Bounds spriteBounds = new Bounds(new Vector3(0, 0, 0), new Vector3(10, 10, 0));
        Bounds blockBounds = new Bounds(new Vector3(0, 0, 0), new Vector3(2, 2, 0));

        // Act
        Rect uvRegion = _sync.CalculateUVRegionFromBounds(spriteBounds, blockBounds);

        // Assert
        // Block center is at (0,0), size is 2x2, so it spans from (-1,-1) to (1,1)
        // Sprite center is at (0,0), size is 10x10, so it spans from (-5,-5) to (5,5)
        // UV X = (blockMin.x - spriteMin.x) / spriteSize.x = (-1 - (-5)) / 10 = 4/10 = 0.4
        // UV Y = (blockMin.y - spriteMin.y) / spriteSize.y = (-1 - (-5)) / 10 = 4/10 = 0.4
        // UV Width = blockSize.x / spriteSize.x = 2 / 10 = 0.2
        // UV Height = blockSize.y / spriteSize.y = 2 / 10 = 0.2
        Assert.AreEqual(0.4f, uvRegion.x, 0.001f);
        Assert.AreEqual(0.4f, uvRegion.y, 0.001f);
        Assert.AreEqual(0.2f, uvRegion.width, 0.001f);
        Assert.AreEqual(0.2f, uvRegion.height, 0.001f);
    }

    [Test]
    public void CalculateUVRegion_OffsetSpriteAndBlock_ReturnsCorrectUV()
    {
        // Arrange - Sprite at (100, 200) with size 20x20, Block at (105, 205) with size 4x4
        Bounds spriteBounds = new Bounds(new Vector3(100, 200, 0), new Vector3(20, 20, 0));
        Bounds blockBounds = new Bounds(new Vector3(105, 205, 0), new Vector3(4, 4, 0));

        // Act
        Rect uvRegion = _sync.CalculateUVRegionFromBounds(spriteBounds, blockBounds);

        // Assert
        // Sprite spans from (90, 190) to (110, 210)
        // Block spans from (103, 203) to (107, 207)
        // UV X = (103 - 90) / 20 = 13/20 = 0.65
        // UV Y = (203 - 190) / 20 = 13/20 = 0.65
        // UV Width = 4 / 20 = 0.2
        // UV Height = 4 / 20 = 0.2
        Assert.AreEqual(0.65f, uvRegion.x, 0.001f);
        Assert.AreEqual(0.65f, uvRegion.y, 0.001f);
        Assert.AreEqual(0.2f, uvRegion.width, 0.001f);
        Assert.AreEqual(0.2f, uvRegion.height, 0.001f);
    }

}
