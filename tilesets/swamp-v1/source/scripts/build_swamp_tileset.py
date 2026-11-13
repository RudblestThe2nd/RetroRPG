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
    ("muddy-clearing", "Çamurlu Açıklık"), ("wet-grass", "Islak Çimen"),
    ("shallow-swamp-water", "Sığ Bataklık Suyu"), ("deep-dark-water", "Derin Karanlık Su"),
    ("lily-pad-pond", "Nilüfer Göleti"), ("dense-reed-marsh", "Sık Sazlık"),
    ("mangrove-roots", "Mangrov Kökleri"), ("cypress-grove", "Servi Korusu"),
    ("dead-tree-marsh", "Ölü Ağaç Bataklığı"), ("dense-swamp-forest", "Sık Bataklık Ormanı"),
    ("misty-mire", "Sisli Bataklık"), ("poisoned-green-bog", "Zehirli Yeşil Bataklık"),
    ("peat-moss-ground", "Turba Yosunu"), ("giant-fungus-bog", "Dev Mantar Bataklığı"),
    ("firefly-marsh", "Ateşböceği Bataklığı"), ("bubbling-mud-pools", "Kaynayan Çamur Havuzları"),
    ("quicksand-mire", "Balçık Tuzağı"), ("flooded-forest", "Su Basan Orman"),
    ("swamp-hill", "Bataklık Tepesi"), ("rocky-islet", "Kayalık Adacık"),
    ("dark-sinkhole", "Karanlık Düden"), ("sulfur-springs", "Kükürt Kaynakları"),
    ("forest-swamp-edge", "Orman Bataklık Geçişi"), ("burned-marsh", "Yanmış Bataklık"),
]

CONNECTIONS = [
    ("trail-straight-e-w", "Patika Düz E-W"), ("trail-straight-ne-sw", "Patika Düz NE-SW"),
    ("trail-straight-nw-se", "Patika Düz NW-SE"), ("trail-curve-e-se", "Patika Dönüş E-SE"),
    ("trail-curve-se-sw", "Patika Dönüş SE-SW"), ("trail-curve-sw-w", "Patika Dönüş SW-W"),
    ("trail-curve-w-nw", "Patika Dönüş W-NW"), ("trail-curve-nw-ne", "Patika Dönüş NW-NE"),
    ("trail-curve-ne-e", "Patika Dönüş NE-E"), ("trail-three-way-e-sw-nw", "Patika Üçlü E-SW-NW"),
    ("trail-three-way-w-ne-se", "Patika Üçlü W-NE-SE"), ("trail-six-way", "Patika Altılı Kavşak"),
    ("channel-straight-e-w", "Kanal Düz E-W"), ("channel-straight-ne-sw", "Kanal Düz NE-SW"),
    ("channel-straight-nw-se", "Kanal Düz NW-SE"), ("channel-curve-e-se", "Kanal Dönüş E-SE"),
    ("channel-curve-se-sw", "Kanal Dönüş SE-SW"), ("channel-curve-sw-w", "Kanal Dönüş SW-W"),
    ("channel-curve-w-nw", "Kanal Dönüş W-NW"), ("channel-curve-nw-ne", "Kanal Dönüş NW-NE"),
    ("channel-curve-ne-e", "Kanal Dönüş NE-E"), ("boardwalk-e-w", "Ahşap Geçit E-W"),
    ("boardwalk-ne-sw", "Ahşap Geçit NE-SW"), ("boardwalk-nw-se", "Ahşap Geçit NW-SE"),
]

