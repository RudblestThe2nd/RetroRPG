from __future__ import annotations

import json
from collections import deque
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont


ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT / "source" / "imagegen" / "class-outfits-source-v1.png"
SHEETS = ROOT / "sheets"
SPRITES = ROOT / "sprites"
MANIFESTS = ROOT / "manifests"
PREVIEWS = ROOT / "previews"

CLASSES = (
    "barbarian", "bard", "cleric", "druid", "fighter", "monk",
    "paladin", "ranger", "rogue", "sorcerer", "warlock", "wizard",
)
TURKISH = (
    "Barbar", "Ozan", "Ruhban", "Druid", "Savaşçı", "Keşiş",
    "Paladin", "Korucu", "Haydut", "Büyü Kaynaklı", "Antlaşmalı Büyücü", "Sihirbaz",
)
CELL_W = CELL_H = 256


def color_delta(first: tuple[int, int, int], second: tuple[int, int, int]) -> int:
    return max(abs(first[index] - second[index]) for index in range(3))


def remove_connected_background(image: Image.Image, threshold: int = 7) -> Image.Image:
    rgb = image.convert("RGB")
    width, height = rgb.size
    pixels = rgb.load()
    visited = bytearray(width * height)
    queue: deque[tuple[int, int]] = deque()

    def add(x: int, y: int) -> None:
        index = y * width + x
        if not visited[index]:
            visited[index] = 1
            queue.append((x, y))

    for x in range(width):
        add(x, 0)
        add(x, height - 1)
    for y in range(height):
        add(0, y)
        add(width - 1, y)

    while queue:
        x, y = queue.popleft()
        current = pixels[x, y]
        for nx, ny in ((x - 1, y), (x + 1, y), (x, y - 1), (x, y + 1)):
            if not (0 <= nx < width and 0 <= ny < height):
                continue
            index = ny * width + nx
            if visited[index]:
                continue
            candidate = pixels[nx, ny]
            # Background is a smoothly changing warm brown field. The dark outline
            # and detailed clothing create a steep local color edge that stops fill.
            if color_delta(current, candidate) <= threshold:
                visited[index] = 1
                queue.append((nx, ny))

    rgba = image.convert("RGBA")
    alpha = Image.new("L", (width, height), 255)
    alpha_pixels = alpha.load()
    for y in range(height):
        for x in range(width):
            if visited[y * width + x]:
                alpha_pixels[x, y] = 0
    rgba.putalpha(alpha)
    return rgba


def main() -> None:
    for folder in (SHEETS, SPRITES, MANIFESTS, PREVIEWS):
        folder.mkdir(parents=True, exist_ok=True)

    source = Image.open(SOURCE).convert("RGBA")
    if source.size != (1536, 1024):
        raise ValueError(f"Unexpected source size: {source.size}")

    records = []
    atlas = Image.new("RGBA", source.size, (0, 0, 0, 0))
    entries = []
    for row in range(4):
        gender = "male" if row in (0, 2) else "female"
        class_offset = 0 if row < 2 else 6
        for column in range(6):
            class_index = class_offset + column
            class_id = CLASSES[class_index]
            crop = source.crop((column * CELL_W, row * CELL_H, (column + 1) * CELL_W, (row + 1) * CELL_H))
            sprite = crop
            output_dir = SPRITES / class_id
            output_dir.mkdir(parents=True, exist_ok=True)
            filename = f"{class_id}-{gender}-outfit-baseline-v1.png"
            path = output_dir / filename
            sprite.save(path)
            atlas.alpha_composite(sprite, (column * CELL_W, row * CELL_H))
            relative = path.relative_to(ROOT).as_posix()
            records.append({
                "class": class_id,
                "class_name_tr": TURKISH[class_index],
                "gender": gender,
                "file": relative,
                "source_cell": [column, row],
            })
            entries.append({"id": f"{class_id}-{gender}", "x": column * CELL_W, "y": row * CELL_H,
                            "width": CELL_W, "height": CELL_H})

    atlas.save(SHEETS / "class-outfits-transparent-atlas-v1.png")
    (MANIFESTS / "class-outfits-v1.json").write_text(json.dumps({
        "id": "class-outfits-v1",
        "classes": list(CLASSES),
        "genders": ["male", "female"],
        "sprite_size": [CELL_W, CELL_H],
        "count": len(records),
        "sprites": records,
        "atlas": "sheets/class-outfits-transparent-atlas-v1.png",
        "atlas_entries": entries,
        "note": "Concept baselines for class identity. Runtime animation equipment layers still use anchor profiles.",
    }, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")

    preview = Image.new("RGBA", source.size, (28, 22, 20, 255))
    checker_a, checker_b = (46, 48, 54, 255), (35, 37, 43, 255)
    draw = ImageDraw.Draw(preview)
    for y in range(0, source.height, 32):
        for x in range(0, source.width, 32):
            draw.rectangle((x, y, x + 31, y + 31), fill=checker_a if (x // 32 + y // 32) % 2 == 0 else checker_b)
    preview.alpha_composite(atlas)
    preview.convert("RGB").save(PREVIEWS / "class-outfits-v1-preview.png")

    errors = []
    for record in records:
        image = Image.open(ROOT / record["file"])
        extrema = image.getchannel("A").getextrema()
        if extrema[0] != 0 or extrema[1] < 250:
            errors.append(f"Invalid alpha: {record['file']} {extrema}")
    qa = {"status": "pass" if not errors else "fail", "checked": len(records), "errors": errors}
    (MANIFESTS / "qa-report-v1.json").write_text(
        json.dumps(qa, ensure_ascii=False, indent=2) + "\n", encoding="utf-8"
    )
    print(json.dumps(qa, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
