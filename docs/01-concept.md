# 01 — Concept (v2)

> Version 2 · 2026-09-21 · Status: decided (D17–D21). Working title; the game name is open decision D25.
>
> **In one sentence:** defend a Core at the centre of the screen from enemies coming from every side by **mounting modules on the ring around it**: neighbouring modules combine and damage grows to enormous numbers. Runs of 10–15 minutes, **no waiting, no pay-to-win**, offline. And you challenge other players' recorded runs.
>
> *Concept v1 ("musical maze defense") is in `archive/`; the reasons for the pivot are in `04` D17.*

---

## 1. Why this idea (the market gap)
- **A huge, unhappy audience:** the "defend the core, numbers go up" genre (The Tower: about $1 M/month, 7.1 M downloads) is loved, but players hate timers, pay-to-win, too many currencies, mandatory connection and hours-long runs. **Nobody offers the same satisfaction honestly.** (`00` B2)
- **The 2024–2026 hit formula** (Balatro, Ball x Pit, Backpack Battles): simple familiar base + **exploding combos** + short runs + honest model. (`00` B1)
- **Asynchronous multiplayer** (Super Auto Pets, Backpack Battles) keeps games alive without expensive servers: ideal for a solo developer. (`00` B3)

## 2. The loop
1. **Wave** (about 25–30 s): enemies come from all sides; the modules on the ring fire by themselves; you decide **when** to use the Core's ability (**Pulse**).
2. **Shop** (no time limit): with the Credits earned you buy modules, **merge duplicates** to level them up, **arrange them on the ring** to create combos, sell and reroll. Saving pays a small interest.
3. After 5 waves the act's **Guardian** arrives; **3 acts** make a run (about 10–15 minutes).
4. Between runs you **unlock new modules and new Cores**: more variety, never stats to grind.

## 3. Distinctive elements
1. **The combo ring:** 6 slots (up to 8) around the Core. An Amplifier boosts its two neighbours, an Echo repeats their shots, a Prism doubles the boosters next to it… **Position matters**: a spatial puzzle readable at a glance and shareable as a screenshot.
2. **Exploding numbers:** damage = (base + flat bonuses) × multipliers, Balatro style. From 10 damage on wave 1 to millions in the best builds, shown with **soft, calm** numbers (`03`).
3. **Merging duplicates:** two equal modules make a stronger one. Merge is the fastest-growing mechanic (+74%) and is immediately satisfying.
4. **Cores as "decks":** each Core changes the rules (economy, resistance, damage), like Balatro's decks. Huge variety without grind.
5. **Short, honest runs:** 10–15 minutes, zero timers, zero energy, works offline.
6. **Challenge other players' ghosts:** your run becomes a challenge for others (§6).

## 4. What we take from the best (without copying)
| From | What players like | Our version |
|---|---|---|
| **The Tower** | Central core, rising numbers, minimal | Same pleasure, but **short runs, no timers, no pay-to-win**, and a different visual identity (D20) |
| **Balatro** | Shop, economy, interest, jokers, decks, escalation | Shop between waves, modules as jokers, **Cores as decks** |
| **Backpack Battles** | Spatial arrangement, asynchronous PvP | The **ring** with adjacency combos, duels against ghosts |
| **Super Auto Pets** | Merging duplicates, honest asynchronous play | Module merge up to level 3 |
| **Slay the Spire** | Acts, bosses, Ascension | 3 acts + Guardians + Grades |
| **Vampire Survivors** | Auto-fire, one touch | Modules fire by themselves; you manage the build and the Pulse |
| **Arknights** | Skill counts, not money | Competitive modes with the same kit for everyone |
| **Ball x Pit / Balatro mobile** | Honest model (trial + unlock, or single price) | Honest monetization (§7 and `02`) |

