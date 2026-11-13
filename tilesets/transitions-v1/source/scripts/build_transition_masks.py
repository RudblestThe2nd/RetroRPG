from __future__ import annotations

import json
import math
from itertools import combinations
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont


ROOT = Path(__file__).resolve().parents[2]
TILESETS_ROOT = Path(__file__).resolve().parents[3]
MASKS_DIR = ROOT / "tiles" / "masks"
SAMPLES_DIR = ROOT / "samples"
ATLASES_DIR = ROOT / "atlases"
MANIFESTS_DIR = ROOT / "manifests"
PREVIEWS_DIR = ROOT / "previews"

SIZE = 256
HEX_POINTS = [(128, 0), (239, 64), (239, 192), (128, 255), (17, 192), (17, 64)]
DIRECTIONS = ("e", "se", "sw", "w", "nw", "ne")
SIDE_SEGMENTS = tuple(zip(HEX_POINTS, HEX_POINTS[1:] + HEX_POINTS[:1]))
BIOMES = (
    "forest", "swamp", "desert", "snow", "mountain",
    "tropical", "volcanic", "corrupted", "underground", "plains",
)


def point_in_polygon(x: float, y: float) -> bool:
    inside = False
    j = len(HEX_POINTS) - 1
    for i, (xi, yi) in enumerate(HEX_POINTS):
        xj, yj = HEX_POINTS[j]
        if ((yi > y) != (yj > y)) and x < (xj - xi) * (y - yi) / (yj - yi) + xi:
            inside = not inside
        j = i
    return inside


def segment_distance(px: float, py: float, a: tuple[int, int], b: tuple[int, int]) -> float:
    ax, ay = a
    bx, by = b
    dx, dy = bx - ax, by - ay
    length_sq = dx * dx + dy * dy
    t = max(0.0, min(1.0, ((px - ax) * dx + (py - ay) * dy) / length_sq))
    qx, qy = ax + t * dx, ay + t * dy
    return math.hypot(px - qx, py - qy)


def smoothstep(value: float) -> float:
    value = max(0.0, min(1.0, value))
    return value * value * (3.0 - 2.0 * value)


def build_mask(bits: int) -> Image.Image:
    alpha = Image.new("L", (SIZE, SIZE), 0)
    pixels = alpha.load()
    if bits == 0:
        return Image.new("RGBA", (SIZE, SIZE), (255, 255, 255, 0))

    for y in range(SIZE):
        for x in range(SIZE):
            if not point_in_polygon(x + 0.5, y + 0.5):
                continue
            if bits == 63:
                pixels[x, y] = 255
                continue
            influence = 0.0
            for side, segment in enumerate(SIDE_SEGMENTS):
                if bits & (1 << side):
                    distance = segment_distance(x + 0.5, y + 0.5, *segment)
                    edge_value = 1.0 - smoothstep((distance - 14.0) / 58.0)
                    influence = max(influence, edge_value)
            # Pixel-art friendly alpha stepping prevents blurry shader-like edges.
            pixels[x, y] = int(round(influence * 7.0) / 7.0 * 255)

    rgba = Image.new("RGBA", (SIZE, SIZE), (255, 255, 255, 0))
    rgba.putalpha(alpha)
    return rgba


def load_base_tile(biome: str) -> Image.Image:
    folder = TILESETS_ROOT / f"{biome}-v1" / "tiles" / "base"
    candidates = sorted(folder.glob("*.png"))
    if not candidates:
        raise FileNotFoundError(f"No base tile for {biome}: {folder}")
    return Image.open(candidates[0]).convert("RGBA")


def make_preview(images: list[Image.Image], labels: list[str], path: Path, columns: int, title: str) -> None:
    cell_w, cell_h = 290, 300
    rows = math.ceil(len(images) / columns)
    canvas = Image.new("RGB", (columns * cell_w, 70 + rows * cell_h), (10, 12, 15))
    draw = ImageDraw.Draw(canvas)
    font = ImageFont.load_default()
    draw.text((20, 20), title, fill=(118, 218, 255), font=font)
    for index, (image, label) in enumerate(zip(images, labels)):
        col, row = index % columns, index // columns
        x = col * cell_w + 17
        y = 60 + row * cell_h
        checker = Image.new("RGBA", (SIZE, SIZE), (30, 34, 40, 255))
        checker.alpha_composite(image)
        canvas.paste(checker.convert("RGB"), (x, y))
        draw.text((x, y + SIZE + 8), label, fill=(230, 234, 239), font=font)
    path.parent.mkdir(parents=True, exist_ok=True)
    canvas.save(path)


