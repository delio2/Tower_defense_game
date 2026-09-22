using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.TextCore.Text;

namespace TowerDefense.Editor
{
    /// <summary>
    /// Builds the UI font assets from the two variable fonts in Assets/UI/Fonts (docs/09 §5, design system
    /// "Dusk Garden"). Outfit carries the numbers, Nunito the text; the design system asks for weights 600, 700
    /// and 800, which a variable font exposes as named instances. FreeType addresses instance n as (n &lt;&lt; 16),
    /// and Unity passes the index straight through — but the two files do not list the same instances (Outfit
    /// starts at Thin, Nunito at ExtraLight), so the style is looked up by name instead of hard-coding an index.
    ///
    /// The assets are <b>dynamic</b> (Unity 6 no longer draws static ones) with the Latin-1 atlas already baked, so
    /// the text of the game costs nothing at runtime while a character from outside the set is still rasterised on
    /// demand instead of showing as a box. The .ttf files ship with the player; a build that loses glyphs to a full
    /// atlas is retried on a larger one.
    /// </summary>
    public static class FontMenu
    {
        private const string FontFolder = "Assets/UI/Fonts";
        // 64 pt keeps the whole Latin-1 set on a single 1024 atlas (1 MB per weight); an SDF face scales up from
        // there without fraying, and these two typefaces are geometric enough to survive it.
        private const int SamplingPointSize = 64;
        private const int AtlasPadding = 6;
        private static readonly int[] AtlasSizes = { 1024, 2048 };

        /// <summary>One weight of the design system: which file it comes from and which named instance carries it.</summary>
        private readonly struct Face
        {
            public Face(string file, string assetName, string styleName)
            {
                File = file;
                AssetName = assetName;
                StyleName = styleName;
            }

            public string File { get; }
            public string AssetName { get; }

            /// <summary>Name of the named instance inside the variable font, e.g. "SemiBold" (weight 600).</summary>
            public string StyleName { get; }
        }

        private static readonly Face[] Faces =
        {
            new Face("Outfit-VF.ttf", "Outfit-SemiBold", "SemiBold"),   // 600: number-hero, number-xl, number, number-sm
            new Face("Nunito-VF.ttf", "Nunito-SemiBold", "SemiBold"),   // 600: body
            new Face("Nunito-VF.ttf", "Nunito-ExtraBold", "ExtraBold"), // 800: title, heading, label; also caption (700),
                                                                        // whose 12 dp size hides the difference
        };

