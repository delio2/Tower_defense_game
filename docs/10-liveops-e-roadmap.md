# 10 — Competizione, modalità, lancio e roadmap a 6 mesi (brief di Fase 4–7, v1 · 21/09/2026)

> **Ruolo del documento:** come il gioco vive a lungo (Daily, classifiche, modalità) e **il calendario operativo** mese per mese fino al rilascio.
> Questo calendario **sostituisce** quello indicativo di `06` §3 (le fasi di `06` restano; le date si comprimono). Le voci **PROPOSTA** vanno confermate in `04`.
> Vincoli che non si toccano: offline completo, replay deterministici (D11, D19), dotazione fissa nel competitivo, UGS come backend (D21), niente "The Tower" da nessuna parte (D20).

---

## 1. Daily Challenge e classifiche

### 1.1 La Daily in una tabella
| Parametro | Valore | Perché |
|---|---|---|
| Quando | un nuovo seme ogni giorno a **00:00 UTC**; la run si può iniziare fino alle 23:59 UTC e finire anche dopo | un solo confine mondiale, deciso dal server (GDD §12) |
| Seme | `SHA-256(sale_del_server ∥ data)` → i primi 64 bit. **Il sale è pubblicato dal server ogni giorno**, non è nell'app | senza sale l'app potrebbe calcolare i semi futuri e allenarsi offline; con il sale si conosce solo il seme di oggi |
| Offline | il seme si scarica al primo momento online della giornata (pochi byte, in cache); chi è offline **tutto il giorno** gioca una "Daily locale" con seme dalla data, valida per i Blueprints ma **fuori classifica** | l'offline resta completo; la classifica esiste solo online per definizione |
| Dotazione | Core Standard, **pool fisso** (i 14 dell'MVP, poi ruota per stagione), Grade 0, nessun bonus da Archive, pubblicità o Pass | condizioni uguali per tutti (D19) |
| Tentativi | **uno**. La run è salvata a ogni negozio: si può chiudere e riprendere entro la finestra | "una sola run" è il cuore del formato (Spelunky, Slay the Spire) |
| Dopo la Daily | "Rigioca questo seme" illimitato, **fuori classifica**, marcato nel replay come `practice` | trasforma la frustrazione in allenamento senza inquinare la classifica |
| Punteggio | ondate superate (Endless compreso, se vinci si continua) → danno totale → **meno tick** | tre chiavi già nel GDD; il tempo è quello simulato, non l'orologio, quindi la velocità 3x non conta |
| Premi | +3 Blueprints per averla giocata, +2 se nella metà alta *(PROPOSTA)*; **nessun premio di potenza** | il premio è la classifica |
| Fantasmi | scarichi il replay di un giocatore (top 10, amici, o "uno vicino a te") e giochi lo stesso seme vedendo la sua ondata e il suo DPS in tempo reale in un angolo | il "multiplayer" più economico che esiste: un file di pochi KB |

**Weekly** (`05` §12): stesso schema, seme settimanale (lunedì 00:00 UTC), con **un mutatore** (§2.2) e fino a **3 tentativi** *(PROPOSTA)*: il migliore conta. È la Daily per chi non può giocare ogni giorno.

### 1.2 Classifiche: struttura
| Livello | Chi vede cosa | Note |
|---|---|---|
| **Globale** | tutti, top 100 + la tua posizione ± 5 | una per Daily, Weekly, Endless (per Grade) |
| **Paese** | top 100 del tuo Paese | dal Paese dell'account (UGS), non dal GPS |
| **Amici** | chi hai aggiunto per codice (o Play Games, facoltativo) | è la classifica che conta davvero per la retention |
| **Fasce (tier)** | Diamante top 1% · Oro 10% · Argento 25% · Bronzo 50% · Ferro resto | calcolate alla chiusura della Daily; danno un badge sul riepilogo, nient'altro |
| **Leghe** (agg. 1) | gruppi da 30 con lo stesso seme; promozione/retrocessione settimanale | vedi §2.4: **solo se** i segnali della community lo chiedono |

Ogni classifica è divisa per **versione di bilanciamento** (GDD §13): quando cambiano i numeri, riparte una stagione. Le stagioni sono lunghe (6–10 settimane) e annunciate nel gioco con una riga.

### 1.3 Anti-cheat: perché chi bara e chi spende non domina
| Minaccia | Difesa | Dove |
|---|---|---|
| Punteggio inventato (memoria modificata, richiesta falsa) | il client **non manda un punteggio**: manda il **replay**. Il server (Cloud Code C#, stesso assembly `Simulation`) lo **rigioca** e calcola lui il punteggio. Hash diverso = scartato, silenziosamente | Fase 4–5 |
| Replay costruito da un bot esterno ("tool-assisted") | è l'unico attacco che la verifica non ferma. Mitigazioni: (a) il replay contiene i **tick** dei comandi, e un pattern "sovrumano" (comandi a tick perfetti, negozi risolti in 0 tick) si rileva statisticamente; (b) i replay dei top 100 sono **pubblici**: la community li guarda; (c) segnalazione in un tocco dal fantasma; (d) revisione manuale dei top 10 nelle prime settimane | Fase 5 |
| Più account / più tentativi | account UGS anonimo legato all'installazione + Play Games facoltativo; **il primo replay valido del giorno conta**, i successivi sono rifiutati; reinstallare per riprovare è possibile ma il nuovo account riparte da zero (niente Archive, niente storico) | Fase 5 |
| Versione modificata dell'app | il replay porta la versione del gioco e del bilanciamento; il server accetta solo le versioni firmate note; Play Integrity API come secondo controllo *(PROPOSTA, solo se servono prove)* | Fase 5 |
| Chi spende | **non c'è niente da comprare che entri nella Daily**: pool fisso, niente Second Chance, niente Free Reroll, niente moltiplicatori (`08` §2.4) | per costruzione |
| Chi ha tutto sbloccato | l'Archive non conta nel competitivo: pool fisso uguale per tutti | per costruzione |
| Flood di replay | limite: 1 invio valido per Daily, 3 per Weekly, 10 al giorno per Endless; dimensione massima 64 KB | Fase 5 |

**Sanzioni:** nessun ban "rumoroso". Un replay non valido non compare, e basta. Un account con 3 replay non validi finisce in una **lista ombra**: vede la sua posizione, gli altri no. Si esce dalla lista con 14 giorni puliti. Le liste ombra si rivedono a mano finché i numeri sono piccoli.

### 1.4 Cosa serve sul server (UGS) e cosa costa
| Servizio | Uso | Soglia gratuita (da verificare in Fase 4, `04` D21) |
|---|---|---|
| Authentication | account anonimi, collegamento Play Games | gratis |
| Cloud Code (C#) | `SubmitReplay` (verifica e punteggio), `GetDailySeed`, `GetGhost` | a consumo; una verifica = una risimulazione di ~1–3 s di CPU |
| Leaderboards | Daily, Weekly, Endless × Grade, per Paese e amici | a consumo |
| Cloud Save | Archive, opzioni, run in corso (il locale fa fede, `08`) | a consumo |
| Remote Config | sale del giorno, versione di bilanciamento attiva, mutatore della settimana, interruttori (Daily on/off, modalità) | gratis |
| Cloud Storage (o un bucket) | replay dei top 100 (pochi KB l'uno) | trascurabile |
Stima *(da confermare con il calcolatore UGS)*: sotto i 10k giocatori attivi al giorno si resta nella fascia gratuita o vicino. **Interruttore:** se i costi superano il budget, la verifica passa "a campione" (top 100 sempre, il resto al 10%) senza cambiare il client.

---

## 2. Modalità di gioco

### 2.1 Le modalità al rilascio (tutte offline, tutte replay)
| Modalità | Cosa cambia | Durata | Quando si sblocca |
|---|---|---|---|
| **Run** | il gioco base: Core, Grade, Archive | 10–15 min | sempre |
| **Daily / Weekly** | §1 | 10–15 min | run 2 / prima vittoria |
| **Endless** | dopo la vittoria: la crescita continua; ogni 6 ondate un Guardian più forte | finché regge | prima vittoria |
| **Guardian Gauntlet** *(PROPOSTA, la "Boss Rush")* | **6 Guardian di fila**, senza ondate normali; tra uno e l'altro un negozio con **10 Credits fissi** (niente interesse); ogni Guardian aggiunge una regola (scudo, scorta doppia, evoca élite…). Dotazione fissa, seme settimanale, classifica | **4–6 min** | prima vittoria |
| **Surge** *(PROPOSTA, la "Endless modificata")* | Endless in cui **ogni 3 ondate arriva un mutatore casuale dal seme** (§2.2), cumulativo. Il giocatore vede il prossimo mutatore nel negozio, come vede l'ondata | finché regge | 5 vittorie |

Perché queste due: il **Gauntlet** risponde a "ho 5 minuti" e mette alla prova la build contro l'esame puro; **Surge** risponde a "l'Endless dopo un po' è uguale" (la critica a Vampire Survivors) senza toccare il bilanciamento base. Entrambe riusano tutto: nessun contenuto nuovo, solo regole.

### 2.2 Mutatori (per Weekly e Surge, dal seme)
Dodici regole leggibili in una riga, cumulabili in Surge:
1. Anello a 5 slot · 2. Solo booster nel negozio per 3 negozi · 3. Nemici +50% velocità, −30% HP · 4. Il Pulse ricarica in 10 s ma fa metà danno · 5. Interesse doppio, Credits base dimezzati · 6. Ogni ondata è "a tema" · 7. Le élite arrivano dall'ondata 1 · 8. Il rilancio è gratis, ma le offerte sono 3 · 9. I moduli venduti restituiscono tutto, ma non si può annullare · 10. I Guardian evocano il doppio · 11. Corazza +2 a tutti · 12. Merge fino a L4 (×4,5) *(da tarare)*.
Ogni mutatore è un **flag nel `RunConfig`**, quindi nel replay, quindi verificabile.

### 2.3 Modalità con "handicap" volontario
Il **Grade** (`07` §2.2) *è* la sfida con handicap: dieci regole cumulative scelte dal giocatore. Non serve una modalità a parte. Nelle classifiche Endless ogni Grade ha la sua classifica.

### 2.4 La porta aperta al multiplayer: i tre livelli e i loro cancelli
Il multiplayer è **solo asincrono** (D19) e si aggiunge **solo se i segnali lo chiedono**. La Daily con fantasmi è già "multiplayer" nel senso che conta; il resto è opzionale.

| Livello | Cosa | Cancello per costruirlo (tutti e tre) | Costo |
|---|---|---|---|
| **0 — al rilascio** | Daily/Weekly, classifiche, fantasmi, amici per codice | — | già nel piano |
| **1 — Leghe e Duel** | gruppi da 30 con lo stesso seme, promozioni; Duel = stesso seme di un avversario reale con rating | D30 ≥ 5% · ≥ 25% dei giocatori attivi gioca la Daily · ≥ 3.000 giocatori attivi al giorno *(per riempire le leghe senza bot)* · richiesta esplicita nei sondaggi/Discord (≥ 30% "voglio leghe") | 4–6 settimane |
| **2 — Siege e Community Guardian** | ondate costruite dai giocatori; boss con vita condivisa | Leghe attive da 8 settimane con partecipazione ≥ 40% · costi UGS sotto budget · segnalazioni di cheat < 1% | 6–8 settimane |

Se un cancello non si apre, la modalità **non si costruisce** e il tempo va su contenuto e Grade. Questo è scritto qui perché la tentazione di "aggiungere multiplayer" è il modo più rapido per non uscire mai.

---

## 3. Strategia di lancio e marketing iniziale (a basso costo)

### 3.1 Il principio
Il gioco produce da solo il materiale: **ogni run è una clip** (numeri che salgono), **ogni anello è un'immagine**, **ogni replay è una sfida**. Il marketing consiste nel far uscire queste cose dal gioco con un tocco e nel mostrarle con costanza. Budget in denaro: **0 € fino al soft launch**, 300–1.000 € in Google Ads al soft launch (`02` §5).

### 3.2 Canali, in ordine di rendimento atteso per costo
| Canale | Cosa | Cadenza | Da quando |
|---|---|---|---|
| **Video brevi** (TikTok, Shorts, Reels) | clip di 10–20 s **dal gioco**: una combo che si accende, un Guardian che si scioglie, "10 → 10M". Nessuna voce, un testo di 4 parole. Formato identico al video dello store (`07` §4.3) | 2 a settimana | mese 2 (appena la grafica regge) |
| **Reddit** | devlog onesti con GIF: r/TowerDefense, r/roguelikes, r/incremental_games, r/AndroidGaming, r/iosgaming, r/IndieDev, r/Unity3D; **chiedere feedback, non pubblicizzare** | 1 al mese per subreddit, mai lo stesso post in due | mese 2 |
| **Demo giocabile sul web** *(PROPOSTA)* | l'atto 1 in **WebGL su itch.io** (il modulo è installato; la simulazione è pura C#): "prova in 5 minuti, poi pre-registrati". È il modo più economico per trasformare una GIF in un giocatore | una, aggiornata a ogni gate | mese 3 |
| **Pre-registrazione Google Play** | attiva con il video; premio alla pre-registrazione: **un tema estetico** (mai potenza); tappe pubbliche (1k, 5k, 10k) | dal mese 4 | mese 4 |
| **Discord** | il canale dei tester (servono 12 per il test chiuso, D13) e poi della community; sondaggi per i cancelli del §2.4 | continuo | mese 2 |
| **Creator piccoli** (5–50k iscritti, TD/roguelite/incrementali) | codici e una build in anticipo, nessun pagamento; un press kit (icona, 6 screenshot, video, 3 righe, senza nomi di altri giochi) | 20 contatti al mese 5 | mese 5 |
| **Programmi Google** | Indie Games Festival / Accelerator quando aprono le candidature; Play Pass da valutare per il modello A | quando aprono | mese 4+ |
| **ASO** | titolo + sottotitolo con le parole chiave (`07` §4.4), esperimenti sulla scheda (3 icone, 2 set di screenshot) | dal soft launch | mese 6 |

### 3.3 Cosa **non** facciamo
Pubblicità ingannevoli o "fake gameplay"; confronti con altri giochi (D20); influencer a pagamento prima di avere i dati; spam cross-subreddit; annunci "coming soon" senza una data.

### 3.4 Obiettivi *(stima, da rivedere con i primi dati)*
| Momento | Obiettivo |
|---|---|
| Fine mese 3 (demo web) | 500 partite giocate sulla demo; 100 iscritti al Discord |
| Fine mese 5 | 3.000–5.000 pre-registrazioni; 12+ tester attivi |
| Soft launch | D1 ≥ 35%, D7 ≥ 12% (GDD §17); CPI < 1 € sui video organici migliori |
| Rilascio | 10k download nel primo mese *(fascia bassa dell'obiettivo di `00`)* |

---

## 4. Roadmap a 6 mesi (ottobre 2026 → marzo 2027)

### 4.0 Le tre scelte che rendono possibili i 6 mesi
1. **Il bilanciamento è continuo, non un mese.** Il bot (`06` Fase 1) gira ad ogni modifica; il mese 5 è la *taratura finale con i dati dei tester*, non la prima.
2. **La grafica entra dal mese 2, in parallelo al contenuto**, perché il Gate 1 (tattile, forme semplici) si chiude a fine mese 1. Se il Gate 1 slitta, slitta tutto: è il rischio numero uno e va detto.
3. **Fuori dal rilascio:** Season Pass, Leghe, Duel, Siege, iOS, le lingue oltre EN/IT (+ ES/PT-BR/DE/FR solo per lo store). Sono tutte "porte aperte" con un cancello (§2.4, `08` §3).

Ogni mese = una fase di `06`, un **gate** e una riga "se fallisce".

### Mese 1 — Ottobre 2026 · Core mechanics & prototipo tattile (= Fase 1)
- Bot runner + CSV; ScriptableObject `ContentLoader`; `BuySlot`; notazione compatta.
- UI Toolkit al posto di IMGUI; trascinamento con calamita e anteprima; Annulla e Vendi; merge animato; riepilogo di fine ondata; opzioni base; vibrazioni.
- Prima build Android di sviluppo sul telefono.
- **Avviare la pratica dell'account Google Play** (tempi lunghi, D13).
- **Gate 1** (`06`): 3–5 tester, 3/5 chiedono di rigiocare, vicinato capito entro il secondo negozio.
- *Se fallisce:* si resta sul mese 1 (regole del vicinato, slot, feedback). Non si apre la grafica.

### Mese 2 — Novembre 2026 · UI & integrazione della grafica (= Fase 3, prima metà) + contenuto (= Fase 2, prima metà)
- **Mood shot** approvata entro la settimana 1 (`03` §A8): è il cancello della grafica.
- Blender via MCP: Core, 7 moduli esistenti, 4 nemici; URP con luce e post; camera a 35°; livelli di qualità.
- UI definitiva per ondata, negozio, fine run, pausa (`09` §2.2–2.4, §2.8); token in `Theme.uss`.
- In parallelo, simulazione: Arc, Lance, Mortar, Echo, Salvage, Frost, Capacitor (14 moduli); Dasher, Splitter, Warden; élite; atti 2–3; Guardian con scorte variate.
- Primi video brevi e Discord.
- **Gate 2a:** 30 fps sul telefono di riferimento con 60 nemici; 5 tester dicono "non sembra un prototipo".
- *Se fallisce:* si tagliano ombre e bloom dal livello base; la grafica dei moduli mancanti usa forme semplici colorate finché non c'è tempo.

### Mese 3 — Dicembre 2026 · Progressione (= Fase 2, seconda metà + Fase 4, prima metà)
- Core type (4), Grade 1–3, Endless, salvataggio e ripresa a ogni negozio, replay `R2`.
- Archive ad albero e Blueprints (`08` §1.3–1.4); "prossimo sblocco tra N"; schermata Archive (`09` §2.5).
- FTUE completa (`07` §1.5) e rivelazione progressiva (`09` §3.2); Home (`09` §2.1).
- Bilanciamento v0.3 con il bot (obiettivi in `06` Fase 2); simulatore della meta.
- **Demo WebGL dell'atto 1 su itch.io** *(PROPOSTA)*.
- **Gate 2b + 4:** run intera 10–15 min; 3 build vincenti diverse; un tester nuovo arriva alla terza run con uno sblocco senza aiuto.
- *Se fallisce:* meno moduli al rilascio (12 invece di 14), Grade solo 1–2; l'Archive resta.

### Mese 4 — Gennaio 2027 · LiveOps & backend delle classifiche (= Fase 4, seconda metà + Fase 5, prima metà)
- Daily/Weekly con seme dal server e "Daily locale"; codice di replay condivisibile; fantasmi; Guardian Gauntlet e mutatori (Surge se c'è tempo, altrimenti aggiornamento 1).
- UGS: Authentication, Cloud Code `SubmitReplay` (risimulazione), Leaderboards (globale, Paese, amici), Cloud Save, Remote Config; stima dei costi scritta in `04`.
- Interfacce `Services` con implementazioni vere dietro le finte; consenso UMP; analytics e crash report (D24); privacy policy.
- Pre-registrazione aperta con il video; classifica dei replay pubblici.
- **Gate 5a:** un replay inviato da un telefono viene verificato sul server e compare in classifica; un replay manomesso no; costi stimati sotto budget.
- *Se fallisce (costi):* verifica a campione (§1.4); *(server):* la Daily parte con classifica **locale** e la globale arriva con l'aggiornamento 1 — il gioco resta completo.

### Mese 5 — Febbraio 2027 · Bilanciamento ed economia con i tester (= Fase 5, seconda metà)
- **Test chiuso Google Play**: 12+ tester × 14 giorni (obbligatorio per l'account personale). Ciclo settimanale dati → bot → numeri → build.
- Taratura finale: curva degli atti 2–3, Guardian, economia dei Credits (`08` §1.2), Blueprints (`08` §1.4), Grade.
- Entrambi i modelli di monetizzazione dietro un flag (`08` §2); temi estetici (3); IARC; dichiarazione AI; **un'ora con l'avvocato IP** (D20); nome definitivo (D25).
- Localizzazione dello store in ES/PT-BR/DE/FR; il gioco in EN/IT.
- Creator: 20 contatti con codici.
- **Gate 5b:** 14 giorni di test completati; crash-free ≥ 99%; D1 dei tester ≥ 35%; nessuna strategia del bot sopra l'85%.
- *Se fallisce (retention):* si allunga il test di 2 settimane e si rimanda il soft launch: **qui si spende tempo, non dopo**.

### Mese 6 — Marzo 2027 · QA, soft launch e rilascio (= Fase 6 compressa + Fase 7)
- Settimane 1–3: **soft launch** in due Paesi simili (uno per modello, o A/B); 300–1.000 € di Google Ads sui video organici migliori; esperimenti sulla scheda (icone, screenshot).
- QA: matrice di 6 telefoni (2 fascia bassa, 2 media, 2 alta), Android 8 → 16; test di ripresa (chiusura durante l'ondata, batteria, chiamate); test di rete (offline totale, rete lenta, cambio rete a metà invio).
- Settimana 4: **decisione del modello** (ricavo per installazione + voto) e **rilascio** su Google Play, oppure rinvio di 2–4 settimane con un piano scritto.
- **Gate 6:** KPI del GDD §17 raggiunti o piano correttivo.
- *Dopo:* aggiornamento 1 (+8–12 settimane) con ciò che i cancelli hanno aperto (§2.4), Surge se rimandata, Pass se D30 ≥ 7%; iOS quando c'è un Mac o Build Automation.

### 4.1 Vista d'insieme
| Mese | Fase di `06` | Gate | Marketing |
|---|---|---|---|
| 1 Ott | 1 | tattile | account Play avviato |
| 2 Nov | 3a + 2a | grafica e prestazioni | video, Discord |
| 3 Dic | 2b + 4a | run completa, progressione | demo web |
| 4 Gen | 4b + 5a | classifiche verificate | pre-registrazione |
| 5 Feb | 5b | test chiuso, retention | creator |
| 6 Mar | 6 + 7 | soft launch → rilascio | ads, ASO |

### 4.2 Rischi della compressione (da guardare ogni mese)
| Rischio | Segnale | Decisione già presa |
|---|---|---|
| Gate 1 slitta | fine ottobre senza 3/5 "rigioco" | tutto slitta di pari passo: **non** si salta il gate |
| Grafica e contenuto insieme nel mese 2 saturano una persona | fine novembre con 10 moduli invece di 14 | si rilascia con 12 moduli; i 2 mancanti nell'aggiornamento 1 |
| Account Google Play in ritardo | pratica aperta dopo il mese 2 | il test chiuso può partire su un canale interno via link, ma i 14 giorni valgono solo sulla Play Console: avviare **nel mese 1** |
| UGS più caro del previsto | stima a gennaio | verifica a campione; classifica locale al rilascio |
| Retention dei tester bassa | Gate 5b | si allunga febbraio; marzo diventa aprile. Meglio un mese in più che un rilascio a 3,8★ |
| "Aggiungiamo il multiplayer" | qualsiasi mese | §2.4: solo con i cancelli aperti, mai prima del rilascio |

---

## 5. Decisioni da confermare (poi in `04`)
Premio Daily +2 per la metà alta · Weekly a 3 tentativi · Guardian Gauntlet e Surge · Play Integrity solo se servono prove · demo WebGL su itch.io · i tre cancelli del multiplayer (§2.4) · le tre scelte della compressione (§4.0) e la data di rilascio a **fine marzo 2027** con possibile rinvio ad aprile.
