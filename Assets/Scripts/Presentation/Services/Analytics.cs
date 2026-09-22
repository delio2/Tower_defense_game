using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

namespace TowerDefense.Presentation.Services
{
    /// <summary>
    /// Analytics behind an interface (docs/05 §16, D13): during development a local fake writes every event to a file
    /// on the device; the real service (D24: GameAnalytics or Firebase, after UMP consent, D16) replaces it in Phase 5
    /// without touching the callers. Presentation only — the simulation never knows.
    /// </summary>
    public interface IAnalytics
    {
        void Track(string name, params (string Key, object Value)[] fields);
    }

    /// <summary>
    /// The development fake: one JSON object per line in <c>persistentDataPath/sessions/&lt;start&gt;.jsonl</c>, one
    /// file per app session, appended and flushed at every event so a crash loses nothing. Pulled from a test phone
    /// by <c>Tools/playtest/report.py</c>, which turns it into the Gate numbers (docs/13).
    /// </summary>
    public sealed class LocalAnalytics : IAnalytics
    {
        public const string Folder = "sessions";

        private readonly string _path;
        private readonly StringBuilder _line = new StringBuilder(256);
        private readonly float _start;

        public LocalAnalytics()
        {
            string folder = Path.Combine(Application.persistentDataPath, Folder);
            Directory.CreateDirectory(folder);
            _path = Path.Combine(folder, DateTime.Now.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture) + ".jsonl");
            _start = Time.realtimeSinceStartup;
        }

        public string FilePath => _path;

        public void Track(string name, params (string Key, object Value)[] fields)
        {
            _line.Clear();
            _line.Append("{\"t\":").Append((Time.realtimeSinceStartup - _start).ToString("0.00", CultureInfo.InvariantCulture))
                 .Append(",\"event\":\"").Append(name).Append('"');
            foreach ((string key, object value) in fields)
            {
                _line.Append(",\"").Append(key).Append("\":");
                AppendValue(value);
            }

            _line.Append('}');
            string json = _line.ToString();
            try
            {
                File.AppendAllText(_path, json + "\n");
            }
            catch (IOException e)
            {
                Debug.LogWarning($"[EVT] could not write {_path}: {e.Message}");
            }
        }

        private void AppendValue(object value)
        {
            switch (value)
            {
                case null:
                    _line.Append("null");
                    break;
                case bool b:
                    _line.Append(b ? "true" : "false");
                    break;
                case int or long or short or byte:
                    _line.Append(Convert.ToInt64(value, CultureInfo.InvariantCulture));
                    break;
                case float f:
                    _line.Append(f.ToString("0.###", CultureInfo.InvariantCulture));
                    break;
                case double d:
                    _line.Append(d.ToString("0.###", CultureInfo.InvariantCulture));
                    break;
                default:
                    _line.Append('"').Append(value.ToString().Replace("\\", "\\\\").Replace("\"", "\\\"")).Append('"');
                    break;
            }
        }
    }

    /// <summary>Accepts everything and keeps nothing (tests, and before consent once UMP exists).</summary>
    public sealed class NullAnalytics : IAnalytics
    {
        public void Track(string name, params (string Key, object Value)[] fields)
        {
        }
    }
}
