# TowerDefense — instructions for Claude Code

Roguelite core defense for Android (Unity 6, URP, C#). Deterministic simulation + calm visuals. Solo developer + AI.

## Language
- Chat with the user in **Italian**. Everything else in **English**: docs, code, comments, logs, commit messages.

## Start of every session
1. Read `docs/README.md` (index + conventions) and `docs/06-development-plan.md` §4 (next steps).
2. Run `git log --oneline -10` and `git status` to see where the work stopped.
3. Do not install software: propose it, the user installs by hand.

## Project facts
- Unity **6000.6.2f1**, Universal 3D (URP 17.6), new Input System, UI Toolkit (D22). Portrait, one-handed. minSdk 26, targetSdk 36.
- Assemblies: `Assets/Scripts/Simulation` (pure C#, `noEngineReferences`), `Assets/Scripts/Presentation` (Unity), `Assets/Tests/EditMode` (NUnit).
- Scene: `Assets/Scenes/Prototype.unity`. Balance data: `ContentDatabase.CreatePrototypeDefaults()` until ScriptableObjects exist (`Assets/Content/`).
- Git: this folder is the repository; Git LFS for binaries. **Commit only when the user asks** (`/commit` skill).

## Tools (MCP)
- **unity** (MCP for Unity, `http://127.0.0.1:8080/mcp`): requires Unity open with *Window → MCP for Unity → Start Server*. If not connected, open the session in the parent folder that holds `.mcp.json` or start the server and restart the session (`docs/setup/NEW-PC-SETUP.md` §5).
- Instance name `TowerDefense@<hash>`; with several projects open, read `mcpforunity://instances` and call `set_active_instance`.
- Verify Unity APIs with `unity_reflect` / `unity_docs`, not from memory.
- Screenshots: `output_folder` = `Temp/Screenshots`, never inside `Assets`. The Game View does not repaint while the editor is unfocused: a screenshot taken during a wave may be stale — pause or take it in the shop.
- `read_console` shows only the first line of a multi-line log: log one line per entry (see `BalanceMenu`).
- **blender** (MCP for Blender, port 9876): only for models; ask the user to save the `.blend` before destructive `execute_blender_code`.

## Verify before saying "done"
- After any C# change: `read_console` until there are no compile errors, then `/verify` (runs the EditMode tests, assembly `TowerDefense.Simulation.Tests`). All tests must pass.
- After visual changes: take a screenshot and look at it.

## Rules that always apply
- Simulation: deterministic, 60 ticks/s, integers only, no Unity types, every state change is a `Command` (details load from `.claude/rules/simulation.md` when you touch those files).
- Visuals: calm by default — no flashing, ≤ 2 repeats/s in one spot, eased motion (`.claude/rules/presentation.md`).
- Never write "The Tower" or other games' names in code, assets or store text (D20).
- Keep `docs/05-gdd.md` section numbers stable: code comments cite them.

## Where things are documented
- `docs/05-gdd.md` — rules and numbers (source of truth) · `docs/03-art-and-audio-direction.md` — art bible · `docs/04-decision-log.md` — decisions and open proposals · `docs/06` + `docs/10` §4 — plan and calendar · `docs/07`–`09` — phase briefs.
- Read the relevant document before working on gameplay, UI, audio or monetization; propose changes to numbers as **PROPOSAL** and record decisions in `docs/04`.
