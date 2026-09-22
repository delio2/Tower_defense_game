# 07 — Gameplay brief (Phase 1)

> Version 1.1 · 2026-09-22 · Status: brief (HP curve aligned with v0.3; P2 now proposed as the Resonator module in `11` §2.3). The *executable* specification of Phase 1 (`06`) from the game-design side: gesture, second and frame level. Numbers stay in `05`; visuals and interaction feel in `03`; screens in `09`. **PROPOSAL** items extend the GDD and are listed in `04` §Open decisions.

---

## 1. Core gameplay and logic

### 1.1 The fantasy in one sentence
*"I build a number engine around a heart that breathes."*
The player does not "place towers": they **assemble a ring** where every piece changes its neighbours, and watch the numbers grow every wave. The emotional reward is double: the **click** of the right piece in the right place (Backpack Battles, Balatro) and the **explosion** of numbers (Balatro, The Tower), shown calmly.

### 1.2 The two input states (one hand, portrait)
The game has exactly two states. In each, the thumb has **one main gesture**.

| State | Main gesture | Secondary gestures | What you cannot do |
|---|---|---|---|
| **Wave** (time runs) | **Tap the Pulse** (large button at the bottom, or tap the Core) | speed 1x/2x/3x · pause · tap a module = read-only tooltip | buy, move, sell |
| **Shop** (timeless) | **Drag** a card onto a slot / a module onto another slot or onto the Sell zone | tap card → tap slot (alternative) · Undo · Reroll · Next wave · long press = details | time does not pass: no pressure |

**Touch specification (implement exactly this, not "by feel"):**
- Targets ≥ **48 dp**; ring slots have a circular hit area of 0.6 units (larger than the visible shape).
- **Drag threshold: 8 dp.** Below it is a tap. So "tap card → tap slot" and dragging coexist without modes.
- The dragged object sits **1 cm above the finger**; the preview is **at the top**, never under the finger (Backpack Battles mobile lesson).
- **Magnet:** within 0.45 units of a valid slot centre the object "falls" into the slot (light haptic tick). Leaving the radius detaches it.
- Release outside a valid slot → **soft return** to the origin (250 ms, ease-out), no error flash, no text.
- **Long press 250 ms** on a card or module → details sheet (stats, level, neighbours affecting it). Closes when the finger lifts.
- **Double-tap guard:** irreversible buttons (Next wave, Reroll, Sell) ignore a second tap within 300 ms.
- **Undo** is always visible in the shop, greyed when the stack is empty. There is no confirmation dialog for any action: Undo is the confirmation.

