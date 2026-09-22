# 04 — Decision log

> Version 3 · 2026-09-21 · Status: living document. Format per decision: **question → data → what the best do → decision → confidence**.
> Confidence: 🟢 high · 🟡 medium, to confirm in the prototype · 🔴 low, hypothesis.
> Decisions are numbered in the order they were taken. **Superseded** decisions are kept as one-line history. The D17 pivot (2026-09-21) replaced the "musical maze defense" concept with "central Core + modules with combos + asynchronous multiplayer" (D17–D21).

---

## Current decisions

### D1 — Must the game work without audio? → **Yes, first of all** 🟢
- **Data:** studies disagree: one reports over 90% of players muted (TapResearch), a 2025 one 60% with audio and 9% always muted. On TikTok 88% consider sound essential.
- **Decision:** *"it works muted first, audio amplifies"*. No game decision depends on sound: threats and timing are **seen**. Music is a marketing hook (TikTok is a sound-on channel) and the "extra" of the experience.
- **Validation:** the prototype is judged **muted first**, then with audio.
- *D17 update:* music is no longer a mechanic; the principle stays: **no audio-only game information**.

### D2 — Portrait or landscape? → **Portrait, one hand** 🟢
- **Data:** recent successful mobile TDs are portrait: Rush Royale, Random Dice, The Tower (7.1 M downloads). Marvel Snap is portrait to play one-handed. Bloons and Kingdom Rush are landscape but were born on PC/tablet. Balatro mobile is criticized for being landscape-only (`00` B4).
- **Mistake to avoid:** PvZ3 is portrait **and** simplified (no seed choice, imposed formations) and was criticized for **lost depth**, not for the orientation.
- **Decision:** portrait, **no scrolling**, every control reachable with the thumb. Depth stays intact.
- *D17 update:* the arena is **circular with the Core at the centre**, the shape best suited to a portrait screen.

### D3 — *Superseded by D18.* ~~6 waves + boss per sector, about 17-minute runs~~ → now 3 acts × (5 waves + Guardian), 10–15 minutes.

### D4 — Which phones do we support? → **4 GB+ RAM, Android 8+, target API 36** 🟢
- **RAM data:** 8 GB 38.5% · 6 GB 25.3% · 4 GB 16.3% · 12 GB 13% · 3 GB 3.2% · 2 GB 1.7%. **93% have 4 GB or more.** In 2026 budget phones are declining because of the memory crisis.
- **Android data:** according to AppBrain (September 2026) Android 16 is the most common version at 25.2%. Other collected percentages came from different sources and dates, summed above 100% and were discarded. Real `minSdk` coverage is checked on the **Play Console device catalogue** before publishing.
- **Google Play requirement:** from **2026-08-31** new apps and updates must target **API 36** (Android 16).
- **Decision:** `targetSdk 36`, `minSdk 26` (Android 8, to verify on the device catalogue). Reference phone: **low-end with 4 GB**, where we guarantee 30 fps; 60 fps on mid-range.

### D5 — 2D or 3D? → **Low-poly 3D with a tilted orthographic camera** 🟡
- **Reason:** volume and soft shadows with URP, depth in videos, the Blender/MCP pipeline is ready, and it stays as readable as 2D.
- **Risk:** bloom and shadow performance on budget phones. Mitigation: quality levels with bloom and shadows off.
- **Validation:** quick comparison in the mood shot (`03` A9): tilted orthographic 3D vs top-down.
- *D17 update (`03` v2):* "Dusk Garden" uses **softly lit** materials (warm key + cool ambient, soft shadows) and an orthographic camera **tilted about 35°**. The prototype still uses unlit materials and a top-down view.

### D6 — Online or offline? → **Offline-first; Unity Gaming Services backend (D21), Play Games Services optional** 🟢
- **Data:** offline play is a recurring player request and a strength of Mini TD 2 and Thronefall.
- **Decision:** the local save is the source of truth; cloud sync happens when there is a network. No mandatory account.
- *D21 update:* with asynchronous multiplayer, **leaderboards, replays, verification and cloud save go through UGS** (replays must be verified on the server, which Play Games Services cannot do). Play Games Services v2 stays **optional** (Google sign-in, achievements; the v1 APIs were removed in May 2026). Complete offline play does not change.

