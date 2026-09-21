# 03 — Direzione artistica: grafica minimal e colonna sonora tech/house

> **Sensazione da trasmettere:** un club di notte visto dall'alto, pulito, elegante e ipnotico.
> Forme semplici che **pulsano a tempo**, luce al neon su fondo scuro, musica house/tech che cresce con la tua difesa.
> Riferimenti di spirito, non da copiare: Mini Metro (eleganza), Thronefall (minimal), Tetris Effect (audio e video sincronizzati), Geometry Wars (neon), Infinitode 2 (leggibilità).

---

## PARTE A — Grafica

### 1. Principi
1. **Leggibilità prima di tutto**: sul telefono si deve capire in mezzo secondo cosa è tuo, cosa è nemico e cosa è pericoloso.
2. **Ogni cosa pulsa a tempo**: il ritmo si **vede**, e questo rende il gioco giocabile anche senza audio.
3. **Pochi elementi, molta luce**: niente texture, solo forme, colore ed emissione (bloom).
4. **Elegante, non infantile**: geometria pulita e contrasti netti, niente faccine né colori "da cartone".

### 2. Linguaggio delle forme
| Elemento | Forma | Perché |
|---|---|---|
| Torri (giocatore) | **cerchi, esagoni, forme morbide** | armonia, "suono" |
| Nemici ("rumore") | **triangoli, forme spigolose e frastagliate** | pericolo, distorsione |
| Boss | forme composte che ruotano o si ripetono | "loop" |
| Percorso | linee sottili luminose sulla griglia | come la "pista" di un mixer |
| Livello della torre | **anelli concentrici** attorno alla torre | si legge senza numeri |

### 3. Tavolozza di base (da validare nel prototipo)
| Ruolo | Colore | Hex |
|---|---|---|
| Sfondo | blu notte profondo | `#0B0F1A` |
| Griglia | grigio-blu tenue | `#1C2333` |
| Giocatore / torri | ciano elettrico | `#2DE2E6` |
| Nemici | magenta / rosso neon | `#FF2E63` |
| Valuta / premi | oro caldo | `#FFC857` |
| Drop / energia | viola | `#9D4EDD` |
| Testo / UI | bianco caldo | `#F2F2F2` |

- **Ogni settore cambia leggermente l'accento**: Deep House più caldo e blu, Tech House più ciano, Techno più freddo e bianco. La difficoltà si "sente" e si vede.
- **Regola fissa:** gli accenti di settore e le tavolozze stagionali **non usano mai la tinta dei nemici** (magenta/rosso) su torri o interfaccia.
- **Daltonismo**: colore sempre abbinato a **forma** (tondo = tuo, triangolo = nemico). Tavolozze alternative nelle opzioni.

### 4. Camera e resa
- **3D low-poly con camera ortografica dall'alto**, leggermente inclinata per dare profondità. Si usano Blender e Unity URP, ma resta "piatto" e leggibile.
- Alternativa da valutare nel prototipo: 2D puro. Il vantaggio del 3D ortografico è l'**emissione con bloom** e la **profondità** nei video (volumi, rotazioni), non l'illuminazione: i materiali sono unlit.
- **URP**: materiali unlit/emissivi, **bloom** moderato, niente ombre in tempo reale sui livelli di qualità bassi.
- **Il pulsare a tempo**: scala ed emissione guidate dall'orologio della musica (vedi Parte B §4). Kick = "respiro" dell'intera griglia; Hi-hat = scintille piccole; Bass = onda pesante.

### 5. Effetti ed esplosioni
- Particelle **poche e geometriche**: frammenti di triangoli, anelli che si espandono, linee.
- Il **Drop**: un flash controllato, un'onda circolare e la griglia che si illumina a ondate, **sincronizzata col drop audio**.
- Morte dei nemici a tempo: le esplosioni aspettano la suddivisione di battito più vicina. È molto soddisfacente e **perfetto per i video**.

