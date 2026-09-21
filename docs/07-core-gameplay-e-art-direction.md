# 07 — Core gameplay e direzione artistica (brief di Fase 1, v1 · 21/09/2026)

> **Ruolo del documento:** la specifica *eseguibile* della Fase 1 (`06` §2) dal punto di vista di game design e art direction.
> Il GDD (`05`) resta la fonte dei numeri; `03` la fonte dell'identità visiva. Qui si scende al livello del **gesto, del frame e del pixel**.
> Le voci marcate **PROPOSTA** cambiano o estendono il GDD e vanno confermate (poi registrate in `04`).

---

## 1. Core gameplay e logica

### 1.1 La fantasia in una frase
*"Costruisco un motore di numeri attorno a un cuore che respira."*
Il giocatore non "piazza torri": **assembla un anello** dove ogni pezzo cambia i vicini, e vede i numeri crescere a ogni ondata. La ricompensa emotiva è doppia: il **click** del pezzo giusto nel posto giusto (Backpack Battles, Balatro) e l'**esplosione** dei numeri (Balatro, The Tower) mostrata con calma.

### 1.2 Le due modalità di input (una mano, verticale)
Il gioco ha esattamente due stati. In ognuno il pollice ha **un solo gesto principale**.

| Stato | Gesto principale | Gesti secondari | Cosa NON si può fare |
|---|---|---|---|
| **Ondata** (tempo scorre) | **Tocca il Pulse** (pulsante grande in basso, oppure tocca il Core) | velocità 1x/2x/3x · pausa · tocco su un modulo = tooltip di sola lettura | comprare, spostare, vendere |
| **Negozio** (senza tempo) | **Trascina** una carta su uno slot / un modulo su un altro slot o sulla zona Vendi | tocca carta → tocca slot (alternativa) · Annulla · Rilancia · Next wave · pressione lunga = dettagli | il tempo non passa: niente pressione |

**Specifica del tocco (da implementare così, non "a sentimento"):**
- Bersagli ≥ **48 dp**; slot dell'anello con hit-area circolare di 0,6 unità (più grande della forma visibile).
- **Soglia di trascinamento: 8 dp.** Sotto è un tocco. Così "tocca carta → tocca slot" e il trascinamento convivono senza modalità.
- L'oggetto trascinato sta **1 cm sopra il dito**; l'anteprima è **in alto**, mai sotto il dito (lezione Backpack Battles mobile).
- **Calamita:** entro 0,45 unità dal centro di uno slot valido l'oggetto "cade" nello slot (tick di vibrazione leggera). Uscendo dal raggio si stacca.
- Rilascio fuori da uno slot valido → **ritorno morbido** al punto di origine (250 ms, ease-out), nessun flash d'errore, nessun testo.
- **Pressione lunga 250 ms** su carta o modulo → scheda dei dettagli (statistiche, livello, vicini che lo influenzano). Si chiude sollevando il dito.
- **Anti-doppio tocco:** i pulsanti irreversibili (Next wave, Rilancia, Vendi) ignorano un secondo tocco entro 300 ms.
- **Annulla** è sempre visibile nel negozio, grigio se la pila è vuota. Non esiste un dialogo di conferma per nessuna azione: Annulla è la conferma.

### 1.3 Le dieci regole che creano profondità
(tutte già nel GDD; qui il *perché* di ciascuna)
1. **Vicinato a due lati.** Un booster potenzia solo i due vicini → la posizione è una scelta, non un dettaglio. Con 6 slot, al massimo 3 booster "puri" senza sprecare adiacenze.
2. **Anello circolare.** Il primo e l'ultimo slot sono vicini → non esiste un "angolo morto"; ogni build è un ciclo.
3. **Merge automatico.** Comprare un doppione lo unisce (fino a L3) senza occupare slot → i doppioni sono desiderabili, non sprecati (Super Auto Pets).
4. **Interesse con tetto.** 1 Credit ogni 5 risparmiati, max 5 → tensione "compro ora o risparmio?" (Balatro). Bank alza il tetto: un'economia come archetipo.
5. **Rilancio a costo crescente.** 1, 2, 3… nello stesso negozio → cercare la carta giusta ha un prezzo che sale.
6. **Vendita a metà.** Cambiare idea costa, ma non è proibitivo → le build possono "pivotare" a metà run.
7. **Anteprima dell'ondata.** Vedi cosa arriva → il negozio è una risposta, non un azzardo. Corazza → serve danno alto; sciame → serve multi-bersaglio.
8. **Anteprima dell'effetto.** Il DPS prima → dopo mentre trascini → il gioco insegna le combo da solo.
9. **Pulse pronto a ogni ondata, poi 20 s di ricarica.** Un solo uso "gratis" per ondata: **quando** usarlo è la scelta durante l'ondata.
10. **Core type.** Cambia le regole di partenza → la stessa run si gioca in modi diversi (i mazzi di Balatro).

