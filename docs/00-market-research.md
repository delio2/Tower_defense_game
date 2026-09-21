# 00 — Market research

> Version 2 · 2026-09-21 · Status: reference. Figures are from September 2026 sources (listed at the end); `(estimate)` marks our own reasoning.
> Part A was gathered for the abandoned v1 concept (see `04` D17) but the market, audience and genre data remain valid. Part B is the research behind the current concept.

---

## Part A — Market, audience, genre

### A1. Mobile market 2026
- Global revenue flat at about **$40 B in H1 2026** (−0.8%). **Downloads −12%**, but **revenue per download +11%**.
- Growing: **Puzzle +20%** (Merge-2 +74% revenue, Sort puzzle ×3), **Hybrid-casual +23%**, Party +29%.
- Declining: **Strategy −4%** (dominated by big 4X titles), **RPG −14%**.
- Minimal games lead the charts: **Block Blast** was the most downloaded game worldwide in 2024, 2025 and Q1 2026 (870 M downloads). **Arrows – Puzzle Escape** created a category in 2026 and is already #4 by downloads.
- Fashionable mechanics: energy-gated exploration minigames, battle passes with gacha, collectible albums, roguelite.

### A2. Audience
- Average mobile player age **36**. Brackets: 25–34 (29.5%), 16–24 (28.3%), 35–44 (23.1%). Gender split about 50/50.
- Men prefer strategy and sports; women puzzle; **75% play to relax**.
- **Chosen target: adults 18–45, mixed audience.** Not a children's game.

### A3. Why not a children's game
- Google Play Families Policy: no personalized ads, no advertising identifiers, certified SDKs only (the programme is closed to new entrants).
- Ad revenue **2.5–5× lower**. AppLovin MAX blocks under-13s; ironSource Direct shut down in April 2026.
- New age laws (Louisiana 2026, California and Utah 2027, UK Online Safety Act).
- ⚠️ A **childish** art style can get the game classified "for children" even if the declared audience is adult. The style must be elegant, **never childish** (see `03`).

### A4. Economics (Google Play, solo developer)
| Metric | 2026 value |
|---|---|
| Rewarded eCPM, rich countries | sources disagree: $15–30 (general estimates) vs about $13 measured on Android US → **we use $10–15** |
| Interstitial eCPM, rich countries | $5–8 |
| Hybrid ARPDAU (ads + IAP) | $0.15–0.50 |
| Median retention D1 / D7 / D30 | 22% / <4% / 0.8% |
| Android CPI, rich countries | about $3 (midcore $3–10) |
| Google commission on IAP | 15% up to $1 M/year (requires **enrolling** in the programme on Play Console) |
| Installs from store search | 50–70% |

- Value per download `(estimate)`: weak $0.05–0.15, fair $0.30–0.60, good $1–2.
- Realistic goal `(estimate)`: 10k–100k downloads in the first year with constant promotion.
- **New personal developer account:** mandatory closed test with **12 testers for 14 days**, then about 7 days of review.

### A5. Tower defense: what players criticize
| Game | Criticism |
|---|---|
| Kingdom Rush 5 (mobile) | Paid game with about half the content; heroes and towers cost as much as the game. Complete on Steam |
| Bloons TD 6 | Monkey Knowledge became a paid currency grind; 45+ minute sessions late game |
| Rush Royale / Random Dice | Pay-to-win, matchmaking against players twice as strong, too much randomness, co-op without seeing the partner |
| PvZ 2 / 3 | Paid plants; PvZ3 simplified, imposed formations, less freedom |
| Infinitode 2 | Arbitrary grind late game, phone left on overnight |
| Arknights | Energy ("Sanity") limits play |
| The Tower | Huge commercial success but 4.0 rating: a sign of discontent (see B2) |

Common genre problems: **passivity** (watch and wait), **repetitiveness**, timers and energy, **unfair randomness**, **poor information**.

### A6. Tower defense: what players love
| Game | What they like |
|---|---|
| Bloons TD 6 | 3 upgrade paths per tower, heroes, constant updates and balancing, co-op |
| Kingdom Rush | Hand-made levels, movable heroes, charm |
| Arknights | Skill matters more than money, active abilities, roguelike without energy, auto-repeat |
| Thronefall | Minimal, day/night cycle, **zero microtransactions**; 1–3 people, 315k copies, about $1.5 M |
| Gnomes | Roguelike with mazes; **2 developers, 10 months**, over $360k |
| Infinitode 2 / Mini TD 2 | Minimal, deep, honest; Mini TD 2 made by **1 developer** |
| The Tower | Numbers that grow, minimal; about $1 M/month (third-party estimate) |

### A7. What the community asks for and does not find
Hybrid tower defense (roguelite / builder / action) · honest monetization · **synergies and status effects** · several modes in one game · co-op and asynchronous social · cloud save and cross-platform · complete QoL (speed, pause, wave preview) · **"cozy mobile with deep strategy"** · a modern "reference" tower defense.

