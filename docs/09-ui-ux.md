# 09 — UI/UX mobile (brief di Fase 1 e 3, v1 · 21/09/2026)

> **Ruolo del documento:** struttura delle schermate, navigazione, componenti e micro-interazioni dell'interfaccia.
> Non ripete: la FTUE al secondo (`07` §1.5), le micro-animazioni **di gioco** (`07` §3.5), la leggibilità in arena (`07` §3.6), le regole del trascinamento (`03` parte C §3). Le richiama.
> Riferimento di layout: **1080 × 1920 (9:16)**, misure in **dp** (1 dp ≈ 3 px a questa densità). Le voci **PROPOSTA** vanno confermate.

---

## 0. Sette regole dell'interfaccia
1. **Una sola azione primaria per schermata**, sempre nel terzo inferiore, sempre dello stesso colore (avorio pieno). Tutto il resto è secondario (contorno) o terziario (testo).
2. **Il gioco è la Home.** Non esiste un menu "davanti" al gioco: il Core che respira è la prima cosa che si vede, sempre.
3. **Niente finestre modali con "OK".** Le informazioni stanno in fogli che salgono dal basso e si chiudono trascinando o toccando fuori. Le azioni irreversibili si confermano **tenendo premuto** (1 s), mai con un dialogo.
4. **Massimo due livelli di profondità** da qualsiasi punto (Home → Archive → dettaglio di un nodo). Il gesto "indietro" di sistema funziona ovunque e non chiede mai "sei sicuro?".
5. **Poco testo, molte forme.** Ogni azione principale ha icona **e** etichetta corta (≤ 12 caratteri in ogni lingua). I numeri sono grandi, le parole piccole.
6. **Vetro scuro, non pannelli.** L'interfaccia è un velo (avorio al 6%, bordo 1 px al 12%, raggio 24 dp) sopra l'arena, mai un rettangolo opaco che la nasconde.
7. **Tutto risponde in ≤ 100 ms** (stato visivo del tocco) e nulla dura più di 300 ms (transizioni). Il giocatore non aspetta mai l'interfaccia.

---

## 1. Mappa di navigazione
```
                 ┌──────────── Opzioni (foglio) ───────────┐
                 │                                          │
  Home ──▶ Run (Negozio ⇄ Ondata) ──▶ Fine run ──▶ Home     │
   │  ▲                     │                               │
   │  │                  Pausa (velo)                       │
   ├──┼──▶ Daily / Weekly (stessa Run, seme fisso)          │
   ├──┼──▶ Archive ──▶ dettaglio nodo (foglio)              │
   ├──┼──▶ Classifiche ──▶ replay/fantasma                  │
   └──┴──▶ Pass (solo modello B, dopo il soft launch) ──────┘
```
- **Transizioni:** Home ⇄ schermate secondarie = dissolvenza + scala 0,98 → 1 (200 ms). Negozio ⇄ Ondata = movimento di camera (600 ms) e carte che entrano/escono: **nessun cambio di schermata**, è lo stesso mondo.
- **Indietro** (gesto o pulsante): chiude il foglio aperto; dalla Run apre la pausa; dalla Home esce dall'app (comportamento Android standard, senza conferma).
- **Nessuna schermata di caricamento** visibile: la scena è una sola; l'Archive e le classifiche sono fogli sopra l'arena.

---

## 2. Le schermate

