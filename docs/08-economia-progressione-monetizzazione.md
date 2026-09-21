# 08 — Economia, progressione e monetizzazione non pay-to-win (brief di Fase 2, v1 · 21/09/2026)

> **Ruolo del documento:** la specifica economica che la Fase 2 (`06`) implementa e che la Fase 4 (meta) e la Fase 6 (soft launch) completano.
> Principio fermo (`02`): *"paghi per arrivare prima o per comodità, mai per vincere"*. Le voci marcate **PROPOSTA** vanno confermate e registrate in `04`.
> Tutti i numeri sono **v0, da validare con il bot** (`06` Fase 2) e con il simulatore della meta (§1.4).

---

## 0. Le due economie (e il muro tra loro)
| | **Dentro la run** | **Fuori dalla run (meta)** |
|---|---|---|
| Valuta | **Credits** | **Blueprints** |
| Si compra | moduli, merge, rilanci, slot extra | **varietà**: moduli nel pool, Core type, temi |
| Cresce | esponenzialmente, per 10–15 minuti | linearmente, per 60–80 run, poi **finisce** |
| Si azzera | a ogni run | mai |
| Tocca la potenza | sì, è il gioco | **no, mai** |
| Acquistabile con denaro | **mai** | solo più veloce (modello B) |

**Il muro:** nessun oggetto della meta modifica un numero della simulazione. Questo è ciò che permette (a) classifiche sullo stesso seme, (b) replay verificabili, (c) un giocatore free che compete al massimo livello dal giorno 1 nelle modalità competitive (pool fisso).

---

## 1. "Albero dei potenziamenti": come è fatto da noi

### 1.1 Potenziamento dentro la run: i livelli dei moduli
Un modulo si potenzia **solo** comprandone un doppione (merge), fino a L3. Non esiste un pulsante "potenzia".

**Rendimento per Credit (Emitter, costo 3, danno 8):**
| Livello | Credits investiti | Danno per colpo | Danno per Credit | Slot occupati |
|---|---|---|---|---|
| L1 | 3 | 8,0 | 2,67 | 1 |
| L2 (×1,8) | 6 | 14,4 | 2,40 | 1 |
| L3 (×3,0) | 9 | 24,0 | 2,67 | 1 |
| *tre Emitter L1 separati* | 9 | 24,0 (in 3 slot) | 2,67 | **3** |

La logica: il merge **non è più efficiente per Credit** di tre moduli separati — è più efficiente **per slot**. Il valore del merge è lo spazio sull'anello (che è la risorsa scarsa), e il fatto che un solo modulo L3 riceve i booster dei suoi due vicini: 24 × booster, invece di 8 × booster tre volte con solo 6 slot. La crescita esponenziale nasce **dalla moltiplicazione tra vicini**, non dai livelli. Questo evita l'inflazione: i livelli sono lineari-ish, i moltiplicatori si accumulano solo se il giocatore costruisce bene.

**Booster (Amplifier ×1,5):** L2 → ×1,8 · L3 → ×2,1. Due Amplifier L3 attorno a un'arma: ×4,41. È il tetto di ciò che 6 slot permettono con una sola arma (arma + 2 booster + 3 slot per economia/altre armi).

### 1.2 Flusso dei Credits in una run (stima, Core Standard)
| Fonte | Per ondata | Su 18 ondate |
|---|---|---|
| Base | 4 | 72 |
| Interesse (1 ogni 5, max 5) | 0–5, in media 2–3 | ~45 |
| Guardian | +3 × 3 | 9 |
| Bank (se posseduta, L1) | +1 | ~10 |
| Iniziali | — | 6 |
| **Totale** | | **~130–145** |

| Spesa tipica | Credits |
|---|---|
| 6 moduli a L1 (costo medio 3,7) | ~22 |
| Portarne 4 a L3 (8 merge) | ~30 |
| Slot extra ×2 | 16 |
| Rilanci (media 1,5 a negozio × 17 negozi, costo crescente) | ~40 |
| Vendite/pivot | −10 (rientro) |
| **Totale** | **~100–110** |

