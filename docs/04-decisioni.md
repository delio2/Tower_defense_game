# 04 — Registro delle decisioni (dall'alto verso il basso)

Formato: **domanda → dati → cosa fanno i migliori (pregi e errori) → decisione → sicurezza**.
Sicurezza: 🟢 alta · 🟡 media, da confermare nel prototipo · 🔴 bassa, ipotesi.

> ⚠️ **Svolta del 21/09/2026 (D17):** il concept "difesa musicale a labirinto" è stato **abbandonato**.
> Il nuovo concept è "nucleo centrale + moduli con combo + multiplayer asincrono" (D17–D21).
> Le decisioni superate sono marcate **[SUPERATA]**; restano qui come storico.
> Valgono ancora: D1 (giocabile senza audio), D2, D4, D6, D7, D8, D10 (in parte), D11, D13, D16.

---

## D1 — Il gioco deve funzionare senza audio? → **Sì, prima di tutto** 🟢
- **Dati:** le ricerche non concordano. Una indica oltre il 90% di giocatori muti (TapResearch), una più recente del 2025 il 60% con audio e il 9% sempre muti. Su TikTok invece l'88% considera il suono essenziale.
- **Decisione:** *"prima funziona muto, la musica amplifica"*. Nessuna decisione di gioco dipende dal suono: il ritmo si **vede** (pulsazioni), le minacce si **vedono**, e il Drop ha un conto alla rovescia visivo e un aiuto automatico.
  La musica è il gancio del marketing (TikTok è un canale con l'audio acceso) e il "di più" dell'esperienza.
- **Validazione:** il prototipo va giudicato **prima muto**, poi con l'audio.
- *Aggiornamento D17:* la musica non è più una meccanica, ma il principio resta: **nessuna informazione di gioco solo sonora**.

## D2 — Verticale o orizzontale? → **Verticale, una mano** 🟢
- **Dati:** i tower defense mobile di successo recenti sono verticali: Rush Royale, Random Dice, The Tower (7,1M download). Marvel Snap è verticale per giocare con una mano. Bloons e Kingdom Rush sono orizzontali, ma nascono su PC o tablet.
- **Errore da evitare:** PvZ3 è verticale **e** semplificato (niente scelta dei semi, formazioni imposte), ed è stato criticato per la **perdita di profondità**, non per l'orientamento in sé.
- **Decisione:** verticale, **senza scorrimento**, tutti i comandi raggiungibili col pollice. La profondità resta intera.
- *Aggiornamento D17:* niente più griglia 9×14. L'arena è **circolare con il nucleo al centro**, la forma più adatta allo schermo verticale.

## D3 — Quanto dura una partita? → **[SUPERATA da D18: run di 10–15 minuti, 3 atti da 5 ondate + guardiano]** ~~6 ondate + boss per settore, run di circa 17 minuti~~
- **Dati:** la sessione media mobile dura 17 minuti, quella dei giochi di strategia 37,5 (+18% nel 2025). Si giocano 2–6 sessioni al giorno, 4 per i midcore.
- **Calcolo:** un'ondata con preparazione e scelta 1 su 3 dura circa 60 s. Con 8 ondate un settore durerebbe 8–9 minuti e la run circa 25: troppo per l'obiettivo.
- **Decisione:** **6 ondate + boss** per settore (circa 5–6 minuti), run di 3 settori **circa 17 minuti**, con **salvataggio automatico a fine ondata** e ripresa in qualsiasi momento. Le formule sono ricalcolate per mantenere la stessa difficoltà finale (GDD §9).
  Il Daily Mix è un solo settore, circa 6 minuti. Il numero di ondate resta un parametro, da verificare nel prototipo.

## D4 — Quali telefoni supportiamo? → **RAM 4 GB+, Android 8+, target API 36** 🟢
- **Dati sulla RAM:** 8 GB 38,5% · 6 GB 25,3% · 4 GB 16,3% · 12 GB 13% · 3 GB 3,2% · 2 GB 1,7%. **Il 93% ha 4 GB o più.** Nel 2026 i telefoni economici sono in calo per la crisi delle memorie.
- **Dati su Android:** secondo AppBrain (settembre 2026) Android 16 è la versione più diffusa, al 25,2%. Le altre percentuali raccolte venivano da fonti e date diverse, e sommate superavano il 100%: sono state tolte. Prima della pubblicazione si verifica la copertura reale del `minSdk` sul **catalogo dispositivi della Play Console**.
- **Obbligo di Google Play:** dal **31 agosto 2026** le nuove app e gli aggiornamenti devono avere **target API 36** (Android 16).
- **Decisione:** `targetSdk 36`, `minSdk 26` (Android 8, da verificare sul catalogo dispositivi della Play Console). Telefono di riferimento: **fascia bassa con 4 GB**, dove garantiamo 30 fps; 60 fps sulla fascia media.

## D5 — 2D o 3D? → **3D low-poly con camera ortografica** 🟡
- **Motivo:** emissione e bloom (il neon) con URP, profondità nei video (volumi, rotazioni), pipeline Blender/MCP già pronta, e resta leggibile come un 2D. I materiali sono unlit: il vantaggio non è l'illuminazione.
- **Rischio:** prestazioni del bloom sui telefoni economici. Mitigazione: livelli di qualità con bloom disattivabile.
- **Validazione:** confronto veloce nel prototipo (stessa scena in 3D ortografico e in 2D).

## D6 — Online o offline? → **Offline-first + Play Games Services v2** 🟢
- **Dati:** giocare offline è una richiesta ricorrente dei giocatori ed è un pregio di Mini TD 2 e Thronefall. Il plugin ufficiale PGS v2 per Unity supporta classifiche, obiettivi e salvataggi nel cloud. Le **API vecchie sono state rimosse da maggio 2026**: si usa solo la v2.
- **Decisione:** il salvataggio locale fa fede, la sincronizzazione cloud avviene quando c'è rete. Nessun server nostro, nessun account obbligatorio.

## D7 — Quali lingue? → **Poco testo + 6 lingue al lancio** 🟡
- **Dati:** i giochi localizzati incassano il **50–80% in più** fuori dal Paese d'origine. Oltre il 50% del fatturato mobile viene da Cina, Giappone e Corea. Il Brasile e l'America Latina hanno CPI bassi e molti download.
- **Decisione:** interfaccia **con poco testo**, fatta di icone e numeri (è minimal anche in questo).
  - Al lancio: **EN, IT, ES, PT-BR, DE, FR**.
  - Dopo il soft launch: **JA, KO**, poi ZH se i dati lo giustificano.
  - ⚠️ Google Play non opera in Cina continentale, dove pubblicare richiede anche una licenza governativa. Il cinese servirebbe solo per Taiwan e Hong Kong (tradizionale), a meno di aprire un canale apposito.
  - Traduzione con AI più una revisione madrelingua per lo store.

## D8 — Primo avvio (FTUE) → **In partita in meno di 10 secondi, "aha" entro 90 secondi** 🟢
- **Dati:** l'app media perde il **77% degli utenti in 3 giorni**. Se il momento "è divertente" arriva dopo **90 secondi**, molti non tornano. Un onboarding ottimizzato alza la retention fino al 50%.
- **Errori da evitare:** account, impostazioni e schermate di testo all'inizio.
- **Decisione:** nessun menu al primo avvio: si parte da un **atto guidato** (vedi GDD). Unica eccezione: in UE/UK il **modulo di consenso** compare prima del tutorial (D16). Pubblicità solo dopo il tutorial.

## D9 — Costruzione del labirinto → **[SUPERATA: nel nuovo concept non c'è labirinto]** ~~Libera ma mai bloccante~~
- **Dati:** in Fieldrunners e simili i nemici aggirano le torri e **non si può chiudere il percorso**. Il "juggling" (vendi e ricostruisci per far tornare indietro i nemici) divide la community: molti lo considerano un exploit.
- **Decisione:** un piazzamento che chiuderebbe il percorso viene rifiutato, con il motivo mostrato. **Vendita e annulla sono permessi solo in preparazione**, a prezzo pieno. Durante l'ondata si può solo costruire o potenziare. Così il juggling è eliminato alla radice.
- **Tecnica:** flow field calcolato dall'uscita (BFS), ricalcolato a ogni piazzamento (vedi Red Blob Games).

## D10 — Economia della partita → **I nemici che passano non danno ricompensa, e risparmiare paga poco** 🟡
- **Dati:** se un nemico che sfugge costa solo vite, a volte conviene lasciarlo passare. La correzione è che **perda anche la ricompensa**.
- **Decisione:** un nemico che passa costa vite e **nessuna ricompensa**. Piccolo **interesse** a fine ondata (5%, con un tetto) per premiare chi pianifica. "Chiama l'ondata prima" dà un bonus di crediti (una scelta attiva).

## D11 — Simulazione deterministica → **Sì, dal primo giorno** 🟢
- **Motivo:** i semi condivisibili, il Daily Mix uguale per tutti, i replay per verificare le classifiche e le **simulazioni automatiche di bilanciamento** (un bot gioca migliaia di run) richiedono che, dati seme e mosse, il risultato sia sempre identico.
- **Decisione:** logica di gioco su **tempo di simulazione a passo fisso**, RNG con seme, **niente fisica Unity** nel gameplay. Audio e grafica *seguono* la simulazione.
- **Matematica intera o a virgola fissa** per i valori critici (HP, danni, posizioni, tempi): i float possono dare risultati diversi tra processori diversi (ARM e x86), e questo romperebbe la verifica dei replay e il confronto tra dispositivi.
- **Flussi RNG separati** (mappa, ondate, scelte 1 su 3, Vinili, effetti): le scelte del giocatore non devono cambiare la mappa o le ondate. È necessario perché il Daily Mix sia davvero uguale per tutti.
- **Risoluzione dell'orologio: 96 tick per battito** (come nel MIDI). Copre 1/16 (24 tick), 1/32 (12 tick) e le terzine (8 o 16 tick), usati da Hi-Hat "Roll" e dalla carta Polyrhythm.

## D12 — Accelerazione (velocità 2x/3x) in un gioco musicale → **[SUPERATA: la musica non è più una meccanica; la velocità 2x/3x accelera tutto normalmente]**
- **Problema:** i giocatori si aspettano la velocità 2x e 3x, ma raddoppiare il BPM rovinerebbe la musica.
- **Decisione:** con la velocità attiva la simulazione accelera, mentre **la musica resta al suo tempo, attenuata da un filtro** (effetto "sto saltando avanti"). I suoni dei colpi vengono diradati.
- **Risincronizzazione:** quando si torna a 1x, gli strati musicali ripartono allineati **all'inizio della battuta di simulazione successiva**, con una breve dissolvenza (al massimo una battuta di attesa, 2 s a 120 BPM).

---

## D13 — Servizi esterni: quando? → **Non servono per sviluppare e testare da soli** 🟢
Il 90% del gioco si sviluppa e si prova **da soli, senza servizi esterni**. I servizi stanno dietro interfacce (`Services`, GDD §19),
con implementazioni "finte" (log in console) durante lo sviluppo. **Diventano obbligatori appena il gioco esce dalle mani dello sviluppatore.**

### TODO — prima di far provare il gioco ad altre persone (test chiuso)
- [ ] **Account Google Play**: scegliere tra personale e aziendale, verificare l'identità, riservare il nome del pacchetto
      *(il personale richiede 12 tester × 14 giorni + circa 7 giorni di revisione; l'aziendale richiede D-U-N-S e può richiedere settimane → avviare con anticipo)*
- [ ] **Analytics + crash report** (Firebase o GameAnalytics → da ricercare)
- [ ] Eventi analytics: tutorial, inizio/fine run, settore raggiunto, sconfitta (ondata e causa), scelte 1 su 3, uso del Drop
- [ ] Privacy policy minima (serve già per il test chiuso se si raccolgono dati)
- [ ] ⚖️ **Consenso al primo avvio (D16)**: integrare Google UMP **prima** del tutorial (solo dove è obbligatorio) e far partire analytics e crash report **dopo** la risposta. Da far verificare a un legale prima della pubblicazione
- [ ] Discord / gruppo di tester (almeno 12 per l'account personale)

### TODO — prima di pubblicare (soft launch e lancio)
- [ ] **Mediazione pubblicitaria**: AdMob o Unity LevelPlay → da ricercare (confronto 2026)
- [ ] **Acquisti in-app**: Unity IAP o Google Play Billing diretto → da ricercare
- [ ] **Consenso GDPR** con Google UMP (obbligatorio in UE/UK) + privacy policy completa
- [ ] Classificazione per età **IARC** e questionario sui contenuti di Google Play
- [ ] Dichiarazione dei contenuti generati con AI (policy di Google Play)
- [ ] **Nome del gioco** + scheda dello store (icona, screenshot, video, ASO) + pre-registrazione
- [ ] Google Play Games Services v2 (classifiche, obiettivi, cloud) configurato sulla console
- [ ] Aspetti fiscali (partita IVA / regime) → commercialista
- [ ] Diritti della musica definitivi (vedi `03`)

---

## D14 — Muri economici per il labirinto → **[SUPERATA: niente labirinto]** ~~Acoustic Panel a 10 crediti~~
- **Problema:** con 150 crediti iniziali e torri da 40 in su, all'inizio si costruiscono solo 3 torri e il labirinto quasi non esiste.
- **Riferimenti:** nei tower defense a labirinto (Gnomes, Infinitode, le mappe "maze" di Warcraft 3) si usano muri o blocchi economici.
- **Decisione:** **Acoustic Panel**, 10 crediti, nessun attacco, blocca il percorso. Si trasforma in torre pagando la differenza. Non conta per le sinergie basate sui tipi di torre. Dettagli: GDD §6.
- **Da verificare:** che il labirinto non diventi troppo lungo e facile. La leva è il costo del pannello, ed eventualmente un tetto per settore.

## D15 — Potenziare o costruire → **[SUPERATA da D18: i moduli si potenziano unendo i doppioni]** ~~Potenziare conviene~~
- **Problema:** con +60% per il 100% del costo, potenziare rendeva meno che costruire. In un labirinto con spazio limitato questo spinge a riempire la griglia di torri deboli.
- **Decisione:** L2 **+80% per 0,6×**, L3 **+100% per 0,9×** (più il ramo). Una torre al L3 rende 1,12 per credito, contro 1,00 di una torre nuova. Premia il labirinto compatto e le scelte dei rami; le torri nuove restano utili per coprire il percorso.

## D16 — Consenso GDPR e analytics → **Consenso al primo avvio, prima del tutorial** 🟢 *(deciso dall'utente)*
- **Opzioni considerate:** raccogliere dati anonimi prima del consenso (tiene il primo avvio senza schermate, ma è un'area grigia legale) · consenso al primo avvio · nessun dato fino al consenso (si perde l'analisi del tutorial).
- **Decisione:** **modulo di consenso Google UMP al primo avvio**, prima del tutorial. Compare solo dove è obbligatorio (UE/UK); altrove si entra subito nel gioco. Analytics e crash report partono dopo la risposta, e rispettano la scelta.
- **Costo accettato:** qualche secondo in più prima del divertimento per i giocatori europei, in cambio della massima sicurezza legale.
- ⚠️ Da far verificare a un legale prima della pubblicazione.

---

## D17 — Svolta: niente musica come meccanica → **Nuovo concept "nucleo + moduli + combo"** 🟢 *(deciso dall'utente)*
- **Perché abbandonare la musica:** con D1 (giocabile senza audio) la musica sarebbe diventata decorazione. Il gioco restava "un altro TD minimal a labirinto" in una nicchia affollata (Infinitode, Emberward, Gnomes). Anche il primo prototipo non risultava attraente.
- **Dati della nuova direzione:**
  - Balatro: 5M+ copie, primo tra i giochi a pagamento su mobile, circa 21M $ solo mobile. La formula è una **base familiare + combo roguelite + numeri che esplodono**.
  - Ball x Pit: 1M+ copie, circa 10M $, su mobile con prova gratuita + sblocco.
  - Uscite di giochi a pagamento su mobile **+77% nel 2025**.
  - **The Tower**: circa 1M $ al mese e 7,1M download, ma è criticato per timer di laboratorio, pay-to-win, troppe valute, connessione obbligatoria e partite di ore. È un **pubblico enorme scontento**.
- **Decisione:** tower defense con **nucleo al centro, nemici da tutti i lati, moduli che si combinano** e numeri che crescono. Partite brevi, onesto, offline. Dettagli: `01` e `05`.

## D18 — Ciclo di gioco → **Negozio tra le ondate + anello di slot + abilità attiva** 🟢 *(deciso dall'utente)*
- **Moduli:** si comprano in un **negozio tra un'ondata e l'altra**, alla Balatro: crediti, rilancio a pagamento, vendita, interesse sul risparmio. **Comprare un doppione lo unisce e lo potenzia** (fino al livello 3); il merge è il sottogenere più in crescita (+74%).
- **Posizione:** 6 slot (espandibili a 8) su un **anello attorno al nucleo**. I moduli vicini si combinano: è un piccolo puzzle spaziale, alla Backpack Battles, e le build sono leggibili e condivisibili.
- **Durante l'ondata:** un'**abilità attiva del nucleo** (Pulse) con ricarica. Dà un momento di scelta senza agitare.
- **Durata:** 3 atti × (5 ondate + guardiano) = 18 ondate, **circa 10–15 minuti**.

## D19 — Multiplayer → **Solo asincrono, costruito sui replay deterministici** 🟢 *(deciso dall'utente)*
- **Dati:** Super Auto Pets, Backpack Battles e The Bazaar sono successi con il **PvP asincrono** contro partite registrate. Non servono server in tempo reale e il gioco funziona anche con pochi giocatori.
- **Decisione:** nessun tempo reale. Modalità asincrone introdotte a fasi:
  1. **al lancio**: sfida del giorno e della settimana (stesso seme, stessa dotazione) con classifiche + fantasmi dei migliori da sfidare;
  2. **aggiornamento 1**: leghe settimanali (gruppi da 30) + duelli contro fantasmi con punteggio di abilità;
  3. **aggiornamento 2**: **Siege** (i giocatori costruiscono ondate d'attacco per gli altri) + boss di comunità.
- **Tecnica:** un **replay** (versione + seme + modalità + lista di tick e comandi) di pochi KB. I punteggi si verificano sul server **rigiocando** la partita con lo stesso motore C# (senza Unity), tramite Unity Gaming Services Cloud Code.
- **I replay valgono solo per la stessa versione di bilanciamento:** classifiche e fantasmi sono divisi per versione o stagione.
- **L'offline resta sempre completo.**

## D20 — Regole anti-copia (The Tower e altri) 🟢
- Le **meccaniche sono libere** (CGUE, *SAS c. World Programming*, 2012). Il **"look and feel" no** (USA, *Tetris c. Xio*, 2012) e nemmeno nomi e marchi (policy "Impersonation" di Google Play, che può chiudere l'account).
- **Regole:** identità visiva diversa (niente neon su nero con una torre quadrata al centro), interfaccia diversa (i moduli arrivano come offerte del negozio e si montano sull'anello, non come schede di statistiche), nomi originali per tutto, **mai "The Tower" in store, annunci, parole chiave o file**.
- Prima del lancio: **un'ora con un avvocato di proprietà intellettuale** per rivedere nome, icona e screenshot.

## D21 — Backend per il multiplayer asincrono → **Unity Gaming Services** 🟡
- **Dati:** UGS si paga a consumo con una soglia gratuita per servizio (classifiche, Cloud Save, Cloud Code in C#). **PlayFab** da marzo 2026 ha ridotto la soglia gratuita a **1.000 giocatori** (-99%). **Nakama** costa circa 10 $ al mese in autogestione, ma va mantenuto.
- **Decisione:** UGS (integrato con Unity, Cloud Code C# per verificare i replay). Da confermare con una stima dei costi prima dell'aggiornamento 1. **Non serve per il lancio offline** (D13).

## Fonti
- Svolta (D17–D21): vedi fonti in `00-ricerca-mercato.md` §Ricerca v2
- Audio muto: https://blog.tapresearch.com/how-sound-preferences-impact-player-engagement · https://www.international-sound-directory.com/2025/12/07/do-people-really-play-mobile-games-without-sound-myth-or-reality/
- TikTok e suono: https://www.socialmediatoday.com/news/tiktok-shares-new-insights-into-the-importance-of-sound-for-marketing-promo/601569/
- Sessioni: https://gamedevreports.substack.com/p/adjust-gaming-app-insights-report · https://www.blog.udonis.co/mobile-marketing/mobile-games/mobile-gaming-statistics
- Versioni Android 2026: https://www.androidheadlines.com/2026/01/android-version-distribution-numbers-2025-2026-market-share.html · https://www.appbrain.com/stats/top-android-sdk-versions
- Target API: https://support.google.com/googleplay/android-developer/answer/11926878?hl=en · https://developer.android.com/google/play/requirements/target-sdk
- RAM dei dispositivi: https://commandlinux.com/android/android-global-market-share-statistics/ · https://www.idc.com/resource-center/blog/smartphone-shipments-set-for-record-16-7-drop-in-2026-as-the-memory-crisis-hits-full-force/
- PGS v2 Unity: https://github.com/playgameservices/play-games-plugin-for-unity · https://developer.android.com/games/pgs/unity/migrate-to-v2
- Localizzazione: https://www.transphere.com/mobile-games-localization/ · https://speequalgames.com/predicting-2026-gaming-landscape-through-the-lens-of-mobile-game-localization/
- FTUE: https://blog.playio.co/mobile-game-onboarding-retention · https://segwise.ai/blog/mobile-gaming-app-user-retention-strategies
- Mazing: https://maultactics.gg/articles/mazing-guide · https://en.wikipedia.org/wiki/Fieldrunners
- Flow field: https://www.redblobgames.com/pathfinding/tower-defense/
- Bilanciamento TD: https://www.gamedeveloper.com/design/balance-in-td-games
- PvZ3: https://en.wikipedia.org/wiki/Plants_vs._Zombies_3:_Evolved
- Vertical one-hand: https://rovingames.com/blog/one-handed-mobile-games-for-real-life-breaks/
