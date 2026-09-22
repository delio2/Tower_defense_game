using System;
using System.Collections.Generic;
using TowerDefense.Simulation;
using UnityEngine;
using UnityEngine.UIElements;

namespace TowerDefense.Presentation.UI
{
    /// <summary>
    /// UI Toolkit HUD (docs/09): reads the simulation and raises intents; never changes game state itself.
    /// Structure comes from Hud.uxml, tokens from Theme.uss. This class is the one face the runner and the input
    /// see; each part of the screen lives in its own panel: <see cref="WaveHud"/> (top bar, Pulse, speed, pause),
    /// <see cref="ShopPanel"/> (with <see cref="OfferCards"/> and <see cref="WavePreview"/>), <see cref="ArenaOverlay"/>
    /// (Integrity ring, edge markers, Guardian strip, numbers, inspector, toast), <see cref="WaveSummaryPanel"/>,
    /// <see cref="RunEndPanel"/>, <see cref="PauseSheet"/> and <see cref="OptionsSheet"/>.
    /// </summary>
    public sealed class HudView
    {
        public event Action RerollTapped;
        public event Action NextWaveTapped;
        public event Action UndoTapped;
        public event Action BuySlotTapped;
        public event Action SellTapped;
        public event Action MoveTapped;
        public event Action CloseTapped;
        public event Action PulseTapped;
        public event Action SpeedTapped;
        public event Action PauseTapped;
        public event Action PlayAgainTapped;

        /// <summary>Raised after any option changes (values are already saved in <see cref="Settings.PlayerOptions"/>).</summary>
        public event Action OptionsChanged;

        /// <summary>Raised when the graphics quality option changes; the label refreshes after the switch.</summary>
        public event Action QualityChanged;

        private readonly VisualElement _root;
        private readonly WaveHud _waveHud;
        private readonly ShopPanel _shop;
        private readonly ArenaOverlay _overlay;
        private readonly WaveSummaryPanel _summary;
        private readonly RunEndPanel _runEnd;
        private readonly PauseSheet _pause;
        private readonly OptionsSheet _options;

        /// <summary>Phase the HUD last drew, to catch the moment the shop opens and stagger the cards in.</summary>
        private GamePhase _lastPhase = GamePhase.Wave;

        public HudView(VisualElement root)
        {
            _root = root;
            Loc.Bind(root);
            _overlay = new ArenaOverlay(root);
            _waveHud = new WaveHud(root,
                pulse: () => PulseTapped?.Invoke(),
                speed: () => SpeedTapped?.Invoke(),
                pause: () => PauseTapped?.Invoke());
            _shop = new ShopPanel(root,
                sell: () => SellTapped?.Invoke(),
                move: () => MoveTapped?.Invoke(),
                close: () => CloseTapped?.Invoke(),
                undo: () => UndoTapped?.Invoke(),
                reroll: () => RerollTapped?.Invoke(),
                buySlot: () => BuySlotTapped?.Invoke(),
                next: () => NextWaveTapped?.Invoke());
            _summary = new WaveSummaryPanel(root);
            _runEnd = new RunEndPanel(root, () => PlayAgainTapped?.Invoke(), _overlay.ShowToast);
            _pause = new PauseSheet(root,
                resume: () => PauseTapped?.Invoke(),
                speed: () => SpeedTapped?.Invoke(),
                abandon: () => PlayAgainTapped?.Invoke(),
                optionsChanged: () => OptionsChanged?.Invoke());
            _options = new OptionsSheet(root, () => OptionsChanged?.Invoke(), () => QualityChanged?.Invoke());
        }

        public bool IsSummaryOpen => _summary.IsOpen;

        public bool IsOptionsOpen => _options.IsOpen;

        /// <summary>Size of the panel in its own coordinates (1080 x 1920 at the reference resolution).</summary>
        public Vector2 PanelSize => _root.layout.size;

        // ------------------------------------------------------------------ frame

