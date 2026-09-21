# 10 — Competition, modes, launch and the 6-month roadmap (Phase 4–7 brief)

> Version 1 · 2026-09-21 · Status: brief; §4 is the **operational calendar** that the `06` phases follow.
> Constraints that do not move: complete offline play, deterministic replays (D11, D19), fixed kit in competition, UGS backend (D21), no "The Tower" anywhere (D20). **PROPOSAL** items are listed in `04` §Open decisions.

---

## 1. Daily Challenge and leaderboards

### 1.1 The Daily in one table
| Parameter | Value | Why |
|---|---|---|
| When | a new seed every day at **00:00 UTC**; the run can be started until 23:59 UTC and finished later | one worldwide boundary, decided by the server (`05` §12) |
| Seed | `SHA-256(server_salt ∥ date)` → first 64 bits. **The salt is published by the server every day**, it is not in the app | without the salt the app could compute future seeds and practise offline; with it only today's seed is known |
| Offline | the seed is downloaded at the first online moment of the day (a few bytes, cached); a player offline **all day** plays a "local Daily" with a date-derived seed, valid for Blueprints but **unranked** | offline stays complete; a leaderboard exists only online by definition |
| Kit | Standard Core, **fixed pool** (the 14 MVP modules, later rotating per season), Grade 0, no Archive, ad or Pass bonus | equal conditions for everyone (D19) |
| Attempts | **one**. The run saves at every shop: you can close and resume within the window | "one run" is the heart of the format (Spelunky, Slay the Spire) |
| After the Daily | "Replay this seed" unlimited, **unranked**, marked `practice` in the replay | turns frustration into practice without polluting the leaderboard |
| Score | waves cleared (Endless included: if you win, you continue) → total damage → **fewer ticks** | the three keys of the GDD; time is simulated time, so 3x speed does not count |
| Rewards | +3 Blueprints for playing it, +2 if in the top half *(PROPOSAL)*; **no power reward** | the reward is the leaderboard |
| Ghosts | download a player's replay (top 10, friends, or "someone near you") and play the same seed seeing their wave and DPS live in a corner | the cheapest "multiplayer" there is: a file of a few KB |

**Weekly** (`05` §12): same scheme, weekly seed (Monday 00:00 UTC), with **one mutator** (§2.2) and up to **3 attempts** *(PROPOSAL)*: the best counts. It is the Daily for people who cannot play every day.

### 1.2 Leaderboards: structure
| Level | Who sees what | Notes |
|---|---|---|
| **Global** | everyone, top 100 + your position ± 5 | one per Daily, Weekly, Endless (per Grade) |
| **Country** | top 100 of your country | from the account country (UGS), not GPS |
| **Friends** | people added by code (or Play Games, optional) | the leaderboard that really matters for retention |
| **Tiers** | Diamond top 1% · Gold 10% · Silver 25% · Bronze 50% · Iron the rest | computed at Daily close; a badge on the summary, nothing else |
| **Leagues** (update 1) | groups of 30 with the same seed; weekly promotion/relegation | see §2.4: **only if** the community signals ask for it |

Every leaderboard is split by **balance version** (`05` §13): when numbers change, a new season starts. Seasons are long (6–10 weeks) and announced in-game with one line.

