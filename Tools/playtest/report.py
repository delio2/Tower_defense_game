"""Playtest report: the Gate numbers from the session logs the game writes (docs/13, LocalAnalytics).

The game appends one JSON line per event to <persistentDataPath>/sessions/<start>.jsonl. This script pulls those
files from every phone connected over USB (or reads a folder), and compares them with the targets of docs/06:
wave 25-30 s, shop 10-20 s, run 10-15 min, nobody idle in a shop for more than 10 s, the first-run funnel.

Usage (repository root):
  python Tools/playtest/report.py                      # pull from every connected phone, then report
  python Tools/playtest/report.py --folder <path>      # a folder of .jsonl files (e.g. from the editor)
  python Tools/playtest/report.py --editor             # this PC's editor sessions
  python Tools/playtest/report.py --include-bots       # also count benchmark (bot) sessions
Numbers are for the moderator's sheet, not a verdict on their own: with 3-5 testers every run matters.
"""
import argparse
import datetime
import glob
import json
import os
import statistics
import subprocess
import sys

PACKAGE = "com.d3lioss.towerdefense"
REMOTE = f"/sdcard/Android/data/{PACKAGE}/files/sessions"
EDITOR = os.path.join(os.environ.get("USERPROFILE", ""), "AppData", "LocalLow", "D3lioss", "TowerDefense", "sessions")
ADB_CANDIDATES = [
    os.path.join(os.environ.get("LOCALAPPDATA", ""), "Android", "Sdk", "platform-tools", "adb.exe"),
    r"C:\Program Files\Unity\Hub\Editor\6000.6.2f1\Editor\Data\PlaybackEngines\AndroidPlayer\SDK\platform-tools\adb.exe",
]
FUNNEL = ["first_tap", "first_kill", "first_shop", "first_combo", "wave_2", "act_1_clear"]


def adb_path():
    for candidate in ADB_CANDIDATES:
        if os.path.exists(candidate):
            return candidate
    sys.exit("adb not found (Android Studio SDK or Unity's Android module)")


def pull_from_phones(out_root):
    adb = adb_path()
    listing = subprocess.run([adb, "devices"], capture_output=True, text=True).stdout.splitlines()[1:]
    serials = [line.split()[0] for line in listing if line.strip().endswith("device") and not line.startswith("emulator-")]
    folders = []
    for serial in serials:
        target = os.path.join(out_root, serial)
        os.makedirs(target, exist_ok=True)
        subprocess.run([adb, "-s", serial, "pull", REMOTE + "/.", target], capture_output=True)
        print(f"pulled {serial} -> {target}")
        folders.append(target)
    if not serials:
        print("no phone connected (emulators are skipped: use --folder for their pulled logs)")
    return folders


def load_sessions(folders, include_bots):
    sessions = []
    for folder in folders:
        for path in sorted(glob.glob(os.path.join(folder, "*.jsonl"))):
            events = []
            with open(path, encoding="utf-8") as f:
                for line in f:
                    line = line.strip()
                    if line:
                        try:
                            events.append(json.loads(line))
                        except json.JSONDecodeError:
                            pass  # a line cut by a crash
            start = next((e for e in events if e["event"] == "session_start"), {})
            if start.get("bot") and not include_bots:
                continue
            sessions.append({"file": path, "device": start.get("device", "?"), "events": events})
    return sessions


def median(values):
    return statistics.median(values) if values else None


def fmt(value, digits=1):
    return "-" if value is None else f"{value:.{digits}f}"


def check(ok):
    return "PASS" if ok else "FAIL"


