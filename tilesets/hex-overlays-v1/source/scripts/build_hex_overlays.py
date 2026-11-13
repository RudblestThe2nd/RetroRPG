from __future__ import annotations

import json
import math
import random
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont


ROOT = Path(__file__).resolve().parents[2]
TILESETS_ROOT = Path(__file__).resolve().parents[3]
STATIC_DIR = ROOT / "static"
ANIMATED_DIR = ROOT / "animated"
ATLASES_DIR = ROOT / "atlases"
MANIFESTS_DIR = ROOT / "manifests"
PREVIEWS_DIR = ROOT / "previews"

SIZE = 256
HEX = [(128, 2), (237, 65), (237, 191), (128, 253), (19, 191), (19, 65)]
CX, CY = 128, 128


def blank() -> Image.Image:
    return Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))


def hex_border(image: Image.Image, color: tuple[int, int, int, int], width: int = 6, inset: int = 0) -> None:
    points = []
    for x, y in HEX:
        vx, vy = x - CX, y - CY
        length = math.hypot(vx, vy)
        points.append((int(x - inset * vx / length), int(y - inset * vy / length)))
    ImageDraw.Draw(image).line(points + [points[0]], fill=color, width=width, joint="curve")


def hex_fill(image: Image.Image, color: tuple[int, int, int, int]) -> None:
    ImageDraw.Draw(image).polygon(HEX, fill=color)


def center_marker(image: Image.Image, color: tuple[int, int, int, int], radius: int = 24) -> None:
    draw = ImageDraw.Draw(image)
    draw.ellipse((CX - radius, CY - radius, CX + radius, CY + radius), outline=color, width=6)
    draw.ellipse((CX - 5, CY - 5, CX + 5, CY + 5), fill=color)


def pixel_exclamation(image: Image.Image, color: tuple[int, int, int, int]) -> None:
    draw = ImageDraw.Draw(image)
    draw.rectangle((119, 78, 137, 132), fill=(20, 16, 10, 220))
    draw.rectangle((123, 82, 133, 128), fill=color)
    draw.rectangle((119, 142, 137, 160), fill=(20, 16, 10, 220))
    draw.rectangle((123, 146, 133, 156), fill=color)


def lock_icon(image: Image.Image, color: tuple[int, int, int, int]) -> None:
    draw = ImageDraw.Draw(image)
    draw.arc((98, 72, 158, 132), 180, 360, fill=color, width=9)
    draw.rectangle((92, 112, 164, 174), fill=(25, 28, 34, 235), outline=color, width=7)
    draw.rectangle((124, 132, 132, 156), fill=color)


def simple_icon(image: Image.Image, kind: str, color: tuple[int, int, int, int]) -> None:
    draw = ImageDraw.Draw(image)
    if kind == "flag":
        draw.rectangle((105, 74, 112, 178), fill=color)
        draw.polygon([(112, 78), (164, 94), (112, 116)], fill=color)
    elif kind == "camp":
        draw.polygon([(88, 169), (128, 92), (168, 169)], outline=color)
        draw.line((128, 92, 128, 169), fill=color, width=5)
        draw.line((88, 169, 168, 169), fill=color, width=5)
    elif kind == "house":
        draw.polygon([(78, 126), (128, 82), (178, 126)], fill=color)
        draw.rectangle((91, 122, 165, 176), fill=color)
        draw.rectangle((121, 143, 139, 176), fill=(24, 25, 28, 230))
    elif kind == "chest":
        draw.rectangle((82, 116, 174, 172), fill=color, outline=(25, 20, 10, 255), width=6)
        draw.arc((82, 76, 174, 142), 180, 360, fill=color, width=18)
        draw.rectangle((123, 128, 137, 151), fill=(25, 20, 10, 255))
    elif kind == "skull":
        draw.ellipse((86, 76, 170, 156), fill=color)
        draw.rectangle((104, 140, 152, 178), fill=color)
        draw.ellipse((105, 105, 122, 126), fill=(30, 15, 36, 255))
        draw.ellipse((134, 105, 151, 126), fill=(30, 15, 36, 255))
    elif kind == "check":
        draw.line((78, 132, 112, 166, 180, 88), fill=(18, 24, 18, 240), width=20)
        draw.line((78, 128, 112, 160, 180, 84), fill=color, width=11)
    elif kind == "burst":
        points = []
        for index in range(16):
            angle = -math.pi / 2 + index * math.pi / 8
            radius = 58 if index % 2 == 0 else 28
            points.append((CX + int(math.cos(angle) * radius), CY + int(math.sin(angle) * radius)))
        draw.polygon(points, fill=color)
        draw.ellipse((112, 112, 144, 144), fill=(45, 24, 12, 230))
    elif kind == "cross":
        draw.line((82, 82, 174, 174), fill=color, width=12)
        draw.line((174, 82, 82, 174), fill=color, width=12)


