# 08 — Economy, progression and non-pay-to-win monetization (Phase 2/4 brief)

> Version 1 · 2026-09-21 · Status: brief. The economic specification implemented in Phase 2 (`06`) and completed in Phase 4 (meta) and Phase 6 (soft launch).
> Fixed principle (`02`): *"you pay to get there sooner or for convenience, never to win"*. **PROPOSAL** items are listed in `04` §Open decisions.
> All numbers are **v0, to validate with the bot** (`06` Phase 2) and with the meta simulator (§1.4).

---

## 0. The two economies (and the wall between them)
| | **Inside the run** | **Outside the run (meta)** |
|---|---|---|
| Currency | **Credits** | **Blueprints** |
| Buys | modules, merges, rerolls, extra slot | **variety**: modules in the pool, Core types, themes |
| Grows | exponentially, for 10–15 minutes | linearly, for 60–80 runs, then **ends** |
| Resets | every run | never |
| Touches power | yes, it is the game | **no, never** |
| Purchasable with money | **never** | only faster (model B) |

**The wall:** no meta item changes a number in the simulation. This is what allows (a) same-seed leaderboards, (b) verifiable replays, (c) a free player competing at the top level from day 1 in competitive modes (fixed pool).

---

## 1. The "upgrade tree", our way

### 1.1 In-run upgrades: module levels
A module levels up **only** by buying a duplicate (merge), up to L3. There is no "upgrade" button.

**Yield per Credit (Emitter, cost 3, damage 8):**
| Level | Credits invested | Damage per hit | Damage per Credit | Slots used |
|---|---|---|---|---|
| L1 | 3 | 8.0 | 2.67 | 1 |
| L2 (×1.8) | 6 | 14.4 | 2.40 | 1 |
| L3 (×3.0) | 9 | 24.0 | 2.67 | 1 |
| *three separate L1 Emitters* | 9 | 24.0 (in 3 slots) | 2.67 | **3** |

The logic: merging is **not more efficient per Credit** than three separate modules — it is more efficient **per slot**. The value of a merge is ring space (the scarce resource), and the fact that one L3 module receives the boosters of its two neighbours: 24 × boosters, instead of 8 × boosters three times with only 6 slots. Exponential growth comes **from multiplication between neighbours**, not from levels. This prevents inflation: levels are roughly linear, multipliers stack only if the player builds well.

**Boosters (Amplifier ×1.5):** L2 → ×1.8 · L3 → ×2.1. Two L3 Amplifiers around one weapon: ×4.41. That is the ceiling 6 slots allow with one weapon (weapon + 2 boosters + 3 slots for economy/other weapons).

### 1.2 Credit flow in a run (estimate, Standard Core)
| Source | Per wave | Over 18 waves |
|---|---|---|
| Base | 4 | 72 |
| Interest (1 per 5, max 5) | 0–5, average 2–3 | ~45 |
| Guardian | +3 × 3 | 9 |
| Bank (if owned, L1) | +1 | ~10 |
| Starting | — | 6 |
| **Total** | | **~130–145** |

| Typical spending | Credits |
|---|---|
| 6 modules at L1 (average cost 3.7) | ~22 |
| Taking 4 of them to L3 (8 merges) | ~30 |
| Extra slot ×2 | 16 |
| Rerolls (average 1.5 per shop × 17 shops, rising cost) | ~40 |
| Sales/pivots | −10 (refund) |
| **Total** | **~100–110** |

The margin (~30 Credits) is the room to **choose**: save for interest, reroll more, or pivot mid-run. If the bot shows a margin > 50 the game is too generous (choices do not weigh); if < 10 it is stingy (frustration). **Levers:** base Credits per wave, interest cap, reroll cost.

### 1.3 Out-of-run upgrades: the Archive (variety, not power)
The Archive is a tree with **four branches**. Every node adds something to the game; **no node changes a number**.

