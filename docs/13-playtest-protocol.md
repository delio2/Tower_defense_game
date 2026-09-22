# 13 — Playtest protocol (Phase 2.5-E3: Gates 1, 2 and 2.5 in one session)

> Version 1 · 2026-09-22 · Status: ready to use. How to run the first tester sessions and turn them into the Gate verdicts of `06`. The game records its own timings (`LocalAnalytics`, D13 fake); this document covers what the numbers cannot: what people understand, feel and say.

---

## 1. What one session answers
| Gate | Question | How we know |
|---|---|---|
| 1 | Is the shop with the ring pleasant to touch? | they start another run without being asked (3 of 5) |
| 1 | Is the neighbourhood understood without explanations? | by the second shop they place a booster next to a weapon on purpose, and can say why afterwards |
| 1 | Does anyone get stuck? | no shop with more than 10 s without a touch (report) + your notes |
| 1 | Real wave and shop length | report: wave 25–30 s, shop 10–20 s |
| 2 | Is a full run 10–15 minutes? | report: finished runs |
| 2.5 | "Calm but attractive, or prototype?" | the direct question at the end (3 of 5 say attractive) |

## 2. Who
- 3–5 adults (18–45), **one at a time**, who have never seen the game. Mix: at least one who does not play mobile games, at least one who does.
- Not people who will be kind no matter what; say explicitly that problems found now are the most useful thing they can give.

## 3. Before each tester
1. Build the tester APK (Unity: *TowerDefense → Build → Android tester APK*) or reuse the last one.
2. **Uninstall and reinstall** the app: this resets the saved run and the first-run funnel. `adb install -r` alone keeps them.
3. Phone at full brightness, **sound off** (D1: the game must work muted first), battery above 50%, notifications silenced.
4. Language: leave *Auto* (the phone's language). Do not open the options for them.
5. Have this sheet and a pen; note the time the phone changes hands.

## 4. What you say (and nothing more)
- Before: *"This is an early version of a game. There are no instructions yet on purpose: I want to see what is clear and what is not. Play as you like, say out loud what you think, and stop whenever you want. You cannot do anything wrong: if something is confusing, the game is wrong, not you."*
- During: **do not help, do not explain, do not point.** If they ask a question, answer *"What do you think it does?"* and write the question down: every question is a finding.
- If they are stuck for more than 30 s, you may say *"What would you try?"* and mark it as a rescue on the sheet.
- When they finish a run (win or lose), say nothing and **wait 10 seconds**: whether they press *Play again* on their own is the Gate 1 answer.

## 5. Observation sheet (one per tester)
| Moment | Watch for | Note |
|---|---|---|
| First 30 s | do they find ▶ / *Next* on their own? | seconds to first wave |
| First shop | first card dragged or tapped? drag or tap-tap? | |
| Second shop | booster placed **next to** a weapon on purpose? | yes / no / by chance |
| Waves | do they use the Pulse? when (panic, groups, never)? | |
| Any time | taps on things that are not buttons; reading the preview line; hesitations over 10 s | |
| Speed / pause | do they find them? | |
| Run end | *Play again* within 10 s without prompting? | yes / no |

## 6. Questions after the session (in this order)
1. *"Would you play it again tomorrow? Why?"* (not "did you like it")
2. *"Explain to me how the ring works, as if to a friend."* — understood = mentions that pieces affect their neighbours.
3. *"What did you not understand, or what annoyed you?"*
4. *"Looking at the screen: does it feel calm but attractive, or does it look like a prototype?"* — write their exact words (Gate 2.5).
5. 1–5: how clear, how calm, how fun, how much you wanted "one more wave".
6. *"If you could change one thing, what would it be?"*

## 7. The numbers
After each tester, with the phone on USB:
```
python Tools/playtest/report.py
```
It pulls the session logs (`<app files>/sessions/*.jsonl`) and prints the medians against the targets, every shop with more than 10 s without a touch (compare with your notes: thinking is fine, being lost is not), where runs ended, and the first-run funnel. Benchmark (bot) sessions are left out.

## 8. Verdict and where it goes
| Result | Action |
|---|---|
| Gates pass | record the verdict and quotes in `04` D30 (look) and a new decision (Gates 1–2); `05` §18 gets the measured durations; Phase 3 can open once `11`–`12` are approved |
| Neighbourhood not understood | do **not** add a tutorial first: revise the feedback (combo lines, preview) and retest — the golden rule of `06` |
| Shops too long / people stuck | list the exact moments from the report + notes; fix the interface before balance |
| Runs too long or too short | balance levers in `05` §18 (bot farm first, then testers) |
| "Prototype" | compare their words with the design system; the art pass of 3b is the lever |
