# 01 — Concept del gioco

> **In una frase:** un tower defense roguelite minimal in cui **la tua difesa compone una traccia tech-house**.
> Ogni torre è uno strumento, ogni ondata è un build-up, e il **drop** lo scateni tu.
> Mappe sempre nuove, difficoltà che cresce con te. **Paghi per arrivare prima, mai per vincere.**

---

> **Principio fondamentale (D1 in `04`):** *prima funziona muto, la musica amplifica.*
> Il gioco deve essere un ottimo tower defense anche senza audio; nessuna decisione di gioco dipende dal suono.

## 1. Il gancio originale: "Defense = Music"

Nessun tower defense mobile affermato lo fa (vedi `00-ricerca-mercato.md`: Rhythm Towers è PC/console, gli altri sono prototipi su itch.io).

### Torri = strumenti
Ogni torre spara **a tempo** sull'orologio globale (BPM) e aggiunge **il suo strato** alla musica.

| Torre | Ritmo | Ruolo | Effetto |
|---|---|---|---|
| **Kick** | ogni battito (1/4) | danno ad area attorno a sé | onda d'urto |
| **Hi-Hat** | sedicesimi (1/16) | tanti colpi piccoli | **riduce la corazza** |
| **Bass** | in levare, sui battiti 2 e 4 | colpo pesante su un bersaglio | danno alto |
| **Clap** | battiti 2 e 4 | controllo | **stordisce** |
| **Lead** (synth) | ogni battito | catena che rimbalza tra i nemici | colpisce più bersagli |
| **Pad** | continuo | aura | **rallenta** |
| *(sbloccabili)* FX Sweep, Vocal Chop, Sub… | vari | nicchie tattiche | vari |

- **Niente riflessi obbligatori**: il ritmo è automatico e prevedibile, quindi la strategia resta al centro. Il tempo rende il danno **leggibile**: vedi e senti quanto spara ogni torre.
- **Suona sempre bene**: tonalità e scala fisse per ogni settore, colpi quantizzati, tetto ai suoni simultanei. È la tecnica usata da Disasterpeace per Mini Metro.
- **I potenziamenti cambiano il pattern**: un Kick di livello 3 suona un pattern più ricco, non solo "più danno".

### Ondate = struttura di una traccia
**Intro** (preparazione) → **Build-up** (le ondate crescono, il filtro si apre) → **Peak** (il boss del settore) → **Breakdown** (pausa e scelta del potenziamento).

### Il "Drop": la parte attiva (dettagli in GDD §11)
- Ogni uccisione carica la **barra del Drop**. Quando è piena tocchi 💥: parte un build-up di una battuta (con il conto alla rovescia visivo) e il Drop colpisce **sul battere successivo**, con un'onda devastante.
- **Perfect (facoltativo):** se durante il build-up tocchi di nuovo lo schermo sul battito, il colpo è più forte. Premia l'abilità ma non punisce chi non ha senso del ritmo; c'è l'opzione "Assistito".
- Risolve il difetto storico del genere: la **passività**.

### Sinergie = armonia
Torri vicine o combinate si potenziano a vicenda. È la richiesta della community di "sinergie ed effetti di stato".
- **Groove** (Kick + Bass vicini): il Bass colpisce più forte, perché "pompa" in levare dopo il Kick, come il sidechain nella musica.
- **Harmony** (Lead + Pad): la catena del Lead si allunga sui nemici rallentati.
- **Exposed** (Hi-Hat + qualsiasi torre): i nemici a cui l'Hi-Hat ha ridotto la corazza subiscono più danno da tutte le torri.
- *(elenco completo nel GDD §7)*

### Nemici = "rumore"
Segnali corrotti con forme geometriche spigolose, in contrasto con le forme morbide del giocatore.
| Nemico | Comportamento | Segnale audio/visivo |
|---|---|---|
| **Static** | base | fruscio |
| **Glitch** | salta una casella del percorso | scatto visivo |
| **Distortion** | corazzato | suono saturato |
| **Feedback** | alla morte si divide | fischio |
| **Silence** | **zittisce le torri vicine**: quello strato musicale sparisce | la musica si svuota, quindi **lo senti arrivare** |
| **Boss "The Loop"** | ripete l'ultima ondata dentro di sé | loop che si ripete |

---

## 2. Struttura di una partita (run)

- **Una run è un "DJ set" di 3 settori**, circa 17 minuti in totale, con salvataggio automatico a fine ondata e ripresa in qualunque momento (D3).
- **La difficoltà è letteralmente il tempo**:
  | Settore | Stile | BPM |
  |---|---|---|
  | 1 | Deep House | 120 |
  | 2 | Tech House | 124 |
  | 3 | Techno | 128 |
