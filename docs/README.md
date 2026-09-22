# Design documentation — working title "TowerDefense"

**Status (2026-09-22):** pre-production. Phases 1 and 2 of [06-development-plan.md](06-development-plan.md) are done in code (deterministic simulation with the full MVP roster, UI Toolkit HUD with magnetic drag, options, save/resume, balance v0.3 tuned by bots; 47 automated tests). Now in **Phase 2.5 — visual overhaul** (D30): design system and mood shot v1 done and approved by the user; Unity foundations A1–A7 done, and the three vertical slices **B (wave), C (shop) and D (overlays)** with them: models in motion, the information layer of the wave, offer cards v2 with reach dots, long-press sheets, wave preview with a spawn compass, damage share, pause and run end, large text and high contrast. The Android build runs on a Pixel 10 at 60 fps (E1) and the comparison sheets are in `art/` (E2). Left: the Low/Medium/High quality split, a 4 GB phone, and the tester gate (E3). Proposals under review: `11`, `12`. **Start from `06` §4.**

## How these documents work
- **One source of truth per topic.** Numbers and rules live in the GDD (`05`). Direction lives in `03`. Decisions live in `04`. The briefs (`07`–`10`) add detail for a phase; when a brief changes a rule, the change goes into `05` and the reason into `04`.
- **Markers.** `(estimate)` = our own reasoning, not market data. **PROPOSAL** = not decided yet; every open proposal is listed in `04` §Open decisions. ✅ done · 🔄 in progress · ⬜ not started.
- **Header** of every document: version · date · status. Dates are ISO (`YYYY-MM-DD`).
- **Language.** Documents, code, comments and commit messages are in English. In-game terms are English and defined once, in `05` §0.
- **Cross-references** use the document number and section, e.g. `05 §7`, and are links where it matters.

## Index
| File | Content |
|---|---|
| [00-market-research.md](00-market-research.md) | Market data, audience, what tower-defense players praise and criticize, the reference titles behind the current concept (Balatro, The Tower, asynchronous PvP), copyright |
| [01-concept.md](01-concept.md) | **Concept v2**: a Core at the centre, a ring of modules with neighbour combos, asynchronous multiplayer on replays |
| [02-monetization-and-marketing.md](02-monetization-and-marketing.md) | Fixed rules ("never pay-to-win"), models A (trial + unlock) and B (fair free-to-play), rewards, marketing principles |
| [03-art-and-audio-direction.md](03-art-and-audio-direction.md) | **Art bible** "Dusk Garden": principles, identity, palette, shapes, lighting, micro-animation catalogue, tactile interaction, haptics, audio |
| [04-decision-log.md](04-decision-log.md) | Decisions D1–D21 (current form; superseded ones as one-line history) and **open decisions** |
| [05-gdd.md](05-gdd.md) | **Game Design Document v0.2**: glossary, rules, numbers, modes, replay format, architecture, open questions, prototype status |
| [06-development-plan.md](06-development-plan.md) | Seven phases with deliverables and exit gates; what the community loves/hates mapped to phases; next steps |
| [07-gameplay-brief.md](07-gameplay-brief.md) | Phase 1 brief: input spec, second-by-second first run, enemies-as-questions, difficulty curves |
| [08-economy-brief.md](08-economy-brief.md) | Phase 2/4 brief: the two economies, merge yield, Archive tree, what is sold and what never is, season pass, anti-frustration |
| [09-ui-ux.md](09-ui-ux.md) | UI rules, navigation map, wireframes per screen, progressive disclosure, components, design tokens |
| [10-liveops-and-roadmap.md](10-liveops-and-roadmap.md) | Daily challenge, leaderboards and anti-cheat, secondary modes, multiplayer gates, launch marketing and store listing, **6-month roadmap** |
| [11-content-and-systems-brief.md](11-content-and-systems-brief.md) | **PROPOSAL** (D31): simpler names, shape grammar, 36 modules, 14 enemies + 3 Guardians, spawn gates and sectors, act rhythm, rewards, economy v2 |
| [12-run-structure-and-synergies-brief.md](12-run-structure-and-synergies-brief.md) | **PROPOSAL** (D32): rings as range bands (decided), act path, Charms, affinities, Nests and allied Sprites, rare and special petals, editions, the Pouch |
| [art/](art/) | Mood shot v1 renders, transparent module/enemy renders, the 47 SVG icons (source: `Art/Blender/moodshot.blend`) |
| [Dusk Garden design system](https://claude.ai/artifact/5aKRr9H8e3WJsMui64MNnu) | **Living visual reference** (private artifact): tokens for 4 themes, icons, components and every screen as an interactive mockup, research, screen map |
| [setup/NEW-PC-SETUP.md](setup/NEW-PC-SETUP.md) | How to set up a new PC: software, clone, Unity, MCP, Claude Code, Git |

## Working with Claude Code
The repository carries its own Claude configuration, so a fresh session on any PC knows how to move:
- `CLAUDE.md` — always-on instructions (< 60 lines): session start checklist, project facts, tools, verification, pointers.
- `.claude/rules/` — path-scoped rules loaded only when the matching files are touched: `simulation.md`, `presentation.md`, `docs.md`.
- `.claude/skills/` — `/verify` (console + EditMode tests through MCP) and `/commit` (user-only).
- `.claude/settings.json` — enables the `unity` MCP server from `.mcp.json` and pre-approves read-only tools.
- Personal, per-machine notes go in `CLAUDE.local.md` (gitignored). Claude's auto-memory lives outside the repository and is not a substitute for these files.
History of the abandoned v1 concept ("musical maze defense") is in git history before commit `b136844`; it is not kept in the tree.

## Acronyms
| Acronym | Meaning |
|---|---|
| **A/B** | split test between two variants |
| **API** | application programming interface |
| **ARPDAU** | average revenue per daily active user |
| **ASO** | app store optimization |
| **CPI** | cost per install |
| **D1 / D7 / D30** | share of players who return 1 / 7 / 30 days after install (retention) |
| **DAU** | daily active users |
| **DPS** | damage per second |
| **dp / sp** | density-independent pixels / scalable pixels (Android UI units) |
| **eCPM** | effective cost per thousand ad impressions (ad revenue) |
| **F2P / P2W** | free-to-play / pay-to-win |
| **fps** | frames per second |
| **FTUE** | first-time user experience (the first minutes after install) |
| **GDD** | game design document |
| **GDPR** | EU General Data Protection Regulation |
| **HP** | hit points |
| **IAP** | in-app purchase |
| **IARC** | International Age Rating Coalition (store age ratings) |
| **IL2CPP** | Unity's C++ scripting backend |
| **IMGUI** | Unity's immediate-mode GUI (prototype-only UI) |
| **IP** | intellectual property |
| **KPI** | key performance indicator |
| **LFS** | Git Large File Storage |
| **LTS** | long-term support release |
| **MCP** | Model Context Protocol (how Claude drives Unity and Blender) |
| **MVP** | minimum viable product |
| **PGS** | Google Play Games Services |
| **PvP** | player versus player |
| **QoL** | quality of life (features) |
| **RNG** | random number generator |
| **SDK** | software development kit |
| **SO** | ScriptableObject (Unity data asset) |
| **TD** | tower defense |
| **UGS** | Unity Gaming Services (backend) |
| **UI / UX** | user interface / user experience |
| **UMP** | Google User Messaging Platform (consent form) |
| **URP** | Unity Universal Render Pipeline |
| **USS** | Unity style sheets (UI Toolkit) |
| **UTC** | Coordinated Universal Time |
| **WCAG** | Web Content Accessibility Guidelines |
