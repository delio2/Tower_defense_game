# 01 — Concept del gioco (v2)

> **In una frase:** difendi un nucleo al centro dello schermo da nemici che arrivano da ogni lato,
> **montando moduli sull'anello che lo circonda**: i moduli vicini si combinano e i danni crescono fino a numeri enormi.
> Partite da 10–15 minuti, **niente attese, niente pay-to-win**, offline. E sfidi le partite registrate degli altri giocatori.
>
> *Titolo da definire. Il concept v1 ("difesa musicale a labirinto") è in `archivio/`, e le ragioni della svolta sono in `04` D17.*

---

## 1. Perché questa idea (il buco di mercato)
- **Pubblico enorme e scontento:** il genere "difesa del nucleo con numeri che salgono" (The Tower: circa 1M $ al mese, 7,1M download) piace moltissimo, ma i giocatori odiano timer, pay-to-win, troppe valute, connessione obbligatoria e partite di ore. **Nessuno offre la stessa soddisfazione in modo onesto.**
- **La formula dei successi 2024–2026** (Balatro, Ball x Pit, Backpack Battles): base semplice e familiare + **combo che esplodono** + partite brevi + modello onesto.
- **Il multiplayer asincrono** (Super Auto Pets, Backpack Battles) tiene vivi i giochi senza server costosi: perfetto per uno sviluppatore da solo.

## 2. Il ciclo di gioco
1. **Ondata** (circa 30 s): i nemici arrivano da tutti i lati; i moduli sull'anello sparano da soli; tu decidi **quando** usare l'abilità del nucleo (**Pulse**).
2. **Negozio** (senza limite di tempo): con i crediti guadagnati compri moduli, **unisci i doppioni** per potenziarli, **li disponi sull'anello** per creare combo, vendi e rilanci il negozio. Se risparmi, prendi un piccolo interesse.
3. Dopo 5 ondate arriva il **Guardiano** dell'atto; **3 atti** formano una run (circa 10–15 minuti).
4. Tra una run e l'altra **sblocchi nuovi moduli e nuovi nuclei**: più varietà, non statistiche da grindare.

## 3. Gli elementi distintivi
1. **L'anello delle combo**: 6 slot (fino a 8) attorno al nucleo. Un Amplifier potenzia i suoi due vicini, un Echo fa ripetere i loro colpi, un Prism raddoppia i booster accanto… **La posizione conta**: è un puzzle spaziale che si legge a colpo d'occhio e si condivide con uno screenshot.
2. **Numeri che esplodono**: danno = (base + bonus) × moltiplicatori, in stile Balatro. Da 10 danni all'ondata 1 a milioni nelle build migliori, mostrati con numeri **morbidi e calmi** (docs/03).
3. **Unione dei doppioni**: due moduli uguali ne fanno uno più forte. È la meccanica merge, la più in crescita (+74%), ed è soddisfacente e immediata.
4. **Nuclei diversi come "mazzi"**: ogni nucleo cambia le regole (economia, resistenza, danni), come i mazzi di Balatro. Varietà enorme senza grind.
5. **Partite brevi e oneste**: 10–15 minuti, zero timer e zero energia, funziona offline.
6. **Sfidi i fantasmi degli altri**: la tua partita diventa una sfida per gli altri (§6).

## 4. Cosa prendiamo dai migliori (senza copiare)
| Da | Cosa piace | La nostra versione |
|---|---|---|
| **The Tower** | Nucleo centrale, numeri che salgono, minimal | Stesso piacere, ma **partite brevi, niente timer né pay-to-win** e identità visiva diversa (D20) |
| **Balatro** | Negozio, economia, interesse, jolly, mazzi, escalation | Negozio tra le ondate, moduli come jolly, **nuclei come mazzi** |
| **Backpack Battles** | Disposizione spaziale, PvP asincrono | L'**anello** con combo di vicinanza, duelli contro fantasmi |
| **Super Auto Pets** | Unire i doppioni, asincrono onesto | Merge dei moduli fino al livello 3 |
| **Slay the Spire** | Atti, guardiani, Ascension | 3 atti + Guardiani + Livelli di difficoltà |
| **Vampire Survivors** | Si spara da soli, un solo tocco | I moduli sparano da soli; tu gestisci la build e il Pulse |
| **Arknights** | Conta l'abilità, non i soldi | Modalità competitive con dotazione uguale per tutti |
| **Ball x Pit / Balatro mobile** | Modello onesto (prova + sblocco, o prezzo unico) | Monetizzazione onesta (§7 e `02`) |

