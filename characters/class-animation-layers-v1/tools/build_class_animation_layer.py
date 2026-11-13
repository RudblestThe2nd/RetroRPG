from __future__ import annotations

import argparse
import json
from pathlib import Path

from PIL import Image, ImageDraw


CELL = 256
COLS = 6
ROWS = 4
DIRECTIONS = ["front", "right", "back", "left"]


def row_entries(gender: str) -> list[tuple[str, str, str]]:
    return [
        (gender, "walk", "front"),
        (gender, "walk", "right"),
        (gender, "walk", "back"),
        (gender, "walk", "left"),
        (gender, "attack", "front"),
        (gender, "attack", "right"),
        (gender, "attack", "back"),
        (gender, "attack", "left"),
        (gender, "cast", "front"),
        (gender, "cast", "right"),
        (gender, "cast", "back"),
        (gender, "cast", "left"),
    ]


LAYOUT = row_entries("male") + row_entries("female")


def build(root: Path, class_slug: str, class_name_tr: str, source_name: str) -> dict:
    class_root = root / class_slug
    source_path = class_root / "source" / "imagegen" / source_name
    frames_root = class_root / "frames"
    atlases_dir = class_root / "atlases"
    manifests_dir = class_root / "manifests"
    previews_dir = class_root / "previews"
    for folder in (frames_root, atlases_dir, manifests_dir, previews_dir):
        folder.mkdir(parents=True, exist_ok=True)

    source = Image.open(source_path).convert("RGBA")
    if source.size != (COLS * CELL, ROWS * CELL):
        raise ValueError(f"Unexpected source size: {source.size}")

    atlas = Image.new("RGBA", source.size, (0, 0, 0, 0))
    records = []
    errors = []
    for index, (gender, action, direction) in enumerate(LAYOUT):
        row = index // COLS
        column = index % COLS
        frame = source.crop((column * CELL, row * CELL, (column + 1) * CELL, (row + 1) * CELL))
        folder = frames_root / gender / action
        folder.mkdir(parents=True, exist_ok=True)
        filename = f"{class_slug}-{gender}-{action}-{direction}-outfit-layer-v1.png"
        path = folder / filename
        frame.save(path)
        atlas.alpha_composite(frame, (column * CELL, row * CELL))
        bbox = frame.getchannel("A").getbbox()
        if bbox is None:
            errors.append(f"Empty frame: {path.relative_to(class_root).as_posix()}")
        records.append(
            {
                "id": f"{class_slug}-{gender}-{action}-{direction}",
                "class": class_slug,
                "gender": gender,
                "action": action,
                "direction": direction,
                "file": path.relative_to(class_root).as_posix(),
                "source_cell": [column, row],
                "anchor": [128, 236],
                "layer_order": "body_outfit",
            }
        )

    atlas_path = atlases_dir / f"{class_slug}-outfit-animation-layers-atlas-v1.png"
    atlas.save(atlas_path)

    alpha_extrema = source.getchannel("A").getextrema()
    if alpha_extrema[0] != 0 or alpha_extrema[1] < 240:
        errors.append(f"Source transparency is insufficient: {alpha_extrema}")
    for record in records:
        image = Image.open(class_root / record["file"])
        if image.mode != "RGBA" or image.size != (CELL, CELL):
            errors.append(record["file"])

    manifest = {
        "id": f"{class_slug}-outfit-animation-layers-v1",
        "class": class_slug,
        "class_name_tr": class_name_tr,
        "frame_size": [CELL, CELL],
        "frame_count": len(records),
        "genders": ["male", "female"],
        "actions": ["walk", "attack", "cast"],
        "directions": DIRECTIONS,
        "source_alpha_extrema": list(alpha_extrema),
        "runtime_note": "Composite over the normalized base-body frame using the bottom-center anchor, before held weapons and spell VFX.",
        "atlas": atlas_path.relative_to(class_root).as_posix(),
        "frames": records,
    }
    (manifests_dir / f"{class_slug}-outfit-animation-layers-v1.json").write_text(
        json.dumps(manifest, ensure_ascii=False, indent=2) + "\n", encoding="utf-8"
    )

    preview = Image.new("RGBA", source.size, (25, 27, 32, 255))
    draw = ImageDraw.Draw(preview)
    for y in range(0, preview.height, 32):
        for x in range(0, preview.width, 32):
            color = (45, 48, 56, 255) if (x // 32 + y // 32) % 2 == 0 else (34, 37, 44, 255)
            draw.rectangle((x, y, x + 31, y + 31), fill=color)
    preview.alpha_composite(atlas)
    preview.convert("RGB").save(previews_dir / f"{class_slug}-outfit-animation-layers-v1-preview.png")

    qa = {"status": "pass" if not errors else "fail", "checked": len(records), "errors": errors}
    (manifests_dir / "qa-report-v1.json").write_text(
        json.dumps(qa, ensure_ascii=False, indent=2) + "\n", encoding="utf-8"
    )
    return qa


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, required=True)
    parser.add_argument("--class-slug", required=True)
    parser.add_argument("--class-name-tr", required=True)
    parser.add_argument("--source", required=True)
    args = parser.parse_args()
    print(
        json.dumps(
            build(args.root.resolve(), args.class_slug, args.class_name_tr, args.source),
            ensure_ascii=False,
            indent=2,
        )
    )


if __name__ == "__main__":
    main()
