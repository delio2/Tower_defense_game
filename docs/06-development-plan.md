# 06 — Development plan by phases

> Version 3 · 2026-09-22 · Status: active. v2 inserted **Phase 2.5 — Visual overhaul** (D30); v3 details Phase 2.5 and Phase 3 as task lists with estimates and "done" criteria, and adopts **vertical slices on shared foundations** (D33). The month-by-month calendar is in `10` §4; this document defines the **phases, deliverables and exit gates**.
> **Golden rule (`01` §11):** if the shop with the ring is not fun with simple shapes, we do not move on to art.
> Rules and numbers: `05`. Direction: `03`. This file says **in what order** to build things and **why**.

---

## 0. How we work
1. **One phase at a time.** Every phase has deliverables, tests and an exit criterion ("gate"). The next phase does not open until the gate passes or is consciously deferred (written in `04`). Work inside a phase runs in parallel where it makes sense.
2. **Simulation first, visuals second.** Every mechanic is born in the `Simulation` assembly with a test; presentation only reads it.
3. **The bot plays before the testers.** Every balance change goes through the automatic simulator (thousands of runs in seconds) before reaching a person.
4. **Real testers at every gate** (3–5 people in early phases, 12+ in the closed test), **muted first** (D1).
5. **Calm by default** (`03` A1) and **honest by construction** (`02` §1): these are constraints from the first line, not final polish.
6. **Everything is a replay** (D19): every function that touches game state is a recordable command, or it does not exist.
7. **Vertical slices on shared foundations** (D33). First the foundations every screen shares (camera, light, shader, UI theme, model pipeline, screen architecture); then one **slice** per moment of the loop, each shipping mechanics + tests + bot probe + visuals + UI + feedback together. A slice is **done** when: (a) its simulation tests pass and the bot probe is recorded in `05` §18; (b) its screens match the design-system mockup side by side (screenshot in `Temp/Screenshots`); (c) it respects calm/readability rules and the options (reduce motion, intensity, text size); (d) it runs on the Pixel at 60 fps and on the low quality level; (e) `05`/`04` are updated. A slice whose mechanics are still a PROPOSAL does not start.

---

## 1. What the community wants and hates: the list that drives the plan
Summary of `00`, `03` Part B and the reference titles. Each row says **where** we address it.

### 1.1 What players hate in the big titles (and our rule)
| Game | What they hate | Our rule | Phase |
|---|---|---|---|
| **The Tower** | lab timers skipped by paying; pay-to-win; too many currencies; mandatory connection; hours-long runs; crowded menus with small text | zero timers; fixed-kit competition; **2 currencies**; complete offline; 10–15 min runs; little information at a time, large text | 2, 4, 6 |
| **Bloons TD 6** | 45+ minute sessions; Monkey Knowledge became a currency grind | short runs with **save at every shop**; the Archive unlocks variety, **never stats** | 2, 4 |
| **Rush Royale / Random Dice** | pay-to-win tournaments; opponents 2× stronger; unfair randomness | competitive modes with the **same seed and pool** for everyone; reroll, wave preview, undo | 1, 4, 7 |
| **Super Auto Pets** | matchmaking by win count → "it is all luck" | ghosts compared **on the same seed**; leagues of 30 with the same seed and kit | 4, 7 |
| **Backpack Battles** | ghosts from old versions; on mobile the finger covers the text, imprecise dragging, ruined builds without undo | replays valid **only for the same balance version**; object above the finger, magnet, **free undo** | 1, 4 |
| **Balatro (mobile)** | landscape only; small text; unreliable cloud save; motion sickness; slow, unskippable scoring | **portrait** (D2); text size option; **the local save is the truth**, cloud is optional sync with conflict handling; still background + "reduce motion"; tap-to-skip counting | 1, 3, 4 |
| **Vampire Survivors** | late snowball turns the run into passive spectacle; visual clutter without options | Grades and Endless with real growth; the Pulse as a choice; adjustable **effect intensity** + automatic attenuation | 1, 2, 3 |
| **Brotato** | screen crammed with enemies and projectiles | cap on simultaneous effects; numbers only for kills and big hits | 1, 3 |
| **Ball x Pit** | a second grind (building) on top of the first | **no second grind system**: only the Archive (variety) and Grades (challenge) | 4 |
| **Kingdom Rush 5** | pay for the game, then pay for heroes and towers | model A: **complete** one-time unlock; model B: never power for money | 6 |
| **Infinitode 2** | arbitrary late-game grind | full unlock in 60–80 runs, then only Grades and leaderboards | 4 |
| **Arknights** | energy | no energy, ever | — |
| **PvZ 3** | simplified, imposed formations, lost depth | portrait **without** losing depth: shop, ring, merge, Core types | 2 |

