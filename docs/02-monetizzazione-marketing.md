# 02 — Monetizzazione, premi e marketing

> Principio: **"Paghi per arrivare prima, mai per vincere."**
> Pagando si accelera la progressione (come Supercell), ma le modalità competitive hanno **la stessa dotazione per tutti**.
> Qui evitiamo la critica che colpisce Supercell: da loro l'accelerazione pesa nel PvP.

---

## 1. Regole fisse
- ❌ Niente energia, niente timer di attesa, niente casse casuali a pagamento.
- ❌ Niente interstitial, niente banner, niente pubblicità all'avvio.
- ❌ Niente contenuto di gioco venduto **solo** a pagamento: tutto si ottiene anche giocando.
- ✅ Daily Mix e Weekly Set: Studio e acquisti **disattivati**, stessa dotazione per tutti.
- ✅ Funziona offline. La monetizzazione non blocca mai il gioco.

## 2. Pubblicità: solo rewarded (nomi a tema)
| Nome | Momento | Premio |
|---|---|---|
| **Encore** | fine run | Royalties **×2** |
| **Rewind** | sconfitta | riavvolgi l'ultima ondata (1 per run) |
| **Remix** | scelta del potenziamento | rilancia le 3 scelte (1 extra per run) |
| **Crate Digging** | forziere del giorno | contenuto potenziato |