### 6. Interfaccia
- Piatta, minimal, **pulsanti grandi** (almeno circa 48 dp), tutto raggiungibile con il pollice in verticale.
- Informazioni sempre visibili: ondata / BPM / vite / valuta. Anteprima dell'ondata in alto.
- Tooltip con **danno reale** della torre e sinergie attive, evidenziate con linee tra le torri.

### 7. Accessibilità e sicurezza
- **Opzione "riduci flash ed effetti"** (fotosensibilità). Il tutorial "Soundcheck" usa **già effetti ridotti** (nessun flash forte), e l'opzione viene proposta **subito dopo il tutorial**. Così il primo avvio resta senza menu (D8).
- Opzione per ridurre il bloom e il movimento della camera.
- Il gioco è pienamente giocabile senza audio.

### 8. Prestazioni (telefono economico come riferimento)
- 60 fps su un telefono di fascia media, 30 fps garantiti su quelli economici. Livelli di qualità: bloom sì/no, numero di particelle, risoluzione di rendering.
- GPU instancing per torri e nemici; pochi materiali condivisi.

### 9. Produzione con l'AI
- **Blender MCP**: le forme sono primitive semplici, quindi le modelliamo direttamente via script (esagoni, anelli, triangoli). Non serve generare modelli complessi.
- **Unity MCP**: materiali, shader emissivi, post-processing e prefab.
- Icone, key art e screenshot per lo store: generatore di immagini (per esempio fal.ai tramite Unity MCP) **dentro una guida di stile fissa** (tavolozza e forme sopra).

---

## PARTE B — Colonna sonora e audio

### 1. Genere e struttura
| Settore | Stile | BPM | Carattere |
|---|---|---|---|
| 1 | **Deep House** | 120 | caldo, rilassato, accordi morbidi |
| 2 | **Tech House** | 124 | groove, percussioni, basso più presente |
| 3 | **Techno** | 128 | ipnotico, più duro, tensione |

- Una **tonalità fissa per settore**, così tutto quello che suona è armonico.
- Struttura musicale = struttura di gioco: **Intro → Build-up → Peak (boss) → Breakdown**. Il **Drop** è l'abilità del giocatore, e musicalmente è il momento in cui "entra tutto".

### 2. Musica adattiva a strati (verticale)
- Una **base** (atmosfera e pad di sottofondo) suona sempre.
- **Ogni strumento-torre aggiunge il suo strato**: Kick, Hi-hat, Bass, Clap, Lead, Pad.
- Più torri dello stesso tipo **non** aumentano il volume: arricchiscono il pattern (varianti, ghost notes).
- **L'audio è una rappresentazione stilizzata**, non un suono per ogni colpo: i colpi di gioco seguono il GDD §6, lo strato musicale ne riflette tipo, numero e livello. La corrispondenza 1:1 colpo-per-colpo è **visiva** (lampo o pulsazione sulla torre).
- I **potenziamenti** passano a pattern più complessi (livello 1 = semplice, livello 3 = pieno).
- Il **nemico Silence** zittisce gli strati vicini: la musica si svuota e il pericolo si sente.
- **Build-up**: filtro passa-basso che si apre, riser, rullante che accelera. **Drop**: entra tutto.

### 3. Regole per non diventare caos (tecniche alla Mini Metro)
- **Quantizzazione**: ogni suono di evento (morti, esplosioni) scatta sulla **successiva** suddivisione di 1/16 (non si può suonare nel passato).
- **Tetto ai suoni simultanei** per strato; gli eventi in eccesso vengono distribuiti sui sedicesimi successivi.
- **Suoni dei colpi intonati** alla tonalità del settore.
- **Sidechain ("ducking")**: il Kick abbassa leggermente gli altri strati. È il "respiro" tipico della house, e corrisponde alla sinergia *Groove*.