def fog_overlay(alpha: int, seed: int, dark: bool = False) -> Image.Image:
    image = blank()
    base = (8, 11, 17, alpha) if dark else (94, 110, 126, alpha)
    hex_fill(image, base)
    draw = ImageDraw.Draw(image)
    rng = random.Random(seed)
    for _ in range(55):
        x, y = rng.randrange(22, 235), rng.randrange(15, 241)
        if ((x - CX) / 112) ** 2 + ((y - CY) / 126) ** 2 > 1.15:
            continue
        tone = rng.choice((20, 35, 50, 65))
        draw.rectangle((x, y, x + rng.randrange(2, 7), y + rng.randrange(1, 4)), fill=(150, 165, 180, tone))
    return image


def directional_fog(direction_index: int) -> Image.Image:
    mask_path = TILESETS_ROOT / "transitions-v1" / "tiles" / "masks"
    candidate = next(mask_path.glob(f"{1 << direction_index:02d}-*.png"))
    mask = Image.open(candidate).convert("RGBA").getchannel("A")
    image = Image.new("RGBA", (SIZE, SIZE), (82, 96, 112, 0))
    image.putalpha(mask.point(lambda value: int(value * 0.75)))
    return image


def build_static() -> list[dict]:
    outputs: list[tuple[str, str, Image.Image]] = []
    outputs.extend([
        ("unexplored", "exploration", fog_overlay(246, 1, True)),
        ("fog-of-war", "exploration", fog_overlay(178, 2)),
        ("explored-out-of-sight", "exploration", fog_overlay(112, 3, True)),
        ("recently-revealed", "exploration", fog_overlay(58, 4)),
    ])
    for index, direction in enumerate(("e", "se", "sw", "w", "nw", "ne")):
        outputs.append((f"fog-edge-{direction}", "exploration", directional_fog(index)))

    def state(identifier: str, color: tuple[int, int, int, int], icon: str | None = None,
              fill: tuple[int, int, int, int] | None = None, width: int = 7) -> None:
        image = blank()
        if fill:
            hex_fill(image, fill)
        hex_border(image, color, width)
        if icon == "exclamation":
            pixel_exclamation(image, color)
        elif icon == "lock":
            lock_icon(image, color)
        elif icon == "marker":
            center_marker(image, color)
        elif icon:
            simple_icon(image, icon, color)
        outputs.append((identifier, "state", image))

    state("selected", (255, 210, 62, 255), fill=(255, 196, 32, 28), width=9)
    state("hovered", (245, 248, 255, 235), fill=(255, 255, 255, 20), width=5)
    state("locked", (152, 164, 179, 245), "lock", (25, 30, 38, 125))
    state("danger", (244, 54, 54, 250), "exclamation", (150, 16, 16, 42), 9)
    state("quest-main", (255, 190, 30, 255), "exclamation", (255, 190, 30, 26), 9)
    state("quest-side", (80, 180, 255, 255), "exclamation", (40, 130, 240, 22))
    state("player-location", (70, 235, 255, 255), "marker", (40, 190, 230, 24), 8)
    state("route-start", (92, 230, 110, 255), "marker", (50, 180, 70, 20))
    state("route-step", (80, 205, 255, 240), "marker", (50, 160, 230, 18), 4)
    state("route-end", (255, 215, 65, 255), "flag", (255, 190, 30, 20))
    state("reachable", (75, 220, 105, 230), fill=(40, 190, 70, 42), width=5)
    state("unreachable", (225, 70, 70, 230), "cross", (170, 30, 30, 42), 5)
    state("movement-range", (75, 160, 255, 220), fill=(45, 120, 255, 36), width=4)
    state("enemy-occupied", (245, 55, 55, 250), "cross", (170, 20, 20, 30), 8)
    state("ally-occupied", (80, 225, 130, 250), "marker", (35, 160, 85, 24), 7)
    state("random-encounter", (255, 130, 35, 255), "burst", (210, 70, 20, 22), 8)
    state("camp-available", (255, 175, 65, 255), "camp", (180, 80, 20, 20))
    state("settlement-discovered", (238, 238, 220, 250), "house", (220, 220, 190, 16))
    state("loot-available", (255, 205, 45, 255), "chest", (180, 120, 20, 18))
    state("boss-location", (205, 70, 255, 255), "skull", (120, 30, 160, 30), 10)
    state("objective-complete", (100, 240, 135, 255), "check", (50, 180, 80, 22), 7)
    state("travel-blocked", (175, 70, 70, 250), "lock", (90, 20, 20, 55), 8)

    records = []
    for index, (identifier, category, image) in enumerate(outputs, 1):
        filename = f"{index:02d}-{identifier}.png"
        image.save(STATIC_DIR / filename)
        records.append({"id": identifier, "category": category, "file": f"static/{filename}"})
    return records