### D7 — Which languages? → **Little text + 6 languages at launch** 🟡
- **Data:** localized games earn **50–80% more** outside their home country. Over 50% of mobile revenue comes from China, Japan and Korea. Brazil and Latin America have low CPIs and many downloads.
- **Decision:** an interface **with little text**, made of icons and numbers (minimal in this too).
  - At launch: **EN, IT, ES, PT-BR, DE, FR** (the game in EN + IT first; the others for the store listing first, then in-game).
  - After soft launch: **JA, KO**, then ZH if data justify it.
  - ⚠️ Google Play does not operate in mainland China, where publishing also requires a government licence. Chinese would serve Taiwan and Hong Kong (traditional) only, unless a dedicated channel is opened.
  - AI translation plus native review for the store.

### D8 — First-time user experience → **In the game in under 10 seconds, "aha" within 90 seconds** 🟢
- **Data:** the average app loses **77% of users in 3 days**. If the "this is fun" moment comes after **90 seconds**, many do not return. Optimized onboarding raises retention up to 50%.
- **Mistakes to avoid:** accounts, settings and text screens at the start.
- **Decision:** no menu at first start: a **guided act** (`05` §14, second-by-second in `07` §1.5). Only exception: in the EU/UK the **consent form** appears before the tutorial (D16). Ads only after the tutorial.

### D9 — *Superseded by D17.* ~~Maze building, free but never blocking (flow field)~~ → no maze in the current concept.

### D10 — Run economy → **Credits at wave end + capped interest on savings** 🟢 *(rewritten after D18)*
- **Data:** if an escaping enemy costs only lives, it is sometimes better to let it through; the v1 fix was "no reward for leaked enemies".
- **Decision (current form):** Credits arrive **at the end of each wave** (base + interest + Guardian bonus, `05` §10), not per kill; an enemy reaching the Core damages its integrity. A small **interest** (1 Credit per 5 saved, capped) rewards planning, the Balatro way.

### D11 — Deterministic simulation → **Yes, from day one** 🟢
- **Reason:** shareable seeds, a Daily identical for everyone, replays to verify leaderboards and **automatic balance simulations** (a bot plays thousands of runs) all require that seed + moves always give the same result.
- **Decision:** game logic on **fixed-step simulation time**, seeded RNG, **no Unity physics** in gameplay. Audio and visuals *follow* the simulation.
- **Integer or fixed-point maths** for critical values (HP, damage, positions, times): floats can differ between processors (ARM vs x86), which would break replay verification and cross-device comparison.
- **Separate RNG streams** (Waves, Shop, Effects): player choices must not change the waves. Required for the Daily to be truly identical for everyone.
- *D17 update (implemented):* without music the clock is **60 ticks per second** (v1 used 96 ticks per beat); positions in micro-units with an integer table of 192 directions. Time advances **only during waves** (the shop is timeless).

### D12 — *Superseded by D17.* ~~2x/3x speed with music at fixed tempo~~ → speed simply accelerates everything.

### D13 — External services: when? → **Not needed to develop and test alone** 🟢
90% of the game is developed and tested **alone, without external services**. Services sit behind interfaces (`Services`, `05` §16) with "fake" implementations (console log) during development. **They become mandatory as soon as the game leaves the developer's hands.**

**TODO — before letting other people try the game (closed test)**
- [ ] **Google Play account**: choose personal vs organization, verify identity, reserve the package name *(personal requires 12 testers × 14 days + about 7 days of review; organization requires D-U-N-S and can take weeks → start in month 1, `10` §4)*
- [ ] **Analytics + crash reporting** (Firebase or GameAnalytics → D24)
- [ ] Analytics events: tutorial funnel (`07` §4.5), run start/end, act and wave reached, defeat (wave and cause), shop buys/sells/merges/undos, Pulse use, wave and shop durations
- [ ] Minimal privacy policy (needed for the closed test if data is collected)
- [ ] ⚖️ **Consent at first start (D16)**: integrate Google UMP **before** the tutorial (only where mandatory) and start analytics and crash reporting **after** the answer. To be checked by a lawyer before publishing
- [ ] Discord / tester group (at least 12 for a personal account)