### 2.1 Home — "il Core ti aspetta"
```
┌──────────────────────────────┐ 0
│ ⚙                    ◆ 128   │  ← ingranaggio (opzioni) · Blueprints (tocca → Archive)
│                              │
│                              │
│          ·  ·  ·  ·          │  ← anelli dell'arena, fermi, al 15%
│       ·    ╭────╮    ·       │
│      ·    (  ◉   )    ·      │  ← il Core con l'ultimo anello giocato (o l'Emitter di default)
│       ·    ╰────╯    ·       │     scorri a sinistra/destra sul Core = cambia Core type
│          ·  ·  ·  ·          │
│                              │
│   ‹ Standard ›   Grade 2 ±   │  ← nome del Core type · selettore del Grade (solo se sbloccato)
│                              │
│ ┌──────────────────────────┐ │
│ │ Daily · 1.284 giocatori  │ │  ← carta della Daily: il tuo miglior risultato o "non ancora giocata"
│ │ Tu: —      Miglior: 18   │ │     (compare dalla run 2)
│ └──────────────────────────┘ │
│ ┌──────────────────────────┐ │
│ │        ▶  Gioca          │ │  ← azione primaria, 64 dp di altezza, "respira" se è l'unica
│ └──────────────────────────┘ │
│   Archive    Classifiche     │  ← secondarie, testo + icona, 48 dp
└──────────────────────────────┘ 1920
```
- Nessun banner, nessuna notizia, nessun popup, nessun negozio in vista. La Home non "vende": mostra il gioco.
- **Ripresa:** se c'è una run salvata, il pulsante primario diventa **"Riprendi · Ondata 7"** e sotto compare "Nuova run" in secondario.
- **Rivelazione progressiva** (§3): al primo avvio la Home **non esiste**: si entra direttamente nella run. Compare dopo la prima run, e i suoi elementi si accendono uno alla volta.

### 2.2 In partita — Ondata
```
┌──────────────────────────────┐
│ ♥ 100      Ondata 7/18   ◈ 12│  ← tre numeri, 20 sp, avorio; niente altro in alto
│                              │
│        (arena intera,        │
│        camera a 9,4 u,       │
│        integrità = arco      │  ← PROPOSTA: l'integrità è anche un arco sottile attorno al Core
│        attorno al Core)      │     (si vede senza alzare lo sguardo)
│                              │
│                              │
│                              │
│ ┌────────────────┐ ┌───────┐ │
│ │                │ │  2x   │ │  ← velocità: cicla 1x/2x/3x, ricorda l'ultima scelta
│ │   ◎  Pulse     │ ├───────┤ │
│ │  (ricarica ad  │ │  ⏸    │ │  ← pausa
│ │   anello)      │ └───────┘ │
│ └────────────────┘           │  ← Pulse: 60% larghezza × 120 dp; ricarica come anello che si riempie
└──────────────────────────────┘
```
- **Pulse pronto:** l'anello del pulsante è pieno e il bordo emette un alone morbido (0,3 → 0,6 → 0,3 in 2 s). Mai un lampeggio.
- **Pulse in ricarica:** anello che si riempie in senso orario, numero dei secondi al centro solo sotto i 5 s.
- Tocco su un **modulo** durante l'ondata: tooltip di sola lettura (nome, livello, DPS attuale) per 2 s, ancorato sopra il modulo.
- Tocco su un **nemico**: niente (a meno della PROPOSTA P3 "Focus", `07` §1.7).
- **Nessun testo** durante l'ondata oltre ai tre numeri e ai numeri dei danni.

