# 09 — Mobile UI/UX (Phase 1 and 3 brief)

> Version 3 · 2026-09-22 · Status: brief; every screen below is now mocked up, interactive, in the **Dusk Garden design system** (https://claude.ai/artifact/5aKRr9H8e3WJsMui64MNnu) — the mockups are the visual reference, this file keeps the rules. Screen structure, navigation, components and interface micro-interactions.
> Does not repeat: the second-by-second FTUE (`07` §1.5), the **game** micro-animation catalogue (`03` A7), in-arena readability (`03` A8), the drag rules (`03` B3). It references them.
> Layout reference: **1080 × 1920 (9:16)**, measures in **dp** (1 dp ≈ 3 px at this density). **PROPOSAL** items are listed in `04` §Open decisions.

---

## 0. Seven interface rules
1. **One primary action per screen**, always in the bottom third, always the same colour (solid ivory). Everything else is secondary (outline) or tertiary (text).
2. **The game is the Home.** There is no menu "in front of" the game: the breathing Core is the first thing seen, always.
3. **No modal windows with "OK".** Information lives in sheets that rise from the bottom and close by dragging or tapping outside. Irreversible actions are confirmed by **holding** (1 s), never with a dialog.
4. **At most two levels of depth** from any point (Home → Archive → node detail). The system "back" gesture works everywhere and never asks "are you sure?".
5. **Little text, many shapes.** Every main action has an icon **and** a short label (≤ 12 characters in every language). Numbers are big, words are small.
6. **Dark glass, not panels.** The interface is a veil (ivory at 6%, 1 px border at 12%, radius 24 dp) over the arena, never an opaque rectangle hiding it.
7. **Everything responds within ≤ 100 ms** (touch visual state) and nothing lasts more than 300 ms (transitions). The player never waits for the interface.

---

## 1. Navigation map
```
                 ┌──────────── Options (sheet) ────────────┐
                 │                                          │
  Home ──▶ Run (Shop ⇄ Wave) ──▶ Run end ──▶ Home           │
   │  ▲                     │                               │
   │  │                  Pause (veil)                       │
   ├──┼──▶ Daily / Weekly (same Run, fixed seed)            │
   ├──┼──▶ Archive ──▶ node detail (sheet)                  │
   ├──┼──▶ Leaderboards ──▶ replay / ghost                  │
   └──┴──▶ Pass (model B only, after soft launch) ──────────┘
```
- **Transitions:** Home ⇄ secondary screens = fade + scale 0.98 → 1 (200 ms). Shop ⇄ Wave = camera move (600 ms) and cards entering/leaving: **no screen change**, it is the same world.
- **Back** (gesture or button): closes the open sheet; from the Run opens pause; from Home exits the app (standard Android behaviour, no confirmation).
- **No visible loading screen:** there is one scene; Archive and leaderboards are sheets over the arena.

---

## 2. The screens

### 2.1 Home — "the Core is waiting for you"
```
┌──────────────────────────────┐ 0
│ ⚙                    ◆ 128   │  ← gear (options) · Blueprints (tap → Archive)
│                              │
│                              │
│          ·  ·  ·  ·          │  ← arena rings, still, at 15%
│       ·    ╭────╮    ·       │
│      ·    (  ◉   )    ·      │  ← the Core with the last ring played (or the default Emitter)
│       ·    ╰────╯    ·       │     swipe left/right on the Core = change Core type
│          ·  ·  ·  ·          │
│                              │
│   ‹ Standard ›   Grade 2 ±   │  ← Core type name · Grade stepper (only once unlocked)
│                              │
│ ┌──────────────────────────┐ │
│ │ Daily · 1,284 players    │ │  ← Daily card: your best result or "not played yet"
│ │ You: —      Best: 18     │ │     (appears from run 2)
│ └──────────────────────────┘ │
│ ┌──────────────────────────┐ │
│ │        ▶  Play           │ │  ← primary action, 64 dp high, "breathes" when it is the only one
│ └──────────────────────────┘ │
│   Archive    Leaderboards    │  ← secondary, text + icon, 48 dp
└──────────────────────────────┘ 1920
```
- No banner, no news, no popup, no shop in sight. The Home does not "sell": it shows the game.
- **Resume:** with a saved run the primary button becomes **"Resume · Wave 7"** and "New run" appears below as secondary.
- **Progressive disclosure** (§3): at first start the Home **does not exist**: the player goes straight into the run. It appears after the first run, and its elements light up one at a time.

### 2.2 In-game — Wave
```
┌──────────────────────────────┐
│ ♥ 100        Wave 7/18   ◈ 12│  ← three numbers, 20 sp, ivory; nothing else at the top
│                              │
│        (whole arena,         │
│        camera at 9.4 u,      │
│        integrity = arc       │  ← built (2.5-B3): a ring around the Core, drawn in panel space
│        around the Core)      │     because a flat ring vanishes under the 35° camera
│                              │
│                              │
│                              │
│ ┌────────────────┐ ┌───────┐ │
│ │                │ │  2x   │ │  ← speed: cycles 1x/2x/3x, remembers the last choice
│ │   ◎  Pulse     │ ├───────┤ │
│ │  (cooldown as  │ │  ⏸    │ │  ← pause
│ │   a ring)      │ └───────┘ │
│ └────────────────┘           │  ← Pulse: 60% width × 120 dp; cooldown as a ring that fills
└──────────────────────────────┘
```
- **Pulse ready:** the button ring is full and the edge emits a soft halo (0.3 → 0.6 → 0.3 in 2 s). Never a flash.
- **Pulse cooling down:** ring filling clockwise, seconds in the centre only under 5 s.
- Tap on a **module** during the wave: read-only tooltip (name, level, current DPS) for 2 s, anchored above the module.
- Tap on an **enemy**: nothing (unless PROPOSAL P3 "Focus", `07` §1.7).
- **No text** during the wave beyond the three numbers and the damage numbers.

### 2.3 In-game — Shop
```
┌──────────────────────────────┐
│ ♥ 100        Shop 7      ◈ 12│
│ ┌──────────────────────────┐ │
│ │ Next: ▲▲▲▲▲ ◆◆ ●         │ │  ← wave preview: icons × quantity; tap = names
│ │ DPS 240 → 360 (+50%)     │ │  ← preview line: appears ONLY during a drag
│ └──────────────────────────┘ │
│                              │
│         (ring close-up,      │
│         camera at 3.4 u)     │  ← valid slots breathe during a drag
│                              │
│ ┌──────────────────────────┐ │
│ │      ⌄ Sell  +2          │ │  ← Sell zone: appears ONLY while dragging a ring module
│ └──────────────────────────┘ │
│ ┌────┐ ┌────┐ ┌────┐ ┌────┐ │
│ │ ◇  │ │ ○  │ │ ◇  │ │ ●  │ │  ← 4 cards (3D icon, name, cost, effect in ≤ 6 words)
│ │Emit│ │Ampl│ │Scat│ │Bank│ │     border = rarity (C none · U silver · R gold)
│ │ 3  │ │ 3  │ │ 4  │ │ 4  │ │     "⇧ L2" top right if the module is owned (merge)
│ └────┘ └────┘ └────┘ └────┘ │     greyed card if unaffordable (never hidden)
│ ┌──────┐ ┌──────┐ ┌────────┐│
│ │↶ Undo│ │⟳ 1   │ │ ▶ Next ││  ← Undo (greyed if stack empty) · Reroll with cost · Next wave (primary)
│ └──────┘ └──────┘ └────────┘│
└──────────────────────────────┘
```
- **Card anatomy** (78 × 108 dp, v2): render of the 3D model (same light as the game), name, cost with the Credits icon, category glyph in the corner, **reach dots ●○○/●●○/●●●** (range bands, D32), rarity border, "⇧ L2" (★★ if `11` §1 is approved) badge when buying would merge. The effect sentence moved to the drag preview and the module sheet: at card width it wrapped to four lines.
- **Card states:** normal · lifted (while dragging) · unaffordable (60% alpha, cost in faint coral) · bought (fades out and the others **do not shift**: the gap stays, so card positions are stable).
- **Module sheet** (long press on a card or module, 250 ms): rises to half screen: large 3D model, stats at L1/L2/L3 with the current one highlighted, "affected by: Amplifier (left), Lens (right)". Closes by dragging down. From here: **Sell** (hold 1 s) as an alternative to the Sell zone. *Built so far (2.5-C4): the sheet is the read-only bubble with stats, sell value and, for a booster, the neighbours it lifts. Hold-to-sell is deliberately not wired: without a visible hold meter it sells by accident, and the module panel already has Sell.*
- **Extra slot** (current rule, `05` §5; `12` proposes making it rare via Grove/Charm): appears as a special **fifth card** "Slot +1 · 8", draggable onto the ring where you want to insert it (the ring opens with a 300 ms animation). *Today it is an icon button beside Reroll; the card form waits for Phase 3.*

### 2.4 Run end — victory and defeat (same structure)
```
┌──────────────────────────────┐
│                              │
│       Wave 12 of 18          │  ← title: how far you got (defeat) / "Victory" (win)
│   Stopped by: Brute (elite)  │  ← defeat only: what killed you, with the icon
│                              │
│        (your ring,           │  ← the final ring, 3D, rotates slowly ONCE (4 s) then stops
│         close-up)            │
│                              │
│   Total damage     1.24M     │  ← progressive count, 0.8 s, tap = skip
│   Time             11:42     │
│   Kills              318     │
│                              │
│   ◆ +14 Blueprints           │  ← added after the damage, one at a time (rising tone)
│   ▓▓▓▓▓▓▓▓░░ Lance in 6      │  ← bar towards the chosen next unlock (tap → Archive)
│                              │
│ ┌──────────────────────────┐ │
│ │      ▶  Play again       │ │  ← primary
│ └──────────────────────────┘ │
│  Same seed    Share       ⌂  │  ← secondary: replay this seed (unranked) · share · Home
└──────────────────────────────┘
```
- **Order and timing:** title (0 s) → ring (0.3 s) → damage (0.6–1.4 s) → time and kills (1.4–1.8 s) → Blueprints (2.0–2.6 s) → bar (2.6–3.0 s) → buttons (3.0 s, but **tappable from the start**: a tap skips everything).
- **Share:** generates a 9:16 image with the ring, the three numbers, the seed and the replay code. No intermediate screen: the system share sheet opens directly.
- **Daily variant:** adds the leaderboard position ("#47 of 1,284") and the comparison with the chosen ghost ("Ghost: wave 15 · you: 12").
- **Third defeat in act 1:** one contextual tip line (`07` §2.4) above the buttons, nothing else.

### 2.5 Archive — the unlocks
```
┌──────────────────────────────┐
│ ‹                    ◆ 128   │
│  Weapons  Boosters  Econ. Core│  ← 4 tabs; sliding underline (200 ms)
│  ───────                     │
│ ┌──────────┐ ┌──────────┐    │
│ │ ◇ Arc    │ │ ◇ Lance  │    │  ← AVAILABLE nodes: 3D icon, name, cost, one effect line
│ │ 20 ◆     │ │ 20 ◆     │    │
│ └──────────┘ └──────────┘    │
│ ┌──────────┐                 │
│ │ ◇ Mortar │   ░░░░░░░░░     │  ← LOCKED nodes: silhouette + "requires Arc"; no red padlocks
│ │ 25 ◆     │   ░░░░░░░░░     │
│ └──────────┘                 │
│ ✓ Emitter  ✓ Scatter          │  ← owned: a compact row at the bottom, not cards
│                              │
│ ┌──────────────────────────┐ │
│ │  Next: Lance · in 6      │ │  ← the "targeted" node: shown on the run-end screen
│ └──────────────────────────┘ │
└──────────────────────────────┘
```
- **Unlock in two taps, no dialog:** tap the node → the card expands (sheet) with the large model, the stats and **"Unlock · 20 ◆"** as primary. Tap → the Blueprints count down, the node "lights up" (soft glow 400 ms), neighbouring cards open. If there are not enough: the primary says **"Target · in 6"** and sets the node as the goal (run-end bar).
- **Try before you unlock** (**PROPOSAL**): from the sheet, "Try in a trial run" opens a 3-wave run with the module, unranked, without Blueprints. Avoids buying blind.
- No money is spent on this screen (model B: pack caps and rewarded ads live elsewhere, `08`).

### 2.6 Pass (model B only, after soft launch)
- A **horizontal track** of 30 tiers, two rows (Free above, Premium below), current position centred; inertial scrolling with a magnet on tiers.
- **Automatic claiming:** reached rewards are granted by themselves (one line on the run-end screen: "Pass · tier 8 · Ring theme"). **No 30 "claim" taps**: the number-one pass annoyance.
- Premium shows the Core theme **applied to your Core** as a preview; the purchase button is secondary, does not flash, has no timer.

### 2.7 Options (full-height sheet)
One list, grouped, with **immediate preview** (changes are visible behind the veil):
- **Visual:** reduce motion · effect intensity (slider) · damage numbers (all / big / none) · text size (100–200%, with a sample text) · colour-blind (3 presets, with a colour preview) · high contrast.
- **Audio and touch:** music · effects · haptics.
- **Game:** default speed · animation speed (1x / 2x / instant) · Calm Mode (`08` §4.3, with the note "unranked").
- **Account and privacy:** consent (reviewable), restore purchases, delete data (hold 1 s).
- The four most requested options (reduce motion, intensity, numbers, haptics) are also reachable **from pause** in one tap.

### 2.2b In-run mini windows (v2, Phase 2.5)
Overlays that inform without pausing; one at a time; each closes by itself.
| Window | Trigger | Content | Closes |
|---|---|---|---|
| Module tooltip | tap a module during a wave | render, name, level, live DPS, what boosts it, share of this wave's damage | 2 s or next tap |
| Enemy card | tap an enemy (48 dp hit area) | name, elite tag, HP bar and numbers, armour, "weak to" modules | 2 s or next tap |
| Edge marker | elite, Guardian or large group outside the view | glyph + count + chevron on the screen edge (rose / coral) | when on screen |
| Guardian banner | Guardian wave starts | eyebrow, name, one trait line → collapses to a rose HP bar under the top bar | end of the fight |
| Wave preview + spawn compass | top of the shop (ⓘ for detail) | threat chips, arcs showing the sides the wave comes from, "new enemy" strip | stays; fades while dragging |
| Combo inspector | long-press a booster on the ring | gold links to neighbours labelled with their gain, half sheet with the numbers | on release |
| Module sheet | long-press a card or module (250 ms) | large render, L1/L2/L3 side by side, range, affected by, hold to sell | drag down / tap outside |
The top bar gains a 3 dp **wave progress** hairline under the wave number; Integrity is also an arc around the Core (PROPOSAL, drawn this way in the mockups).

### 2.8 Pause (veil over the wave)
Dark veil at 60%, the arena stays visible but still. Three buttons: **Resume** (primary), Quick options, **Abandon** (secondary, **hold 1 s**: the ring fills, then confirms). Tapping outside the veil resumes.

---

### 2.9 New screens (v2)
- **Run setup** (sheet from Play once a second Core or Grade is unlocked): Core tiles, Grade, mode; remembers the last choice.
- **Act card** (after each Guardian): the sky cross-fades to the next act's palette; act name and the new enemies; 3.2 s, tap to skip.
- **Codex**: every enemy and module met, render + one line + counter-play; unmet entries as silhouettes; unlocked by seeing, never bought.
- **History and records**: best wave, best damage, wins; last 50 runs with a generated build name; each run can be watched or replayed (unranked).
- **Replay and ghost**: timeline with wave and Guardian ticks, scrub, ghost overlay, "Challenge this seed".
- **Share card**: the 9:16 image with the ring, the big number and the replay code.
- **Leagues and Duel** (Update 1) and **Pass** (model B) are mocked up so the layout rules hold when they arrive.

## 3. First impression and the first minutes
The FTUE is specified second by second in `07` §1.5 and the first 10 seconds in `07` §3. Here the **interface** rules that make it possible:

### 3.1 What is absent at first start
No Home, no long logo, no login, no notification or rating request, no language choice (the system language is used; changeable in options), no "welcome". Only the UMP consent where mandatory (D16), then the Core.

### 3.2 Progressive disclosure
| Moment | What lights up | How |
|---|---|---|
| First start | only ▶ and (in the shop) the cards | everything else is invisible, not greyed |
| First shop | Undo and Reroll appear **after** the first drag | fade, no explanation |
| First wave 2 | speed and pause | idem |
| End of the first run | the Home, with **Play** and ⚙; the Blueprints earned and "Archive" appearing with the bar | items appear one at a time (150 ms stagger) |
| End of the second run | the **Daily** card on the Home ("Today: 1,284 players") | — |
| First win | **Grade** and **Core type** selectors (the unlocked ones) | — |
| Third run | Leaderboards | — |
Every new element appears with a **fade and a light breath** (scale 0.96 → 1), once. No "new!" tooltip, no red dot.

### 3.3 Hints
Only **one line** at a time, white on nothing, above the bottom third, fading on the first correct gesture. At most five in the player's whole life (▶, drag, Pulse, duplicate, Daily). No panel, no guide character, no "OK".

### 3.4 Empty states and waiting
- Leaderboards before the network: "Leaderboards arrive when there is a connection" + your local history. No infinite spinner: requests time out at 3 s and then show the local state.
- Archive with 0 Blueprints: available nodes are still visible with "in N": the player sees what they are earning.

---

## 4. Interface feedback and micro-interactions
(game animations are in `03` A7; here only the interface)

### 4.1 Components and states
| Component | Rest | Touch (≤ 100 ms) | Release | Disabled |
|---|---|---|---|---|
| Primary button | solid ivory, 64 dp, radius 24 | scale 0.96, 80 ms | return with 2% overshoot, 150 ms; light haptic | 40% alpha, **never hidden** |
| Secondary button | 1 px outline, ivory 40% | 10% fill | idem | idem |
| Card | glass, rarity border | lifts 12 px, scale 1.05, wider shadow | (drag: `03` B3) | 60% alpha, cost in faint coral |
| Tab | text 60% | — | underline slides 200 ms, ease-out | — |
| Slider | line + knob | knob scale 1.2 | haptic tick at notable values (0, 50, 100) | — |
| Toggle | pill | — | slides 150 ms; colour ivory ↔ 30% | — |
| Sheet | — | — | rises from the bottom 250 ms ease-out, veil at 40%; closes by dragging (follows the finger) or tapping outside | — |
| Toast (line) | — | — | appears at the top, 2 s, one at a time; the next replaces the previous | — |
| Counter | — | — | digits roll upwards, 600–1,200 ms, ease-out, rising tone; tap = final value | — |
| Progress bar | — | — | fills with ease-out 400 ms; a soft glow on completion, never a flash | — |

### 4.2 "Breathing" as the signal
Only one element per screen may breathe (scale 1.00 → 1.02, 2 s): **the action the game expects**. ▶ on the Home, Next wave in the shop when no useful Credits remain, the Pulse when ready with 3+ enemies close. It is our replacement for red dots and flashing arrows.

### 4.3 Haptics (Android `VibrationEffect`, `03` B5)
| Level | When |
|---|---|
| Light (15 ms) | button tap, card pick-up, magnet, slider tick |
| Medium (25 ms) | place, Pulse, Next wave, Archive unlock |
| Success (two taps) | merge, victory, Archive completed |
| None | errors (the soft return is enough), scrolling, counters |

### 4.4 Interface sound (`03` C3)
Each component has **one** sound, soft and short: button (warm tock), card picked (note), magnet (warm click), release (lower note), merge (rising crystal), counter (rising tone per step), sheet (breath). Never two identical sounds in a row: ±3% random pitch variation.

### 4.5 Transitions
| From → to | Transition | Duration |
|---|---|---|
| Shop → Wave | camera pulls back; cards drop staggered by 40 ms; Pulse rises | 600 ms |
| Wave → Shop | ring pulse (wave end) → summary → camera moves in → cards rise | 800 + count + 600 ms |
| Home ↔ Archive/Leaderboards | fade + scale 0.98 → 1 | 200 ms |
| Any → Run end | the arena stays; a veil rises to 40% and the content enters from the bottom | 300 ms |
| Sheet opening | from the bottom, ease-out, follows the finger | 250 ms |
Never lateral slides between screens (they imply a hierarchy that does not exist), never "explosive" zooms.

---

## 5. Design tokens (for UI Toolkit, D22)
> **v2:** the source of truth is now `tokens.json` in the design system (https://claude.ai/artifact/5aKRr9H8e3WJsMui64MNnu): 4 themes (Act I Dusk, Act II Twilight, Act III Night, High contrast), sky tokens, `surface-glass`/`surface-raised`, `ally`, Outfit + Nunito type styles, shadows and sizes. `Theme.uss` is to be regenerated from it (Phase 2.5-A). The block below is the v1 summary kept for history.

**How it is wired (2026-09-22, task 2.5-A7).**
- `Tools/design/tokens.json` is the local copy of the design system's tokens; `python Tools/tokens_to_uss.py` rewrites the `tokens:begin`/`tokens:end` block of `Assets/UI/Resources/Theme.uss` (1 dp = 3 px at the 1080-wide reference). Hand-written rules below the block are kept. **No rule writes a literal colour**: the four translucent states the design system describes but does not tokenise — pressed button, Pulse fill, uncommon border, sheet scrim — are derived from their base colour by the script (`--press-tint`, `--fill-tint`, `--border-uncommon`, `--scrim`).
- **Typefaces.** Outfit writes the numbers the player tracks, Nunito the words. *TowerDefense → Content → Build font assets* bakes three weights out of the two variable fonts in `Assets/UI/Fonts` (SIL OFL, from Google Fonts): `Outfit-SemiBold` (600), `Nunito-SemiBold` (600), `Nunito-ExtraBold` (800, also used for the 700 caption, whose 12 dp size hides the difference). The assets are dynamic with the Latin-1 atlas pre-baked, so the text costs nothing at runtime and an unexpected character is still drawn instead of showing a box; Nunito falls back to Outfit for the arrow of the drag preview, which it does not draw. Static font assets are *not* an option: Unity 6 no longer renders them.
- **Icons.** `python Tools/icons_to_png.py` rasterises the design system's 47 line icons (`docs/art/icons/*.svg`) into `Assets/UI/Icons` at 96 px, white on transparency, through headless Edge; each use tints its own copy (`-unity-background-image-tint-color`). In the HUD so far: Integrity, Credits, Options. The rest arrive with slices 2.5-B and 2.5-C.
```
Grid            8 dp · gutter 16 dp · thumb zone = bottom third
Radii           cards 16 · buttons 24 · sheets 32 (top corners only)
Glass           background rgba(243,233,215,0.06) · border 1 dp rgba(243,233,215,0.12)
Colours         --ivory #F3E9D7 · --ivory-60 · --ivory-40 · --teal #5CC8C0 · --gold #E9B872
                --mint #9AD9A1 · --coral #F07A6A (enemies and unaffordable costs only) · --rose #C8507A
Type (Nunito)   display 32 · title 24 · numbers 20 · body 16 · caption 12 (× scale 1.0–2.0)
Durations       touch 80 · state 150 · component 250 · screen 200 · camera 600
Curves          ease-out (cubic 0.2,0.8,0.2,1) · ease-out-back (0.34,1.56,0.64,1) for settles
Touch           minimum 48 × 48 dp · drag threshold 8 dp · long press 250 ms · hold 1,000 ms
```
Everything lives in one USS file (`Assets/UI/Theme.uss`) with variables: seasonal themes change only colour variables, never measures.

---

## 6. Accessibility and localization (from day one)
- All text scales to 200% without breaking layouts: cards use the icon as the main element and text may wrap to two lines.
- Every primary action has icon **and** text; every colour has a shape (`03` A4).
- Text contrast ≥ 4.5:1; targets ≥ 48 dp; no audio-only (D1) or colour-only information.
- **Character budget** for strings (for DE and FR, the longest): button labels ≤ 12, effect lines ≤ 30, hints ≤ 40. Strings have keys, never text in code.
- "Reduce motion" also turns off breathing (§4.2), replaced by a brighter border on the expected action.

---

## 7. What enters which phase
- **Phase 2.5** (visual overhaul): restyle of everything built in Phase 1 from the design-system tokens; §2.2b mini windows; wave summary damage share; pause run info. Full list in the design system's *Screen map*.
- **Phase 1** (simple shapes, UI Toolkit): §2.2, §2.3, §2.4 (without Blueprints), §2.8, §4 (components, breathing, haptics), §5 (tokens), minimal progressive disclosure (Undo/Reroll after the first drag).
- **Phase 3** (final art): §2.1 Home, 3D icons on cards, interface sounds, §2.7 complete options, §6 complete.
- **Phase 4:** §2.5 Archive, "next unlock" bar, Daily on the Home, sharing, §3.2 complete.
- **Phase 6+:** §2.6 Pass.

**Decisions to confirm:** integrity arc around the Core · "try before you unlock" in the Archive · hold 1 s as the only confirmation for irreversible actions · automatic Pass claiming.
