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
- **Grafica calma di default** (docs/03 §A1): niente lampeggi né flash a tutto schermo, nessun effetto ripetuto più di 2 volte al secondo nello stesso punto, movimenti morbidi con dissolvenze. Minimal ma attraente, mai agitato.
- Screenshot via MCP sempre con `output_folder` = `Temp/Screenshots` (mai dentro `Assets`).

## Convenzioni
- Cartelle: `Assets/Scripts`, `Prefabs`, `Scenes`, `Models`, `Materials`, `Textures`, `Audio`, `UI`.
- C#: PascalCase per classi/metodi, `_camelCase` per campi privati, `[SerializeField] private` invece di campi pubblici.
- Dati di bilanciamento (moduli, nemici, ondate) in ScriptableObject nel layer `Presentation`, che li carica nel `ContentDatabase` della simulazione (l'assembly `Simulation` non può usare tipi Unity). Il prototipo usa ancora i default in `ContentDatabase.CreatePrototypeDefaults()`: quando arrivano gli SO, quel metodo resta solo per i test.
- Input: nuovo Input System, pensato per il touch.
- Git: repo in questa cartella, Git LFS per i binari. Commit solo quando l'utente lo chiede.

## Game design
Documenti in `docs/` (leggili prima di lavorare su gameplay, UI, audio o monetizzazione):
- `docs/00-ricerca-mercato.md` — dati e concorrenti (+ Ricerca v2: Balatro, The Tower, multiplayer asincrono, diritto d'autore)
- `docs/01-concept.md` — **concept v2**: Core al centro + anello di moduli con combo + multiplayer asincrono
- `docs/02-monetizzazione-marketing.md` — modelli A (prova + sblocco) / B (F2P onesto); mai pay-to-win
- `docs/03-grafica-audio.md` — "Dusk Garden": calmo, minimal, **diverso da The Tower**; audio solo atmosfera
- `docs/04-decisioni.md` — registro D1–D21 (le superate sono marcate; svolta in D17)
- `docs/05-gdd.md` — **GDD v0.2**: la fonte di verità per meccaniche, numeri e architettura
- `docs/06-piano-sviluppo.md` — **piano a fasi** con gate: dice cosa fare adesso (§4) e cosa aspetta la fase dopo
- `docs/archivio/` — concept v1 musicale abbandonato: **non usarlo come riferimento**

Vincoli chiave: single player offline completo + multiplayer **solo asincrono** (replay), competitivo a dotazione fissa,
niente energia/timer/pubblicità forzate/loot box, pubblico adulto (mai stile infantile).
**Simulazione:** deterministica, tick fisso 60/s, interi o virgola fissa, niente `Math.Pow`/trigonometria float nello stato,
RNG a flussi separati; grafica e audio la seguono. Ogni run è un replay verificabile (versione + seme + comandi).
**Anti-copia (D20):** mai il nome "The Tower" (né altri giochi) in codice, asset, store o marketing; stile e UI diversi.
Terminologia: glossario nel GDD §0 (Core, Ring, Module, Merge, Credits, Blueprints, Pulse, Act, Guardian).