### A8. Minimal / original TD competitors (watch list)
- **Emberward**, **Refactor**: roguelite TD with Tetris-block mazes, also on mobile (the "block maze" is not original — one reason v1 was dropped).
- Minimal TD: Infinitode 2, Mini TD 2, Neon Defense, HexTowerDefense, Color Defense.

---

## Part B — Research v2 (2026-09-21), the basis of the D17 pivot

### B1. The formula of recent hits
| Game | Result | Formula |
|---|---|---|
| **Balatro** | 5 M+ copies; #1 paid game on iOS and Android; about $21 M on mobile | familiar base (poker) + roguelite jokers + exploding numbers; single price, no microtransactions |
| **Ball x Pit** | 1 M+ copies, about $10 M; on mobile since March 2026 with **free trial + $9.99 unlock** | familiar base (Breakout) + fusions + progression |
| **Balatro-likes** (CloverPit, PiN, Dice A Million…) | a genre born within a year | slots, pinball, dice + combos |
| **Backpack Battles** | 100k copies in 2 days, 92% positive | spatial arrangement + **asynchronous PvP** |
| **Super Auto Pets** | big free success, not pay-to-win | merging duplicates + **asynchronous PvP** |

- Paid mobile games: **+77% releases in 2025** (about 750); free-to-play still 96% of downloads.
- "Billion-dollar roguelike" analysis: familiar base, exponential synergies, "number go up", short runs, meta-progression, non-P2W monetization.

### B2. The market gap: The Tower
- About $1 M/month (third-party estimate), 7.1 M downloads, 4.0 rating.
- **Recurring criticism:** very long lab timers skipped by paying, pay-to-win, too many currencies, mandatory connection, "not really idle", hours-long runs, crowded upgrade menus with small text.
- **Tournaments:** 30-player league groups, twice a week, with tickets.

### B3. Asynchronous multiplayer and backend
- Asynchronous PvP against "ghosts" (Backpack Battles, Super Auto Pets, The Bazaar): no real-time servers, works with few players.
- Known complaints to avoid: Super Auto Pets matches by win count, perceived as "pure luck"; Backpack Battles serves ghosts from old versions.
- **Unity Gaming Services (UGS):** pay-as-you-go with a free tier per service. **PlayFab:** free tier cut to 1,000 players (March 2026). **Nakama:** open source, about $10/month self-hosted.
- Anti-cheat: re-simulate the deterministic replay (seed + commands) on the server.

### B4. Mobile ports: what players complain about (Balatro, Vampire Survivors, Brotato, Ball x Pit)
- Balatro mobile: landscape only, small text, unreliable cloud save (lost progress, endless "loading cloud save"), motion sickness from the animated background, slow score counting with no skip.
- Vampire Survivors: late-game snowball turns the run into passive spectacle; visual clutter with no options.
- Brotato: screen crammed with enemies and projectiles.
- Ball x Pit: a second grind (building) stacked on the first.

### B5. Copyright (basis of D20)
- Game mechanics are not protected (CJEU, *SAS v. World Programming*, 2012); "look and feel" is (US, *Tetris v. Xio*, 2012; *Triple Town v. Yeti Town*).
- Google Play forbids names, icons and descriptions that suggest a link to another app (Impersonation policy).

---

