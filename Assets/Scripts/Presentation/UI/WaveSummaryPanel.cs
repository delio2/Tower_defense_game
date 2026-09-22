using System.Collections.Generic;
using TowerDefense.Simulation;
using UnityEngine;
using UnityEngine.UIElements;

namespace TowerDefense.Presentation.UI
{
    /// <summary>
    /// The end-of-wave summary v2 (docs/06 2.5-D1): damage rolls up, Credits add up with the interest called out, then
    /// the damage share of the top modules fills in; it closes by itself and a tap skips it (docs/03 B4).
    /// </summary>
    internal sealed class WaveSummaryPanel
    {
        /// <summary>How many modules the summary lists.</summary>
        private const int MaxShares = 4;

        /// <summary>Timeline: damage rolls 0–0.8 s, coins add 0.8–1.6 s, bars fill until 2.2 s, then it closes.</summary>
        private const float DamageEnd = 0.8f;
        private const float CoinsEnd = 1.6f;
        private const float End = 2.2f;

        private readonly VisualElement _summary;
        private readonly Label _title;
        private readonly Label _damage;
        private readonly Label _credits;
        private readonly VisualElement _sharesRoot;
        private readonly List<VisualElement> _sharePool = new List<VisualElement>();
        private readonly List<ModuleShare> _shares = new List<ModuleShare>();
        private float _start = -1f;
        private int _wave;
        private long _damageValue;
        private int _creditsAfter;
        private int _creditsGained;
        private int _interest;

        public WaveSummaryPanel(VisualElement root)
        {
            _summary = root.Q<VisualElement>("wave-summary");
            _title = root.Q<Label>("summary-title");
            _damage = root.Q<Label>("summary-damage");
            _credits = root.Q<Label>("summary-credits");
            _sharesRoot = root.Q<VisualElement>("summary-shares");
            _summary.RegisterCallback<PointerDownEvent>(_ => Close());
        }

        public bool IsOpen => _start >= 0f;

        /// <summary>Opens the summary; the shop stays hidden until it closes or is tapped.</summary>
        public void Open(int wave, long waveDamage, int creditsAfter, int creditsGained, int interest, Ring ring)
        {
            CollectShares(ring);
            _start = Time.time;
            _wave = wave;
            _damageValue = waveDamage;
            _creditsAfter = creditsAfter;
            _creditsGained = creditsGained;
            _interest = interest;
            Ui.Show(_summary, true);
        }

        private void Close()
        {
            _start = -1f;
            Ui.Show(_summary, false);
        }

        public void Refresh()
        {
            if (!IsOpen)
            {
                return;
            }

            float t = Time.time - _start;
            if (t >= End)
            {
                Close();
                return;
            }

            int gained = _creditsGained;
            Ui.SetText(_title, Loc.T("summary.title", _wave));
            float damageT = Mathf.Clamp01(t / DamageEnd);
            long shownDamage = (long)(_damageValue * Ui.EaseOut(damageT));
            Ui.SetText(_damage, Loc.T("summary.damage", NumberFormat.CompactHundredths(shownDamage)));
            float coinT = Mathf.Clamp01((t - DamageEnd) / (CoinsEnd - DamageEnd));
            int shownGain = Mathf.RoundToInt(gained * coinT);
            string interest = _interest > 0 && coinT >= 1f ? Loc.T("summary.interest", _interest) : string.Empty;
            Ui.SetText(_credits, Loc.T("summary.credits", _creditsAfter - gained + shownGain, shownGain, interest));

            // The bars fill last, after the numbers have landed.
            float shareT = Mathf.Clamp01((t - CoinsEnd) / (End - CoinsEnd));
            for (int i = 0; i < _shares.Count && i < _sharePool.Count; i++)
            {
                VisualElement track = _sharePool[i][1];
                track[0].style.width = Length.Percent(_shares[i].Share * 100f * Ui.EaseOut(shareT));
            }
        }

        /// <summary>
        /// The modules that did damage this wave, biggest first, as shares of the total. Only the top few: the
        /// panel answers "what carried this wave", not "here is a table".
        /// </summary>
        private void CollectShares(Ring ring)
        {
            _shares.Clear();
            long total = 0;
            for (int slot = 0; slot < ring.SlotCount; slot++)
            {
                ModuleInstance module = ring.At(slot);
                if (module != null && module.DamageThisWave > 0)
                {
                    total += module.DamageThisWave;
                    _shares.Add(new ModuleShare(module.Kind, module.DamageThisWave, 0f));
                }
            }

            _shares.Sort((a, b) => b.Damage.CompareTo(a.Damage));
            if (_shares.Count > MaxShares)
            {
                _shares.RemoveRange(MaxShares, _shares.Count - MaxShares);
            }

            for (int i = 0; i < _shares.Count; i++)
            {
                _shares[i] = new ModuleShare(_shares[i].Kind, _shares[i].Damage,
                    total > 0 ? _shares[i].Damage / (float)total : 0f);
            }

            for (int i = 0; i < _shares.Count; i++)
            {
                while (_sharePool.Count <= i)
                {
                    VisualElement row = BuildShareRow();
                    _sharesRoot.Add(row);
                    _sharePool.Add(row);
                }

                VisualElement built = _sharePool[i];
                built.style.display = DisplayStyle.Flex;
                Ui.SetText((Label)built[0], $"{Loc.ModuleName(_shares[i].Kind)}  {Mathf.RoundToInt(_shares[i].Share * 100f)}%");
                built[1][0].style.width = Length.Percent(0f);
            }

            for (int i = _shares.Count; i < _sharePool.Count; i++)
            {
                _sharePool[i].style.display = DisplayStyle.None;
            }
        }

        private static VisualElement BuildShareRow()
        {
            var row = new VisualElement { pickingMode = PickingMode.Ignore };
            row.AddToClassList("share");
            var name = new Label { pickingMode = PickingMode.Ignore };
            name.AddToClassList("share__name");
            var track = new VisualElement { pickingMode = PickingMode.Ignore };
            track.AddToClassList("share__track");
            var fill = new VisualElement { pickingMode = PickingMode.Ignore };
            fill.AddToClassList("share__fill");
            track.Add(fill);
            row.Add(name);
            row.Add(track);
            return row;
        }
    }
}