**TODO — before publishing (soft launch and launch)**
- [ ] **Ad mediation**: AdMob or Unity LevelPlay → research (2026 comparison); only for model B
- [ ] **In-app purchases**: Unity IAP or direct Google Play Billing → research
- [ ] **GDPR consent** with Google UMP (mandatory in EU/UK) + full privacy policy
- [ ] **IARC** age rating and Google Play content questionnaire
- [ ] AI-generated content declaration (Google Play policy)
- [ ] **Game name** (D25) + store listing (icon, screenshots, video, ASO, `10` §3.5) + pre-registration
- [ ] Unity Gaming Services (leaderboards, Cloud Save, Cloud Code for replay verification) configured — D21; Play Games Services v2 optional
- [ ] Tax setup (VAT / regime) → accountant
- [ ] Final music rights (see `03` C4)

### D14 — *Superseded by D17.* ~~Acoustic Panel economic walls for the maze~~.

### D15 — *Superseded by D18.* ~~Upgrading more efficient than building (L2 +80% for 0.6×, L3 +100% for 0.9×)~~ → modules level up by merging duplicates (`05` §6, `08` §1.1).

### D16 — GDPR consent and analytics → **Consent at first start, before the tutorial** 🟢 *(user decision)*
- **Options considered:** anonymous data before consent (keeps the first start screen-free but is a legal grey area) · consent at first start · no data until consent (loses tutorial analytics).
- **Decision:** **Google UMP consent form at first start**, before the tutorial. Shown only where mandatory (EU/UK); elsewhere the game starts immediately. Analytics and crash reporting start after the answer and respect the choice.
- **Accepted cost:** a few extra seconds before the fun for European players, in exchange for maximum legal safety.
- ⚠️ To be checked by a lawyer before publishing.

---