| Branch | Nodes | What it adds | Example |
|---|---|---|---|
| **Weapons** | 10 | a weapon in the shop pool | Arc, Lance, Mortar, then Prism, Singularity… |
| **Boosters** | 8 | a booster in the pool | Echo, then Resonator, Mirror… |
| **Economy and utility** | 8 | an economy or defensive module | Salvage, Frost, Capacitor, Harvester… |
| **Cores** | 3 (+ themes) | a Core type (different rules) | Merchant, Bastion, Glass |
| *(free, no Blueprints)* | Grades 1–10 | difficulty | unlocked by winning |

**A tree, not a list:** each branch has 2–3 nodes available at a time; unlocking one opens others. The player **chooses** what to unlock (no randomness in unlocks: randomness lives in the shop, not in the meta). "Sidegrade" nodes (Core types) are our version of "passive abilities": they change rules, not stats.

### 1.4 Blueprint maths (anti-inflation by construction)
**Income:**
| Event | Blueprints |
|---|---|
| Wave cleared | 1 |
| Guardian defeated | 3 |
| First win of the day (any mode) | +5 (**PROPOSAL**) |
| Daily Run completed (even if lost) | +3 (**PROPOSAL**) |
| Grade (per level, wins only) | +1 × Grade |

Average run of a new player (loses at wave 10): ~11. Winning run: 27. **Steady-state average: ~15 per run, ~30 per day** with 2 runs + Daily.

**Costs (linear per branch, never exponential):**
| Node in branch | 1st | 2nd | 3rd | 4th | 5th… | last |
|---|---|---|---|---|---|---|
| Weapons / Boosters / Economy | 10 | 15 | 20 | 25 | +5 | 55 |
| Core type | 40 | 60 | 80 | — | — | — |
| Cosmetic themes (optional) | 20 | 20 | 20 | … | | |

**Total gameplay Archive** (26 modules + 3 Cores): ~ 26 × 30 (average) + 180 ≈ **960 Blueprints** → **~65 runs** at 15 average, **~32 days** at 30 per day. Consistent with the 60–80 run target (`05` §11). Themes are outside the count: they are the "sink" for those who finished.

**Why linear:** with exponential costs (typical of the genre) the last node costs as much as all the others together → grind wall. With linear costs the **cadence** of unlocks is predictable: one every 1–2 runs early, one every 3–4 near the end. The pace slows, it never stops.

**Meta simulator** (to build in Phase 2 with the bot): N synthetic players × sessions per day × win probability per Grade → "days to unlock everything" curve. Target: median 30–45 days for daily players, 60–80 runs in total.

### 1.5 What we do NOT do (and what it would cost)
| Typical genre request | Why not | What would happen |
|---|---|---|
| Permanent base stats (+damage, +HP) | break fixed-kit competition and turn every run into "am I upgraded enough?" | leaderboards dominated by time played, not skill; the main criticism of The Tower and Infinitode |
| Permanent passive abilities | the same, disguised | new players lose to the meta, not to the game |
| Exponential unlock costs | guaranteed grind wall | D30 drops when the player "sees" the wall |

---

## 2. "Pay to progress": what makes sense to monetize

### 2.1 Model A — trial + unlock (primary choice to test)
- **Free:** complete act 1, Daily Run, leaderboard, ghosts. No ads.
- **One-time unlock (€4.99–7.99):** acts 2–3, full Archive, Weekly, Endless, Core types, Grades.
- **Optional:** cosmetic theme packs (€1.99–2.99) and a "Supporter pack" (€4.99: exclusive theme + leaderboard title). Nothing else.
- Trial players compete in the Daily **on equal terms** (fixed pool). It is the Ball x Pit / Balatro mobile model: "no tricks" positioning.

### 2.2 Model B — fair free-to-play
The whole game free. Monetize **only** meta speed and cosmetics:

| What | Price | Effect | Touches competition? |
|---|---|---|---|
| **Rewarded ads** (optional, ~8/day) | free | Blueprints ×2 at run end · Second Chance (1/run, **not in competitive modes**) · Free Reroll (1/shop, **not in competitive modes**) · Daily Crate | no |
| **Season Pass** (§3) | €4.99 / 30 days, no renewal | Blueprints ×2 + cosmetics | no |
| **Lifetime VIP** | €7.99 | no ads; rewarded-ad rewards granted automatically (same limits) | no |
| **Blueprint packs** | €1.99 = 60 · €4.99 = 180 · €9.99 = 400 | speed up the Archive | no (the Archive is not power) |
| **Cosmetic themes** | €1.99–2.99 | Core, ring, Pulse wave, number style, victory animation | no |

