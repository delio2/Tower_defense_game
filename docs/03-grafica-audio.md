# 03 — Direzione artistica e audio (v2)

> **Sensazione da trasmettere:** un giardino geometrico al tramonto. Calmo, morbido, elegante; i numeri crescono ma lo schermo resta sereno.
> **Regola anti-copia (D20):** l'identità deve essere **chiaramente diversa** da The Tower (neon su nero, torre quadrata, cerchio di portata).

---

## PARTE A — Grafica

### 1. Principi (regole fisse)
1. **Calmo di default:**
   - niente lampeggi, nessun flash a tutto schermo, nessun effetto ripetuto più di **2 volte al secondo** nello stesso punto (il limite delle linee guida sulla fotosensibilità è 3);
   - movimenti morbidi con easing e dissolvenze, mai scatti on/off;
   - i colpi rapidi si mostrano come **linee stabili e tenui**.
2. **Leggibilità:** in mezzo secondo si capisce cosa è tuo, cosa è nemico e cosa è pericoloso.
3. **Minimal ma attraente:** poche forme, curate. Volume morbido, luce gentile, gradienti; non colori piatti "da prototipo".
4. **Elegante, non infantile:** niente facce, niente colori da cartone.

### 2. Identità visiva: "Dusk Garden"
| Elemento | Scelta | Diversa da The Tower perché |
|---|---|---|
| Sfondo | **Gradiente blu-viola crepuscolare** con vignettatura morbida e anelli concentrici appena visibili | loro: nero pieno |
| Core | **Seme o fiore geometrico**: una sfera morbida con **6 petali (fino a 8)**, che sono gli slot dell'anello | loro: torre quadrata |
| Moduli | Piccoli oggetti arrotondati appoggiati sui petali, ognuno con una **silhouette propria** (Emitter = gemma, Amplifier = anello, Bank = moneta…) | loro: statistiche in un pannello, non oggetti |
| Nemici | **Frammenti angolari** (schegge, prismi) color corallo e rosa, con un movimento fluttuante lento | loro: quadrati e forme piatte |
| Camera | 3D ortografica **inclinata di circa 35°**: dà volume e ombre morbide | loro: 2D dall'alto |
| Interfaccia | **Carte offerta** morbide in basso, pulsante del Pulse circolare, tipografia arrotondata | loro: schede e liste di upgrade |

### 3. Tavolozza (da validare nella mood shot)
| Ruolo | Colore | Hex |
|---|---|---|
| Sfondo (alto → basso) | indaco → blu notte | `#2B2F5A` → `#141630` |
| Anelli dell'arena | lavanda tenue | `#3B3F72` |
| Core e petali | avorio caldo | `#F3E9D7` |
| Armi | acqua / teal | `#5CC8C0` |
| Booster | oro morbido | `#E9B872` |
| Economia | menta | `#9AD9A1` |
| Nemici | corallo | `#F07A6A` |
| Élite e Guardian | rosa profondo | `#C8507A` |
| Testo | avorio | `#F3E9D7` |

- **Daltonismo:** il colore è sempre abbinato a una **forma** (rotondo = tuo, angolare = nemico). Tavolozze alternative nelle opzioni.
- **Regola fissa:** le tavolozze stagionali e i temi estetici non usano mai la tinta dei nemici su Core e moduli.

### 4. Resa
- **URP:** materiali semplici con illuminazione morbida (una luce direzionale calda più una ambientale fredda), **ombre morbide** a qualità media e alta, bloom **leggero** solo su colpi e Pulse.
- **Movimento di fondo: minimo.** Il Core "respira" lentamente; **niente rotazioni continue** di sfondo o anello (la lezione della nausea da movimento di Balatro, parte C §2). I nemici si muovono solo perché avanzano, con una rotazione lenta su se stessi. Tutto il movimento decorativo si spegne con "riduci movimento".
- **Livelli di qualità:** ombre e bloom disattivabili; 30 fps garantiti sui telefoni da 4 GB (D4).

### 5. Effetti e numeri
- **Colpi:** linee sottili e tenui (arma → bersaglio), che durano 0,2–0,3 s con dissolvenza.
- **Pulse:** un'onda circolare morbida che si allarga dal Core, con un leggero aumento di luminosità in dissolvenza. Niente flash bianco.
- **Merge:** i due moduli si avvicinano e si fondono con un bagliore morbido. È il momento "soddisfacente" per eccellenza.
- **Combo attive:** linee morbide dorate tra i vicini, visibili nel negozio.
- **Numeri:** piccoli, tenui, font arrotondato, notazione compatta (1,2K · 3,4M). Più grandi (senza flash) solo per i colpi importanti. Si possono nascondere.
- **Morte dei nemici:** i frammenti si sciolgono in piccole scaglie che svaniscono.

