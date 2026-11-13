from __future__ import annotations

import argparse
import json
from pathlib import Path

from PIL import Image, ImageDraw


SOURCE_CELL = 256
ICON_SIZE = 128
COLS = 6
ROWS = 4


def build(root: Path, category: str, title: str, source_name: str) -> dict:
    source_path = root / "source" / "imagegen" / source_name
    icons_dir = root / "icons" / category
    atlases_dir = root / "atlases"
    manifests_dir = root / "manifests"
    previews_dir = root / "previews"
    for folder in (icons_dir, atlases_dir, manifests_dir, previews_dir):
        folder.mkdir(parents=True, exist_ok=True)

    source = Image.open(source_path).convert("RGBA")
    if source.size != (COLS * SOURCE_CELL, ROWS * SOURCE_CELL):
        raise ValueError(f"Unexpected source size: {source.size}")

    alpha_extrema = source.getchannel("A").getextrema()
    atlas = Image.new("RGBA", (COLS * ICON_SIZE, ROWS * ICON_SIZE), (0, 0, 0, 0))
    records = []
    errors = []

    for row in range(ROWS):
        for column in range(COLS):
            variant = row * COLS + column + 1
            crop = source.crop(
                (
                    column * SOURCE_CELL,
                    row * SOURCE_CELL,
                    (column + 1) * SOURCE_CELL,
                    (row + 1) * SOURCE_CELL,
                )
            )
            icon = crop.resize((ICON_SIZE, ICON_SIZE), Image.Resampling.NEAREST)
            filename = f"{category}-{variant:02d}-v1.png"
            path = icons_dir / filename
            icon.save(path)
            atlas.alpha_composite(icon, (column * ICON_SIZE, row * ICON_SIZE))
            records.append(
                {
                    "id": f"{category}-{variant:02d}",
                    "category": category,
                    "variant": variant,
                    "file": path.relative_to(root).as_posix(),
                    "source_cell": [column, row],
                    "atlas_rect": [
                        column * ICON_SIZE,
                        row * ICON_SIZE,
                        ICON_SIZE,
                        ICON_SIZE,
                    ],
                }
            )

    atlas_path = atlases_dir / f"{category}-icons-transparent-atlas-v1.png"
    atlas.save(atlas_path)

    preview = Image.new("RGBA", atlas.size, (25, 27, 32, 255))
    draw = ImageDraw.Draw(preview)
    for y in range(0, preview.height, 16):
        for x in range(0, preview.width, 16):
            color = (45, 48, 56, 255) if (x // 16 + y // 16) % 2 == 0 else (34, 37, 44, 255)
            draw.rectangle((x, y, x + 15, y + 15), fill=color)
    preview.alpha_composite(atlas)
    preview.convert("RGB").save(previews_dir / f"{category}-icons-v1-preview.png")

    for record in records:
        image = Image.open(root / record["file"])
        if image.size != (ICON_SIZE, ICON_SIZE) or image.mode != "RGBA":
            errors.append(record["file"])
    if alpha_extrema[0] != 0 or alpha_extrema[1] < 240:
        errors.append(f"Source transparency is insufficient: {alpha_extrema}")

    manifest = {
        "id": f"{category}-customization-icons-v1",
        "title": title,
        "category": category,
        "icon_size": [ICON_SIZE, ICON_SIZE],
        "count": len(records),
        "source_alpha_extrema": list(alpha_extrema),
        "icons": records,
        "atlas": atlas_path.relative_to(root).as_posix(),
    }
    (manifests_dir / f"{category}-icons-v1.json").write_text(
        json.dumps(manifest, ensure_ascii=False, indent=2) + "\n", encoding="utf-8"
    )
    qa = {"status": "pass" if not errors else "fail", "checked": len(records), "errors": errors}
    (manifests_dir / f"{category}-qa-report-v1.json").write_text(
        json.dumps(qa, ensure_ascii=False, indent=2) + "\n", encoding="utf-8"
    )
    return qa


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, required=True)
    parser.add_argument("--category", required=True)
    parser.add_argument("--title", required=True)
    parser.add_argument("--source", required=True)
    args = parser.parse_args()
    print(
        json.dumps(
            build(args.root.resolve(), args.category, args.title, args.source),
            ensure_ascii=False,
            indent=2,
        )
    )


if __name__ == "__main__":
    main()