- Ogni settore ha **6 ondate** più il boss (**Peak**), circa 5–6 minuti. Tra un'ondata e l'altra: **scegli 1 potenziamento su 3** (1 rilancio gratuito per run).
- Tra un settore e l'altro: scegli un **"Vinile"**, cioè un modificatore che dura per tutta la run (le "reliquie" dei roguelite).
- **Mappe generate** su griglia verticale (circa 9×14), con ingresso, uscita e ostacoli. **Costruisci il labirinto** con le torri e con gli **Acoustic Panel** (muri economici, trasformabili in torri): bloccano il percorso, e il pathfinding garantisce che un percorso esista sempre. È la meccanica amata di Gnomes e Infinitode, **su celle singole e non a blocchi Tetris**, per non sovrapporci a Emberward.
- Il generatore ha delle regole che evitano mappe ingiuste: lunghezza minima del percorso, spazio per costruire, strettoie bilanciate. Ogni mappa ha un **seme** condivisibile.

### Progressione della difficoltà
1. **Dentro la run**: il budget delle ondate cresce, nuovi tipi di nemico per settore, BPM che sale.
2. **Tra le run: i "Livelli di Pressione"**, da 0 a 20, come l'Ascension di Slay the Spire. Vinci al livello N e sblocchi N+1, che aggiunge un modificatore (nemici +10% velocità, meno risorse, boss con scudo…; elenco nel GDD §9). La sfida è dichiarata: **niente difficoltà che si adatta di nascosto**.

### Progressione permanente (meta)
- **Valuta**: *Royalties* (nome provvisorio), guadagnate a ogni run, anche perdendo. Da non confondere con i *Vinili*, che sono i modificatori della run.
- **Studio** (il laboratorio): albero di potenziamenti **con un tetto finale** che si completa. Niente grind infinito, al contrario di Infinitode 2.
- **Sblocchi di varietà**: nuovi strumenti, nuovi Vinili nel pool, nuove modalità. Danno più opzioni, non solo numeri.
- **Maestria degli strumenti**: più usi una torre, più varianti sblocchi (per esempio Kick "808", Kick "Rumble").

### Modalità
| Modalità | Quando | Note |
|---|---|---|
| **Run** (roguelite) | lancio | il cuore del gioco |
| **Daily Mix** | lancio | **stesso seme e stessa dotazione per tutti**, classifica mondiale. Si vince con l'abilità |
| **Weekly Set** | lancio o poco dopo | set più lungo, classifica settimanale |
| **After Hours** (infinita) | aggiornamento | per chi vuole spingersi al limite |
| **Jam Session** (sandbox musicale) | aggiornamento | costruisci difese per creare musica. Rilassante e virale |
| **Co-op** | futuro, se i numeri lo giustificano | vedi §5 |

---

## 3. Cosa prendiamo dai migliori (senza copiare)

| Da | Cosa piace | La nostra versione |
|---|---|---|
| **Bloons TD 6** | Scelte profonde nei potenziamenti, aggiornamenti costanti | Potenziamenti che cambiano **pattern ed effetto** + stagioni regolari |
| **Kingdom Rush** | Controllo attivo (eroi) | Il **Drop** come azione attiva a tempo |
| **Arknights** | Conta l'abilità e non i soldi; roguelike senza energia | Nessuna energia; le modalità competitive hanno la stessa dotazione per tutti |
| **Thronefall** | Minimal, ciclo stretto, scope piccolo | Run brevi, ciclo build-up/drop, scope contenuto |
| **Gnomes / Infinitode 2** | Costruire labirinti | Labirinto a celle su mappe generate |
| **Slay the Spire / Hades** | Scelte a ogni tappa, Ascension | 1 su 3 a ogni ondata, Vinili, Livelli di Pressione |
| **Mini Metro** | Musica generata dal gioco, eleganza | La difesa compone la traccia |
| **Random Dice / Rush Royale** | La tensione della casualità | Casualità **controllata**: rilanci, niente sconfitte "per sfortuna" |
| **Block Blast** | Immediatezza, sessioni brevi, soddisfazione | Una mano sola, in verticale, sessioni brevi, feedback audiovisivo forte |
| **Clash of Clans (Gold Pass)** | Pass utile che accelera | Backstage Pass (vedi `02`) |

## 4. Cosa facciamo che gli altri non fanno (e per cui vengono criticati)

| Critica agli altri | Nostra risposta |
|---|---|
| Pay-to-win (Rush Royale, Random Dice) | **Mai pay-to-win**. Si accelera solo la progressione, e **le modalità competitive hanno la stessa dotazione per tutti** |
| Metà gioco a pagamento (Kingdom Rush 5, PvZ2) | Tutti i contenuti che influenzano il gioco **si ottengono giocando**. Chi non vuole pubblicità paga una volta sola ("VIP per sempre") |
| Grind infinito (Infinitode 2, BTD6) | Lo Studio ha un **tetto finale**. Dopo il massimo restano le sfide, non il farming |
| Energia e timer (Arknights, genere) | **Zero energia, zero timer di attesa** |
| Passività ("guardo e aspetto") | Scelte a ogni ondata, il **Drop attivo**, costruzione del labirinto |
| Ripetitività | Mappe generate, 1 su 3, Vinili, Livelli di Pressione |
| Casualità ingiusta | Rilanci, anteprima delle ondate, regole di equità nel generatore |
| Informazioni scarse | **Anteprima delle ondate**, danno reale per torre, velocità 1–3x, pausa, **annulla**, vendita a prezzo pieno in preparazione |
| Pubblicità invasive | **Solo rewarded, mai forzate** (vedi `02`) |
| Serve internet | **Funziona offline**; classifiche e cloud quando c'è rete |
| Versione mobile peggiore di quella Steam | Il mobile è la piattaforma principale, e **la versione mobile è quella completa** |