### 1.4 Perché non è superficiale: lo spazio delle decisioni in un negozio
In un negozio medio il giocatore valuta: 4 offerte × (compra/salta) × fino a 6 posizioni × spostamenti × vendite × rilancio × risparmio. Ma la profondità vera sta nelle **tensioni**, non nel conteggio:

| Tensione | Le due forze | Dove si sente |
|---|---|---|
| Potenza ora vs interesse dopo | comprare il terzo modulo o tenere 10 Credits (+2 a ondata) | ogni negozio dall'ondata 2 |
| Spazio sull'anello vs merge | un modulo nuovo occupa uno slot; un doppione no ma costa uguale | dal 4° modulo |
| Booster "a sandwich" | Arma–Booster–Arma sfrutta entrambi i lati; Booster–Booster spreca un lato | quando si ha il 2° booster |
| Tipo di arma vs composizione dell'ondata | Scatter (3 bersagli) contro sciami; Lance contro Brute corazzati | quando la preview mostra un nemico nuovo |
| Economia vs difesa | Bank/Bulwark non sparano: ogni slot economico è un'arma in meno | atto 1, ondate 3–5 |
| Rilanciare vs accettare | 1 Credit per 4 carte nuove, ma il costo sale | ogni volta che manca "quella" carta |

**Nemici come domande, moduli come risposte** (la chiave per tutti gli atti):
| Nemico | La "domanda" | Risposte buone | Risposte cattive |
|---|---|---|---|
| Swarmlet (gruppi da 5) | "hai danno su più bersagli?" | Scatter, Arc, Echo, Pulse | Lance, Emitter singolo |
| Brute (corazza 3) | "i tuoi colpi sono abbastanza pesanti?" | Lance, Mortar, Amplifier, Lens (+2 fisso batte la corazza) | Scatter L1 (5 − 3 = 2 per colpo) |
| Dasher (scatti ×3) | "sei veloce a reagire?" | Overclock, Frost, portata alta (Lens) | armi lente senza booster |
| Splitter (si divide) | "finisci ciò che inizi?" | danno alto in un colpo, Scatter per i figli | danno "a goccia" |
| Warden (scudo −50% vicino) | "sai colpire lontano?" | Lens, Mortar (esplosione), Lance (trapassa) | armi a corto raggio |
| Guardian (+5 Swarmlet ogni 25%) | "la tua build regge un boss *e* uno sciame?" | build bilanciata, Pulse tenuto per lo sciame | build tutta single-target |

### 1.5 I primi 30 secondi, al secondo (FTUE, GDD §14 dettagliato)
Il primo avvio usa un **seme fisso** e un'unica eccezione al negozio casuale: il primo negozio **contiene sempre un Amplifier** (marcato nel replay come `tutorial`, così resta verificabile). Nessun menu, nessun testo oltre a una riga.

