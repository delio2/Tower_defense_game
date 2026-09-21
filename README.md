# Tower Defense Game (titolo provvisorio)

Difesa del nucleo roguelite per mobile (Android, poi iOS): il nucleo al centro, nemici da ogni lato,
**moduli sull'anello che si combinano**, negozio tra le ondate, partite da 10–15 minuti,
**offline** e con multiplayer **asincrono** basato su replay deterministici. Mai pay-to-win.

- **Motore:** Unity 6 (6000.6.2f1), URP, C#
- **Stato:** pre-produzione, prototipo v2 giocabile nell'editor (settembre 2026)

## Documentazione
- [docs/README.md](docs/README.md) — indice: ricerca di mercato, concept, monetizzazione, grafica e UX, decisioni, **GDD**
- [docs/setup/SETUP-NUOVO-PC.md](docs/setup/SETUP-NUOVO-PC.md) — **come configurare un nuovo PC** per sviluppare il gioco
- [CLAUDE.md](CLAUDE.md) — regole per lo sviluppo con Claude Code

## Struttura del codice
| Cartella | Contenuto |
|---|---|
| `Assets/Scripts/Simulation` | Simulazione deterministica pura in C# (senza Unity): arena, anello, moduli, negozio, ondate, replay |
| `Assets/Scripts/Presentation` | Parte visiva e input del prototipo |
| `Assets/Tests/EditMode` | Test automatici della simulazione (Test Runner → EditMode) |
| `Assets/Scenes/Prototype.unity` | Scena del prototipo |

## Avvio rapido
1. Segui [docs/setup/SETUP-NUOVO-PC.md](docs/setup/SETUP-NUOVO-PC.md).
2. Apri `Assets/Scenes/Prototype.unity` e premi **Play** (Game View verticale 1080×1920).
