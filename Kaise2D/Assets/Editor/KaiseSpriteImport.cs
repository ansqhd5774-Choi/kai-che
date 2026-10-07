using UnityEditor;
using UnityEngine;

public sealed class KaiseSpriteImport : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        if (!assetPath.Contains("/Resources/Hormelz/")) return;

        var ti = (TextureImporter)assetImporter;
        ti.textureType = TextureImporterType.Default;
        ti.alphaIsTransparency = true;
        ti.mipmapEnabled = false;
        ti.filterMode = FilterMode.Point;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        ti.wrapMode = TextureWrapMode.Clamp;
    }
}
