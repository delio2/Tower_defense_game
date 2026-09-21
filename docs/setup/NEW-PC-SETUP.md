# Setting up a new PC to develop the game

> Version 1 · 2026-09-21 · Status: reference. Complete guide to start from scratch on another computer (Windows 10/11, 64-bit). Estimated time: about 1–2 hours, mostly Unity downloads.

---

## 1. Recommended requirements
- Windows 10 or 11 64-bit, **16 GB RAM or more**, a recent GPU, **at least 40 GB free** (Unity + Android modules + project).
- Accounts: **Unity** (free, for Unity Hub), **GitHub** (with access to the repository), **Claude** (for Claude Code).

## 2. Software to install (by hand, in this order)
| # | Software | Version | Where | Notes |
|---|---|---|---|---|
| 1 | **Git for Windows** | latest | https://git-scm.com/download/win | Includes **Git LFS** (needed for models, images, audio) |
| 2 | **Unity Hub** | latest | https://unity.com/download | Sign in with your Unity account |
| 3 | **Unity Editor 6000.6.2f1** | **exactly this one** | Unity Hub → Installs → Install Editor (if missing: *Archive* on the Unity site) | Modules to tick: **Android Build Support** with **OpenJDK** and **Android SDK & NDK Tools**. Optional: Windows Build Support (IL2CPP), iOS, WebGL. **Untick** "Microsoft Visual Studio Community" |
| 4 | **VS Code** | latest | https://code.visualstudio.com | Microsoft's **"Unity"** extension (also installs C# Dev Kit) |
| 5 | **.NET SDK 10** | 10.x | https://dotnet.microsoft.com/download | Needed by the VS Code C# extension |
| 6 | **uv** (with uvx) | latest | `winget install astral-sh.uv` or https://docs.astral.sh/uv/ | Needed by the MCP servers (Blender); downloads Python by itself when needed |
| 7 | **Claude Code** | latest | Claude desktop app (Code tab) or CLI | To work with the AI on the project |
| 8 | **Blender 5.2 LTS** | 5.2.x | https://www.blender.org/download/ | Optional: only for 3D models. **Open it once** before installing the MCP add-on |

After the installations **close and reopen the terminal** so PATH is updated.

## 3. Downloading the project
Recommended folder structure (the same as the original PC):
```
Projects/
└── Games/                   ← working folder (Claude Code opens here)
    ├── .mcp.json            ← MCP configuration (to create, see §5)
    ├── .claude/settings.json
    └── TowerDefense/        ← this repository
```
On the original PC the folders are named `Progetti/Giochi`; any names work.

From PowerShell, inside the working folder:
```powershell
git lfs install
git clone https://github.com/delio2/Tower_defense_game.git TowerDefense
```
The first GitHub access opens a login window (Git Credential Manager).

## 4. Opening the project in Unity
1. **Unity Hub → Projects → Add → Add project from disk** → select the `TowerDefense` folder.
2. Open it with **Unity 6000.6.2f1**. The first opening regenerates the `Library` folder and **takes several minutes**.
3. The **MCP for Unity** package downloads by itself, because it is listed in `Packages/manifest.json` (Git must be on PATH).
4. If the **"MCP Setup"** window appears → **Skip** (Claude Code uses `.mcp.json`).
5. **Edit → Preferences → External Tools → External Script Editor → Visual Studio Code**.
6. Open the scene **`Assets/Scenes/Prototype.unity`**.
7. In the Game View choose a **portrait** resolution: resolution menu → **+** → *Fixed Resolution* 1080 × 1920.
8. Press **Play** to try the prototype (keep Unity in the foreground).

**Check that everything works:** Window → General → **Test Runner** → EditMode → **Run All** → all tests must pass (20 as of 2026-09-21).

## 5. Connecting Claude Code (MCP)
The repository already contains `.mcp.json` (Unity server), `.claude/settings.json`, rules and skills: **opening Claude Code inside `TowerDefense` is enough for Unity.** The parent-folder configuration below adds the Blender server and lets one session span several game projects.

1. Copy `TowerDefense/docs/setup/mcp.json.example` to `<working folder>/.mcp.json`.
2. Find the uvx path on the new PC:
   ```powershell
   (Get-Command uvx).Source
   ```
   and put it in the `"command"` field of the `blender` server (with doubled backslashes `\\`).
3. Copy `TowerDefense/docs/setup/claude-settings.json` to `<working folder>/.claude/settings.json` (it enables the project's MCP servers automatically).
4. **Unity:** open the project → **Window → MCP for Unity** → transport **HTTP Local**, URL `http://127.0.0.1:8080` → **Start Server**.
5. **Blender** (optional): open Blender once, then from a terminal:
   ```powershell
   uvx mcp-for-blender install-addon
   ```
   In Blender: Edit → Preferences → Add-ons → enable **"MCP for Blender"** → in the 3D view press **N** → **MCP for Blender** tab → **Start MCP Server** (port 9876). Enable **Poly Haven**.
6. **Claude Code:** open the session in the working folder (Unity + Blender) or inside `TowerDefense` (Unity only) and approve the servers when asked.
   Start Unity and Blender **first**, then Claude Code (servers are read when the session starts).

### Claude's "memory"
Project rules live in the repository (`TowerDefense/CLAUDE.md` and `docs/`), so they apply on the new PC too.
Claude's personal notes (memory) are stored **only on the original PC**, in `%USERPROFILE%\.claude\projects\<folder>\memory\`. Copy that folder if you want them; it is not essential.

## 6. Daily work
```powershell
cd TowerDefense
git pull                 # before starting: get the latest changes
# ... work ...
git add -A
git commit -m "Describe the change in English"
git push                 # at the end: save online
```
- Commit messages are **in English** (project rule); chat with Claude is in Italian.
- Models, images and audio go through **Git LFS** automatically (see `.gitattributes`).

## 7. Testing on an Android phone (when needed)
1. On the phone: Settings → About → tap "Build number" 7 times → Developer options → **USB debugging** on.
2. Connect with the USB cable and accept the authorization.
3. Unity: **File → Build Profiles → Android → Switch Platform** (slow the first time) → **Build And Run**.
   Android settings are already in the project: portrait, Android 8+ minimum, target Android 16, IL2CPP ARM64, provisional package `com.d3lioss.towerdefense`.

## 8. Common problems
| Problem | Solution |
|---|---|
| Claude says "unity: ConnectionRefused" | Unity closed or MCP server not started → Start Server, then reopen the Claude session |
| `localhost` does not work | Use `127.0.0.1` (on Windows localhost may resolve to IPv6) |
| `uvx` not found | Close and reopen the terminal; check with `(Get-Command uvx).Source` |
| The Blender add-on installer cannot find the folder | Open Blender at least once, then repeat the command |
| Unity asks for another version | Install exactly **6000.6.2f1** (see `ProjectSettings/ProjectVersion.txt`) |
| Large files missing after the clone ("empty" images) | `git lfs install` then `git lfs pull` |
| MCP screenshots end up in `Assets` | Always use the `Temp/Screenshots` folder (rule in `CLAUDE.md`) |
