# 06 — Development plan by phases

> Version 1 · 2026-09-21 · Status: active. The month-by-month calendar is in `10` §4; this document defines the **phases, deliverables and exit gates**.
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

- [ ] **Missing modules** (`05` §6): Arc, Lance, Mortar, Echo, Salvage, Frost, Capacitor → 14. Each with a behaviour test and a determinism test.
- [ ] **Missing enemies** (`05` §8): Dasher, Splitter, Warden; **elites** from act 2; unlocks per act (not only per wave within the act).
- [ ] **Acts 2 and 3:** Guardians with varied escorts; themed waves; preview with icons and quantities.
- [ ] **Core types** (Standard, Merchant, Bastion, Glass) as data; choice at run start.
- [ ] **Grades 1–3** (the first three modifiers) and **Endless** after the win.
- [ ] **Save and resume at every shop** (the save is seed + commands + version, i.e. the replay itself; resume = re-simulation). Test: save → reload → same hash.
- [ ] **Replay `R2`:** + game version, mode, Core type, Grade. `R1` stays readable.
- [ ] **Balance v0.3** with the bot: targets `(estimate)` — naive bot wins act 1 in 80–90% and the whole run in 10–20%; "max DPS" bot wins the run in 50–70% at Grade 0 and under 30% at Grade 3; no strategy above 85%; simulated run 8–12 minutes of combat (+ shops).
- [ ] Update `05` with the v0.3 numbers and `04` with the decisions taken.
- [ ] **Start now** the **Google Play account** procedure (D13): it takes long and is needed for Phase 5.

**Gate 2:** whole run playable on the phone; bot targets met; real duration with 3–5 testers between 10 and 15 minutes; at least **3 different winning builds** found by testers or the bot (otherwise the 6-slot ring is not enough: `05` §18).

---

### Phase 3 — Mood shot and vertical slice (4–6 weeks)
**Purpose:** the "Dusk Garden" identity (`03`) on a real run, with audio, at 30 fps on a 4 GB phone.

- [ ] **Mood shot** (`03` A9): one carefully made screen, shown to someone **before** producing the rest. Includes the tilted-orthographic vs top-down comparison (D5).
- [ ] Models in **Blender via MCP**: Core with petals, 14 module silhouettes, 6 enemy shards + Guardian, all low-poly, exported to `Assets/Models/`.
- [ ] URP: warm key + cool ambient light, soft shadows at medium/high quality, light bloom only on hits and Pulse; **quality levels** with shadows and bloom off; test on a low-end phone (D4).
- [ ] Final effects (`03` A7): faint lines, Pulse wave, "flake" death, soft numbers.
- [ ] Final UI (rounded typography, soft glass cards, icons). Complete options (`03` B7): text size 100–200%, colour-blind (3 presets + shapes), high contrast.
- [ ] **FTUE** (`05` §14, `07` §1.5): guided act, one line per hint, the "aha" moments within 90 s. Measured with a tester who has never seen the game.
- [ ] **Audio** (`03` Part C): layered ambient (shop / wave / Guardian), interaction sounds, cap on simultaneous sounds. Clearly licensed loops and SFX for now; composer decision deferred to soft launch.
- [ ] Localization: key-based system, **EN + IT** now; text reduced to the minimum (D7).
- [ ] On-device profiling: 30 fps budget with 60 enemies and all effects.

**Gate 3:** mood shot approved; 5 testers say the visuals are "calm but attractive", not "prototype" (direct question); stable 30 fps on the reference phone; FTUE completed by 100% of testers without help.

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
The operational calendar is in **`10` §4** (6-month roadmap, October 2026 → March 2027, one month per phase with gates and a "if it fails" line). The phases in this file stay; the dates in `10` apply to them. The compression to 6 months rests on three explicit choices (`10` §4.0): continuous balancing with the bot, art in parallel with content from month 2, and Pass/Leagues/Duel/Siege/iOS outside the release. Durations are for one person with AI; **the gate matters more than the date**.

---

## 4. Next steps (in order)
1. ✅ Bot runner + CSV (2026-09-21).
2. ✅ `ContentLoader` from ScriptableObjects + equivalence tests (2026-09-22).
3. ✅ `BuySlot` command + tests (2026-09-22).
4. ✅ UI Toolkit instead of IMGUI (2026-09-22).
5. ✅ Dragging with magnet and preview (2026-09-22) — manual check on the phone pending.
6. ✅ Undo and Sell in the UI; animated merge; wave-end summary (2026-09-22).
7. ✅ Basic options + haptics (2026-09-22).
8. Android development build on the phone → Gate 1 with 3–5 testers.

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
