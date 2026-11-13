from __future__ import annotations

import json
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw, ImageFont


ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / "source" / "imagegen"
TILE_SIZE = 256
HEX_LEFT = 17
HEX_RIGHT = 239
HEX_POINTS = [(128, 0), (239, 64), (239, 192), (128, 255), (17, 192), (17, 64)]


BASE = [
    ("light-meadow", "Açık Çayır"), ("wildflower-meadow", "Çiçekli Çayır"),
    ("grass-shrubs", "Çalılık Çimen"), ("grass-rocks", "Kayalık Çimen"),
    ("sparse-forest", "Seyrek Orman"), ("birch-grove", "Huş Korusu"),
    ("pine-forest", "Çam Ormanı"), ("oak-forest", "Meşe Ormanı"),
    ("dense-forest", "Sık Orman"), ("ancient-forest", "Kadim Orman"),
    ("dark-forest", "Karanlık Orman"), ("autumn-grove", "Sonbahar Korusu"),
    ("mossy-ground", "Yosunlu Zemin"), ("fern-clearing", "Eğrelti Açıklığı"),
    ("mushroom-grove", "Mantar Korusu"), ("ruined-stones", "Yıkık Taşlar"),
    ("standing-stones", "Dikili Taşlar"), ("fallen-tree", "Devrilmiş Ağaç"),
    ("forest-hill", "Orman Tepesi"), ("rocky-hill", "Kayalık Tepe"),
    ("cliff-top", "Uçurum Tepesi"), ("forest-pond", "Orman Göleti"),
    ("marsh-edge", "Bataklık Kenarı"), ("burned-clearing", "Yanmış Açıklık"),
]

CONNECTIONS = [
    ("road-straight-e-w", "Yol Düz E-W"), ("road-straight-ne-sw", "Yol Düz NE-SW"),
    ("road-straight-nw-se", "Yol Düz NW-SE"), ("road-curve-e-se", "Yol Dönüş E-SE"),
    ("road-curve-se-sw", "Yol Dönüş SE-SW"), ("road-curve-sw-w", "Yol Dönüş SW-W"),
    ("road-curve-w-nw", "Yol Dönüş W-NW"), ("road-curve-nw-ne", "Yol Dönüş NW-NE"),
    ("road-curve-ne-e", "Yol Dönüş NE-E"), ("road-three-way-e-sw-nw", "Yol Üçlü E-SW-NW"),
    ("road-three-way-w-ne-se", "Yol Üçlü W-NE-SE"), ("road-six-way", "Yol Altılı Kavşak"),
    ("river-straight-e-w", "Nehir Düz E-W"), ("river-straight-ne-sw", "Nehir Düz NE-SW"),
    ("river-straight-nw-se", "Nehir Düz NW-SE"), ("river-curve-e-se", "Nehir Dönüş E-SE"),
    ("river-curve-se-sw", "Nehir Dönüş SE-SW"), ("river-curve-sw-w", "Nehir Dönüş SW-W"),
    ("river-curve-w-nw", "Nehir Dönüş W-NW"), ("river-curve-nw-ne", "Nehir Dönüş NW-NE"),
    ("river-curve-ne-e", "Nehir Dönüş NE-E"), ("bridge-e-w", "Köprü E-W"),
    ("bridge-ne-sw", "Köprü NE-SW"), ("bridge-nw-se", "Köprü NW-SE"),
]

POI = [
    ("tiny-village", "Küçük Köy"), ("woodland-town", "Orman Kasabası"),
    ("ranger-lodge", "Korucu Kulübesi"), ("hunter-camp", "Avcı Kampı"),
    ("lumber-camp", "Oduncu Kampı"), ("forest-farmstead", "Orman Çiftliği"),
    ("mossy-ruins", "Yosunlu Harabeler"), ("ruined-watchtower", "Yıkık Gözetleme Kulesi"),
    ("ancient-temple", "Kadim Tapınak"), ("roadside-shrine", "Yol Kenarı Mabedi"),
    ("ritual-stones", "Ritüel Taşları"), ("old-graveyard", "Eski Mezarlık"),
    ("cave-entrance", "Mağara Girişi"), ("mine-entrance", "Maden Girişi"),
    ("root-tunnel", "Kök Tüneli"), ("waterfall-grotto", "Şelale Mağarası"),
    ("pond-dock", "Gölet İskelesi"), ("ancient-tree", "Kadim Ağaç"),
    ("bandit-camp", "Haydut Kampı"), ("monster-nest", "Yaratık Yuvası"),
    ("witch-hut", "Cadı Kulübesi"), ("forest-watchtower", "Orman Gözetleme Kulesi"),
    ("magic-circle", "Büyü Çemberi"), ("boss-clearing", "Patron Açıklığı"),
]

