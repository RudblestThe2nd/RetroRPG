from __future__ import annotations

import json
from pathlib import Path

import cv2
import numpy as np
from PIL import Image, ImageDraw, ImageEnhance, ImageFilter, ImageFont


ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / "source" / "imagegen"
ICON_SIZE = (256, 256)


MATERIALS = [
    ("firewood", "Odun Demeti"), ("rough-stone", "Kaba Taş"),
    ("iron-ore", "Demir Cevheri"), ("silver-ore", "Gümüş Cevheri"),
    ("gold-ore", "Altın Cevheri"), ("obsidian-shard", "Obsidyen Parçası"),
    ("cured-leather", "İşlenmiş Deri"), ("wolf-fur", "Kurt Kürkü"),
    ("linen-cloth", "Keten Kumaş"), ("silk-bundle", "İpek Demeti"),
    ("monster-bone", "Yaratık Kemiği"), ("beast-horn", "Canavar Boynuzu"),
    ("eagle-feathers", "Kartal Tüyleri"), ("healing-herbs", "Şifalı Otlar"),
    ("cave-mushrooms", "Mağara Mantarları"), ("venom-sac", "Zehir Kesesi"),
    ("crystal-dust", "Kristal Tozu"), ("coal", "Kömür"),
    ("salt-pouch", "Tuz Kesesi"), ("tree-resin", "Ağaç Reçinesi"),
    ("hemp-rope", "Kendir Halat"), ("brass-parts", "Pirinç Parçalar"),
    ("empty-vial", "Boş Şişe"), ("blank-parchment", "Boş Parşömen"),
]

CAMP_TOOLS = [
    ("bedroll", "Yatak Rulosu"), ("canvas-tent", "Bez Çadır"),
    ("firewood-and-tinder", "Odun ve Kav"), ("cooking-pot", "Yemek Kazanı"),
    ("iron-skillet", "Demir Tava"), ("waterskin", "Su Tulumu"),
    ("fishing-rod", "Olta"), ("hunting-trap", "Av Tuzağı"),
    ("repair-hammer", "Tamir Çekici"), ("sewing-kit", "Dikiş Seti"),
    ("whetstone", "Bileme Taşı"), ("shovel", "Kürek"),
    ("pickaxe", "Kazma"), ("lantern", "Fener"),
    ("lamp-oil", "Lamba Yağı"), ("compass", "Pusula"),
    ("map-case", "Harita Çantası"), ("climbing-rope", "Tırmanma Halatı"),
    ("grappling-hook", "Kanca"), ("spyglass", "Dürbün"),
    ("mortar-and-pestle", "Havan ve Tokmak"), ("herbalist-pouch", "Şifacı Çantası"),
    ("alchemy-kit", "Simya Seti"), ("adventurer-backpack", "Maceracı Çantası"),
]

QUEST_RELICS = [
    ("eye-tower-key", "Göz Kulesi Anahtarı"), ("eye-sigil", "Göz Mührü"),
    ("royal-seal", "Kraliyet Mührü"), ("ancient-tablet", "Kadim Tablet"),
    ("broken-black-crown", "Kırık Kara Taç"), ("dragon-scale", "Ejderha Pulu"),
    ("phoenix-feather", "Anka Tüyü"), ("moon-shard", "Ay Parçası"),
    ("sun-medallion", "Güneş Madalyonu"), ("swamp-idol", "Bataklık Putu"),
    ("glacier-rune", "Buzul Rünü"), ("volcanic-core", "Volkan Çekirdeği"),
    ("storm-pearl", "Fırtına İncisi"), ("haunted-locket", "Perili Madalyon"),
    ("cursed-mirror", "Lanetli Ayna"), ("celestial-compass", "Göksel Pusula"),
    ("shadow-orb", "Gölge Küresi"), ("forest-seed", "Kadim Orman Tohumu"),
    ("desert-scarab", "Çöl Bokböceği"), ("abyss-chain", "Uçurum Zinciri"),
    ("sealed-letter", "Mühürlü Mektup"), ("blood-contract", "Kan Sözleşmesi"),
    ("chained-grimoire", "Zincirli Büyü Kitabı"), ("world-map-fragment", "Dünya Haritası Parçası"),
]

CATEGORIES = [
    ("materials", "materials-source-v1.png", MATERIALS),
    ("camp-tools", "camp-tools-source-v1.png", CAMP_TOOLS),
    ("quest-relics", "quest-relics-source-v1.png", QUEST_RELICS),
]


def crop_grid(image: Image.Image, column: int, row: int) -> Image.Image:
    left = round(column * image.width / 6)
    right = round((column + 1) * image.width / 6)
    top = round(row * image.height / 4)
    bottom = round((row + 1) * image.height / 4)
    return image.crop((left, top, right, bottom)).convert("RGBA")