def report(sessions):
    runs_started = runs_ended = runs_abandoned = 0
    run_minutes, wave_seconds, shop_seconds, idle = [], [], [], []
    defeats, funnel = {}, {step: 0 for step in FUNNEL}
    replay_sessions = 0
    for s in sessions:
        starts = [e for e in s["events"] if e["event"] == "run_start" and not e.get("resumed")]
        runs_started += len(starts)
        if len(starts) >= 2:
            replay_sessions += 1
        for e in s["events"]:
            kind = e["event"]
            if kind == "run_end":
                runs_ended += 1
                run_minutes.append(e["minutes"])
                if e.get("result") == "defeat":
                    key = f"wave {e['waves'] + 1} ({e.get('defeated_by')})"
                    defeats[key] = defeats.get(key, 0) + 1
            elif kind == "run_abandon":
                runs_abandoned += 1
            elif kind == "wave_end":
                wave_seconds.append(e["seconds"])
            elif kind == "shop_close":
                shop_seconds.append(e["seconds"])
                idle.append((e["idle_max"], e["wave"], os.path.basename(s["file"])))
            elif kind == "funnel" and e.get("step") in funnel:
                funnel[e["step"]] += 1

    stuck = [i for i in idle if i[0] > 10.0]
    lines = [
        f"# Playtest report - {datetime.datetime.now():%Y-%m-%d %H:%M}",
        "",
        f"Sessions: {len(sessions)} on {len({s['device'] for s in sessions})} device(s). Runs started: {runs_started}, "
        f"finished: {runs_ended}, abandoned: {runs_abandoned}. Sessions with a second run: {replay_sessions}.",
        "",
        "| Measure | Median | Range | Target | Check |",
        "|---|---|---|---|---|",
    ]
    if wave_seconds:
        m = median(wave_seconds)
        lines.append(f"| Wave (real s) | {fmt(m)} | {fmt(min(wave_seconds))}-{fmt(max(wave_seconds))} | 25-30 | {check(20 <= m <= 35)} |")
    if shop_seconds:
        m = median(shop_seconds)
        lines.append(f"| Shop (real s) | {fmt(m)} | {fmt(min(shop_seconds))}-{fmt(max(shop_seconds))} | 10-20 | {check(8 <= m <= 25)} |")
    if run_minutes:
        m = median(run_minutes)
        lines.append(f"| Finished run (min) | {fmt(m)} | {fmt(min(run_minutes))}-{fmt(max(run_minutes))} | 10-15 | {check(10 <= m <= 15)} |")
    if idle:
        lines.append(f"| Longest idle in a shop (s) | {fmt(median([i[0] for i in idle]))} | max {fmt(max(i[0] for i in idle))} | none > 10 | {check(not stuck)} |")
    lines += ["", "**Shops with more than 10 s without a touch** (watch these moments in the notes):"]
    lines += [f"- {i[2]}: shop before wave {i[1]}, {i[0]:.1f} s" for i in sorted(stuck, reverse=True)[:15]] or ["- none"]
    lines += ["", "**Where runs ended:**"]
    lines += [f"- {k}: {v}" for k, v in sorted(defeats.items(), key=lambda kv: -kv[1])] or ["- no defeats"]
    lines += ["", "**First-run funnel** (installs reaching each step; docs/07 section 3 - any step losing >10% is a design bug):"]
    lines += [f"- {step}: {funnel[step]}" for step in FUNNEL]
    return "\n".join(lines) + "\n"


def main():
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--folder", action="append", help="folder with .jsonl session files (repeatable)")
    parser.add_argument("--editor", action="store_true", help="read this PC's editor sessions")
    parser.add_argument("--include-bots", action="store_true", help="also count benchmark sessions")
    args = parser.parse_args()

    out_root = os.path.join("Temp", "Playtest")
    folders = list(args.folder or [])
    if args.editor:
        folders.append(EDITOR)
    if not folders:
        folders = pull_from_phones(out_root)

    sessions = load_sessions(folders, args.include_bots)
    if not sessions:
        sys.exit("no sessions found" + ("" if args.include_bots else " (bot sessions are skipped: --include-bots)"))
    text = report(sessions)
    os.makedirs(out_root, exist_ok=True)
    path = os.path.join(out_root, f"report-{datetime.datetime.now():%Y%m%d-%H%M%S}.md")
    with open(path, "w", encoding="utf-8") as f:
        f.write(text)
    print(text)
    print(f"-> {path}")


if __name__ == "__main__":
    main()
