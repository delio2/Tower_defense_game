# 06 — Piano di sviluppo a fasi (v1, 21/09/2026)

> **Obiettivo:** portare il gioco dal prototipo attuale al soft launch in **6–9 mesi** *(stima)*, una fase alla volta, con un **criterio di uscita** misurabile per ogni fase.
> **Regola d'oro (01 §11):** se il negozio con l'anello non diverte con forme semplici, non si va avanti con la grafica.
> Fonte di verità per meccaniche e numeri: `05-gdd.md`. Questo file dice **in che ordine** costruirle e **perché**.

---

## 0. Come lavoriamo
1. **Una fase alla volta.** Ogni fase ha consegne, test e un criterio di uscita ("gate"). Non si apre la fase successiva finché il gate non è passato o consapevolmente rimandato (scritto in `04`).
2. **Simulazione prima, grafica dopo.** Ogni meccanica nasce nell'assembly `Simulation` con un test; la presentazione la legge soltanto.
3. **Il bot gioca prima dei tester.** Ogni cambio di bilanciamento passa dal simulatore automatico (migliaia di run in pochi secondi) prima di arrivare a una persona.
4. **Tester veri a ogni gate** (3–5 persone nelle prime fasi, 12+ nel test chiuso), **prima senza audio** (D1).
5. **Calmo di default** (docs/03 §A1) e **onesto per costruzione** (02 §1): non sono rifiniture finali, sono vincoli fin dalla prima riga.
6. **Tutto è un replay** (D19): ogni funzione che tocca lo stato di gioco è un comando registrabile, o non esiste.

---

## 1. Cosa cerca la community e cosa odia: la lista che guida il piano
Sintesi di `00`, `03` parte C e delle ricerche di oggi. Ogni riga dice **dove** la risolviamo.

### 1.1 Cosa odiano nei grandi titoli (e la nostra regola)
| Gioco | Cosa odiano | Nostra regola | Fase |
|---|---|---|---|
| **The Tower** | timer di laboratorio da saltare pagando; pay-to-win; troppe valute; connessione obbligatoria; partite di ore; menu affollati e testo piccolo | zero timer; competitivo a dotazione fissa; **2 valute**; offline completo; run 10–15 min; poche informazioni per volta, testo grande | 2, 4, 6 |
| **Bloons TD 6** | sessioni da 45+ minuti; Monkey Knowledge diventato grind di valuta | run brevi con **salvataggio a ogni negozio**; l'Archive sblocca varietà, **mai statistiche** | 2, 4 |
| **Rush Royale / Random Dice** | pay-to-win nei tornei; avversari 2× più forti; casualità ingiusta | modalità competitive con **stesso seme e stesso pool** per tutti; rilancio, anteprima ondate, annulla | 1, 4, 7 |
| **Super Auto Pets** | matchmaking per numero di vittorie → "è tutta fortuna" | i fantasmi si confrontano **sullo stesso seme**; Leghe da 30 con **stesso seme e dotazione** | 4, 7 |
| **Backpack Battles** | fantasmi di versioni vecchie; sul mobile il dito copre il testo, trascinamento impreciso, build rovinate senza annulla | replay validi **solo per la stessa versione di bilanciamento**; oggetto sopra il dito, calamita, **annulla gratuito** | 1, 4 |
| **Balatro (mobile)** | solo orizzontale; testo piccolo; cloud save inaffidabile (progressi persi, "loading cloud save" infinito); nausea da movimento; punteggio lento non saltabile | **verticale** (D2); opzione dimensione testo; **il salvataggio locale fa fede**, il cloud è sincronizzazione facoltativa con gestione dei conflitti; sfondo fermo + "riduci movimento"; conteggio saltabile con un tocco | 1, 3, 4 |
| **Vampire Survivors** | a fine run si "snowballa" e si guarda soltanto; disordine visivo senza opzioni | Grade ed Endless con crescita reale; Pulse come scelta; **intensità effetti** regolabile + attenuazione automatica | 1, 2, 3 |
| **Brotato** | schermo intasato di nemici e proiettili | tetto agli effetti simultanei; numeri solo per uccisioni e colpi grandi | 1, 3 |
| **Ball x Pit** | un secondo grind (costruzione) sopra il primo | **nessun secondo sistema di grind**: solo Archive (varietà) e Grade (sfida) | 4 |
| **Kingdom Rush 5** | paghi il gioco e poi paghi eroi e torri | modello A: sblocco **completo** una tantum; modello B: mai potenza a pagamento | 6 |
| **Infinitode 2** | grind arbitrario nell'avanzato | sblocco completo in 60–80 run, poi solo Grade e classifiche | 4 |
| **Arknights** | energia | niente energia, mai | — |
| **PvZ 3** | semplificato, formazioni imposte, profondità persa | verticale **senza** perdere profondità: negozio, anello, merge, Core type | 2 |