### 2.3 In partita — Negozio
```
┌──────────────────────────────┐
│ ♥ 100      Negozio 7     ◈ 12│
│ ┌──────────────────────────┐ │
│ │ Prossima: ▲▲▲▲▲ ◆◆ ●     │ │  ← anteprima dell'ondata: icone × quantità; tocco = nomi
│ │ DPS 240 → 360 (+50%)     │ │  ← riga dell'anteprima: compare SOLO durante un trascinamento
│ └──────────────────────────┘ │
│                              │
│         (anello in           │
│        primo piano,          │
│        camera a 3,4 u)       │  ← slot validi che respirano durante il trascinamento
│                              │
│ ┌──────────────────────────┐ │
│ │      ⌄ Vendi  +2         │ │  ← zona Vendi: compare SOLO trascinando un modulo dell'anello
│ └──────────────────────────┘ │
│ ┌────┐ ┌────┐ ┌────┐ ┌────┐ │
│ │ ◇  │ │ ○  │ │ ◇  │ │ ●  │ │  ← 4 carte (icona 3D, nome, costo, effetto in ≤ 6 parole)
│ │Emit│ │Ampl│ │Scat│ │Bank│ │     bordo = rarità (C nessuno · U argento · R oro)
│ │ 3  │ │ 3  │ │ 4  │ │ 4  │ │     "⇧ L2" in alto a destra se il modulo è già posseduto (merge)
│ └────┘ └────┘ └────┘ └────┘ │     carta grigia se non si può pagare (mai nascosta)
│ ┌──────┐ ┌──────┐ ┌────────┐│
│ │↶ Ann.│ │⟳ 1   │ │ ▶ Next ││  ← Annulla (grigio se pila vuota) · Rilancia con costo · Next wave (primaria)
│ └──────┘ └──────┘ └────────┘│
└──────────────────────────────┘
```
- **Anatomia della carta** (240 × 300 dp a 1080 px ≈ 80 × 100 dp): icona resa dal modello 3D (stessa luce del gioco), nome (16 sp), costo con l'icona dei Credits (20 sp), una riga d'effetto (12 sp, es. "vicini ×1,5"), bordo di rarità, badge "⇧ L2" quando comprarla farebbe un merge.
- **Stati della carta:** normale · sollevata (durante il trascinamento) · non acquistabile (60% alpha, costo in corallo tenue) · comprata (scompare con dissolvenza e le altre **non si spostano**: il vuoto resta, così la posizione delle carte è stabile).
- **Foglio del modulo** (pressione lunga su carta o modulo, 250 ms): sale dal basso a metà schermo: modello 3D grande, statistiche a L1/L2/L3 con quella attuale evidenziata, "influenzato da: Amplifier (sinistra), Lens (destra)". Si chiude trascinando giù. Da qui: **Vendi** (tieni premuto 1 s) come alternativa alla zona Vendi.
- **Slot extra** (dopo il primo Guardian): compare come **quinta carta** speciale "Slot +1 · 8", trascinabile sull'anello dove vuoi inserirlo (l'anello si apre con un'animazione di 300 ms).

### 2.4 Fine run — vittoria e sconfitta (stessa struttura)
```
┌──────────────────────────────┐
│                              │
│      Ondata 12 di 18         │  ← titolo: dove sei arrivato (sconfitta) / "Vittoria" (vittoria)
│   Fermato da: Brute (élite)  │  ← solo sconfitta: cosa ti ha ucciso, con l'icona
│                              │
│        (il tuo anello,       │  ← l'anello finale, 3D, ruota lentamente UNA volta (4 s) poi si ferma
│         in primo piano)      │
│                              │
│   Danni totali     1,24M     │  ← conteggio progressivo, 0,8 s, tocco = salta
│   Tempo            11:42     │
│   Uccisioni          318     │
│                              │
│   ◆ +14 Blueprints           │  ← si aggiungono dopo i danni, uno alla volta (tono che sale)
│   ▓▓▓▓▓▓▓▓░░ Lance tra 6     │  ← barra verso il prossimo sblocco scelto (tocca → Archive)
│                              │
│ ┌──────────────────────────┐ │
│ │      ▶  Rigioca          │ │  ← primaria
│ └──────────────────────────┘ │
│  Stesso seme   Condividi   ⌂ │  ← secondarie: rigioca questo seme (fuori classifica) · condividi · Home
└──────────────────────────────┘
```
- **Ordine e tempi:** titolo (0 s) → anello (0,3 s) → danni (0,6–1,4 s) → tempo e uccisioni (1,4–1,8 s) → Blueprints (2,0–2,6 s) → barra (2,6–3,0 s) → pulsanti (3,0 s, ma **toccabili da subito**: il tocco salta tutto).
- **Condividi:** genera un'immagine 9:16 con l'anello, i tre numeri, il seme e il codice del replay. Nessuna schermata intermedia: si apre direttamente il foglio di condivisione di sistema.
- **Variante Daily:** in più la posizione in classifica ("#47 di 1.284") e il confronto con il fantasma scelto ("Fantasma: ondata 15 · tu: 12").
- **Sconfitta all'atto 1 per la terza volta:** una riga di consiglio contestuale (`07` §2.4) sopra i pulsanti, nient'altro.

