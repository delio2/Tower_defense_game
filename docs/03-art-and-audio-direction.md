# 03 — Art and audio direction ("Dusk Garden")

> Version 4 · 2026-09-22 · Status: direction v2 (Phase 2.5, D30): mood shot v1 done, to approve. This is the **art bible**: every visual or audio rule lives here. UI screens and components are in `09`; the living tokens, icons, components and interactive screen mockups are in the **Dusk Garden design system** (https://claude.ai/artifact/5aKRr9H8e3WJsMui64MNnu). Where the two disagree, the design system's tokens win and this file is updated.
> **Feeling to convey:** a geometric garden at dusk. Calm, soft, elegant; the numbers grow but the screen stays serene.
> **Anti-copy rule (D20):** the identity must be **clearly different** from The Tower (neon on black, square tower, range circle).

---

## Part A — Visuals

### A1. Principles (fixed rules)
1. **Calm by default:**
   - no flashing, no full-screen flashes, no effect repeated more than **2 times per second** in the same spot (the photosensitivity guideline limit is 3);
   - soft motion with easing and fades, never on/off jumps;
   - rapid hits are shown as **stable, faint lines**.
2. **Readability:** within half a second you know what is yours, what is enemy and what is dangerous.
3. **Minimal but attractive:** few shapes, each one finished. Soft volume, gentle light, gradients; not flat "prototype" colours.
4. **Elegant, not childish:** no faces, no cartoon colours (`00` A3).

### A2. Visual identity
| Element | Choice | Different from The Tower because |
|---|---|---|
| Background | **Dusk blue-violet gradient** with soft vignette and faint concentric rings that mark the **range bands** (D32) | theirs: solid black |
| Core | **A geometric seed or flower**: a soft sphere with **6 petals (up to 8)**, which are the ring slots | theirs: square tower |
| Modules | Small rounded objects resting on the petals, each with **its own silhouette** (Emitter = gem, Amplifier = ring, Bank = coin…) | theirs: stats in a panel, not objects |
| Enemies | **Angular fragments** (shards, prisms) in coral and rose, floating slowly | theirs: squares and flat shapes |
| Camera | Orthographic 3D **tilted about 35°**: volume and soft shadows | theirs: flat top-down 2D |
| Interface | Soft **offer cards** at the bottom, round Pulse button, rounded typography | theirs: upgrade sheets and lists |

### A3. The rule of three surfaces
"Minimal but detailed" does not mean flat: it means **few shapes, each one finished**. Every object has exactly three layers:
1. **Body:** opaque, softly lit (warm key light + cool ambient): gives volume.
2. **Edge:** thin rim light (1–2 px at 1080p) in the category colour: gives readability on the dark background.
3. **Inside:** low emissive (0.15–0.3): gives "life"; rises to 0.6–1.0 only on Pulse, merge and kills, always with a fade.
No painted textures: materials, colour and light do everything. This keeps weight low, perceived detail high, and makes every asset **generatable by script** in Blender.

### A4. Palette (to validate in the mood shot)
| Role | Colour | Hex |
|---|---|---|
| Background (top → bottom) | indigo → night blue | `#2B2F5A` → `#141630` |
| Arena rings | faint lavender | `#3B3F72` |
| Core and petals | warm ivory | `#F3E9D7` |
| Weapons | aqua / teal | `#5CC8C0` |
| Boosters | soft gold | `#E9B872` |
| Economy | mint | `#9AD9A1` |
| Enemies | coral | `#F07A6A` |
| Elites and Guardians | rose (lightened in v4 for contrast on the far sky) | `#E8709A` |
| Text | ivory | `#F3E9D7` |
| Allies (Sprites, `12` §5) | pale lavender | `#C9C2FF` |

Rules of use:
- **60 / 30 / 10:** 60% background, 30% player elements (ivory, teal, gold, mint), 10% accents (enemy coral, combo gold, boss rose).
- **The enemy hue (coral/rose) never appears** on Core, modules, UI or text. Danger has one colour. Seasonal themes follow the same rule.
- **Shape + colour always together** (colour blindness): round = yours, angular = enemy; the three module categories have three silhouettes as well as three colours. Alternative palettes in the options.
- **Text contrast ≥ 4.5:1** on its panel; in-arena numbers ≥ 3:1 with a soft halo.
- **Night tints, never pure black:** darkest value `#141630`, lightest `#F3E9D7`. Bloom stays soft and the screen never "burns".

**Act skies (v4).** Each act has its own sky theme; only the sky, arena rings, panel surfaces and the key-light colour change. Enemy colours are nudged so they keep **≥ 3:1 on the lightest sky** in every act (checked by script; Alto's Odyssey lesson, Part D).
| Theme | Sky far → mid → near | Key light | Enemy coral |
|---|---|---|---|
| Act I · Dusk | `#3B3F7A` → `#1C2050` → `#0E1030` | `#FFD9A8` warm | `#F07A6A` |
| Act II · Twilight | `#224A60` → `#12324A` → `#081A26` | `#E8F0FF` cool | `#F5846F` |
| Act III · Night | `#27265A` → `#10123A` → `#06071A` | `#C9D4FF` moonlight | `#FF8A78` |
| High contrast | `#1A1C3A` → `#0C0D22` → `#07081A` | white | `#FF9A8A` |
The palette switches only at the act card, never during a wave. Endless stays in Night.

### A5. Shape catalogue (for Blender via MCP)
| Element | Shape | Size (units) | Note |
|---|---|---|---|
| Core | soft sphere + 6–8 flat rounded petals | Ø 1.4; petals at radius 1.5 | breathes (scale 1.00 → 1.03, 4 s) |
| Slot | the petal itself; empty = darker petal; valid target = breathing petal | — | no range circle (D20) |
| Emitter / Scatter / Arc / Lance / Mortar | pointed gem / three gems / gem with a ring / long crystal / squat gem | 0.5 | weapons: **pointed** silhouette |
| Amplifier / Lens / Overclock / Echo | ring / lens / notched ring / double ring | 0.5 | boosters: **annular** silhouette |
| Bank / Salvage / Bulwark / Frost / Capacitor | coin / small chest / round shield / drop / stack | 0.5 | economy: **solid, round** silhouette |
| Drifter | elongated shard | 0.35 | coral |
| Swarmlet | small triangle | 0.2 | in groups of 5, tight |
| Brute | thick hexagonal prism | 0.55 | dark coral, thicker edge |
| Dasher | arrow | 0.35 | short trail during the dash |
| Splitter | shard with a glowing crack | 0.4 | the crack opens on death |
| Warden | shard with an annular halo | 0.45 | the halo shows the shield radius |
| Guardian | large crystal with 3 orbiting shards | 1.0 | deep rose; the shards become the summoned Swarmlets |
| Elite | any enemy with a **double halo** | +10% | never a new colour |

Module levels: L1 = base shape · L2 = a **second, brighter face** · L3 = **three faces** and a golden edge. Readable from afar without numbers.

### A6. Lighting and rendering (URP, mobile)
- **Camera:** orthographic, tilted **35°**; during waves the arena fills the screen width (edge ring 9.0 visible); in the shop the whole ring sits in the upper half (about 4 units, the prototype's 3.4 cropped the petals); 0.6 s ease-in-out transition. No camera motion during a wave.
- **Tone mapping:** none/Standard (sRGB). AgX and filmic curves grey the ivory Core and shift the palette (mood shot v1 lesson).
- **Lights:** one warm directional light (`#FFD9A8`, 35° elevation, from the upper left) + gradient ambient (sky `#3B3F72`, ground `#141630`). Soft shadows only at medium/high quality (1 cascade, 1024 resolution).
- **Post-processing:** bloom threshold 1.1, intensity 0.3 (light: visible only on emissive); vignette 0.25; **no** chromatic aberration, grain or CRT (motion sickness, Balatro lesson); one "dusk" LUT (shadows towards blue, warm highlights).
- **Background:** gradient + concentric rings at 15–20% + vignette. **Still.** Only the Core breathes. **No continuous rotation** of background or ring.
- **The rings are range bands (v4, D32):** ring I 3.0 (Pulse reach; glows softly when the Pulse is ready), II 5.5, III 7.5, edge 9.0 (gates). The band a module reaches lights in its sector while dragging. Allied Sprites (`12` §5) use the `ally` pale lavender `#C9C2FF`, never coral.
- **Budget:** ≤ 40 draw calls, ≤ 60k triangles on screen, 4 materials (one per category + enemies) with the SRP Batcher; shadows and bloom off in the "low" quality level. Target: 30 fps on 4 GB phones with 60 enemies, 60 fps on mid-range (D4).
- **Motion:** enemies move only because they advance, with a slow self-rotation and a light vertical float (±0.03 units, 2 s). All decorative motion turns off with "reduce motion".

### A7. Micro-animation catalogue (all eased, none stepped)
| Moment | Animation | Duration | Curve | Rule |
|---|---|---|---|---|
| Core idle | scale 1.00 → 1.03 | 4 s cycle | sine | off with "reduce motion" |
| Module idle | rises/falls 0.02 units | 3 s cycle | sine, offset per slot | idem |
| Spawn | grows 0.35 → 1 + slow self-rotation | 300 ms | ease-out | a scale-in, not an alpha fade: the lit three-surface shader is opaque, and a fade would need a second, transparent material per enemy |
| Weapon fires | 4% recoil inward; thin line weapon → target | 120 ms; line 200–300 ms | ease-out-back | max 2 visible repeats per second per module: faster fire keeps the line lit instead of repeating; one module never draws overlapping lines |
| Enemy hit | 3% visual nudge outward (visual only; the simulated position does not change) | 80 ms | ease-out | numbers only for kills and hits > 25% of HP |
| Enemy dies | 6–10 flakes, alpha 1 → 0, scale 1 → 0.6 | 400 ms | ease-out | cap: 12 animated deaths at once, the rest just fade |
| Pulse | ring 0 → 3.0 units, alpha 0.6 → 0; Core emissive 0.3 → 1.0 → 0.3 | 500 ms; 600 ms | ease-out | no white flash |
| Knockback | slide backwards (the simulation jumps in one tick, the visuals interpolate) | 250 ms | ease-out | — |
| Card picked | +12 px, scale 1.05, wider shadow | 120 ms | ease-out | light haptic |
| Magnet | object slides into the slot | 90 ms | ease-out | haptic tick |
| Valid drop | settle with 5% overshoot | 180 ms | ease-out-back | medium haptic |
| Invalid drop | float back to origin | 250 ms | ease-in-out | no error colour, no text |
| Merge | attraction, fusion, scale 1.15 → 1, soft glow | 300 + 150 ms | ease-in, ease-out | "success" haptic (two taps) |
| Active combo | golden link line alpha 0 → 0.7 | 400 ms | ease-out | stays lit in the shop; 30% during waves |
| Wave end | ring pulses outward | 800 ms | ease-out | then the summary |
| Summary | digits roll, coins add one by one | 600–1,200 ms | ease-out | **tap = skip** |
| Core hit | coral tint 0.3 → 0 | 400 ms | ease-out | no screen shake by default |
| Shop ⇄ wave | camera move + cards rise/fall staggered by 40 ms | 600 ms | ease-in-out | — |

**Global rules:** never more than 2 repeats per second in one spot; no full-screen flash; with "reduce motion" only functional animations remain (fire, deaths, Pulse), shortened by 50%; "effect intensity" scales alpha and flake count.

### A8. On-screen readability
- **Three depth planes:** background (range-band rings at 15–20%), arena (Core, modules, enemies, effects), interface (dark glass: ivory at 6%, 1 px border at 12%).
- **Scale hierarchy:** Core 1.4 > Guardian 1.0 > Brute 0.55 > modules 0.5 > Drifter 0.35 > Swarmlet 0.2. Bigger = more important.
- **Typography (v4, PROPOSAL):** **Outfit** (SIL OFL) for every number and for act/Guardian titles, **Nunito** (SIL OFL) for words; tabular figures for counters. Minimum 14 sp for in-game numbers, 16 sp in the interface, 20 sp for Credits and wave. Text size option 100–200%. Styles are the design system's type tokens.
- **Numbers:** compact notation above 9,999 (12.3K · 4.5M · 6.7B), ivory; important hits at 150% in gold, without flash; can be hidden.
- **Clutter control** (for exploding numbers): cap on simultaneous effects; numbers **only for kills** and important hits, a summary at wave end for the rest; effects **attenuate automatically** with many enemies.

### A9. The mood shot
One carefully made screen (Core with petals, 3 modules, some enemies, shop cards), shown to someone **before** producing any other art. It also settles D5 (tilted orthographic 3D vs top-down) and validates the palette.

**v1 (2026-09-22, Blender 5.2 via MCP, EEVEE):** `docs/art/moodshot-wave.png` (wave view) and `docs/art/moodshot-shop.png` (shop close-up); source `Art/Blender/moodshot.blend`; transparent module/enemy renders in `docs/art/renders/`. What it taught:
- the three-surface material works: lit body + fresnel rim + low emissive reads as "finished" with no textures;
- modules must be about **1.7× larger** than the first guess (0.5 → ~0.85 units) to read on the petals; petals need a lighter body (`#6A6FB0`) and an ivory-lavender rim or they turn to mud;
- the AgX view transform greys the ivory Core: use **Standard** (sRGB) so the palette matches the UI;
- warm key light on a violet floor drifts brown: keep the floor blue-violet (`#1C2050` mid) and the key slightly paler (`#FFE2BC`);
- arena rings at 16% alpha are enough; the combo link reads well as a thin gold arc.

### A10. Production with AI
- **Blender via MCP:** parametric scripts for petals, gems, rings, shards (one `.blend` per family); glTF export to `Assets/Models/<family>/`; 1 unit = 1 m.
- **Unity via MCP:** 4 URP Lit materials + 1 Unlit for lines; post-processing volume; prefabs for Core, module (L1–L3 variants), enemy.
- **Card icons rendered from the 3D models** (same light, same camera): free consistency between game and cards.
- Store icons and art: image generator **within this style guide** (palette and shapes above).
- Every new asset is compared with the mood shot before entering the game.

---

## Part B — Tactile interaction ("Tactile Calm")

> **Goal:** the game must be **pleasant to touch**, like Balatro and Marvel Snap, but **without agitation**. Every touch has a rich, immediate response; nothing moves when it does not need to.

### B1. What players love (research, September 2026)
| Game | What they like | What we take |
|---|---|---|
| **Balatro** | Layered "juice": the card lifts on hover (about +12 px, scale 1.05), **drags with physical inertia** and a magnet effect, **numbers roll** digit by digit with a rising tone, jokers trigger **one at a time** while the total updates | Modules that lift, magnetic drag, **progressive counting** of damage, sequential combo activation |
| **Marvel Snap** | **Curated haptics** (a light tap for small actions, an impact for important ones), synced with animation and sound; clean UI with controls **at the bottom** where the thumb is; "dark glass" panels with light | Tiered haptics, bottom UI, soft glass panels |
| **Mini Metro / Mini Motorways** | Minimal without clutter; dragging produces **soft musical sounds**; everything clear and relaxing | Soft sounds while dragging, zero useless elements |
| **Backpack Battles** (PC) | The shop as "arranging objects", satisfying | The pleasure of arranging modules on the ring |
| **2026 trend: "tactile minimalism"** | Surfaces that seem to have weight and material, simple compositions, reactive micro-interactions | Modules and cards with volume, soft shadows, buttons that "press" |

### B2. What they get wrong (to avoid)
| Game | Problem | Our rule |
|---|---|---|
| **Balatro** | **Motion sickness** from the swirling background, always-swaying cards and CRT effect; options came late | **Still** background; no continuous swaying; **"reduce motion"** from day one |
| **Balatro** | Slow scoring with no way to skip (players used mods) | **Animation speed** 1x/2x/instant, tap to skip counting |
| **Vampire Survivors** | **Visual clutter** late in a run; no transparency option | Adjustable **effect intensity** + automatic reduction with many enemies |
| **Backpack Battles mobile** | The **finger covers the text**; imprecise dragging; objects pushed out of place; ruined builds | Dragged object **above the finger**, information **at the top, never under the finger**, magnet on slots, **never automatic moves**, free **undo** |
| **The Tower** | Crowded upgrade menus, small text, too many numbers | Little information at a time, large text, details on request |

### B3. Drag rules (fundamental on mobile)
1. **Pick up:** touch and drag a shop card or a ring module. The object **lifts** (scale 1.1, wider shadow) with a **light haptic**.
2. **Visible above the finger:** the object follows the finger **offset upwards** (about 1 cm), so it is never covered.
3. **Guidance:** valid slots light up softly; near a slot the **magnet** snaps (the object "falls" into the slot) with a small haptic tick.
4. **Preview before confirming:** at the top the effect appears, e.g. *"DPS 16 → 24 (+50%)"* or *"Level 2 → ×1.8"*, and combo lines preview. **The player knows what happens before letting go.** The preview is computed by the simulation (`05` §15.1).
5. **Release:** **soft** elastic settle (max 5% overshoot, 150–200 ms) with a medium haptic. Releasing on an invalid spot floats the object **back to its place**, no error flash.
6. **Sell:** drag a module onto the **"Sell" zone** at the bottom, which shows the refund.
7. **Undo:** a button reverts the last purchase, sale or move of the shop visit. Reroll **cannot** be undone (future offers could be peeked for free).
8. **No-drag alternative:** tap the card, then tap the slot (accessibility).

Exact thresholds (8 dp drag threshold, 0.45-unit magnet radius, 250 ms long-press) are in `07` §1.2.

### B4. Satisfying moments (calm juice)
| Moment | Response |
|---|---|
| **Merge** | The two modules attract, fuse with a soft glow and a slight swell (scale 1.15 → 1); crystalline rising sound; "success" haptic (two taps) |
| **Active combo** | Golden lines between neighbours light up softly when a booster is placed |
| **Wave end** | **Summary with progressive counting**: the wave's damage rolls (0.6–1.2 s), interest adds coin by coin, rising tone. Tap to skip |
| **Pulse** | Soft circular wave, medium haptic, deep sound |
| **Core hit** | Light coral tint fading out, short weak haptic. **No screen shake** by default |
| **Big numbers** | Grow in scale without flashing; compact notation |

### B5. Haptics
- **Few levels, all short:** light (about 15 ms, low amplitude: pick up, magnet, button tap), medium (about 25 ms: place, Pulse, Next wave), success (two taps: merge, victory). Never long or continuous vibrations. **None on errors** (the float-back is enough).
- On Android use `VibrationEffect` with amplitude (available from Android 8, our minimum); phones without amplitude control use shorter durations.
- **Can be disabled** in the options and respect the system setting.

### B6. Thumb layout
- Controls and cards in the **bottom third** of the screen (about 75% of touches are with the thumb).
- In the shop the camera **moves closer and lower** to the ring, towards the thumb zone; during the wave it shows the whole arena.
- Touch targets of **at least 48 dp**.

### B7. Options (most requested by communities)
- **Reduce motion** (no breathing, no trails, no animated zooms), **effect intensity** (0–100%), **damage numbers** (all / big only / none), **animation speed** (1x / 2x / instant), **haptics** (on/off), **text size** (100–200%), **colour-blind** (three presets + shapes), **high contrast**.
- The main options are **offered at first start** or reachable in one tap from pause (`09` §2.7).

---

## Part C — Audio

### C1. Role of audio
Music **is not a mechanic** (D17): it is **atmosphere**. No game information is audio-only (D1). The game is fully playable muted, and the prototype is judged **muted first**.

### C2. Soundtrack
- **Ambient or calm electronic** (lo-fi / downtempo / ambient), in line with the "garden at dusk".
- **Adaptive layers:** more layers as the wave intensifies, few in the shop (relaxing), a tenser layer for the Guardian.
- **Ducks** during the shop to help concentration.

### C3. Sound effects
- Soft and **pleasant**: muffled hits, a crystalline sound for merge, a deep "breath" for the Pulse, a light chime for Credits.
- **Interaction sounds** (Mini Motorways and Balatro lesson): picking up and dragging a module produces soft tuned notes; the magnet makes a warm "click"; the wave-end counter **rises in pitch** at each step. Each UI component has **one** sound; never two identical sounds in a row (±3% random pitch variation).
- Never harsh, never annoyingly repetitive: random pitch variation and a cap on simultaneous sounds.

### C4. Production and rights ⚠️
| Route | Pros | Cons |
|---|---|---|
| **AI (e.g. Suno Pro/Premier)** | Fast, cheap | Commercial rights only for tracks made under an active subscription; you do not own the tracks; lawsuits still open (UMG, Sony) → **risky for the official soundtrack** |
| **Royalty-free music and loops** | Clear licences | Must check that the licence covers use in games |
| **Composer** | Identity and clear ownership | Cost |

**Recommended choice:** AI or free loops for the prototype; clearly licensed loops or a composer for the final version. Sound effects (ElevenLabs SFX, fal.ai via Unity MCP) **after checking the commercial licence**.

## Part D — Visual overhaul v2 (Phase 2.5)

**Why:** the prototype looked like a prototype: flat unlit shapes on one uniform navy, a Core that filled half the shop while modules were small blobs, cards made of four wrapped lines of text with clipped names, a generic system font and an off-palette emoji pause icon, and no way to read the fight (no wave progress, no inspection of modules or enemies).

**Research summary** (full tables with sources in the design system's *Research* section):
| Take from | What | Their criticised weakness → our rule |
|---|---|---|
| Monument Valley | every screen works as a poster | thin, familiar atmosphere → act skies, named Guardians |
| Alto's Odyssey | impressionistic layers, palettes by time of day | night scenes where obstacles vanish → enemy contrast ≥ 3:1 per act, checked |
| Thronefall | silhouette carries information; small layered placement feedback | units hard to tell apart → one silhouette per module, L1–L3 faces |
| Balatro | rolling numbers, colour-coded values, lift and magnet | motion sickness, unskippable scoring → still sky, tap-to-skip, speed 1×/2×/instant |
| Mini Motorways | minimal, per-map palettes, soft sounds | one-recolour colour-blind mode → shapes carry meaning in every preset |
| Marvel Snap | premium cards, curated haptics, thumb-zone UI | fireworks spectacle → glow fades, nothing explodes |
| Kingdom Rush | wave preview icons, "new enemy", encyclopedia | — → wave preview with "new enemy", Codex |
| Sky | light as emotion | floaty touch, heavy on phones → ≤ 100 ms response, quality levels |
| Infinitode 2 | depth with a minimal look | "square tiles and little icons" → three-surface materials, renders |

**What v2 adds:** one sky per act (A4), Outfit + Nunito (A8), a drawn icon set, card art rendered from the models, and an **information layer** that never pauses the game: Integrity arc around the Core, wave progress hairline, wave preview with spawn compass and "new enemy", module tooltip and enemy card on tap, edge markers for off-screen elites and Guardians, a Guardian title card that becomes a health bar, combo inspector on long-press, damage share per module in the wave summary. Every screen (current and future) is mocked up, interactive, in the design system; `09` §2 lists them.

## Sources
- Photosensitivity guidelines (WCAG 2.3.1, three flashes): https://www.w3.org/WAI/WCAG21/Understanding/three-flashes-or-below-threshold.html
- Tetris v. Xio (look and feel): https://en.wikipedia.org/wiki/Tetris_Holding,_LLC_v._Xio_Interactive,_Inc.
- Suno and rights 2026: https://blog.dubspot.com/ai-music-licensing-explained-2026 · https://www.digitalmusicnews.com/2025/12/22/suno-warner-music-deal-changes/
- Adaptive music: https://en.wikipedia.org/wiki/Adaptive_music
- Minimal art: https://pixune.com/blog/minimalist-game-art-guide/
- Balatro, juice and interaction: https://blakecrosley.com/guides/design/balatro · https://medium.com/@yyh19971004/balatro-design-analysis-visual-packaging-and-interactive-feedback-cc6fa6a65370 · https://80.lv/articles/balatro-s-card-movements-shaders-recreated-in-unity
- Balatro, motion sickness: https://steamcommunity.com/app/2379780/discussions/0/4346606879508745169/ · https://www.getdroidtips.com/balatro-motion-sickness-while-playing/
- Balatro, speed requests: https://steamcommunity.com/app/2379780/discussions/0/4201364524144843418/
- Marvel Snap, haptics and UI: https://www.xda-developers.com/marvel-snap-mobile-game-haptics/ · https://medium.com/design-bootcamp/marvels-snap-ui-ux-case-study-9f727d8f3875 · https://www.artstation.com/artwork/GemNDd
- Mini Motorways, sound and interaction: https://www.gamedeveloper.com/audio/-i-mini-motorways-i-and-the-delicate-art-of-marrying-complexity-and-minimalism
- Backpack Battles mobile, control criticism: https://apps.apple.com/us/app/backpack-battles/id6572290447?see-all=reviews&platform=iphone
- Vampire Survivors, visual clutter: https://steamcommunity.com/app/1794680/discussions/0/4631482569784862581/
- UI trends 2026 (tactile minimalism): https://aaagameartstudio.com/blog/mobile-games-art · https://pixune.com/blog/best-examples-mobile-game-ui-design/
- Touch drag and drop: https://smart-interface-design-patterns.com/articles/drag-and-drop-ux/ · https://inkbotdesign.com/mobile-ux/
- Android haptics: https://developer.android.com/develop/ui/views/haptics/haptics-principles · https://developer.android.com/develop/ui/views/haptics/haptics-apis
- Game feel: https://www.gameanalytics.com/blog/squeezing-more-juice-out-of-your-game-design
- Accessibility: https://gameaccessibilityguidelines.com/basic/ · https://caniplaythat.com/2020/01/29/color-blindness-accessibility-guide/