POI = [
    ("stilt-village", "Kazık Köyü"), ("swamp-trading-town", "Bataklık Ticaret Kasabası"),
    ("ferryman-hut", "Kayıkçı Kulübesi"), ("herbalist-hut", "Şifacı Kulübesi"),
    ("fishing-camp", "Balıkçı Kampı"), ("reed-farmstead", "Sazlık Çiftliği"),
    ("sunken-ruins", "Batık Harabeler"), ("drowned-watchtower", "Suya Gömülmüş Kule"),
    ("serpent-temple", "Yılan Tapınağı"), ("bog-shrine", "Bataklık Mabedi"),
    ("ritual-stone-circle", "Ritüel Taş Çemberi"), ("flooded-graveyard", "Su Basan Mezarlık"),
    ("cave-entrance", "Mağara Girişi"), ("abandoned-peat-mine", "Terk Edilmiş Turba Madeni"),
    ("giant-root-hollow", "Dev Kök Oyuğu"), ("hidden-swamp-grotto", "Gizli Bataklık Mağarası"),
    ("wooden-dock", "Ahşap İskele"), ("ancient-cypress", "Kadim Servi Ağacı"),
    ("bandit-raft-camp", "Haydut Sal Kampı"), ("hydra-nest", "Hidra Yuvası"),
    ("witch-hut", "Cadı Kulübesi"), ("raised-watchtower", "Yükseltilmiş Gözetleme Kulesi"),
    ("poison-altar", "Zehir Sunağı"), ("boss-island", "Patron Adası"),
]