**Cosa nessuno fa insieme:**
1. la difesa che **compone musica** su mobile;
2. la **difficoltà espressa come BPM** (Deep House → Techno);
3. **nemici che si "sentono"** (Silence svuota la musica);
4. un **tower defense musicale ma strategico**, senza riflessi obbligatori;
5. *(aggiornamento)* **ogni run genera una traccia unica condivisibile**: marketing gratuito prodotto dai giocatori.

---

## 5. Si parte in single player? **Sì.**

**Perché:**
- **Nessun server da gestire**: niente costi, niente manutenzione, niente problemi di connessione. Per 1–2 persone è fondamentale.
- **Offline**: è un punto di forza molto richiesto su mobile.
- **Scope**: il multiplayer moltiplica tempi e bug. Thronefall e Gnomes dimostrano che si finisce solo con uno scope piccolo.
- **Equità facile**: niente matchmaking, niente pay-to-win possibile.

**Il social arriva lo stesso, senza server nostri:**
- **Google Play Games Services v2** (plugin ufficiale per Unity): **classifiche** per Daily Mix e Weekly Set, **obiettivi**, **salvataggio nel cloud**.
- **Seme condivisibile**: "prova a battere il mio punteggio su questa mappa".
- Rischio: i punteggi calcolati sul telefono si possono falsificare. All'inizio lo accettiamo. Più avanti: **verifica tramite replay** (seme più sequenza di mosse, ricalcolati).
  ⚠️ La verifica dei replay richiede un piccolo backend (per esempio una funzione serverless): è l'unica eccezione prevista al "nessun server", solo se le classifiche lo giustificano.

**Più avanti, solo se la retention lo giustifica:**
- **Co-op asincrono** (ognuno difende metà traccia e le metà si uniscono), oppure co-op in tempo reale.
- Niente PvP pay-to-win, mai.

---

## 6. Punti di forza dell'idea

1. **Originale e facile da raccontare**: "la tua difesa compone una traccia". Si capisce in 3 secondi di video.
2. **Perfetta per i video brevi**: audio e video a tempo sono soddisfacenti da guardare. Ogni partita è contenuto per TikTok e Shorts.
3. **Minimal = fattibile da soli con l'AI**: forme semplici, stile coerente, prestazioni alte.
4. **Nel filone delle tendenze**: roguelite, sessioni brevi, un'estetica "cozy / focus" per adulti (musica house, relax con profondità).
5. **Profondità vera**: labirinto, sinergie, scelte, Livelli di Pressione.
6. **Monetizzazione onesta e comunicabile**: "mai pay-to-win" diventa un argomento di marketing.
7. **Rigiocabilità infinita** a costo di contenuto basso (generazione procedurale più combinazioni).

## 7. Rischi e come li gestiamo

| Rischio | Mitigazione |
|---|---|
| **Molti giocano senza audio** | Il ritmo si vede (pulsazioni, anelli, flash). Il gioco è **pienamente giocabile muto**. Nei video degli store la musica fa da gancio |
| La qualità della musica è decisiva | Sistema a strati e stem, tonalità fisse, quantizzazione; vedi `03` |
| Fotosensibilità (flash al neon) | Opzione **"riduci flash ed effetti"** dal primo avvio |
| Il ritmo scoraggia chi "non ha orecchio" | Nessun input ritmico obbligatorio; assistenza per il Drop |
| Il generatore produce mappe brutte o ingiuste | Regole e validatore, e test automatici su migliaia di semi |
| Scope che cresce | Un MVP rigido (§8), il resto va negli aggiornamenti |
| Esistono già TD minimal | L'elemento distintivo è la **musica**, non solo lo stile |

## 8. Prima versione giocabile (MVP) *(stima)*

- 1 settore completo (Deep House 120 BPM) più boss; generatore di mappe; labirinto a celle.
- 5 torri (Kick, Hi-Hat, Bass, Clap, Pad), 5 nemici, 1 boss, 15 potenziamenti, 6 Vinili.
- Drop attivo, anteprima ondate, velocità, pausa, annulla.
- Studio base e 3 Livelli di Pressione.
- **Tempi indicativi con l'aiuto dell'AI** *(stima)*:
  | Fase | Tempo |
  |---|---|
  | Prototipo con forme grigie e metronomo (GDD §22) | 2–3 settimane |
  | Vertical slice con musica e grafica finale | circa 3 mesi |
  | Soft launch | 6–9 mesi |

**Regola d'oro dal prototipo:** se non è divertente con cubi grigi e un metronomo, non si va avanti con la grafica.
