from __future__ import annotations

import argparse
import json
from pathlib import Path

from PIL import Image, ImageDraw


CELL = 256
SOURCE_COLS = 5
ROWS = 4

GROUPS = {
    "group-a": ["forest", "swamp", "desert", "snow", "mountain"],
    "group-b": ["tropical", "volcanic", "corrupted", "underground", "plains"],
}

SOURCE_LAYOUTS = {
    "group-a": "six-column-grid",
    "group-b": "five-equal-columns",
}

BOSSES = {
    "forest": {"mini": "briar-matriarch", "main": "ancient-hollow-king"},
    "swamp": {"mini": "plague-toad-regent", "main": "mire-hydra-queen"},
    "desert": {"mini": "scarab-warlord", "main": "sunken-pharaoh"},
    "snow": {"mini": "frosthorn-chieftain", "main": "white-maw-titan"},
    "mountain": {"mini": "stormclaw-matriarch", "main": "living-peak-colossus"},
    "tropical": {"mini": "blood-orchid-queen", "main": "sun-temple-behemoth"},
    "volcanic": {"mini": "cinder-wyrm", "main": "heart-of-caldera-titan"},
    "corrupted": {"mini": "eye-bishop", "main": "abyssal-crown"},
    "underground": {"mini": "crystal-broodmother", "main": "deepstone-leviathan"},
    "plains": {"mini": "warband-khan", "main": "tempest-giant"},
}


def checkerboard(size: tuple[int, int]) -> Image.Image:
    preview = Image.new("RGBA", size, (25, 27, 32, 255))
    draw = ImageDraw.Draw(preview)
    for y in range(0, size[1], 32):
        for x in range(0, size[0], 32):
            color = (45, 48, 56, 255) if (x // 32 + y // 32) % 2 == 0 else (34, 37, 44, 255)
            draw.rectangle((x, y, x + 31, y + 31), fill=color)
    return preview


def build(root: Path, group: str, source_name: str) -> dict:
    biomes = GROUPS[group]
    source_path = root / "source" / "imagegen" / source_name
    sprites_root = root / "sprites"
    atlases_dir = root / "atlases"
    manifests_dir = root / "manifests"
    previews_dir = root / "previews"
    for folder in (sprites_root, atlases_dir, manifests_dir, previews_dir):
        folder.mkdir(parents=True, exist_ok=True)

    source = Image.open(source_path).convert("RGBA")
    expected = (1536, ROWS * CELL)
    if source.size != expected:
        raise ValueError(f"Unexpected source size: {source.size}; expected {expected}")

    atlas_size = (SOURCE_COLS * CELL, ROWS * CELL)
    atlas = Image.new("RGBA", atlas_size, (0, 0, 0, 0))
    records: list[dict] = []
    errors: list[str] = []
    row_specs = [
        ("mini", "idle"),
        ("mini", "attack"),
        ("main", "idle"),
        ("main", "attack"),
    ]
    for row, (boss_class, pose) in enumerate(row_specs):
        for column, biome in enumerate(biomes):
            boss_id = BOSSES[biome][boss_class]
            if SOURCE_LAYOUTS[group] == "six-column-grid":
                left = column * CELL
                right = (column + 1) * CELL
            else:
                left = round(column * source.width / SOURCE_COLS)
                right = round((column + 1) * source.width / SOURCE_COLS)
            sprite = source.crop((left, row * CELL, right, (row + 1) * CELL))
            sprite = sprite.resize((CELL, CELL), Image.Resampling.LANCZOS)
            folder = sprites_root / biome / boss_id
            folder.mkdir(parents=True, exist_ok=True)
            filename = f"{biome}-{boss_id}-{pose}-v1.png"
            path = folder / filename
            sprite.save(path)
            atlas.alpha_composite(sprite, (column * CELL, row * CELL))
            bbox = sprite.getchannel("A").getbbox()
            if bbox is None:
                errors.append(f"Empty sprite: {path.relative_to(root).as_posix()}")
            records.append({
                "id": f"{biome}-{boss_id}-{pose}",
                "biome": biome,
                "boss": boss_id,
                "boss_class": boss_class,
                "pose": pose,
                "file": path.relative_to(root).as_posix(),
                "source_cell": [column, row],
                "anchor": [128, 240],
                "collision_profile": "large" if boss_class == "mini" else "huge",
            })

    atlas_path = atlases_dir / f"bosses-{group}-transparent-atlas-v1.png"
    atlas.save(atlas_path)
    alpha_extrema = source.getchannel("A").getextrema()
    if alpha_extrema[0] != 0 or alpha_extrema[1] < 240:
        errors.append(f"Source transparency is insufficient: {alpha_extrema}")

    manifest = {
        "id": f"boss-pack-{group}-v1",
        "group": group,
        "biomes": biomes,
        "boss_count": len(biomes) * 2,
        "sprite_count": len(records),
        "sprite_size": [CELL, CELL],
        "anchor": [128, 240],
        "source_alpha_extrema": list(alpha_extrema),
        "atlas": atlas_path.relative_to(root).as_posix(),
        "bosses": {biome: BOSSES[biome] for biome in biomes},
        "sprites": records,
    }
    (manifests_dir / f"boss-pack-{group}-v1.json").write_text(
        json.dumps(manifest, ensure_ascii=False, indent=2) + "\n", encoding="utf-8"
    )

    preview = checkerboard(atlas_size)
    preview.alpha_composite(atlas)
    preview.convert("RGB").save(previews_dir / f"boss-pack-{group}-v1-preview.png")

    qa = {"status": "pass" if not errors else "fail", "checked": len(records), "errors": errors}
    (manifests_dir / f"qa-report-{group}-v1.json").write_text(
        json.dumps(qa, ensure_ascii=False, indent=2) + "\n", encoding="utf-8"
    )
    return qa


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, required=True)
    parser.add_argument("--group", choices=sorted(GROUPS), required=True)
    parser.add_argument("--source", required=True)
    args = parser.parse_args()
    print(json.dumps(build(args.root.resolve(), args.group, args.source), ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
