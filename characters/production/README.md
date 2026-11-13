# Character Production Package

This folder contains the production character visuals for the 800 x 800 retro RPG.

## Locked visual standard

- Presentation: front-facing and four-direction retro RPG sprite atlases.
- Medium: detailed late-1990s fantasy pixel art with crisp hard edges and a restrained palette.
- Background: genuine transparency; no scenery, labels, grids, shadows, text, or watermark.
- Base clothing: charcoal-brown lace-up tunic, dark trousers, brown belt and boots, matching the baseline sheets.
- Identity: race anatomy and body proportions stay consistent between every pose.
- Equipment: no weapons, shields, armor, jewelry, bags, or class-specific props in base atlases.
- 800 x 800 integration: assets are authored as source atlases and later normalized to the project's exact cell grid with nearest-neighbor scaling.

## Required races and body sets

1. Human — male and female — Medium
2. Elf — male and female — Slim
3. Dwarf — male and female — Broad
4. Halfling — male and female — Short
5. Gnome — male and female — Short
6. Half-Elf — male and female — Medium
7. Drow — male and female — Slim
8. Half-Orc — male and female — Large
9. Tiefling — male and female — Horned
10. Dragonborn — male and female — Dragonborn
11. Githyanki — male and female — Slim

## Atlas contents per character

- Four idle directions: front, right, back, left
- Walk cycle keyframes in four directions
- Melee attack keyframes in four directions
- Spell-casting keyframes in four directions
- Defensive stance
- Hit reaction
- Death sequence
- Talk gesture
- Sitting pose
- Item-use pose

## Portrait contents per race

- Male and female head-and-shoulders portraits
- Neutral, friendly, angry, afraid, hurt, and determined expressions
- Later face/skin/hair variants must reuse the same portrait crop and lighting

## Folder layout

- `atlases/<race>/male/`
- `atlases/<race>/female/`
- `portraits/<race>/`
- `qa/`

