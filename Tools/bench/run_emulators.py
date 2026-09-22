"""Autoplay benchmark on Android emulators (and real phones), in parallel.

Boots each AVD headless, installs the benchmark APK, starts the game with an intent extra so a balance bot plays a
whole run (Assets/Scripts/Presentation/Diagnostics/AutoplayBenchmark.cs), reads the [BENCH] lines from logcat, takes
screenshots of the shop and of the Guardian waves, and writes one folder per device plus a summary table.

What it is for: layout on many screen shapes, crashes, memory, relative cost of the quality levels, and a
cross-platform determinism check (every device must end the same run with the same state hash). Emulator frame
rates run on the PC's GPU: they are NOT phone performance. Real fps come from --device (a phone over USB).

Usage (repository root):
  python Tools/bench/run_emulators.py                          # all AVDs, MaxDps, speed 3, quality Auto
  python Tools/bench/run_emulators.py --avds Small_Phone,Pixel_Tablet --strategy Planner --quality Low
  python Tools/bench/run_emulators.py --device <adb serial>    # a real phone instead of emulators
  python Tools/bench/run_emulators.py --show                   # emulator windows visible
Build the APK first: Unity menu TowerDefense > Build > Android benchmark APK (ARM64: the x86_64 emulator
images run it through their built-in ARM translation, since Unity 6.6 no longer builds x86_64 for Android).
"""
import argparse
import datetime
import os
import queue
import re
import subprocess
import sys
import threading
import time

SDK = os.path.join(os.environ.get("LOCALAPPDATA", ""), "Android", "Sdk")
ADB = os.path.join(SDK, "platform-tools", "adb.exe")
EMULATOR = os.path.join(SDK, "emulator", "emulator.exe")
PACKAGE = "com.d3lioss.towerdefense"
APK = os.path.join("Builds", "Android", "TowerDefense-bench.apk")

# Emulated RAM per AVD, so "quality=Auto" meets a low-end, a mid-range and high-end phones (D4, D36).
MEMORY_MB = {"Small_Phone": 4096, "Medium_Phone": 6144}
DEFAULT_MEMORY_MB = 8192


KV = re.compile(r'(\w+)=("[^"]*"|\S+)')
print_lock = threading.Lock()


def say(name, text):
    with print_lock:
        print(f"[{name}] {text}", flush=True)


def adb(serial, *args, timeout=120, binary=False):
    cmd = [ADB, "-s", serial, *args]
    result = subprocess.run(cmd, capture_output=True, timeout=timeout)
    return result.stdout if binary else result.stdout.decode("utf-8", "replace").strip()


def list_avds():
    out = subprocess.run([EMULATOR, "-list-avds"], capture_output=True, text=True).stdout
    return [line.strip() for line in out.splitlines() if line.strip() and not line.startswith("INFO")]


def boot(name, port, show):
    serial = f"emulator-{port}"
    cmd = [EMULATOR, "-avd", name, "-port", str(port), "-no-snapshot", "-no-boot-anim", "-no-audio",
           "-gpu", "host", "-memory", str(MEMORY_MB.get(name, DEFAULT_MEMORY_MB))]
    if not show:
        cmd.append("-no-window")
    log = open(os.path.join("Temp", "Bench", f"emulator-{name}.log"), "w")
    process = subprocess.Popen(cmd, stdout=log, stderr=subprocess.STDOUT)
    adb(serial, "wait-for-device", timeout=300)
    deadline = time.time() + 300
    while time.time() < deadline:
        if adb(serial, "shell", "getprop", "sys.boot_completed") == "1":
            return serial, process
        time.sleep(3)
    raise RuntimeError("boot timed out")


def launcher_activity(serial):
    out = adb(serial, "shell", "cmd", "package", "resolve-activity", "--brief",
              "-c", "android.intent.category.LAUNCHER", PACKAGE)
    return out.splitlines()[-1].strip()


PNG_SIGNATURE = bytes([0x89]) + b"PNG"


def screenshot(serial, path):
    data = adb(serial, "exec-out", "screencap", "-p", binary=True, timeout=30)
    if not data.startswith(PNG_SIGNATURE):
        # A foldable has several displays and screencap refuses to guess: take the first one it lists.
        ids = re.findall(r"Display (\d+)", adb(serial, "shell", "dumpsys", "SurfaceFlinger", "--display-id"))
        if ids:
            data = adb(serial, "exec-out", "screencap", "-p", "-d", ids[0], binary=True, timeout=30)
    if data.startswith(PNG_SIGNATURE):
        with open(path, "wb") as f:
            f.write(data)