Il margine (~30 Credits) è lo spazio per **scegliere**: risparmiare per l'interesse, rilanciare di più, o un pivot a metà run. Se il bot mostra un margine > 50 il gioco è troppo generoso (le scelte non pesano); se < 10 è avaro (frustrazione). **Leve:** Credits base per ondata, tetto dell'interesse, costo del rilancio.

### 1.3 Potenziamento fuori dalla run: l'Archive (varietà, non potenza)
L'Archive è un albero a **quattro rami**. Ogni nodo aggiunge qualcosa al gioco; **nessun nodo modifica un numero**.

| Ramo | Nodi | Cosa aggiunge | Esempio |
|---|---|---|---|
| **Armi** | 10 | un'arma nel pool del negozio | Arc, Lance, Mortar, poi Prism, Singularity… |
| **Booster** | 8 | un booster nel pool | Echo, poi Resonator, Mirror… |
| **Economia e utilità** | 8 | un modulo economico o difensivo | Salvage, Frost, Capacitor, Harvester… |
| **Core** | 3 (+ temi) | un Core type (regole diverse) | Merchant, Bastion, Glass |
| *(gratis, senza Blueprints)* | Grade 1–10 | difficoltà | si sbloccano vincendo |

**Struttura ad albero, non a lista:** ogni ramo ha 2–3 nodi disponibili alla volta; sbloccarne uno ne apre altri. Il giocatore **sceglie** cosa sbloccare (niente casualità sugli sblocchi: la casualità sta nel negozio, non nella meta). I nodi "sidegrade" (Core type) sono la nostra versione delle "abilità passive": cambiano le regole, non le statistiche.

### 1.4 Matematica dei Blueprints (anti-inflazione per costruzione)
**Entrate:**
| Evento | Blueprints |
|---|---|
| Ondata superata | 1 |
| Guardian sconfitto | 3 |
| Prima vittoria della giornata (qualsiasi modalità) | +5 (**PROPOSTA**) |
| Daily Run completata (anche persa) | +3 (**PROPOSTA**) |
| Grade (per livello, solo su vittoria) | +1 × Grade |