| Tempo | Cosa vede | Cosa fa | Cosa impara |
|---|---|---|---|
| 0,0–1,5 s | dissolvenza dal gradiente crepuscolare; il Core respira; sul petalo in alto una gemma (Emitter) pulsa piano | niente | "quello è mio, è vivo" |
| 1,5 s | in basso un solo pulsante attivo: **▶** con un alone morbido; riga: *"Tocca ▶"* | tocca | l'unico comando |
| 2–20 s | **Ondata 1: 5 Drifter**, uno ogni 3 s, da 5 direzioni diverse. L'Emitter spara: linea teal sottile, un "8" tenue; la scheggia si scioglie in scaglie | guarda (o tocca a vuoto: nessuna punizione) | "la difesa spara da sola" |
| ~8 s | il pulsante **Pulse** si riempie (pronto) senza suggerimento. **Solo se** un Drifter scende sotto 2,0 dal Core: riga *"Tocca Pulse"* | eventualmente tocca | l'abilità esiste; la prova quando serve |
| ~20 s | ultimo Drifter morto → **riepilogo**: "Ondata 1 ✓", i Credits salgono 6 → 10 cifra per cifra (0,6 s), tono che sale | tocca per saltare (facoltativo) | "vincere paga" |
| ~22 s | il negozio: la camera si avvicina all'anello (0,6 s ease); 4 carte salgono dal basso; riga: *"Trascina l'**Amplifier** accanto all'Emitter"*; i due slot vicini all'Emitter respirano | prende la carta | il gesto principale |
| ~25 s | mentre trascina: slot validi accesi, calamita, in alto *"Danni al secondo: 16 → 24"*; al rilascio una **linea dorata** si accende tra Amplifier ed Emitter, vibrazione media | lascia | **primo "aha"**: la posizione conta |
| ~30 s | riga: *"Tocca ▶"*. Ondata 2: 5 Drifter + 1 gruppo di Swarmlet; i numeri sono più grandi ("12") | tocca | "i numeri salgono" |
| ~55–60 s | lo sciame arriva stretto; riga *"Tocca Pulse"* quando 3+ nemici sono entro 3,0 | tocca Pulse | **secondo "aha"**: l'onda respinge tutto |
| ~90 s | secondo negozio con un Emitter in offerta; riga *"Compra un altro Emitter"* → merge animato, "Livello 2" | trascina sull'Emitter | **terzo "aha"**: i doppioni si uniscono |

Dopo il terzo "aha" i suggerimenti spariscono per sempre. **Regola:** una riga per suggerimento, mai un pannello, mai un "OK".

### 1.6 Loop di un'ondata normale (la "fisica" senza fisica)
La simulazione è deterministica e senza motore fisico; la **presentazione** aggiunge il peso:
- **Spawn:** i nemici entrano a raggio 9,0 con una dissolvenza da 0 → 1 in 300 ms e una rotazione lenta su se stessi (mai rotazioni dello sfondo).
- **Avanzata radiale** con un leggero "galleggiamento" verticale (±0,03 unità, 2 s) per non sembrare su binari.
- **Colpo:** linea arma → bersaglio (0,2–0,3 s, dissolvenza), il bersaglio riceve una **spinta visiva** del 3% verso l'esterno (80 ms) — solo visiva, la posizione simulata non cambia.
- **Morte:** 6–10 scaglie che si allontanano e svaniscono in 400 ms; numero dell'uccisione in avorio.
- **Pulse:** onda circolare 0 → 3,0 unità in 500 ms (ease-out), i nemici respinti scivolano indietro con ease-out in 250 ms (la simulazione li sposta in un tick: la grafica interpola).
- **Contatto col Core:** tinta corallo che sfuma in 400 ms, vibrazione breve e debole, **nessuna scossa** di default.
- **Fine ondata:** anello che pulsa verso l'esterno (800 ms), poi il riepilogo.

### 1.7 PROPOSTE per rendere il Pulse una scelta e non un riflesso
Rispondono alla domanda aperta del GDD §18 ("Il Pulse è una scelta?"). Da provare **una alla volta** nel Gate 1, dietro un flag:
- **P1 — Finestra di risonanza:** se il Pulse colpisce **3+ nemici**, la ricarica successiva è −25%. Premia l'attesa del gruppo senza punire chi lo usa subito.
- **P2 — Il Pulse attiva i vicini:** al Pulse, tutte le armi sull'anello sparano subito (ricarica azzerata). Lega l'abilità alla build: più armi, più valore.
- **P3 — Focus (secondo gesto):** tocca un nemico durante l'ondata → per 3 s tutte le armi in portata lo preferiscono; 8 s di ricarica. Aggiunge agency ma è un secondo gesto: da valutare contro la regola "un gesto per stato".
Raccomandazione: P1 subito (costo nullo, nessun nuovo gesto), P2 come modulo (**Resonator**, booster raro) invece che regola globale, P3 solo se i tester dicono "durante l'ondata mi annoio".

