using UnityEngine;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Animations;

namespace MVVM.Editor
{
    public class TexturePackerConfigProcessor : AssetPostprocessor
    {
        private List<SpriteRect> cachedSpriteData;
        private const float DefaultFrameRate = 30f;

        private void OnPreprocessTexture()
        {
            if (assetPath.EndsWith(".png"))
            {
                string configPath = assetPath.Replace(".png", ".atlas");
                if (File.Exists(configPath))
                {
                    TextureImporter textureImporter = assetImporter as TextureImporter;
                    if (textureImporter != null)
                    {
                        ConfigureTextureImporter(textureImporter, configPath);
                    }
                }
            }
        }

        private void OnPostprocessTexture(Texture2D texture)
        {
            if (cachedSpriteData != null && cachedSpriteData.Count > 0)
            {
                TextureImporter textureImporter = assetImporter as TextureImporter;
                if (textureImporter != null)
                {
                    var factory = new SpriteDataProviderFactories();
                    factory.Init();
                    var dataProvider = factory.GetSpriteEditorDataProviderFromObject(textureImporter);
                    if (dataProvider != null)
                    {
                        dataProvider.InitSpriteEditorDataProvider();
                        
                        var existingRects = dataProvider.GetSpriteRects();
                        var existingGUIDs = new Dictionary<string, GUID>();
                        foreach (var existing in existingRects)
                        {
                            existingGUIDs[existing.name] = existing.spriteID;
                        }
                        
                        foreach (var sprite in cachedSpriteData)
                        {
                            if (existingGUIDs.ContainsKey(sprite.name))
                            {
                                sprite.spriteID = existingGUIDs[sprite.name];
                            }
                        }
                        
                        dataProvider.SetSpriteRects(cachedSpriteData.ToArray());
                        dataProvider.Apply();
                        
                        EditorApplication.delayCall += () => CreateAnimationClip(assetPath);
                    }
                }
                cachedSpriteData = null;
            }
        }

        private void ConfigureTextureImporter(TextureImporter textureImporter, string configPath)
        {
            textureImporter.textureType = TextureImporterType.Sprite;
            textureImporter.spriteImportMode = SpriteImportMode.Multiple;
            textureImporter.mipmapEnabled = false;
            textureImporter.filterMode = FilterMode.Bilinear;
            textureImporter.textureCompression = TextureImporterCompression.Uncompressed;

            cachedSpriteData = ParseTexturePackerConfig(configPath);
        }

        private List<SpriteRect> ParseTexturePackerConfig(string configPath)
        {
            var sprites = new List<SpriteRect>();
            string[] lines = File.ReadAllLines(configPath);

            if (lines.Length < 2)
                return sprites;

            string textureName = lines[0].Trim();
            string sizeInfo = lines[1].Trim();
            
            int textureWidth = 0;
            int textureHeight = 0;
            if (sizeInfo.StartsWith("size:"))
            {
                string[] sizeParts = sizeInfo.Substring(5).Split(',');
                if (sizeParts.Length == 2)
                {
                    int.TryParse(sizeParts[0], out textureWidth);
                    int.TryParse(sizeParts[1], out textureHeight);
                }
            }

            string currentName = "";
            int currentIndex = -1;
            Rect currentBounds = Rect.zero;
            Vector4 currentOffsets = Vector4.zero;

            for (int i = 3; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                
                if (string.IsNullOrEmpty(line))
                    continue;

                if (line.StartsWith("index:"))
                {
                    if (!string.IsNullOrEmpty(currentName) && currentIndex >= 0)
                    {
                        var sprite = CreateSpriteRect(currentName, currentIndex, currentBounds, currentOffsets, textureHeight);
                        sprites.Add(sprite);
                    }

                    int.TryParse(line.Substring(6), out currentIndex);
                }
                else if (line.StartsWith("bounds:"))
                {
                    string[] parts = line.Substring(7).Split(',');
                    if (parts.Length == 4)
                    {
                        float x = float.Parse(parts[0]);
                        float y = float.Parse(parts[1]);
                        float width = float.Parse(parts[2]);
                        float height = float.Parse(parts[3]);
                        currentBounds = new Rect(x, y, width, height);
                    }
                }
                else if (line.StartsWith("offsets:"))
                {
                    string[] parts = line.Substring(8).Split(',');
                    if (parts.Length == 4)
                    {
                        currentOffsets = new Vector4(
                            float.Parse(parts[0]),
                            float.Parse(parts[1]),
                            float.Parse(parts[2]),
                            float.Parse(parts[3])
                        );
                    }
                }
                else if (!line.Contains(":"))
                {
                    currentName = line;
                }
            }

            if (!string.IsNullOrEmpty(currentName) && currentIndex >= 0)
            {
                var sprite = CreateSpriteRect(currentName, currentIndex, currentBounds, currentOffsets, textureHeight);
                sprites.Add(sprite);
            }

            var filledSprites = FillMissingIndices(sprites, currentName);
            return filledSprites.OrderBy(s => 
            {
                string[] parts = s.name.Split('_');
                if (parts.Length > 1 && int.TryParse(parts[parts.Length - 1], out int index))
                {
                    return index;
                }
                return 0;
            }).ToList();
        }

