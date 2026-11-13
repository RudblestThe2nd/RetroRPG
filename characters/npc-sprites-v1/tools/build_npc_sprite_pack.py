from __future__ import annotations

import argparse
import json
from pathlib import Path

from PIL import Image, ImageDraw


CELL = 256
COLS = 6
ROWS = 4

ROLE_SETS = {
    "civilian": [
        "villager-young",
        "villager-adult",
        "villager-elder",
        "traveler",
        "refugee",
        "minor-noble",
        "dockworker",
        "stablehand",
        "street-vendor",
        "pilgrim",
        "entertainer",
        "household-servant",
    ],
    "profession": [
        "merchant",
        "blacksmith",
        "innkeeper",
        "healer-apothecary",
        "town-guard",
        "farmer",
        "fisher",
        "miner",
        "scholar-scribe",
        "priest",
        "carpenter-artisan",
        "courier",
    ],
}


def build(root: Path, pack: str, source_name: str) -> dict:
    roles = ROLE_SETS[pack]
    source_path = root / "source" / "imagegen" / source_name
    sprites_root = root / "sprites" / pack
    atlases_dir = root / "atlases"
    manifests_dir = root / "manifests"
    previews_dir = root / "previews"
    for folder in (sprites_root, atlases_dir, manifests_dir, previews_dir):
        folder.mkdir(parents=True, exist_ok=True)

    source = Image.open(source_path).convert("RGBA")
    if source.size != (COLS * CELL, ROWS * CELL):
        raise ValueError(f"Unexpected source size: {source.size}")

    atlas = Image.new("RGBA", source.size, (0, 0, 0, 0))
    records = []
    errors = []
    for row in range(ROWS):
        gender = "male" if row < 2 else "female"
        role_index = (row % 2) * COLS
        for column in range(COLS):
            role = roles[role_index + column]
            sprite = source.crop((column * CELL, row * CELL, (column + 1) * CELL, (row + 1) * CELL))
            folder = sprites_root / gender
            folder.mkdir(parents=True, exist_ok=True)
            filename = f"{pack}-{role}-{gender}-npc-v1.png"
            path = folder / filename
            sprite.save(path)
            atlas.alpha_composite(sprite, (column * CELL, row * CELL))
            bbox = sprite.getchannel("A").getbbox()
            if bbox is None:
                errors.append(f"Empty sprite: {path.relative_to(root).as_posix()}")
            records.append(
                {
                    "id": f"{pack}-{role}-{gender}",
                    "pack": pack,
                    "role": role,
                    "gender": gender,
                    "file": path.relative_to(root).as_posix(),
                    "source_cell": [column, row],
                    "anchor": [128, 238],
                }
            )

    atlas_path = atlases_dir / f"{pack}-npc-transparent-atlas-v1.png"
    atlas.save(atlas_path)
    alpha_extrema = source.getchannel("A").getextrema()
    if alpha_extrema[0] != 0 or alpha_extrema[1] < 240:
        errors.append(f"Source transparency is insufficient: {alpha_extrema}")

    manifest = {
        "id": f"{pack}-npc-sprites-v1",
        "pack": pack,
        "sprite_size": [CELL, CELL],
        "anchor": [128, 238],
        "count": len(records),
        "roles": roles,
        "genders": ["male", "female"],
        "source_alpha_extrema": list(alpha_extrema),
        "atlas": atlas_path.relative_to(root).as_posix(),
        "sprites": records,
    }
    (manifests_dir / f"{pack}-npc-sprites-v1.json").write_text(
        json.dumps(manifest, ensure_ascii=False, indent=2) + "\n", encoding="utf-8"
    )

    preview = Image.new("RGBA", source.size, (25, 27, 32, 255))
    draw = ImageDraw.Draw(preview)
    for y in range(0, source.height, 32):
        for x in range(0, source.width, 32):
            color = (45, 48, 56, 255) if (x // 32 + y // 32) % 2 == 0 else (34, 37, 44, 255)
            draw.rectangle((x, y, x + 31, y + 31), fill=color)
    preview.alpha_composite(atlas)
    preview.convert("RGB").save(previews_dir / f"{pack}-npc-sprites-v1-preview.png")

    qa = {"status": "pass" if not errors else "fail", "checked": len(records), "errors": errors}
    (manifests_dir / f"{pack}-qa-report-v1.json").write_text(
        json.dumps(qa, ensure_ascii=False, indent=2) + "\n", encoding="utf-8"
    )
    return qa


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, required=True)
    parser.add_argument("--pack", choices=sorted(ROLE_SETS), required=True)
    parser.add_argument("--source", required=True)
    args = parser.parse_args()
    print(json.dumps(build(args.root.resolve(), args.pack, args.source), ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
