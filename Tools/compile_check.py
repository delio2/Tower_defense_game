"""Offline compile check for the four project assemblies.

Uses the Roslyn compiler shipped with the .NET SDK and the reference paths from the
csproj files Unity generates (open the project once with VS Code as the script editor).
It catches compile errors without Unity or MCP; it does NOT run the tests.

Usage (from the repository root):  python Tools/compile_check.py
"""
import glob
import io
import os
import re
import subprocess
import sys

ASSEMBLIES = [
    ("TowerDefense.Simulation", "Assets/Scripts/Simulation", []),
    ("TowerDefense.Presentation", "Assets/Scripts/Presentation", ["TowerDefense.Simulation"]),
    ("TowerDefense.Editor", "Assets/Scripts/Editor", ["TowerDefense.Simulation", "TowerDefense.Presentation"]),
    ("TowerDefense.Simulation.Tests", "Assets/Tests/EditMode",
     ["TowerDefense.Simulation", "TowerDefense.Presentation", "TowerDefense.Editor"]),
]
OUT = "Temp/Compile"


def find_csc():
    roots = [os.environ.get("ProgramFiles", r"C:\Program Files")]
    for root in roots:
        sdks = sorted(glob.glob(os.path.join(root, "dotnet", "sdk", "*", "Roslyn", "bincore", "csc.dll")))
        if sdks:
            return ["dotnet", sdks[-1]]
    sys.exit("csc.dll not found: install the .NET SDK (docs/setup/NEW-PC-SETUP.md)")


def refs_and_defines(csproj):
    if not os.path.exists(csproj):
        sys.exit(f"{csproj} missing: open the project in Unity once (VS Code as script editor) to generate it")
    text = io.open(csproj, encoding="utf-8").read()
    hints = [h for h in re.findall(r"<HintPath>(.*?)</HintPath>", text) if os.path.exists(h)]
    defines = re.findall(r"<DefineConstants>(.*?)</DefineConstants>", text)
    return hints, (defines[0] if defines else "")


def compile_assembly(csc, name, src, deps):
    hints, defines = refs_and_defines(f"{name}.csproj")
    files = glob.glob(f"{src}/**/*.cs", recursive=True)
    rsp = os.path.join(OUT, name + ".rsp")
    with io.open(rsp, "w", encoding="utf-8") as f:
        f.write(f"/nologo /target:library /langversion:9.0 /nowarn:CS1701,CS1702 /out:{OUT}/{name}.dll\n")
        if defines:
            f.write(f"/define:{defines.replace(';', ',')}\n")
        for h in hints:
            f.write(f'/reference:"{h}"\n')
        for d in deps:
            f.write(f'/reference:"{OUT}/{d}.dll"\n')
        for fl in files:
            f.write(f'"{fl}"\n')
    result = subprocess.run(csc + ["@" + rsp], capture_output=True, text=True)
    errors = [l.strip() for l in (result.stdout + result.stderr).splitlines() if "error CS" in l]
    print(f"{name}: {len(files)} files, {len(errors)} errors")
    for e in errors[:20]:
        print("   ", e)
    return result.returncode == 0


def main():
    os.makedirs(OUT, exist_ok=True)
    csc = find_csc()
    ok = True
    for name, src, deps in ASSEMBLIES:
        ok = compile_assembly(csc, name, src, deps) and ok
    print("COMPILE OK" if ok else "COMPILE FAILED")
    sys.exit(0 if ok else 1)


if __name__ == "__main__":
    main()
