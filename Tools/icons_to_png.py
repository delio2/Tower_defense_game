"""Rasterise the design-system icons (docs/art/icons/*.svg) into the sprites the HUD uses.

The 24 x 24 line icons come from the Dusk Garden design system. They are drawn in white on transparency so a
single file can be tinted per context from the stylesheet (-unity-background-image-tint-color), and written at
96 px: three times the 32 dp the HUD asks for, which covers the densest phone without a second set.

Rendering uses Microsoft Edge in headless mode, already installed on Windows — no extra tool to install. One
page per icon: Edge has no batch mode, and at ~1 s each the whole set still takes under a minute.

    python Tools/icons_to_png.py            # all icons
    python Tools/icons_to_png.py pulse undo # only these
"""
import os
import re
import subprocess
import sys
import tempfile

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
SOURCE = os.path.join(ROOT, "docs", "art", "icons")
TARGET = os.path.join(ROOT, "Assets", "UI", "Icons")
SIZE = 96
EDGE = [
    r"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
    r"C:\Program Files\Microsoft\Edge\Application\msedge.exe",
]
PAGE = """<!doctype html><html><head><meta charset="utf-8"><style>
html,body{{margin:0;padding:0;background:transparent;width:{size}px;height:{size}px;overflow:hidden}}
svg{{display:block;width:{size}px;height:{size}px}}
</style></head><body>{svg}</body></html>"""


def edge():
    for path in EDGE:
        if os.path.exists(path):
            return path
    sys.exit("Edge not found: this script rasterises through headless Edge (see the module docstring).")


def white(svg):
    """The icons ship in ivory; white lets the stylesheet tint each one for its context."""
    return re.sub(r"#[0-9A-Fa-f]{6}", "#FFFFFF", svg)


def main():
    wanted = {name.lower() for name in sys.argv[1:]}
    browser = edge()
    os.makedirs(TARGET, exist_ok=True)
    names = sorted(f[:-4] for f in os.listdir(SOURCE) if f.endswith(".svg"))
    if wanted:
        names = [n for n in names if n.lower() in wanted]

    with tempfile.TemporaryDirectory() as work:
        for index, name in enumerate(names, 1):
            with open(os.path.join(SOURCE, name + ".svg"), encoding="utf-8") as f:
                svg = white(f.read())

            page = os.path.join(work, name + ".html")
            with open(page, "w", encoding="utf-8") as f:
                f.write(PAGE.format(size=SIZE, svg=svg))

            out = os.path.join(TARGET, name + ".png")
            subprocess.run([
                browser,
                "--headless=new",
                "--disable-gpu",
                "--hide-scrollbars",
                "--force-device-scale-factor=1",
                "--default-background-color=00000000",
                f"--window-size={SIZE},{SIZE}",
                f"--screenshot={out}",
                "file:///" + page.replace("\\", "/"),
            ], check=True, capture_output=True, timeout=60)
            print(f"[{index}/{len(names)}] {name}.png")

    print(f"{len(names)} icons written to {os.path.relpath(TARGET, ROOT)}")


if __name__ == "__main__":
    main()