CATEGORIES = [
    ("base", "swamp-base-source-v1.png", BASE),
    ("connections", "swamp-connections-source-v1.png", CONNECTIONS),
    ("poi", "swamp-poi-source-v1.png", POI),
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

    cell = cell.resize((HEX_RIGHT - HEX_LEFT + 1, TILE_SIZE), Image.Resampling.LANCZOS)
    canvas = Image.new("RGBA", (TILE_SIZE, TILE_SIZE), (0, 0, 0, 0))
    canvas.alpha_composite(cell, (HEX_LEFT, 0))

    mask = Image.new("L", (TILE_SIZE, TILE_SIZE), 0)
    ImageDraw.Draw(mask).polygon(HEX_POINTS, fill=255)
    source_alpha = canvas.getchannel("A").point(lambda value: 255 if value >= 32 else 0)
    final_alpha = Image.fromarray(
        np.minimum(np.asarray(mask), np.asarray(source_alpha)).astype(np.uint8), "L"
    )
    canvas.putalpha(final_alpha)
    return canvas


def tile_metadata(category: str, tile_id: str, name_tr: str, index: int) -> dict:
    movement_cost = 2
    passable = True
    danger = 2
    requires = None

    if category == "base":
        if tile_id in {"muddy-clearing", "wet-grass", "peat-moss-ground", "forest-swamp-edge"}:
            movement_cost = 1
            danger = 1
        if tile_id in {"dense-reed-marsh", "mangrove-roots", "dense-swamp-forest", "flooded-forest"}:
            movement_cost = 3
        if tile_id in {"deep-dark-water", "dark-sinkhole", "quicksand-mire"}:
            passable = False
            movement_cost = 99
        if tile_id in {"shallow-swamp-water", "lily-pad-pond"}:
            requires = "wading_or_boat"
        if tile_id in {"poisoned-green-bog", "sulfur-springs", "burned-marsh"}:
            danger = 3
    elif category == "connections":
        if tile_id.startswith("trail-") or tile_id.startswith("boardwalk-"):
            movement_cost = 1
            danger = 1
        else:
            movement_cost = 3
            passable = False
            requires = "boat"
    else:
        movement_cost = 1 if tile_id in {
            "stilt-village", "swamp-trading-town", "ferryman-hut", "herbalist-hut", "reed-farmstead"
        } else 2
        danger = 3 if tile_id in {
            "hydra-nest", "witch-hut", "poison-altar", "boss-island", "serpent-temple"
        } else 1

    return {
        "id": tile_id,
        "name_tr": name_tr,
        "category": category,
        "movement_cost": movement_cost,
        "passable": passable,
        "danger": danger,
        "requires": requires,
        "index": index,
    }


def build_atlas(category: str, tile_paths: list[Path]) -> tuple[Path, Path]:
    atlas = Image.new("RGBA", (1536, 1024), (0, 0, 0, 0))
    sprites = {}
    for index, path in enumerate(tile_paths):
        image = Image.open(path).convert("RGBA")
        x = (index % 6) * TILE_SIZE
        y = (index // 6) * TILE_SIZE
        atlas.alpha_composite(image, (x, y))
        sprites[path.stem] = {"x": x, "y": y, "width": TILE_SIZE, "height": TILE_SIZE}
    png_path = ROOT / "atlases" / f"swamp-{category}-atlas-v1.png"
    json_path = ROOT / "atlases" / f"swamp-{category}-atlas-v1.json"
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
    canvas = Image.new("RGB", (820, 1740), (14, 20, 14))
    draw = ImageDraw.Draw(canvas)
    draw.text((26, 20), "SWAMP HEX TILESET V1", font=find_font(30), fill=(192, 207, 75))
    labels = ("BASE TERRAIN", "TRAILS / CHANNELS / BOARDWALKS", "POINTS OF INTEREST")
    y = 76
    for label, atlas_path in zip(labels, atlas_paths):
        draw.text((26, y), label, font=find_font(18), fill=(222, 228, 204))
        y += 30
        atlas = Image.open(atlas_path).convert("RGBA").resize((768, 512), Image.Resampling.LANCZOS)
        canvas.paste(atlas, (26, y), atlas)
        y += 530
    path = ROOT / "previews" / "swamp-hex-tileset-v1-preview.png"
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
    canvas = Image.new("RGB", (width, height), (10, 17, 11))
    base_paths = sorted((ROOT / "tiles" / "base").glob("*.png"))
    for index, path in enumerate(base_paths[: columns * rows]):
        row = index // columns
        column = index % columns
        x = column * horizontal_step + (odd_offset if row % 2 else 0)
        y = row * vertical_step
        tile = Image.open(path).convert("RGBA")
        canvas.paste(tile, (x, y), tile)
    path = ROOT / "previews" / "swamp-hex-adjacency-test-v1.png"
    canvas.save(path)
    return path


def main() -> None:
    all_records = []
    atlas_paths = []
    errors = []

    for category, source_name, definitions in CATEGORIES:
        source = Image.open(SOURCE / source_name).convert("RGBA")
        output_dir = ROOT / "tiles" / category
        tile_paths = []
        for index, (tile_id, name_tr) in enumerate(definitions):
            tile = standardize_hex(crop_cell(source, index))
            path = output_dir / f"{index + 1:02d}-{tile_id}.png"
            tile.save(path)
            tile_paths.append(path)

            record = tile_metadata(category, tile_id, name_tr, index)
            record["image"] = path.relative_to(ROOT).as_posix()
            all_records.append(record)

            alpha = np.asarray(tile.getchannel("A"))
            if int(alpha.max()) == 0:
                errors.append(f"Blank tile: {path.relative_to(ROOT).as_posix()}")
            for x, y in ((0, 0), (255, 0), (0, 255), (255, 255)):
                if tile.getpixel((x, y))[3] != 0:
                    errors.append(f"Non-transparent corner: {path.relative_to(ROOT).as_posix()}")

        atlas_path, _ = build_atlas(category, tile_paths)
        atlas_paths.append(atlas_path)

    manifest = {
        "pack": "swamp-hex-tileset",
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
    (ROOT / "manifests" / "swamp-hex-tileset-v1.json").write_text(
        json.dumps(manifest, ensure_ascii=False, indent=2), encoding="utf-8"
    )
    qa = {"status": "pass" if not errors else "fail", "checked_tiles": len(all_records), "errors": errors}
    (ROOT / "manifests" / "qa-report-v1.json").write_text(
        json.dumps(qa, ensure_ascii=False, indent=2), encoding="utf-8"
    )
    preview = build_preview(atlas_paths)
    adjacency = build_adjacency_test()
    print(f"Built {len(all_records)} swamp hex tiles")
    print(f"Preview: {preview}")
    print(f"Adjacency test: {adjacency}")
    print(json.dumps(qa, ensure_ascii=False))
    raise SystemExit(1 if errors else 0)


if __name__ == "__main__":
    main()
