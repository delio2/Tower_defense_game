> ⚠️ **ARCHIVED on 2026-09-21 (D17):** concept v1 "musical maze defense", abandoned. **Not a reference.** Kept in its original Italian as project history; the current concept is in `../01-concept.md` and `../05-gdd.md`.

# 05 — Game Design Document (v0.1)

> **Titolo provvisorio:** TowerDefense · **Genere:** tower defense roguelite minimal con musica adattiva
> **Piattaforma:** Android (Google Play), poi iOS · **Orientamento:** verticale · **Pubblico:** adulti 18–45
> **Modello:** free-to-play onesto: *"paghi per arrivare prima, mai per vincere"*
>
> Tutti i numeri sono **valori di partenza v0**, da tarare con il prototipo e con le simulazioni automatiche (D11).
> Contesto e motivazioni: `00`–`04`. I nomi usati nel gioco (torri, nemici, sistemi) sono in inglese, come nel codice.

---

## 0. Glossario (per evitare ambiguità)
| Termine | Significato |
|---|---|
| **Drop** | l'abilità attiva del giocatore (§11). Non indica il boss |
| **Peak** | il boss di fine settore (il momento culminante del "set") |
| **Vinili** (Vinyls) | i modificatori della run scelti tra un settore e l'altro (§13). **Non** sono la valuta |
| **Royalties** | la valuta permanente (meta) guadagnata a fine run (§14) |
| **Crediti** | la valuta *dentro* la partita, con cui costruisci e potenzi (§10) |
| **Shred** | l'effetto dell'Hi-Hat: riduce la corazza |
| **Exposed** | la sinergia: più danno ai nemici con la corazza ridotta dallo Shred |
| **Studio** | l'albero della progressione permanente (§14) |
| **Jam Session** | la modalità sandbox musicale (aggiornamento futuro) |
| **Core** | la base da difendere, in fondo alla griglia (= l'uscita dei nemici) |
| **Élite** | la variante potenziata di un nemico (§8) |

## 1. Pilastri
1. **Strategia leggibile**: ogni decisione è informata (anteprima delle ondate, danno reale, portata visibile).
2. **Sempre diverso**: mappe generate, 1 potenziamento su 3, Vinili, Livelli di Pressione.
3. **Il ritmo si vede e si sente**: tutto pulsa a tempo; funziona muto, con l'audio è speciale (D1).
4. **Mai pay-to-win**: le modalità competitive hanno la stessa dotazione per tutti.
5. **Rispetto del tempo**: niente energia né timer, run divisibili, un tetto alla progressione.

## 2. Ciclo di gioco
| Livello | Durata | Ciclo |
|---|---|---|
| **Momento** | secondi | Piazza o potenzia → vedi e senti la torre entrare a tempo → i nemici esplodono sul battito |
| **Ondata** | circa 30 s | Preparazione (senza limite di tempo) → ondata → scegli 1 su 3 |
| **Settore** | circa 5–6 min | **6 ondate** → boss (**Peak**) → scegli un Vinile |
| **Run** | circa 17 min | 3 settori (Deep House → Tech House → Techno) → risultati → Royalties |
| **Meta** | settimane | Studio (potenziamenti), sblocco di strumenti, Livelli di Pressione, Daily Mix |

## 3. Comandi e interfaccia di gioco
- **Tocca una casella libera** → si apre un menu radiale con l'**Acoustic Panel** e le torri disponibili (costo e ruolo in icona) → tocca per costruire.
  Mentre scegli vedi in anteprima la **portata** e il **nuovo percorso** dei nemici (linea tratteggiata).
- **Tocca una torre** → scheda con danno reale al secondo, sinergie attive, potenziamento e (solo in preparazione) vendita.
- **Barra in basso** (zona del pollice): ▶ Avvia ondata / ⏩ 1x-2x-3x · ⏸ Pausa · ↶ Annulla (solo in preparazione) · 💥 Drop.
- **Barra in alto**: vite · crediti · ondata X/6 · BPM · **anteprima dell'ondata successiva** (icone dei nemici e quantità).
- Nessuno scorrimento: la griglia 9×14 sta tutta nello schermo. Le caselle sono di circa 48 dp o più.

## 4. Griglia e generazione delle mappe
- **Griglia 9 colonne × 14 righe.** I nemici entrano dall'alto (1–2 caselle di ingresso); il **Core** (la base da difendere) sta in basso.
- **Tipi di casella:** libera · **Roccia** (bloccata) · **Amplifier** (+20% danno alla torre sopra) · **Dead Zone** (non edificabile, ma percorribile).
- **Generatore (con seme):**
  1. posiziona ingressi e Core;
  2. posiziona 8–14 Rocce con un rumore controllato;
  3. aggiunge 2–3 Amplifier e 0–3 Dead Zone;
  4. **validatore**: percorso minimo iniziale di almeno **18 caselle** (su 14 righe un percorso dritto ne fa già 14: serve una deviazione reale) · almeno il 60% delle caselle edificabile · nessun Amplifier irraggiungibile · almeno 2 "strettoie" utili. Se la mappa non passa, cambia sottoseme e riprova.
- **Pathfinding:** flow field BFS dal Core, ricalcolato a ogni piazzamento. Un piazzamento che chiuderebbe il percorso viene **rifiutato** con un feedback visivo (D9).
- **Durante l'ondata** non si può costruire su una casella occupata da un nemico. Costruire può deviare i nemici già nel labirinto (costa crediti e non si può vendere: accettato come tattica).
- **Test automatico:** 10.000 semi generati e validati a ogni modifica del generatore.

## 5. Orologio musicale (il sistema centrale)
- **BPM per settore:** 120 / 124 / 128. **Risoluzione: 96 tick per battito** (copre 1/16, 1/32 e le terzine; D11).
- **Tempo di simulazione** a passo fisso: il battito è un evento *della simulazione*, ed è **l'orologio che comanda**. L'audio lo segue ed è programmato in anticipo su `AudioSettings.dspTime` (D11, D12).
- Il DPS delle torri cresce con il BPM (+3,3% a 124, +6,7% a 128). È **voluto** e rientra nel bilanciamento dei settori.
- **Torri:** sparano su suddivisioni specifiche (tabella §6).
- **Morti e esplosioni:** effetto visivo e sonoro quantizzati al sedicesimo successivo (al massimo circa 125 ms di ritardo a 120 BPM). Il danno resta immediato.
- **Velocità 2x e 3x:** la simulazione accelera, la musica resta al suo tempo e viene filtrata (D12).
- **Pausa:** simulazione ferma, musica sfumata.

## 6. Torri (strumenti) — v0
Valori al **livello 1** a 120 BPM (1 battito = 0,5 s). Portata in caselle.

| Torre | Costo | Ritmo | Danno/colpo | Portata | Effetto | DPS indicativo |
|---|---|---|---|---|---|---|
| **Kick** | 50 | ogni battito | 12 (area r1,5) | 1,5 | onda ad area | 24 per bersaglio |
| **Hi-Hat** | 40 | 1/16 | 2 | 2,5 | **Shred**: −1 corazza per colpo (max −5, 2 battiti) | 16 |
| **Bass** | 70 | "e" del 2 e del 4 | 45 | 3 | colpo singolo pesante | 45 |
| **Clap** | 60 | battiti 2 e 4 | 8 | 2 | **Stun** di ¼ battito (i boss sono immuni dopo 3) | 8 + controllo |
| **Pad** | 60 | continuo | — | aura 2 | **Slow** del 30% | — |
| **Lead** | 80 | ogni battito | 8 | 3 | **catena** su 3 bersagli, −20% a ogni rimbalzo | circa 39 totali (su 3 bersagli) |

- **Potenziamenti (D15): potenziare conviene più che costruire.**
  | Livello | Costo | Efficacia (rispetto al L1) | Resa per credito |
  |---|---|---|---|
  | L1 (costruzione) | 1,0× | 100% | 1,00 |
  | L2 | +0,6× | +80% → 180% | 1,33 per il potenziamento |
  | L3 (+ ramo) | +0,9× | +100% → 280% | 1,11 per il potenziamento |
  | **Totale L3** | **2,5×** | **280%** | **1,12** (una torre nuova rende 1,00) |

  Ogni livello porta anche **un pattern ritmico più ricco**. Così si premiano il labirinto compatto e le scelte dei rami, senza rendere inutili le torri nuove.
- **Al livello 3 scegli uno di 2 rami** (la profondità di Bloons, in forma più leggera):
  | Torre | Ramo A | Ramo B |
  |---|---|---|
  | Kick | "808" (area più grande, danno prolungato) | "Punch" (doppio colpo, più danno) |
  | Hi-Hat | "Open Hat" (Shred più forte) | "Roll" (1/32, più colpi) |
  | Bass | "Sub" (penetra la corazza) | "Wobble" (rallenta chi colpisce) |
  | Clap | "Snare Roll" (stun più frequente) | "Reverb Clap" (stun ad area) |
  | Pad | "Drone" (aura che fa danno) | "Freeze Pad" (slow del 50%) |
  | Lead | "Arp" (6 rimbalzi) | "Pluck" (1 bersaglio, danno critico) |
- **Sblocchi:** si parte con Kick, Hi-Hat, Bass e Clap. Pad si sblocca alla run 2, Lead vincendo il primo settore. Sweep, Vocal Chop e Sub Station arrivano dallo Studio o dalle stagioni.
- **Vendita:** 100% del valore, **solo in preparazione** (D9).
- **Acoustic Panel (D14):** muro economico, **costo 10**, nessun attacco. Blocca il percorso come una torre, quindi serve a costruire il labirinto fin dalla prima ondata.
  - Si può **trasformare in torre** pagando la differenza (per esempio Panel → Kick = 40). La torre prende il posto del pannello.
  - Stesse regole delle torri: vendita e annulla solo in preparazione, e mai un piazzamento che chiude il percorso.
  - Non conta come "tipo di torre" per le sinergie (Full Band, Solo) né per il Vinile Minimal.
  - Aspetto: una lastra scura bassa, **non pulsa** (non è uno strumento): si distingue a colpo d'occhio dalle torri.

## 7. Sinergie (armonia)
| Nome | Condizione | Effetto |
|---|---|---|
| **Groove** | Kick e Bass entro 2 caselle | il Bass fa +30%: "pompa" in levare dopo il Kick, come il sidechain. Il Bass spara in levare e il Kick sul battere, quindi non coincidono mai: la sinergia è sulla vicinanza |
| **Harmony** | Lead che colpisce nemici rallentati dal Pad | +2 rimbalzi |
| **Exposed** | qualsiasi torre contro nemici con corazza ridotta dallo Shred dell'Hi-Hat | +15% danno |
| **Backbeat** | Clap e Kick entro 2 caselle | lo stun del Clap dura il doppio |
| **Full Band** | almeno 5 tipi diversi di torre in campo | +10% danno globale |
| **Solo** | una sola torre di un tipo sulla mappa | quella torre fa +25% |

Le sinergie attive sono **mostrate con linee luminose tra le torri** e anche con un **suono** (lo strato musicale si arricchisce).

## 8. Nemici ("rumore") — v0
HP base = 30 (settore 1, ondata 1). Velocità in caselle al secondo. Corazza = riduzione fissa per colpo (minimo 1 danno).

| Nemico | HP × | Velocità | Corazza | Ricompensa | Speciale | Da |
|---|---|---|---|---|---|---|
| **Static** | 1,0 | 1,0 | 0 | 3 | — | S1 |
| **Noise** (sciame) | 0,3 | 1,4 | 0 | 1 | gruppi da 8 | S1 |
| **Glitch** | 0,7 | 1,2 | 0 | 4 | ogni 2 battute salta avanti di 1 casella | S1 |
| **Distortion** | 2,5 | 0,7 | 4 | 7 | corazzato: il Kick scende a 8 danni per colpo, il Bass resta forte (41). L'Hi-Hat toglie la corazza in circa mezzo battito e apre la strada a tutte le torri | S1 |
| **Feedback** | 1,5 | 0,9 | 0 | 5 | alla morte si divide in 2 Static da 0,4 HP | S2 |
| **Silence** | 1,8 | 0,8 | 1 | 8 | **zittisce** le torri entro 1,5 caselle, che si spengono (visivo e audio) | S2 |
| **Phase** | 1,2 | 1,0 | 0 | 6 | invulnerabile nei battiti dispari (1 e 3), e diventa **trasparente** in quei battiti (leggibile senza audio). Clap e Bass lo colpiscono sempre | S3 |
| **Boss: The Loop** | 40 | 0,5 | 2 | 50 | al 50% di HP torna indietro di 3 caselle ed evoca la composizione dell'ultima ondata al 30% | fine settore |

- **Élite:** variante di qualsiasi nemico base con HP ×3, corazza +1 e ricompensa ×3, riconoscibile da un **alone e un contorno doppio**. Compaiono dall'ondata 4 di ogni settore (1–2 per ondata) e più spesso con i Livelli di Pressione.
- **Vite:** 20. Un nemico che passa costa: normale 1, élite 2, boss 5. **Nessuna ricompensa per chi passa** (D10).

## 9. Ondate e difficoltà
- **6 ondate per settore + il boss (Peak)** (D3). Indice globale `g` = 1…18. Resta un **parametro** nei dati.
- **Moltiplicatore di HP:** `hp(g) = 1.148^(g-1)` → ondata 18 ≈ 10,5×.
- **Budget di punti per ondata:** `budget(g) = 12 · 1.21^(g-1)` → ondata 18 ≈ 25× l'ondata 1. Ogni nemico ha un costo in punti (Static 1, Noise 0,3, Glitch 1,2, Distortion 3, Feedback 2, Silence 3, Phase 2,5).
- **Verifica di massima delle formule** (calcolo a tavolino): in una run completa si guadagnano circa **10.900 crediti** in totale, mentre l'ondata 18 ha circa **96.000 HP** totali. Il rapporto crediti/HP resta quello della versione a 8 ondate (circa 0,11), quindi il settore 3 è ancora **impegnativo ma superabile**. Va confermato con il simulatore.
- **Regole di composizione:** massimo 3 tipi per ondata · un nemico nuovo appare prima **da solo**, così si legge · l'**ondata 3** di ogni settore è "a tema" (tutta corazzata, tutta sciame…) · l'ondata 6 prepara il Peak.
- Le ondate del settore sono **generate dal seme e mostrate in anteprima**: niente sorprese ingiuste.
- **Livelli di Pressione** (sblocco sequenziale, cumulativi):
  | Livello | Modificatore | Livello | Modificatore |
  |---|---|---|---|
  | 1 | nemici +10% HP | 6 | meno Amplifier sulla mappa |
  | 2 | crediti iniziali −20% | 7 | il boss ha uno scudo che si rigenera |
  | 3 | nemici +10% velocità | 8 | le scelte 1 su 3 diventano 1 su 2 |
  | 4 | Feedback e Silence già dal settore 1 | 9 | vite 15 |
  | 5 | élite in ogni ondata dispari | 10 | +1 nemico speciale per settore |

  Livelli 11–20: da definire dopo i test.

## 10. Economia della partita
- **Crediti iniziali:** 150.
- **Ricompensa:** quella del nemico × `(1 + 0.068·(g−1))` (→ ×2,16 all'ondata 18, come prima all'ondata 24).
- **Bonus a fine ondata:** `20 + 5·w` (w = ondata nel settore).
- **Interesse:** +5% dei crediti non spesi a fine ondata, **tetto 25**.
- **Chiama prima l'ondata** (mentre la precedente è ancora in corso, dopo che tutti i suoi nemici sono entrati): +10% del budget dell'ondata in crediti.
  Le **scelte 1 su 3** delle ondate finite si **accumulano** (icona con un numero) e si fanno alla prima preparazione utile. Nessuna finestra interrompe un'ondata in corso.
- **Obiettivo di bilanciamento:** con un gioco "buono" si arriva al boss del settore 3 con 3–8 vite perse. Con un gioco "medio" si perde tra i settori 2 e 3.

## 11. Il Drop (azione attiva)
- **Carica:** ogni uccisione riempie la barra in base alla sua ricompensa; è piena circa ogni 1,5 ondate.
- **Attivazione:** tocchi 💥 → parte un **build-up di 1 battuta** (riser audio, la griglia si illumina) → il Drop colpisce **sul battere della battuta successiva**.
- **Effetto:** 150 danni più l'8% degli HP massimi a tutti i nemici sullo schermo, e stun di 1 battito.
- **Perfect (facoltativo):** se durante il build-up tocchi di nuovo lo schermo sul battito (±90 ms), il danno sale del 25%. Un indicatore visivo mostra il battito. Opzione "Assistito": il Perfect è automatico e il danno sale del 10%.
  Nelle modalità competitive il Perfect assistito **non dà punti** (§15).
- Nel tutorial la barra è **caricata dal copione** (§16).

## 12. Scelta 1 su 3 dopo ogni ondata
- **Rarità:** Comune 70% · Rara 25% · Epica 5%. **Garanzia:** almeno una Rara ogni 4 scelte.
- 1 rilancio gratuito per run (più Remix via pubblicità o Pass, **tranne nel Daily Mix**).
- Le carte proposte vengono da un **flusso RNG dedicato** (D11): a parità di seme e di scelte, tutti vedono le stesse proposte.
- **Categorie ed esempi v0:**
  | Carta | Rarità | Effetto |
  |---|---|---|
  | Tight Kick | C | Kick +20% danno |
  | Swing | C | Hi-Hat: Shred dura +1 battito |
  | Deep Low End | C | Bass +1 portata |
  | Tip Jar | C | +40 crediti subito |
  | Rehearsal | C | la prossima torre costa −30% |
  | Crowd Energy | C | la barra del Drop si carica +20% più veloce |
  | Double Clap | R | il Clap spara anche sul battito 4 in levare |
  | Resonance | R | gli Amplifier danno +40% invece di +20% |
  | Encore Budget | R | bonus di fine ondata +50% |
  | Wide Stereo | R | tutte le aure +0,5 portata |
  | Polyrhythm | R | una torre a scelta spara anche in terzine |
  | Sidechain | R | la sinergia Groove vale +60% invece di +30% |
  | Headliner | E | la torre più potenziata fa ×2 danno |
  | Second Drop | E | il Drop si può usare 2 volte di seguito |
  | Key Change | E | tutte le torri +1 livello di pattern (senza pagare) |
  | Mixdown | E | +1 scelta in tutte le prossime decisioni 1 su 3 |

  Obiettivo per il lancio: circa 40 carte.

## 13. Vinili (reliquie tra un settore e l'altro)
Scegli 1 su 3 alla fine dei settori 1 e 2. Esempi v0:
| Vinile | Effetto |
|---|---|
| **White Label** | le carte Rare appaiono il doppio delle volte |
| **Minimal** | puoi usare al massimo **2 tipi** di torre, ma fanno +50% (un limite sul numero di torri romperebbe il labirinto) |
| **Maximal** | torri −25% di costo, −15% di danno |
| **Four to the Floor** | ogni 4 battiti tutte le torri sparano un colpo extra |
| **Afterparty** | +2 vite a ogni boss sconfitto |
| **Vinyl Crackle** | i nemici corazzati perdono 1 corazza per battito nella portata di un Hi-Hat |
| **Crate Digger** | +30% Royalties a fine run |
| **Remix Culture** | +2 rilanci per settore |
| **Bassline** | i Bass colpiscono anche in levare sugli altri battiti, con −40% di danno |
| **Silent Disco** | immunità agli effetti Silence |

## 14. Progressione permanente (Studio)
- **Valuta: Royalties.** Si guadagnano circa 14 per ondata superata, 50 per boss, +10% per ogni Livello di Pressione. Una run media dà circa 250 (una run vinta circa 400).
- **Studio: 4 rami × 10 nodi, con un tetto finale**:
  | Ramo | Contenuto |
  |---|---|
  | Rhythm Section | danno per tipo di torre, **massimo +15%** in totale |
  | Economy | crediti iniziali, interesse, bonus di fine ondata |
  | Tempo | carica ed effetto del Drop |
  | Crates | rilanci, probabilità di rarità, scelte extra di Vinili |
- **Costo totale:** circa 65.000 Royalties. Tempi *(stima)*: gratuito circa 3 mesi · Pass circa 6 settimane · Pass più pacchetti 2–3 settimane.
  Conto di verifica: 65.000 / 90 giorni ≈ 720 al giorno, cioè circa 1,5 run al giorno con Encore (×2). È coerente con 3 o più sessioni al giorno.
- **Principio:** lo Studio aggiunge al massimo circa +20% di potenza complessiva. **L'abilità deve contare di più.** Tutto lo Studio è **disattivato nel Daily Mix e nel Weekly Set**.
- **Maestria:** ogni torre ha 5 livelli di maestria (in base all'uso). Sblocca varianti estetiche e sonore e, a livello 5, un terzo ramo al L3.

## 15. Modalità
| Modalità | Regole | Classifica |
|---|---|---|
| **Run** | 3 settori, Studio attivo, Livelli di Pressione | record personale |
| **Daily Mix** | 1 settore (circa 6 min), **seme = data del "giorno PGS"** (le classifiche giornaliere di Play Games si azzerano a mezzanotte **UTC−7**: seme e classifica devono cambiare nello stesso momento), **dotazione fissa** (Acoustic Panel più tutte le 6 torri base, anche quelle non ancora sbloccate: fa da "assaggio"; niente Studio, niente pubblicità o Pass), tentativi illimitati, vale il migliore | PGS, giornaliera |
| **Weekly Set** | run da 3 settori con seme settimanale (cambia tra sabato e domenica, **UTC−7**, come la classifica settimanale PGS) e dotazione fissa | PGS, settimanale |
| *After Hours* (aggiornamento) | infinita | tutti i tempi |
| *Jam Session* (aggiornamento) | sandbox musicale, nessun nemico o nemici opzionali | — |

**Punteggio (modalità competitive):** `ondate superate × 1000 + vite rimaste × 50 + crediti a fine run ÷ 10 + Drop Perfect × 100`.
Il Perfect **assistito** non conta nel punteggio.

## 16. Primo avvio (FTUE) — copione
| Tempo | Evento |
|---|---|
| 0 s | Logo → **solo in UE/UK: modulo di consenso** (Google UMP, compare solo dove è obbligatorio; D16) → **settore guidato "Soundcheck"** (mappa fissa, 3 ondate, **effetti ridotti**: nessun flash forte). Nessun menu, nessun account |
| circa 5 s | Freccia su una casella: "Tocca" → costruisci un **Kick** → la griglia **pulsa** e parte il battito (**primo "aha"**) |
| circa 15 s | Ondata 1 (4 Static): esplodono a tempo |
| circa 35 s | "Metti un pannello qui" → un **Acoustic Panel** (costa pochissimo) **allunga il percorso** (vedi la linea che cambia), poi "Costruisci qui" → un Hi-Hat. Impari labirinto e torri giocando |
| circa 50 s | Ondata 2 → **prima scelta 1 su 3** (tutte e 3 le carte utili) |
| circa 75 s | Ondata 3 con un mini-boss → la barra del Drop è piena → "Tocca 💥" → **DROP** (**secondo "aha"**, entro 90 s) |
| circa 100 s | Risultati → prime Royalties → si sblocca il menu principale |
| dopo | Proposta "riduci flash" (fotosensibilità) → prima run vera. Gli altri sistemi (Studio, Daily Mix, Pass) si sbloccano **uno alla volta** nelle prime 3 run |

Regola: al massimo **una riga di testo** per suggerimento; si impara facendo.

## 17. Schermate e flusso
`Avvio → [UE/UK, primo avvio: consenso UMP] → [primo avvio: Soundcheck → proposta "riduci flash"] → Menu principale`
- **Menu principale:** Gioca (Run) · Daily Mix · Studio · Backstage Pass · Negozio · Impostazioni
- **Preparazione run:** scelta del Livello di Pressione e del seme (casuale o inserito)
- **Partita** (§3) → **Scelta 1 su 3** (finestra) → **Fine settore: scelta del Vinile** → **Risultati** (Royalties, Encore ×2, punteggio)
- **Studio** (albero) · **Maestria** · **Pass** (percorso gratuito e premium) · **Negozio** (Starter, VIP, pacchetti, Sound Pack)
- **Impostazioni:** audio (musica, effetti), **riduci flash**, qualità grafica, daltonismo, Drop assistito, lingua, account PGS, privacy e consenso

## 18. Monetizzazione (sintesi, dettagli in `02`)
- Rewarded: **Encore** (×2 Royalties) · **Rewind** (1 per run: ricarica il salvataggio automatico all'inizio dell'ondata persa, così puoi rigiocarla cambiando strategia) · **Remix** (+1 rilancio) · **Crate Digging** (forziere del giorno). Circa 10 al giorno, nessun effetto nelle modalità competitive.
- **Backstage Pass** (circa 4,99 € ogni 30 giorni, senza rinnovo automatico): premi senza pubblicità (stessi limiti), progressione ×2, percorso premium. Si lancia solo con **D30 ≥ 7%** e un test positivo (vedi `02`).
- **VIP per sempre** (circa 7,99 €), **Starter Pack** (1,99 €), **pacchetti di Royalties**, **Sound Pack** estetici.
- Da definire più avanti (non servono per il prototipo): contenuto del forziere Crate Digging, missioni giornaliere e settimanali, come avanza il percorso del Pass (punti ottenuti giocando).
- Niente energia, niente interstitial, niente casse casuali a pagamento.

## 19. Architettura tecnica (alto livello)
Codice in inglese, un modulo per responsabilità, dati di bilanciamento in **ScriptableObject**.

| Modulo | Responsabilità |
|---|---|
| `Simulation` | ciclo a passo fisso, deterministico, RNG con seme; nessuna dipendenza da Unity nella logica pura (testabile) |
| `BeatClock` | BPM, suddivisioni, eventi di battito nel tempo di simulazione, collegamento a `dspTime` |
| `Grid` / `MapGenerator` | griglia, tipi di casella, generazione più validatore |
| `Pathfinding` | flow field BFS, verifica dei piazzamenti |
| `Towers` / `Enemies` | stato, bersagli, effetti, sinergie |
| `WaveDirector` | budget, composizione, anteprima |
| `Draft` / `Vinyls` | scelte 1 su 3, rarità, garanzia |
| `Economy` | crediti, interesse, ricompense |
| `Music` | strati, programmazione `PlayScheduled`, filtri, quantizzazione |
| `Presentation` | grafica, pulsazioni, VFX (legge lo stato, non lo modifica) |
| `Meta` | Studio, maestrie, sblocchi, valuta |
| `Save` | salvataggio locale (a fine ondata) più cloud PGS |
| `Services` | pubblicità, IAP, PGS, analytics, consenso (dietro interfacce, sostituibili). **Durante lo sviluppo: implementazioni finte**; quelle reali prima dei test esterni o della pubblicazione (vedi `04`, D13) |

- **Determinismo (D11):** valori critici in interi o virgola fissa · flussi RNG separati (mappa, ondate, scelte, Vinili, effetti) · orologio a 96 tick per battito.
- **Dati:** gli ScriptableObject vengono convertiti in **strutture C# semplici** all'avvio della run. La simulazione non tocca mai tipi Unity e sta in un **assembly separato** (asmdef senza riferimento a UnityEngine), testabile da solo.
- **Test:** test EditMode sulla simulazione (determinismo: stesso seme e stesse mosse danno lo stesso risultato) · validatore delle mappe · **simulatore di bilanciamento** (bot che gioca migliaia di run senza grafica).
- **Prestazioni:** pooling degli oggetti, GPU instancing, zero allocazioni nel ciclo di gioco.

## 20. Obiettivi (KPI)
| Metrica | Obiettivo | Riferimento di mercato |
|---|---|---|
| Tutorial completato | ≥ 85% | — |
| D1 | ≥ 35% | mediana 22%, buono 27–40% |
| D7 | ≥ 12% | mediana <4%, buono 8–14% |
| D30 | ≥ 5% (≥ 7% per lanciare il Pass) | mediana 0,8%, buono 3–7%, top 1% circa 13–15% |
| Sessioni al giorno | ≥ 3 | midcore circa 4 |
| Voto sullo store | ≥ 4,5★ | — |
| Crash-free | ≥ 99,5% | — |

## 21. Domande aperte (da risolvere nel prototipo)
- [ ] È divertente **muto**? (D1)
- [ ] 3D ortografico o 2D? (D5)
- [ ] La griglia 9×14 è abbastanza grande per labirinti interessanti su un telefono piccolo?
- [ ] Il "zittire" di Silence è chiaro senza audio?
- [ ] Il Perfect del Drop è divertente o solo stressante?
- [ ] Durata reale di un settore (obiettivo 5–6 min, D3).
- [ ] La preparazione senza limite di tempo rende il ritmo troppo lento? Alternativa: timer facoltativo con bonus.
- [x] ~~Durata della run~~ → **deciso: 6 ondate + boss**, settore circa 5–6 min, run circa 17 min (D3). Da verificare la durata reale nel prototipo.
- [x] ~~Efficienza dei potenziamenti~~ → **deciso: potenziare conviene** (resa 1,12 al L3 contro 1,00 di una torre nuova; D15). Da verificare che le torri nuove restino utili.
- [x] ~~Muro economico~~ → **deciso: Acoustic Panel** a 10 crediti (D14). Da verificare che il labirinto non diventi troppo lungo e facile (leva: costo del pannello).

## 22. Piano del prototipo (2–3 settimane)
1. `Simulation` + `BeatClock` + griglia fissa + flow field. Forme grigie.
2. Acoustic Panel + 3 torri (Kick, Hi-Hat, Bass) con 3 livelli + 3 nemici (Static, Glitch, Distortion) + 1 settore da 6 ondate + boss.
3. Scelta 1 su 3 con 8 carte, generatore di mappe con validatore.
4. **Test muto** con 3–5 persone.
5. Aggiunta di 3 strati musicali e del Drop → **test con audio**.
6. Decisione: si va avanti, si corregge o si cambia (vedi `04`, D1).

**Criteri per dire "funziona"** (misurabili durante i test):
- almeno 3 tester su 5 chiedono **spontaneamente** di rigiocare ("ancora una");
- i tester capiscono il labirinto e la scelta 1 su 3 **senza spiegazioni** entro il primo settore;
- con l'audio, almeno 3 tester su 5 lo preferiscono chiaramente (altrimenti la musica non è il nostro elemento distintivo);
- durata reale di un settore misurata (dato per decidere la questione della durata in §21);
- nessun tester si blocca più di 10 secondi senza sapere cosa fare.