### 6. Accessibilità
- Elenco completo delle opzioni nella parte C §7 (riduci movimento, intensità degli effetti, numeri, velocità, vibrazioni, testo, daltonismo, contrasto).
- Il gioco è pienamente giocabile senza audio (D1).

### 7. Produzione con l'AI
- **Blender MCP:** forme semplici e arrotondate modellate via script (petali, gemme, schegge).
- **Unity MCP:** materiali, luce, post-processing, prefab.
- Icone e grafica per lo store: generatore di immagini **dentro la guida di stile** (tavolozza e forme sopra).

### 8. Prossimo passo: la "mood shot"
Una sola schermata curata (Core con i petali, 3 moduli, qualche nemico, carte del negozio), da far vedere a qualcuno prima di produrre altra grafica.

---

## PARTE C — Esperienza tattile e interazione ("Tactile Calm")

> **Obiettivo:** il gioco deve essere **piacevole da toccare**, come Balatro e Marvel Snap, ma **senza agitare**.
> Ogni tocco ha una risposta ricca e immediata; niente si muove quando non serve.

### 1. Cosa amano i giocatori (ricerca, settembre 2026)
| Gioco | Cosa piace | Cosa prendiamo |
|---|---|---|
| **Balatro** | "Juice" stratificato: la carta si solleva al passaggio (circa +12 px, scala 1,05), la **trascini con inerzia fisica** e con un effetto calamita, i **numeri scorrono** cifra per cifra con un suono che sale di tono, i jolly si attivano **uno alla volta** con il totale che si aggiorna | Moduli che si sollevano, trascinamento con calamita, **conteggio progressivo** dei danni, attivazione in sequenza delle combo |
| **Marvel Snap** | **Vibrazioni curate** (un tocco leggero per le azioni piccole, un impatto per quelle importanti), sincronizzate con animazione e suono; interfaccia pulita con i comandi **in basso**, dove arriva il pollice; pannelli in "vetro scuro" con luce | Vibrazioni a livelli, interfaccia in basso, pannelli in vetro morbido |
| **Mini Metro / Mini Motorways** | Minimal senza disordine; trascinare produce **suoni musicali morbidi**; tutto chiaro e rilassante | Suoni morbidi durante il trascinamento, zero elementi inutili |
| **Backpack Battles** (PC) | Il negozio come "sistemare oggetti", soddisfacente | Il piacere di disporre i moduli sull'anello |
| **Tendenza 2026: "tactile minimalism"** | Superfici che sembrano avere peso e materia, composizioni semplici, micro-interazioni reattive | Moduli e carte con volume, ombre morbide, pulsanti che "si premono" |

### 2. Cosa sbagliano (da evitare)
| Gioco | Problema | Nostra regola |
|---|---|---|
| **Balatro** | **Nausea da movimento** per lo sfondo che vortica, le carte che ondeggiano sempre e l'effetto CRT; opzioni per ridurli arrivate tardi | Sfondo **fermo**; nessun ondeggiamento continuo; opzione **"riduci movimento"** dal primo giorno |
| **Balatro** | Punteggio lento senza modo di saltarlo (i giocatori hanno usato le mod) | **Velocità delle animazioni** 1x/2x/istantanea, tocco per saltare il conteggio |
| **Vampire Survivors** | **Disordine visivo** a fine partita: non si vede più niente, e nessuna opzione di trasparenza | **Intensità degli effetti** regolabile + riduzione automatica quando ci sono molti nemici |
| **Backpack Battles mobile** | Il **dito copre il testo**; trascinamento impreciso; oggetti spinti fuori posto; build rovinate | Oggetto trascinato **sopra il dito**, informazioni **in alto, mai sotto il dito**, calamita sugli slot, **mai spostamenti automatici**, **annulla** gratuito |
| **The Tower** | Menu di potenziamenti affollati, testo piccolo, troppi numeri | Poche informazioni per volta, testo grande, dettagli solo a richiesta |