        public void Refresh(GameSimulation sim, HudState state, Func<ModuleKind, string> describe)
        {
            _waveHud.RefreshTopBar(sim);
            _overlay.Tick();
            if (sim.Phase != _lastPhase)
            {
                if (sim.Phase == GamePhase.Shop)
                {
                    _shop.Cards.StaggerIn();
                }

                _lastPhase = sim.Phase;
            }

            _summary.Refresh();
            bool shop = sim.Phase == GamePhase.Shop;
            _shop.SetVisible(shop && !sim.IsOver && !IsSummaryOpen);
            _waveHud.SetControlsVisible(sim.Phase == GamePhase.Wave);
            if (shop)
            {
                _shop.Refresh(sim, state, describe);
            }
            else if (sim.Phase == GamePhase.Wave)
            {
                _waveHud.RefreshControls(sim, state);
            }

            _runEnd.Refresh(sim, state);
        }

        // ------------------------------------------------------------------ pointer and drag (ShopInput)

        /// <summary>True when a screen point (bottom-left origin) is over an interactive element of the HUD.</summary>
        public bool IsPointerOver(Vector2 screenPosition)
        {
            IPanel panel = _root.panel;
            if (panel == null)
            {
                return false;
            }

            VisualElement picked = panel.Pick(Ui.ToPanel(_root, screenPosition));
            return picked != null && picked != _root;
        }

        /// <summary>Offer card under a screen point, or -1. Sold cards do not count.</summary>
        public int CardAt(Vector2 screenPosition)
        {
            if (_root.panel == null || IsSummaryOpen || IsOptionsOpen || !_shop.CardsVisible)
            {
                return -1;
            }

            return _shop.Cards.CardAt(Ui.ToPanel(_root, screenPosition));
        }

        public bool IsOverSellZone(Vector2 screenPosition)
        {
            return _root.panel != null && _shop.IsOverSellZone(Ui.ToPanel(_root, screenPosition));
        }

        /// <summary>Panel position of a card centre, to float a ghost back to it.</summary>
        public Vector2 CardCentre(int index) => _shop.Cards.CardCentre(index);

        /// <summary>Shows the drag ghost above the finger.</summary>
        public void ShowGhost(string title, ModuleKind kind, Vector2 screenPosition) =>
            _shop.ShowGhost(title, kind, Ui.ToPanel(_root, screenPosition));

        /// <summary>Hides the ghost; with a return point it floats back first (soft return, no error flash).</summary>
        public void HideGhost(Vector2? returnToPanel = null) => _shop.HideGhost(returnToPanel);

        public void MarkEnemySeen(EnemyKind kind) => _shop.Preview.MarkEnemySeen(kind);

        // ------------------------------------------------------------------ overlays

        /// <summary>Opens the end-of-wave summary; the shop stays hidden until it closes or is tapped.</summary>
        public void ShowWaveSummary(int wave, long waveDamage, int creditsAfter, int creditsGained, int interest, Ring ring) =>
            _summary.Open(wave, waveDamage, creditsAfter, creditsGained, interest, ring);

        /// <summary>Shows the pause sheet with what this run is (docs/09 §2.8), or puts it away.</summary>
        public void SetPaused(bool paused, GameSimulation sim, int seed, int speed) => _pause.SetPaused(paused, sim, seed, speed);

        public void ShowToast(string text) => _overlay.ShowToast(text);

        // ------------------------------------------------------------------ arena overlay (panel space)

        /// <summary>Converts a world point to panel coordinates for floating labels.</summary>
        public Vector2 WorldToPanel(Camera camera, Vector3 world)
        {
            Vector3 screen = camera.WorldToScreenPoint(world);
            return Ui.ToPanel(_root, new Vector2(screen.x, screen.y));
        }

        public void SetCoreArc(Vector2? panelPosition, float fraction) => _overlay.SetCoreArc(panelPosition, fraction);

        public void SetEdgeMarkers(List<Vector2> positions) => _overlay.SetEdgeMarkers(positions);

        public void SetGuardian(string guardianName, float healthFraction) => _overlay.SetGuardian(guardianName, healthFraction);

        public void SetNumbers(List<FloatingLabel> labels) => _overlay.SetNumbers(labels);

        public void ShowInspector(Vector2 panelPosition, string title, string body) => _overlay.ShowInspector(panelPosition, title, body);

        public void HideInspector() => _overlay.HideInspector();
    }
}