### 1.3 Anti-cheat: why cheaters and spenders do not dominate
| Threat | Defense | When |
|---|---|---|
| Fabricated score (memory edit, fake request) | the client **does not send a score**: it sends the **replay**. The server (Cloud Code C#, same `Simulation` assembly) **re-plays** it and computes the score itself. Different hash = silently discarded | Phases 4–5 |
| Replay built by an external bot ("tool-assisted") | the only attack verification does not stop. Mitigations: (a) the replay carries the **ticks** of commands, and a "superhuman" pattern (tick-perfect commands, shops solved in 0 ticks) is detected statistically; (b) the top-100 replays are **public**: the community watches them; (c) one-tap report from a ghost; (d) manual review of the top 10 in the first weeks | Phase 5 |
| Multiple accounts / attempts | anonymous UGS account bound to the install + optional Play Games; **the first valid replay of the day counts**, later ones are rejected; reinstalling to retry is possible but the new account starts from zero (no Archive, no history) | Phase 5 |
| Modified app | the replay carries the game and balance versions; the server accepts only known signed versions; Play Integrity API as a second check *(PROPOSAL, only if evidence shows it is needed)* | Phase 5 |
| Spenders | **there is nothing to buy that enters the Daily**: fixed pool, no Second Chance, no Free Reroll, no multipliers (`08` §2.4) | by construction |
| Fully unlocked accounts | the Archive does not count in competition: the pool is fixed and equal for everyone | by construction |
| Replay flooding | limits: 1 valid submission per Daily, 3 per Weekly, 10 per day for Endless; maximum size 64 KB | Phase 5 |

**Sanctions:** no "loud" bans. An invalid replay simply does not appear. An account with 3 invalid replays goes on a **shadow list**: it sees its own position, others do not. It leaves the list after 14 clean days. Shadow lists are reviewed by hand while numbers are small.

### 1.4 What the server needs (UGS) and what it costs
| Service | Use | Free tier (to verify in month 4, `04` D21) |
|---|---|---|
| Authentication | anonymous accounts, Play Games linking | free |
| Cloud Code (C#) | `SubmitReplay` (verification and score), `GetDailySeed`, `GetGhost` | metered; one verification = one re-simulation of about 1–3 s CPU |
| Leaderboards | Daily, Weekly, Endless × Grade, by country and friends | metered |
| Cloud Save | Archive, options, run in progress (local is the truth, `08`) | metered |
| Remote Config | daily salt, active balance version, weekly mutator, switches (Daily on/off, modes) | free |
| Cloud Storage (or a bucket) | top-100 replays (a few KB each) | negligible |
Estimate *(to confirm with the UGS calculator)*: below 10k daily active players we stay within or near the free tier. **Switch:** if costs exceed the budget, verification becomes "sampled" (top 100 always, the rest at 10%) without changing the client.

---

## 2. Game modes

### 2.1 Modes at release (all offline, all replays)
| Mode | What changes | Length | Unlocks |
|---|---|---|---|
| **Run** | the base game: Core, Grade, Archive | 10–15 min | always |
| **Daily / Weekly** | §1 | 10–15 min | run 2 / first win |
| **Endless** | after the win: growth continues; every 6 waves a stronger Guardian | as long as you last | first win |
| **Guardian Gauntlet** *(PROPOSAL, the "boss rush")* | **6 Guardians in a row**, no normal waves; between them a shop with **10 fixed Credits** (no interest); every Guardian adds a rule (shield, double escort, summons elites…). Fixed kit, weekly seed, leaderboard | **4–6 min** | first win |
| **Surge** *(PROPOSAL, the "modified endless")* | Endless where **every 3 waves a random seeded mutator** (§2.2) arrives, cumulative. The player sees the next mutator in the shop, like the wave preview | as long as you last | 5 wins |

Why these two: the **Gauntlet** answers "I have 5 minutes" and tests the build against the pure exam; **Surge** answers "Endless gets samey after a while" (the Vampire Survivors criticism) without touching base balance. Both reuse everything: no new content, only rules.

### 2.2 Mutators (for Weekly and Surge, from the seed)
Twelve one-line readable rules, stackable in Surge:
1. 5-slot ring · 2. Boosters only in the shop for 3 shops · 3. Enemies +50% speed, −30% HP · 4. The Pulse recharges in 10 s but deals half damage · 5. Double interest, base Credits halved · 6. Every wave is themed · 7. Elites from wave 1 · 8. Reroll is free, but offers are 3 · 9. Sold modules refund everything, but no undo · 10. Guardians summon twice as much · 11. Armor +2 for everyone · 12. Merge up to L4 (×4.5) *(to tune)*.
Every mutator is a **flag in `RunConfig`**, hence in the replay, hence verifiable.

### 2.3 Voluntary "handicap" modes
The **Grade** (`07` §2.2) *is* the handicap challenge: ten cumulative rules chosen by the player. No separate mode needed. Endless leaderboards exist per Grade.

### 2.4 The open door to multiplayer: three levels and their gates
Multiplayer is **asynchronous only** (D19) and is added **only if the signals ask for it**. The Daily with ghosts is already "multiplayer" in the sense that matters; the rest is optional.

| Level | What | Gate to build it (all three) | Cost |
|---|---|---|---|
| **0 — at release** | Daily/Weekly, leaderboards, ghosts, friends by code | — | already planned |
| **1 — Leagues and Duel** | groups of 30 with the same seed, promotions; Duel = same seed as a real opponent with a rating | D30 ≥ 5% · ≥ 25% of active players play the Daily · ≥ 3,000 daily active players *(to fill leagues without bots)* · explicit demand in surveys/Discord (≥ 30% "I want leagues") | 4–6 weeks |
| **2 — Siege and Community Guardian** | player-built attack waves; a boss with shared health | Leagues active for 8 weeks with ≥ 40% participation · UGS costs within budget · cheat reports < 1% | 6–8 weeks |

If a gate does not open, the mode **is not built** and the time goes to content and Grades. This is written down because the temptation to "add multiplayer" is the fastest way to never ship.

---

## 3. Launch strategy and initial marketing (low cost)

### 3.1 The principle
The game produces its own material: **every run is a clip** (rising numbers), **every ring is an image**, **every replay is a challenge**. Marketing consists of getting these out of the game in one tap and showing them consistently. Money budget: **€0 until soft launch**, €300–1,000 of Google Ads at soft launch (`02` §5).

### 3.2 Channels, in order of expected return per cost
| Channel | What | Cadence | From |
|---|---|---|---|
| **Short videos** (TikTok, Shorts, Reels) | 10–20 s clips **from the game**: a combo lighting up, a Guardian melting, "10 → 10M". No voice, a 4-word caption. Same format as the store video (§3.5) | 2 per week | month 2 (as soon as the art holds up) |
| **Reddit** | honest devlogs with GIFs: r/TowerDefense, r/roguelikes, r/incremental_games, r/AndroidGaming, r/iosgaming, r/IndieDev, r/Unity3D; **ask for feedback, do not advertise** | 1 per month per subreddit, never the same post in two | month 2 |
| **Playable web demo** *(PROPOSAL)* | act 1 in **WebGL on itch.io** (the module is installed; the simulation is pure C#): "try it in 5 minutes, then pre-register". The cheapest way to turn a GIF into a player | one, updated at every gate | month 3 |
| **Google Play pre-registration** | live with the video; pre-registration reward: **a cosmetic theme** (never power); public milestones (1k, 5k, 10k) | from month 4 | month 4 |
| **Discord** | the tester channel (12 are needed for the closed test, D13), then the community; polls for the §2.4 gates | continuous | month 2 |
| **Small creators** (5–50k subscribers, TD/roguelite/incremental) | codes and an early build, no payment; a press kit (icon, 6 screenshots, video, 3 lines, no other games' names) | 20 contacts in month 5 | month 5 |
| **Google programmes** | Indie Games Festival / Accelerator when applications open; Play Pass to evaluate for model A | when open | month 4+ |
| **ASO** | title + subtitle with the keywords (§3.5), listing experiments (3 icons, 2 screenshot sets) | from soft launch | month 6 |

### 3.3 What we do **not** do
Misleading ads or "fake gameplay"; comparisons with other games (D20); paid influencers before having data; cross-subreddit spam; "coming soon" announcements without a date.

### 3.4 Targets `(estimate, to revise with the first data)`
| Moment | Target |
|---|---|
| End of month 3 (web demo) | 500 demo plays; 100 Discord members |
| End of month 5 | 3,000–5,000 pre-registrations; 12+ active testers |
| Soft launch | D1 ≥ 35%, D7 ≥ 12% (`05` §17); CPI < €1 on the best organic videos |
| Release | 10k downloads in the first month *(low end of the `00` target)* |

### 3.5 Store listing and first impression
**Icon:** **one subject**: the Core with its petals, seen from the game camera, on the dusk gradient, with **one** golden combo line. No text, no enemies, no range circle. Legible at **48 px**. Three variants for the listing A/B test (Play Console → *Store listing experiments*): a) Core alone; b) Core + one golden module; c) Core + three coral shards approaching (more "game", less "calm").

**Screenshots** (portrait 9:16; the first 3 are the ones that count):
| # | Scene | Caption (≤ 4 words) |
|---|---|---|
| 1 | mid-wave, a big golden "24.6K", combo lines lit, a swarm melting | *"Numbers explode"* |
| 2 | shop with a card being dragged, preview "16 → 24 (+50%)" | *"Position matters"* |
| 3 | merge in progress, "Level 2" | *"Merge and grow"* |
| 4 | three different rings side by side | *"Every run, a build"* |
| 5 | Daily leaderboard with a ghost | *"Challenge their replays"* |
| 6 | calm screen with three lines | *"Offline · No timers · Never pay-to-win"* |
Never other games' names, never fake UI: **real gameplay only** (`02` §5), the same build the player downloads.

**Video (15–30 s, no voice):** first 3 seconds: a number growing from 10 to 10M with the ring filling in time-lapse. Then: drag → golden line → bigger numbers → Pulse → a Guardian melting → the final ring. Closing: the three lines of screenshot 6. The same video is the devlog format.

**Listing text (draft):** short description *"Defend the Core. Combine the modules. Numbers that explode. Offline, no timers."* ASO keywords: tower defense, roguelike, offline, no pay to win, idle defense, merge, strategy (never other games' names, D20).

**In-app first 10 seconds and funnel:** `07` §3. **Pre-launch:** pre-registration with the video; devlogs in the same format (build in public, `02` §5); the ring **shareable from the game** in one tap (9:16 image with seed and replay code): players do the marketing; the **Daily** visible on the Home ("Today's Daily: 1,284 players") is the hook to come back tomorrow.

---

## 4. Six-month roadmap (October 2026 → March 2027)

### 4.0 The three choices that make 6 months possible
1. **Balancing is continuous, not a month.** The bot (`06` Phase 1) runs on every change; month 5 is the *final tuning with tester data*, not the first.
2. **Art enters from month 2, in parallel with content**, because Gate 1 (tactile, simple shapes) closes at the end of month 1. If Gate 1 slips, everything slips: it is risk number one and it must be said.
3. **Outside the release:** Season Pass, Leagues, Duel, Siege, iOS, languages beyond EN/IT (+ ES/PT-BR/DE/FR for the store only). All are "open doors" with a gate (§2.4, `08` §3).

Each month = a `06` phase, a **gate** and an "if it fails" line. Work inside a month runs in parallel; the order is logical, not rigid.

### Month 1 — October 2026 · Core mechanics and tactile prototype (= Phase 1)
- ✅ Bot runner + CSV (2026-09-21) · ScriptableObject `ContentLoader`; `BuySlot`; compact notation.
- UI Toolkit instead of IMGUI; dragging with magnet and preview; Undo and Sell; animated merge; wave-end summary; basic options; haptics.
- First Android development build on the phone.
- **Start the Google Play account procedure** (long lead time, D13).
- **Gate 1** (`06`): 3–5 testers, 3/5 ask to play again, neighbourhood understood by the second shop.
- *If it fails:* stay in month 1 (neighbourhood rules, slots, feedback). Art does not open.

### Month 2 — November 2026 · UI and art integration (= Phase 3, first half) + content (= Phase 2, first half)
- **Mood shot** approved by week 1 (`03` A9): the gate of art.
- Blender via MCP: Core, the 7 existing modules, 4 enemies; URP with light and post; 35° camera; quality levels.
- Final UI for wave, shop, run end, pause (`09` §2.2–2.4, §2.8); tokens in `Theme.uss`.
- In parallel, simulation: Arc, Lance, Mortar, Echo, Salvage, Frost, Capacitor (14 modules); Dasher, Splitter, Warden; elites; acts 2–3; Guardians with varied escorts.
- First short videos and Discord.
- **Gate 2a:** 30 fps on the reference phone with 60 enemies; 5 testers say "it does not look like a prototype".
- *If it fails:* cut shadows and bloom from the base level; missing module art uses coloured simple shapes until there is time.

### Month 3 — December 2026 · Progression (= Phase 2, second half + Phase 4, first half)
- Core types (4), Grades 1–3, Endless, save and resume at every shop, replay `R2`.
- Archive tree and Blueprints (`08` §1.3–1.4); "next unlock in N"; Archive screen (`09` §2.5).
- Complete FTUE (`07` §1.5) and progressive disclosure (`09` §3.2); Home (`09` §2.1).
- Balance v0.3 with the bot (targets in `06` Phase 2); meta simulator.
- **WebGL demo of act 1 on itch.io** *(PROPOSAL)*.
- **Gate 2b + 4:** full run 10–15 min; 3 different winning builds; a new tester reaches the third run with an unlock unaided.
- *If it fails:* fewer modules at release (12 instead of 14), Grades 1–2 only; the Archive stays.

### Month 4 — January 2027 · LiveOps and leaderboard backend (= Phase 4, second half + Phase 5, first half)
- Daily/Weekly with the server seed and the "local Daily"; shareable replay code; ghosts; Guardian Gauntlet and mutators (Surge if there is time, otherwise update 1).
- UGS: Authentication, Cloud Code `SubmitReplay` (re-simulation), Leaderboards (global, country, friends), Cloud Save, Remote Config; cost estimate written in `04`.
- `Services` interfaces with real implementations behind the fakes; UMP consent; analytics and crash reporting (D24); privacy policy.
- Pre-registration open with the video; public top replays.
- **Gate 5a:** a replay sent from a phone is verified on the server and appears on the leaderboard; a tampered one does not; estimated costs within budget.
- *If it fails (costs):* sampled verification (§1.4); *(server):* the Daily launches with a **local** leaderboard and the global one arrives with update 1 — the game stays complete.

### Month 5 — February 2027 · Balancing and economy with testers (= Phase 5, second half)
- **Google Play closed test:** 12+ testers × 14 days (mandatory for a personal account). Weekly cycle data → bot → numbers → build.
- Final tuning: acts 2–3 curve, Guardians, Credit economy (`08` §1.2), Blueprints (`08` §1.4), Grades.
- Both monetization models behind a flag (`08` §2); cosmetic themes (3); IARC; AI declaration; **one hour with the IP lawyer** (D20); final name (D25).
- Store localization in ES/PT-BR/DE/FR; the game in EN/IT.
- Creators: 20 contacts with codes.
- **Gate 5b:** 14 days of testing completed; crash-free ≥ 99%; tester D1 ≥ 35%; no bot strategy above 85%.
- *If it fails (retention):* extend the test by 2 weeks and delay the soft launch: **spend the time here, not later**.

### Month 6 — March 2027 · QA, soft launch and release (= Phase 6 compressed + Phase 7)
- Weeks 1–3: **soft launch** in two similar countries (one per model, or A/B); €300–1,000 of Google Ads on the best organic videos; listing experiments (icons, screenshots).
- QA: a 6-phone matrix (2 low-end, 2 mid, 2 high), Android 8 → 16; resume tests (closing during a wave, battery, calls); network tests (fully offline, slow network, network change mid-submission).
- Week 4: **model decision** (revenue per install + rating) and **release** on Google Play, or a 2–4 week delay with a written plan.
- **Gate 6:** `05` §17 KPIs reached or a corrective plan.
- *Afterwards:* update 1 (+8–12 weeks) with whatever the gates opened (§2.4), Surge if deferred, the Pass if D30 ≥ 7%; iOS when there is a Mac or Build Automation.

### 4.1 Overview
| Month | `06` phase | Gate | Marketing |
|---|---|---|---|
| 1 Oct | 1 | tactile | Play account started |
| 2 Nov | 3a + 2a | art and performance | videos, Discord |
| 3 Dec | 2b + 4a | full run, progression | web demo |
| 4 Jan | 4b + 5a | verified leaderboards | pre-registration |
| 5 Feb | 5b | closed test, retention | creators |
| 6 Mar | 6 + 7 | soft launch → release | ads, ASO |

### 4.2 Risks of the compression (check every month)
| Risk | Signal | Decision already taken |
|---|---|---|
| Gate 1 slips | end of October without 3/5 "play again" | everything slips together: the gate is **not** skipped |
| Art and content together in month 2 saturate one person | end of November with 10 modules instead of 14 | release with 12 modules; the missing 2 in update 1 |
| Google Play account late | procedure opened after month 2 | the closed test can start on an internal link channel, but the 14 days count only on the Play Console: start in **month 1** |
| UGS more expensive than expected | estimate in January | sampled verification; local leaderboard at release |
| Low tester retention | Gate 5b | extend February; March becomes April. Better one more month than a 3.8★ release |
| "Let us add multiplayer" | any month | §2.4: only with the gates open, never before release |

---

## 5. Decisions to confirm (then in `04`)
Daily +2 for the top half · Weekly with 3 attempts · Guardian Gauntlet and Surge · Play Integrity only if evidence shows the need · WebGL demo on itch.io · the three multiplayer gates (§2.4) · the three compression choices (§4.0) and a release at the **end of March 2027** with a possible slip to April.
