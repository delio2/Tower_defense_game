---
paths:
  - "Assets/Scripts/Simulation/**"
  - "Assets/Tests/**"
---

# Simulation rules (deterministic core)

- **No Unity types** in `Assets/Scripts/Simulation`: the asmdef has `noEngineReferences: true` and the same assembly runs on the server (Cloud Code) to verify replays.
- **Integers only** in game state: positions in micro-units (`SimConstants.Micro`), HP/damage in hundredths (`SimConstants.HpScale`), ratios in permille. No `float`, `double`, `Math.Pow`, trigonometry: directions come from the 192-entry table in `Directions`.
- **Growth by repeated multiplication**, never `Math.Pow` (see `WaveDirector.HpMultiplierPpm`).
- **Every state change is a `Command`** applied through `GameSimulation.Enqueue` + `Step`/`ApplyPendingCommandsNow`. No public setters, no side channels: if it is not a command, it is not replayable.
- **Randomness** only through the `Pcg32` streams (`RngStream.Waves`, `Shop`, `Effects`) in a fixed call order. Player choices must not touch the Waves stream.
- **Determinism test for every new mechanic:** same seed + same commands → same `ComputeStateHash()`. Add the new state to the hasher when you add fields.
- **Replay compatibility:** a change that alters outcomes bumps `RunConfig.BalanceVersion`; a format change bumps `Replay.FormatTag` and keeps the old one readable.
- Balance numbers follow `docs/05-gdd.md` (cite the section in a comment, e.g. `GDD v0.2 §9`). Changing a number without updating the GDD is a bug.
- Tests: NUnit in `Assets/Tests/EditMode/SimulationTests.cs`, one behaviour per test, names `Subject_DoesThing`. Run them with `/verify`.