### 1.2 What they love and cannot find (our answer)
| Recurring request | Answer | Phase |
|---|---|---|
| "cozy mobile with deep strategy" | calm visuals + a shop with real choices | 1, 3 |
| synergies and status effects | ring neighbourhood, multiplicative boosters, Frost, Echo | 2 |
| complete QoL: speed, pause, wave preview, undo | all from the prototype | 1 |
| asynchronous social, "beat my build" | shareable replay codes, ghosts, Daily | 4, 7 |
| cloud save and cross-platform | UGS Cloud Save (D21), local always valid | 5 |
| curated tactile feedback (Marvel Snap, Balatro) | tiered haptics, magnet, progressive counting | 1 |
| pleasant interaction sounds (Mini Motorways) | soft notes on pick/drop, rising tone in the counter | 3 |
| a modern "reference" TD without microtransactions (Thronefall, Mindustry) | model A as the primary option, decided with data | 6 |

---

## 2. The phases

### Phase 0 — Foundations ✅ (done, September 2026)
Deterministic simulation (60 ticks/s, integers), 7 modules, 3 enemies + Guardian, shop with merge and undo, Pulse, replay `R1` with verification, minimal presentation in the editor, balance bots (20 tests in total).

---

### Phase 1 — Tactile prototype and tools (2–3 weeks)
**Purpose:** answer the golden rule. The shop with the ring must be pleasant **to touch** already with simple shapes.

**Simulation**
- [x] `BuySlot(insertAt)` command (extra slot for 8 Credits after the first Guardian, `05` §5): ring 6 → 8, insertion opens an empty slot, undo, `TryPreviewBuySlot`, bots buy it when the ring is full; nearest-direction rounding for 7 slots.
- [x] **Bot runner** (`BalanceBot`, `BalanceRunner` in `Simulation`; menu `TowerDefense/Balance/Run bots`): N seeds × strategies (Naive, MaxDps, EconomyFirst); wins, defeat wave and cause, seconds per wave, unused Credits, rerolls, merges, Pulse uses. CSV in `Temp/Balance/`, summary in the console. First results in `05` §18.
- [x] **Content in ScriptableObjects** (`Assets/Content/Resources/`): `ModuleAsset`, `EnemyAsset`, `ContentCatalog` in `Presentation` (integers only), `ContentLoader` fills `ContentDatabase`; the prototype loads the catalog from Resources. Menu *TowerDefense → Content → Generate…* creates the assets from `CreatePrototypeDefaults()`, which stays the reference. Tests: lossless round trip and asset ≡ defaults (same hash). `RunConfigAsset` deferred to Phase 2 (Core types).
- [x] **Compact number notation** (`NumberFormat`, integer, invariant: 9999 · 12.3K · 123K · 1.2M · 5.6B · 1.5T) with a test; the prototype uses it.

**Presentation (`05` §15.1, `03` Part B, `07` §1.2, `09`)**
- [x] Dragging with the **object above the finger** (ghost +150 px), valid slots lit, **magnet** 0.45 units, 8 dp threshold (tap keeps the card-then-slot path); invalid release → soft return (250 ms). Cards → slots (buy/merge), modules → slots (move) or Sell zone.
- [x] **Preview above the cards** while dragging, computed by `TryPreviewBuy/Move/Sell` ("DPS 16 → 36 (+125%)", "Level 2 · DPS …", "Sell: +2 · DPS …").
- [x] **Sell zone** (appears only while dragging a module), **Undo** in the UI, tap-card-then-slot as the alternative.
- [x] Animated merge (swell 1.15 → 1 in 150 ms + a faint golden ring; attraction from the card deferred), combo lines already shown in the shop.
- [x] **Wave-end summary**: damage rolls (0.8 s), Credits add up (0.8–1.6 s, interest shown), closes at 2.2 s, tap anywhere to skip; the shop appears afterwards.
- [x] Haptics at 3 levels (`Haptics`: Android `VibrationEffect` one-shot/waveform, silent elsewhere): light on pick-up and magnet, medium on drop/Pulse/Next wave, success on merge; never on errors; option to disable.
- [x] Basic options (`PlayerOptions` in `PlayerPrefs`, sheet from the ⚙ button): reduce motion (breathing and enemy rotation off, effects halved), effect intensity (alpha and caps), damage numbers all/big/none, haptics, default speed.
- [x] Cap on simultaneous tracers (32 × intensity) and automatic attenuation above 20 enemies.
- [x] Replace IMGUI with **UI Toolkit** (D22): `HudView` + `Hud.uxml` + `Theme.uss` (tokens of `09` §5) loaded from `Assets/UI/Resources`, `PanelSettings` 1080×1920; top bar, offer cards with rarity/merge badge, module panel, Undo/Reroll/Next/Slot +1, Pulse with cooldown fill, speed/pause, run-end overlay, toasts and floating numbers. Options screen still to do.