### 3. Le regole del trascinamento (fondamentale su mobile)
1. **Prendere:** tocchi e trascini una carta del negozio o un modulo dell'anello. L'oggetto **si solleva** (scala 1,1, ombra più ampia) con una **vibrazione leggera**.
2. **Visibile sopra il dito:** l'oggetto segue il dito **spostato verso l'alto** (circa 1 cm), così non viene mai coperto.
3. **Guida:** gli slot validi si illuminano piano; avvicinandosi a uno slot scatta la **calamita** (l'oggetto "cade" nello slot), con un piccolo tick di vibrazione.
4. **Anteprima prima di confermare:** in alto compare l'effetto, per esempio *"+42% danni al secondo"* o *"Livello 2 → ×1,8"*, e le linee delle combo appaiono in anteprima. **Il giocatore sa cosa succede prima di lasciare.**
5. **Lasciare:** assestamento elastico **morbido** (rimbalzo massimo 5%, 150–200 ms) con una vibrazione media. Se lasci in un punto non valido, l'oggetto **torna al suo posto** fluttuando, senza flash d'errore.
6. **Vendere:** trascina un modulo nella **zona "Vendi"** in basso, che mostra il ricavo.
7. **Annulla:** un pulsante annulla l'ultimo acquisto, vendita o spostamento del negozio. Il rilancio **non** si annulla, altrimenti si potrebbero vedere le offerte future gratis.
8. **Alternativa senza trascinamento:** tocca la carta e poi tocca lo slot (accessibilità).

### 4. Momenti "soddisfacenti" (juice calmo)
| Momento | Risposta |
|---|---|
| **Merge** | I due moduli si attirano, si fondono con un bagliore morbido e un leggero rigonfiamento (scala 1,15 → 1); suono cristallino che sale di tono; vibrazione "successo" (due tocchi) |
| **Combo attiva** | Le linee dorate tra i vicini si accendono piano quando piazzi un booster |
| **Fine ondata** | **Riepilogo con conteggio progressivo**: danni dell'ondata che scorrono (0,6–1,2 s), interesse che si aggiunge moneta per moneta, tono che sale. Si salta con un tocco |
| **Pulse** | Onda circolare morbida, vibrazione media, suono profondo |
| **Nucleo colpito** | Leggera tinta corallo che sfuma, vibrazione breve e debole. **Niente scossa dello schermo** di default |
| **Numeri grandi** | Crescono di scala senza lampeggiare; notazione compatta |

### 5. Vibrazioni (haptics)
- **Pochi livelli, tutti brevi:** leggera (circa 15 ms, ampiezza bassa: prendere, calamita), media (circa 25 ms: piazzare, Pulse), successo (due tocchi: merge, vittoria). Mai vibrazioni lunghe o continue.
- Su Android si usa `VibrationEffect` con ampiezza (disponibile da Android 8, il nostro minimo); se il telefono non controlla l'ampiezza si usano durate più brevi.
- **Disattivabili** nelle opzioni (e rispettano l'impostazione del sistema).

### 6. Controllo del disordine (per i "numeri che esplodono")
- Tetto agli effetti simultanei; i colpi dello stesso modulo **non** generano più linee sovrapposte.
- Numeri **solo per le uccisioni** e i colpi importanti; un riepilogo a fine ondata per il resto.
- Con molti nemici gli effetti si **attenuano automaticamente**.

### 7. Opzioni (richieste più frequenti dalle community)
- **Riduci movimento** (niente respiro, niente scie, niente zoom animati), **intensità degli effetti** (0–100%), **numeri dei danni** (tutti / solo grandi / nessuno), **velocità delle animazioni** (1x / 2x / istantanea), **vibrazioni** (on/off), **dimensione del testo** (100–200%), **daltonismo** (tre preset + forme), **alto contrasto**.
- Le opzioni principali sono **proposte al primo avvio** o raggiungibili in un tocco dalla pausa.

### 8. Layout per il pollice
- Comandi e carte nel **terzo inferiore** dello schermo (circa il 75% dei tocchi è col pollice).
- Nel negozio la camera **avvicina e abbassa** l'anello verso la zona del pollice; durante l'ondata mostra tutta l'arena.
- Bersagli toccabili di **almeno 48 dp**.

---

## PARTE B — Audio

### 1. Ruolo dell'audio
La musica **non è più una meccanica** (D17): è **atmosfera**. Nessuna informazione di gioco solo sonora (D1).

### 2. Colonna sonora
- **Ambient o elettronica calma** (lo-fi / downtempo / ambient), in linea con il "giardino al tramonto".
- **Adattiva a strati:** più strati quando l'ondata si fa intensa, pochi nel negozio (rilassante), uno strato più teso per il Guardian.
- Si **abbassa** durante il negozio per favorire la concentrazione.

### 3. Effetti sonori
- Morbidi e **gradevoli**: colpi ovattati, un suono cristallino per il merge, un "respiro" profondo per il Pulse, un tintinnio leggero per i Credits.
- **Suoni dell'interazione** (lezione di Mini Motorways e Balatro): prendere e trascinare un modulo produce note morbide intonate; la calamita fa un "clic" caldo; il conteggio di fine ondata **sale di tono** a ogni scatto.
- Mai stridenti, mai ripetitivi in modo fastidioso: variazioni casuali di tono e un limite ai suoni simultanei.

### 4. Produzione e diritti ⚠️
| Strada | Pro | Contro |
|---|---|---|
| **AI (es. Suno Pro/Premier)** | Veloce, economica | Diritti commerciali solo per le tracce create con abbonamento attivo; non sei proprietario delle tracce; cause ancora aperte (UMG, Sony) → **rischioso per la colonna sonora ufficiale** |
| **Musica e loop royalty-free** | Licenze chiare | Serve controllare che la licenza copra l'uso nei giochi |
| **Compositore** | Identità e proprietà chiara | Costo |

**Scelta consigliata:** AI o loop gratuiti per il prototipo; loop con licenza chiara o un compositore per la versione finale. Effetti sonori (ElevenLabs SFX, fal.ai tramite Unity MCP) **verificando la licenza commerciale**.

## Fonti
- Linee guida fotosensibilità (WCAG 2.3.1, tre flash): https://www.w3.org/WAI/WCAG21/Understanding/three-flashes-or-below-threshold.html
- Tetris c. Xio (look and feel): https://en.wikipedia.org/wiki/Tetris_Holding,_LLC_v._Xio_Interactive,_Inc.
- Suno e diritti 2026: https://blog.dubspot.com/ai-music-licensing-explained-2026 · https://www.digitalmusicnews.com/2025/12/22/suno-warner-music-deal-changes/
- Musica adattiva: https://en.wikipedia.org/wiki/Adaptive_music
- Arte minimal: https://pixune.com/blog/minimalist-game-art-guide/
- Balatro, juice e interazione: https://blakecrosley.com/guides/design/balatro · https://medium.com/@yyh19971004/balatro-design-analysis-visual-packaging-and-interactive-feedback-cc6fa6a65370 · https://80.lv/articles/balatro-s-card-movements-shaders-recreated-in-unity
- Balatro, nausea da movimento: https://steamcommunity.com/app/2379780/discussions/0/4346606879508745169/ · https://www.getdroidtips.com/balatro-motion-sickness-while-playing/
- Balatro, richiesta di velocità: https://steamcommunity.com/app/2379780/discussions/0/4201364524144843418/
- Marvel Snap, vibrazioni e UI: https://www.xda-developers.com/marvel-snap-mobile-game-haptics/ · https://medium.com/design-bootcamp/marvels-snap-ui-ux-case-study-9f727d8f3875 · https://www.artstation.com/artwork/GemNDd
- Mini Motorways, suono e interazione: https://www.gamedeveloper.com/audio/-i-mini-motorways-i-and-the-delicate-art-of-marrying-complexity-and-minimalism
- Backpack Battles mobile, critiche ai controlli: https://apps.apple.com/us/app/backpack-battles/id6572290447?see-all=reviews&platform=iphone
- Vampire Survivors, disordine visivo: https://steamcommunity.com/app/1794680/discussions/0/4631482569784862581/
- Tendenze UI 2026 (tactile minimalism): https://aaagameartstudio.com/blog/mobile-games-art · https://pixune.com/blog/best-examples-mobile-game-ui-design/
- Drag and drop su touch: https://smart-interface-design-patterns.com/articles/drag-and-drop-ux/ · https://inkbotdesign.com/mobile-ux/
- Haptics Android: https://developer.android.com/develop/ui/views/haptics/haptics-principles · https://developer.android.com/develop/ui/views/haptics/haptics-apis
- Game feel: https://www.gameanalytics.com/blog/squeezing-more-juice-out-of-your-game-design
- Accessibilità: https://gameaccessibilityguidelines.com/basic/ · https://caniplaythat.com/2020/01/29/color-blindness-accessibility-guide/
