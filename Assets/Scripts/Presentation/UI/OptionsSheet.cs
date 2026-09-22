using System;
using TowerDefense.Presentation.Settings;
using TowerDefense.Simulation;
using UnityEngine.UIElements;

namespace TowerDefense.Presentation.UI
{
    /// <summary>
    /// The options sheet (docs/09 §2.7, docs/05 §15.2) and the choices for the next run until the Home exists.
    /// Every change is saved in <see cref="PlayerOptions"/> at once; presentation only, so replays are unaffected.
    /// </summary>
    internal sealed class OptionsSheet
    {
        private readonly VisualElement _root;
        private readonly VisualElement _sheet;
        private readonly Toggle _reduceMotion;
        private readonly SliderInt _effects;
        private readonly Button _numbers;
        private readonly Toggle _haptics;
        private readonly Button _speed;
        private readonly Button _quality;
        private readonly Button _language;
        private readonly Button _core;
        private readonly Button _grade;
        private readonly Toggle _endless;
        private readonly Toggle _largeText;
        private readonly Toggle _contrast;

        public OptionsSheet(VisualElement root, Action optionsChanged, Action qualityChanged)
        {
            _root = root;
            _sheet = root.Q<VisualElement>("options");
            _reduceMotion = root.Q<Toggle>("opt-reduce-motion");
            _effects = root.Q<SliderInt>("opt-effects");
            _numbers = root.Q<Button>("opt-numbers");
            _haptics = root.Q<Toggle>("opt-haptics");
            _speed = root.Q<Button>("opt-speed");
            _quality = root.Q<Button>("opt-quality");
            _language = root.Q<Button>("opt-language");
            _core = root.Q<Button>("opt-core");
            _grade = root.Q<Button>("opt-grade");
            _endless = root.Q<Toggle>("opt-endless");
            _largeText = root.Q<Toggle>("opt-large-text");
            _contrast = root.Q<Toggle>("opt-contrast");

            _core.clicked += () => { PlayerOptions.Core = (CoreType)(((int)PlayerOptions.Core + 1) % 4); RefreshLabels(); };
            _grade.clicked += () => { PlayerOptions.Grade = (PlayerOptions.Grade + 1) % 4; RefreshLabels(); };
            _endless.RegisterValueChangedCallback(e => PlayerOptions.Endless = e.newValue);
            _largeText.RegisterValueChangedCallback(e => { PlayerOptions.LargeText = e.newValue; ApplyAccessibility(); });
            _contrast.RegisterValueChangedCallback(e => { PlayerOptions.HighContrast = e.newValue; ApplyAccessibility(); });
            ApplyAccessibility();

            root.Q<Button>("options-open").clicked += Open;
            root.Q<Button>("options-close").clicked += Close;
            _reduceMotion.RegisterValueChangedCallback(e => { PlayerOptions.ReduceMotion = e.newValue; optionsChanged(); });
            _effects.RegisterValueChangedCallback(e => { PlayerOptions.EffectIntensity = e.newValue; optionsChanged(); });
            _haptics.RegisterValueChangedCallback(e => { PlayerOptions.Haptics = e.newValue; optionsChanged(); });
            _numbers.clicked += () =>
            {
                PlayerOptions.DamageNumbers = (DamageNumbersMode)(((int)PlayerOptions.DamageNumbers + 1) % 3);
                RefreshLabels();
                optionsChanged();
            };
            _speed.clicked += () =>
            {
                PlayerOptions.DefaultSpeed = PlayerOptions.DefaultSpeed % 3 + 1;
                RefreshLabels();
                optionsChanged();
            };
            _quality.clicked += () =>
            {
                PlayerOptions.Quality = (QualityChoice)(((int)PlayerOptions.Quality + 1) % 4);
                qualityChanged();
                RefreshLabels();
            };
            _language.clicked += () =>
            {
                PlayerOptions.Language = (LanguageChoice)(((int)PlayerOptions.Language + 1) % 3);
                Loc.SetLanguage(Ui.ResolveLanguage(PlayerOptions.Language));
                RefreshLabels();
            };
        }

        public bool IsOpen => !_sheet.ClassListContains("hidden");

        /// <summary>
        /// Turns the two accessibility settings into classes on the root: the stylesheet does the rest, because
        /// both are only a different set of the same variables (docs/09 §6).
        /// </summary>
        private void ApplyAccessibility()
        {
            _root.EnableInClassList("text-large", PlayerOptions.LargeText);
            _root.EnableInClassList("theme-contrast", PlayerOptions.HighContrast);
            PlayerOptions.Save();
        }

        private void Open()
        {
            _largeText.SetValueWithoutNotify(PlayerOptions.LargeText);
            _contrast.SetValueWithoutNotify(PlayerOptions.HighContrast);
            _reduceMotion.SetValueWithoutNotify(PlayerOptions.ReduceMotion);
            _effects.SetValueWithoutNotify(PlayerOptions.EffectIntensity);
            _haptics.SetValueWithoutNotify(PlayerOptions.Haptics);
            _endless.SetValueWithoutNotify(PlayerOptions.Endless);
            RefreshLabels();
            Ui.Show(_sheet, true);
        }

        private void Close()
        {
            PlayerOptions.Save();
            Ui.Show(_sheet, false);
        }

        private void RefreshLabels()
        {
            Ui.SetText(_numbers, Ui.DamageNumbersLabel());
            Ui.SetText(_speed, $"{PlayerOptions.DefaultSpeed}x");
            string level = Loc.T(GraphicsQuality.Level switch
            {
                GraphicsQuality.Low => "quality.low",
                GraphicsQuality.High => "quality.high",
                _ => "quality.medium",
            });
            Ui.SetText(_quality, PlayerOptions.Quality == QualityChoice.Auto ? Loc.T("options.auto", level) : level);
            string language = Loc.T("language." + Loc.Language);
            Ui.SetText(_language, PlayerOptions.Language == LanguageChoice.Auto ? Loc.T("options.auto", language) : language);
            Ui.SetText(_core, Loc.CoreName(PlayerOptions.Core));
            Ui.SetText(_grade, Loc.T("options.grade_value", PlayerOptions.Grade));
        }
    }
}