### 1.2 Cosa amano e non trovano (la nostra risposta)
| Richiesta ricorrente | Risposta | Fase |
|---|---|---|
| "cozy mobile ma con strategia profonda" | grafica calma + negozio con scelte vere | 1, 3 |
| sinergie ed effetti di stato | vicinato sull'anello, booster moltiplicativi, Frost, Echo | 2 |
| QoL completa: velocità, pausa, anteprima ondate, annulla | tutte dal prototipo | 1 |
| social asincrono, "batti la mia build" | replay condivisibili con un codice, fantasmi, Daily | 4, 7 |
| salvataggio cloud e multipiattaforma | UGS Cloud Save (D21), locale sempre valido | 5 |
| feedback tattile curato (Marvel Snap, Balatro) | vibrazioni a livelli, calamita, conteggio progressivo | 1 |
| suoni piacevoli nell'interazione (Mini Motorways) | note morbide su prendere/lasciare, tono che sale nel conteggio | 3 |
| un tower defense "moderno di riferimento" senza microtransazioni (Thronefall, Mindustry) | modello A come opzione principale, deciso con i dati | 6 |

---

## 2. Le fasi

### Fase 0 — Fondamenta ✅ (fatta, settembre 2026)
Simulazione deterministica (60 tick/s, interi), 7 moduli, 3 nemici + Guardian, negozio con merge e annulla, Pulse, replay `R1` con verifica, 17 test, presentazione minima nell'editor.

---

### Fase 1 — Prototipo tattile e strumenti (2–3 settimane)
**Scopo:** rispondere alla regola d'oro. Il negozio con l'anello deve essere piacevole **da toccare** già con forme semplici.

**Simulazione**
- [ ] Comando `BuySlot` (slot extra a 8 Credits dopo il primo Guardian, GDD §5) e `Ring` a 7–8 slot.
- [ ] **Bot runner** in EditMode (menu `TowerDefense/Balance/Run bots`): N semi × strategie (ingenua, "massimo DPS", "economia prima"), stampa vittorie per ondata, durata media, causa di sconfitta, Credits inutilizzati. Salva un CSV in `Temp/Balance/`. È lo strumento di tutte le fasi successive.
- [ ] **Contenuto in ScriptableObject** (`Assets/Content/`): `ModuleAsset`, `EnemyAsset`, `RunConfigAsset` in `Presentation`, con un `ContentLoader` che riempie `ContentDatabase`. `CreatePrototypeDefaults()` resta per i test. Test: gli SO e i default producono lo stesso hash sullo stesso seme.
- [ ] Tabella di **notazione compatta** dei numeri (1,2K · 3,4M · 5,6B) nella simulazione (intera, deterministica) con test.

**Presentazione (GDD §15.1, docs/03 parte C)**
- [ ] Trascinamento con **oggetto sopra il dito**, slot validi illuminati, **calamita**; rilascio non valido → ritorno morbido.
- [ ] **Anteprima in alto** durante il trascinamento, calcolata da `TryPreviewBuy/Move/Sell` (DPS prima → dopo, "Livello 2 → ×1,8", "Vendi: +2").
- [ ] **Zona Vendi** in basso, **Annulla** nella UI, tocca-carta-poi-slot come alternativa.
- [ ] Merge animato (attrazione + bagliore morbido, scala 1,15 → 1), linee delle combo che si accendono piano.
- [ ] **Riepilogo di fine ondata** con conteggio progressivo saltabile con un tocco.
- [ ] Vibrazioni a 3 livelli (Android `VibrationEffect`), disattivabili.
- [ ] Opzioni base: riduci movimento, intensità effetti, numeri dei danni, vibrazioni, velocità. Salvate in `PlayerPrefs` per ora.
- [ ] Tetto agli effetti simultanei e attenuazione automatica con molti nemici.
- [ ] Sostituire IMGUI con **UI Toolkit** (decisione D22 sotto) per carte, HUD e opzioni: serve già ora per il trascinamento e per i 48 dp.