**Pack cap (PROPOSAL):** the Archive is finite (~960 BP): someone buying everything completes it in ~5 runs instead of ~65. The "advantage" is **variety sooner**, and it vanishes when the free player reaches the end. To avoid "I bought the game and I am done", packs are limited to **400 BP per 7 days** per account: paying shortens, it does not skip.

### 2.3 What is **not** sold in any model (fixed rules, `02` §1)
| Typical request | Why not | Honest alternative |
|---|---|---|
| **Time-skip** | there are no timers | — |
| **Paid extra slot** | it is in-run power (8th slot = +1 booster = ×1.5) → pay-to-win in the Daily | the extra slot is bought **with Credits**, inside the run, for 8 Credits |
| **Temporary damage or Credit multipliers** | the same; and The Tower is hated for this | a **Blueprint** multiplier (meta) via pass or ads |
| Energy, random boxes, paid rerolls | break respect for time, honesty, decision-making | — |

### 2.4 The guarantee for the skilled free player
1. **Daily, Weekly, Leagues, Duel use a fixed pool and Core:** unlocks do not count, skill does. A day-1 player can win the day-1 Daily.
2. **Endless and per-Grade leaderboards** are split by **balance version**, not by "account level".
3. **Second Chance and Free Reroll do not exist in competitive modes:** no ad changes a run there.
4. The Archive **ends:** after ~65 runs everyone has everything. There is no "paid endgame".
5. **Replay verification** on the server: cheaters do not appear on leaderboards (D19, `10` §1.3).

The maximum gap between free and paying is therefore: *"less variety in free runs for a month"*. No gap in competition, ever.

---

## 3. Two-track Season Pass

**Conditions to launch it** (`02` §3): model B chosen, D30 ≥ 7%, positive test. Not in the game at launch.

### 3.1 Structure
| Parameter | Value | Reason |
|---|---|---|
| Length | **30 days**, no auto-renewal | one season = one balance cycle and one theme |
| Premium price | €4.99 | below the "let me think" threshold |
| Tiers | **30** | one per day for daily players |
| Points per tier | 100 | readable numbers |
| Points from | wave cleared **5** · Guardian **15** · Daily completed **60** · daily mission **30** (×3) · weekly mission **150** | a winning run ≈ 135; an average day (2 runs + Daily + missions) ≈ **330 = 3.3 tiers** |
| Time to complete | ~10 days playing daily; ~20 on alternate days | never a daily obligation; **retention comes from the Daily**, not from pass anxiety |
| Retroactive purchase | yes, any time in the season, with rewards already earned | zero "I lost the rewards" |
| Expiry | points do not expire; the pass does; no tier can be bought with money | no "pay to finish the pass" |

### 3.2 Rewards (draft)
| Tier | **Free** track | **Premium** track (in addition) |
|---|---|---|
| 1 | 10 Blueprints | seasonal Core theme (immediately: the "wow" on purchase) |
| 5 | 15 BP | Blueprints ×2 for the whole season (from purchase) |
| 10 | "season" number style | 40 BP |
| 15 | 20 BP | seasonal Pulse wave |
| 20 | leaderboard title | 60 BP |
| 25 | 25 BP | ring theme |
| 30 | **"season completed" frame** | seasonal victory animation + 100 BP |
| every other tier | 5 BP | 10 BP |

Total Blueprints: Free ~200 (≈ 13 runs), Premium ~600 more. **No exclusive module** in either track: variety is unlocked in the Archive, the same for everyone; the pass speeds it up and dresses the game.
**Theme rule:** never the enemy hue on Core and modules (`03` A4); every theme passes the readability test.