### 1.3 The ten rules that create depth
(all in the GDD; here the *why* of each)
1. **Two-sided neighbourhood.** A booster boosts only its two neighbours → position is a choice, not a detail. With 6 slots, at most 3 "pure" boosters without wasting adjacencies.
2. **Circular ring.** First and last slots are neighbours → no "dead corner"; every build is a cycle.
3. **Automatic merge.** Buying a duplicate fuses it (up to L3) without taking a slot → duplicates are desirable, not wasted (Super Auto Pets).
4. **Capped interest.** 1 Credit per 5 saved, max 5 → tension "buy now or save?" (Balatro). Bank raises the cap: an economy archetype.
5. **Rising reroll cost.** 1, 2, 3… in the same shop → hunting for the right card has a rising price.
6. **Selling at half.** Changing your mind costs, but not prohibitively → builds can "pivot" mid-run.
7. **Wave preview.** You see what comes → the shop is an answer, not a gamble. Armor → you need high damage; swarm → you need multi-target.
8. **Effect preview.** DPS before → after while dragging → the game teaches combos by itself.
9. **Pulse ready every wave, then 20 s cooldown.** One "free" use per wave: **when** to use it is the in-wave choice.
10. **Core types.** Change the starting rules → the same run plays differently (Balatro's decks).

### 1.4 Why it is not shallow: the decision space of one shop
In an average shop the player weighs: 4 offers × (buy/skip) × up to 6 positions × moves × sales × reroll × saving. But real depth is in the **tensions**, not in the count:

| Tension | The two forces | Where it is felt |
|---|---|---|
| Power now vs interest later | buy the third module or keep 10 Credits (+2 per wave) | every shop from wave 2 |
| Ring space vs merge | a new module takes a slot; a duplicate does not, but costs the same | from the 4th module |
| "Sandwich" boosters | Weapon–Booster–Weapon uses both sides; Booster–Booster wastes one | with the 2nd booster |
| Weapon type vs wave composition | Scatter (3 targets) against swarms; Lance against armored Brutes | when the preview shows a new enemy |
| Economy vs defense | Bank/Bulwark do not fire: every economy slot is one weapon less | act 1, waves 3–5 |
| Reroll vs accept | 1 Credit for 4 new cards, but the cost rises | every time "that" card is missing |

**Enemies as questions, modules as answers** (the key to every act):
| Enemy | The "question" | Good answers | Bad answers |
|---|---|---|---|
| Swarmlet (groups of 5) | "do you have multi-target damage?" | Scatter, Arc, Echo, Pulse | Lance, single Emitter |
| Brute (armor 3) | "are your hits heavy enough?" | Lance, Mortar, Amplifier, Lens (+2 flat beats armor) | Scatter L1 (5 − 3 = 2 per hit) |
| Dasher (×3 dashes) | "can you react fast?" | Overclock, Frost, high range (Lens) | slow weapons without boosters |
| Splitter (splits) | "do you finish what you start?" | high single-hit damage, Scatter for the children | "drip" damage |
| Warden (−50% shield nearby) | "can you hit from afar?" | Lens, Mortar (explosion), Lance (pierces) | short-range weapons |
| Guardian (+5 Swarmlets per 25%) | "does your build handle a boss *and* a swarm?" | balanced build, Pulse saved for the swarm | all single-target builds |

### 1.5 The first 30 seconds, second by second (FTUE, `05` §14 detailed)
The first start uses a **fixed seed** and a single exception to the random shop: the first shop **always contains an Amplifier** (marked `tutorial` in the replay, so it stays verifiable). No menu, no text beyond one line.

| Time | What they see | What they do | What they learn |
|---|---|---|---|
| 0.0–1.5 s | fade from the dusk gradient; the Core breathes; a gem (Emitter) pulses softly on the top petal | nothing | "that is mine, it is alive" |
| 1.5 s | one active button at the bottom: **▶** with a soft halo; line: *"Tap ▶"* | tap | the only control |
| 2–20 s | **Wave 1: 5 Drifters**, one every 3 s, from 5 different directions. The Emitter fires: thin teal line, a faint "8"; the shard melts into flakes | watch (or tap around: no punishment) | "the defense fires by itself" |
| about 8 s | the **Pulse** button fills (ready) without a hint. **Only if** a Drifter gets within 2.0 of the Core: line *"Tap Pulse"* | maybe tap | the ability exists; they try it when needed |
| about 20 s | last Drifter dead → **summary**: "Wave 1 ✓", Credits roll 6 → 10 digit by digit (0.6 s), rising tone | tap to skip (optional) | "winning pays" |
| about 22 s | the shop: the camera moves in on the ring (0.6 s ease); 4 cards rise from the bottom; line: *"Drag the **Amplifier** next to the Emitter"*; the two slots next to the Emitter breathe | pick up the card | the main gesture |
| about 25 s | while dragging: valid slots lit, magnet, at the top *"DPS 16 → 24"*; on release a **golden line** lights up between Amplifier and Emitter, medium haptic | release | **first "aha"**: position matters |
| about 30 s | line: *"Tap ▶"*. Wave 2: 5 Drifters + 1 Swarmlet group; the numbers are bigger ("12") | tap | "the numbers go up" |
| about 55–60 s | the swarm arrives tight; line *"Tap Pulse"* when 3+ enemies are within 3.0 | tap Pulse | **second "aha"**: the wave pushes everything back |
| about 90 s | second shop with an Emitter on offer; line *"Buy another Emitter"* → animated merge, "Level 2" | drag onto the Emitter | **third "aha"**: duplicates merge |

After the third "aha" the hints disappear forever. **Rule:** one line per hint, never a panel, never an "OK".

### 1.6 The loop of a normal wave ("physics" without physics)
The simulation is deterministic and has no physics engine; the **presentation** adds the weight (catalogue in `03` A7): spawn fade and slow self-rotation, radial advance with a light float, hit lines with a visual nudge, flake deaths, the Pulse ring with interpolated knockback, a coral tint on Core hits (no screen shake), the ring pulsing at wave end, then the summary.

### 1.7 PROPOSALS to make the Pulse a choice, not a reflex
They answer the open question in `05` §18 ("Is the Pulse a choice?"). To test **one at a time** at Gate 1, behind a flag:
- **P1 — Resonance window:** if the Pulse hits **3+ enemies**, the next cooldown is −25%. Rewards waiting for the group without punishing early use.
- **P2 — The Pulse triggers the neighbours:** on Pulse, every weapon on the ring fires immediately (cooldown reset). Ties the ability to the build: more weapons, more value.
- **P3 — Focus (second gesture):** tap an enemy during the wave → for 3 s every weapon in range prefers it; 8 s cooldown. Adds agency but is a second gesture: weigh against the "one gesture per state" rule.
Recommendation: P1 first (zero cost, no new gesture), P2 as a module (**Resonator**, rare booster) rather than a global rule, P3 only if testers say "I get bored during the wave".

---

## 2. Difficulty and learning curve

### 2.1 Inside the run: two curves that must cross at the right moment
- **Enemy curve:** HP ×1.12 per wave since balance v0.3 (wave 18 ≈ ×6.9; ×1.20 in v0 made the run unwinnable, `05` §18), budget ×1.10 (wave 18 ≈ 40 points = about 40 Drifters or 13 Brutes). New types one at a time, each introduced **alone** with a 2 s pause.
- **Player curve (estimate with an average build):**
  | Wave | Typical build | Estimated DPS | Drifter HP | Hits to kill |
  |---|---|---|---|---|
  | 1 | Emitter | 16 | 20 | 3 |
  | 3 | Emitter + Amplifier + Scatter | ~45 | 29 | 2–3 |
  | 6 (Guardian 1) | Emitter L2 + 2 Amplifiers + Scatter + Bank | ~120 | 50 (Guardian 1,000) | 2 |
  | 12 (Guardian 2) | L2–L3 weapons, L2 boosters | ~600 | 150 (Guardian 3,000) | 1–2 |
  | 18 (Guardian 3) | 2 L3 weapons between 3 L2–L3 boosters | ~2,500+ | 440 (Guardian 8,800) | 1 |
  Player growth is **multiplicative** (level × booster × booster), enemy growth **exponential but linear in the log**: the game is designed so the player *overtakes* the curve **if** they build well, and crosses below it **if** they do not. The intended crossing point is wave 9–12 for a new player, never wave 18 (otherwise defeat always comes at the end and feels unfair).
- **The Guardian is the act's exam:** it asks two things at once (boss + swarm). A single-target build finds out at 75% of its health, when the first 5 Swarmlets arrive, not at death.
- **Elites** from act 2 (HP ×3, armor +1): a "mini exam" per wave, never more than two.
- **Themed wave** (wave 3 of every act): one type only → the build is tested on a single question.

**Tuning levers** (order to turn them, with the bot): first-wave budget → HP growth → rare module cost → interest cap → Credits per wave. Numeric targets in `06` Phase 2.

### 2.2 Run after run: four sources of novelty, none of power
| Source | What changes | When |
|---|---|---|
| **Archive** (Blueprints) | the shop **pool**: new modules and Core types | from run 2–3, complete in 60–80 runs |
| **Core type** | starting rules (economy, resistance, ×1.5 damage with 50 HP) | from the first win |
| **Grades 1–10** | one extra rule per Grade, cumulative | after the first win |
| **Daily / Weekly** | fixed seed and pool, weekly modifiers ("rare boosters only", "5-slot ring") | from launch |

**PROPOSAL — the ten Grades** (one at a time, cumulative, like Slay the Spire's Ascension):
1. Enemies +10% HP · 2. −1 Credit per wave · 3. Elites from act 1 · 4. The Guardian has a shield (−25% damage) while its escort lives · 5. Reroll starts at 2 · 6. Interest cap 4 · 7. One more elite per wave · 8. Pulse cooldown 25 s · 9. Enemies enter over 16 s instead of 20 · 10. 5-slot ring until the first Guardian.
Each Grade changes **one** readable rule: the player always knows why it is harder.

**Expected learning curve:**
| Run | What they understand | Signal in the data |
|---|---|---|
| 1–3 | neighbourhood, merge, Pulse | clears act 1 |
| 4–10 | interest, reroll discipline, selling to pivot | unused Credits at run end decreasing |
| 10–30 | **archetypes**: swarm (Scatter+Echo), sniper (Lance+Lens+Amplifier), economy (Bank → late), fortress (Bulwark+Capacitor+Pulse) | wins at Grade 0, tries different Cores |
| 30+ | Grades, Daily, seed optimization | stable time per run, Daily scores |

### 2.3 Anti-boredom: why run 40 is not run 4
- **Seed:** directions, wave composition and offers differ every run (but are identical for everyone in the Daily).
- **Every act introduces an enemy that contradicts a build** (table §1.4): there is no "always right" build.
- **Identity by wave 6:** the shop pushes you to "become something" (merge and boosters) before the first Guardian; runs get told ("the three-Scatter run").
- **Endless** after the win: growth continues and "broken" builds find their place on the leaderboard, not in the base game.
- **Shareable builds** (ring screenshot, replay code): the community produces content.

### 2.4 Defeat must teach, not punish
- Defeat screen: **what killed you** (enemy type, wave), the final ring, DPS, a *"Replay this seed"* button and *"Share"* (`09` §2.4).
- No loss of meta-progression: the Blueprints of the waves cleared stay.
- 10–15 minute runs: retrying is cheap (Brotato, Balatro).
- **PROPOSAL:** after 3 consecutive defeats in the same act, the game offers **one** contextual tip ("Brutes have armor: Lens adds +2 flat damage"), never a hidden difficulty nerf (players notice and hate it).

---

## 3. The first 10 seconds in the app and the funnel
Where the 77% three-day loss happens (D8).
| Second | What happens | Target |
|---|---|---|
| 0–1.5 | cold start; **one** logo (≤ 0.8 s) or none; no visible loading | start < 2 s on mid-range |
| 1.5 | (EU/UK only) UMP consent, one screen | answer < 5 s |
| 2 | breathing Core, Emitter, ▶ button | **zero menus** |
| 2–6 | tap → first Drifter → first kill | first kill within 6 s of the tap |
| 20 | first Credits, first shop | first choice within 25 s |
| 30 | first combo (aha 1) | — |
No login, no notification request, no store rating prompt, no purchase shop in the first 3 days.

**Measured funnel** (events, D13): `app_open → first_tap → first_kill → first_shop → first_combo → wave_2 → act_1_clear`. Targets `(estimate)`: first_tap ≥ 95% · first_combo ≥ 85% (= tutorial completed, `05` §17) · act_1_clear ≥ 60% on the first run. Any step losing more than 10% is a design bug, not the player's fault.

---

## 4. What enters Phase 1 from this document
All of §1 (input, FTUE, wave loop), P1 from §1.7, and the funnel events of §3 defined in the fake `Services`. The Grades (§2.2) are Phase 2. The visual catalogues are in `03` and follow the `06` phases.

**Decisions to confirm:** P1 (Pulse resonance) · the ten Grades · the contextual tip after 3 defeats.