def main() -> None:
    for folder in (MASKS_DIR, SAMPLES_DIR, ATLASES_DIR, MANIFESTS_DIR, PREVIEWS_DIR):
        folder.mkdir(parents=True, exist_ok=True)

    records = []
    masks = []
    for bits in range(64):
        selected = [direction for index, direction in enumerate(DIRECTIONS) if bits & (1 << index)]
        slug = "none" if not selected else "-".join(selected)
        filename = f"{bits:02d}-transition-{slug}.png"
        image = build_mask(bits)
        image.save(MASKS_DIR / filename)
        masks.append(image)
        records.append({
            "id": f"transition-{bits:02d}",
            "bits": bits,
            "directions": selected,
            "file": f"tiles/masks/{filename}",
        })

    atlas = Image.new("RGBA", (SIZE * 8, SIZE * 8), (0, 0, 0, 0))
    for index, image in enumerate(masks):
        atlas.alpha_composite(image, ((index % 8) * SIZE, (index // 8) * SIZE))
    atlas.save(ATLASES_DIR / "biome-transition-mask-atlas-v1.png")

    manifest = {
        "id": "biome-transition-masks-v1",
        "tile_size": [SIZE, SIZE],
        "orientation": "pointy-top",
        "direction_bit_order": list(DIRECTIONS),
        "runtime_rule": "Blend target biome over source biome using the selected mask alpha.",
        "mask_count": len(records),
        "masks": records,
    }
    (MANIFESTS_DIR / "biome-transition-masks-v1.json").write_text(
        json.dumps(manifest, ensure_ascii=False, indent=2) + "\n", encoding="utf-8"
    )

    mask_preview_images = []
    mask_preview_labels = []
    for index, image in enumerate(masks):
        display = Image.new("RGBA", (SIZE, SIZE), (25, 28, 34, 255))
        tint = Image.new("RGBA", (SIZE, SIZE), (90, 210, 255, 0))
        tint.putalpha(image.getchannel("A"))
        display.alpha_composite(tint)
        mask_preview_images.append(display)
        dirs = records[index]["directions"]
        mask_preview_labels.append(f"{index:02d}: {','.join(dirs) if dirs else 'none'}")
    make_preview(
        mask_preview_images, mask_preview_labels,
        PREVIEWS_DIR / "biome-transition-masks-v1-preview.png", 8,
        "BIOME TRANSITION MASKS V1 — ALL 64 NEIGHBOR COMBINATIONS",
    )

    pair_images = []
    pair_labels = []
    blend_mask = masks[35].getchannel("A")  # E + SE + NE
    for first, second in combinations(BIOMES, 2):
        source = load_base_tile(first)
        target = load_base_tile(second)
        mixed = Image.composite(target, source, blend_mask)
        hex_alpha = Image.new("L", (SIZE, SIZE), 0)
        ImageDraw.Draw(hex_alpha).polygon(HEX_POINTS, fill=255)
        mixed.putalpha(hex_alpha)
        filename = f"{first}-to-{second}-sample.png"
        mixed.save(SAMPLES_DIR / filename)
        pair_images.append(mixed)
        pair_labels.append(f"{first} -> {second}")
    make_preview(
        pair_images, pair_labels,
        PREVIEWS_DIR / "biome-transition-pairs-v1-preview.png", 5,
        "BIOME TRANSITION PAIR SAMPLES — 45 UNIQUE PAIRS",
    )

    qa = {
        "status": "pass",
        "mask_count": len(list(MASKS_DIR.glob("*.png"))),
        "sample_pair_count": len(list(SAMPLES_DIR.glob("*.png"))),
        "expected_masks": 64,
        "expected_pairs": 45,
        "errors": [],
    }
    if qa["mask_count"] != 64 or qa["sample_pair_count"] != 45:
        qa["status"] = "fail"
        qa["errors"].append("Unexpected output count")
    (MANIFESTS_DIR / "qa-report-v1.json").write_text(
        json.dumps(qa, ensure_ascii=False, indent=2) + "\n", encoding="utf-8"
    )
    print(json.dumps(qa, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