- **Limite di circa 10 al giorno** (per la strategia il riferimento di settore è 8–12). Un premio **legato al momento di bisogno** converte molto di più.
- **Nessun effetto** nel Daily Mix e nel Weekly Set.
- Chi guarda pubblicità rewarded ha una retention fino a **3,5 volte più alta**. È una **correlazione**: i giocatori più coinvolti guardano più pubblicità, non per forza è la pubblicità a trattenerli.
- Mediazione: **AdMob** oppure **Unity LevelPlay** (da decidere al momento dell'integrazione). Pubblico adulto, **non** Families.

## 3. Backstage Pass (stagionale)
Circa **4,99 €** ogni 30 giorni, **acquisto singolo senza rinnovo automatico**, come il Gold Pass di Clash of Clans.

**Tre benefici, spiegabili in tre righe:**
1. 🎬 **Premi senza pubblicità**: Encore, Rewind, Remix e Crate Digging **senza guardare video**. Il Pass toglie il video, **non i limiti**: restano le stesse regole (per esempio Rewind 1 per run).
2. ⚡ **Progressione ×2**: il doppio delle Royalties, così lo Studio si completa in metà tempo.
3. 🎁 **Percorso premi premium**: Royalties, **sblocco anticipato** del nuovo strumento della stagione (che si ottiene comunque gratis più avanti), tema audio e grafico della stagione (tavolozza più un pacchetto di suoni).
   Le tavolozze stagionali devono rispettare le regole di leggibilità di `03` (mai il colore dei nemici sulle torri).

**Regole di design:**
- Il percorso gratuito è per tutti; quello premium si aggiunge sopra.
- **Completabile una settimana prima** giocando in modo normale, con recupero automatico per chi è indietro.
- Valore percepito di circa **10 volte il prezzo**; il percorso premium restituisce buona parte del valore in Royalties.
- ⚠️ **Quando lanciare il Pass:** una fonte indica D30 ≥ 15%, ma è un valore da top 1% (i giochi migliori stanno al 13–15%) e sarebbe irrealistico.
  Criterio adottato: **D30 ≥ 7%** (fascia "buona") **e** un test su un gruppo di giocatori in cui almeno il 40% dei possessori completa il percorso. Al lancio si parte con pubblicità e acquisti singoli.

## 4. Altri acquisti
| Acquisto | Prezzo | Cosa dà |
|---|---|---|
| **Starter Pack** (una volta) | 1,99 € | Royalties più uno strumento subito. Trasforma chi non ha mai pagato in chi paga |
| **VIP per sempre** | circa 7,99 € | Premi senza pubblicità **per sempre**, senza accelerazione. Per chi odia i pass, ed è la risposta al "pago una volta e basta" |
| **Pacchetti di Royalties** | 1,99–19,99 € | Per arrivare prima al massimo |
| **Sound Pack** (estetici) | 1,99–3,99 € | Suoni e tavolozze alternative. Un contorno, non il cuore dei ricavi |

## 5. Premi e ritorno quotidiano
- **Daily Mix**: stesso seme per tutti più classifica. È il motivo principale per tornare.
- **3 missioni giornaliere e 1 settimanale** (per esempio "vinci con solo 3 strumenti").
- **Serie di giorni senza punizione**: se salti un giorno non perdi tutto.
- Obiettivi (Play Games Services) e maestria degli strumenti.

## 6. Obiettivi di bilanciamento *(stima)*
| Giocatore | Studio completo |
|---|---|
| Gratuito con qualche pubblicità | circa 3 mesi |
| Con Backstage Pass | circa 6 settimane |
| Con Pass e pacchetti | 2–3 settimane |

Dopo il massimo restano i Livelli di Pressione (0–20), le sfide e le stagioni. Nessun grind obbligatorio.

---

## 7. Marketing: cosa sbagliano gli altri e cosa facciamo noi

| Errore comune | Nostro approccio |
|---|---|
| **Pubblicità ingannevoli** (gameplay che nel gioco non c'è) | **Solo gameplay vero** nei video, che è anche la cosa che converte meglio nei giochi di gioco puro |
| Marketing solo al lancio | **Build in public dal primo giorno**, con il marketing come parte della produzione |
| Dipendere da un solo canale | Più canali: store (ASO) + video brevi + Reddit + Discord + creator |
| Promettere "free" e poi il pay-to-win | "**Mai pay-to-win**" è un argomento forte, scritto nello store |
| Recensioni negative da pubblicità invasive | Solo rewarded, per puntare a un voto di 4,5★ o più |

### Il vantaggio dell'idea
**La musica fa marketing da sola**: un video di 10–15 secondi in cui la difesa "costruisce" il drop è intrinsecamente condivisibile.
Formato ideale: **inizio in silenzio → piazzi le torri → la traccia cresce → DROP → i nemici esplodono a tempo.**

### Piano per fasi
**Pre-produzione e prototipo (adesso):**
- Account TikTok, YouTube Shorts, Instagram e Reddit con **devlog settimanali** (brevi, spontanei, senza rifinitura).
- Un **Discord** fin da subito: sarà la fonte dei **12 tester** obbligatori per Google Play.
- Test dei concetti: pubblichiamo 3–5 video di "idee" e vediamo quale aggancia di più.

**Vertical slice:**
- **Pre-registrazione su Google Play**.
- Candidatura al **Google Play Indie Games Accelerator / Festival**.
- Contatto con i creator di tower defense e musica elettronica.

**Soft launch (1–2 Paesi economici):**
- Budget di prova **300–1.000 €** su Google Ads (campagne App), usando i **video organici migliori** come annunci.
- Si misurano D1, D7, D30, ARPDAU e costo per installazione. Si scala **solo se il guadagno per giocatore supera il costo**.
- Annunci giocabili: convertono 2–3 volte più degli statici.

**Lancio globale:**
- ASO: icona, i primi 2 screenshot (sono i più importanti), video da 15–30 secondi con il ciclo di gioco, parole chiave ("tower defense", "roguelike", "music", "offline", "no pay to win").
- **Colonna sonora ufficiale** su Spotify e YouTube, se i diritti lo permettono (vedi `03`), come canale di scoperta in più.
- *(aggiornamento)* **Condivisione della traccia della tua run**: contenuto generato dai giocatori.

### Note
- Da TikTok la **portata gratuita è calata dal 2025**: funziona "organico + piccola spinta a pagamento" sui video migliori.
- Il 50–70% delle installazioni arriva dalla ricerca nello store: l'**ASO** non va trascurato.

## Fonti
- Gold Pass (Supercell): https://supercell.com/en/games/clashofclans/blog/news/big-changes-are-coming-to-gold-pass/
- Gold Pass 2026 value: https://www.sportsdunia.com/gaming/clash-of-clans-gold-pass-august-2026
- Battle pass 2026: https://dev.to/sam_novak_574b07811e18495/should-you-add-a-battle-pass-to-your-mobile-game-in-2026-2kao
- Battle pass economics: https://blog.playio.co/battle-pass-monetization
- GameRefinery battle pass: https://www.gamerefinery.com/12-ways-to-take-battle-passes-to-the-next-level-in-mobile-games/
- Rewarded ads 2026: https://maf.ad/en/blog/rewarded-ads-stats/ · https://blog.playio.co/rewarded-ad-benchmarks-2026
- Rewarded best practices: https://appsamurai.com/blog/rewarded-ads-in-mobile-games-strategy-data-and-best-practices/
- Supercell / Mo.co: https://naavik.co/digest/mo-co-and-supercells-new-launch-philosophy/
- Marketing 2026: https://stepico.com/blog/mobile-game-marketing-strategy-in-2026/ · https://www.thegamemarketer.com/insight-posts/effective-ways-to-promote-your-mobile-game
- Indie marketing (TikTok): https://appscalelab.com/indie-devs-marketing-survival-guide-for-2026/
- Ad creatives 2026: https://creative.artstash.io/mobile-game-ad-creative-best-practices/
- Squishy Cats (solo dev, 100k): https://gameplaydev.substack.com/p/my-mobile-game-app-reached-100k-downloads
- Google Play Indie Games: https://play.google.com/console/about/programs/indiegames/