def build_animations(records: list[dict]) -> list[dict]:
    animated_ids = ("selected", "quest-main", "danger", "player-location", "route-step")
    results = []
    by_id = {record["id"]: record for record in records}
    for identifier in animated_ids:
        source = Image.open(ROOT / by_id[identifier]["file"]).convert("RGBA")
        frames = []
        for index in range(8):
            factor = 0.55 + 0.45 * (0.5 + 0.5 * math.sin(index / 8 * math.tau))
            frame = source.copy()
            frame.putalpha(source.getchannel("A").point(lambda value, f=factor: int(value * f)))
            frames.append(frame)
        path = ANIMATED_DIR / f"{identifier}-pulse-v1.gif"
        frames[0].save(path, save_all=True, append_images=frames[1:], duration=90, loop=0, disposal=2)
        results.append({"id": f"{identifier}-pulse", "frames": 8, "file": f"animated/{path.name}"})

    frames = []
    for index in range(8):
        frames.append(fog_overlay(165, 100 + index))
    path = ANIMATED_DIR / "fog-drift-v1.gif"
    frames[0].save(path, save_all=True, append_images=frames[1:], duration=140, loop=0, disposal=2)
    results.append({"id": "fog-drift", "frames": 8, "file": f"animated/{path.name}"})
    return results


def build_atlas_and_preview(records: list[dict]) -> None:
    images = [Image.open(ROOT / record["file"]).convert("RGBA") for record in records]
    atlas = blank().resize((SIZE * 8, SIZE * 4))
    for index, image in enumerate(images):
        atlas.alpha_composite(image, ((index % 8) * SIZE, (index // 8) * SIZE))
    atlas.save(ATLASES_DIR / "hex-overlays-static-atlas-v1.png")

    cell_w, cell_h = 290, 300
    canvas = Image.new("RGBA", (cell_w * 8, 70 + cell_h * 4), (11, 13, 17, 255))
    draw = ImageDraw.Draw(canvas)
    font = ImageFont.load_default()
    draw.text((20, 20), "HEX EXPLORATION AND STATE OVERLAYS V1", fill=(255, 211, 70), font=font)
    sample_tile = Image.open(TILESETS_ROOT / "plains-v1" / "tiles" / "base" / sorted(
        path.name for path in (TILESETS_ROOT / "plains-v1" / "tiles" / "base").glob("*.png")
    )[0]).convert("RGBA")
    for index, (image, record) in enumerate(zip(images, records)):
        x, y = (index % 8) * cell_w + 17, 60 + (index // 8) * cell_h
        merged = sample_tile.copy()
        merged.alpha_composite(image)
        canvas.alpha_composite(merged, (x, y))
        draw.text((x, y + SIZE + 8), record["id"], fill=(230, 234, 240), font=font)
    canvas.convert("RGB").save(PREVIEWS_DIR / "hex-overlays-v1-preview.png")


def main() -> None:
    for folder in (STATIC_DIR, ANIMATED_DIR, ATLASES_DIR, MANIFESTS_DIR, PREVIEWS_DIR):
        folder.mkdir(parents=True, exist_ok=True)
    records = build_static()
    animations = build_animations(records)
    build_atlas_and_preview(records)
    manifest = {
        "id": "hex-overlays-v1",
        "tile_size": [SIZE, SIZE],
        "orientation": "pointy-top",
        "static_count": len(records),
        "animation_count": len(animations),
        "static": records,
        "animated": animations,
        "draw_order": ["terrain", "exploration", "route", "state", "player"],
    }
    (MANIFESTS_DIR / "hex-overlays-v1.json").write_text(
        json.dumps(manifest, ensure_ascii=False, indent=2) + "\n", encoding="utf-8"
    )
    qa = {
        "status": "pass",
        "static_count": len(list(STATIC_DIR.glob("*.png"))),
        "animation_count": len(list(ANIMATED_DIR.glob("*.gif"))),
        "expected_static": 32,
        "expected_animations": 6,
        "errors": [],
    }
    if qa["static_count"] != qa["expected_static"] or qa["animation_count"] != qa["expected_animations"]:
        qa["status"] = "fail"
        qa["errors"].append("Unexpected output count")
    (MANIFESTS_DIR / "qa-report-v1.json").write_text(
        json.dumps(qa, ensure_ascii=False, indent=2) + "\n", encoding="utf-8"
    )
    print(json.dumps(qa, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
