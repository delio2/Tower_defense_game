---
name: commit
description: Review the working tree, verify, and commit (and push if asked) with the project's commit conventions. Only the user triggers it.
disable-model-invocation: true
argument-hint: "[push]"
---

Commit the current work:

1. `git status --short` and `git diff --stat`. List what will be committed; make sure no generated folders (`Library/`, `Temp/`, `Logs/`, `*.csproj`) and no screenshots under `Assets/` are included.
2. If C# files changed, run `/verify` first. Do not commit with a red test or a compile error.
3. If a rule or a number changed in code, confirm `docs/05-gdd.md` (and `docs/04-decision-log.md` when it is a decision) were updated in the same change.
4. `git add -A`, then commit with a message in **English**: an imperative summary line (≤ 72 characters), a blank line, short bullets of what and why. End with the attribution line configured for this session.
5. If `$ARGUMENTS` contains `push`, run `git push origin HEAD` and report the pushed range.

Never amend or force-push; never commit when the user has not asked for a commit.
