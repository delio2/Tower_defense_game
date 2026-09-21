---
name: verify
description: Check that the Unity project compiles and that the simulation tests pass. Use after any C# change, before reporting work as done, or when the user asks "does it still work?".
argument-hint: "[test-name-filter]"
---

Verify the project through MCP for Unity (the editor must be open with the MCP server started):

1. `read_console` with `types: ["error"]`. If there are compile errors, fix them and repeat until the console is clean. Do not run tests with compile errors.
2. `run_tests` with `mode: "EditMode"`, `assembly_names: "TowerDefense.Simulation.Tests"`, `include_failed_tests: true`. If `$ARGUMENTS` is given, pass it as `test_names` filter.
3. `get_test_job` with the returned `job_id` and `wait_timeout: 60` until `status` is `succeeded` or `failed`.
4. Report: passed/failed/total and each failing test with its message. If anything fails, fix the cause and run again; never report "done" with a red test.

If the Unity MCP server is not reachable, say so and tell the user to start it (*Window → MCP for Unity → Start Server*) and restart the session; do not claim the tests passed.