Run media di un giocatore nuovo (perde all'ondata 10): ~11. Run vinta: 27. **Media a regime: ~15 per run, ~30 al giorno** con 2 run + Daily.

**Costi (lineari per ramo, mai esponenziali):**
| Nodo del ramo | 1° | 2° | 3° | 4° | 5°… | ultimo |
|---|---|---|---|---|---|---|
| Armi / Booster / Economia | 10 | 15 | 20 | 25 | +5 | 55 |
| Core type | 40 | 60 | 80 | — | — | — |
| Temi estetici (facoltativi) | 20 | 20 | 20 | … | | |

**Totale Archive di gioco** (26 moduli + 3 Core): ~ 26 × 30 (media) + 180 ≈ **960 Blueprints** → **~65 run** a 15 di media, **~32 giorni** a 30 al giorno. Coerente con l'obiettivo 60–80 run (`05` §11). I temi sono fuori dal conto: sono il "pozzo" per chi ha finito.

**Perché lineare:** con costi esponenziali (tipico del genere) l'ultimo nodo vale quanto tutti gli altri insieme → grind wall. Con costi lineari la **cadenza** degli sblocchi è prevedibile: uno ogni 1–2 run all'inizio, uno ogni 3–4 verso la fine. Il ritmo si rallenta, non si ferma.

**Simulatore della meta** (da fare in Fase 2 con il bot): N giocatori sintetici × sessioni al giorno × probabilità di vittoria per Grade → curva "giorni per sbloccare tutto". Obiettivo: mediana 30–45 giorni per chi gioca ogni giorno, 60–80 run in totale.

### 1.5 Cosa NON facciamo (e cosa costerebbe farlo)
| Richiesta tipica del genere | Perché no | Cosa succederebbe |
|---|---|---|
| Statistiche base permanenti (+danno, +HP) | rompono il competitivo a dotazione fissa e trasformano ogni run in "sono abbastanza potenziato?" | classifiche dominate dal tempo giocato, non dall'abilità; è la critica principale a The Tower e Infinitode |
| Abilità passive permanenti | idem, mascherate | i nuovi giocatori perdono contro la meta, non contro il gioco |
| Costi esponenziali degli sblocchi | grind wall garantito | il D30 cala quando il giocatore "vede" il muro |

---

## 2. "Pay to progress": cosa ha senso monetizzare

### 2.1 Modello A — prova + sblocco (scelta principale da testare)
- **Gratis:** atto 1 completo, Daily Run, classifica, fantasmi. Nessuna pubblicità.
- **Sblocco unico (4,99–7,99 €):** atti 2–3, Archive completo, Weekly, Endless, Core type, Grade.
- **Facoltativo:** pacchetti di temi estetici (1,99–2,99 €) e un "Supporter pack" (4,99 €: tema esclusivo + titolo nella classifica). Niente altro.
- Chi prova gratis compete nella Daily **alla pari** (pool fisso). È il modello Ball x Pit / Balatro mobile: posizionamento "niente trucchi".

### 2.2 Modello B — free-to-play onesto
Tutto il gioco gratis. Si monetizza **solo** la velocità della meta e l'estetica:

| Cosa | Prezzo | Effetto | Tocca il competitivo? |
|---|---|---|---|
| **Pubblicità ricompensate** (facoltative, ~8/giorno) | gratis | Blueprints ×2 a fine run · Second Chance (1/run, **non nelle modalità competitive**) · Free Reroll (1/negozio, **non nelle competitive**) · Daily Crate | no |
| **Season Pass** (§3) | 4,99 € / 30 giorni, senza rinnovo | Blueprints ×2 + estetica | no |
| **VIP per sempre** | 7,99 € | niente pubblicità, i premi delle ricompensate sono automatici (stessi limiti) | no |
| **Pacchetti di Blueprints** | 1,99 € = 60 · 4,99 € = 180 · 9,99 € = 400 | accelerano l'Archive | no (l'Archive non è potenza) |
| **Temi estetici** | 1,99–2,99 € | Core, anello, onda del Pulse, stile dei numeri, animazione di vittoria | no |

**Tetto ai pacchetti (PROPOSTA):** l'Archive è finito (~960 BP): chi paga tutto lo completa in ~5 run invece di ~65. Il "vantaggio" è **varietà prima**, e svanisce quando il giocatore free arriva in fondo. Per evitare l'effetto "ho comprato il gioco e ho finito", i pacchetti sono limitati a **400 BP ogni 7 giorni** per account: pagare accorcia, non salta.

### 2.3 Cosa **non** si vende in nessun modello (regole fisse, 02 §1)
| Richiesta del template | Perché no | Alternativa onesta |
|---|---|---|
| **Time-skip** | non esistono timer | — |
| **Slot extra a pagamento** | è potenza dentro la run (8° slot = +1 booster = ×1,5) → pay-to-win nella Daily | lo slot extra si compra **con i Credits**, dentro la run, 8 Credits |
| **Moltiplicatori temporanei di danno o Credits** | idem; e The Tower è odiato per questo | moltiplicatore **di Blueprints** (meta) via pass o pubblicità |
| Energia, casse casuali, rilanci a pagamento | rompono rispettivamente rispetto del tempo, onestà, decisione | — |

### 2.4 La garanzia per il giocatore free ad alta abilità
1. **Daily, Weekly, Leghe, Duel usano un pool e un Core fissi**: gli sblocchi non contano, l'abilità sì. Un giocatore al giorno 1 può vincere la Daily del giorno 1.
2. **Endless e le classifiche per Grade** sono divise per **versione di bilanciamento**, non per "livello dell'account".
3. **Second Chance e Free Reroll non esistono nelle modalità competitive**: lì nessuna pubblicità cambia una run.
4. L'Archive **finisce**: dopo ~65 run tutti hanno tutto. Non esiste un "endgame a pagamento".
5. **Verifica dei replay** sul server: chi bara non compare in classifica (D19, D23).

