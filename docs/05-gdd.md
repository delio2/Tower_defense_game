# 05 — Game Design Document (GDD v0.2)

> Version 0.2 · 2026-09-21 · Status: **source of truth** for rules, numbers and architecture. Balance values are **v0 starting values**, to be tuned with the prototype and the balance bot (D11, `06` Phase 2). Section numbers are referenced from code comments: **do not renumber**.
> **Working title:** open (D25) · **Genre:** roguelite core defense with a shop and combos · **Platform:** Android, then iOS · **Orientation:** portrait · **Audience:** adults 18–45 · **Model:** honest, *"never pay-to-win"* (`02`).
> Rationale: `00`–`04` (especially D17–D21). In-game names are English, like the code.

---

## 0. Glossary (in-game terms)
| Term | Meaning |
|---|---|
| **Core** | the nucleus at the centre, to defend. When its integrity reaches 0 the run ends |
| **Integrity** | the Core's hit points (100 at start) |
| **Ring** | the ring of slots around the Core (6 at start, up to 8) |
| **Slot** | a position on the Ring that holds one Module |
| **Module** | what is mounted on a slot: weapon, booster or economy |
| **Neighbours** | the two adjacent slots on the ring (the ring is circular: first and last are neighbours) |
| **Merge** | buying a module you already own fuses it with the existing one and raises its level (max 3) |
| **Credits** | the currency **inside** the run, for the shop |
| **Blueprints** | the **permanent** currency, to unlock new modules and Cores. It never buys power |
| **Archive** | the unlock tree where Blueprints are spent (`08` §1.3) |
| **Pulse** | the Core's active ability, with a cooldown |
| **Wave** | one attack; time advances only during waves |
| **Act** | 5 waves + 1 Guardian. A run has 3 acts |
| **Guardian** | the end-of-act boss |
| **Core type** | the Core variant chosen at run start, which changes the rules (like Balatro's decks) |
| **Grade** | an unlockable extra difficulty level (like Ascension) |
| **Replay** | version + seed + mode + Core + list of (tick, command): enough to reproduce the run |
| **Ghost** | another player's replay played alongside your run on the same seed |
| **Seed** | the number that fixes every random draw of a run |
| **Tick** | one simulation step (60 per second) |

## 1. Pillars
1. **Choices that matter:** shop, merge and arrangement on the ring. Every wave changes the build.
2. **Numbers that explode, calmly:** readable exponential growth, calm visuals (`03`).
3. **Respect for time:** 10–15 minute runs, no timers, no energy, offline.
4. **Honest:** never pay-to-win; competition is fixed-kit.
5. **Shareable:** every run is a replay, challengeable and verifiable.

## 2. Loop and duration
| Level | Duration | Content |
|---|---|---|
| Wave | about 25–30 s | Enemies from all sides, modules fire by themselves, you use the Pulse |
| Shop | free (about 10–20 s) | Buy, merge, arrange, sell, reroll → "Next wave" |
| Act | about 4–5 min | 5 waves + Guardian |
| Run | about 10–15 min | 3 acts (18 waves). After a win you can continue in Endless |
| Meta | weeks | Unlock modules and Cores with Blueprints, Grades, online modes |

Automatic save **at every shop**, resumable at any time.

## 3. Arena and units (deterministic)
- **Fixed tick: 60 per second.** No floats in game state (D11).
- **Units:** balance data is in **milli-units** (1000 = 1 unit); simulation state uses **micro-units** (1,000,000 = 1 unit) to avoid precision loss. The Core is at (0,0) with radius 0.7. Ring slots are at radius 1.5. Enemies spawn at radius 9.0.
- **Directions:** **192** fixed directions (integer cosine table ×10,000; sine is the cosine shifted by a quarter turn). 192 is divisible by 6 and 8, the possible ring sizes.
- **Enemy movement:** **radial** towards the Core. Each enemy has a direction and a distance; every tick the distance decreases by its speed. Knockback increases it.
- **Contact:** at distance ≤ 0.7 the enemy hits the Core (contact damage) and disappears.

## 4. The Core
- **Integrity:** 100.
- **Pulse:** 20 damage + **1.5-unit knockback** to every enemy within 3.0 of the Core. Cooldown 20 s (1,200 ticks); ready at the start of every wave. Modules can modify it. *(Proposals to make it a richer choice: `07` §1.7.)*
- **Core types** (variants; the prototype uses Standard):
  | Core type | Rule | Start |
  |---|---|---|
  | **Standard** | — | Emitter in slot 0, 6 Credits |
  | **Merchant** | +1 interest cap, 5 slots | 10 Credits, no module |
  | **Bastion** | integrity 150, Pulse cooldown −25% | Bulwark, 4 Credits |
  | **Glass** | all damage ×1.5, integrity 50 | Emitter + Amplifier, 2 Credits |

## 5. The Ring
- **6 slots** at start, arranged as a hexagon. The **extra slot** (up to 8) is bought in the shop after the first Guardian, for 8 Credits *(not in the prototype yet, which has one act)*.
- **Neighbourhood:** every slot has 2 neighbours; the ring is circular.
- In the shop modules **move freely** (drag or swap); during a wave they are locked.

## 6. Modules (v0: 14 for the MVP; the prototype has 7 — Emitter, Scatter, Amplifier, Lens, Overclock, Bank, Bulwark)
Cost in Credits. Range in units, measured from the module's position on the ring. Cooldown in ticks (60 = 1 s). Rarity: C common · U uncommon · R rare.

**Weapons**
| Module | Rarity | Cost | Damage | Cooldown | Range | Behaviour |
|---|---|---|---|---|---|---|
| **Emitter** | C | 3 | 8 | 30 | 4.0 | hits the enemy closest to the Core |
| **Scatter** | C | 4 | 5 ×3 | 45 | 3.5 | hits the 3 enemies closest to the Core |
| **Arc** | U | 5 | 6 | 40 | 4.0 | chains over 4 enemies (1.5 jump), −10% per jump |
| **Lance** | U | 5 | 18 | 90 | 6.0 | pierces every enemy on a line towards the target |
| **Mortar** | R | 7 | 16 | 90 | 7.0 | 1.2-radius explosion on the farthest enemy in range (minimum 2.0) |

**Boosters** (affect the **two neighbours**)
| Module | Rarity | Cost | Effect (level 1) |
|---|---|---|---|
| **Amplifier** | C | 3 | neighbours' damage ×1.5 |
| **Lens** | C | 3 | neighbours: +1.5 range and +2 flat damage |
| **Overclock** | U | 4 | neighbours: cooldown −25% |
| **Echo** | R | 6 | every neighbour shot fires a second one at 50% |

**Economy and utility**
| Module | Rarity | Cost | Effect (level 1) |
|---|---|---|---|
| **Bank** | U | 4 | +1 interest cap and +1 Credit per wave |
| **Salvage** | C | 3 | +1 Credit per 10 kills in the wave |
| **Bulwark** | C | 3 | +25 max integrity; repairs 5 every wave |
| **Frost** | U | 4 | enemies within 3.0 of the Core are slowed by 25% |
| **Capacitor** | U | 4 | Pulse cooldown −20%; Pulse damage +50% |

**Merge (levels):** buying an owned module (below level 3) **merges automatically**, without taking another slot.
| Level | Weapons (damage) | Boosters and economy (effect) |
|---|---|---|
| 1 | ×1.0 | ×1.0 |
| 2 | ×1.8 | ×1.6 (e.g. Amplifier ×1.8 instead of ×1.5) |
| 3 | ×3.0 | ×2.2 (Amplifier ×2.1) |

**Selling:** half of the Credits invested (rounded down, minimum 1). Yield analysis: `08` §1.1.
*(After the MVP: Prism, Singularity, Harvester and other legendary modules; target about 40 modules at launch.)*

## 7. Damage formula ("numbers that explode")
**Hit damage = (base damage × level + flat bonuses) × product of multipliers**

- **Flat bonuses:** Lens and similar. **Multipliers:** Amplifier, Glass Core, Echo (50% on the second shot)…
- Effects **multiply each other** (two Amplifiers next to a weapon = ×2.25): the source of exponential growth.
- **Armor:** flat reduction per hit, with a minimum damage of 1.
- **Showing numbers:** compact notation (1.2K · 3.4M · 5.6B). Small, faint numbers for normal hits; larger (no flash) for hits above 25% of the target's health. An option hides them.

## 8. Enemies (v0)
HP in game units at wave 1. Speed in units per second.
| Enemy | HP | Armor | Speed | Contact | Points | Special | From |
|---|---|---|---|---|---|---|---|
| **Drifter** | 20 | 0 | 0.9 | 5 | 1.0 | — | act 1 |
| **Swarmlet** | 6 | 0 | 1.3 | 2 | 0.3 | arrives in groups of 5, close together | act 1 (wave 2+) |
| **Brute** | 70 | 3 | 0.55 | 15 | 3.0 | armored | act 1 (wave 3+) |
| **Dasher** | 14 | 0 | 0.8 | 5 | 1.5 | every 3 s dashes ×3 for 0.5 s | act 2 |
| **Splitter** | 30 | 0 | 0.85 | 6 | 2.0 | on death splits into 2 Swarmlets | act 2 |
| **Warden** | 40 | 1 | 0.7 | 8 | 3.0 | shield: −50% damage to enemies within 1.5 | act 3 |
| **Guardian** | 400 | 5 | 0.4 | 40 | — | end-of-act boss; every 25% of health lost it calls 5 Swarmlets | wave 6 of every act |

- **Elites** (from act 2): HP ×3, armor +1, double halo. One or two per wave.
- **Shapes:** faceless and never childish; the visual language is in `03` A5. Which enemy counters which build: `07` §1.4.

## 9. Waves and difficulty
- Global index **g = 1…18** (act a, wave w: g = 6(a−1) + w; w = 6 is the Guardian).
- **HP:** `hp(g) = 1.20^(g−1)` (wave 18 ≈ 22×). **Budget:** `budget(g) = 8 × 1.10^(g−1)` points (wave 18 ≈ 40).
- Enemies enter over **20 s**, at regular intervals, from **random but seeded directions**. Rules: a new type first enters **alone** (then a 2 s pause); at most 3 types per wave; wave 3 of every act is "themed" (one type).
- **Guardian wave:** the Guardian enters first; after 3 s an escort arrives with **half the wave budget**.
- Swarmlets cost 0.3 points **each**, so a group of 5 costs 1.5.
- **Preview:** the shop shows the next wave's composition (icons and quantities).
- Integer or fixed-point maths, no `Math.Pow` (not deterministic across platforms): growth is computed by repeated multiplication.
- **Grade** (Ascension): after the first win, Grades 1–10 unlock (a proposed list of rules is in `07` §2.2).
- **Endless** after the win: growth continues, for leaderboards and wild builds.

## 10. Economy and shop
- **Starting Credits:** by Core type (Standard: 6).
- **At wave end:** 4 Credits + **interest** (1 per 5 held, max 5) + 3 after a Guardian.
- **Shop:** **4 offers**. Probabilities: C 60% · U 30% · R 10% (R only from act 2). Unlocked modules only.
- **Reroll:** 1 Credit, +1 for every further reroll in the same shop.
- **Buying:** requires a free slot, or a merge. With a full ring you must sell first.
- **Extra slot:** fixed offer after the first Guardian (8 Credits, up to 8 slots).
- **Wave preview** always visible in the shop.
- Expected Credit flow over a run and tuning levers: `08` §1.2.

## 11. Permanent progression (no power grind)
- **Blueprints:** 1 per wave cleared, 3 per Guardian, + Grade bonus.
- **Archive** (unlocks): spend Blueprints to add **new modules to the pool** and **new Core types**. **No stat upgrades.** Tree structure and costs: `08` §1.3–1.4.
- Goal: full unlock in about 60–80 runs `(estimate)`. Timing and honest acceleration in `02` and `08`.
- **Competitive modes** use a **fixed pool and Core, identical for everyone**: unlock state does not count.

## 12. Modes
| Mode | Phase | Rules |
|---|---|---|
| **Run** | launch | Choose Core and Grade; offline |
| **Daily Run** | launch | Seed of the day, fixed Core and pool; leaderboard; ghosts of the best (`10` §1) |
| **Weekly Run** | launch | As above, weekly, with special rules (mutators, `10` §2.2) |
| **Endless** | launch | After the win: infinite growth |
| **Leagues** | update 1 (gated) | Groups of 30, weekly, promotion and relegation; same seed and kit for the group |
| **Duel** | update 1 (gated) | Same seed as a real opponent (their replay); you see their progress; skill rating |
| **Siege** | update 2 (gated) | Build an attack wave with a budget; others face it; rewards if they fall |
| **Community Guardian** | update 2 (gated) | Weekly boss with health shared by everyone |

Proposed secondary modes (Guardian Gauntlet, Surge): `10` §2.1. Gates for updates 1 and 2: `10` §2.4.
**Competitive score:** waves cleared (first) → total damage (second key) → fewer ticks (third).
**Day and week:** the Daily seed changes at midnight **UTC**; the Weekly between Sunday and Monday UTC. Our backend (UGS, D21) decides the boundary, not Play Games Services.

## 13. Replay and verification (D19)
- **Target format:** game version + balance version + mode + seed + Core type + Grade + list of (tick, command).
- **Current prototype format (`R1`):** balance version + seed + final hash + waves + total damage + ticks + commands. **Missing:** game version, mode, Core type and Grade: to be added (format `R2`) when Core types and modes arrive.
- **Commands:** `Buy(offerIndex, slot)` · `Sell(slot)` · `Move(from, to)` · `Undo` · `Reroll` · `StartWave` · `Pulse`. Pause and game speed are **not** commands: they do not change the outcome.
- **`Undo`** restores the shop state before the last `Buy`, `Sell` or `Move` of the current visit (multi-level). `Reroll` and `StartWave` clear the stack: a reroll cannot be undone, otherwise future offers could be peeked for free. `Undo` does not use the RNG, so it stays deterministic and is recorded in the replay.
- **Verification:** re-playing the replay must give the same final hash and score. Locally now (tests); on the server with Cloud Code C# (D23).
- Replays are valid **only for the same balance version**: leaderboards and ghosts are split by version or season.

## 14. First run (FTUE)
| Time | Event |
|---|---|
| 0 s | (EU/UK only) consent → guided act, reduced effects, no menu |
| about 5 s | The Core already has an Emitter: "Tap ▶". 5 Drifters arrive and the Emitter takes them down |
| about 30 s | **First shop (guided):** "Buy the Amplifier and put it **next to** the Emitter" → next wave the numbers rise ×1.5 (**first "aha"**) |
| about 60 s | A swarm arrives: "Tap the Core for the **Pulse**" → a wave that pushes enemies back (**second "aha"**) |
| about 90 s | Second shop: "Buy another Emitter" → **merge** to level 2 (**third "aha"**) |
| later | The rest is discovered by playing; Archive and Daily unlock one at a time in the first 3 runs |

At most **one line of text** per hint. Second-by-second script: `07` §1.5. Progressive disclosure of the UI: `09` §3.2.

## 15. Interface (portrait, one hand)
- **Top:** Core integrity · wave X/18 · Credits.
- **Centre:** the circular arena with the Core and the ring.
- **Bottom, during a wave:** a large **Pulse** button with visible cooldown · speed 1x/2x/3x · pause.
- **Bottom, in the shop:** 4 **offer cards** (icon, name, cost, short effect) · Reroll · **Undo** · **Next wave**. Active combos are shown as **soft lines** between neighbours.
- Buttons of at least 48 dp, very little text, icons. Screens and components: `09`.

### 15.1 Shop interaction: dragging (details in `03` Part B)
| Gesture | Result |
|---|---|
| Drag a **card** onto a free slot | Buys and places the module |
| Drag a card onto an **equal module** | **Merge**, with its animation |
| Drag a **module** onto another slot | Moves it (swap if the slot is occupied) |
| Drag a module onto the **Sell zone** | Sells it (refund shown before release) |
| Tap a card, then a slot | No-drag alternative |
| **Undo** | Reverts the last shop purchase, sale or move (not the reroll) |

- **While dragging:** the object stays **above the finger**; valid slots light up; the **magnet** snaps to the nearby slot; the **effect preview** appears at the top (e.g. "DPS 16 → 24 (+50%)", "Level 2 → ×1.8", "Sell: +2").
- **Preview computed by the simulation** (read-only function on a copy of the ring): what you see is exactly what happens.

### 15.2 Options (from the prototype)
Reduce motion · effect intensity · damage numbers (all / big only / none) · haptics · speed 1x/2x/3x. Later: text size, colour-blind modes, high contrast (`03` B7).

## 16. Technical architecture
| Module | Responsibility |
|---|---|
| `Simulation` | fixed-tick loop (60/s), deterministic, integers; no Unity types; separate assembly (asmdef without UnityEngine) |
| `Arena` | directions (integer table), positions, ranges |
| `Core` / `Pulse` | integrity, ability, knockback |
| `Modules` / `Ring` | definitions, slots, neighbourhood, multiplier calculation, merge |
| `Enemies` / `Waves` | types, radial movement, seeded spawn, preview |
| `Economy` / `Shop` | Credits, interest, offers, reroll, selling |
| `Replay` | recording, format, playback, verification (hash) |
| `Presentation` | calm visuals, numbers, effects, UI: reads the state, never modifies it; owns the ScriptableObject content that fills the simulation's `ContentDatabase` |
| `Meta` | Blueprints, Archive, unlocks, Grades |
| `Save` | local save at every shop (the replay itself: seed + commands + version) + cloud |
| `Services` | leaderboards, online replays, analytics, ads, purchases (behind interfaces; **fake during development**, D13) |

- **Separate RNG streams:** Waves (composition and directions), Shop (offers and rerolls), Effects. Player choices do not change the waves.
- **Tests:** determinism, replay verification, economy, merge, neighbourhood, formulas; **balance bot** that plays thousands of runs (`06` Phase 1).

## 17. Targets (KPI)
| Metric | Target |
|---|---|
| Tutorial completed | ≥ 85% |
| D1 / D7 / D30 | ≥ 35% / ≥ 12% / ≥ 5% |
| Average run length | 10–15 min |
| Runs per player per day | ≥ 2 |
| Store rating | ≥ 4.5★ |

## 18. Open questions (to answer in the prototype)
- [ ] Is the shop with the ring fun **already with simple shapes**?
- [ ] Are combos understood without explanations (lines between neighbours, numbers)?
- [ ] Is the real duration of waves and shops 10–15 minutes per run?
- [ ] Is the Pulse an interesting choice or a button to press as soon as it is ready? (proposals: `07` §1.7)
- [ ] Do big numbers stay readable and calm?
- [ ] Is a 6-slot ring enough to create different builds?
- [x] **First bot probe (2026-09-21, 100 seeds, 1 act):** a "naive" bot (buys at random, first free slot, Pulse when enemies are close) wins **90%**; all defeats are on waves 5–6 (Guardian). Average combat about **24 s per wave**. → Act 1 works as an entry; **acts 2–3 need tuning** (growth and new enemies).

## 19. Prototype status
1. ✅ **Simulation v2** (reuses RNG, hash and commands): radial arena, Core, 3 enemies (Drifter, Swarmlet, Brute) + Guardian, 7 modules (Emitter, Scatter, Amplifier, Lens, Overclock, Bank, Bulwark), shop with merge and undo, previews, Pulse, 1 act. **17 tests.**
2. ✅ **Replay:** recording, playback and hash verification (automated test, including tampering).
3. 🔄 **Calm presentation** with simple shapes and a provisional interface, **with tactile interaction** (§15.1). *Done:* `Prototype` scene, simple shapes, tap card → tap slot, Sell/Move, Reroll, Next wave, Pulse, speed 1x/2x/3x, pause, damage numbers, combo lines. *Missing:* magnetic drag, **Undo in the UI** (the command exists), **previews in the UI** (the `TryPreview*` functions exist but are unused), drag-to-sell, animated merge, wave-end summary, haptics, basic options.
4. ⬜ **Test with 3–5 people** (muted first).
5. ⬜ Decision: go on, fix, or change.

**Exit criteria ("it works"):** at least 3 testers out of 5 ask to play again; they understand the neighbourhood without explanations by the second shop; nobody is stuck for more than 10 s; real duration is measured. The full phase plan is in `06`.
