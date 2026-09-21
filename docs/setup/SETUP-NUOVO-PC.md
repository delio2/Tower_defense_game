# Configurare un nuovo PC per sviluppare il gioco

Guida completa per ripartire da zero su un altro computer (Windows 10/11 64 bit).
Tempo stimato: circa 1–2 ore, soprattutto per i download di Unity.

---

## 1. Requisiti consigliati
- Windows 10 o 11 a 64 bit, **16 GB di RAM o più**, scheda video recente, **almeno 40 GB liberi** (Unity + moduli Android + progetto).
- Account: **Unity** (gratuito, per Unity Hub), **GitHub** (con accesso al repository), **Claude** (per Claude Code).

## 2. Programmi da installare (a mano, in quest'ordine)
| # | Programma | Versione | Dove | Note |
|---|---|---|---|---|
| 1 | **Git for Windows** | ultima | https://git-scm.com/download/win | Include **Git LFS** (serve per modelli, immagini, audio) |
| 2 | **Unity Hub** | ultima | https://unity.com/download | Accedi con il tuo account Unity |
| 3 | **Unity Editor 6000.6.2f1** | **esattamente questa** | Unity Hub → Installs → Install Editor (se non compare: *Archive* sul sito Unity) | Moduli da spuntare: **Android Build Support** con **OpenJDK** e **Android SDK & NDK Tools**. Facoltativi: Windows Build Support (IL2CPP), iOS, WebGL. **Togli la spunta** a "Microsoft Visual Studio Community" |
| 4 | **VS Code** | ultima | https://code.visualstudio.com | Estensione **"Unity"** di Microsoft (installa anche C# Dev Kit) |
| 5 | **.NET SDK 10** | 10.x | https://dotnet.microsoft.com/download | Serve all'estensione C# di VS Code |
| 6 | **uv** (con uvx) | ultima | `winget install astral-sh.uv` oppure https://docs.astral.sh/uv/ | Serve ai server MCP (Unity e Blender); scarica Python da solo quando serve |
| 7 | **Claude Code** | ultima | app desktop Claude (scheda Code) oppure CLI | Per lavorare con l'AI sul progetto |
| 8 | **Blender 5.2 LTS** | 5.2.x | https://www.blender.org/download/ | Facoltativo: solo per i modelli 3D. **Aprilo una volta** prima di installare l'addon MCP |

Dopo le installazioni **chiudi e riapri il terminale**, così PATH è aggiornato.

## 3. Scaricare il progetto
Struttura delle cartelle consigliata (la stessa del PC originale):
```
Progetti/
└── Giochi/                  ← cartella di lavoro (qui si apre Claude Code)
    ├── .mcp.json            ← configurazione MCP (da creare, vedi §5)
    ├── .claude/settings.json
    └── TowerDefense/        ← questo repository
```

Da PowerShell, nella cartella `Giochi`:
```powershell
git lfs install
git clone https://github.com/delio2/Tower_defense_game.git TowerDefense
```
Il primo accesso a GitHub apre una finestra di login (Git Credential Manager).

## 4. Aprire il progetto in Unity
1. **Unity Hub → Projects → Add → Add project from disk** → seleziona la cartella `TowerDefense`.
2. Aprilo con **Unity 6000.6.2f1**. La prima apertura rigenera la cartella `Library` e **richiede alcuni minuti**.
3. Il pacchetto **MCP for Unity** si scarica da solo, perché è elencato in `Packages/manifest.json` (serve Git nel PATH).
4. Se compare la finestra **"MCP Setup"** → **Skip** (Claude Code usa `.mcp.json`).
5. **Edit → Preferences → External Tools → External Script Editor → Visual Studio Code**.
6. Apri la scena **`Assets/Scenes/Prototype.unity`**.
7. Nella Game View scegli una risoluzione **verticale**: dal menu delle risoluzioni → **+** → *Fixed Resolution* 1080 × 1920.
8. Premi **Play** per provare il prototipo (tieni Unity in primo piano).

