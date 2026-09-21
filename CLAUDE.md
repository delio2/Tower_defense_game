# TowerDefense

- **Language:** chat with the user in **Italian**; **everything else in English**: documentation, code (class, method, variable, file and folder names), comments, log and error messages, commit messages.
- Do not install software on the PC: propose what to install, the user does it by hand.

## Project
- **Unity:** 6000.6.2f1, Universal 3D (URP 17.6), C#.
- **Platform:** Android first (then iOS). **Portrait**, one-handed (D2). minSdk 26, targetSdk 36 (D4).
- **3D:** Blender 5.2 LTS. **Code editor:** VS Code + Unity extension.
- **Unity MCP instance:** `TowerDefense@05fb0f4e1489f906` (the hash changes if the project is moved).

## MCP (configured in `../.mcp.json`)
- **unity** → MCP for Unity (CoplayDev), `http://127.0.0.1:8080/mcp`. Requires Unity open with the server started.
  - If several Unity projects are open, read `mcpforunity://instances` and `set_active_instance` to TowerDefense.
  - Verify Unity APIs with `unity_reflect` / `unity_docs` instead of relying on memory.
- **blender** → MCP for Blender, port 9876. Requires Blender open with "Start MCP Server".
  - Before destructive operations with `execute_blender_code`, ask the user to save the .blend.

## Workflow
- C# code: write files under `Assets/Scripts/`; after every change check `read_console` until there are no compile errors, and run the EditMode tests (`run_tests`, assembly `TowerDefense.Simulation.Tests`) when the simulation changes.
- Scenes, prefabs, components, play mode, screenshots: through MCP Unity.
- After significant changes take a screenshot (Unity or Blender) and check the result.
- Blender → Unity: export FBX or glTF to `Assets/Models/<category>/`; 1 unit = 1 m, "Apply Transform" on FBX.
- Mobile first: low-poly, few materials, textures ≤ 1024 px unless justified, low draw calls.
- **Calm visuals by default** (docs/03 A1): no flashing or full-screen flashes, no effect repeated more than 2 times per second in one spot, soft eased motion with fades. Minimal but attractive, never agitated.
- MCP screenshots always with `output_folder` = `Temp/Screenshots` (never inside `Assets`).
- Git: repository in this folder, Git LFS for binaries. Commit only when the user asks.

## Conventions
- Folders: `Assets/Scripts`, `Prefabs`, `Scenes`, `Models`, `Materials`, `Textures`, `Audio`, `UI`, `Content` (ScriptableObjects).
- C#: PascalCase for classes/methods, `_camelCase` for private fields, `[SerializeField] private` instead of public fields.
- Balance data (modules, enemies, waves) in ScriptableObjects in the `Presentation` layer, loaded into the simulation's `ContentDatabase` (the `Simulation` assembly cannot use Unity types). The prototype still uses the defaults in `ContentDatabase.CreatePrototypeDefaults()`: once the SOs exist, that method is for tests only.
- Input: the new Input System, designed for touch.
- Documentation: see `docs/README.md` for conventions (headers, markers, cross-references, acronyms). Keep `docs/05-gdd.md` section numbers stable: code comments reference them.

## Game design
Documents in `docs/` (read them before working on gameplay, UI, audio or monetization):
- `docs/00-market-research.md` — data and reference titles (Balatro, The Tower, asynchronous PvP, copyright)
- `docs/01-concept.md` — **concept v2**: Core at the centre + ring of modules with combos + asynchronous multiplayer
- `docs/02-monetization-and-marketing.md` — models A (trial + unlock) / B (fair F2P); never pay-to-win
- `docs/03-art-and-audio-direction.md` — **art bible** "Dusk Garden": calm, minimal, **different from The Tower**; catalogues of shapes and micro-animations; audio as atmosphere
- `docs/04-decision-log.md` — D1–D21 and the **open decisions/proposals**
- `docs/05-gdd.md` — **GDD v0.2**: the source of truth for rules, numbers and architecture
- `docs/06-development-plan.md` — phases with gates; `docs/10` §4 has the month-by-month calendar
- `docs/07-gameplay-brief.md`, `08-economy-brief.md`, `09-ui-ux.md`, `10-liveops-and-roadmap.md` — phase briefs (proposals marked **PROPOSAL**)
- `docs/archive/` — abandoned v1 music concept: **do not use as a reference**

Key constraints: complete offline single player + **asynchronous-only** multiplayer (replays), fixed-kit competition,
no energy/timers/forced ads/loot boxes, adult audience (never a childish style).
**Simulation:** deterministic, fixed 60 ticks/s, integers or fixed point, no `Math.Pow`/float trigonometry in state,
separate RNG streams; visuals and audio follow it. Every run is a verifiable replay (version + seed + commands).
**Anti-copy (D20):** never the name "The Tower" (or other games) in code, assets, store or marketing; different style and UI.
Terminology: glossary in the GDD §0 (Core, Ring, Module, Merge, Credits, Blueprints, Pulse, Act, Guardian).
