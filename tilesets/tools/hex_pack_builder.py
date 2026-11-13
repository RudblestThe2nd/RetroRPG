from __future__ import annotations

import json
from pathlib import Path
from typing import Callable

import numpy as np
from PIL import Image, ImageDraw, ImageFont


TILE_SIZE = 256
HEX_LEFT = 17
HEX_RIGHT = 239
HEX_POINTS = [(128, 0), (239, 64), (239, 192), (128, 255), (17, 192), (17, 64)]


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
        cell = cell.crop((int(xs.min()), int(ys.min()), int(xs.max()) + 1, int(ys.max()) + 1))

    cell = cell.resize((HEX_RIGHT - HEX_LEFT + 1, TILE_SIZE), Image.Resampling.LANCZOS)
    canvas = Image.new("RGBA", (TILE_SIZE, TILE_SIZE), (0, 0, 0, 0))
    canvas.alpha_composite(cell, (HEX_LEFT, 0))
    mask = Image.new("L", (TILE_SIZE, TILE_SIZE), 0)
    ImageDraw.Draw(mask).polygon(HEX_POINTS, fill=255)
    source_alpha = canvas.getchannel("A").point(lambda value: 255 if value >= 32 else 0)
    canvas.putalpha(
        Image.fromarray(np.minimum(np.asarray(mask), np.asarray(source_alpha)).astype(np.uint8), "L")
    )
    return canvas


def find_font(size: int):
    path = Path("C:/Windows/Fonts/consolab.ttf")
    return ImageFont.truetype(path, size) if path.exists() else ImageFont.load_default()


def build_hex_pack(
    *,
    root: Path,
    pack_slug: str,
    pack_title: str,
    categories: list[tuple[str, str, list[tuple[str, str]]]],
    metadata_fn: Callable[[str, str, str, int], dict],
    preview_labels: tuple[str, str, str],
    preview_background: tuple[int, int, int],
    preview_accent: tuple[int, int, int],
) -> dict:
    source_dir = root / "source" / "imagegen"
    all_records = []
    atlas_paths = []
    errors = []

    for category, source_name, definitions in categories:
        source = Image.open(source_dir / source_name).convert("RGBA")
        output_dir = root / "tiles" / category
        output_dir.mkdir(parents=True, exist_ok=True)
        tile_paths = []

        for index, (tile_id, name_tr) in enumerate(definitions):
            tile = standardize_hex(crop_cell(source, index))
            path = output_dir / f"{index + 1:02d}-{tile_id}.png"
            tile.save(path)
            tile_paths.append(path)

            record = metadata_fn(category, tile_id, name_tr, index)
            record["image"] = path.relative_to(root).as_posix()
            all_records.append(record)

            alpha = np.asarray(tile.getchannel("A"))
            if int(alpha.max()) == 0:
                errors.append(f"Blank tile: {path.relative_to(root).as_posix()}")
            for x, y in ((0, 0), (255, 0), (0, 255), (255, 255)):
                if tile.getpixel((x, y))[3] != 0:
                    errors.append(f"Non-transparent corner: {path.relative_to(root).as_posix()}")

        atlas = Image.new("RGBA", (1536, 1024), (0, 0, 0, 0))
        sprites = {}
        for index, path in enumerate(tile_paths):
            image = Image.open(path).convert("RGBA")
            x = (index % 6) * TILE_SIZE
            y = (index // 6) * TILE_SIZE
            atlas.alpha_composite(image, (x, y))
            sprites[path.stem] = {"x": x, "y": y, "width": TILE_SIZE, "height": TILE_SIZE}

        atlas_dir = root / "atlases"
        atlas_dir.mkdir(parents=True, exist_ok=True)
        atlas_path = atlas_dir / f"{pack_slug}-{category}-atlas-v1.png"
        atlas_json = atlas_dir / f"{pack_slug}-{category}-atlas-v1.json"
        atlas.save(atlas_path)
        atlas_json.write_text(
            json.dumps({"image": atlas_path.name, "sprites": sprites}, ensure_ascii=False, indent=2),
            encoding="utf-8",
        )
        atlas_paths.append(atlas_path)

    preview_dir = root / "previews"
    preview_dir.mkdir(parents=True, exist_ok=True)
    preview = Image.new("RGB", (820, 1740), preview_background)
    draw = ImageDraw.Draw(preview)
    draw.text((26, 20), pack_title, font=find_font(30), fill=preview_accent)
    y = 76
    for label, atlas_path in zip(preview_labels, atlas_paths):
        draw.text((26, y), label, font=find_font(18), fill=(235, 229, 211))
        y += 30
        atlas = Image.open(atlas_path).convert("RGBA").resize((768, 512), Image.Resampling.LANCZOS)
        preview.paste(atlas, (26, y), atlas)
        y += 530
    preview_path = preview_dir / f"{pack_slug}-hex-tileset-v1-preview.png"
    preview.save(preview_path)

    columns, rows = 6, 4
    horizontal_step, vertical_step, odd_offset = 222, 192, 111
    width = (columns - 1) * horizontal_step + odd_offset + TILE_SIZE
    height = (rows - 1) * vertical_step + TILE_SIZE
    adjacency = Image.new("RGB", (width, height), preview_background)
    base_paths = sorted((root / "tiles" / "base").glob("*.png"))
    for index, path in enumerate(base_paths[: columns * rows]):
        row, column = divmod(index, columns)
        x = column * horizontal_step + (odd_offset if row % 2 else 0)
        y = row * vertical_step
        tile = Image.open(path).convert("RGBA")
        adjacency.paste(tile, (x, y), tile)
    adjacency_path = preview_dir / f"{pack_slug}-hex-adjacency-test-v1.png"
    adjacency.save(adjacency_path)

    manifest_dir = root / "manifests"
    manifest_dir.mkdir(parents=True, exist_ok=True)
    manifest = {
        "pack": f"{pack_slug}-hex-tileset",
        "version": 1,
        "orientation": "pointy-top",
        "tile_canvas": [TILE_SIZE, TILE_SIZE],
        "hex_visual_bounds": [HEX_LEFT, 0, HEX_RIGHT, 255],
        "hex_points": HEX_POINTS,
        "recommended_layout": {
            "hex_width": 222,
            "hex_height": 256,
            "horizontal_step": horizontal_step,
            "vertical_step": vertical_step,
            "odd_row_offset_x": odd_offset,
        },
        "tile_count": len(all_records),
        "tiles": all_records,
    }
    manifest_path = manifest_dir / f"{pack_slug}-hex-tileset-v1.json"
    manifest_path.write_text(json.dumps(manifest, ensure_ascii=False, indent=2), encoding="utf-8")
    qa = {"status": "pass" if not errors else "fail", "checked_tiles": len(all_records), "errors": errors}
    qa_path = manifest_dir / "qa-report-v1.json"
    qa_path.write_text(json.dumps(qa, ensure_ascii=False, indent=2), encoding="utf-8")

    result = {
        "tile_count": len(all_records),
        "preview": str(preview_path),
        "adjacency": str(adjacency_path),
        "manifest": str(manifest_path),
        "qa": qa,
    }
    print(json.dumps(result, ensure_ascii=False, indent=2))
    if errors:
        raise SystemExit(1)
    return result