### 4. Implementazione in Unity (appunti tecnici)
- **L'orologio che comanda è quello della simulazione** (passo fisso, battiti come eventi di simulazione, D11). Colpi, danni e pulsazioni visive dipendono da lui.
- L'audio **segue**: a velocità 1x i battiti di simulazione vengono mappati su `AudioSettings.dspTime` e gli strati sono programmati in anticipo con `AudioSource.PlayScheduled`, tutti sullo stesso inizio. Volume e filtri sono controllati dal gioco.
- A 2x/3x l'audio si stacca (resta al suo tempo, filtrato) e si **risincronizza alla battuta successiva** quando si torna a 1x (D12).
- ⚠️ Mai usare `dspTime` come orologio del gameplay: si rompono il determinismo, la pausa e la velocità 2x/3x.
- L'audio nativo di Unity basta per l'MVP. FMOD si valuta più avanti, solo se servono funzioni avanzate (verificare la licenza indie).

### 5. Effetti sonori
- Corti, puliti, **intonati**. Colpi delle torri = parte della musica; nemici = rumori "glitch" filtrati.
- Interfaccia: click morbidi, suoni di conferma sulla tonalità del settore.

### 6. Produzione della musica e diritti ⚠️
| Strada | Pro | Contro |
|---|---|---|
| **AI (es. Suno Pro/Premier)** | Veloce, economica | Diritti commerciali **solo per le tracce create con abbonamento attivo**; **non sei proprietario** delle tracce; UMG e Sony hanno ancora cause aperte con Suno; nuovi modelli su licenza nel 2026. **Rischioso per una colonna sonora ufficiale** |
| **Pacchetti di loop e campioni royalty-free** | Stem separati (perfetti per gli strati), licenze chiare | Serve montarli; controllare che la licenza copra l'uso nei giochi |
| **Produttore umano** (freelance) | Qualità, identità, **proprietà chiara**, colonna sonora pubblicabile | Costo |

**Scelta consigliata:**
1. **Prototipo**: AI e loop gratuiti per provare subito il sistema a strati.
2. **Versione finale**: **loop e campioni con licenza chiara** montati per strati, oppure **un produttore house/tech** con cessione dei diritti. Solo così la colonna sonora può uscire su Spotify e YouTube senza rischi.
3. Effetti sonori: ElevenLabs SFX o fal.ai (tramite Unity MCP), **verificando la licenza commerciale** del piano usato.

---

## Checklist per il prototipo
- [ ] Orologio di simulazione (BPM) con 2 torri a tempo e pulsazione visiva; audio agganciato via dspTime
- [ ] 3 strati musicali che entrano e escono con le torri
- [ ] Tavolozza e forme applicate con primitive semplici
- [ ] Bloom e opzione "riduci flash"
- [ ] Test: si capisce tutto **senza audio**? È soddisfacente **con** l'audio?

## Fonti
- Mini Metro, musica programmata (Disasterpeace): https://designingsound.org/2016/02/18/the-programmed-music-of-mini-metro-interview-with-rich-vreeland-disasterpeace/
- Postmortem Mini Metro audio: https://disasterpeace.com/blog/mini-metro.postmortem.html
- Musica adattiva: https://en.wikipedia.org/wiki/Adaptive_music · https://splice.com/blog/adaptive-music-video-games/
- Generative music framework: https://www.gamedeveloper.com/audio/deep-dive-generative-music-in-video-games
- Tetris Effect: https://en.wikipedia.org/wiki/Tetris_Effect
- Suno e diritti 2026: https://musicinafrica.net/magazine/suno-adjusts-ai-music-ownership-terms-after-warner-music-partnership/ · https://blog.dubspot.com/ai-music-licensing-explained-2026 · https://www.digitalmusicnews.com/2025/12/22/suno-warner-music-deal-changes/
- Arte minimal: https://pixune.com/blog/minimalist-game-art-guide/
- Tendenze arte casual 2026: https://aaagameartstudio.com/blog/casual-game-art
- Rhythm Towers (TD musicale PC/console): https://rhythmtowers.com/
