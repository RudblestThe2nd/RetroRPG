from __future__ import annotations

import argparse
import json
from pathlib import Path

from PIL import Image, ImageDraw


CELL = 256


def build(root: Path, race: str, title: str, source_name: str) -> dict:
    source_path = root / "source" / "imagegen" / source_name
    portraits_dir = root / "portraits"
    atlases_dir = root / "atlases"
    manifests_dir = root / "manifests"
    previews_dir = root / "previews"
    for folder in (portraits_dir, atlases_dir, manifests_dir, previews_dir):
        folder.mkdir(parents=True, exist_ok=True)

    source = Image.open(source_path).convert("RGBA")
    if source.size != (1536, 1024):
        raise ValueError(f"Unexpected source size: {source.size}")
    alpha_extrema = source.getchannel("A").getextrema()

    atlas = Image.new("RGBA", source.size, (0, 0, 0, 0))
    records = []
    counters = {"male": 0, "female": 0}
    for row in range(4):
        gender = "male" if row < 2 else "female"
        for column in range(6):
            counters[gender] += 1
            variant = counters[gender]
            portrait = source.crop((column * CELL, row * CELL, (column + 1) * CELL, (row + 1) * CELL))
            folder = portraits_dir / gender
            folder.mkdir(parents=True, exist_ok=True)
            filename = f"{race}-{gender}-face-{variant:02d}-v1.png"
            path = folder / filename
            portrait.save(path)
            atlas.alpha_composite(portrait, (column * CELL, row * CELL))
            records.append({
                "id": f"{race}-{gender}-face-{variant:02d}",
                "race": race,
                "gender": gender,
                "variant": variant,
                "file": path.relative_to(root).as_posix(),
                "source_cell": [column, row],
            })

    atlas_path = atlases_dir / f"{race}-face-variants-transparent-atlas-v1.png"
    atlas.save(atlas_path)
    manifest = {
        "id": f"{race}-face-variants-v1",
        "title": title,
        "race": race,
        "portrait_size": [CELL, CELL],
        "male_count": counters["male"],
        "female_count": counters["female"],
        "source_alpha_extrema": list(alpha_extrema),
        "portraits": records,
        "atlas": atlas_path.relative_to(root).as_posix(),
    }
    (manifests_dir / f"{race}-face-variants-v1.json").write_text(
        json.dumps(manifest, ensure_ascii=False, indent=2) + "\n", encoding="utf-8"
    )

    preview = Image.new("RGBA", source.size, (25, 27, 32, 255))
    draw = ImageDraw.Draw(preview)
    for y in range(0, source.height, 32):
        for x in range(0, source.width, 32):
            color = (45, 48, 56, 255) if (x // 32 + y // 32) % 2 == 0 else (34, 37, 44, 255)
            draw.rectangle((x, y, x + 31, y + 31), fill=color)
    preview.alpha_composite(atlas)
    preview.convert("RGB").save(previews_dir / f"{race}-face-variants-v1-preview.png")

    errors = []
    for record in records:
        image = Image.open(root / record["file"])
        if image.size != (CELL, CELL) or image.mode != "RGBA":
            errors.append(record["file"])
    if alpha_extrema[0] != 0 or alpha_extrema[1] < 240:
        errors.append(f"Source transparency is insufficient: {alpha_extrema}")
    qa = {"status": "pass" if not errors else "fail", "checked": len(records), "errors": errors}
    (manifests_dir / "qa-report-v1.json").write_text(
        json.dumps(qa, ensure_ascii=False, indent=2) + "\n", encoding="utf-8"
    )
    return qa


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, required=True)
    parser.add_argument("--race", required=True)
    parser.add_argument("--title", required=True)
    parser.add_argument("--source", required=True)
    args = parser.parse_args()
    print(json.dumps(build(args.root.resolve(), args.race, args.title, args.source), ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
