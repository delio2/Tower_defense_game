using System.IO;
using UnityEditor;
using UnityEngine;

namespace TowerDefense.Editor
{
    /// <summary>
    /// Import settings for the HUD icons (Assets/UI/Icons, written by Tools/icons_to_png.py from the design
    /// system's SVGs). They are 96 px line drawings in white on transparency, tinted per context by the
    /// stylesheet, so they want their alpha left alone and no compression: a compressed 96 px line would fray.
    /// </summary>
    public sealed class IconImporter : AssetPostprocessor
    {
        private const string IconFolder = "Assets/UI/Icons/";

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(IconFolder, System.StringComparison.Ordinal) || Path.GetExtension(assetPath) != ".png")
            {
                return;
            }

            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Default;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.maxTextureSize = 128;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.sRGBTexture = true;
        }
    }
}
