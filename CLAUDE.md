# TowerDefense

- Lingua: **chat con l'utente in italiano**; **tutto il codice in inglese**: nomi (classi, metodi, variabili, file, cartelle), commenti, messaggi di log/errore e messaggi di commit.
- Non installare software sul PC: proponi cosa installare, l'utente lo fa a mano.

## Progetto
- **Unity:** 6000.6.2f1, Universal 3D (URP 17.6), C#.
- **Piattaforma:** mobile Android (poi iOS). **Orientamento verticale**, una mano (D2). minSdk 26, targetSdk 36 (D4).
- **3D:** Blender 5.2 LTS. **Editor codice:** VS Code + estensione Unity.
- **Istanza MCP Unity:** `TowerDefense@05fb0f4e1489f906` (l'hash cambia se il progetto viene spostato).

## MCP (configurati in `../.mcp.json`)
- **unity** → MCP for Unity (CoplayDev), `http://127.0.0.1:8080/mcp`. Richiede Unity aperto con il server avviato.
  - Se sono aperti più progetti Unity, leggi `mcpforunity://instances` e usa `set_active_instance` su TowerDefense.
  - Per le API Unity verifica con `unity_reflect` / `unity_docs` invece di andare a memoria.
- **blender** → MCP for Blender, porta 9876. Richiede Blender aperto con "Start MCP Server".
  - Prima di operazioni distruttive con `execute_blender_code`, chiedi all'utente di salvare il .blend.

## Flusso di lavoro
- Codice C#: scrivi i file in `Assets/Scripts/`; dopo ogni modifica controlla `read_console` finché non ci sono errori di compilazione.
- Scene, prefab, componenti, play mode, screenshot: via MCP Unity.
- Dopo modifiche importanti fai uno screenshot (Unity o Blender) e verifica il risultato.
- Blender → Unity: esporta FBX o glTF in `Assets/Models/<categoria>/`; 1 unità = 1 m, "Apply Transform" su FBX.
- Mobile first: low-poly, pochi materiali, texture ≤ 1024 px salvo motivo, draw call basse.

## Convenzioni
- Cartelle: `Assets/Scripts`, `Prefabs`, `Scenes`, `Models`, `Materials`, `Textures`, `Audio`, `UI`.
- C#: PascalCase per classi/metodi, `_camelCase` per campi privati, `[SerializeField] private` invece di campi pubblici.
- Dati di bilanciamento (torri, nemici, ondate) in ScriptableObject, non hardcodati.
- Input: nuovo Input System, pensato per il touch.
- Git: repo in questa cartella, Git LFS per i binari. Commit solo quando l'utente lo chiede.

## Game design
Documenti in `docs/` (leggili prima di lavorare su gameplay, UI, audio o monetizzazione):
- `docs/00-ricerca-mercato.md` — dati e analisi concorrenti
- `docs/01-concept.md` — concept: TD roguelite minimal, "la tua difesa compone una traccia tech-house"
- `docs/02-monetizzazione-marketing.md` — "paghi per arrivare prima, mai per vincere"
- `docs/03-grafica-audio.md` — neon minimal su fondo scuro + musica adattiva a strati
- `docs/04-decisioni.md` — registro decisioni (verticale, minSdk 26/target 36, offline-first + PGS v2, simulazione deterministica…)
- `docs/05-gdd.md` — **GDD v0.1**: la fonte di verità per meccaniche, numeri e architettura

Vincoli chiave: single player first (offline), modalità competitive con dotazione uguale per tutti,
niente energia/timer/interstitial/loot box, pubblico adulto (mai stile infantile).
**Orologio:** comanda la simulazione deterministica (passo fisso, 96 tick per battito, interi o virgola fissa, RNG a flussi separati);
audio e grafica la seguono (l'audio è agganciato a `AudioSettings.dspTime` solo a 1x). Mai usare `dspTime` per il gameplay.
Terminologia: vedi il glossario nel GDD §0 (Drop ≠ Peak, Vinili ≠ Royalties, Shred ≠ Exposed).
