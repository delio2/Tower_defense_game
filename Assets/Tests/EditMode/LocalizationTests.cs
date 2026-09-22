using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using TowerDefense.Presentation.UI;
using UnityEngine;

namespace TowerDefense.Simulation.Tests
{
    /// <summary>
    /// The string table (Assets/UI/Resources/Strings.txt, <see cref="Loc"/>) must answer every key the game asks for,
    /// in both languages, with characters the fonts can draw. A missing key would show its name on a player's screen.
    /// </summary>
    public class LocalizationTests
    {
        private static Dictionary<string, string[]> Table()
        {
            var asset = Resources.Load<TextAsset>(Loc.TableResource);
            Assert.IsNotNull(asset, "Assets/UI/Resources/Strings.txt");
            return Loc.Parse(asset.text);
        }

        [Test]
        public void StringTable_EveryKeyHasEnglishAndItalian()
        {
            foreach (KeyValuePair<string, string[]> row in Table())
            {
                Assert.IsNotEmpty(row.Value[0].Trim(), $"{row.Key}: english");
                Assert.IsNotEmpty(row.Value[1].Trim(), $"{row.Key}: italian");
                Assert.AreEqual(Regex.Matches(row.Value[0], @"\{\d\}").Count, Regex.Matches(row.Value[1], @"\{\d\}").Count,
                    $"{row.Key}: both languages take the same arguments");
            }
        }

        [Test]
        public void StringTable_UsesOnlyCharactersTheFontsDraw()
        {
            foreach (KeyValuePair<string, string[]> row in Table())
            {
                foreach (string text in row.Value)
                {
                    foreach (char c in text)
                    {
                        // Latin-1 is baked in the atlases; the arrow is drawn by the Outfit fallback (D34).
                        Assert.IsTrue(c <= 'ÿ' || c == '→', $"{row.Key}: '{c}' (U+{(int)c:X4}) is outside Latin-1");
                    }
                }
            }
        }

        [Test]
        public void StringTable_CoversEveryKeyTheGameUses()
        {
            Dictionary<string, string[]> table = Table();
            var missing = new List<string>();

            string uxml = File.ReadAllText(Path.Combine(Application.dataPath, "UI/Resources/Hud.uxml"));
            foreach (Match m in Regex.Matches(uxml, "text=\"@([a-z_.A-Z]+)\""))
            {
                Check(m.Groups[1].Value);
            }

            string scripts = Path.Combine(Application.dataPath, "Scripts/Presentation");
            foreach (string file in Directory.GetFiles(scripts, "*.cs", SearchOption.AllDirectories))
            {
                foreach (Match m in Regex.Matches(File.ReadAllText(file), "Loc\\.T\\(\"([a-z_.A-Z]+)\""))
                {
                    Check(m.Groups[1].Value);
                }

                foreach (Match m in Regex.Matches(File.ReadAllText(file), "\"((?:hud|shop|quality|numbers|reject)\\.[a-z_]+)\""))
                {
                    Check(m.Groups[1].Value);
                }
            }

            // Keys built from names: every module, enemy, Core and language has its entries.
            foreach (ModuleKind kind in Enum.GetValues(typeof(ModuleKind)))
            {
                Check("module." + kind);
                Check("module." + kind + ".desc");
            }

            foreach (EnemyKind kind in Enum.GetValues(typeof(EnemyKind)))
            {
                Check("enemy." + kind);
            }

            foreach (CoreType core in Enum.GetValues(typeof(CoreType)))
            {
                Check("core." + core);
            }

            foreach (GameLanguage language in Enum.GetValues(typeof(GameLanguage)))
            {
                Check("language." + language);
            }

            Assert.IsEmpty(missing, "missing keys: " + string.Join(", ", missing));

            void Check(string key)
            {
                if (key.EndsWith(".", StringComparison.Ordinal))
                {
                    return; // a prefix completed at run time ("module." + kind): the enum loops below cover it
                }

                if (!table.ContainsKey(key) && !missing.Contains(key))
                {
                    missing.Add(key);
                }
            }
        }

        [Test]
        public void Loc_SwitchesLanguageAndFormatsArguments()
        {
            GameLanguage before = Loc.Language;
            try
            {
                Loc.SetLanguage(GameLanguage.English);
                Assert.AreEqual("Wave 3/18", Loc.T("hud.wave", 3, 18));
                Loc.SetLanguage(GameLanguage.Italian);
                Assert.AreEqual("Ondata 3/18", Loc.T("hud.wave", 3, 18));
            }
            finally
            {
                Loc.SetLanguage(before);
            }
        }
    }
}