---

## 2. Difficoltà e curva di apprendimento

### 2.1 Dentro la run: due curve che devono incrociarsi al momento giusto
- **Curva del nemico:** HP ×1,20 per ondata (ondata 18 ≈ ×22), budget ×1,10 (ondata 18 ≈ 40 punti = ~40 Drifter o 13 Brute). Nuovi tipi uno per volta, ognuno introdotto **da solo** con 2 s di pausa.
- **Curva del giocatore (stima con build media):**
  | Ondata | Build tipica | DPS stimato | HP Drifter | Colpi per uccidere |
  |---|---|---|---|---|
  | 1 | Emitter | 16 | 20 | 3 |
  | 3 | Emitter + Amplifier + Scatter | ~45 | 29 | 2–3 |
  | 6 (Guardian 1) | Emitter L2 + 2 Amplifier + Scatter + Bank | ~120 | 50 (Guardian 1.000) | 2 |
  | 12 (Guardian 2) | armi L2–L3, booster L2 | ~600 | 150 (Guardian 3.000) | 1–2 |
  | 18 (Guardian 3) | 2 armi L3 tra 3 booster L2–L3 | ~2.500+ | 440 (Guardian 8.800) | 1 |
  La crescita del giocatore è **moltiplicativa** (livello × booster × booster), quella del nemico **esponenziale ma lineare nel log**: la partita è progettata perché il giocatore *superi* la curva **se** costruisce bene, e la incroci verso il basso **se** sbaglia. Il punto d'incrocio voluto è l'ondata 9–12 per un giocatore nuovo, mai la 18 (altrimenti la sconfitta arriva sempre alla fine e sembra ingiusta).
- **Il Guardian è l'esame dell'atto:** chiede due cose insieme (boss + sciame). Chi ha una build a un solo bersaglio lo scopre al 75% della sua vita, quando arrivano i primi 5 Swarmlet, non alla morte.
- **Élite** dall'atto 2 (HP ×3, corazza +1): un "mini esame" per ondata, mai più di due.
- **Ondata a tema** (la 3 di ogni atto): un solo tipo → la build viene messa alla prova su una domanda sola.

**Leve di taratura** (ordine in cui girarle, con il bot): budget della prima ondata → crescita dell'HP → costo dei moduli rari → tetto dell'interesse → Credits per ondata. Obiettivi numerici in `06` Fase 2.

### 2.2 Run dopo run: quattro fonti di novità, nessuna di potenza
| Fonte | Cosa cambia | Quando |
|---|---|---|
| **Archive** (Blueprints) | il **pool** del negozio: nuovi moduli e Core type | dalla run 2–3, completo in 60–80 run |
| **Core type** | le regole di partenza (economia, resistenza, danno ×1,5 con 50 HP) | dalla prima vittoria |
| **Grade 1–10** | una regola in più per Grade, cumulativa | dopo la prima vittoria |
| **Daily / Weekly** | seme e pool fissi, modificatori settimanali ("solo booster rari", "anello a 5 slot") | dal lancio |