CATEGORIES = [
    ("base", "forest-base-source-v1.png", BASE),
    ("connections", "forest-connections-source-v1.png", CONNECTIONS),
    ("poi", "forest-poi-source-v1.png", POI),
]


def crop_cell(image: Image.Image, index: int) -> Image.Image:
    column = index % 6
    row = index // 6
    left = round(column * image.width / 6)
    right = round((column + 1) * image.width / 6)
    top = round(row * image.height / 4)
    bottom = round((row + 1) * image.height / 4)
    return image.crop((left, top, right, bottom)).convert("RGBA")


def standardize_hex(cell: Image.Image) -> Image.Image:
    alpha = np.asarray(cell.getchannel("A"))
    ys, xs = np.where(alpha >= 64)
    if len(xs):
        left, right = int(xs.min()), int(xs.max()) + 1
        top, bottom = int(ys.min()), int(ys.max()) + 1
        cell = cell.crop((left, top, right, bottom))

    # Scale the generated hex into a mathematically consistent pointy-top footprint.
    cell = cell.resize((HEX_RIGHT - HEX_LEFT + 1, TILE_SIZE), Image.Resampling.LANCZOS)
    canvas = Image.new("RGBA", (TILE_SIZE, TILE_SIZE), (0, 0, 0, 0))
    canvas.alpha_composite(cell, (HEX_LEFT, 0))

    mask = Image.new("L", (TILE_SIZE, TILE_SIZE), 0)
    ImageDraw.Draw(mask).polygon(HEX_POINTS, fill=255)
    source_alpha = canvas.getchannel("A")
    source_alpha = source_alpha.point(lambda value: 255 if value >= 32 else 0)
    final_alpha = Image.new("L", (TILE_SIZE, TILE_SIZE), 0)
    final_alpha = Image.fromarray(np.minimum(np.asarray(mask), np.asarray(source_alpha)).astype(np.uint8), "L")
    canvas.putalpha(final_alpha)
    return canvas


def tile_metadata(category: str, tile_id: str, name_tr: str, index: int) -> dict:
    movement_cost = 1
    passable = True
    danger = 1
    if category == "base":
        if tile_id in {"dense-forest", "ancient-forest", "dark-forest", "rocky-hill", "marsh-edge"}:
            movement_cost = 2
        if tile_id == "cliff-top":
            movement_cost = 3
        if tile_id in {"dark-forest", "burned-clearing", "marsh-edge"}:
            danger = 2
    elif category == "connections":
        if tile_id.startswith("road-") or tile_id.startswith("bridge-"):
            movement_cost = 1
        else:
            movement_cost = 3
            passable = False
    else:
        movement_cost = 1 if tile_id in {"tiny-village", "woodland-town", "ranger-lodge", "forest-farmstead"} else 2
        danger = 3 if tile_id in {"bandit-camp", "monster-nest", "boss-clearing"} else 1
    return {
        "id": tile_id,
        "name_tr": name_tr,
        "category": category,
        "movement_cost": movement_cost,
        "passable": passable,
        "danger": danger,
        "index": index,
    }


