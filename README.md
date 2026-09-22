# Tower Defense Game (working title)

Roguelite core defense for mobile (Android, then iOS): the Core at the centre, enemies from every side,
**modules on a ring that combine**, a shop between waves, 10–15 minute runs,
**offline** and with **asynchronous** multiplayer built on deterministic replays. Never pay-to-win.

- **Engine:** Unity 6 (6000.6.2f1), URP, C#
- **Status:** pre-production (2026-09-22): deterministic simulation with 14 modules, 7 enemies, 3 acts and replays; UI Toolkit prototype running on Android; **Phase 2.5 visual overhaul** in progress — see the plan in [docs/06](docs/06-development-plan.md) §4
- **Visual reference:** the Dusk Garden design system (link in [docs/README.md](docs/README.md)) and the mood shot in [docs/art/](docs/art/)

## Documentation
- [docs/README.md](docs/README.md) — index and conventions: market research, concept, monetization, art bible, decisions, **GDD**, development plan, briefs, **roadmap**
- [docs/setup/NEW-PC-SETUP.md](docs/setup/NEW-PC-SETUP.md) — **how to set up a new PC** to develop the game
- [CLAUDE.md](CLAUDE.md) + `.claude/` (rules, skills, settings) — how Claude Code works in this repository (see docs/README.md §Working with Claude Code)

## Code structure
| Folder | Content |
|---|---|
| `Assets/Scripts/Simulation` | Pure C# deterministic simulation (no Unity): arena, ring, modules, shop, waves, replay |
| `Assets/Scripts/Presentation` | Visuals and input of the prototype |
| `Assets/Tests/EditMode` | Automated simulation tests (Test Runner → EditMode) |
| `Assets/Scenes/Prototype.unity` | Prototype scene (the only scene in Build Settings) |

## Quick start
1. Follow [docs/setup/NEW-PC-SETUP.md](docs/setup/NEW-PC-SETUP.md).
2. Open `Assets/Scenes/Prototype.unity` and press **Play** (portrait Game View, 1080×1920).