        private List<SpriteRect> FillMissingIndices(List<SpriteRect> sprites, string baseName)
        {
            if (sprites.Count == 0)
                return sprites;

            var indices = new HashSet<int>();
            foreach (var sprite in sprites)
            {
                string[] parts = sprite.name.Split('_');
                if (parts.Length > 1 && int.TryParse(parts[parts.Length - 1], out int index))
                {
                    indices.Add(index);
                }
            }

            int maxIndex = indices.Max();
            var result = new List<SpriteRect>(sprites);

            for (int i = 0; i <= maxIndex; i++)
            {
                if (!indices.Contains(i))
                {
                    string blankName = $"{baseName}_{i}";
                    var blankSprite = new SpriteRect
                    {
                        name = blankName,
                        rect = new Rect(0, 0, 1, 1),
                        alignment = SpriteAlignment.Center,
                        pivot = new Vector2(0.5f, 0.5f),
                        spriteID = GenerateDeterministicGUID(blankName)
                    };
                    result.Add(blankSprite);
                }
            }

            return result;
        }

        private SpriteRect CreateSpriteRect(string name, int index, Rect bounds, Vector4 offsets, int textureHeight)
        {
            float y = textureHeight - bounds.y - bounds.height;
            string spriteName = $"{name}_{index}";

            var spriteData = new SpriteRect
            {
                name = spriteName,
                rect = new Rect(bounds.x, y, bounds.width, bounds.height),
                alignment = SpriteAlignment.Center,
                pivot = new Vector2(0.5f, 0.5f),
                spriteID = GenerateDeterministicGUID(spriteName)
            };

            return spriteData;
        }

        private GUID GenerateDeterministicGUID(string spriteName)
        {
            var hash = spriteName.GetHashCode();
            var bytes = new byte[16];
            var hashBytes = System.BitConverter.GetBytes(hash);
            System.Array.Copy(hashBytes, bytes, System.Math.Min(hashBytes.Length, bytes.Length));
            return new GUID(System.BitConverter.ToString(bytes).Replace("-", ""));
        }

        [MenuItem("Assets/Reimport Texture with Config", true)]
        private static bool ValidateReimportTexture()
        {
            if (Selection.activeObject == null)
                return false;

            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(path))
                return false;

            if (path.EndsWith(".png"))
            {
                string configPath = path.Replace(".png", ".atlas");
                return File.Exists(configPath);
            }

            return false;
        }

        [MenuItem("Assets/Reimport Texture with Config")]
        private static void ReimportTexture()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            Debug.Log($"Reimported texture: {path}");
        }

        private void CreateAnimationClip(string texturePath)
        {
            string animationPath = texturePath.Replace(".png", ".anim");
            
            if (File.Exists(animationPath))
            {
                return;
            }

            var sprites = AssetDatabase.LoadAllAssetsAtPath(texturePath)
                .OfType<Sprite>()
                .OrderBy(s => 
                {
                    string[] parts = s.name.Split('_');
                    if (parts.Length > 1 && int.TryParse(parts[parts.Length - 1], out int index))
                    {
                        return index;
                    }
                    return 0;
                })
                .ToArray();

            if (sprites.Length == 0)
                return;

            AnimationClip clip = new AnimationClip();
            clip.frameRate = DefaultFrameRate;

            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            var binding = new EditorCurveBinding
            {
                type = typeof(SpriteRenderer),
                path = "",
                propertyName = "m_Sprite"
            };

            var keyframes = new ObjectReferenceKeyframe[sprites.Length];
            for (int i = 0; i < sprites.Length; i++)
            {
                keyframes[i] = new ObjectReferenceKeyframe
                {
                    time = i / clip.frameRate,
                    value = sprites[i]
                };
            }

            AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

            AssetDatabase.CreateAsset(clip, animationPath);
            Debug.Log($"Created animation clip: {animationPath}");
        }
    }
}
