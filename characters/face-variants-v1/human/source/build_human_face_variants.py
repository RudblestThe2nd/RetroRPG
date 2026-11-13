from __future__ import annotations

import json
from collections import deque
from pathlib import Path

from PIL import Image, ImageDraw


ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT / "source" / "imagegen" / "human-face-variants-source-v1.png"
PORTRAITS = ROOT / "portraits"
ATLASES = ROOT / "atlases"
MANIFESTS = ROOT / "manifests"
PREVIEWS = ROOT / "previews"
CELL = 256


def delta(first: tuple[int, int, int], second: tuple[int, int, int]) -> int:
    return max(abs(first[index] - second[index]) for index in range(3))


def remove_background(image: Image.Image, threshold: int = 7) -> Image.Image:
    rgb = image.convert("RGB")
    pixels = rgb.load()
    width, height = rgb.size
    visited = bytearray(width * height)
    queue: deque[tuple[int, int]] = deque()

    def add(x: int, y: int) -> None:
        offset = y * width + x
        if not visited[offset]:
            visited[offset] = 1
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
            offset = ny * width + nx
            if not visited[offset] and delta(current, pixels[nx, ny]) <= threshold:
                visited[offset] = 1
                queue.append((nx, ny))
    result = image.convert("RGBA")
    alpha = Image.new("L", (width, height), 255)
    alpha_pixels = alpha.load()
    for y in range(height):
        for x in range(width):
            if visited[y * width + x]:
                alpha_pixels[x, y] = 0
    result.putalpha(alpha)
    return result


def main() -> None:
    for folder in (PORTRAITS, ATLASES, MANIFESTS, PREVIEWS):
        folder.mkdir(parents=True, exist_ok=True)
    source = Image.open(SOURCE).convert("RGBA")
    if source.size != (1536, 1024):
        raise ValueError(f"Unexpected source size: {source.size}")

    atlas = Image.new("RGBA", source.size, (0, 0, 0, 0))
    records = []
    counters = {"male": 0, "female": 0}
    for row in range(4):
        gender = "male" if row < 2 else "female"
        for column in range(6):
            counters[gender] += 1
            variant = counters[gender]
            crop = source.crop((column * CELL, row * CELL, (column + 1) * CELL, (row + 1) * CELL))
            # Portrait cards are displayed inside framed character-creation UI.
            # Preserve the complete source pixels instead of risking hair loss
            # during background extraction (especially black and dark-brown hair).
            portrait = crop
            folder = PORTRAITS / gender
            folder.mkdir(parents=True, exist_ok=True)
            filename = f"human-{gender}-face-{variant:02d}-v1.png"
            path = folder / filename
            portrait.save(path)
            atlas.alpha_composite(portrait, (column * CELL, row * CELL))
            records.append({
                "id": f"human-{gender}-face-{variant:02d}",
                "race": "human",
                "gender": gender,
                "variant": variant,
                "file": path.relative_to(ROOT).as_posix(),
                "source_cell": [column, row],
            })

    atlas.save(ATLASES / "human-face-variants-transparent-atlas-v1.png")
    (MANIFESTS / "human-face-variants-v1.json").write_text(json.dumps({
        "id": "human-face-variants-v1",
        "race": "human",
        "portrait_size": [CELL, CELL],
        "male_count": counters["male"],
        "female_count": counters["female"],
        "portraits": records,
        "atlas": "atlases/human-face-variants-transparent-atlas-v1.png",
    }, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")

    preview = Image.new("RGBA", source.size, (25, 27, 32, 255))
    draw = ImageDraw.Draw(preview)
    for y in range(0, source.height, 32):
        for x in range(0, source.width, 32):
            color = (45, 48, 56, 255) if (x // 32 + y // 32) % 2 == 0 else (34, 37, 44, 255)
            draw.rectangle((x, y, x + 31, y + 31), fill=color)
    preview.alpha_composite(atlas)
    preview.convert("RGB").save(PREVIEWS / "human-face-variants-v1-preview.png")

    errors = []
    for record in records:
        image = Image.open(ROOT / record["file"])
        if image.size != (CELL, CELL) or image.mode != "RGBA":
            errors.append(record["file"])
    qa = {"status": "pass" if not errors else "fail", "checked": len(records), "errors": errors}
    (MANIFESTS / "qa-report-v1.json").write_text(
        json.dumps(qa, ensure_ascii=False, indent=2) + "\n", encoding="utf-8"
    )
    print(json.dumps(qa, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