**Verifica che tutto funzioni:** Window → General → **Test Runner** → EditMode → **Run All** → devono passare tutti i test (17 a settembre 2026).

## 5. Collegare Claude Code (MCP)
1. Copia `TowerDefense/docs/setup/mcp.json.example` in `Giochi/.mcp.json`.
2. Trova il percorso di uvx sul nuovo PC:
   ```powershell
   (Get-Command uvx).Source
   ```
   e sostituiscilo nel campo `"command"` del server `blender` (con le barre doppie `\\`).
3. Copia `TowerDefense/docs/setup/claude-settings.json` in `Giochi/.claude/settings.json` (abilita da solo i server MCP del progetto).
4. **Unity:** apri il progetto → **Window → MCP for Unity** → trasporto **HTTP Local**, URL `http://127.0.0.1:8080` → **Start Server**.
5. **Blender** (facoltativo): apri Blender una volta, poi da terminale:
   ```powershell
   uvx mcp-for-blender install-addon
   ```
   In Blender: Edit → Preferences → Add-ons → attiva **"MCP for Blender"** → nella vista 3D premi **N** → scheda **MCP for Blender** → **Start MCP Server** (porta 9876). Attiva **Poly Haven**.
6. **Claude Code:** apri la sessione **nella cartella `Giochi`** (non dentro `TowerDefense`) e approva i server `unity` e `blender`.
   Avvia **prima** Unity e Blender, **poi** Claude Code (i server vengono letti all'avvio della sessione).

### La "memoria" di Claude
Le regole del progetto sono nel repository (`TowerDefense/CLAUDE.md` e `docs/`), quindi valgono anche sul nuovo PC.
Le note personali di Claude (memoria) invece sono salvate **solo sul PC originale**, in `%USERPROFILE%\.claude\projects\<cartella>\memory\`. Se vuoi portarle, copia quella cartella; non è indispensabile.

## 6. Lavorare ogni giorno
```powershell
cd Giochi\TowerDefense
git pull                 # prima di iniziare: prendi le ultime modifiche
# ... lavora ...
git add -A
git commit -m "Describe the change in English"
git push                 # a fine lavoro: salva online
```
- I messaggi di commit sono **in inglese** (regola del progetto); la chat con Claude è in italiano.
- Modelli, immagini e audio passano automaticamente da **Git LFS** (vedi `.gitattributes`).

## 7. Provare su un telefono Android (quando serve)
1. Sul telefono: Impostazioni → Info → tocca 7 volte "Numero build" → Opzioni sviluppatore → **Debug USB** attivo.
2. Collegalo con il cavo USB e accetta l'autorizzazione.
3. Unity: **File → Build Profiles → Android → Switch Platform** (la prima volta richiede tempo) → **Build And Run**.
   Le impostazioni Android sono già nel progetto: verticale, Android 8+ minimo, target Android 16, IL2CPP ARM64, pacchetto provvisorio `com.d3lioss.towerdefense`.

## 8. Problemi comuni
| Problema | Soluzione |
|---|---|
| Claude dice "unity: ConnectionRefused" | Unity chiuso o server MCP non avviato → Start Server, poi riapri la sessione di Claude |
| `localhost` non funziona | Usa `127.0.0.1` (su Windows localhost può andare su IPv6) |
| `uvx` non trovato | Chiudi e riapri il terminale; controlla con `(Get-Command uvx).Source` |
| L'installer dell'addon Blender non trova la cartella | Apri Blender almeno una volta, poi ripeti il comando |
| Unity chiede un'altra versione | Installa esattamente **6000.6.2f1** (vedi `ProjectSettings/ProjectVersion.txt`) |
| File grandi mancanti dopo il clone (immagini "vuote") | `git lfs install` e poi `git lfs pull` |
| Gli screenshot via MCP finiscono in `Assets` | Usa sempre la cartella `Temp/Screenshots` (regola in `CLAUDE.md`) |