### D17 — Pivot: music is no longer a mechanic → **New concept "Core + modules + combos"** 🟢 *(user decision, 2026-09-21)*
- **Why drop music:** with D1 (playable without audio) music would have become decoration. The game remained "another minimal maze TD" in a crowded niche (Infinitode, Emberward, Gnomes). The first prototype was not attractive either.
- **Data for the new direction:** Balatro (5 M+ copies, #1 paid mobile game, about $21 M mobile-only: **familiar base + roguelite combos + exploding numbers**); Ball x Pit (1 M+ copies, about $10 M, mobile with trial + unlock); paid mobile releases **+77% in 2025**; **The Tower** (about $1 M/month, 7.1 M downloads) criticized for lab timers, pay-to-win, too many currencies, mandatory connection and hours-long runs: a **huge unhappy audience** (`00` B).
- **Decision:** tower defense with a **Core at the centre, enemies from all sides, modules that combine** and growing numbers. Short runs, honest, offline. Details: `01` and `05`.

### D18 — Game loop → **Shop between waves + ring of slots + active ability** 🟢 *(user decision)*
- **Modules:** bought in a **shop between waves**, Balatro style: Credits, paid reroll, selling, interest on savings. **Buying a duplicate merges and levels it up** (to level 3); merge is the fastest-growing sub-genre (+74%).
- **Position:** 6 slots (expandable to 8) on a **ring around the Core**. Neighbouring modules combine: a small spatial puzzle, Backpack Battles style; builds are readable and shareable.
- **During the wave:** an **active Core ability** (Pulse) with a cooldown. A moment of choice without agitation.
- **Length:** 3 acts × (5 waves + Guardian) = 18 waves, **about 10–15 minutes**.

### D19 — Multiplayer → **Asynchronous only, built on deterministic replays** 🟢 *(user decision)*
- **Data:** Super Auto Pets, Backpack Battles and The Bazaar succeeded with **asynchronous PvP** against recorded runs. No real-time servers, and the game works with few players.
- **Decision:** no real time. Asynchronous modes introduced in stages, each behind a gate (`10` §2.4): (1) launch: daily and weekly challenge (same seed, same kit) with leaderboards + ghosts; (2) update 1: weekly leagues (groups of 30) + duels with a skill rating; (3) update 2: **Siege** + community boss.
- **Technique:** a **replay** (version + seed + mode + list of tick/command) of a few KB. Scores are verified on the server by **re-playing** the run with the same C# engine (no Unity), via UGS Cloud Code.
- **Replays are valid only for the same balance version:** leaderboards and ghosts are split by version or season.
- **Offline always stays complete.**

### D20 — Anti-copy rules (The Tower and others) 🟢
- **Mechanics are free** (CJEU, *SAS v. World Programming*, 2012). **"Look and feel" is not** (US, *Tetris v. Xio*, 2012), nor are names and brands (Google Play "Impersonation" policy, which can close the account).
- **Rules:** different visual identity (no neon on black with a square tower in the centre), different interface (modules arrive as shop offers and are mounted on the ring, not as stat sheets), original names for everything, **never "The Tower" in store, ads, keywords or files**.
- Before launch: **one hour with an IP lawyer** to review name, icon and screenshots.

### D21 — Backend for asynchronous multiplayer → **Unity Gaming Services** 🟡
- **Data:** UGS is pay-as-you-go with a free tier per service (leaderboards, Cloud Save, Cloud Code in C#). **PlayFab** cut its free tier to **1,000 players** in March 2026 (−99%). **Nakama** costs about $10/month self-hosted but must be maintained.
- **Decision:** UGS (integrated with Unity, Cloud Code C# to verify replays). To confirm with a cost estimate in month 4 (`10` §1.4). **Not needed for the offline launch** (D13). Fallback: sampled verification, then Nakama.

### D22 — UI framework → **UI Toolkit** 🟢 *(2026-09-22, implemented)*
- **Decision:** the HUD, cards, sheets and options are UI Toolkit (`Hud.uxml`, `Theme.uss` with the `09` §5 tokens, `HudView`). Dragging over the 3D arena works with one pointer path in the runner and hit tests from the HUD, so the uGUI fallback was not needed.

### D26 — Balance v0.3: enemy HP growth ×1.12 per wave 🟡 *(2026-09-22, bot data)*
- **Data:** with the GDD v0.2 value (×1.20) no bot passed the second Guardian on 100 seeds; a sweep (`05` §18) gave ×1.12 → Naive 8% (95% act 1), MaxDps 56%, EconomyFirst 27%, matching the `06` Phase 2 targets.
- **Decision:** ×1.12, Credits per wave unchanged. Balance version `0.3.0`. To confirm with testers (Gate 2); next lever if too hard: 5 Credits per wave.

### D27 — Core types tuned by bots 🟡 *(2026-09-22)*
- **Data:** Bastion (Bulwark, 4 Credits) and Glass (Emitter + Amplifier, 2 Credits) reached 92% bot wins, above the "no strategy above 85%" rule.
- **Decision:** Bastion starts with **2 Credits**; Glass starts with **Emitter only and 0 Credits**. Resulting bot wins: 76% and 68% (Glass stays the riskier Core for a naive player: 16% vs 8%). Merchant 45% is kept: its edge (interest) is one bots do not exploit.

### D28 — Grades 1–3 implemented as proposed 🟡 *(2026-09-22)*
- **Decision:** Grade 1 enemies +10% HP · Grade 2 −1 Credit per wave · Grade 3 elites from act 1 (`07` §2.2 proposal, first three). Bot wins: 42% / 18% / 9%. The step from Grade 1 to 2 is steep; Grades 4–10 remain proposals.

### D29 — Save = replay, resume = re-simulation 🟢 *(2026-09-22, implemented)*
- **Decision:** the run in progress is saved as its replay text at every shop (`RunSave`, PlayerPrefs for now); resuming re-simulates it, so a save can never disagree with the rules, and a save from another balance version is discarded. Replay format `R2` (game version, mode, Core, Grade) with `R1` still readable.

---

## Open decisions
| # | Question | Proposal | Where |
|---|---|---|---|
| **D23** | Server-side replay verification at launch? | Yes, with Cloud Code C#, if the UGS cost estimate is acceptable; otherwise sampled verification with replays kept for later checks | `10` §1.3–1.4 |
| **D24** | Analytics: Firebase or GameAnalytics? | Decide in month 4: GameAnalytics is free and simple; Firebase adds Crashlytics and Remote Config | `10` §4 |
| **D25** | Game name | Choose in month 5, after trademark and store checks (never references to other games, D20) | `10` §4 |

### Open proposals from the briefs (each marked **PROPOSAL** in its document)
| Topic | Proposal | Document |
|---|---|---|
| Pulse as a choice | P1 resonance (3+ enemies hit → next cooldown −25%); P2 as a rare "Resonator" booster; P3 "Focus" tap only if testers get bored | `07` §1.7 |
| Difficulty | Grades 4–10 (1–3 are implemented, D28) | `07` §2.2 |
| Defeat | one contextual tip after 3 defeats in the same act, never a hidden nerf | `07` §2.4 |
| Typography | Nunito (SIL OFL) | `03` A8 |
| Blueprints | +5 for the first win of the day, +3 for the Daily (even if lost), +2 for the top half | `08` §1.4, `10` §1.1 |
| Blueprint packs | cap of 400 per 7 days | `08` §2.2 |
| Shop bad-luck protection | 2 shops without a booster → the third guarantees one (deterministic); a rare guaranteed in the first shop of acts 2 and 3 | `08` §4.2 |
| Calm Mode | accessibility option (−20% enemy HP, −25% Pulse cooldown), outside leaderboards, never sold | `08` §4.3 |
| Streaks | "days played this month", never a chain that breaks | `08` §3.3 |
| Integrity display | also as an arc around the Core | `09` §2.2 |
| Archive | "try before you unlock" in a 3-wave trial run | `09` §2.5 |
| Confirmations | hold 1 s as the only confirmation for irreversible actions | `09` §0 |
| Pass | automatic reward claiming | `09` §2.6 |
| Weekly | up to 3 attempts, best counts | `10` §1.1 |
| Modes | Guardian Gauntlet (boss rush) and Surge (mutating endless) | `10` §2.1 |
| Anti-cheat | Play Integrity API only if evidence shows it is needed | `10` §1.3 |
| Marketing | WebGL demo of act 1 on itch.io | `10` §3.2 |
| Schedule | the three compression choices and a release at the end of March 2027 (possible slip to April) | `10` §4.0 |

## Sources
- Pivot (D17–D21): see sources in `00` Part B
- Muted play: https://blog.tapresearch.com/how-sound-preferences-impact-player-engagement · https://www.international-sound-directory.com/2025/12/07/do-people-really-play-mobile-games-without-sound-myth-or-reality/
- TikTok and sound: https://www.socialmediatoday.com/news/tiktok-shares-new-insights-into-the-importance-of-sound-for-marketing-promo/601569/
- Sessions: https://gamedevreports.substack.com/p/adjust-gaming-app-insights-report · https://www.blog.udonis.co/mobile-marketing/mobile-games/mobile-gaming-statistics
- Android versions 2026: https://www.androidheadlines.com/2026/01/android-version-distribution-numbers-2025-2026-market-share.html · https://www.appbrain.com/stats/top-android-sdk-versions
- Target API: https://support.google.com/googleplay/android-developer/answer/11926878?hl=en · https://developer.android.com/google/play/requirements/target-sdk
- Device RAM: https://commandlinux.com/android/android-global-market-share-statistics/ · https://www.idc.com/resource-center/blog/smartphone-shipments-set-for-record-16-7-drop-in-2026-as-the-memory-crisis-hits-full-force/
- PGS v2 Unity: https://github.com/playgameservices/play-games-plugin-for-unity · https://developer.android.com/games/pgs/unity/migrate-to-v2
- Localization: https://www.transphere.com/mobile-games-localization/ · https://speequalgames.com/predicting-2026-gaming-landscape-through-the-lens-of-mobile-game-localization/
- FTUE: https://blog.playio.co/mobile-game-onboarding-retention · https://segwise.ai/blog/mobile-gaming-app-user-retention-strategies
- TD balance: https://www.gamedeveloper.com/design/balance-in-td-games
- PvZ3: https://en.wikipedia.org/wiki/Plants_vs._Zombies_3:_Evolved
- One-handed portrait play: https://rovingames.com/blog/one-handed-mobile-games-for-real-life-breaks/
