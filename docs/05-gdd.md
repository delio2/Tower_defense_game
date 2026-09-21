# 05 — Game Design Document (v0.2)

> **Titolo:** da definire · **Genere:** difesa del nucleo roguelite con negozio e combo · **Piattaforma:** Android, poi iOS
> **Orientamento:** verticale · **Pubblico:** adulti 18–45 · **Modello:** onesto, *"mai pay-to-win"* (vedi `02`)
>
> Tutti i numeri sono **valori di partenza v0**, da tarare con il prototipo e con il simulatore automatico (D11).
> Motivazioni: `00`–`04` (in particolare D17–D21). I nomi nel gioco sono in inglese, come nel codice.

---

## 0. Glossario
| Termine | Significato |
|---|---|
| **Core** | il nucleo al centro, da difendere. Se la sua integrità arriva a 0, la run finisce |
| **Ring** | l'anello di slot attorno al Core (6 all'inizio, fino a 8) |
| **Module** | ciò che si monta su uno slot: arma, booster o economia |
| **Neighbours** | i due slot adiacenti sull'anello (l'anello è circolare: il primo e l'ultimo sono vicini) |
| **Merge** | comprare un modulo già posseduto lo unisce a quello esistente e ne alza il livello (massimo 3) |
| **Credits** | la valuta **dentro** la run, per il negozio |
| **Blueprints** | la valuta **permanente**, per sbloccare nuovi moduli e Core. Non compra potenza |
| **Pulse** | l'abilità attiva del Core, con ricarica |
| **Act** | 5 ondate + 1 Guardian. Una run è fatta di 3 atti |
| **Guardian** | il boss di fine atto |
| **Core type** | la variante di Core scelta a inizio run, che cambia le regole (come i mazzi di Balatro) |
| **Grade** | livello di difficoltà aggiuntivo a sblocco (come l'Ascension) |
| **Replay** | versione + seme + modalità + Core + lista di (tick, comando): basta a riprodurre la run |

## 1. Pilastri
1. **Scelte che contano:** negozio, merge e disposizione sull'anello. Ogni ondata cambia la build.
2. **Numeri che esplodono, con calma:** crescita esponenziale leggibile, grafica calma (docs/03).
3. **Rispetto del tempo:** run di 10–15 minuti, niente timer, niente energia, offline.
4. **Onesto:** mai pay-to-win; il competitivo è a dotazione fissa.
5. **Condivisibile:** ogni run è un replay, sfidabile e verificabile.

## 2. Ciclo e durata
| Livello | Durata | Contenuto |
|---|---|---|
| Ondata | circa 25–30 s | Nemici da tutti i lati, i moduli sparano da soli, tu usi il Pulse |
| Negozio | libero (circa 10–20 s) | Compra, unisci, disponi, vendi, rilancia → "Next wave" |
| Atto | circa 4–5 min | 5 ondate + Guardian |
| Run | circa 10–15 min | 3 atti (18 ondate). Se vinci puoi continuare in modalità infinita |
| Meta | settimane | Sblocchi di moduli e Core con i Blueprints, Grade, modalità online |

Salvataggio automatico **a ogni negozio**, con ripresa in qualsiasi momento.

## 3. Arena e unità (deterministico)
- **Tick fisso: 60 al secondo.** Nessun float nello stato di gioco (D11).
- **Unità:** 1 unità = 1000 milli. Il Core sta in (0,0) con raggio 0,7. Gli slot dell'anello sono a raggio 1,5. I nemici nascono a raggio 9,0.
- **Direzioni:** 128 direzioni fisse (tabella intera di coseno e seno ×10.000, uguale su ogni piattaforma).
- **Movimento dei nemici:** **radiale** verso il Core. Ogni nemico ha una direzione e una distanza, e ogni tick la distanza diminuisce della sua velocità. Il respingimento la aumenta.
- **Contatto:** a distanza ≤ 0,7 il nemico colpisce il Core (danno da contatto) e scompare.

## 4. Il Core
- **Integrità:** 100.
- **Pulse:** 20 danni + **respingimento di 1,5 unità** a tutti i nemici entro 3,0 dal Core. Ricarica di 20 s (1.200 tick); è pronto all'inizio di ogni ondata. I moduli possono modificarlo.
- **Core type** (varianti; il prototipo usa Standard):
  | Core type | Regola | Inizio |
  |---|---|---|
  | **Standard** | — | Emitter nello slot 0, 6 Credits |
  | **Merchant** | +1 al tetto dell'interesse, 5 slot | 10 Credits, nessun modulo |
  | **Bastion** | integrità 150, ricarica del Pulse −25% | Bulwark, 4 Credits |
  | **Glass** | tutti i danni ×1,5, integrità 50 | Emitter + Amplifier, 2 Credits |

## 5. Il Ring (anello)
- **6 slot** all'inizio, disposti a esagono. Lo **slot extra** (fino a 8) si compra nel negozio dopo il primo Guardian, a 8 Credits.
- **Vicinato:** ogni slot ha 2 vicini; l'anello è circolare.
- Nel negozio i moduli si **spostano liberamente** (trascinamento o scambio); durante l'ondata sono bloccati.

## 6. Moduli (v0, 14 per il prototipo e l'MVP)
Costo in Credits. Portata in unità, misurata dalla posizione del modulo sull'anello. Ricarica in tick (60 = 1 s).

**Armi**
| Module | Rarità | Costo | Danno | Ricarica | Portata | Comportamento |
|---|---|---|---|---|---|---|
| **Emitter** | C | 3 | 8 | 30 | 4,0 | colpisce il nemico più vicino al Core |
| **Scatter** | C | 4 | 5 ×3 | 45 | 3,5 | colpisce i 3 nemici più vicini al Core |
| **Arc** | U | 5 | 6 | 40 | 4,0 | catena su 4 nemici (salto 1,5), −10% a ogni salto |
| **Lance** | U | 5 | 18 | 90 | 6,0 | trapassa tutti i nemici su una linea verso il bersaglio |
| **Mortar** | R | 7 | 16 | 90 | 7,0 | esplosione di raggio 1,2 sul nemico più lontano in portata (minimo 2,0) |

**Booster** (agiscono sui **due vicini**)
| Module | Rarità | Costo | Effetto (livello 1) |
|---|---|---|---|
| **Amplifier** | C | 3 | danno dei vicini ×1,5 |
| **Lens** | C | 3 | vicini: +1,5 portata e +2 danno fisso |
| **Overclock** | U | 4 | vicini: ricarica −25% |
| **Echo** | R | 6 | ogni colpo dei vicini ne genera un secondo al 50% |

**Economia e utilità**
| Module | Rarità | Costo | Effetto (livello 1) |
|---|---|---|---|
| **Bank** | U | 4 | +1 al tetto dell'interesse e +1 Credit per ondata |
| **Salvage** | C | 3 | +1 Credit ogni 10 uccisioni nell'ondata |
| **Bulwark** | C | 3 | +25 integrità massima; ripara 5 a ogni ondata |
| **Frost** | U | 4 | i nemici entro 3,0 dal Core sono rallentati del 25% |
| **Capacitor** | U | 4 | ricarica del Pulse −20%; il Pulse fa +50% danno |

**Merge (livelli):** comprare un modulo già posseduto (sotto il livello 3) lo **unisce automaticamente**, senza occupare un altro slot.
| Livello | Armi (danno) | Booster ed economia (effetto) |
|---|---|---|
| 1 | ×1,0 | ×1,0 |
| 2 | ×1,8 | ×1,6 (per esempio Amplifier ×1,8 invece di ×1,5) |
| 3 | ×3,0 | ×2,2 |

**Vendita:** metà dei Credits spesi (arrotondata per difetto, minimo 1).
*(Dopo l'MVP: Prism, Singularity, Harvester e altri moduli leggendari; obiettivo circa 40 moduli al lancio.)*

## 7. Formula del danno ("numeri che esplodono")
**Danno del colpo = (danno base × livello + bonus fissi) × prodotto dei moltiplicatori**

- **Bonus fissi:** Lens e simili. **Moltiplicatori:** Amplifier, Core Glass, Echo (50% sul secondo colpo)…
- Gli effetti **si moltiplicano tra loro** (due Amplifier vicini a un'arma = ×2,25): è la fonte della crescita esponenziale.
- **Corazza:** riduzione fissa per colpo, con un danno minimo di 1.
- **Mostrare i numeri:** notazione compatta (1,2K · 3,4M · 5,6B). Numeri piccoli e tenui per i colpi normali, più grandi (senza flash) per i colpi oltre il 25% della vita del bersaglio. Un'opzione permette di nasconderli.

## 8. Nemici (v0)
HP in unità di gioco, alla prima ondata. Velocità in unità al secondo.
| Enemy | HP | Corazza | Velocità | Contatto | Punti | Speciale | Da |
|---|---|---|---|---|---|---|---|
| **Drifter** | 20 | 0 | 0,9 | 5 | 1,0 | — | atto 1 |
| **Swarmlet** | 6 | 0 | 1,3 | 2 | 0,3 | arriva in gruppi da 5 vicini | atto 1 |
| **Brute** | 70 | 3 | 0,55 | 15 | 3,0 | corazzato | atto 1 (ondata 3+) |
| **Dasher** | 14 | 0 | 0,8 | 5 | 1,5 | ogni 3 s scatta ×3 per 0,5 s | atto 2 |
| **Splitter** | 30 | 0 | 0,85 | 6 | 2,0 | alla morte si divide in 2 Swarmlet | atto 2 |
| **Warden** | 40 | 1 | 0,7 | 8 | 3,0 | scudo: −50% danno ai nemici entro 1,5 | atto 3 |
| **Guardian** | 400 | 5 | 0,4 | 40 | — | boss di fine atto; ogni 25% di vita perso chiama 5 Swarmlet | ondata 6 di ogni atto |

- **Élite** (dall'atto 2): HP ×3, corazza +1, alone doppio. Una o due per ondata.
- **Forme:** senza facce e mai infantili; la lingua visiva è in docs/03.

## 9. Ondate e difficoltà
- Indice globale **g = 1…18** (atto a, ondata w: g = 6(a−1) + w; w = 6 è il Guardian).
- **HP:** `hp(g) = 1.20^(g−1)` (ondata 18 ≈ 22×). **Budget:** `budget(g) = 8 × 1.10^(g−1)` punti (ondata 18 ≈ 40).
- I nemici entrano in **20 s**, a intervalli regolari, da **direzioni casuali ma dal seme**. Regole: un nuovo tipo entra prima **da solo**; massimo 3 tipi per ondata; l'ondata 3 di ogni atto è "a tema".
- **Anteprima:** nel negozio si vede la composizione dell'ondata successiva (icone e quantità).
- Calcoli interi o a virgola fissa, senza `Math.Pow` (non è deterministico tra piattaforme): la crescita si calcola per moltiplicazioni successive.
- **Grade** (Ascension): dopo la prima vittoria si sbloccano i Grade 1–10 (nemici +10% HP, meno Credits, Guardian con scudo, un'élite in più…).
- **Modalità infinita** dopo la vittoria: la crescita continua, per le classifiche e per le build folli.

## 10. Economia e negozio
- **Credits iniziali:** secondo il Core type (Standard: 6).
- **A fine ondata:** 4 Credits + **interesse** (1 ogni 5 posseduti, massimo 5) + 3 dopo un Guardian.
- **Negozio:** **4 offerte**. Probabilità: C 60% · U 30% · R 10% (le R solo dall'atto 2). Solo moduli sbloccati.
- **Rilancio:** 1 Credit, +1 per ogni altro rilancio nello stesso negozio.
- **Comprare:** serve uno slot libero, oppure un merge. Con l'anello pieno bisogna prima vendere.
- **Slot extra:** offerta fissa dopo il primo Guardian (8 Credits, fino a 8 slot).
- **Anteprima dell'ondata** sempre visibile nel negozio.

## 11. Progressione permanente (senza grind di potenza)
- **Blueprints:** 1 per ondata superata, 3 per Guardian, +bonus Grade.
- **Archive** (lo sblocco): spendi i Blueprints per aggiungere **nuovi moduli al pool** e **nuovi Core type**. **Nessun potenziamento di statistiche.**
- Obiettivo: sblocco completo in circa 60–80 run *(stima)*. Tempi e accelerazione onesta in `02`.
- Nelle **modalità competitive** si usa un pool e un Core **fissi e uguali per tutti**: lo stato degli sblocchi non conta.

## 12. Modalità
| Modalità | Fase | Regole |
|---|---|---|
| **Run** | lancio | Scegli Core e Grade; offline |
| **Daily Run** | lancio | Seme del giorno, Core e pool fissi; classifica; fantasmi dei migliori |
| **Weekly Run** | lancio | Come sopra, settimanale, con regole speciali (per esempio "solo booster rari") |
| **Endless** | lancio | Dopo la vittoria: crescita infinita |
| **Leagues** | aggiornamento 1 | Gruppi da 30, settimanali, promozioni e retrocessioni; seme e dotazione uguali per il gruppo |
| **Duel** | aggiornamento 1 | Stesso seme di un avversario reale (il suo replay); si vede il suo andamento; rating di abilità |
| **Siege** | aggiornamento 2 | Componi un'ondata d'attacco con un budget; gli altri la affrontano; premi se cadono |
| **Community Guardian** | aggiornamento 2 | Boss settimanale con vita condivisa da tutti |

**Punteggio competitivo:** ondate superate (prima) → danno totale (seconda chiave) → meno tick impiegati (terza).

## 13. Replay e verifica (D19)
- **Formato:** versione del gioco + versione del bilanciamento + modalità + seme + Core type + Grade + lista di (tick, comando).
- **Comandi:** `Buy(offerIndex, slot)` · `Sell(slot)` · `Move(from, to)` · `Undo` · `Reroll` · `StartWave` · `Pulse`. La pausa e la velocità **non** sono comandi: non cambiano il risultato.
- **`Undo`** ripristina lo stato del negozio prima dell'ultimo `Buy`, `Sell` o `Move` della visita corrente (a più livelli). `Reroll` e `StartWave` svuotano la pila: il rilancio non si può annullare, perché si vedrebbero le offerte future gratis. `Undo` non usa il generatore casuale, quindi resta deterministico e registrato nel replay.
- **Verifica:** rigiocando il replay si deve ottenere lo stesso hash finale e lo stesso punteggio. In locale subito (test); sul server con Cloud Code C# dall'aggiornamento 1.
- I replay valgono **solo per la stessa versione di bilanciamento**: classifiche e fantasmi sono divisi per versione o stagione.

## 14. Primo avvio (FTUE)
| Tempo | Evento |
|---|---|
| 0 s | (solo UE/UK) consenso → atto guidato, effetti ridotti, nessun menu |
| circa 5 s | Il Core ha già un Emitter: "Tocca Next wave". 5 Drifter arrivano e l'Emitter li abbatte |
| circa 30 s | **Primo negozio (guidato):** "Compra l'Amplifier e mettilo **accanto** all'Emitter" → nell'ondata successiva i numeri salgono di ×1,5 (**primo "aha"**) |
| circa 60 s | Arriva uno sciame: "Tocca il Core per il **Pulse**" → onda che respinge i nemici (**secondo "aha"**) |
| circa 90 s | Secondo negozio: "Compra un altro Emitter" → **merge** a livello 2 |
| poi | Il resto si scopre giocando; Archive e Daily si sbloccano uno alla volta nelle prime 3 run |

Al massimo **una riga di testo** per suggerimento.

## 15. Interfaccia (verticale, una mano)
- **In alto:** integrità del Core · ondata X/18 · Credits.
- **Al centro:** l'arena circolare con il Core e l'anello.
- **In basso, durante l'ondata:** grande pulsante **Pulse** con la ricarica visibile · velocità 1x/2x/3x · pausa.
- **In basso, nel negozio:** 4 **carte offerta** (icona, nome, costo, effetto breve) · Rilancio · **Annulla** · **Next wave**. Le combo attive si vedono come **linee morbide** tra i vicini.
- Pulsanti di almeno 48 dp, pochissimo testo, icone.

### 15.1 Interazione nel negozio: trascinare (dettagli in docs/03 parte C)
| Gesto | Risultato |
|---|---|
| Trascina una **carta** su uno slot libero | Compra e piazza il modulo |
| Trascina una carta su un **modulo uguale** | **Merge**, con la sua animazione |
| Trascina un **modulo** su un altro slot | Lo sposta (scambio se lo slot è occupato) |
| Trascina un modulo sulla **zona Vendi** | Lo vende (ricavo mostrato prima di lasciare) |
| Tocca una carta e poi uno slot | Alternativa senza trascinamento |
| **Annulla** | Annulla l'ultimo acquisto, vendita o spostamento del negozio (non il rilancio) |

- **Durante il trascinamento:** l'oggetto sta **sopra il dito**; gli slot validi si illuminano; la **calamita** aggancia lo slot vicino; in alto compare l'**anteprima** dell'effetto (per esempio "danni al secondo: 16 → 24 (+50%)", "Livello 2 → ×1,8", "Vendi: +2").
- **Anteprima calcolata dalla simulazione** (funzione di sola lettura su una copia dell'anello): quello che vedi è esattamente quello che succede.

### 15.2 Opzioni (dal prototipo)
Riduci movimento · intensità degli effetti · numeri dei danni (tutti / solo grandi / nessuno) · vibrazioni · velocità 1x/2x/3x. In seguito: dimensione del testo, daltonismo, alto contrasto (docs/03 parte C §7).

## 16. Architettura tecnica (aggiornata)
| Modulo | Responsabilità |
|---|---|
| `Simulation` | ciclo a tick fisso (60/s), deterministico, interi; nessun tipo Unity; assembly separato (asmdef senza UnityEngine) |
| `Arena` | direzioni (tabella intera), posizioni, portate |
| `Core` / `Pulse` | integrità, abilità, respingimento |
| `Modules` / `Ring` | definizioni, slot, vicinato, calcolo dei moltiplicatori, merge |
| `Enemies` / `Waves` | tipi, movimento radiale, spawn dal seme, anteprima |
| `Economy` / `Shop` | Credits, interesse, offerte, rilancio, vendita |
| `Replay` | registrazione, formato, riproduzione, verifica (hash) |
| `Presentation` | grafica calma, numeri, effetti: legge lo stato, non lo modifica |
| `Meta` | Blueprints, Archive, sblocchi, Grade |
| `Save` | salvataggio locale a ogni negozio + cloud |
| `Services` | classifiche, replay online, analytics, pubblicità, acquisti (dietro interfacce; **finti durante lo sviluppo**, D13) |

- **RNG a flussi separati:** Waves (composizione e direzioni), Shop (offerte e rilanci), Effects. Le scelte del giocatore non cambiano le ondate.
- **Test:** determinismo, verifica dei replay, economia, merge, vicinato, formule; **bot di bilanciamento** che gioca migliaia di run.

## 17. Obiettivi (KPI)
| Metrica | Obiettivo |
|---|---|
| Tutorial completato | ≥ 85% |
| D1 / D7 / D30 | ≥ 35% / ≥ 12% / ≥ 5% |
| Durata media della run | 10–15 min |
| Run per giocatore al giorno | ≥ 2 |
| Voto sullo store | ≥ 4,5★ |

## 18. Domande aperte (da risolvere nel prototipo)
- [ ] Il negozio con l'anello è divertente **già con forme semplici**?
- [ ] Le combo si capiscono senza spiegazioni (linee tra i vicini, numeri)?
- [ ] La durata reale di ondate e negozi è 10–15 minuti per run?
- [ ] Il Pulse è una scelta interessante o un pulsante da premere appena è pronto?
- [ ] I numeri grandi restano leggibili e calmi?
- [ ] Un anello di 6 slot basta per creare build diverse?
- [ ] **Prima sonda del bot (21/09/2026, 100 semi, 1 atto):** un bot "ingenuo" (compra a caso, primo slot libero, Pulse quando i nemici sono vicini) vince il **90%**; le sconfitte sono tutte alle ondate 5–6 (Guardian). Combattimento medio circa **24 s per ondata**. → L'atto 1 va bene come ingresso; **atti 2–3 da tarare** (crescita e nuovi nemici).

## 19. Piano del prototipo (2–3 settimane)
1. **Simulazione v2** (riusa RNG, hash e comandi): arena radiale, Core, 3 nemici (Drifter, Swarmlet, Brute), 6 moduli (Emitter, Scatter, Amplifier, Lens, Bank, Bulwark), negozio con merge, Pulse, 1 atto. **Test.**
2. **Replay:** registrazione, riproduzione e verifica dell'hash (test automatico).
3. **Presentazione calma** con forme semplici e interfaccia provvisoria, **con l'interazione tattile** (§15.1): trascinamento con calamita e anteprima, vendita trascinando, annulla, merge animato, conteggio di fine ondata, vibrazioni, opzioni base.
4. **Test con 3–5 persone** (prima senza audio).
5. Decisione: avanti, correggere o cambiare.

**Criteri per dire "funziona":** almeno 3 tester su 5 chiedono di rigiocare; capiscono il vicinato senza spiegazioni entro il secondo negozio; nessuno resta bloccato più di 10 s; la durata reale è misurata.
