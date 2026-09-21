---
paths:
  - "docs/**/*.md"
  - "*.md"
---

# Documentation rules

- English only. Header on every document: `Version · date (ISO) · Status`. Sentence-case headings, tables for comparisons, short bullets.
- **One home per topic:** rules and numbers → `05-gdd.md`; visual/audio direction → `03`; decisions and open proposals → `04`; phases and gates → `06`; calendar → `10` §4. A brief (`07`–`10`) may propose, never redefine: mark **PROPOSAL** and add it to `04` §Open decisions.
- Markers: `(estimate)` for our own reasoning, **PROPOSAL** for undecided items, ✅ 🔄 ⬜ for status.
- Cross-references as `NN §x` (e.g. `05 §7`); use links for files. Acronyms are defined in `docs/README.md` — add new ones there.
- **Never renumber `05-gdd.md` sections** (code comments cite them); append new sections.
- When code changes a rule or a number, update the GDD in the same commit and log the reason in `04`.
- Never mention "The Tower" or other games in store-facing text; in research docs cite them as sources only (D20).