### 2.5 Archive — gli sblocchi
```
┌──────────────────────────────┐
│ ‹                    ◆ 128   │
│  Armi   Booster   Econ.  Core│  ← 4 schede; sottolineatura che scorre (200 ms)
│ ─────                        │
│ ┌──────────┐ ┌──────────┐    │
│ │ ◇ Arc    │ │ ◇ Lance  │    │  ← nodi DISPONIBILI: icona 3D, nome, costo, una riga d'effetto
│ │ 20 ◆     │ │ 20 ◆     │    │
│ └──────────┘ └──────────┘    │
│ ┌──────────┐                 │
│ │ ◇ Mortar │   ░░░░░░░░░     │  ← nodi BLOCCATI: silhouette + "richiede Arc"; niente lucchetti rossi
│ │ 25 ◆     │   ░░░░░░░░░     │
│ └──────────┘                 │
│ ✓ Emitter  ✓ Scatter          │  ← posseduti: riga compatta in fondo, non carte
│                              │
│ ┌──────────────────────────┐ │
│ │  Prossimo: Lance · tra 6 │ │  ← il nodo "puntato": appare sulla schermata di fine run
│ └──────────────────────────┘ │
└──────────────────────────────┘
```
- **Sblocco in due tocchi, senza dialogo:** tocco sul nodo → la carta si espande (foglio) con il modello grande, le statistiche e **"Sblocca · 20 ◆"** come primaria. Tocco → i Blueprints scendono con un conteggio, il nodo "si accende" (bagliore morbido 400 ms), le carte vicine si aprono. Se non bastano: la primaria dice **"Punta · tra 6"** e mette il nodo come obiettivo (barra di fine run).
- **Prova prima di sbloccare** (**PROPOSTA**): dal foglio, "Prova in una run di prova" apre una run di 3 ondate con il modulo, fuori classifica, senza Blueprints. Evita l'acquisto al buio.
- Nessuna spesa in denaro in questa schermata (modello B: il tetto ai pacchetti e le ricompensate stanno altrove, `08`).

### 2.6 Pass (solo modello B, dopo il soft launch)
- Una **pista orizzontale** di 30 livelli, due file (Free sopra, Premium sotto), la posizione attuale centrata; scorrimento con inerzia e calamita sui livelli.
- **Riscossione automatica:** i premi raggiunti si assegnano da soli (una riga sulla schermata di fine run: "Pass · livello 8 · Tema dell'anello"). **Niente 30 tocchi "riscuoti"**: è il fastidio n.1 dei pass.
- Il premium mostra il tema del Core **applicato al tuo Core** in anteprima; il pulsante d'acquisto è secondario, non lampeggia, non ha timer.

### 2.7 Opzioni (foglio a tutta altezza)
Un elenco solo, raggruppato, con **anteprima immediata** (le modifiche si vedono dietro il velo):
- **Visivo:** riduci movimento · intensità degli effetti (cursore) · numeri dei danni (tutti / grandi / nessuno) · dimensione del testo (100–200%, con un testo d'esempio) · daltonismo (3 preset, con l'anteprima dei colori) · alto contrasto.
- **Audio e tatto:** musica · effetti · vibrazioni.
- **Gioco:** velocità predefinita · velocità delle animazioni (1x / 2x / istantanea) · Modalità Calma (`08` §4.3, con la nota "fuori classifica").
- **Account e privacy:** consenso (rivedibile), ripristino acquisti, cancella dati (tieni premuto 1 s).
- Le quattro opzioni più richieste (riduci movimento, intensità, numeri, vibrazioni) sono raggiungibili anche **dalla pausa** in un tocco.

### 2.8 Pausa (velo sull'ondata)
Velo scuro al 60%, l'arena resta visibile ma ferma. Tre pulsanti: **Riprendi** (primaria), Opzioni rapide, **Abbandona** (secondaria, **tieni premuto 1 s**: l'anello si riempie, poi conferma). Toccando fuori dal velo si riprende.

---

## 3. Primo impatto ed esperienza dei primi minuti
La FTUE è specificata al secondo in `07` §1.5 e i primi 10 secondi in `07` §4.5. Qui le regole **dell'interfaccia** che la rendono possibile:

### 3.1 Cosa non c'è al primo avvio
Nessuna Home, nessun logo lungo, nessun login, nessuna richiesta di notifiche o valutazione, nessuna scelta della lingua (si prende quella del sistema; cambiabile nelle opzioni), nessun "benvenuto". Solo il consenso UMP dove obbligatorio (D16), poi il Core.