**Gate 1 (`05` §19):** 3–5 testers, muted, on the phone (Android development build). At least 3 out of 5 ask to play again; they understand the neighbourhood without explanations by the second shop; nobody is stuck more than 10 s; real wave and shop durations measured (targets 25–30 s and 10–20 s). The naive bot stays below 95% wins in act 1 (above that, act 1 is too easy).

---

### Phase 2 — MVP content and full run (3–4 weeks)
**Purpose:** the whole 3-act run, with every MVP element, balanced by the bot.

- [x] **Missing modules** (`05` §6): Arc, Lance, Mortar, Echo, Salvage, Frost, Capacitor → 14, each with a behaviour test (`MechanicsTests`).
- [x] **Missing enemies** (`05` §8): Dasher, Splitter, Warden; **elites** from act 2 (HP ×3, armor +1, one or two per wave); unlocks by global wave.
- [x] **Acts 2 and 3:** 18 waves by default; Guardian escorts follow the act roster; themed waves. (Preview with icons: Phase 3 UI.)
- [x] **Core types** (Standard, Merchant, Bastion, Glass) in `RunSetup`; chosen for the next run from the options sheet until the Home exists.
- [x] **Grades 1–3** (+10% HP, −1 Credit per wave, elites from act 1) and **Endless** (`RunMode.Endless`: victory is recorded, waves go on until defeat).
- [x] **Save and resume at every shop** (`RunSave`: the replay text in PlayerPrefs, written on every `ShopOpened`; resume = re-simulation; discarded on another balance version). Verified in the editor: stop and re-enter Play resumes at the same shop with the same Credits.
- [x] **Replay `R2`:** + game version, mode, Core type, Grade; `R1` still readable; verification rebuilds the config from the choices.
- [x] **Balance v0.3** with the bot (first pass, `05` §18): HP growth ×1.12 → Naive 8% run / 95% act 1, MaxDps 56%, EconomyFirst 27%; about 7 minutes of combat. Grade targets pending the Grades.
- [x] `05` §4, §9, §18 carry the v0.3 numbers and probes; `04` D22, D26–D29 record the decisions.
- [ ] **Start now** the **Google Play account** procedure (D13): it takes long and is needed for Phase 5.

**Gate 2:** whole run playable on the phone; bot targets met; real duration with 3–5 testers between 10 and 15 minutes; at least **3 different winning builds** found by testers or the bot (otherwise the 6-slot ring is not enough: `05` §18).

---

### Phase 2.5 — Visual overhaul: art direction v2 and every screen designed (2–3 weeks) 🔄
**Purpose:** the prototype plays well but looks like a prototype (flat unlit shapes on one navy, text-heavy cards, a system font, emoji icons, no information layer during waves). Before producing art in Phase 3, redesign the **whole** look and interaction — every window, including future ones — against the best-looking games and their criticised weaknesses, and prove it with a first real mood shot. User decision, 2026-09-22 (D30).