        [MenuItem("TowerDefense/Content/Build font assets")]
        public static void BuildFontAssets()
        {
            string characters = CharacterSet();
            foreach (Face face in Faces)
            {
                string sourcePath = Path.Combine(FontFolder, face.File);
                if (!File.Exists(sourcePath))
                {
                    Debug.LogError($"Font: {sourcePath} is missing. docs/06 §4 says where the variable fonts come from.");
                    return;
                }

                Build(face, Path.GetFullPath(sourcePath), characters);
            }

            LinkFallbacks();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Sends every Nunito weight to Outfit for the characters Nunito does not draw (the arrow of the drag
        /// preview, "DPS 16 → 24"). Without it those show as a box.
        /// </summary>
        private static void LinkFallbacks()
        {
            FontAsset numbers = Load("Outfit-SemiBold");
            if (numbers == null)
            {
                return;
            }

            foreach (Face face in Faces)
            {
                FontAsset font = Load(face.AssetName);
                if (font == null || font == numbers)
                {
                    continue;
                }

                font.fallbackFontAssetTable = new List<FontAsset> { numbers };
                EditorUtility.SetDirty(font);
            }
        }

        private static FontAsset Load(string assetName)
        {
            return AssetDatabase.LoadAssetAtPath<FontAsset>($"{FontFolder}/{assetName}.asset");
        }

        private static void Build(Face face, string sourcePath, string characters)
        {
            int faceIndex = FindInstance(sourcePath, face.StyleName);
            if (faceIndex < 0)
            {
                Debug.LogError($"Font: {face.File} has no named instance \"{face.StyleName}\".");
                return;
            }

            string assetPath = $"{FontFolder}/{face.AssetName}.asset";
            foreach (int atlasSize in AtlasSizes)
            {
                FontAsset font = FontAsset.CreateFontAsset(sourcePath, faceIndex, SamplingPointSize, AtlasPadding,
                    GlyphRenderMode.SDFAA, atlasSize, atlasSize);
                if (font == null)
                {
                    Debug.LogError($"Font: could not load {face.AssetName} from {face.File}.");
                    return;
                }

                font.name = face.AssetName;
                font.atlasPopulationMode = AtlasPopulationMode.Dynamic;
                font.isMultiAtlasTexturesEnabled = true;
                font.TryAddCharacters(characters, out string missing);

                // A full atlas silently drops glyphs that would have fitted: retry on the next size up. Characters
                // the typeface simply does not draw are a different matter and no bigger atlas would help.
                string lost = OutOfSpace(sourcePath, faceIndex, missing);
                string undrawn = Without(missing, lost);
                if (lost.Length > 0 && atlasSize != AtlasSizes[AtlasSizes.Length - 1])
                {
                    Object.DestroyImmediate(font);
                    continue;
                }

                font.ReadFontAssetDefinition();
                AssetDatabase.DeleteAsset(assetPath);
                AssetDatabase.CreateAsset(font, assetPath);
                PointAtSourceFont(font, $"{FontFolder}/{face.File}");

                // The atlas and the material live in memory: without this they are lost when the asset reloads.
                foreach (Texture2D atlas in font.atlasTextures)
                {
                    atlas.name = $"{face.AssetName} Atlas";
                    AssetDatabase.AddObjectToAsset(atlas, font);
                }

                if (font.material != null)
                {
                    font.material.name = $"{face.AssetName} Material";
                    AssetDatabase.AddObjectToAsset(font.material, font);
                }

                EditorUtility.SetDirty(font);
                int added = characters.Length - (missing == null ? 0 : missing.Length);
                Debug.Log($"Font: {face.AssetName} = {font.faceInfo.familyName} {font.faceInfo.styleName}, {added} glyphs on a {atlasSize} atlas.");
                if (lost.Length > 0)
                {
                    Debug.LogError($"Font: {face.AssetName} ran out of atlas space for {Describe(lost)} — raise AtlasSizes.");
                }

                if (undrawn.Length > 0)
                {
                    // Not an error: another face covers these through the fallback table.
                    Debug.Log($"Font: {face.AssetName} does not draw {Describe(undrawn)}.");
                }

                return;
            }
        }

        /// <summary>
        /// Points the asset at the .ttf through its GUID instead of the absolute path CreateFontAsset records, so
        /// the asset survives moving the project to another machine and the font file ships with the player. A
        /// dynamic asset needs it: the face is reopened at load, and the weight comes from the serialised face index.
        /// </summary>
        private static void PointAtSourceFont(FontAsset font, string fontFilePath)
        {
            var serialized = new SerializedObject(font);
            serialized.FindProperty("m_SourceFontFilePath").stringValue = string.Empty;
            serialized.FindProperty("m_SourceFontFileGUID").stringValue = AssetDatabase.AssetPathToGUID(fontFilePath);
            serialized.FindProperty("m_SourceFontFile").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Font>(fontFilePath);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>
        /// The subset of <paramref name="missing"/> the typeface does draw, so it was the atlas that ran out of room.
        /// </summary>
        private static string OutOfSpace(string sourcePath, int faceIndex, string missing)
        {
            if (string.IsNullOrEmpty(missing))
            {
                return string.Empty;
            }

            try
            {
                if (FontEngine.LoadFontFace(sourcePath, SamplingPointSize, faceIndex) != FontEngineError.Success)
                {
                    return missing;
                }

                var drawn = new StringBuilder();
                foreach (char c in missing)
                {
                    if (FontEngine.TryGetGlyphIndex(c, out uint index) && index != 0)
                    {
                        drawn.Append(c);
                    }
                }

                return drawn.ToString();
            }
            finally
            {
                FontEngine.UnloadAllFontFaces();
            }
        }

        private static string Without(string text, string removed)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            var kept = new StringBuilder();
            foreach (char c in text)
            {
                if (removed.IndexOf(c) < 0)
                {
                    kept.Append(c);
                }
            }

            return kept.ToString();
        }

        /// <summary>
        /// Index of the named instance whose style is <paramref name="styleName"/>, in the form Unity expects
        /// (instance n shifted into the high 16 bits), or -1. Instance 0 is the file's default outline.
        /// </summary>
        private static int FindInstance(string sourcePath, string styleName)
        {
            try
            {
                for (int instance = 1; instance <= 16; instance++)
                {
                    int faceIndex = instance << 16;
                    if (FontEngine.LoadFontFace(sourcePath, SamplingPointSize, faceIndex) != FontEngineError.Success)
                    {
                        break;
                    }

                    if (FontEngine.GetFaceInfo().styleName == styleName)
                    {
                        return faceIndex;
                    }
                }

                return -1;
            }
            finally
            {
                FontEngine.UnloadAllFontFaces();
            }
        }

        /// <summary>Latin-1 (Western European languages) plus the few typographic signs the HUD writes (docs/09 §5).</summary>
        private static string CharacterSet()
        {
            var set = new StringBuilder();
            for (char c = (char)0x20; c <= 0x7E; c++)
            {
                set.Append(c);
            }

            for (char c = (char)0xA0; c <= 0xFF; c++)
            {
                set.Append(c);
            }

            // Stars, ticks and hearts are drawn icons, not glyphs: these typefaces do not carry them (docs/09 §5).
            set.Append("–—‘’“”…→"); // en dash, em dash, quotes, ellipsis, arrow
            return set.ToString();
        }

        private static string Describe(string missing)
        {
            var names = new List<string>();
            foreach (char c in missing)
            {
                names.Add($"U+{(int)c:X4}");
            }

            return string.Join(", ", names);
        }
    }
}