**Gate 1 (GDD §19):** 3–5 tester, senza audio, sul telefono (build Android di sviluppo). Almeno 3 su 5 chiedono di rigiocare; capiscono il vicinato senza spiegazioni entro il secondo negozio; nessuno bloccato più di 10 s; durata reale di ondata e negozio misurata (obiettivo 25–30 s e 10–20 s). Il bot ingenuo resta sotto il 95% di vittorie nell'atto 1 (se supera, l'atto 1 è troppo facile).

---

### Fase 2 — Contenuto MVP e run completa (3–4 settimane)
**Scopo:** la run intera da 3 atti, con tutti gli elementi dell'MVP, bilanciata dal bot.

- [ ] **Moduli mancanti** (GDD §6): Arc, Lance, Mortar, Echo, Salvage, Frost, Capacitor → 14. Ognuno con un test del comportamento e uno di determinismo.
- [ ] **Nemici mancanti** (GDD §8): Dasher, Splitter, Warden; **élite** dall'atto 2; sblocchi per atto (non solo per ondata nell'atto).
- [ ] **Atti 2 e 3**: Guardian con scorta più varia; ondate a tema; anteprima con icone e quantità.
- [ ] **Core type** (Standard, Merchant, Bastion, Glass) come dati; scelta a inizio run.
- [ ] **Grade 1–3** (i primi tre modificatori) ed **Endless** dopo la vittoria.
- [ ] **Salvataggio e ripresa a ogni negozio** (il salvataggio è: seme + comandi + versione, cioè il replay stesso; ripresa = risimulazione). Test: salva → ricarica → stesso hash.
- [ ] **Replay `R2`**: + versione del gioco, modalità, Core type, Grade. `R1` resta leggibile.
- [ ] **Bilanciamento v0.3** con il bot: obiettivi *(stima)* — bot ingenuo vince l'atto 1 nell'80–90% e la run intera nel 10–20%; bot "massimo DPS" vince la run nel 50–70% a Grade 0 e sotto il 30% a Grade 3; nessuna strategia sopra l'85%; durata simulata della run 8–12 minuti di combattimento (+ negozi).
- [ ] Aggiornare `05-gdd.md` con i numeri v0.3 e `04` con le decisioni prese.
- [ ] **Avviare ora** la pratica per l'**account Google Play** (D13): i tempi sono lunghi e serve per la fase 5.