**PROPOSTA — i dieci Grade** (uno per volta, cumulativi, come l'Ascension di Slay the Spire):
1. Nemici +10% HP · 2. −1 Credit per ondata · 3. Élite dall'atto 1 · 4. Il Guardian ha uno scudo (−25% danno) finché ha una scorta viva · 5. Rilancio parte da 2 · 6. Interesse max 4 · 7. Una élite in più per ondata · 8. Il Pulse si ricarica in 25 s · 9. I nemici entrano in 16 s invece di 20 · 10. Anello a 5 slot fino al primo Guardian.
Ogni Grade cambia **una** regola leggibile: il giocatore sa sempre perché è più difficile.

**Curva di apprendimento attesa:**
| Run | Cosa capisce | Segnale nei dati |
|---|---|---|
| 1–3 | vicinato, merge, Pulse | supera l'atto 1 |
| 4–10 | interesse, disciplina del rilancio, vendere per pivotare | Credits inutilizzati a fine run in calo |
| 10–30 | **archetipi**: sciame (Scatter+Echo), cecchino (Lance+Lens+Amplifier), economia (Bank→tardi), fortezza (Bulwark+Capacitor+Pulse) | vince a Grade 0, prova Core diversi |
| 30+ | Grade, Daily, ottimizzazione del seme | tempo per run stabile, punteggi Daily |

### 2.3 Anti-noia: perché la run 40 non è la run 4
- **Seme:** direzioni, composizione delle ondate e offerte sono diverse ogni run (ma uguali per tutti nella Daily).
- **Ogni atto introduce un nemico che contraddice una build** (tabella §1.4): non esiste una build "sempre giusta".
- **Identità entro l'ondata 6:** il negozio spinge a "diventare qualcosa" (merge e booster) prima del primo Guardian; le run si raccontano ("la run dei tre Scatter").
- **Endless** dopo la vittoria: la crescita continua e le build "rotte" trovano il loro posto in classifica, non nel gioco base.
- **Build condivisibili** (screenshot dell'anello, codice del replay): la community produce contenuto.

### 2.4 La sconfitta deve insegnare, non punire
- Schermata di sconfitta: **cosa ti ha ucciso** (tipo di nemico, ondata), l'anello finale, il DPS, un pulsante *"Rigioca questo seme"* e *"Condividi"*.
- Nessuna perdita di meta-progressione: i Blueprints delle ondate superate restano.
- Run da 10–15 minuti: il costo di riprovare è basso (Brotato, Balatro).
- **PROPOSTA:** dopo 3 sconfitte consecutive nello stesso atto, il gioco suggerisce **un solo** consiglio contestuale ("i Brute hanno corazza: Lens aggiunge +2 danno fisso"), mai un nerf nascosto della difficoltà (i giocatori se ne accorgono e lo odiano).

---

## 3. Direzione artistica (2026): "Dusk Garden, tactile minimalism"

### 3.1 La regola delle tre superfici
"Minimale ma dettagliato" non vuol dire *piatto*: vuol dire **poche forme, ogni forma finita**. Ogni oggetto del gioco ha esattamente tre strati:
1. **Corpo** opaco con illuminazione morbida (luce calda + ambiente freddo): dà volume.
2. **Bordo** (rim light sottile, 1–2 px a 1080p) del colore della categoria: dà leggibilità sullo sfondo scuro.
3. **Interno** emissivo a bassa intensità (0,15–0,3): dà "vita"; sale a 0,6–1,0 solo su Pulse, merge e uccisioni, sempre con dissolvenza.
Niente texture disegnate: materiali, colore e luce fanno tutto. Questo tiene basso il peso, alto il dettaglio percepito e rende ogni asset **generabile via script** in Blender.

### 3.2 Tavolozza e regole di colore
Tavolozza base in `03` §A3. Regole d'uso:
- **60 / 30 / 10:** 60% sfondo (indaco → blu notte), 30% elementi del giocatore (avorio, teal, oro, menta), 10% accenti (corallo dei nemici, oro delle combo, rosa dei boss).
- **La tinta dei nemici (corallo/rosa) non appare mai** su Core, moduli, UI o testo. Il pericolo ha un colore solo.
- **Forma + colore sempre insieme** (daltonismo): rotondo = tuo, angolare = nemico; le tre categorie di moduli hanno tre silhouette diverse oltre ai tre colori.
- **Contrasto del testo ≥ 4,5:1** sul pannello dietro (avorio su vetro scuro passa); numeri in gioco ≥ 3:1 con alone morbido.
- **Tinte notte, mai nero puro:** il valore più scuro è `#141630`; il bianco più chiaro è l'avorio `#F3E9D7`. Così il bloom resta morbido e lo schermo non "buca".

### 3.3 Forme (catalogo per Blender via MCP)
| Elemento | Forma | Dimensione (unità) | Nota |
|---|---|---|---|
| Core | sfera morbida + 6–8 petali piatti arrotondati | Ø 1,4; petali a raggio 1,5 | respira (scala 1,00 → 1,03, 4 s) |
| Slot | il petalo stesso; vuoto = petalo più scuro; valido = petalo che respira | — | nessun cerchio di portata (D20) |
| Emitter / Scatter / Arc / Lance / Mortar | gemma a punta / tre gemme / gemma con anello / cristallo lungo / gemma tozza | 0,5 | armi: silhouette **appuntita** |
| Amplifier / Lens / Overclock / Echo | anello / lente / anello con tacche / doppio anello | 0,5 | booster: silhouette **anulare** |
| Bank / Salvage / Bulwark / Frost / Capacitor | moneta / cofanetto / scudo tondo / goccia / pila | 0,5 | economia: silhouette **piena e tonda** |
| Drifter | scheggia allungata | 0,35 | corallo |
| Swarmlet | triangolino | 0,2 | in 5, stretti |
| Brute | prisma esagonale spesso | 0,55 | corallo scuro, bordo più spesso |
| Dasher | freccia | 0,35 | scia breve durante lo scatto |
| Splitter | scheggia con una crepa luminosa | 0,4 | la crepa si apre alla morte |
| Warden | scheggia con un alone anulare | 0,45 | l'alone mostra il raggio dello scudo |
| Guardian | cristallo grande con 3 schegge orbitanti | 1,0 | rosa profondo; le schegge diventano i Swarmlet evocati |
| Élite | qualsiasi nemico con **alone doppio** | +10% | mai un colore nuovo |

Livelli dei moduli: L1 = forma base · L2 = una **seconda faccia** più luminosa · L3 = **tre facce** e bordo dorato. Si legge da lontano senza numeri.

### 3.4 Illuminazione e resa (URP, mobile)
- **Camera:** ortografica, inclinata di **35°**; dimensione 9,4 unità in ondata, 3,4 nel negozio, transizione 0,6 s ease-in-out. Nessun movimento di camera durante l'ondata.
- **Luci:** una direzionale calda (`#FFD9A8`, elevazione 35°, azimut da sinistra alto) + ambiente a gradiente (cielo `#3B3F72`, terra `#141630`). Ombre morbide solo a qualità media/alta (1 cascata, risoluzione 1024).
- **Post:** bloom soglia 1,1 intensità 0,3 (leggero: si vede solo sull'emissivo); vignettatura 0,25; **niente** aberrazione cromatica, grana o CRT (nausea, lezione Balatro); un LUT "dusk" (ombre verso il blu, alte luci calde).
- **Sfondo:** gradiente + anelli concentrici al 15–20% + vignettatura. **Fermo.** Solo il Core respira.
- **Budget:** ≤ 40 draw call, ≤ 60k triangoli in scena, 4 materiali (uno per categoria + nemici) con SRP Batcher, ombre e bloom spenti nel livello "basso". Obiettivo: 30 fps su 4 GB con 60 nemici, 60 fps sulla fascia media.

### 3.5 Catalogo delle micro-animazioni (tutte con easing, nessuna a scatti)
| Momento | Animazione | Durata | Curva | Regola |
|---|---|---|---|---|
| Core idle | scala 1,00 → 1,03 | 4 s ciclo | seno | si spegne con "riduci movimento" |
| Modulo idle | sale/scende 0,02 unità | 3 s ciclo | seno, sfasato per slot | idem |
| Arma spara | rinculo 4% verso l'interno | 120 ms | ease-out-back | max 2 volte/s visibili per modulo: se spara più veloce, la linea resta accesa invece di ripetersi |
| Nemico colpito | spinta visiva 3% verso l'esterno | 80 ms | ease-out | numeri solo per uccisioni e colpi > 25% HP |
| Nemico muore | 6–10 scaglie, alpha 1 → 0, scala 1 → 0,6 | 400 ms | ease-out | tetto: 12 morti animate simultanee, le altre svaniscono e basta |
| Pulse | anello 0 → 3,0 unità, alpha 0,6 → 0 | 500 ms | ease-out | + emissivo del Core 0,3 → 1,0 → 0,3 in 600 ms |
| Respinta | scorrimento indietro | 250 ms | ease-out | la simulazione salta, la grafica interpola |
| Carta presa | +12 px, scala 1,05, ombra più ampia | 120 ms | ease-out | vibrazione leggera |
| Calamita | l'oggetto scivola nello slot | 90 ms | ease-out | tick di vibrazione |
| Rilascio valido | assestamento con overshoot 5% | 180 ms | ease-out-back | vibrazione media |
| Rilascio non valido | ritorno al punto di origine | 250 ms | ease-in-out | nessun colore d'errore |
| Merge | attrazione, fusione, scala 1,15 → 1, bagliore | 300 + 150 ms | ease-in, ease-out | vibrazione "successo" (due tocchi) |
| Combo attiva | linea dorata alpha 0 → 0,7 | 400 ms | ease-out | resta accesa nel negozio; in ondata al 30% |
| Fine ondata | anello che pulsa verso l'esterno | 800 ms | ease-out | poi il riepilogo |
| Riepilogo | cifre che scorrono, monete che si aggiungono | 600–1.200 ms | ease-out | **tocco = salta** |
| Core colpito | tinta corallo 0,3 → 0 | 400 ms | ease-out | niente scossa dello schermo |
| Cambio stato (negozio ↔ ondata) | camera + carte che salgono/scendono | 600 ms | ease-in-out | le carte entrano sfalsate di 40 ms |

**Regole globali:** mai più di 2 ripetizioni/s nello stesso punto; nessun flash a schermo intero; con "riduci movimento" restano solo le animazioni funzionali (spari, morti, Pulse) accorciate del 50%; "intensità effetti" scala alpha e numero di scaglie.

### 3.6 Leggibilità a schermo
- **Tre piani di profondità:** sfondo (anelli al 15–20%), arena (Core, moduli, nemici, effetti), interfaccia (vetro scuro al 6% di avorio, bordo 1 px al 12%).
- **Gerarchia di scala:** Core 1,4 > Guardian 1,0 > Brute 0,55 > moduli 0,5 > Drifter 0,35 > Swarmlet 0,2. Ciò che è più grande è più importante.
- **Tipografia:** un solo font arrotondato con licenza aperta (proposta: **Nunito**, SIL OFL), tre pesi. Minimo 14 sp nei numeri di gioco, 16 sp nell'interfaccia, 20 sp per Credits e ondata. Opzione dimensione testo 100–200%.
- **Numeri:** notazione compatta sopra 9.999 (12,3K · 4,5M · 6,7B), avorio; colpi importanti al 150% in oro, senza flash.
- **Interfaccia nel terzo inferiore:** carte (4, a tutta larghezza), sotto Annulla · Rilancia · Next wave; durante l'ondata il Pulse occupa il 60% della larghezza, velocità e pausa il resto. In alto solo tre numeri: integrità, ondata, Credits.

### 3.7 Produzione
- **Blender via MCP:** script parametrici per petali, gemme, anelli, schegge (un file `.blend` per famiglia); export glTF in `Assets/Models/<famiglia>/`; 1 unità = 1 m.
- **Unity via MCP:** 4 materiali URP Lit + 1 Unlit per le linee; volume di post-processing; prefab per Core, modulo (con varianti L1–L3), nemico.
- **Guida di stile** (`docs/style/`): tavolozza, forme, curve di easing come file di riferimento; ogni asset nuovo viene confrontato con la **mood shot** (`03` §A8) prima di entrare nel gioco.
- Icone dei moduli: rese **dal modello 3D** (stessa luce, stessa camera): coerenza gratuita tra gioco e carte.

---

## 4. Primo impatto e conversione sullo store

### 4.1 Icona
- **Un solo soggetto:** il Core con i petali, visto dalla camera del gioco, su gradiente crepuscolare, con **una** linea dorata di combo. Niente testo, niente nemici, niente cerchio di portata.
- Leggibile a **48 px**: la silhouette (sfera + petali) è riconoscibile anche in miniatura.
- 3 varianti per il test A/B della scheda (Play Console → *Store listing experiments*): a) Core solo; b) Core + un modulo dorato; c) Core + tre schegge corallo in avvicinamento (più "gioco", meno "calma").

