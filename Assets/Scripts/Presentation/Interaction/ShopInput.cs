using System;
using TowerDefense.Presentation.Arena;
using TowerDefense.Presentation.Settings;
using TowerDefense.Presentation.UI;
using TowerDefense.Simulation;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TowerDefense.Presentation.Interaction
{
    /// <summary>
    /// Pointer input for the arena and the shop (docs/03 B3, docs/07 §1.2): tap a card then a slot, drag a card onto
    /// a slot (buy or merge), drag a module onto another slot (move/swap) or the Sell zone, tap the Core for the
    /// Pulse during a wave. The dragged ghost stays above the finger, the magnet snaps within 0.45 units, and the
    /// preview comes from the simulation. Every state change is a command.
    /// </summary>
    internal sealed class ShopInput
    {
        private const float SlotPickRadius = 0.6f;
        private const float CorePickRadius = 0.9f;

        /// <summary>Tap radius around an enemy: 48 dp of touch target translated into arena units.</summary>
        private const float EnemyPickRadius = 0.75f;

        /// <summary>Magnet radius around a slot centre, world units (docs/07 §1.2).</summary>
        private const float MagnetRadius = 0.45f;

        /// <summary>Drag threshold: 8 dp (docs/07 §1.2); 1920 px reference = 640 dp.</summary>
        private const float DragThresholdDp = 8f;

        /// <summary>Long press opens the detail sheet (docs/07 §1.2).</summary>
        private const float LongPressSeconds = 0.25f;

        private enum DragKind : byte { None, Offer, Module }

        private readonly ArenaKit _kit;
        private readonly CameraRig _cameraRig;
        private readonly Func<HudView> _hud;
        private readonly Action<string> _showMessage;
        private readonly Action _tryPulse;

        /// <summary>Asked to describe what a wave tap landed on: a ring slot, or an enemy id (-1 = neither).</summary>
        private readonly Action<int, int> _inspect;

        private DragKind _pendingKind;
        private DragKind _dragKind;

        /// <summary>When the current press started, and whether it has already opened the sheet.</summary>
        private float _pressTime;
        private bool _longPressed;
        private int _dragIndex = -1;
        private Vector2 _pressScreen;
        private int _dragTargetSlot = -1;
        private bool _dragOverSell;

        public ShopInput(ArenaKit kit, CameraRig cameraRig, Func<HudView> hud, Action<string> showMessage, Action tryPulse,
            Action<int, int> inspect)
        {
            _kit = kit;
            _cameraRig = cameraRig;
            _hud = hud;
            _showMessage = showMessage;
            _tryPulse = tryPulse;
            _inspect = inspect;
        }

        public int SelectedOffer { get; private set; } = -1;
        public int SelectedSlot { get; private set; } = -1;
        public bool MoveMode { get; private set; }
        public string PreviewText { get; private set; }
        public int DragSourceOffer => _dragKind == DragKind.Offer ? _dragIndex : -1;
        public bool IsDraggingModule => _dragKind == DragKind.Module;

        /// <summary>
        /// Where a dragged weapon would reach, for the arena to draw: the slot it is hovering over and the range in
        /// world units. Null while nothing with a reach is being dragged (docs/06 2.5-C2).
        /// </summary>
        public (int Slot, float Range)? DragReach
        {
            get
            {
                if (_dragKind == DragKind.None || _dragTargetSlot < 0)
                {
                    return null;
                }

                ModuleDefinition definition = null;
                if (_dragKind == DragKind.Offer)
                {
                    ModuleKind? offer = Sim.OfferAt(_dragIndex);
                    definition = offer.HasValue ? Sim.Content.Module(offer.Value) : null;
                }
                else
                {
                    ModuleInstance module = Sim.Ring.At(_dragIndex);
                    definition = module?.Definition;
                }

                if (definition == null || definition.Category != ModuleCategory.Weapon)
                {
                    return null;
                }

                return (_dragTargetSlot, definition.RangeMilli / 1000f);
            }
        }
        public bool SellZoneHot => _dragOverSell;

        private GameSimulation Sim => _kit.Sim;

        public void Reset()
        {
            ClearSelection();
            ResetDrag();
        }

        public void ClearSelection()
        {
            SelectedOffer = -1;
            SelectedSlot = -1;
            MoveMode = false;
        }

        public void DeselectOffer() => SelectedOffer = -1;

        public void ToggleMoveMode() => MoveMode = !MoveMode;

        /// <summary>Whether a petal is lit: selected, drag target, valid drop, or free while a card is selected.</summary>
        public bool IsHighlighted(int slot)
        {
            bool validTarget = _dragKind != DragKind.None && IsValidDropSlot(slot);
            return slot == SelectedSlot || slot == _dragTargetSlot || validTarget
                || (SelectedOffer >= 0 && Sim.Ring.At(slot) == null);
        }

        public void HandlePointer()
        {
            Pointer pointer = Pointer.current;
            if (pointer == null)
            {
                return;
            }

            // Not an else-if chain: a fast tap can press and release within one frame.
            Vector2 screen = pointer.position.ReadValue();
            if (pointer.press.wasPressedThisFrame)
            {
                OnPress(screen);
            }

            if (pointer.press.isPressed && !pointer.press.wasReleasedThisFrame && _pendingKind != DragKind.None)
            {
                OnHold(screen);
            }

            if (pointer.press.wasReleasedThisFrame && _pendingKind != DragKind.None)
            {
                OnRelease(screen);
            }
        }

        private void OnPress(Vector2 screen)
        {
            if (Sim.IsOver)
            {
                return;
            }

            HudView hud = _hud();
            _pressScreen = screen;
            _pressTime = Time.unscaledTime;
            _longPressed = false;
            if (Sim.Phase == GamePhase.Shop)
            {
                int card = hud != null ? hud.CardAt(screen) : -1;
                if (card >= 0)
                {
                    _pendingKind = DragKind.Offer;
                    _dragIndex = card;
                    return;
                }
            }

            if (hud != null && hud.IsPointerOver(screen))
            {
                return; // buttons are handled by UI Toolkit
            }

            Vector3 world = _cameraRig.ScreenToWorld(screen);
            if (Sim.Phase == GamePhase.Wave)
            {
                if (new Vector2(world.x, world.z).magnitude < CorePickRadius)
                {
                    _tryPulse();
                    return;
                }

                // Anything else the finger lands on is asked to explain itself, read-only (docs/09 §2.2).
                _inspect?.Invoke(PickSlot(world), PickEnemy(world));
                return;
            }

            int slot = PickSlot(world);
            if (slot >= 0 && Sim.Ring.At(slot) != null && !MoveMode && SelectedOffer < 0)
            {
                // A press on a module may become a drag; a plain tap selects it on release.
                _pendingKind = DragKind.Module;
                _dragIndex = slot;
                return;
            }

            TapArena(slot);
        }

        /// <summary>
        /// What the finger is resting on, in full: a module on the ring with its levels and what selling it returns,
        /// or an offer card with what it does (docs/06 2.5-C4). Read-only, like every inspector.
        /// </summary>
        private void ShowSheet()
        {
            if (_pendingKind == DragKind.Offer)
            {
                ModuleKind? offer = Sim.OfferAt(_dragIndex);
                if (offer.HasValue)
                {
                    _inspect?.Invoke(-2 - _dragIndex, -1);
                }

                return;
            }

            if (_pendingKind == DragKind.Module)
            {
                _inspect?.Invoke(_dragIndex, -1);
            }
        }

        /// <summary>
        /// The enemy nearest the tap within <see cref="EnemyPickRadius"/> (48 dp at the reference height), or -1.
        /// Enemies move, so the pick is generous; the nearest one wins when several overlap.
        /// </summary>
        private int PickEnemy(Vector3 world)
        {
            int best = -1;
            float bestDistance = EnemyPickRadius;
            foreach (Enemy enemy in Sim.Enemies)
            {
                enemy.GetPosition(out long x, out long y);
                var at = new Vector2(x / (float)SimConstants.Micro, y / (float)SimConstants.Micro);
                float distance = Vector2.Distance(at, new Vector2(world.x, world.z));
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = enemy.Id;
                }
            }

            return best;
        }

        /// <summary>The no-drag shop interaction: tap a card, then a slot; or Move mode.</summary>
        private void TapArena(int slot)
        {
            if (slot < 0)
            {
                SelectedSlot = -1;
                MoveMode = false;
                return;
            }

            if (MoveMode && SelectedSlot >= 0)
            {
                Sim.Enqueue(Command.Move(SelectedSlot, slot));
                MoveMode = false;
                SelectedSlot = slot;
                return;
            }

            if (SelectedOffer >= 0)
            {
                Sim.Enqueue(Command.Buy(SelectedOffer, slot));
                SelectedOffer = -1;
                return;
            }

            SelectedSlot = Sim.Ring.At(slot) != null && SelectedSlot != slot ? slot : -1;
        }

        private void OnHold(Vector2 screen)
        {
            HudView hud = _hud();
            if (_dragKind == DragKind.None)
            {
                float thresholdPixels = DragThresholdDp * Screen.height / 640f;
                if ((screen - _pressScreen).sqrMagnitude < thresholdPixels * thresholdPixels)
                {
                    // A finger that stays put is asking about the thing under it, not moving it (docs/09 §2.3).
                    if (!_longPressed && Time.unscaledTime - _pressTime >= LongPressSeconds)
                    {
                        _longPressed = true;
                        Haptics.Light();
                        ShowSheet();
                    }

                    return;
                }

                _dragKind = _pendingKind;
                ClearSelection();
                Haptics.Light();
            }

            Vector3 world = _cameraRig.ScreenToWorld(screen);
            int previousTarget = _dragTargetSlot;
            int nearest = NearestSlot(world, MagnetRadius);
            _dragTargetSlot = nearest >= 0 && IsValidDropSlot(nearest) ? nearest : -1;
            if (_dragTargetSlot >= 0 && _dragTargetSlot != previousTarget)
            {
                Haptics.Light(); // magnet tick
            }

            _dragOverSell = _dragKind == DragKind.Module && hud != null && hud.IsOverSellZone(screen);
            PreviewText = BuildPreview();
            if (hud == null)
            {
                return;
            }

            if (_dragKind == DragKind.Offer)
            {
                ModuleKind? offer = Sim.OfferAt(_dragIndex);
                if (offer == null)
                {
                    CancelDrag();
                    return;
                }

                hud.ShowGhost(Loc.ModuleName(offer.Value), offer.Value, screen);
            }
            else
            {
                ModuleInstance module = Sim.Ring.At(_dragIndex);
                if (module == null)
                {
                    CancelDrag();
                    return;
                }

                hud.ShowGhost(UiText.ModuleTitle(module), module.Kind, screen);
            }
        }

        private void OnRelease(Vector2 screen)
        {
            if (_dragKind == DragKind.None)
            {
                // A tap: cards select an offer (or merge at once); modules open their panel.
                if (_pendingKind == DragKind.Offer)
                {
                    OnOfferTapped(_dragIndex);
                }
                else
                {
                    TapArena(_dragIndex);
                }

                ResetDrag();
                return;
            }

            bool dropped = false;
            if (_dragKind == DragKind.Offer)
            {
                // Only a release on a valid, magnet-snapped slot buys; anywhere else floats the card back (docs/03 B3).
                if (_dragTargetSlot >= 0)
                {
                    CommandResult result = Sim.Validate(Command.Buy(_dragIndex, _dragTargetSlot));
                    if (result == CommandResult.Ok)
                    {
                        Sim.Enqueue(Command.Buy(_dragIndex, _dragTargetSlot));
                        dropped = true;
                    }
                    else
                    {
                        _showMessage(UiText.DescribeRejection(result));
                    }
                }
            }
            else if (_dragOverSell)
            {
                Sim.Enqueue(Command.Sell(_dragIndex));
                dropped = true;
            }
            else if (_dragTargetSlot >= 0 && _dragTargetSlot != _dragIndex)
            {
                Sim.Enqueue(Command.Move(_dragIndex, _dragTargetSlot));
                dropped = true;
            }

            if (!dropped)
            {
                CancelDrag();
                return;
            }

            Haptics.Medium();
            _hud()?.HideGhost();
            ResetDrag();
        }

        /// <summary>Invalid release: the ghost floats back to where it came from (docs/03 B3 rule 5).</summary>
        private void CancelDrag()
        {
            HudView hud = _hud();
            if (hud != null)
            {
                Vector2 origin = _dragKind == DragKind.Offer
                    ? hud.CardCentre(_dragIndex)
                    : hud.WorldToPanel(_cameraRig.Camera, _kit.SlotWorld(_dragIndex));
                hud.HideGhost(origin);
            }

            ResetDrag();
        }

        private void ResetDrag()
        {
            _pendingKind = DragKind.None;
            _dragKind = DragKind.None;
            _dragIndex = -1;
            _dragTargetSlot = -1;
            _dragOverSell = false;
            PreviewText = null;
        }

        /// <summary>
        /// Where the dragged object may land: a new card on any empty slot, a duplicate card only on its merge target,
        /// a ring module on any other slot (swap).
        /// </summary>
        private bool IsValidDropSlot(int slot)
        {
            if (_dragKind == DragKind.Offer)
            {
                int mergeSlot = MergeTargetSlot(_dragIndex);
                return mergeSlot >= 0 ? slot == mergeSlot : Sim.Ring.At(slot) == null;
            }

            return _dragKind == DragKind.Module && slot != _dragIndex;
        }

        private int MergeTargetSlot(int offer)
        {
            ModuleKind? kind = Sim.OfferAt(offer);
            return kind.HasValue ? Sim.Ring.FindMergeTarget(kind.Value)?.Slot ?? -1 : -1;
        }

        /// <summary>The effect of the pending drop, computed by the simulation previews (what you see is what happens).</summary>
        private string BuildPreview()
        {
            long before = Sim.RingDps();
            if (_dragKind == DragKind.Offer)
            {
                int slot = _dragTargetSlot;
                if (slot < 0)
                {
                    return null;
                }

                if (!Sim.TryPreviewBuy(_dragIndex, slot, out long after, out bool merges))
                {
                    return UiText.DescribeRejection(Sim.Validate(Command.Buy(_dragIndex, slot)));
                }

                ModuleInstance target = merges ? Sim.Ring.At(slot) : null;
                string level = target != null ? Loc.T("shop.preview_level", target.Level + 1) : string.Empty;
                return level + UiText.DpsChange(before, after);
            }

            if (_dragOverSell && Sim.TryPreviewSell(_dragIndex, out long afterSell, out int refund))
            {
                return Loc.T("shop.preview_sell", refund) + UiText.DpsChange(before, afterSell);
            }

            if (_dragTargetSlot >= 0 && _dragTargetSlot != _dragIndex && Sim.TryPreviewMove(_dragIndex, _dragTargetSlot, out long afterMove))
            {
                return UiText.DpsChange(before, afterMove);
            }

            return null;
        }

        private void OnOfferTapped(int index)
        {
            ModuleKind? offer = Sim.OfferAt(index);
            if (offer == null)
            {
                return;
            }

            if (Sim.Ring.FindMergeTarget(offer.Value) != null)
            {
                Sim.Enqueue(Command.Buy(index, 0));
                SelectedOffer = -1;
                return;
            }

            if (!Sim.Ring.HasFreeSlot())
            {
                _showMessage(Loc.T("msg.ring_full"));
                return;
            }

            SelectedOffer = SelectedOffer == index ? -1 : index;
            SelectedSlot = -1;
            if (SelectedOffer >= 0)
            {
                _showMessage(Loc.T("msg.tap_slot"));
            }
        }

        private int NearestSlot(Vector3 world, float radius)
        {
            int best = -1;
            float bestDistance = radius;
            for (int slot = 0; slot < Sim.Ring.SlotCount; slot++)
            {
                float distance = Vector3.Distance(new Vector3(world.x, 0f, world.z), _kit.SlotWorld(slot));
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = slot;
                }
            }

            return best;
        }

        private int PickSlot(Vector3 world)
        {
            for (int slot = 0; slot < Sim.Ring.SlotCount; slot++)
            {
                if (Vector3.Distance(new Vector3(world.x, 0f, world.z), _kit.SlotWorld(slot)) < SlotPickRadius)
                {
                    return slot;
                }
            }

            return -1;
        }
    }
}