## 5. Cosa correggiamo (le critiche agli altri)
| Critica | Nostra risposta |
|---|---|
| Timer di laboratorio da saltare pagando (The Tower) | **Zero timer**: si avanza solo giocando |
| Tornei pay-to-win (The Tower, Rush Royale) | **Competitivo a dotazione fissa**: vince l'abilità |
| Troppe valute | **Due valute in tutto**: crediti (dentro la partita) e Blueprints (sblocchi) |
| Connessione obbligatoria | **Offline completo**; online solo per classifiche e fantasmi |
| Partite di ore | **10–15 minuti**, salvataggio automatico a ogni negozio |
| Grind infinito di statistiche (Infinitode, The Tower) | Si sbloccano **varietà e opzioni**, non numeri |
| Passività ("guardo e aspetto") | Negozio pieno di scelte, disposizione sull'anello, **Pulse attivo** |
| Casualità ingiusta | Rilancio del negozio, anteprima delle ondate, semi condivisibili |
| Pubblicità invasive | Nessuna pubblicità forzata, mai |

## 6. Multiplayer asincrono (D19)
Nessun tempo reale, e **l'offline resta sempre completo**.

| Fase | Modalità |
|---|---|
| **Lancio** | **Daily Run** e **Weekly Run**: stesso seme e stesso nucleo per tutti, classifica. Puoi **scaricare il fantasma** di chi è in cima e giocare vedendo il suo andamento |
| **Aggiornamento 1** | **Leghe settimanali** (gruppi da 30, promozioni e retrocessioni) + **Duelli**: stesso seme di un avversario reale e un punteggio di abilità |
| **Aggiornamento 2** | **Siege**: costruisci un'ondata d'attacco che gli altri devono superare, e vieni premiato se cadono + **boss di comunità** con vita condivisa |

**Come funziona:** ogni partita è un **replay** di pochi KB (seme + mosse). Grazie alla simulazione deterministica (D11) il server lo rigioca per verificare il punteggio, e gli altri possono rivederlo come fantasma.

## 7. Monetizzazione (sintesi, dettagli in `02`)
- Principi fermi: **mai pay-to-win**, niente energia, niente timer, niente pubblicità forzate, niente casse casuali a pagamento.
- **Modello da decidere prima del soft launch** con i dati: (a) **prova gratuita + sblocco del gioco completo** una tantum (il modello di Ball x Pit), oppure (b) **free-to-play onesto** (pubblicità facoltative + pass per accelerare gli sblocchi + VIP senza pubblicità).
- In entrambi i casi le modalità competitive restano **a dotazione fissa**.

## 8. Regole anti-copia (D20)
Le meccaniche sono libere, l'aspetto e i nomi no. **Identità visiva diversa** da The Tower, **interfaccia diversa** (negozio + anello, non schede di statistiche), **nomi originali**, **mai il loro nome** nello store o negli annunci. Revisione di un legale prima del lancio.

## 9. Punti di forza
1. **Domanda dimostrata** (The Tower) **+ critiche chiare** da risolvere = posizionamento netto: "il bello del genere, senza le parti odiose".
2. **Formula moderna** (negozio + combo + merge + numeri) che funziona su mobile.
3. **Leggibile in verticale**: nucleo al centro, anello, nemici tutto attorno.
4. **Fattibile da soli**: forme semplici, niente livelli fatti a mano, multiplayer senza server in tempo reale.
5. **Rigiocabile**: nuclei × moduli × disposizioni × semi.
6. **Marketing naturale**: "guarda questa build", "batti il mio fantasma", screenshot dell'anello.

## 10. Rischi e mitigazioni
| Rischio | Mitigazione |
|---|---|
| Sembrare una copia di The Tower | D20 + meccaniche diverse (negozio, anello, merge, partite brevi) + stile diverso |
| Troppa complessità per il mobile | Pochi moduli all'inizio, sblocco graduale, interfaccia a icone, un tutorial giocato |
| Numeri enormi illeggibili | Notazione compatta (1,2K · 3,4M · 5,6B), numeri solo per i colpi importanti, stile calmo |
| Build "rotte" che banalizzano il gioco | Simulatore automatico di bilanciamento (bot) + Livelli di difficoltà + nuove versioni per stagione |
| Imbrogli nelle classifiche | Verifica dei replay sul server (D19) |
| Pochi giocatori per il multiplayer | L'asincrono funziona anche con pochi giocatori; all'inizio anche fantasmi generati da bot |

## 11. Prima versione giocabile (MVP) *(stima)*
- 1 nucleo, 1 atto (5 ondate + Guardiano), i 14 moduli dell'MVP (armi, booster, economia; GDD §6), negozio con merge, Pulse, i 6 nemici + Guardian (GDD §8). *Il prototipo attuale ne ha 7 moduli e 3 nemici + Guardian.*
- Registrazione e verifica locale dei replay; Daily Run con seme del giorno (classifica solo locale per ora).
- **Tempi con l'aiuto dell'AI** *(stima)*:
  | Fase | Tempo |
  |---|---|
  | Prototipo con forme semplici | 2–3 settimane |
  | Vertical slice con grafica e audio finali | circa 3 mesi |
  | Soft launch | 6–9 mesi |

**Regola d'oro:** se il negozio con l'anello non è divertente con forme semplici, non si va avanti con la grafica.