## Sources
### Part A
- PocketGamer.biz — H1 2026 genre analysis: https://www.pocketgamer.biz/h1-2026-genre-analysis-strategy-stumbles-rpgs-fall-and-puzzle-revenue-ramps-up/
- AppMagic — Casual Report H1 2026: https://appmagic.rocks/research/casual-report-H12026/?hl=en
- GameRefinery — July/Aug 2026: https://www.gamerefinery.com/mobile-game-market-review-july-august-2026/
- Block Blast Q1 2026: https://www.businesswire.com/news/home/20260423998721/en/Block-Blast-Ends-Q1-2026-as-the-No.-1-Most-Downloaded-Mobile-Game-Worldwide
- mobilegamer.biz — top 10 downloads 2026: https://mobilegamer.biz/2026s-top-10-mobile-game-downloads-so-far-free-fire-max-block-blast-roblox-arrows-more/
- Demographics: https://www.blog.udonis.co/mobile-marketing/mobile-games/modern-mobile-gamer
- Google Play Families Policy: https://support.google.com/googleplay/android-developer/answer/9893335?hl=en
- COPPA ad monetization 2026: https://www.gamebizconsulting.com/newsletter/admon-newsletter-11-the-shrinking-island-ad-monetization-underage-users
- Tenjin ad benchmark 2026: https://tenjin.com/blog/ad-mon-gaming-2026/
- CPI 2026: https://gamegrowthadvisor.com/blog/2026-03-17-user-acquisition-cpi-benchmarks-2026/
- GameAnalytics benchmarks 2026: https://www.gameanalytics.com/reports/2026-mobile-pc-gaming-benchmarks
- 12 testers / 14 days: https://support.google.com/googleplay/android-developer/answer/14151465?hl=en
- Kingdom Rush 5: https://www.pocketgamer.com/kingdom-rush-5-alliance/review/
- Rush Royale: https://www.complaintsboard.com/rush-royale-b149520
- Infinitode 2 (Steam): https://steamcommunity.com/app/937310/discussions/0/3048356136497549886/
- Arknights (Naavik): https://naavik.co/deep-dives/arknights-tower-defense-redefined/
- Thronefall (GameDiscoverCo): https://newsletter.gamediscover.co/p/deep-dive-how-thronefall-went-minimal
- Gnomes: https://www.gamesradar.com/games/strategy/2-devs-spent-10-months-making-a-roguelike-tower-defense-game-and-its-soaked-up-over-usd360-000-on-steam-as-players-construct-impossibly-dense-labyrinths-for-the-gnome-gods/
- The Tower: https://www.appbrain.com/app/the-tower-idle-tower-defense/com.TechTreeGames.TheTower
- TD 2026 wishes: https://towersdefense.org/articles/tower-defense-2026
- Tower defense 2026 overview (Bloons, Rush Royale, Mindustry, Rogue Tower): https://www.switchbladegaming.com/strategy-games/best-tower-defense-2026/
- Emberward: https://store.steampowered.com/app/2459550/Emberward/ · Refactor: https://m.taptap.io/app/refactor-232307

### Part B
- Balatro: https://www.gematsu.com/2025/01/balatro-sales-top-five-million · https://en.wikipedia.org/wiki/Balatro · https://www.gamesradar.com/games/roguelike/balatro-creator-in-disbelief-as-the-roguelike-hit-tops-mobile-sales-charts-beating-minecraft-and-stardew-valley-despite-one-pesky-issue/
- Premium +77%: https://gamedev.net/news/premium-mobile-games-are-back-with-releases-up-77-in-2025-r4367/
- Ball x Pit: https://www.gematsu.com/2025/12/ball-x-pit-sales-top-one-million-three-free-content-updates-set-for-2026 · https://www.gamerbraves.com/appmagic-reveals-how-ball-x-pit-bounced-to-10m-on-steam/ · https://rogueliker.com/ball-x-pit-mobile/ · https://www.engadget.com/gaming/ball-x-pit-will-land-on-ios-and-android-on-march-12-175159137.html · https://dreager1.com/2026/05/30/ball-x-pit-review/
- Balatro-likes: https://www.howtogeek.com/how-balatro-spawned-its-own-genre/ · https://en.wikipedia.org/wiki/CloverPit
- Billion-dollar roguelike: https://www.robin-guo.com/p/the-billion-dollar-roguelike-game
- Backpack Battles: https://en.wikipedia.org/wiki/Backpack_Battles · https://steamcommunity.com/app/2427700/discussions/0/3883851232929263899/ · https://steamcommunity.com/app/2427700/discussions/0/6770657417430301203
- Auto battlers / Super Auto Pets: https://en.wikipedia.org/wiki/Auto_battler · https://steamcommunity.com/app/1714040/discussions/0/3086646248537424616/
- The Tower: https://kimola.com/reports/unlock-player-insights-for-the-tower-idle-tower-defense-google-play-en-us-157711 · https://lostscotpro.com/reviews/the-tower-idle-tower-defense-review · https://the-tower-idle-tower-defense.fandom.com/wiki/Tournaments
- Backend: https://unity.com/products/gaming-services/pricing · https://accelbyte.io/blog/playfab-just-cut-its-free-tier-by-99.-heres-what-that-means-for-your-game · https://crux.supercraft.host/blog/nakama-open-source-vs-managed-backend/
- Replay verification: https://bugnet.io/blog/debugging-leaderboard-anomalies
- Balatro mobile complaints: https://www.engadget.com/gaming/balatro-is-an-almost-perfect-mobile-port-163050971.html · https://steamcommunity.com/app/2379780/discussions/0/604157441922347014/ · https://roguewiki.com/balatro/guides/balatro-mobile-vs-pc-guide
- Vampire Survivors / Brotato: https://choostgames.com/blog/vampire-survivors-vs-brotato/ · https://videochums.com/article/brotato-vs-vampire-survivors
- Offline, no-microtransaction demand: https://choostgames.com/blog/best-offline-mobile-games-no-microtransactions/
- Copyright: https://en.wikipedia.org/wiki/SAS_Institute_Inc_v_World_Programming_Ltd · https://en.wikipedia.org/wiki/Tetris_Holding,_LLC_v._Xio_Interactive,_Inc. · https://support.google.com/googleplay/android-developer/answer/9888374?hl=en