Il gap massimo tra free e pagante è quindi: *"ho meno varietà nelle run libere per un mese"*. Nessun gap nel competitivo, mai.

---

## 3. Season Pass a due binari

**Condizioni per lanciarlo** (`02` §3): modello B scelto, D30 ≥ 7%, test positivo. Non è nel gioco al lancio.

### 3.1 Struttura
| Parametro | Valore | Motivo |
|---|---|---|
| Durata | **30 giorni**, senza rinnovo automatico | una stagione = un ciclo di bilanciamento e un tema |
| Prezzo premium | 4,99 € | sotto la soglia "ci penso" |
| Livelli | **30** | uno al giorno per chi gioca ogni giorno |
| Punti per livello | 100 | numeri leggibili |
| Punti da | ondata superata **5** · Guardian **15** · Daily completata **60** · missione giornaliera **30** (×3) · missione settimanale **150** | una run vinta ≈ 135; una giornata media (2 run + Daily + missioni) ≈ **330 = 3,3 livelli** |
| Tempo per completarlo | ~10 giorni giocando ogni giorno; ~20 giocando a giorni alterni | mai un obbligo quotidiano; la **retention viene dalla Daily**, non dall'ansia del pass |
| Acquisto retroattivo | sì, in qualsiasi momento della stagione, con i premi già maturati | zero "ho perso i premi" |
| Scadenza | i punti non scadono; il pass sì; nessun livello si compra con denaro | niente "paga per finire il pass" |