### 3.3 Daily return (without punishment)
- **Daily Run:** the real reason (same seed for everyone, leaderboard, ghosts). +3 BP even if lost.
- **3 daily missions**, simple and always completable in 2 runs ("win an act with 2 Amplifiers", "use the Pulse 5 times", "sell a module"). Renew at midnight UTC; **they do not accumulate and are not lost**.
- **1 weekly mission** ("complete 3 Dailies", "win at Grade 2").
- **Streaks:** count only upwards (badges at 7/30/100 days); skipping a day resets nothing tangible (**PROPOSAL:** the streak shows "days played this month", not a chain that breaks).

---

## 4. Anti-frustration systems

### 4.1 Against the grind wall (meta)
| Mechanic | How | Effect |
|---|---|---|
| **Linear costs** (§1.4) | +5 per node | predictable cadence, never a wall |
| **Blueprints even when losing** | 1 per wave cleared | every run progresses, even the worst |
| **Unlock by choice** | the Archive is a tree with 2–3 open options | the player unlocks what they want to try |
| **"Next unlock in N"** at run end | always visible | progress is seen, not guessed |
| **The Archive ends** | ~65 runs, then only Grades, Daily and themes | no infinite grind: the game says so ("Archive complete") |
| **No second grind system** (Ball x Pit lesson) | one tree only | — |

### 4.2 Against bad luck (inside the run)
| Mechanic | How | Note |
|---|---|---|
| **Shop bad-luck protection** (**PROPOSAL**) | if 2 consecutive shops show no booster, the third guarantees one; same for weapons | a **deterministic** rule in `GenerateOffers` (from the seed, replay-safe): does not change the Daily between players |
| **First offer of acts 2 and 3** (**PROPOSAL**) | always contains at least one rare module | rares really exist, not only on paper |
| Reroll, wave preview, undo | already in the GDD | randomness is managed, not suffered |
| **Replay this seed** from defeat | free, not ranked | turns "bad luck" into "let me try better" |

### 4.3 Against "too hard" / "too easy"
- **Grades 1–10** for those who find it easy (free, `07` §2.2).
- **Calm Mode** (**PROPOSAL**): an accessibility option (enemies −20% HP, Pulse cooldown −25%), available from the start, **outside leaderboards** and with an icon on the summary. An option, not a purchase: accessibility is not sold.
- **Contextual tip after 3 defeats in the same act** (`07` §2.4): one, targeted, never a hidden nerf.

### 4.4 Against monetization frustration
- No unrequested ads; the rewarded-ad button says **beforehand** what it gives.
- The purchase shop **does not appear** in the first 3 days and never during a run.
- No "almost there" (bars at 95% asking for money), no discount countdowns, no limited-time offers (**fixed rule**).
- Everything purchasable can be **previewed in the game** (theme applied to the Core for 10 seconds).

### 4.5 Metrics to watch (Phases 5–6)
| Metric | Problem signal |
|---|---|
| Runs between one unlock and the next | > 5 in the first week |
| Blueprints per hour of play | falling after the first week (curve too steep) |
| % of runs abandoned in the shop | > 10% (the shop bores or blocks) |
| Use of Second Chance / Free Reroll | > 60% of runs (too punishing, or rewards too generous) |
| Grade 0 win rate after 20 runs | < 40% (too hard) or > 90% (too easy) |
| Revenue per install, A vs B | decides the model (`02` §2) |

---

## 5. What enters which phase
- **Phase 2:** Credit flow (§1.2) and tuning levers with the bot; meta simulator (§1.4); bad-luck protection and first-offer rare (§4.2) if confirmed; extra slot with Credits.
- **Phase 4:** Archive tree (§1.3), Blueprints (§1.4), missions, "next unlock in N", Calm Mode.
- **Phase 6:** models A and B behind a flag, capped packs, themes; Season Pass **only** after soft launch and with D30 ≥ 7%.

**Decisions to confirm:** +5 BP first win of the day and +3 Daily · pack cap (400 BP / 7 days) · shop bad-luck protection · guaranteed rares at act start · Calm Mode · "upwards only" streaks.