### 3.2 Rivelazione progressiva
| Momento | Cosa si accende | Come |
|---|---|---|
| Primo avvio | solo ▶ e (in negozio) le carte | tutto il resto è invisibile, non grigio |
| Primo negozio | Annulla e Rilancia compaiono **dopo** il primo trascinamento | dissolvenza, senza spiegazione |
| Prima ondata 2 | velocità e pausa | idem |
| Fine della prima run | la Home, con **Gioca** e ⚙; i Blueprints guadagnati e "Archive" che appare con la barra | le voci compaiono una alla volta (150 ms di sfalsamento) |
| Fine della seconda run | la carta **Daily** sulla Home ("Oggi: 1.284 giocatori") | — |
| Prima vittoria | selettore del **Grade** e dei **Core type** (quelli sbloccati) | — |
| Terza run | Classifiche | — |
Ogni elemento nuovo compare con una **dissolvenza e un leggero respiro** (scala 0,96 → 1), una volta sola. Nessun tooltip "novità!", nessun pallino rosso.

### 3.3 Suggerimenti
Solo **una riga** alla volta, bianca su niente, sopra il terzo inferiore, che svanisce al primo gesto giusto. Massimo cinque in tutta la vita del giocatore (▶, trascina, Pulse, doppione, Daily). Nessun pannello, nessun personaggio guida, nessun "OK".

### 3.4 Stati vuoti e attese
- Classifiche prima della rete: "Le classifiche arrivano quando c'è connessione" + il tuo storico locale. Niente rotellina infinita: le richieste hanno un timeout di 3 s e poi mostrano lo stato locale.
- Archive con 0 Blueprints: i nodi disponibili sono comunque visibili con "tra N": il giocatore vede cosa sta guadagnando.

---