### 3.2 Premi (bozza)
| Livello | Binario **Free** | Binario **Premium** (in più) |
|---|---|---|
| 1 | 10 Blueprints | tema stagionale del Core (subito: il "wow" all'acquisto) |
| 5 | 15 BP | Blueprints ×2 per tutta la stagione (dall'acquisto) |
| 10 | Stile dei numeri "stagione" | 40 BP |
| 15 | 20 BP | Onda del Pulse stagionale |
| 20 | Titolo in classifica | 60 BP |
| 25 | 25 BP | Tema dell'anello |
| 30 | **Cornice "stagione completata"** | Animazione di vittoria stagionale + 100 BP |
| ogni livello intermedio | 5 BP | 10 BP |

Totale Blueprints: Free ~200 (≈ 13 run), Premium ~600 in più. **Nessun modulo esclusivo** in nessun binario: la varietà si sblocca nell'Archive, uguale per tutti; il pass la accelera e veste il gioco.
**Regola dei temi:** mai la tinta dei nemici su Core e moduli (`03`); ogni tema passa il test di leggibilità.

### 3.3 Ritorno quotidiano (senza punizioni)
- **Daily Run**: il vero motivo (stesso seme per tutti, classifica, fantasmi). +3 BP anche se persa.
- **3 missioni giornaliere** semplici e sempre completabili in 2 run ("vinci un atto con 2 Amplifier", "usa il Pulse 5 volte", "vendi un modulo"). Si rinnovano a mezzanotte UTC; **non si accumulano né si perdono**.
- **1 missione settimanale** ("completa 3 Daily", "vinci a Grade 2").
- **Serie di giorni:** conta solo in salita (badge a 7/30/100 giorni); saltare un giorno non azzera niente di tangibile (**PROPOSTA:** la serie mostra "giorni giocati questo mese", non una catena che si spezza).

---

## 4. Sistemi anti-frustrazione

### 4.1 Contro il grind wall (meta)
| Meccanica | Come | Effetto |
|---|---|---|
| **Costi lineari** (§1.4) | +5 per nodo | cadenza prevedibile, mai un muro |
| **Blueprints anche perdendo** | 1 per ondata superata | ogni run fa progressi, anche la peggiore |
| **Sblocco a scelta** | l'Archive è un albero con 2–3 opzioni aperte | il giocatore sblocca ciò che vuole provare |
| **"Prossimo sblocco tra N"** a fine run | sempre visibile | il progresso si vede, non si intuisce |
| **Fine dell'Archive** | ~65 run, poi solo Grade, Daily e temi | niente grind infinito: il gioco lo dice chiaramente ("Archive completo") |
| **Nessun secondo sistema di grind** (lezione Ball x Pit) | un solo albero | — |

### 4.2 Contro la sfortuna (dentro la run)
| Meccanica | Come | Nota |
|---|---|---|
| **Protezione dalla sfortuna nel negozio** (**PROPOSTA**) | se per 2 negozi consecutivi non compare nessun booster, il terzo ne garantisce uno; idem per le armi | regola **deterministica** nel `GenerateOffers` (dal seme, replay-safe): non cambia la Daily tra giocatori |
| **Prima offerta dell'atto 2 e 3** (**PROPOSTA**) | contiene sempre almeno un modulo raro | i rari esistono davvero, non solo sulla carta |
| Rilancio, anteprima dell'ondata, annulla | già nel GDD | la casualità si gestisce, non si subisce |
| **Rigioca questo seme** dalla sconfitta | gratis, non entra in classifica | trasforma "sfortuna" in "riprovo meglio" |

### 4.3 Contro il "troppo difficile" / "troppo facile"
- **Grade 1–10** per chi trova il gioco facile (gratis, `07` §2.2).
- **Modalità Calma** (**PROPOSTA**): opzione di accessibilità (nemici −20% HP, Pulse −25% ricarica), disponibile dall'inizio, **fuori classifica** e con un'icona sul riepilogo. È un'opzione, non un acquisto: l'accessibilità non si vende.
- **Consiglio contestuale dopo 3 sconfitte nello stesso atto** (`07` §2.4): uno, mirato, mai un nerf nascosto.

### 4.4 Contro la frustrazione da monetizzazione
- Nessuna pubblicità non richiesta; il pulsante della ricompensata dice **prima** cosa dà.
- Il negozio degli acquisti **non compare** nei primi 3 giorni e mai durante una run.
- Nessun "quasi" (barre al 95% che chiedono denaro), nessun contatore di sconto, nessuna offerta a tempo (**regola fissa**).
- Tutto ciò che si compra si può vedere **in anteprima nel gioco** (tema applicato al Core per 10 secondi).

### 4.5 Metriche da guardare (Fase 5–6)
| Metrica | Segnale di problema |
|---|---|
| Run tra uno sblocco e il successivo | > 5 nella prima settimana |
| Blueprints per ora di gioco | in calo dopo la prima settimana (curva troppo ripida) |
| % di run terminate per abbandono nel negozio | > 10% (il negozio annoia o blocca) |
| Uso di Second Chance / Free Reroll | > 60% delle run (il gioco è troppo punitivo o i premi troppo generosi) |
| Rapporto vittorie Grade 0 dopo 20 run | < 40% (troppo difficile) o > 90% (troppo facile) |
| Ricavo per installazione tra A e B | decide il modello (`02` §2) |

---

## 5. Cosa entra nella Fase 2 e cosa dopo
- **Fase 2:** flusso dei Credits (§1.2) e leve di taratura con il bot; simulatore della meta (§1.4); protezione dalla sfortuna e prima offerta dell'atto (§4.2) se confermate; slot extra con i Credits.
- **Fase 4:** Archive ad albero (§1.3), Blueprints (§1.4), missioni, "prossimo sblocco tra N", Modalità Calma.
- **Fase 6:** modelli A e B dietro un flag, pacchetti con tetto, temi; Season Pass **solo** dopo il soft launch e con D30 ≥ 7%.

**Decisioni da confermare:** +5 BP prima vittoria del giorno e +3 Daily · tetto ai pacchetti (400 BP / 7 giorni) · protezione dalla sfortuna nel negozio · rari garantiti a inizio atto · Modalità Calma · serie "solo in salita".
