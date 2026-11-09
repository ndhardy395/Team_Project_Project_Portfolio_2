using System.IO;
using UnityEditor;
using UnityEngine;

// Imports every picture in Assets/Scenes/LevelSelect/Art as a UI sprite.
// rounded.png and chamfer.png also get 9-slice borders, so they stretch to any size without bending their corners.
// (The icons were drawn from the mockup's SVGs.)
class LevelSelectArtImporter : AssetPostprocessor
{
    public const string Folder = "Assets/Scenes/LevelSelect/Art/";

    void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith(Folder))
            return;

        TextureImporter importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.textureCompression = TextureImporterCompression.Uncompressed;

        string file = Path.GetFileNameWithoutExtension(assetPath);
        if (file == "rounded" || file == "chamfer")
            importer.spriteBorder = new Vector4(16, 16, 16, 16);
    }
}
