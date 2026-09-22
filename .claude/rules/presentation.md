---
paths:
  - "Assets/Scripts/Presentation/**"
  - "Assets/UI/**"
  - "Assets/Content/**"
---

# Presentation rules (visuals, UI, input, content assets)

- **Read-only view of the simulation:** presentation reads `GameSimulation` state and `DrainEvents`; it changes state only by enqueueing `Command`s. Never mutate simulation objects from here.
- **Calm by default** (`docs/03` A1, A7): no flashing, no full-screen flash, no effect repeated more than 2 times per second in one spot, every motion eased with a fade; "reduce motion" and "effect intensity" options must be respected by every effect.
- **Palette and shapes:** colours only from `Palette` / `Theme.uss`, which mirror the design system's `tokens.json` (`docs/03` A4, `docs/09` §5): enemy coral/rose never on Core, modules, UI or text; allies use `ally`; round = player, angular = enemy. Build every screen against its design-system mockup and compare screenshots side by side.
- **Rendering:** orthographic camera tilted 35°, warm key + gradient ambient, three-surface materials (body, fresnel rim, low emissive), tone mapping None (not AgX/ACES: they grey the ivory).
- **Mobile budget:** low-poly, ≤ 4 materials per category, SRP Batcher-friendly, no per-frame allocations in `Update`, pooling for lines and numbers, 30 fps on a 4 GB phone.
- **Touch (`docs/07` §1.2, `docs/09`):** targets ≥ 48 dp, drag threshold 8 dp, dragged object above the finger, magnet 0.45 units, long press 250 ms, hold 1 s for irreversible actions, no confirmation dialogs (Undo is the confirmation).
- **UI Toolkit** for HUD, cards and options (D22); tokens in `Assets/UI/Theme.uss` (`docs/09` §5); strings by key, never text in code.
- **Content ScriptableObjects** (`Assets/Content/`) mirror `ModuleDefinition`/`EnemyDefinition`/`RunConfig` and are loaded into `ContentDatabase` by `ContentLoader`; the numbers must equal `docs/05-gdd.md` and the defaults' hash test must stay green.
- Private fields `_camelCase`, `[SerializeField] private`, no public fields; `Application.targetFrameRate = 60`.
- After a visual change: screenshot via MCP (`Temp/Screenshots`) and look at it before reporting.
- **Player-build rules (learned on the Pixel, 2026-09-22):** no `Shader.Find` at runtime, load material assets from `Assets/Content/Resources/Materials` (shaders no asset references are stripped); no `GameObject.CreatePrimitive` (it adds colliders and the Physics module is stripped), build meshes with `PrimitiveMesh`; UI text only with ASCII/Latin-1 characters (device fonts lack ⚙ ◈ ▶ ↶ ⇧ ✓ ∞), icons are drawn elements; do not rely on `background-color` alpha for panels (it did not blend on the device), use opaque tints.