## 4. Feedback e micro-interazioni dell'interfaccia
(le animazioni di gioco sono in `07` §3.5; qui solo l'interfaccia)

### 4.1 Componenti e stati
| Componente | Riposo | Tocco (≤ 100 ms) | Rilascio | Disabilitato |
|---|---|---|---|---|
| Pulsante primario | avorio pieno, 64 dp, raggio 24 | scala 0,96, 80 ms | ritorno con overshoot 2%, 150 ms; vibrazione leggera | 40% alpha, **mai nascosto** |
| Pulsante secondario | contorno 1 px avorio 40% | riempimento al 10% | idem | idem |
| Carta | vetro, bordo di rarità | si solleva 12 px, scala 1,05, ombra più ampia | (trascinamento: `03` C §3) | 60% alpha, costo in corallo tenue |
| Scheda (tab) | testo 60% | — | sottolineatura che scorre 200 ms, ease-out | — |
| Cursore (slider) | linea + pallino | pallino scala 1,2 | tick di vibrazione ai valori notevoli (0, 50, 100) | — |
| Interruttore | pillola | — | scorre 150 ms; cambia colore avorio ↔ 30% | — |
| Foglio | — | — | sale dal basso 250 ms ease-out, velo al 40%; si chiude trascinando (segue il dito) o toccando fuori | — |
| Toast (riga) | — | — | compare in alto, 2 s, uno alla volta; il successivo sostituisce il precedente | — |
| Contatore | — | — | cifre che scorrono verso l'alto, 600–1.200 ms, ease-out, tono che sale; tocco = valore finale | — |
| Barra di progresso | — | — | si riempie con ease-out 400 ms; al completamento un bagliore morbido, mai un lampeggio | — |

### 4.2 Il "respiro" come segnale
Un solo elemento per schermata può respirare (scala 1,00 → 1,02, 2 s): **l'azione che il gioco si aspetta**. ▶ sulla Home, Next wave nel negozio quando non ci sono più Credits utili, il Pulse quando è pronto e ci sono 3+ nemici vicini. È il nostro sostituto del pallino rosso e delle frecce lampeggianti.

### 4.3 Vibrazioni (Android `VibrationEffect`, `03` C §5)
| Livello | Quando |
|---|---|
| Leggera (15 ms) | tocco su un pulsante, prendere una carta, calamita, tick del cursore |
| Media (25 ms) | piazzare, Pulse, Next wave, sblocco nell'Archive |
| Successo (due tocchi) | merge, vittoria, Archive completato |
| Nessuna | errori (il ritorno morbido basta), scorrimenti, contatori |

### 4.4 Suono dell'interfaccia (`03` parte B §3)
Ogni componente ha **un** suono, morbido e breve: pulsante (tock caldo), carta presa (nota), calamita (clic caldo), rilascio (nota più bassa), merge (cristallo che sale), contatore (tono che sale a ogni scatto), foglio (soffio). Mai due suoni identici di seguito: variazione casuale di ±3% del tono.

### 4.5 Transizioni
| Da → a | Transizione | Durata |
|---|---|---|
| Negozio → Ondata | camera si allontana; carte scendono sfalsate di 40 ms; Pulse sale | 600 ms |
| Ondata → Negozio | anello che pulsa (fine ondata) → riepilogo → camera si avvicina → carte salgono | 800 + conteggio + 600 ms |
| Home ↔ Archive/Classifiche | dissolvenza + scala 0,98 → 1 | 200 ms |
| Qualsiasi → Fine run | l'arena resta; un velo sale al 40% e il contenuto entra dal basso | 300 ms |
| Apertura di un foglio | dal basso, ease-out, segue il dito | 250 ms |
Mai scorrimenti laterali tra schermate (suggeriscono una gerarchia che non c'è), mai zoom "esplosivi".

---

## 5. Design token (per UI Toolkit, `06` D22)
```
Griglia          8 dp · gutter 16 dp · zona del pollice = terzo inferiore
Raggi            carte 16 · pulsanti 24 · fogli 32 (solo angoli superiori)
Vetro            sfondo rgba(243,233,215,0.06) · bordo 1 dp rgba(243,233,215,0.12)
Colori           --ivory #F3E9D7 · --ivory-60 · --ivory-40 · --teal #5CC8C0 · --gold #E9B872
                 --mint #9AD9A1 · --coral #F07A6A (solo nemici e costi non pagabili) · --rose #C8507A
Testo (Nunito)   display 32 · titolo 24 · numeri 20 · corpo 16 · didascalia 12 (× scala 1,0–2,0)
Durate           tocco 80 · stato 150 · componente 250 · schermata 200 · camera 600
Curve            ease-out (cubic 0.2,0.8,0.2,1) · ease-out-back (0.34,1.56,0.64,1) per gli assestamenti
Tocco            minimo 48 × 48 dp · soglia di trascinamento 8 dp · pressione lunga 250 ms · tieni premuto 1.000 ms
```
Tutto vive in un unico foglio USS (`Assets/UI/Theme.uss`) con variabili: i temi stagionali cambiano solo le variabili di colore, mai le misure.

---

## 6. Accessibilità e localizzazione (dal primo giorno)
- Tutto il testo scala fino al 200% senza rompere i layout: le carte usano l'icona come elemento principale e il testo può andare su due righe.
- Ogni azione primaria ha icona **e** testo; ogni colore ha una forma (`03` §A3).
- Contrasto testo ≥ 4,5:1; bersagli ≥ 48 dp; niente informazioni solo sonore (D1) né solo di colore.
- **Budget di caratteri** per le stringhe (per DE e FR, che sono le più lunghe): etichette dei pulsanti ≤ 12, righe d'effetto ≤ 30, suggerimenti ≤ 40. Le stringhe hanno chiavi, mai testo nel codice.
- "Riduci movimento" spegne anche il respiro (§4.2), sostituito da un bordo più luminoso sull'azione attesa.

---

## 7. Cosa entra in quale fase
- **Fase 1** (forme semplici, UI Toolkit): §2.2, §2.3, §2.4 (senza Blueprints), §2.8, §4 (componenti, respiro, vibrazioni), §5 (token), rivelazione progressiva minima (Annulla/Rilancia dopo il primo trascinamento).
- **Fase 3** (grafica definitiva): §2.1 Home, icone 3D sulle carte, suoni dell'interfaccia, §2.7 opzioni complete, §6 completo.
- **Fase 4**: §2.5 Archive, barra "prossimo sblocco", Daily sulla Home, condivisione, §3.2 completa.
- **Fase 6+**: §2.6 Pass.

**Decisioni da confermare:** arco dell'integrità attorno al Core · "prova prima di sbloccare" nell'Archive · tieni premuto 1 s come unica conferma per le azioni irreversibili · riscossione automatica del Pass.
