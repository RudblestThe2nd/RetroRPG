from __future__ import annotations

import argparse
import json
from pathlib import Path

from PIL import Image, ImageDraw


CELL = 256
COLS = 6
ROWS = 4

ENEMIES = {
    "forest": ["wolf", "dire-boar", "giant-spider", "thorn-goblin", "bandit-scout", "root-cultist", "fungal-crawler", "vine-wraith", "corrupted-dryad", "sapling-guardian", "horned-forest-bear", "ancient-bark-brute"],
    "swamp": ["bog-leech", "marsh-serpent", "giant-toad", "swamp-crocodile", "mire-goblin", "plague-zombie", "fungus-brute", "reed-stalker", "bog-wraith", "mud-elemental", "swamp-witch", "moss-troll"],
    "desert": ["giant-scorpion", "dune-jackal", "razor-vulture", "sand-serpent", "dune-bandit", "tomb-raider", "dust-skeleton", "scarab-husk", "desert-ghoul", "sand-elemental", "mummy-guardian", "dune-ogre"],
    "snow": ["frost-wolf", "ice-spider", "snow-stalker", "tundra-raider", "frozen-corpse", "ice-wraith", "frost-golem", "horned-yeti", "glacial-serpent", "winter-witch", "armored-ice-troll", "blizzard-elemental"],
    "mountain": ["cliff-goat", "rock-viper", "giant-eagle", "harpy-raider", "mountain-bandit", "stone-goblin", "cave-bear", "gargoyle", "rock-elemental", "armored-ogre", "storm-shaman", "peak-troll"],
    "tropical": ["jungle-panther", "venom-frog", "giant-centipede", "constrictor-serpent", "tribal-raider", "thorn-hunter", "carnivorous-plant", "jungle-zombie", "vine-beast", "feathered-serpent", "poison-shaman", "temple-guardian"],
    "volcanic": ["ash-hound", "lava-beetle", "cinder-bat", "magma-serpent", "ash-raider", "fire-cultist", "charred-skeleton", "smoke-wraith", "magma-elemental", "obsidian-golem", "flame-shaman", "lava-troll"],
    "corrupted": ["void-rat", "eye-stalker", "twisted-hound", "corrupted-knight", "shadow-cultist", "flesh-vine", "nightmare-wraith", "crystal-mutant", "void-elemental", "many-eyed-brute", "plague-oracle", "abyssal-abomination"],
    "underground": ["cave-rat", "giant-centipede", "cave-spider", "blind-lizard", "tunnel-goblin", "fungal-thrall", "crystal-beetle", "shadow-stalker", "cave-troll", "stone-golem", "deep-cultist", "subterranean-horror"],
    "plains": ["wild-dog", "charging-boar", "giant-hawk", "grass-serpent", "highway-bandit", "plains-raider", "scarecrow-husk", "field-zombie", "wind-wraith", "hay-elemental", "war-ogre", "storm-brute"],
}


def threat_tier(index: int) -> str:
    if index < 4:
        return "common"
    if index < 8:
        return "uncommon"
    if index < 10:
        return "elite"
    return "rare"


def build(root: Path, biome: str, source_name: str) -> dict:
    enemy_ids = ENEMIES[biome]
    biome_root = root / biome
    source_path = biome_root / "source" / "imagegen" / source_name
    sprites_root = biome_root / "sprites"
    atlases_dir = biome_root / "atlases"
    manifests_dir = biome_root / "manifests"
    previews_dir = biome_root / "previews"
    for folder in (sprites_root, atlases_dir, manifests_dir, previews_dir):
        folder.mkdir(parents=True, exist_ok=True)

    source = Image.open(source_path).convert("RGBA")
    if source.size != (COLS * CELL, ROWS * CELL):
        raise ValueError(f"Unexpected source size: {source.size}")

    atlas = Image.new("RGBA", source.size, (0, 0, 0, 0))
    records = []
    errors = []
    for row in range(ROWS):
        pose = "idle" if row % 2 == 0 else "attack"
        group_offset = 0 if row < 2 else 6
        for column in range(COLS):
            enemy_index = group_offset + column
            enemy_id = enemy_ids[enemy_index]
            sprite = source.crop((column * CELL, row * CELL, (column + 1) * CELL, (row + 1) * CELL))
            folder = sprites_root / enemy_id
            folder.mkdir(parents=True, exist_ok=True)
            filename = f"{biome}-{enemy_id}-{pose}-v1.png"
            path = folder / filename
            sprite.save(path)
            atlas.alpha_composite(sprite, (column * CELL, row * CELL))
            if sprite.getchannel("A").getbbox() is None:
                errors.append(f"Empty sprite: {path.relative_to(biome_root).as_posix()}")
            records.append({
                "id": f"{biome}-{enemy_id}-{pose}",
                "biome": biome,
                "enemy": enemy_id,
                "pose": pose,
                "threat_tier": threat_tier(enemy_index),
                "file": path.relative_to(biome_root).as_posix(),
                "source_cell": [column, row],
                "anchor": [128, 238],
            })

    atlas_path = atlases_dir / f"{biome}-enemies-transparent-atlas-v1.png"
    atlas.save(atlas_path)
    alpha_extrema = source.getchannel("A").getextrema()
    if alpha_extrema[0] != 0 or alpha_extrema[1] < 240:
        errors.append(f"Source transparency is insufficient: {alpha_extrema}")

    manifest = {
        "id": f"{biome}-enemy-pack-v1",
        "biome": biome,
        "enemy_count": len(enemy_ids),
        "pose_count": 2,
        "sprite_count": len(records),
        "sprite_size": [CELL, CELL],
        "anchor": [128, 238],
        "enemies": enemy_ids,
        "source_alpha_extrema": list(alpha_extrema),
        "atlas": atlas_path.relative_to(biome_root).as_posix(),
        "sprites": records,
    }
    (manifests_dir / f"{biome}-enemy-pack-v1.json").write_text(
        json.dumps(manifest, ensure_ascii=False, indent=2) + "\n", encoding="utf-8"
    )

    preview = Image.new("RGBA", source.size, (25, 27, 32, 255))
    draw = ImageDraw.Draw(preview)
    for y in range(0, source.height, 32):
        for x in range(0, source.width, 32):
            color = (45, 48, 56, 255) if (x // 32 + y // 32) % 2 == 0 else (34, 37, 44, 255)
            draw.rectangle((x, y, x + 31, y + 31), fill=color)
    preview.alpha_composite(atlas)
    preview.convert("RGB").save(previews_dir / f"{biome}-enemy-pack-v1-preview.png")

    qa = {"status": "pass" if not errors else "fail", "checked": len(records), "errors": errors}
    (manifests_dir / "qa-report-v1.json").write_text(
        json.dumps(qa, ensure_ascii=False, indent=2) + "\n", encoding="utf-8"
    )
    return qa


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, required=True)
    parser.add_argument("--biome", choices=sorted(ENEMIES), required=True)
    parser.add_argument("--source", required=True)
    args = parser.parse_args()
    print(json.dumps(build(args.root.resolve(), args.biome, args.source), ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