def build_atlas(category: str, icon_paths: list[Path]) -> tuple[Path, Path]:
    atlas = Image.new("RGBA", (1536, 1024), (0, 0, 0, 0))
    sprites = {}
    for index, path in enumerate(icon_paths):
        image = Image.open(path).convert("RGBA")
        x = (index % 6) * TILE_SIZE
        y = (index // 6) * TILE_SIZE
        atlas.alpha_composite(image, (x, y))
        sprites[path.stem] = {"x": x, "y": y, "width": TILE_SIZE, "height": TILE_SIZE}
    png_path = ROOT / "atlases" / f"forest-{category}-atlas-v1.png"
    json_path = ROOT / "atlases" / f"forest-{category}-atlas-v1.json"
    atlas.save(png_path)
    json_path.write_text(
        json.dumps({"image": png_path.name, "sprites": sprites}, ensure_ascii=False, indent=2),
        encoding="utf-8",
    )
    return png_path, json_path


def find_font(size: int):
    path = Path("C:/Windows/Fonts/consolab.ttf")
    return ImageFont.truetype(path, size) if path.exists() else ImageFont.load_default()


def build_preview(atlas_paths: list[Path]) -> Path:
    canvas = Image.new("RGB", (820, 1740), (17, 20, 15))
    draw = ImageDraw.Draw(canvas)
    draw.text((26, 20), "FOREST HEX TILESET V1", font=find_font(30), fill=(230, 183, 55))
    labels = ("BASE TERRAIN", "ROADS / RIVERS / BRIDGES", "POINTS OF INTEREST")
    y = 76
    for label, atlas_path in zip(labels, atlas_paths):
        draw.text((26, y), label, font=find_font(18), fill=(225, 229, 216))
        y += 30
        atlas = Image.open(atlas_path).convert("RGBA").resize((768, 512), Image.Resampling.LANCZOS)
        canvas.paste(atlas, (26, y), atlas)
        y += 530
    path = ROOT / "previews" / "forest-hex-tileset-v1-preview.png"
    canvas.save(path)
    return path


def build_adjacency_test() -> Path:
    columns = 6
    rows = 4
    horizontal_step = 222
    vertical_step = 192
    odd_offset = 111
    width = (columns - 1) * horizontal_step + odd_offset + TILE_SIZE
    height = (rows - 1) * vertical_step + TILE_SIZE
    canvas = Image.new("RGB", (width, height), (13, 22, 13))
    base_paths = sorted((ROOT / "tiles" / "base").glob("*.png"))
    for index, path in enumerate(base_paths[: columns * rows]):
        row = index // columns
        column = index % columns
        x = column * horizontal_step + (odd_offset if row % 2 else 0)
        y = row * vertical_step
        tile = Image.open(path).convert("RGBA")
        canvas.paste(tile, (x, y), tile)
    path = ROOT / "previews" / "forest-hex-adjacency-test-v1.png"
    canvas.save(path)
    return path


def main() -> None:
    all_records = []
    atlas_paths = []
    errors = []

    for category, source_name, definitions in CATEGORIES:
        source = Image.open(SOURCE / source_name).convert("RGBA")
        output_dir = ROOT / "tiles" / category
        paths = []
        for index, (tile_id, name_tr) in enumerate(definitions):
            tile = standardize_hex(crop_cell(source, index))
            path = output_dir / f"{index + 1:02d}-{tile_id}.png"
            tile.save(path)
            paths.append(path)
            record = tile_metadata(category, tile_id, name_tr, index)
            record["image"] = path.relative_to(ROOT).as_posix()
            all_records.append(record)

            alpha = np.asarray(tile.getchannel("A"))
            if int(alpha.max()) == 0:
                errors.append(f"Blank tile: {path.relative_to(ROOT).as_posix()}")
            for x, y in ((0, 0), (255, 0), (0, 255), (255, 255)):
                if tile.getpixel((x, y))[3] != 0:
                    errors.append(f"Non-transparent corner: {path.relative_to(ROOT).as_posix()}")
        atlas_path, _ = build_atlas(category, paths)
        atlas_paths.append(atlas_path)

    manifest = {
        "pack": "forest-hex-tileset",
        "version": 1,
        "orientation": "pointy-top",
        "tile_canvas": [256, 256],
        "hex_visual_bounds": [17, 0, 239, 255],
        "hex_points": HEX_POINTS,
        "recommended_layout": {
            "hex_width": 222,
            "hex_height": 256,
            "horizontal_step": 222,
            "vertical_step": 192,
            "odd_row_offset_x": 111,
        },
        "tile_count": len(all_records),
        "tiles": all_records,
    }
    (ROOT / "manifests" / "forest-hex-tileset-v1.json").write_text(
        json.dumps(manifest, ensure_ascii=False, indent=2), encoding="utf-8"
    )
    qa = {"status": "pass" if not errors else "fail", "checked_tiles": len(all_records), "errors": errors}
    (ROOT / "manifests" / "qa-report-v1.json").write_text(
        json.dumps(qa, ensure_ascii=False, indent=2), encoding="utf-8"
    )
    preview = build_preview(atlas_paths)
    adjacency = build_adjacency_test()
    print(f"Built {len(all_records)} forest hex tiles")
    print(f"Preview: {preview}")
    print(f"Adjacency test: {adjacency}")
    print(json.dumps(qa, ensure_ascii=False))
    raise SystemExit(1 if errors else 0)


if __name__ == "__main__":
    main()