def remove_background(image: Image.Image) -> Image.Image:
    rgba = np.array(image.convert("RGBA"))
    rgb = cv2.cvtColor(rgba[:, :, :3], cv2.COLOR_RGB2BGR)
    height, width = rgb.shape[:2]
    mask = np.zeros((height, width), np.uint8)
    bg_model = np.zeros((1, 65), np.float64)
    fg_model = np.zeros((1, 65), np.float64)
    margin_x = max(4, int(width * 0.045))
    margin_y = max(4, int(height * 0.045))
    rect = (margin_x, margin_y, width - margin_x * 2, height - margin_y * 2)
    try:
        cv2.grabCut(rgb, mask, rect, bg_model, fg_model, 7, cv2.GC_INIT_WITH_RECT)
        foreground = np.where(
            (mask == cv2.GC_FGD) | (mask == cv2.GC_PR_FGD), 255, 0
        ).astype(np.uint8)

        # Preserve thin, dark item parts (black keys, chains and crown spikes).
        # The source backdrop is smooth, so strong closed edges overwhelmingly
        # belong to the centered object rather than the background.
        gray = cv2.cvtColor(rgb, cv2.COLOR_BGR2GRAY)
        edges = cv2.Canny(gray, 28, 92)
        edges = cv2.dilate(edges, np.ones((3, 3), np.uint8), iterations=2)
        contours, _ = cv2.findContours(edges, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
        edge_fill = np.zeros_like(foreground)
        for contour in contours:
            if cv2.contourArea(contour) >= 8:
                cv2.drawContours(edge_fill, [contour], -1, 255, thickness=cv2.FILLED)
        foreground = np.maximum(foreground, edge_fill)
        foreground = cv2.morphologyEx(foreground, cv2.MORPH_CLOSE, np.ones((3, 3), np.uint8))
        border = max(8, int(min(width, height) * 0.045))
        foreground[:border, :] = 0
        foreground[-border:, :] = 0
        foreground[:, :border] = 0
        foreground[:, -border:] = 0
        foreground = cv2.GaussianBlur(foreground, (3, 3), 0)
        rgba[:, :, 3] = np.minimum(rgba[:, :, 3], foreground)
    except cv2.error:
        pass
    return Image.fromarray(rgba, "RGBA")


def trim_and_fit(image: Image.Image, size: tuple[int, int] = ICON_SIZE) -> Image.Image:
    alpha = np.asarray(image.getchannel("A"))
    ys, xs = np.where(alpha > 10)
    if len(xs):
        left = max(0, int(xs.min()) - 5)
        right = min(image.width, int(xs.max()) + 6)
        top = max(0, int(ys.min()) - 5)
        bottom = min(image.height, int(ys.max()) + 6)
        image = image.crop((left, top, right, bottom))
    available = (size[0] - 28, size[1] - 28)
    scale = min(available[0] / image.width, available[1] / image.height)
    image = image.resize(
        (max(1, round(image.width * scale)), max(1, round(image.height * scale))),
        Image.Resampling.LANCZOS,
    )
    canvas = Image.new("RGBA", size, (0, 0, 0, 0))
    canvas.alpha_composite(image, ((size[0] - image.width) // 2, (size[1] - image.height) // 2))
    return canvas


def build_rarity_frames() -> list[Path]:
    colors = {
        "common": (150, 150, 158),
        "uncommon": (59, 190, 88),
        "rare": (60, 128, 238),
        "epic": (158, 76, 216),
        "legendary": (238, 169, 35),
        "quest": (215, 55, 48),
    }
    output = []
    target = ROOT / "icons" / "rarity-frames"
    for name, color in colors.items():
        image = Image.new("RGBA", (96, 96), (0, 0, 0, 0))
        draw = ImageDraw.Draw(image)
        dark = tuple(max(0, channel - 95) for channel in color)
        draw.rectangle((5, 5, 90, 90), outline=(18, 18, 22, 255), width=5)
        draw.rectangle((9, 9, 86, 86), outline=(*dark, 255), width=4)
        draw.rectangle((13, 13, 82, 82), outline=(*color, 255), width=4)
        for x, y in ((7, 7), (77, 7), (7, 77), (77, 77)):
            draw.rectangle((x, y, x + 12, y + 12), fill=(*dark, 255), outline=(*color, 255), width=2)
        if name in ("epic", "legendary", "quest"):
            glow = image.getchannel("A").filter(ImageFilter.GaussianBlur(4)).point(lambda value: int(value * 0.34))
            glow_layer = Image.new("RGBA", image.size, (*color, 0))
            glow_layer.putalpha(glow)
            glow_layer.alpha_composite(image)
            image = glow_layer
        path = target / f"rarity-frame-{name}.png"
        image.save(path)
        output.append(path)
    return output


def item_record(category: str, item_id: str, name_tr: str, index: int) -> dict:
    if category == "materials":
        rarity = "rare" if item_id in {"gold-ore", "obsidian-shard", "crystal-dust", "venom-sac"} else "common"
        return {
            "id": item_id, "name_tr": name_tr, "category": category,
            "rarity": rarity, "stackable": True, "max_stack": 99,
            "weight": round(0.1 + (index % 6) * 0.18, 2), "base_value": 4 + index * 3,
            "quest_item": False,
        }
    if category == "camp-tools":
        stackable = item_id in {"firewood-and-tinder", "lamp-oil"}
        return {
            "id": item_id, "name_tr": name_tr, "category": category,
            "rarity": "uncommon" if item_id in {"compass", "spyglass", "alchemy-kit"} else "common",
            "stackable": stackable, "max_stack": 20 if stackable else 1,
            "weight": round(0.5 + (index % 6) * 0.55, 2), "base_value": 12 + index * 5,
            "quest_item": False,
        }
    return {
        "id": item_id, "name_tr": name_tr, "category": category,
        "rarity": "quest", "stackable": False, "max_stack": 1,
        "weight": round(0.1 + (index % 6) * 0.2, 2), "base_value": 0,
        "quest_item": True,
    }


def make_sheet(category: str, icons: list[Path]) -> tuple[Path, Path]:
    sheet = Image.new("RGBA", (1536, 1024), (0, 0, 0, 0))
    sprites = {}
    for index, path in enumerate(icons):
        icon = Image.open(path).convert("RGBA")
        x = (index % 6) * 256
        y = (index // 6) * 256
        sheet.alpha_composite(icon, (x, y))
        sprites[path.stem] = {"x": x, "y": y, "width": 256, "height": 256}
    sheet_path = ROOT / "sheets" / f"{category}-sheet-v1.png"
    atlas_path = ROOT / "atlases" / f"{category}-atlas-v1.png"
    atlas_json = ROOT / "atlases" / f"{category}-atlas-v1.json"
    sheet.save(sheet_path)
    sheet.save(atlas_path)
    atlas_json.write_text(
        json.dumps({"image": atlas_path.name, "sprites": sprites}, ensure_ascii=False, indent=2),
        encoding="utf-8",
    )
    return sheet_path, atlas_json


def find_font(size: int):
    path = Path("C:/Windows/Fonts/consolab.ttf")
    return ImageFont.truetype(path, size) if path.exists() else ImageFont.load_default()


def build_preview(sheets: list[Path]) -> Path:
    canvas = Image.new("RGB", (820, 1740), (18, 16, 22))
    draw = ImageDraw.Draw(canvas)
    draw.text((26, 20), "ITEM PACK 02", font=find_font(30), fill=(241, 183, 48))
    labels = ("CRAFTING MATERIALS", "CAMP & EXPLORATION TOOLS", "QUEST ITEMS & RELICS")
    y = 76
    for label, sheet_path in zip(labels, sheets):
        draw.text((26, y), label, font=find_font(18), fill=(230, 225, 211))
        y += 30
        sheet = Image.open(sheet_path).convert("RGBA").resize((768, 512), Image.Resampling.LANCZOS)
        canvas.paste(sheet, (26, y), sheet)
        y += 530
    path = ROOT / "previews" / "item-pack-02-preview.png"
    canvas.save(path)
    return path


def main() -> None:
    all_items = []
    sheet_paths = []
    qa_errors = []

    for category, source_name, definitions in CATEGORIES:
        source = Image.open(SOURCE / source_name).convert("RGBA")
        icon_paths = []
        output_dir = ROOT / "icons" / category
        for index, (item_id, name_tr) in enumerate(definitions):
            cell = crop_grid(source, index % 6, index // 6)
            icon = trim_and_fit(remove_background(cell))
            path = output_dir / f"{index + 1:02d}-{item_id}.png"
            icon.save(path)
            icon_paths.append(path)
            all_items.append({
                **item_record(category, item_id, name_tr, index),
                "icon": path.relative_to(ROOT).as_posix(),
            })
            alpha = np.asarray(icon.getchannel("A"))
            if int(alpha.max()) == 0:
                qa_errors.append(f"Blank icon: {path.relative_to(ROOT).as_posix()}")
            if any(icon.getpixel(point)[3] > 8 for point in ((0, 0), (255, 0), (0, 255), (255, 255))):
                qa_errors.append(f"Opaque corner: {path.relative_to(ROOT).as_posix()}")
        sheet_path, _ = make_sheet(category, icon_paths)
        sheet_paths.append(sheet_path)

    rarity_frames = build_rarity_frames()
    manifest = {
        "pack": "item-pack-02", "version": 1,
        "icon_size": [256, 256], "grid": [6, 4],
        "item_count": len(all_items), "rarity_frames": [path.relative_to(ROOT).as_posix() for path in rarity_frames],
        "items": all_items,
    }
    (ROOT / "manifests" / "items-v1.json").write_text(
        json.dumps(manifest, ensure_ascii=False, indent=2), encoding="utf-8"
    )
    qa = {"status": "pass" if not qa_errors else "fail", "checked_icons": len(all_items), "errors": qa_errors}
    (ROOT / "manifests" / "qa-report-v1.json").write_text(
        json.dumps(qa, ensure_ascii=False, indent=2), encoding="utf-8"
    )
    preview = build_preview(sheet_paths)
    print(f"Built {len(all_items)} item icons and {len(rarity_frames)} rarity frames")
    print(f"Preview: {preview}")
    print(json.dumps(qa, ensure_ascii=False))
    raise SystemExit(1 if qa_errors else 0)


if __name__ == "__main__":
    main()