def dismiss_fullscreen_prompt(serial, seconds=30):
    """Android's one-time "Viewing full screen" prompt takes focus and Unity waits behind it on the splash (the
    secure setting that used to pre-confirm it is ignored by recent images). Find its button and tap it."""
    deadline = time.time() + seconds
    while True:
        xml = adb(serial, "exec-out", "uiautomator", "dump", "/dev/tty", timeout=30)
        match = re.search(r'text="Got it"[^>]*bounds="\[(\d+),(\d+)\]\[(\d+),(\d+)\]"', xml)
        if match:
            x1, y1, x2, y2 = map(int, match.groups())
            adb(serial, "shell", "input", "tap", str((x1 + x2) // 2), str((y1 + y2) // 2))
            return True
        if time.time() >= deadline:
            return False
        time.sleep(2)


def run_device(name, serial, settings, out_dir, timeout_min, results):
    os.makedirs(out_dir, exist_ok=True)
    result = {"name": name, "status": "?", "lines": []}
    results[name] = result
    say(name, "installing")
    install = adb(serial, "install", "-r", "-g", APK, timeout=600)
    if "Success" not in install:
        result["status"] = "install failed: " + install[-200:]
        say(name, result["status"])
        return

    adb(serial, "logcat", "-c")
    # The system's one-time "Viewing full screen" prompt takes focus, and Unity waits behind it on the splash.
    adb(serial, "shell", "settings", "put", "secure", "immersive_mode_confirmations", "confirmed")
    adb(serial, "shell", "am", "force-stop", PACKAGE)  # a relaunch without it crashes Unity (docs/06 §4)
    activity = launcher_activity(serial)
    logcat = subprocess.Popen([ADB, "-s", serial, "logcat", "-v", "brief", "Unity:V", "AndroidRuntime:E",
                               "libc:F", "*:S"], stdout=subprocess.PIPE, stderr=subprocess.DEVNULL)
    adb(serial, "shell", "am", "start", "-n", activity, "--es", "benchmark", f"'{settings}'")
    time.sleep(2)
    if dismiss_fullscreen_prompt(serial):
        say(name, "dismissed the full-screen prompt")
    pid = adb(serial, "shell", "pidof", PACKAGE)
    say(name, f"running ({activity}, pid {pid or '?'})")

    raw = open(os.path.join(out_dir, "logcat.txt"), "wb")
    deadline = time.time() + timeout_min * 60
    last_alive_check = time.time()
    ended = False
    lines = queue.Queue()

    def pump():
        for raw_line in iter(logcat.stdout.readline, b""):
            lines.put(raw_line)
        lines.put(None)

    threading.Thread(target=pump, daemon=True).start()
    while time.time() < deadline:
        try:
            line = lines.get(timeout=1.0)
        except queue.Empty:
            line = b""
        if line is None:
            break
        text = ""
        if line:
            raw.write(line)
            text = line.decode("utf-8", "replace").rstrip()
        # Only our process counts: emulator images crash their own services (e.g. uwb) every few seconds.
        ours = (pid and f"pid {pid}" in text) or PACKAGE in text
        if ours and ("FATAL" in text or "Fatal signal" in text):
            result["status"] = "CRASH: " + text[-160:]
        if "[BENCH]" in text:
            payload = text.split("[BENCH]", 1)[1].strip()
            result["lines"].append(payload)
            fields = dict(KV.findall(payload))
            kind = payload.split(" ", 1)[0]
            if kind == "end":
                result["end"] = fields
                ended = True
            elif kind == "replay":  # logged in the same frame the run ends, so it may come before "end"
                result["replay"] = payload
            if ended and "replay" in result:
                break
        if time.time() - last_alive_check > 15:
            last_alive_check = time.time()
            if not ended and dismiss_fullscreen_prompt(serial, seconds=0):  # tablets show a taskbar tip mid-run
                say(name, "dismissed a system prompt")
            if not adb(serial, "shell", "pidof", PACKAGE):
                if result["status"] == "?" and not ended:
                    result["status"] = "app died (see logcat.txt)"
                break

    logcat.kill()
    raw.close()
    # The game takes its own screenshots at the right moments (AutoplayBenchmark.Shoot); fetch them.
    adb(serial, "pull", f"/sdcard/Android/data/{PACKAGE}/files/bench/.", out_dir, timeout=120)
    if not any(f.endswith(".png") for f in os.listdir(out_dir)):
        screenshot(serial, os.path.join(out_dir, "last.png"))  # fallback: at least the final screen
    if ended and result["status"] == "?":
        result["status"] = "ok"
    elif result["status"] == "?":
        result["status"] = "timeout"
    with open(os.path.join(out_dir, "bench.log"), "w", encoding="utf-8") as f:
        f.write("\n".join(result["lines"]) + "\n")
    say(name, result["status"])


def emulator_job(name, port, args, stamp, results):
    process = None
    serial = f"emulator-{port}"
    try:
        say(name, "booting")
        serial, process = boot(name, port, args.show)
        run_device(name, serial, args.settings, os.path.join("Temp", "Bench", stamp, name), args.timeout, results)
    except Exception as e:  # keep the other devices going
        results[name] = {"name": name, "status": f"error: {e}", "lines": []}
        say(name, f"error: {e}")
    finally:
        try:
            adb(serial, "emu", "kill", timeout=30)
        except Exception:
            pass
        if process:
            try:
                process.wait(timeout=60)
            except Exception:
                process.kill()


def summary(results, stamp):
    rows = ["| Device | Status | Replay | Screen | RAM | Quality | Result | Waves | fps avg | p95 ms | p99 ms | slow % | enemies max | draws max | mem MB | hash |",
            "|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|"]
    hashes = set()
    for name, r in sorted(results.items()):
        start = next((dict(KV.findall(l)) for l in r["lines"] if l.startswith("start")), {})
        waves = [dict(KV.findall(l)) for l in r["lines"] if l.startswith("wave=")]
        end = r.get("end", {})
        enemies = max((int(w.get("enemies_max", 0)) for w in waves), default=0)
        draws = max((int(w.get("draws_max", 0)) for w in waves), default=0)
        if end.get("hash"):
            hashes.add(end["hash"])
        rows.append("| {} | {} | {} | {} | {} | {} | {} | {} | {} | {} | {} | {} | {} | {} | {} | {} |".format(
            name, r["status"], "verified" if "verified" in r.get("replay", "") else r.get("replay", "-"), start.get("screen", ""), start.get("ram_mb", ""), start.get("quality", ""),
            end.get("result", ""), end.get("waves", ""), end.get("fps_avg", ""), end.get("ms_p95", ""),
            end.get("ms_p99", ""), end.get("slow_pct", ""), enemies, draws, end.get("mem_mb", ""), end.get("hash", "")))
    verdict = ("same final hash on every device" if len(hashes) == 1
               else f"DIFFERENT final hashes: {sorted(hashes)}" if hashes else "no final hash")
    text = "\n".join(rows) + f"\n\nDeterminism: {verdict}\n"
    path = os.path.join("Temp", "Bench", stamp, "summary.md")
    with open(path, "w", encoding="utf-8") as f:
        f.write(text)
    print("\n" + text + f"\n-> {path}")


def main():
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--avds", default="all", help="comma-separated AVD names, or 'all'")
    parser.add_argument("--device", help="adb serial of a real phone (skips the emulators)")
    parser.add_argument("--strategy", default="MaxDps")
    parser.add_argument("--speed", default="3")
    parser.add_argument("--quality", default="Auto")
    parser.add_argument("--seed", default="4")
    parser.add_argument("--timeout", type=float, default=25, help="minutes per device")
    parser.add_argument("--show", action="store_true", help="show the emulator windows")
    args = parser.parse_args()
    args.settings = f"strategy={args.strategy};speed={args.speed};quality={args.quality};seed={args.seed};quit=1"

    if not os.path.exists(APK):
        sys.exit(f"{APK} missing: build it from Unity (TowerDefense > Build > Android benchmark APK)")
    stamp = datetime.datetime.now().strftime("%Y%m%d-%H%M%S")
    os.makedirs(os.path.join("Temp", "Bench", stamp), exist_ok=True)
    results = {}

    if args.device:
        run_device(args.device, args.device, args.settings, os.path.join("Temp", "Bench", stamp, args.device),
                   args.timeout, results)
    else:
        names = list_avds() if args.avds == "all" else args.avds.split(",")
        threads = []
        for i, name in enumerate(names):
            t = threading.Thread(target=emulator_job, args=(name, 5554 + 2 * i, args, stamp, results))
            t.start()
            threads.append(t)
            time.sleep(5)  # stagger the boots
        for t in threads:
            t.join()
    summary(results, stamp)


if __name__ == "__main__":
    main()
