# 12 — Run structure and synergies (path, Charms, affinities, allies, rare petals, rings)

> Version 1 · 2026-09-22 · Status: **PROPOSAL** brief (Phase 2.5 design, D32). Builds on `11` (roster, gates, economy); nothing enters `05` before approval and a bot probe. Numbers are v0.
> Visual reference: the *Run structure (proposal)* cards in the Dusk Garden design system (https://claude.ai/artifact/5aKRr9H8e3WJsMui64MNnu).

---

## 0. The request and the goal
The user (2026-09-22): give the rings around the Core a meaning (**decided: range bands**, §1); take inspiration from Balatro and other great games for the deck; add more abilities, a **path on the map** you build, **barracks that release allies** chasing enemies, and **rare extra petals** — so runs are dynamic, combo-rich and very different from each other.

**Goal:** every run should tell a different story, like Balatro, without losing the three promises (calm, readable in half a second, 10–15 minutes) or fairness (same seed = same options, replays verifiable).

## 0.1 What the best do (research, 2026-09-22)
| Game | The mechanism that creates different runs | What its players criticise | Our rule |
|---|---|---|---|
| **Balatro** | 5 Joker slots that feed each other; vouchers change the shop; tarot/planet cards modify the deck; editions (foil, holo, polychrome, negative); deck choice + stakes | early luck decides runs ("no good joker = lost run"); 2/3 of jokers situational | several ways to steer luck (path, Market, Lock, Pouch); every Charm must be good in at least one common build |
| **Slay the Spire** | a branching map: normal, elite (relic), rest (heal or upgrade), shop, event — the route is a risk/reward plan | — (loved) | a short vertical path per act (§2) |
| **Teamfight Tactics** | traits with breakpoints (2/4/6): a second, global synergy axis | several half-active traits are weak | 5 affinities with breakpoints 2/4 (§4) |
| **Monster Train** | positional floors + two clans; multiplicative scaling | — | petals are our floors; affinities are our clans |
| **Kingdom Rush** | barracks whose soldiers **block** enemies, giving towers a longer fire window; strong vs fast swarms | — (the series' signature) | Nests release Sprites that block (§5) |
| **Rogue Tower** | the map grows each wave and you choose where | the idea is "not explored to the fullest", pacing issues | growth is rare and meaningful: extra petals and Outposts (§6, §8) |
| **Vampire Survivors / Ball x Pit** | evolutions and fusions discovered by combining pieces | repetition once all fusions are known | recipes are few and valuable; most variety comes from Charms + path |
| **Backpack Battles** | adjacency recipes shown by a glowing line | — | neighbour links already glow; recipes reuse that language |

---

## 1. The rings are range bands ✅ (user decision)
| Ring | Radius | Meaning |
|---|---|---|
| I · Pulse | 3.0 | reach of the Pulse, Frost and Barrier; lights softly when the Pulse is ready; an enemy inside it is "close danger" |
| II · Short | 5.5 | how far short-range Attack modules reach on their side (Emitter, Scatter, Needle, Arc, Ember) |
| III · Mid | 7.5 | mid range (Lance, Prism, Mortar; any short weapon next to Lens) |
| Edge · Gates | 9.0 | the spawn gates; long range (Rail, Seeker) reaches it |
- Cards show range as **●○○ / ●●○ / ●●●** (reaches ring II, III, edge) instead of numbers; the module sheet keeps the number.
- While dragging or long-pressing, the band the module reaches lights in its sector only.
- Range upgrades move a module **one ring out** (Lens, Deep petal, Charms): the change is visible. The simulation keeps exact ranges; the tiers are the rounded public face (ranges are re-tuned to land on the rings: 4.0 → 5.5−1.5, etc.).
- Token change: `arena-ring` now carries information (band edges), still at low strength.

---

## 2. The path: choose your next step (Slay the Spire, made short)
- At the start of each act a **vertical path** of 6 rows appears (portrait-native): rows 1–5 offer 2–3 nodes each, row 6 is the Guardian. Lines connect nodes; the player picks one node per row. The whole act path is visible from the start, so it is a plan, not a gamble.
- After each wave (or Grove) the next node is chosen with one tap; the choice is a replay command `ChooseNode(index)` and the map is seeded (Daily = same map for everyone).
| Node | What happens | Reward |
|---|---|---|
| **Wave** | a normal wave (the row's beat from `11` §5) | normal shop |
| **Elite wave** | the same beat + 2–3 elites | shop + **choose 1 of 2 Charms** |
| **Market** | a normal wave, then a big shop | 6 cards, cards −1 Coin, 1 Charm for sale |
| **Grove** | **no wave** (time saved) | pick one: repair 30 Health · **Graft** (+1 star to a module) · **Petal** (rare, once per act, see §6) · **Prune** (remove 2 cards from your Pouch) |
| **Mystery** | a short choice card (risk/reward) | e.g. "Give up your weakest module → 2 Charms"; "Take a mutator on the next wave → +8 Coins" |
| **Guardian** | the act exam (`11` §4.2) | **choose 1 of 3 Charms** |
- Generation rules (seeded): per act 1–2 Elite, 1 Market, 1 Grove, 0–1 Mystery, the rest Waves; no Grove in row 1; Elite and Grove never adjacent in the same column; at least two different node types per row.
- A run has **15–17 waves** instead of 18 (Groves replace waves): runs get slightly shorter and more varied; the bot re-tunes HP growth.

---

## 3. Charms: the Joker layer
- Run-wide passive rule-changers held in **4 Charm slots** above the ring (small round medallions). They are the combo engine that bends the rules; the ring stays the positional engine.
- **Replace Boons** (`11` §6.3): one system, not two. Sources: Guardian (1 of 3), Elite (1 of 2), Market (buy), Mystery. **Sell** a Charm for 3 Coins; drag to reorder does not matter (no order effects, for readability).
- Unlocked in the Garden (meta variety, never stats). ~30 at launch, 4 rarities.
| Charm | Effect | Kind |
|---|---|---|
| **Compound** | interest also counts Coins spent this shop | economy |
| **Harvest moon** | every 10th kill gives +1 Coin; Mites count double | economy |
| **Kinship** | duplicates cost 1 less; merges give +1 Coin | merge |
| **Long shadow** | Boosts also affect the petals **two** away (at 50%) | rule-bender |
| **Opposites** | modules also count the opposite petal as a neighbour | rule-bender |
| **First light** | the first 3 s of every wave, all Attack modules fire ×2 | tempo |
| **Afterglow** | kills inside ring I heal 1 Health | defence |
| **Steady hand** | Attack modules that did not move this shop deal +20% | planning |
| **Echo chamber** | Echo and Fork effects +50% | module |
| **Wide Pulse** | the Pulse reaches ring II | Pulse |
| **Overflow** | overkill damage jumps to the nearest enemy | damage |
| **Petal +1** | an extra empty petal (rare; counts toward the max of 8) | growth |
| **Sun pact / Frost pact / …** | your affinity counts as one more module (reach breakpoints sooner) | affinity |
| **Gardener** | Groves offer all four options instead of three | path |
| **Scout's map** | see the next act's path and gates in advance | information |

## 4. Affinities: a second synergy axis (TFT traits)
- Every module has **one affinity** among five, shown as a small glyph on the card and the model's inner glow tint (never replacing the family colour). Neighbours are the **local** synergy; affinities are the **global** one.
- Breakpoints count modules on the ring: **2** and **4** (and **6** only with 8 petals). One strong breakpoint beats several half-active ones — the UI shows only the active ones plus the one you are closest to.
| Affinity | Theme | 2 | 4 |
|---|---|---|---|
| **Sun** | burst damage | +15% damage | every 5th hit of each module flares ×3 |
| **Storm** | chains and many targets | chains and splits +1 target | kills release a spark to the nearest enemy |
| **Thorn** | pierce and armor | enemies in range −1 armor | hits make enemies take +10% (stacks 3) |
| **Frost** | control | slows +10% | slowed enemies take +25% damage |
| **Bloom** | growth, economy, allies | +1 Coin per wave | Nests +1 Sprite; Support effects ×1.5 |
Proposed assignment (tunable): Sun — Emitter, Rail, Seeker, Amplifier, Crown, Heart · Storm — Scatter, Arc, Prism, Fork, Echo, Orbit · Thorn — Needle, Lance, Mortar, Lens, Focus, Singularity · Frost — Frost, Beacon, Barrier, Overclock, Tempo, Dew · Bloom — Ember, Bank, Salvage, Vault, Magnet, Scout, Bulwark, Capacitor, Catalyst, Twin, Mirror, Resonator, Nests.

## 5. Nests and Sprites: allies that block (the barracks)
- A new kind of **Support** module that releases **Sprites**: small glowing seed-shaped allies (new `ally` colour: pale lavender, never coral) that fly out in the Nest's **sector**, intercept the nearest enemy up to ring II and **hold it in place** while fighting. Other modules get a longer fire window — the Kingdom Rush lesson.
- Sprites have HP, respawn after a delay, and are boosted by neighbours (Amplifier = Sprite damage, Fork = +1 Sprite, Overclock = faster respawn).
| Module | Rarity | Cost | Sprites | Behaviour | Shape |
|---|---|---|---|---|---|
| **Nest** | U | 4 | 3 × (20 HP, 3 dmg/s) | each blocks one enemy; respawn 6 s | dome with three openings |
| **Keep** | R | 6 | 1 × (120 HP, 8 dmg/s) | blocks up to 3 enemies, taunts nearby ones | tall rounded tower |
| **Queen** | L | 9 | — | every Nest and Keep +1 Sprite; Sprites gain Bloom | crowned dome |
- Counter-play: Brutes and Anchors cannot be held (they push through), Herald speeds past, Phaser slips away — so allies are strong but not universal.
- Roster becomes **39 modules**.

## 6. Rare petals and special petals
- **Extra petals are rare:** only from a Grove (once per act), the rare *Petal +1* Charm or a Mystery; max 8. Each one reshapes the ring (new neighbour pairs, new affinity room).
- **Special petals** (Balatro card enhancements): a Grove *Graft* or a Mystery can turn a petal into:
| Petal | Effect |
|---|---|
| **Golden** | the module on it earns +1 Coin per wave |
| **Deep** | the module on it reaches one ring further |
| **Twin** | counts as a neighbour of the petals two away as well |
| **Mirror** | its module also affects the opposite petal |
Visual: the petal's material changes (gold edge, deeper tint, double outline, mirrored sheen); the module on it keeps its colours.

## 7. Editions and the Pouch (the deck)
- **Editions** (Balatro's foil/holo/polychrome): rare cards may appear in an edition, from act II, about 1 card in 12. **Radiant** (effect ×1.5) · **Gilded** (+1 Coin per wave) · **Echoing** (Attack: repeats at 40%; Boost: +25% effect). Shown as a slow iridescent rim — calm, never flashing.
- **The Pouch** is the deck behind the shop (`11` §6.2's run pool, made a player tool):
  - Each Core comes with a **default Pouch** of 16 cards (e.g. Standard: balanced; Merchant: economy and Nests; Glass: Sun and Thorn).
  - After 5 wins the player can **edit** a Pouch (16 from the unlocked, at least 5 per family) and save 3 of them — Balatro's deck choice, Clash Royale's deck building.
  - During a run the Pouch changes: **Prune** (Grove) removes cards, **Plant** (Mystery, some Charms) adds a card — shops get more focused as the run goes on.
  - Daily, Weekly and leaderboards use **fixed Pouches** for everyone.

## 8. Outposts on the rings (later update)
The "map you build" beyond the petals: rings II and III get **6 sockets each** (one per sector). Rare **Outpost** cards are placed on sockets: *Watchtower* (the petal behind it reaches one ring further), *Lure* (enemies of its sector bend toward it), *Totem* (slows its sector), *Nest outpost* (Sprites start from ring II). Proposed for **Update 1**, after the core layers are proven: they add a second board and must earn their complexity.

---

## 9. Keeping it readable (the complexity budget)
| Layer | Where it lives on screen | New taps |
|---|---|---|
| Path | a vertical map between acts and after each wave (one tap) | 1 per wave |
| Charms | 4 medallions in a row under the top bar | long-press to read |
| Affinities | one line of active glyphs above the cards ("Sun 2/4 · Storm 1/2") | none |
| Sprites | in the arena, small ivory-lavender seeds | none |
| Special petals / editions | material of petal or card rim | none |
Everything new is **discovered progressively**: run 1 has no path (straight waves), Charms appear at the first Guardian, affinities after run 2, Nests and special petals from the Garden. The first run stays exactly as the FTUE in `07` §1.5.

## 10. Code impact (for the build phase)
| Area | Change |
|---|---|
| `Run` | act path generation (seeded), `ChooseNode`, Grove actions (`Graft`, `Prune`, `Petal`), Mystery choices |
| `Charms` (new) | 4 slots, `PickCharm`, `SellCharm`; hooks on hit, kill, wave end, shop, merge |
| `Modules` | affinity tag, breakpoint evaluation, editions, special petals, opposite/two-away neighbourhood hooks |
| `Allies` (new) | Sprites: spawn, intercept, block (enemy radial movement pauses), HP, respawn; deterministic like enemies |
| `Shop` | Pouch (default per Core, editable, Prune/Plant), editions in offers |
| `Replay` | new commands; format `R3` with `11`; balance `0.4.0` |
| Bots | path strategies (safe / greedy elites), Charm pickers, affinity-seeking builds |

## 11. Order (proposal)
1. Approve §2–§7 (§8 later).
2. Simulation in this order, each with tests and a bot probe: rings as range tiers → path + Groves → Charms (replacing Boons) → affinities → Nests/Sprites → special petals, editions, Pouch.
3. Design system screens: path map, Charm row and picker, affinity line, Grove, Mystery, Pouch editor.