### 4.2 Screenshot (verticali 9:16; i primi 3 sono quelli che contano)
| # | Scena | Testo (≤ 4 parole) |
|---|---|---|
| 1 | metà ondata, un numero grande "24,6K" in oro, linee di combo accese, sciame che si scioglie | *"I numeri esplodono"* |
| 2 | negozio con una carta trascinata, anteprima "16 → 24 (+50%)" | *"La posizione conta"* |
| 3 | merge in corso, "Livello 2" | *"Unisci e potenzia"* |
| 4 | tre anelli diversi affiancati | *"Ogni run, una build"* |
| 5 | classifica Daily con un fantasma | *"Sfida i loro replay"* |
| 6 | schermata calma con tre righe | *"Offline · Niente timer · Mai pay-to-win"* |
Mai nomi di altri giochi, mai interfaccia finta: **solo gameplay reale** (02 §5), stessa build che il giocatore scaricherà.

### 4.3 Video (15–30 s, senza voce)
Primi 3 secondi: un numero che cresce da 10 a 10M con l'anello che si riempie in time-lapse. Poi: trascina → linea dorata → numeri più grandi → Pulse → Guardian che si scioglie → l'anello finale. Chiusura: le tre righe della schermata 6. Lo stesso video è il formato dei devlog (TikTok/Shorts).