## 5. What we fix (the criticism of others)
| Criticism | Our answer |
|---|---|
| Lab timers skipped by paying (The Tower) | **Zero timers**: you progress only by playing |
| Pay-to-win tournaments (The Tower, Rush Royale) | **Fixed-kit competition**: skill wins |
| Too many currencies | **Two currencies in total**: Credits (inside the run) and Blueprints (unlocks) |
| Mandatory connection | **Complete offline play**; online only for leaderboards and ghosts |
| Hours-long runs | **10–15 minutes**, autosave at every shop |
| Endless stat grind (Infinitode, The Tower) | You unlock **variety and options**, not numbers |
| Passivity ("watch and wait") | A shop full of choices, ring arrangement, **active Pulse** |
| Unfair randomness | Shop reroll, wave preview, shareable seeds |
| Intrusive ads | No forced ads, ever |

## 6. Asynchronous multiplayer (D19)
No real time, and **offline always stays complete**.

| Phase | Modes |
|---|---|
| **Launch** | **Daily Run** and **Weekly Run**: same seed and same Core for everyone, leaderboard. You can **download the ghost** of a top player and play watching their progress |
| **Update 1** | **Weekly leagues** (groups of 30, promotion and relegation) + **Duels**: same seed as a real opponent, with a skill rating |
| **Update 2** | **Siege**: build an attack wave others must survive, rewarded if they fall + **Community Guardian** with shared health |

**How it works:** every run is a **replay** of a few KB (seed + moves). Thanks to the deterministic simulation (D11) the server re-plays it to verify the score, and others can watch it as a ghost. Updates 1 and 2 are built **only if** the gates in `10` §2.4 open.

## 7. Monetization (summary; details in `02` and `08`)
- Fixed principles: **never pay-to-win**, no energy, no timers, no forced ads, no paid loot boxes.
- **Model decided before soft launch, with data:** (a) **free trial + one-time unlock** of the full game (the Ball x Pit model), or (b) **fair free-to-play** (optional ads + a pass that speeds up unlocks + ad-free VIP).
- In both cases competitive modes stay **fixed-kit**.

## 8. Anti-copy rules (D20)
Mechanics are free, look and names are not. **Different visual identity** from The Tower, **different interface** (shop + ring, not stat sheets), **original names**, **never their name** in the store or in ads. Legal review before launch.

## 9. Strengths
1. **Proven demand** (The Tower) **+ clear criticism** to solve = a sharp position: "the good part of the genre, without the hated parts".
2. **Modern formula** (shop + combos + merge + numbers) that works on mobile.
3. **Readable in portrait:** Core in the centre, ring, enemies all around.
4. **Feasible solo:** simple shapes, no hand-made levels, multiplayer without real-time servers.
5. **Replayable:** Cores × modules × arrangements × seeds.
6. **Natural marketing:** "look at this build", "beat my ghost", ring screenshots.

## 10. Risks and mitigations
| Risk | Mitigation |
|---|---|
| Looking like a copy of The Tower | D20 + different mechanics (shop, ring, merge, short runs) + different style |
| Too complex for mobile | Few modules at first, gradual unlocks, icon-based UI, a played tutorial |
| Unreadable huge numbers | Compact notation (1.2K · 3.4M · 5.6B), numbers only for important hits, calm style |
| "Broken" builds that trivialize the game | Automatic balance simulator (bot) + Grades + per-season versions |
| Leaderboard cheating | Server-side replay verification (D19, `10` §1.3) |
| Too few players for multiplayer | Asynchronous play works with few players; bot-generated ghosts at first |

## 11. First playable version (MVP) `(estimate)`
- 1 Core, 1 act (5 waves + Guardian), the 14 MVP modules (weapons, boosters, economy; `05` §6), shop with merge, Pulse, the 6 enemies + Guardian (`05` §8). *The current prototype has 7 modules and 3 enemies + Guardian.*
- Local replay recording and verification; Daily Run with the day's seed (local leaderboard for now).
- **Timeline with AI assistance** `(estimate)`, detailed month by month in `10` §4:
  | Milestone | Time |
  |---|---|
  | Simple-shape prototype | 2–3 weeks |
  | Vertical slice with final art and audio | about 3 months |
  | Soft launch | about 6 months |

**Golden rule:** if the shop with the ring is not fun with simple shapes, we do not move on to art.