**Design (done 2026-09-22, to be approved)**
- [x] Research: what Monument Valley, Alto's Odyssey, Thronefall, Balatro, Mini Motorways, Marvel Snap, Kingdom Rush, Hades, Sky, Dorfromantik and Infinitode 2 do well, and what their players criticise (`03` Part D).
- [x] **Design system "Dusk Garden"** ([artifact](https://claude.ai/artifact/5aKRr9H8e3WJsMui64MNnu)): tokens for four themes (one sky per act + high contrast, enemy contrast checked per theme), Outfit + Nunito type scale, 47-icon line set, 16 components with live previews, **22 screens** as interactive mockups (Shop with real magnetic drag, Wave with tooltips and Pulse, Summary, Run end, Home, Archive, Daily, Codex, Replay, Leagues, Pass…), screen map with phases, research.
- [x] **Mood shot v1 in Blender via MCP** (`Art/Blender/moodshot.blend`, renders in `docs/art/`): Core with petals, five modules, six enemy types + Guardian, tilted 35° orthographic camera, warm key + cool ambient, soft shadows, light bloom; act 2/3 sky variants; transparent module and enemy renders used as card art.
- [x] **Content and systems v2** (`11`, D31): simpler names, shape grammar, 36 modules, 14 enemies + 3 Guardians with silhouettes in the design system, spawn gates and sectors, act rhythm, rewards and economy.
- [x] **Run structure and synergies** (`12`, D32): rings as range bands (decided), act path, Charms, affinities, Nests/Sprites, special petals, editions, Pouch — with interactive cards in the design system.
- [ ] **Approve** `11` and `12` (names and systems first) with the user.
- [ ] **Approve** the mood shot and the screens with 3–5 people (direct question: "calm but attractive, or prototype?") and record the verdict in `04` D30.

**Build in Unity — tasks** (estimates in working days for one person with AI, `(estimate)`)

*2.5-A · Foundations (shared by every screen, ~5 d)*
| # | Task | Done when |
|---|---|---|
| A1 | **Screen architecture:** split `PrototypeRunner` (1,420 lines) into `ArenaView` (sky, rings, Core, petals), `ModuleView`/`EnemyView` pools, `EffectsView` (lines, flakes, Pulse, numbers), `CameraRig`, and one controller per screen state (`ShopScreen`, `WaveScreen`, `SummaryScreen`, `RunEndScreen`, `PauseScreen`) driven by a small state machine | behaviour identical to today; all 45 tests green; no file over ~400 lines — 🔄 **2026-09-22:** runner split into `Arena/` (`ArenaKit`, `CameraRig`, `CoreView`, `RingView`, `EnemyViews`, `EffectsView`), `Interaction/ShopInput`, `UI/UiText`; runner 1,420 → 371 lines; 45/45 tests, Play mode checked (shop, wave). Still to do: per-screen controllers (arrive with slices B–D) and splitting `HudView` (599 lines) during the UI restyle |
| A2 | **Camera rig:** orthographic, 35° tilt; wave framing fills the width to the edge ring; shop framing puts the whole ring in the upper half; 600 ms ease-in-out between them | screenshots match the mood shot framing — ✅ 2026-09-22: 35° tilt with foreshortening-aware framing (`CameraRig`); portrait captures via a 1080 × 2220 render texture |
| A3 | **Light and post:** warm key `#FFE2BC` from the upper left, gradient ambient, soft shadows (medium/high), post volume (bloom 1.1 / 0.3, vignette 0.25, tone mapping None); quality levels Low (no shadows, no bloom) / Medium / High | side-by-side with `docs/art/moodshot-wave.png`; Low level ≥ 60 fps on the Pixel — 🔄 key light, hemisphere ambient, bloom/vignette/tone mapping None done (`ArenaLighting`); shadows on both URP assets (1 cascade, 45 u); the Low/Medium/High quality split and the fps check are still to do |
| A4 | **Three-surface shader** (Shader Graph, URP Lit): body + fresnel rim (category colour) + emissive; materials Core, petal, teal, gold, mint, coral, rose, ally as assets in `Resources/Materials`; SRP Batcher compatible | ≤ 40 draw calls with 60 enemies; works in a player build (no stripping) — 🔄 hand-written URP shader `TowerDefense/ThreeSurface` (lit body, fresnel rim, emissive; ShadowCaster + DepthOnly), one material per style, assets in `Resources/Materials`; draw calls and device build to check |
| A5 | **Sky and rings:** per-act gradient background (dusk/twilight/night) + range-band rings at 3.0 / 5.5 / 7.5 / 9.0 as thin line meshes; act theme switch API | colours equal `tokens.json`; rings at 15–20% — ✅ `SkyGround` shader (gradient, glow, shadows) + range bands 3 / 5.5 / 7.5 / 9 (`ArenaBackdrop`); act theme switch applied at each wave start (`Palette.Acts`) |
| A6 | **Model pipeline:** Blender family script v1 generating the **14 existing modules** and **7 enemies + Guardian** (from the `11` silhouettes), exported as glTF to `Assets/Models/`; prefab per model; `ModuleAsset`/`EnemyAsset` reference a prefab; `PrimitiveMesh` fallback | every current module/enemy shows its own silhouette in game — ✅ 2026-09-22: `Tools/blender/build_models_v1.py` builds the 21 models (`Art/Blender/models_v1.blend`) and exports FBX to `Resources/Models/{Modules,Enemies}`; the game loads them by kind (`ArenaKit.CreateModel`, sub-mesh 1 = glowing accent) with the primitive fallback; modules face outward, shards point at the Core with a slow roll. Star faces (★★/★★★) come with the v2 family scripts (3b) |
| A7 | **UI foundations:** `Tools/tokens_to_uss.py` generates `Theme.uss` variables from `tokens.json`; Outfit + Nunito font assets (Latin-1 atlas); the icons rasterised to 96 px sprites as UI Toolkit backgrounds; card renders for the 14 modules | the HUD uses no hard-coded colour or system font; no missing-glyph box on device — ✅ 2026-09-22: tokens → USS, plus four derived tokens so no rule writes a literal colour (`--press-tint`, `--fill-tint`, `--border-uncommon`, `--scrim`); coral removed from player UI; card art from `models_v1.blend`; **fonts** — Outfit SemiBold + Nunito SemiBold/ExtraBold baked from the variable files by *Content → Build font assets* (dynamic with a pre-baked Latin-1 atlas: Unity 6 no longer draws static ones; Nunito falls back to Outfit for the arrow); **icons** — `Tools/icons_to_png.py` renders the 47 SVGs to `Assets/UI/Icons` through headless Edge, tinted per use, wired for Integrity, Credits and Options. Card name and card height were nudged so the heavier typeface still fits; the card itself is rebuilt in C1. Device check for missing glyphs rides with 2.5-E |

*2.5-B · Wave slice (~4 d)* ✅ **2026-09-22** — mockups *Wave*, *Guardian fight*, *Arena feedback*, *Threat alerts*, *Module tooltip*, *Enemy card*
| # | Task |
|---|---|
| B1 | ✅ Models on petals with idle float (± 3.5 cm, own phase each); enemies hover and grow into place over 0.3 s — a scale-in, not an alpha fade: the lit shader is opaque |
| B2 | ✅ Hit lines throttled per module (one every 0.5 s, the "≤ 2 per second in one spot" rule), flake deaths (4 per death, 12 at once), Pulse ring + Core emissive flare, Core-hit coral tint, knockback smoothed over a few frames |
| B3 | ✅ Top bar with drawn icons + wave progress hairline (fed by `WaveEnemyCount` / `WaveEnemiesLeft`, new read-only counters with a test); **Integrity arc** around the Core — drawn in panel space, since a horizontal ring in the arena flattens against the Core under the 35° camera; damage numbers on kills and on hits worth a quarter of the target's health |
| B4 | ✅ Pulse with a painted cooldown ring and a halo that breathes when ready; speed and pause are round, pause carries the drawn icon |
| B5 | ✅ Module tooltip and enemy card on tap (48 dp pick radius), on a new read-only `DamageThisWave` per module (test: adds up, resets each wave) |
| B6 | ✅ Edge markers for off-screen elites and Guardians; the Guardian announces its name, then the strip stays as its health bar |
| B7 | ✅ Act palette switch at the first wave of an act (already in `ApplyActTheme`; the act card of Phase 3 will own it) |

*2.5-C · Shop slice (~4 d)* ✅ **2026-09-22** — mockups *Shop*, *Offer card*, *Drag feedback*, *Combo inspector*, *Module sheet*, *Wave preview*
| # | Task |
|---|---|
| C1 | ✅ Offer cards v2: render at a fixed height, name on one line, category glyph, reach dots, rarity border, merge badge, unaffordable and bought states. The effect sentence is gone from the card, as `09` §2.3 asks — it wrapped to four lines and now lives in the drag preview and the sheet |
| C2 | ✅ Drag v2: the ghost is the module's own render above the finger, valid petals breathe (4% over 1.4 s), magnet, a **reach circle** drawn at the dragged weapon's range from the hovered slot (a truer answer than a band wedge), preview bubble under the top bar, sell zone |
| C3 | ✅ Merge swell + gold ring, settle-in growth when a module is placed, combo links in gold |
| C4 | ✅ Long press (250 ms) opens the sheet: a module shows what it does, what it sells back for and — for a booster — which neighbours it is lifting (the combo inspector); an offer card shows its effect and cost. Hold-to-sell stays out: without a visible hold meter it is too easy to trigger by accident, and the module panel already has Sell |
| C5 | ✅ Wave preview: a chip per enemy kind with its count and a gold border on a kind never met, plus an eight-sector **spawn compass** weighted by the next wave's directions |
| C6 | ✅ Cards drop in 40 ms apart when the shop opens, over the camera move |

*2.5-D · Wave end and overlays (~3 d)* ✅ **2026-09-22** — mockups *Wave summary*, *Run end*, *Pause*, *Options*
| # | Task |
|---|---|
| D1 | ✅ Wave summary v2: the damage share of the top four modules as bars that fill after the numbers land, rolling Credits with the interest called out, tap to skip |
| D2 | ✅ Run end v2: what stopped the run with its drawn icon, totals rolling up over 1.2 s, seed and replay status, and the "Same seed" / "Share" buttons in place, saying which phase brings them |
| D3 | ✅ Pause v2 as a sheet: wave, Core, grade and seed, the four options worth changing mid-run, and **hold to abandon** with a filling label (a tap can never lose a run). Options gained **Large text** and **High contrast**, both generated from `tokens.json` into `.text-large` and `.theme-contrast` and applied as classes on the root — the arena keeps its act palette for now |

*2.5-E · Device and gate (~2 d)*
| # | Task |
|---|---|
| E1 | ✅ **2026-09-22**, on a Pixel 10 (Android 17). The build is fine and **60 fps flat** in shop and wave (avg 16.6 ms, worst 5% 16.7 ms, max 17.1 ms, measured from SurfaceFlinger frame times — `dumpsys gfxinfo` does not see a Unity SurfaceView). The device pass paid for itself three times: **bloom and vignette were being stripped** (the post-process volume is built at runtime, so the URP stripper saw no volume in any scene — `StripUnusedPostProcessingVariantsAndResources` off), a card name clipped on a taller screen (the name now steps its size down until it fits), and "Merge L2" plus the four action buttons overran their row (the merge badge is now the drawn icon + level, and Undo/Reroll/Slot are icon buttons, as `09` §2.3 draws them). **Left:** the Low/Medium/High quality split of A3 (there are two levels today, Mobile and PC) and a 4 GB phone |
| E2 | ✅ 2026-09-22: `docs/art/compare-wave.png` and `compare-shop.png`. What it showed: the framing gap that became D35, and tracers too faint at wave distance (now ivory at 0.6) |
| E3 | **Testers (3–5, muted):** Gate 1 + Gate 2 questions + "calm but attractive, or prototype?" on the new build |

**Gate 2.5:** the Unity build matches the mood shot side by side (camera, light, palette); the new information layer passes the Gate 1 questions (neighbourhood understood by the second shop, nobody stuck > 10 s); ≥ 3 of 5 testers say "attractive", not "prototype"; 30 fps on the 4 GB phone at Low, 60 fps on the Pixel at High.

---

### Phase 3 — Content, run structure and vertical slice (8–10 weeks)
**Purpose:** turn the approved proposals (`11`, `12`) into the launch game, slice by slice, then finish art, FTUE, audio and languages. Phase 2.5 delivered direction, screens and mood shot v1.
**Entry condition:** the user approves `11` (names and systems) and `12` (which layers are in the launch scope); decisions logged in `04`.

*3a · Content and systems v2 (~5–6 weeks)* — each step = simulation + tests + bot probe + its UI slice
| # | Step | Simulation | UI slice (mockup) | Bot probe |
|---|---|---|---|---|
| 1 | Names | rename player-facing strings (keys), keep code identifiers until a rename commit | all screens | — |
| 2 | Range bands | ranges re-tuned to land on rings II/III/edge | reach dots, band wedge (done in 2.5-C) | balance unchanged ±5% |
| 3 | **Gates + bursts + act rhythm + mutators** | `WaveDirector`: gates, main gate, drift, bursts, beats, mutators | *Spawn gates* rifts, compass, "Next · Siege" | win rates by bot vs 0.3 |
| 4 | **Economy v2** | Flawless, elite reward, Lock command, owned ×1.5, run pool / default Pouch | *Coins, run pool*, lock icon on cards | coin margin 10–50 |
| 5 | **Path + Grove + Mystery** | act path generation, `ChooseNode`, Grove actions, 6–8 Mysteries | *Path map*, *Grove*, Mystery card | safe vs greedy path bots |
| 6 | **Charms** (replace fixed extra slot) | 4 slots, hooks, `PickCharm`/`SellCharm`, ~15 Charms first | *Charms* row, picker, long-press | no Charm > +15% win rate alone |
| 7 | **Affinities** | tags, breakpoints 2/4 | affinity line above cards | every affinity wins with some build |
| 8 | **Modules batch A–D** (22 + Nests) | 6 per batch, behaviour test each | roster renders (3b) | batch probe |
| 9 | **Enemies batch A–B** (8) + **Twins, Eclipse** | behaviours, act pools | Codex entries, "new enemy" | Guardian exam per act |
| 10 | **Allies** (Nest, Keep, Queen) | Sprites: intercept, block, respawn | *Nests and Sprites* | vs rush waves |
| 11 | Special petals, editions, Pouch editor | petal types, editions in offers, Pouch edit/Prune/Plant | *Special petals*, *Pouch* | — |
| 12 | **Balance `0.4.0`** | replay format `R3` | — | ≥ 5 archetypes win at Grade 0 |
**Gate 3a:** all approved systems in; ≥ 5 archetypes win for the bot at Grade 0 and none above 85%; a full run still lasts 10–15 minutes.

*3b · Art completion (~2 weeks)*
- [ ] Blender family scripts v2 (Attack, Boost, Support, Enemy, Guardian, Sprite) generating **every** silhouette of `11`/`12` with ★/★★/★★★ faces; glTF export; card and Codex renders generated by script.
- [ ] Final effects per module family (lines, blasts, chains, burns) within the calm rules; per-act sky variants.
- [ ] Meta and between-act screens: **act card**, **Home**, **Run setup**, **Codex**, **History** (mockups exist).

*3c · FTUE, audio, languages, performance (~1–2 weeks)*
- [ ] **FTUE** (`05` §14, `07` §1.5): guided first run without path/Charms; one line per hint; the three "aha" moments within 90 s; measured with a first-time tester.
- [ ] **Audio** (`03` Part C): layered ambient (shop / wave / Guardian), one sound per UI component, cap on simultaneous sounds; licensed loops and SFX.
- [ ] Localization: key-based strings, **EN + IT**.
- [ ] On-device profiling with 60 enemies, Sprites and all effects: 30 fps on 4 GB (Low), 60 fps mid-range.

**Gate 3:** 5 testers call the visuals "calm but attractive"; stable fps targets; FTUE completed by 100% of testers without help; runs 10–15 minutes.

---

### Phase 4 — Meta, Daily and shared replays (3–4 weeks)
**Purpose:** the reason to come back tomorrow, still **offline and local**.

- [ ] **Blueprints and Archive** (`05` §11, `08` §1.3–1.4): unlocking modules and Core types; curve for 60–80 runs; **no stats**.
- [ ] **Daily and Weekly Run** with a seed derived from the UTC date (`05` §12, `10` §1), fixed pool and Core; **local** leaderboard and personal history.
- [ ] Shareable **replay code** (short text or link): import → watch the **ghost** (replay playback) → challenge on the same seed.
- [ ] Endless with a local leaderboard.
- [ ] `Services` interfaces (`05` §16) with fake implementations: analytics, leaderboards, cloud save, consent, ads, purchases. Console log, no SDK yet.
- [ ] Analytics events defined (D13) and called by the fakes.
- [ ] Simple daily missions (`02` §4) and streaks without punishment.

**Gate 4:** a new tester reaches the third run with at least one unlock and understands the Daily unaided; a replay exported from one phone verifies on another (same hash).

---

### Phase 5 — Services and closed test (4–6 weeks, partly in parallel with Phase 4)
**Purpose:** the game leaves the developer's hands (D13).

- [ ] **Unity Gaming Services** (D21): anonymous Authentication, Leaderboards for Daily/Weekly/Endless split by balance version, Cloud Save (local is the truth; on conflict keep the larger progress and notify), **Cloud Code C#** that re-simulates the replay and accepts the score only if the hash matches. Cost estimate written in `04`.
- [ ] Decision **D23**: server verification at launch (recommended, the engine is ready) or sampled. If sampled, replays are kept anyway for retroactive checks.
- [ ] Analytics + crash reporting (decision **D24**) and **UMP consent** before the tutorial in the EU/UK only (D16).
- [ ] Minimal privacy policy; tester Discord; **closed test** build on Google Play, 12+ testers × 14 days.
- [ ] Weekly cycle: data → bot → balance → new build. Tester replays feed the bot (their strategies become bots).

**Gate 5:** 14 days of closed test completed; crash-free ≥ 99%; Daily leaderboard working with verification; tester D1 ≥ 35% (`05` §17, with due caution on small numbers).

---

### Phase 6 — Soft launch (3–8 weeks)
**Purpose:** decide the monetization model **with data** (`02` §2) and polish.

- [ ] Implement **both** models behind a flag: A (act 1 + Daily free, one-time full unlock) and B (optional ads only, non-renewing pass, VIP). Neither touches competitive play.
- [ ] Store listing: final name (**D25**), icon, screenshots, 15-second video "from 10 damage to 10 million" (`10` §3.5); **one hour with an IP lawyer** (D20); IARC; AI content declaration.
- [ ] Localization: ES, PT-BR, DE, FR (D7), native review for the store.
- [ ] Two similar countries, one per model (or A/B), €300–1,000 of Google Ads on the best organic videos.
- [ ] Measure: D1/D7/D30, run length, runs per day, revenue per install, rating.

**Gate 6:** `05` §17 KPIs reached or a written corrective plan; model chosen and recorded in `04`.

---

### Phase 7 — Launch and updates
- **Launch** (Android): Daily, Weekly, Endless, verified leaderboards, ghosts of the best.
- **Update 1** (+2–3 months, gated by `10` §2.4): weekly **Leagues** of 30 (same seed and kit) and **Duel** with a rating; Grades 4–10; new modules (Prism, Singularity, Harvester…) towards about 40.
- **Update 2** (gated): **Siege** and **Community Guardian**; seasonal cosmetic themes (never the enemy hue on Core and modules).
- **iOS:** after the Android launch, with a Mac or Unity Build Automation.

---

## 3. Calendar
The operational calendar is in **`10` §4** (revised mapping in **§4.2**) (6-month roadmap, October 2026 → March 2027, one month per phase with gates and a "if it fails" line). The phases in this file stay; the dates in `10` apply to them. The compression to 6 months rests on three explicit choices (`10` §4.0): continuous balancing with the bot, art in parallel with content from month 2, and Pass/Leagues/Duel/Siege/iOS outside the release. Durations are for one person with AI; **the gate matters more than the date**.

---

## 4. Where we are and next steps
**Status (2026-09-22):** Phase 0 ✅ · Phase 1 ✅ (Gate 1 with testers pending) · Phase 2 ✅ in code (Gate 2 with testers pending) · **Phase 2.5 🔄** (design ✅; foundations A1–A7 ✅ except the device checks of A3/A4; slices **B, C, D ✅**; E1 build and E3 testers left) · Phase 3 ⬜ (waits for approval of `11`–`12`). 47 automated tests, balance `0.3.0`.

**Done in order:** bot runner → ScriptableObject content → `BuySlot` → compact notation → UI Toolkit HUD → magnetic drag with previews → merge animation and wave summary → options and haptics → 14 modules, 7 enemies, 3 acts, elites → balance v0.3 → Core types, Grades 1–3, Endless → replay R2 → save/resume → Core tuning, enemy silhouettes.

**Next, in order:**
1. ✅ **Android development build** (2026-09-22, Pixel 10, Android 17). Still to check by hand: dragging feel, haptics, fps on a 4 GB phone.
2. ✅ **Slices B (wave), C (shop) and D (overlays)** — 2026-09-22, see their tables above.
3. **Phase 2.5-A — foundations**: A1 view split ✅ · A2 camera ✅ · A5 sky and rings ✅ · A6 models v1 ✅ · A7 UI foundations ✅ (tokens → USS, fonts, icons) · A3 light/post and A4 shader 🔄 (quality levels, draw calls and device check left) · then **2.5-B Wave slice**.
3. **2.5-B Wave → 2.5-C Shop → 2.5-D Wave end and overlays → 2.5-E device + testers** (Gate 1, 2 and 2.5 together, on the new look).
4. In parallel, the user: approve `11` (names, gates, economy) and choose the launch scope of `12`; open the **Google Play account** (D13).
5. **Phase 3a** in the order of its table; then 3b art completion and 3c FTUE, audio, languages, profiling.
6. Phase 4: Garden (Archive), Daily/Weekly with local leaderboard, replay codes and ghosts, `Services` fakes.

**Resume here (handoff, 2026-09-22):**
- *Waiting on the user:* (1) approval of `11` names and systems and the launch scope of `12` — Phase 3 does not start before; (2) Google Play account; (3) **Gate 2.5 with 3–5 testers** (E3), on the build of E1.
- *Next task:* **E3, the testers** (3–5, muted, on the APK in `Builds/Android/`), and the Low/Medium/High quality split left over from A3. After that, Gate 2.5 and Phase 3.
- *Device notes (Pixel 10):* drive it over adb from `…/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb.exe`. Relaunching without `am force-stop` first crashes Unity with "UnityFoldingFeaturesWrapper.init() should be called only once" — that is the relaunch, not the game. Frame times: `dumpsys SurfaceFlinger --latency "<the (BLAST)# layer from --list>"`.
- *Where things are:* design system (tokens, icons, all screen mockups) — artifact linked in `docs/README.md`; tokens copy `Tools/design/tokens.json` → `python Tools/tokens_to_uss.py` regenerates `Theme.uss` (including `.theme-contrast` and `.text-large`); fonts → *TowerDefense → Content → Build font assets* (`Assets/Scripts/Editor/FontMenu.cs`); icons → `python Tools/icons_to_png.py`; models → run `Tools/blender/build_models_v1.py` in Blender; arena code in `Assets/Scripts/Presentation/Arena/`, HUD in `UI/HudView.cs` (now ~1,200 lines: splitting it is the first job of Phase 3's UI work).
- *Working notes:* a **dynamic font asset rewrites itself** whenever Play mode meets a character its atlas does not hold, which shows up as a 2 MB diff on `Assets/UI/Fonts/*.asset`. It is noise: `git checkout -- Assets/UI/Fonts/` before committing, or rebuild them from the menu. Portrait captures = render the camera into a 1080 × 1920 render texture **and** set `PanelSettings.targetTexture`, then read the pixels in a **second** call — the panel needs a frame to repaint (always restore both to null afterwards). `Time.timeScale = 0` freezes anything driven by `Time.time`, which is how a 2.5 s bubble can be captured at all. Unity sometimes drops `InputSystem_Actions` from `preloadedAssets` in `ProjectSettings.asset` after Play mode — revert that line. USS reloads during Play leave a stale HUD in the editor (restart Play).

**Known open points:** the Grade ladder is steep between 1 and 2; Mortar and Echo rarely appear (rare + act 2); the greedy bot never buys economy modules, so economy builds are untested by bots; the state hash gained fields (slot count, elites, dash timing) under balance version 0.3.0 — no replays from 0.2.0 exist.

## 5. Open decisions
D22–D25 and the open proposals are listed in **`04` §Open decisions**.

## 6. Main plan risks
| Risk | Signal | Response |
|---|---|---|
| The shop is not fun with simple shapes | Gate 1 failed | Revisit neighbourhood rules or slot count **before** art |
| Balance "broken" by one combo | the bot exceeds 85% with one strategy | targeted nerf, then Grades; the simulation keeps the loop short |
| Performance on 4 GB phones | < 30 fps with 60 enemies | low quality level: no shadows, no bloom, fewer effects |
| Google Play account timing | procedure not closed by Phase 4 | start it in Phase 1–2 |
| UGS costs above forecast | estimate in Phase 5 | sampled verification; Nakama as plan B (D21) |
| Looking like a copy (D20) | tester feedback | one hour with the IP lawyer before the store; different visual identity from the mood shot on |
| Scope creep from `11`–`12` | 3a past November | launch with one batch less per family (24 modules, 10 enemies); Outposts and editions to update 1 (`10` §4.2) |
| Screens drift apart visually | mockup comparisons fail | every slice is checked against its design-system mockup; tokens are generated, never typed |
