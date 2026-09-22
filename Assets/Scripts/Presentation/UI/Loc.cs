using System;
using System.Collections.Generic;
using System.Globalization;
using TowerDefense.Simulation;
using UnityEngine;
using UnityEngine.UIElements;

namespace TowerDefense.Presentation.UI
{
    public enum GameLanguage : byte
    {
        English = 0,
        Italian = 1,
    }

    /// <summary>
    /// Keyed player-facing strings, English and Italian (D7, docs/06 3c; presentation rule "strings by key, never text
    /// in code"). The table is <c>Assets/UI/Resources/Strings.txt</c>: one line per key, <c>key TAB english TAB
    /// italian</c>, <c>#</c> for comments, <c>\n</c> for a line break, <c>{0}</c> for arguments. UXML labels whose
    /// text is <c>@key</c> are filled by <see cref="Bind"/> and follow a language change. Names of modules, enemies
    /// and Cores are keys too, so the renames proposed in docs/11 §1 are one table edit.
    /// Text must stay ASCII/Latin-1: the font atlases hold nothing else (presentation rules).
    /// </summary>
    public static class Loc
    {
        public const string TableResource = "Strings";

        private static Dictionary<string, string[]> _table;
        private static readonly HashSet<string> MissingReported = new HashSet<string>();
        private static readonly List<(TextElement Element, string Key)> Bound = new List<(TextElement, string)>();
        private static readonly Dictionary<ModuleKind, string> ModuleNames = new Dictionary<ModuleKind, string>();
        private static readonly Dictionary<EnemyKind, string> EnemyNames = new Dictionary<EnemyKind, string>();

        public static GameLanguage Language { get; private set; } = GameLanguage.English;

        /// <summary>Raised after the language changes (bound UXML labels are already updated).</summary>
        public static event Action Changed;

        /// <summary>The phone's language when it is one we have, English otherwise.</summary>
        public static GameLanguage SystemLanguage =>
            Application.systemLanguage == UnityEngine.SystemLanguage.Italian ? GameLanguage.Italian : GameLanguage.English;

        public static void SetLanguage(GameLanguage language)
        {
            Language = language;
            ModuleNames.Clear();
            EnemyNames.Clear();
            foreach ((TextElement element, string key) in Bound)
            {
                element.text = T(key);
            }

            Changed?.Invoke();
        }

        public static string T(string key)
        {
            EnsureLoaded();
            if (_table.TryGetValue(key, out string[] texts))
            {
                string text = texts[(int)Language];
                return string.IsNullOrEmpty(text) ? texts[0] : text;
            }

            if (MissingReported.Add(key))
            {
                Debug.LogWarning($"Missing string key '{key}' in {TableResource}.txt");
            }

            return key;
        }

        public static string T(string key, object a) => string.Format(CultureInfo.InvariantCulture, T(key), a);

        public static string T(string key, object a, object b) => string.Format(CultureInfo.InvariantCulture, T(key), a, b);

        public static string T(string key, params object[] args) => string.Format(CultureInfo.InvariantCulture, T(key), args);

        public static string ModuleName(ModuleKind kind)
        {
            if (!ModuleNames.TryGetValue(kind, out string name))
            {
                name = T("module." + kind);
                ModuleNames[kind] = name;
            }

            return name;
        }

        public static string EnemyName(EnemyKind kind)
        {
            if (!EnemyNames.TryGetValue(kind, out string name))
            {
                name = T("enemy." + kind);
                EnemyNames[kind] = name;
            }

            return name;
        }

        public static string CoreName(CoreType core) => T("core." + core);

        /// <summary>Replaces every <c>@key</c> text under <paramref name="root"/> and keeps it in step with the language.</summary>
        public static void Bind(VisualElement root)
        {
            Bound.Clear(); // one HUD at a time: a rebuilt HUD must not keep the old one's labels alive
            root.Query<TextElement>().ForEach(element =>
            {
                string text = element.text;
                if (text != null && text.Length > 1 && text[0] == '@')
                {
                    string key = text.Substring(1);
                    Bound.Add((element, key));
                    element.text = T(key);
                }
            });
        }

        private static void EnsureLoaded()
        {
            if (_table != null)
            {
                return;
            }

            var asset = Resources.Load<TextAsset>(TableResource);
            _table = asset != null ? Parse(asset.text) : new Dictionary<string, string[]>();
            if (asset == null)
            {
                Debug.LogError($"String table missing: Assets/UI/Resources/{TableResource}.txt");
            }
        }

        /// <summary>Parses the table; a malformed line throws, so a broken table fails its test, not a player.</summary>
        public static Dictionary<string, string[]> Parse(string text)
        {
            var table = new Dictionary<string, string[]>(StringComparer.Ordinal);
            string[] lines = text.Replace("\r\n", "\n").Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (line.Trim().Length == 0 || line.TrimStart().StartsWith("#", StringComparison.Ordinal))
                {
                    continue;
                }

                string[] columns = line.Split('\t');
                if (columns.Length != 3)
                {
                    throw new FormatException($"{TableResource}.txt line {i + 1}: expected key, english, italian separated by tabs");
                }

                string key = columns[0].Trim();
                if (table.ContainsKey(key))
                {
                    throw new FormatException($"{TableResource}.txt line {i + 1}: duplicate key '{key}'");
                }

                table[key] = new[] { Unescape(columns[1]), Unescape(columns[2]) };
            }

            return table;
        }

        private static string Unescape(string text) => text.Replace("\\n", "\n");
    }
}