**Gate 2:** run intera giocabile sul telefono; obiettivi del bot raggiunti; durata reale con 3–5 tester tra 10 e 15 minuti; almeno **3 build diverse** vincenti trovate dai tester o dal bot (altrimenti l'anello da 6 non basta: GDD §18).

---

### Fase 3 — Mood shot e vertical slice (4–6 settimane)
**Scopo:** l'identità "Dusk Garden" (docs/03) su una run vera, con audio, a 30 fps su un telefono da 4 GB.

- [ ] **Mood shot** (03 §A8): una sola schermata curata, mostrata a qualcuno **prima** di produrre il resto. Include il confronto 3D ortografico inclinato vs vista dall'alto (D5).
- [ ] Modelli in **Blender via MCP**: Core con petali, 14 silhouette di moduli, 6 schegge di nemici + Guardian, tutto low-poly, export in `Assets/Models/`.
- [ ] URP: luce calda + ambiente freddo, ombre morbide a qualità media/alta, bloom leggero solo su colpi e Pulse; **livelli di qualità** con ombre e bloom disattivabili; test su un telefono di fascia bassa (D4).
- [ ] Effetti definitivi (03 §A5): linee tenui, onda del Pulse, morte "a scaglie", numeri morbidi.
- [ ] UI definitiva (tipografia arrotondata, carte in vetro morbido, icone). Opzioni complete (03 §C7): dimensione testo 100–200%, daltonismo (3 preset + forme), alto contrasto.
- [ ] **FTUE** (GDD §14): atto guidato, una riga per suggerimento, i due "aha" entro 90 s. Misurato con un tester che non ha mai visto il gioco.
- [ ] **Audio** (03 parte B): ambient a strati (negozio / ondata / Guardian), suoni dell'interazione, tetto ai suoni simultanei. Per ora loop e SFX con licenza chiara per il prototipo; scelta del compositore rimandata al soft launch.
- [ ] Localizzazione: sistema con chiavi, **EN + IT** subito; testo ridotto al minimo (D7).
- [ ] Profiling su dispositivo: budget 30 fps con 60 nemici e tutti gli effetti.

**Gate 3:** mood shot approvata; 5 tester: la grafica è "calma ma attraente" e non "da prototipo" (domanda diretta); 30 fps stabili sul telefono di riferimento; FTUE completato dal 100% dei tester senza aiuto.

---

### Fase 4 — Meta, Daily e replay condivisi (3–4 settimane)
**Scopo:** il motivo per tornare domani, tutto ancora **offline e locale**.

- [ ] **Blueprints e Archive** (GDD §11): sblocco di moduli e Core type; curva per 60–80 run; **nessuna statistica**.
- [ ] **Daily e Weekly Run** con seme derivato dalla data UTC (GDD §12), pool e Core fissi; classifica **locale** e storico personale.
- [ ] **Codice di replay** condivisibile (testo corto o link): importa → guarda il **fantasma** (riproduzione del replay) → sfida sullo stesso seme.
- [ ] Modalità infinita con classifica locale.
- [ ] Interfacce `Services` (GDD §16) con implementazioni finte: analytics, classifiche, cloud save, consenso, pubblicità, acquisti. Log in console, nessun SDK ancora.
- [ ] Eventi analytics definiti (D13) e chiamati dalle finte.
- [ ] Missioni giornaliere semplici (02 §4) e serie senza punizione.

**Gate 4:** un tester nuovo arriva alla terza run con almeno uno sblocco e capisce da solo cos'è la Daily; un replay esportato da un telefono si verifica su un altro (stesso hash).

---

### Fase 5 — Servizi e test chiuso (4–6 settimane, in parte in parallelo con la 4)
**Scopo:** il gioco esce dalle mani dello sviluppatore (D13).

- [ ] **Unity Gaming Services** (D21): Authentication anonima, Leaderboards per Daily/Weekly/Endless divise per versione di bilanciamento, Cloud Save (locale fa fede; conflitto → si tiene il progresso maggiore e si avvisa), **Cloud Code C#** che risimula il replay e accetta il punteggio solo se l'hash coincide. Stima dei costi scritta in `04`.
- [ ] Decisione **D23**: verifica sul server già al lancio (consigliato, il motore è pronto) oppure a campione. Se a campione, i replay si conservano comunque per verifiche retroattive.
- [ ] Analytics + crash report (decisione **D24**: Firebase vs GameAnalytics) e **consenso UMP** prima del tutorial solo in UE/UK (D16).
- [ ] Privacy policy minima; Discord dei tester; build su **test chiuso** Google Play, 12+ tester × 14 giorni.
- [ ] Ciclo settimanale: dati → bot → bilanciamento → nuova build. I replay dei tester alimentano il bot (le loro strategie diventano bot).

**Gate 5:** 14 giorni di test chiuso completati; crash-free ≥ 99%; classifica Daily funzionante con verifica; D1 dei tester ≥ 35% (GDD §17, con le dovute cautele sui numeri piccoli).

---

### Fase 6 — Soft launch (6–8 settimane)
**Scopo:** decidere il modello di monetizzazione **con i dati** (02 §2) e rifinire.

- [ ] Implementare **entrambi** i modelli dietro un flag: A (primo atto + Daily gratis, sblocco completo una tantum) e B (pubblicità solo facoltative, pass senza rinnovo, VIP). Nessuno dei due tocca il competitivo.
- [ ] Scheda dello store: nome definitivo (**D25**), icona, screenshot, video di 15 s "da 10 danni a 10 milioni"; **un'ora con un avvocato IP** (D20); IARC; dichiarazione contenuti AI.
- [ ] Localizzazione: ES, PT-BR, DE, FR (D7), revisione madrelingua per lo store.
- [ ] Due Paesi simili, uno per modello (o A/B), 300–1.000 € di Google Ads sui video organici migliori.
- [ ] Misurare: D1/D7/D30, durata run, run al giorno, ricavo per installazione, voto.

**Gate 6:** KPI del GDD §17 raggiunti o piano correttivo scritto; modello scelto e registrato in `04`.

---

### Fase 7 — Lancio e aggiornamenti
- **Lancio** (Android): Daily, Weekly, Endless, classifiche verificate, fantasmi dei migliori.
- **Aggiornamento 1** (+2–3 mesi): **Leghe** settimanali da 30 (stesso seme e dotazione) e **Duel** con rating; Grade 4–10; nuovi moduli (Prism, Singularity, Harvester…) verso i ~40.
- **Aggiornamento 2**: **Siege** e **Community Guardian**; temi estetici stagionali (mai la tinta dei nemici su Core e moduli).
- **iOS**: dopo il lancio Android, con Mac o Unity Build Automation.

---

## 3. Calendario
Il calendario operativo è in **`10-liveops-e-roadmap.md` §4** (roadmap a 6 mesi, ottobre 2026 → marzo 2027, un mese per fase con gate e piano "se fallisce"). Le fasi di questo file restano; le date di `10` valgono su queste. La compressione a 6 mesi si regge su tre scelte esplicite (`10` §4.0): bilanciamento continuo con il bot, grafica in parallelo al contenuto dal mese 2, e Pass/Leghe/Duel/Siege/iOS fuori dal rilascio. Le durate sono per una persona con l'AI; **il gate conta più della data**.

---

## 4. Da fare subito (prossime sessioni, in ordine)
1. Bot runner in EditMode + CSV (sblocca tutto il bilanciamento).
2. `ContentLoader` da ScriptableObject + test di equivalenza con i default.
3. Comando `BuySlot` + test.
4. UI Toolkit al posto di IMGUI: HUD, carte, pannello modulo.
5. Trascinamento con calamita e anteprima.
6. Annulla e Vendi nella UI; merge animato; riepilogo di fine ondata.
7. Opzioni base + vibrazioni.
8. Build Android di sviluppo sul telefono → Gate 1 con 3–5 tester.

## 5. Decisioni da prendere (da registrare in `04`)
| # | Domanda | Proposta |
|---|---|---|
| **D22** | UI: UI Toolkit o uGUI? | **UI Toolkit** (runtime maturo in Unity 6, stili in USS, buon supporto al trascinamento e al ridimensionamento del testo). uGUI solo se il trascinamento sopra la scena 3D dà problemi |
| **D23** | Verifica dei replay sul server già al lancio? | Sì, con Cloud Code C#, se la stima dei costi UGS è accettabile; altrimenti a campione con replay conservati |
| **D24** | Analytics: Firebase o GameAnalytics? | Da valutare in fase 5: GameAnalytics è gratuito e semplice; Firebase porta anche Crashlytics e Remote Config |
| **D25** | Nome del gioco | Da scegliere in fase 6, dopo il controllo di marchi e store (mai richiami ad altri giochi, D20) |

## 6. Rischi principali del piano
| Rischio | Segnale | Risposta |
|---|---|---|
| Il negozio non diverte con forme semplici | Gate 1 fallito | Rivedere le regole del vicinato o il numero di slot **prima** della grafica |
| Bilanciamento "rotto" da una combo | il bot supera l'85% con una strategia | Nerf mirato, poi Grade; la simulazione rende il ciclo breve |
| Prestazioni sul telefono da 4 GB | < 30 fps con 60 nemici | Livello di qualità basso: niente ombre, niente bloom, meno effetti |
| Tempi dell'account Google Play | pratica non chiusa entro la fase 4 | Avviarla in fase 2 |
| Costi UGS oltre il previsto | stima in fase 5 | Verifica a campione; Nakama come piano B (D21) |
| Sembrare una copia (D20) | feedback dei tester | Un'ora con l'avvocato IP prima dello store; identità visiva diversa sin dalla mood shot |

## Fonti (oltre a quelle in `00`, `03`, `04`)
- Tower defense 2026, cosa piace e cosa no (Bloons, Rush Royale, Mindustry, Rogue Tower): https://www.switchbladegaming.com/strategy-games/best-tower-defense-2026/
- Offline e senza microtransazioni come richiesta: https://choostgames.com/blog/best-offline-mobile-games-no-microtransactions/
- Balatro mobile, critiche (orientamento, testo, cloud save): https://www.engadget.com/gaming/balatro-is-an-almost-perfect-mobile-port-163050971.html · https://steamcommunity.com/app/2379780/discussions/0/604157441922347014/ · https://roguewiki.com/balatro/guides/balatro-mobile-vs-pc-guide
- Super Auto Pets, matchmaking: https://steamcommunity.com/app/1714040/discussions/0/3086646248537424616/
- Backpack Battles, PvP asincrono e versioni: https://steamcommunity.com/app/2427700/discussions/0/6770657417430301203
- Ball x Pit mobile (prova + sblocco, grind della costruzione): https://www.engadget.com/gaming/ball-x-pit-will-land-on-ios-and-android-on-march-12-175159137.html · https://dreager1.com/2026/05/30/ball-x-pit-review/
- Vampire Survivors e Brotato, snowball e disordine: https://choostgames.com/blog/vampire-survivors-vs-brotato/ · https://videochums.com/article/brotato-vs-vampire-survivors