### 4.4 Testo della scheda (bozza)
- **Breve:** *"Difendi il nucleo. Combina i moduli. Numeri che esplodono. Offline, senza timer."*
- **Parole chiave ASO:** tower defense, roguelike, offline, no pay to win, idle defense, merge, strategia (mai nomi di altri giochi, D20).

### 4.5 I primi 10 secondi dentro l'app (dove si perde il 77% in 3 giorni)
| Secondo | Cosa succede | Obiettivo |
|---|---|---|
| 0–1,5 | avvio a freddo; **un solo** logo (≤ 0,8 s) o nessuno; nessun caricamento visibile | avvio < 2 s su fascia media |
| 1,5 | (solo UE/UK) consenso UMP, una schermata | risposta < 5 s |
| 2 | Core che respira, Emitter, pulsante ▶ | **zero menu** |
| 2–6 | tocco → primo Drifter → prima uccisione | prima uccisione entro 6 s dal tocco |
| 20 | primi Credits, primo negozio | prima scelta entro 25 s |
| 30 | prima combo (aha 1) | — |
Nessun login, nessuna richiesta di notifiche, nessuna valutazione dello store, nessun negozio di acquisti nei primi 3 giorni.

**Imbuto misurato** (eventi D13): `app_open → first_tap → first_kill → first_shop → first_combo → wave_2 → act_1_clear`. Obiettivi *(stima)*: first_tap ≥ 95% · first_combo ≥ 85% (= tutorial completato, GDD §17) · act_1_clear ≥ 60% alla prima run. Ogni gradino che perde più del 10% è un bug di design, non del giocatore.

### 4.6 Pre-lancio
- **Pre-registrazione** su Google Play con il video; devlog brevi con lo stesso formato del video (build in public, 02 §5).
- Screenshot dell'anello **condivisibile dal gioco** con un tocco (immagine 9:16 con il seme e il codice del replay): il marketing lo fanno i giocatori.
- La **Daily** visibile già dalla schermata iniziale ("Daily di oggi: 1.284 giocatori") è il gancio per tornare domani.

---

## 5. Cosa entra nella Fase 1 di questo documento
Tutto il §1 (input, FTUE, loop dell'ondata), la P1 del §1.7, il §3.5 (micro-animazioni) e il §3.6 (leggibilità) **con forme semplici**; il §3.3–3.4 (modelli e luce) è Fase 3; il §4 è Fase 6 tranne l'imbuto (§4.5), i cui eventi si definiscono ora nelle interfacce finte. I Grade (§2.2) sono Fase 2.

**Decisioni da confermare:** P1 (risonanza del Pulse) · i dieci Grade · il font (Nunito) · il consiglio contestuale dopo 3 sconfitte · le 3 varianti di icona.
