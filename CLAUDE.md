# TowerDefense — instructions for Claude Code

Roguelite core defense for Android (Unity 6, URP, C#). Deterministic simulation + calm visuals. Solo developer + AI.

## Language
- Chat with the user in **Italian**. Everything else in **English**: docs, code, comments, logs, commit messages.

## Start of every session
1. Read `docs/README.md` (index + conventions) and `docs/06-development-plan.md` §4 (status, next steps, known open points).
2. Run `git log --oneline -10` and `git status` to see where the work stopped.
3. Do not install software: propose it, the user installs by hand.

## Project facts
- Unity **6000.6.2f1**, Universal 3D (URP 17.6), new Input System, UI Toolkit (D22). Portrait, one-handed. minSdk 26, targetSdk 36.
- Assemblies: `Assets/Scripts/Simulation` (pure C#, `noEngineReferences`), `Assets/Scripts/Presentation` (Unity; HUD in `UI/` — `HudView` is the facade, one panel per screen part: `WaveHud`, `ShopPanel` + `OfferCards` + `WavePreview`, `ArenaOverlay`, `WaveSummaryPanel`, `RunEndPanel`, `PauseSheet`, `OptionsSheet` — with `Assets/UI/Resources/Hud.uxml` + `Theme.uss`), `Assets/Scripts/Editor`, `Assets/Tests/EditMode` (NUnit).
- Scene: `Assets/Scenes/Prototype.unity`. Balance data: ScriptableObjects in `Assets/Content/Resources/` (catalog `PrototypeCatalog`), generated from `ContentDatabase.CreatePrototypeDefaults()` by *TowerDefense → Content → Generate…*; the defaults stay the reference for tests.
- UI assets are generated, never hand-edited: `python Tools/tokens_to_uss.py` writes the `Theme.uss` variables (plus `.theme-contrast` and `.text-large`) from `Tools/design/tokens.json`; *TowerDefense → Content → Build font assets* bakes the three weights from the variable fonts; `python Tools/icons_to_png.py` rasterises `docs/art/icons/*.svg` into `Assets/UI/Icons` (outside Resources: reference them from USS, not `Resources.Load`).
- Player-facing text lives only in `Assets/UI/Resources/Strings.txt` (key, English, Italian): `Loc.T("key", args)` in code, `@key` in UXML; a test checks every key (D38).
- Git: this folder is the repository; Git LFS for binaries. **Commit only when the user asks** (`/commit` skill). A dynamic font asset rewrites itself after Play: `git checkout -- Assets/UI/Fonts/` before committing.

## Tools (MCP)
- **unity** (MCP for Unity, `http://127.0.0.1:8080/mcp`): requires Unity open with *Window → MCP for Unity → Start Server*. If not connected, open the session in the parent folder that holds `.mcp.json` or start the server and restart the session (`docs/setup/NEW-PC-SETUP.md` §5).
- Instance name `TowerDefense@<hash>`; with several projects open, read `mcpforunity://instances` and call `set_active_instance`.
- Verify Unity APIs with `unity_reflect` / `unity_docs`, not from memory.
- Screenshots: `output_folder` = `Temp/Screenshots`, never inside `Assets`. The Game View does not repaint while the editor is unfocused: a screenshot taken during a wave may be stale — pause or take it in the shop.
- `read_console` shows only the first line of a multi-line log: log one line per entry (see `BalanceMenu`).
- **blender** (MCP for Blender, port 9876): only for models; ask the user to save the `.blend` before destructive `execute_blender_code`.

## Verify before saying "done"
- After any C# change: `read_console` until there are no compile errors, then `/verify` (runs the EditMode tests, assembly `TowerDefense.Simulation.Tests`). All tests must pass.
- Without Unity MCP: `python Tools/compile_check.py` compiles the four assemblies offline (no tests).
- Balance changes: run the bot farm (`dotnet run -c Release --project Tools/BotFarm -- --seeds 1000 --strategies all`) and record the probe in `docs/05` §18. Device checks: benchmark APK + `python Tools/bench/run_emulators.py` (D37). Playtests: `docs/13`, tester APK, `python Tools/playtest/report.py`.
- After visual changes: take a screenshot and look at it. The camera screenshot excludes the UI Toolkit overlay: use `ScreenCapture.CaptureScreenshot` via `execute_code` for the full Game View (its alpha compositing makes glass panels look opaque: check transparency in the editor, not in the capture).

## Rules that always apply
- Simulation: deterministic, 60 ticks/s, integers only, no Unity types, every state change is a `Command` (details load from `.claude/rules/simulation.md` when you touch those files).
- Visuals: calm by default — no flashing, ≤ 2 repeats/s in one spot, eased motion (`.claude/rules/presentation.md`).
- Never write "The Tower" or other games' names in code, assets or store text (D20).
- Keep `docs/05-gdd.md` section numbers stable: code comments cite them.

## Where things are documented
- `docs/05-gdd.md` — rules and numbers (source of truth) · `docs/03-art-and-audio-direction.md` — art bible · `docs/04-decision-log.md` — decisions and open proposals · `docs/06` + `docs/10` §4.2 — plan and calendar · `docs/07`–`09` — phase briefs · `docs/11`–`12` — content and run-structure **proposals** (not rules until approved).
- **Dusk Garden design system** (https://claude.ai/artifact/5aKRr9H8e3WJsMui64MNnu, private artifact): tokens (`tokens.json`), icons, silhouettes and interactive mockups of every screen — the visual reference for any UI or art work. `docs/art/` holds the mood shot, renders and SVG icons; `Art/Blender/moodshot.blend` is the model source.
- Read the relevant document before working on gameplay, UI, audio or monetization; propose changes to numbers as **PROPOSAL** and record decisions in `docs/04`.
