from __future__ import annotations

import csv
import json
import math
import random
from pathlib import Path

from PIL import Image, ImageDraw, ImageEnhance, ImageFilter, ImageFont, ImageOps


ROOT = Path(__file__).resolve().parents[1]
GAME = ROOT.parents[1]
random.seed(20261030)

BIOMES = ["forest", "swamp", "desert", "snow", "mountain", "tropical", "volcanic", "corrupted", "underground", "plains"]
ROUTE_BIOMES = ["forest", "swamp", "desert", "snow", "mountain", "volcanic", "corrupted", "underground", "plains"]
PALETTES = {
    "forest": ((34, 82, 48), (85, 134, 67)),
    "swamp": ((48, 68, 48), (104, 119, 69)),
    "desert": ((158, 91, 47), (228, 171, 85)),
    "snow": ((79, 116, 154), (211, 232, 241)),
    "mountain": ((76, 82, 91), (151, 165, 176)),
    "tropical": ((25, 102, 77), (65, 178, 112)),
    "volcanic": ((72, 30, 28), (235, 73, 24)),
    "corrupted": ((49, 24, 68), (137, 49, 174)),
    "underground": ((39, 53, 66), (47, 170, 202)),
    "plains": ((96, 113, 58), (199, 177, 82)),
}


def ensure(*paths: Path) -> None:
    for path in paths:
        path.mkdir(parents=True, exist_ok=True)


def font(size: int, bold: bool = False) -> ImageFont.FreeTypeFont:
    name = "consolab.ttf" if bold else "consola.ttf"
    path = Path("C:/Windows/Fonts") / name
    return ImageFont.truetype(str(path), size) if path.exists() else ImageFont.load_default()


def cover(image: Image.Image, size=(800, 800), offset=(0.5, 0.5)) -> Image.Image:
    image = image.convert("RGBA")
    scale = max(size[0] / image.width, size[1] / image.height)
    resized = image.resize((round(image.width * scale), round(image.height * scale)), Image.Resampling.LANCZOS)
    left = max(0, round((resized.width - size[0]) * offset[0]))
    top = max(0, round((resized.height - size[1]) * offset[1]))
    return resized.crop((left, top, left + size[0], top + size[1]))


def clean_alpha(image: Image.Image, threshold=12) -> Image.Image:
    image = image.convert("RGBA")
    alpha = image.getchannel("A").point(lambda value: 0 if value < threshold else value)
    image.putalpha(alpha)
    return image


def cell(source: Image.Image, column: int, row: int, cols=6, rows=4) -> Image.Image:
    left = round(column * source.width / cols)
    right = round((column + 1) * source.width / cols)
    top = round(row * source.height / rows)
    bottom = round((row + 1) * source.height / rows)
    return clean_alpha(source.crop((left, top, right, bottom)).resize((256, 256), Image.Resampling.LANCZOS))


def tint(image: Image.Image, color: tuple[int, int, int], amount=0.2) -> Image.Image:
    base = image.convert("RGBA")
    overlay = Image.new("RGBA", base.size, (*color, round(255 * amount)))
    return Image.alpha_composite(base, overlay)


def weather(image: Image.Image, kind: str, seed: int) -> Image.Image:
    rng = random.Random(seed)
    out = image.convert("RGBA")
    if kind == "day":
        return ImageEnhance.Brightness(out).enhance(1.05)
    if kind == "night":
        return tint(ImageEnhance.Brightness(out).enhance(0.56), (20, 35, 85), 0.28)
    overlay = Image.new("RGBA", out.size, (0, 0, 0, 0))
    draw = ImageDraw.Draw(overlay)
    if kind == "rain":
        overlay = Image.new("RGBA", out.size, (18, 35, 58, 70))
        draw = ImageDraw.Draw(overlay)
        for _ in range(420):
            x, y = rng.randrange(800), rng.randrange(800)
            draw.line((x, y, x - 5, y + 18), fill=(150, 193, 225, 150), width=2)
    elif kind == "fog":
        overlay = Image.new("RGBA", out.size, (140, 150, 155, 70))
        draw = ImageDraw.Draw(overlay)
        for _ in range(28):
            x, y = rng.randrange(-200, 700), rng.randrange(0, 800)
            draw.ellipse((x, y, x + rng.randrange(260, 520), y + rng.randrange(50, 130)), fill=(210, 215, 210, 30))
        overlay = overlay.filter(ImageFilter.GaussianBlur(18))
    else:
        color = (235, 245, 255, 210) if kind == "snow" else (90, 75, 70, 180)
        for _ in range(300):
            x, y = rng.randrange(800), rng.randrange(800)
            radius = rng.choice((1, 1, 2, 3))
            draw.ellipse((x - radius, y - radius, x + radius, y + radius), fill=color)
    return Image.alpha_composite(out, overlay)


def build_routes_and_scenes() -> dict:
    source_a = Image.open(ROOT / "scenes/source/routes-group-a-v1.png").convert("RGBA")
    source_b = Image.open(ROOT / "scenes/source/routes-group-b-v1.png").convert("RGBA")
    bases: dict[str, Image.Image] = {}
    for index, biome in enumerate(["forest", "swamp", "desert", "snow", "mountain"]):
        left, right = round(index * source_a.width / 5), round((index + 1) * source_a.width / 5)
        bases[biome] = cover(source_a.crop((left, 0, right, source_a.height)))
    for index, biome in enumerate(["volcanic", "corrupted", "underground", "plains"]):
        left, right = round(index * source_b.width / 4), round((index + 1) * source_b.width / 4)
        bases[biome] = cover(source_b.crop((left, 0, right, source_b.height)))
    tropical_path = GAME / "scenes/biomes/05-tropical-storm-coast.png"
    bases["tropical"] = cover(Image.open(tropical_path))

    route_count = 0
    for biome in ROUTE_BIOMES:
        folder = ROOT / "scenes/routes" / biome
        ensure(folder)
        base = bases[biome]
        variants = [
            cover(base, offset=(0.35, 0.55)),
            cover(base, offset=(0.65, 0.52)),
            weather(cover(base.resize((920, 920)), offset=(0.5, 0.55)), "fog" if biome in ("swamp", "corrupted", "underground") else "rain", 10 + route_count),
            cover(base.resize((960, 960)), offset=(0.5, 0.45)),
            weather(cover(base.resize((1020, 1020)), offset=(0.5, 0.38)), "night", 20 + route_count),
            cover(base.resize((1080, 1080)), offset=(0.5, 0.32)),
        ]
        names = ["approach", "route-choice", "hazard", "midpoint", "late-route", "arrival"]
        for index, (name, image) in enumerate(zip(names, variants)):
            image.convert("RGB").save(folder / f"{index:02d}-{biome}-{name}-v1.png")
            route_count += 1

    biome_dir = ROOT / "scenes/biomes"
    ensure(biome_dir)
    bases["mountain"].convert("RGB").save(biome_dir / "mountain-biome-concept-v1.png")
    bases["plains"].convert("RGB").save(biome_dir / "plains-biome-concept-v1.png")

    interior_specs = [
        ("settlement-interiors-v1.png", ["village-house", "city-hall", "tavern-inn", "merchant-shop", "temple", "castle-hall"]),
        ("adventure-interiors-v1.png", ["cave", "mine", "dungeon", "graveyard", "crypt", "arcane-ruin"]),
    ]
    interior_count = 0
    for source_name, names in interior_specs:
        source = Image.open(ROOT / "scenes/source" / source_name).convert("RGBA")
        for index, name in enumerate(names):
            image = source.crop(((index % 3) * 512, (index // 3) * 512, (index % 3 + 1) * 512, (index // 3 + 1) * 512))
            folder = ROOT / "scenes/interiors"
            ensure(folder)
            cover(image).convert("RGB").save(folder / f"{name}-interior-v1.png")
            interior_count += 1

    combat_dir = ROOT / "scenes/combat-backgrounds"
    weather_dir = ROOT / "scenes/weather-variants"
    ensure(combat_dir, weather_dir)
    for index, biome in enumerate(BIOMES):
        base = bases[biome]
        arena = base.copy()
        shade = Image.new("RGBA", arena.size, (0, 0, 0, 0))
        draw = ImageDraw.Draw(shade)
        draw.rectangle((0, 575, 800, 800), fill=(10, 10, 14, 75))
        arena = Image.alpha_composite(arena, shade)
        arena.convert("RGB").save(combat_dir / f"{biome}-combat-background-v1.png")
        special = "snow" if biome in ("snow", "mountain") else "ash" if biome in ("volcanic", "corrupted") else "fog"
        for kind in ("day", "night", "rain", "fog", special):
            weather(arena, kind, 1000 + index * 10 + len(kind)).convert("RGB").save(weather_dir / f"{biome}-{kind}-v1.png")

    prop_source = Image.open(ROOT / "scenes/source/environment-props-v1.png").convert("RGBA")
    prop_names = [
        "crate", "barrel", "market-cart", "street-lamp", "signpost", "stone-well",
        "green-tree", "dead-tree", "rock-cluster", "flower-bush", "fallen-log", "camp-tent",
        "treasure-chest", "iron-cage", "stone-altar", "sarcophagus", "mine-cart", "barricade",
        "campfire", "torch-stand", "poison-mushrooms", "ice-crystals", "lava-vent", "broken-portal",
    ]
    props_dir = ROOT / "scenes/props"
    ensure(props_dir)
    props = []
    for index, name in enumerate(prop_names):
        image = cell(prop_source, index % 6, index // 6)
        path = props_dir / f"{name}-prop-v1.png"
        image.save(path)
        props.append(path)

    camps_dir = ROOT / "scenes/camps"
    ensure(camps_dir)
    fire = Image.open(props_dir / "campfire-prop-v1.png").convert("RGBA").resize((210, 210), Image.Resampling.LANCZOS)
    for biome in ("forest", "snow", "underground", "plains"):
        camp = weather(bases[biome], "night", 500 + len(biome))
        camp.alpha_composite(fire, ((800 - fire.width) // 2, 555))
        camp.convert("RGB").save(camps_dir / f"{biome}-camp-v1.png")
    return {"routes": route_count, "interiors": interior_count, "combat": 10, "weather": 50, "props": len(props), "camps": 4, "biome_concepts": 2}


def transform_equipment(image: Image.Image, state: str, direction: str) -> Image.Image:
    result = image.copy()
    if direction == "west":
        result = ImageOps.mirror(result)
    elif direction == "north":
        result = ImageEnhance.Brightness(result).enhance(0.78)
    if state == "walk":
        shifted = Image.new("RGBA", result.size, (0, 0, 0, 0)); shifted.alpha_composite(result, (0, -3)); result = shifted
    elif state == "attack":
        result = result.rotate(-18 if direction != "west" else 18, resample=Image.Resampling.BICUBIC, center=(128, 150))
    elif state == "cast":
        result = tint(result, (114, 70, 210), 0.18)
    return result


def build_equipment() -> dict:
    weapon_source = Image.open(ROOT / "equipment/source/weapon-directions-v1.png").convert("RGBA")
    wearable_source = Image.open(ROOT / "equipment/source/wearable-parts-v1.png").convert("RGBA")
    weapon_names = ["longsword", "battleaxe", "spear", "longbow", "wizard-staff", "round-shield"]
    directions = ["south", "west", "east", "north"]
    states = ["idle", "walk", "attack", "cast"]
    weapon_records = []
    for column, name in enumerate(weapon_names):
        for row, direction in enumerate(directions):
            base = cell(weapon_source, column, row)
            for state in states:
                image = transform_equipment(base, state, direction)
                path = ROOT / "equipment/weapons" / name / direction / f"{name}-{direction}-{state}-v1.png"
                ensure(path.parent); image.save(path)
                weapon_records.append({"id": f"{name}-{direction}-{state}", "file": path.relative_to(ROOT).as_posix(), "anchor": [128, 150]})

    categories = ["helmet", "torso", "boots", "cloak", "accessory", "back-gear"]
    tiers = ["traveler", "ranger", "knight", "arcane"]
    wearable_records = []
    for row, tier in enumerate(tiers):
        for column, category in enumerate(categories):
            base = cell(wearable_source, column, row)
            for direction in directions:
                for state in states:
                    image = transform_equipment(base, state, direction)
                    asset_id = f"{tier}-{category}-{direction}-{state}"
                    path = ROOT / "equipment/wearables" / category / tier / direction / f"{asset_id}-v1.png"
                    ensure(path.parent); image.save(path)
                    wearable_records.append({"id": asset_id, "file": path.relative_to(ROOT).as_posix(), "anchor": [128, 150]})

    loot_records = []
    icon_paths = sorted((GAME / "items").glob("item-pack-*/icons/**/*.png"))
    for index, source_path in enumerate(icon_paths):
        source = Image.open(source_path).convert("RGBA")
        bbox = source.getchannel("A").getbbox()
        crop = source.crop(bbox) if bbox else source
        crop.thumbnail((72, 72), Image.Resampling.LANCZOS)
        image = Image.new("RGBA", (96, 96), (0, 0, 0, 0))
        draw = ImageDraw.Draw(image)
        draw.ellipse((17, 68, 79, 84), fill=(0, 0, 0, 80))
        image.alpha_composite(crop, ((96 - crop.width) // 2, 10 + (66 - crop.height)))
        asset_id = source_path.stem
        path = ROOT / "equipment/dropped-loot" / f"{asset_id}-drop-v1.png"
        ensure(path.parent); image.save(path)
        loot_records.append({"id": asset_id, "file": path.relative_to(ROOT).as_posix(), "pivot": [48, 76]})

    profiles = {}
    races = ["human", "elf", "dwarf", "halfling", "gnome", "half-elf", "drow", "half-orc", "tiefling", "dragonborn", "githyanki"]
    size_offsets = {"dwarf": (0, 9), "halfling": (0, 18), "gnome": (0, 20), "half-orc": (0, -4), "dragonborn": (0, -6)}
    for race in races:
        for gender in ("female", "male"):
            ox, oy = size_offsets.get(race, (0, 0))
            profiles[f"{race}-{gender}"] = {
                "hand": {"south": [128 + ox, 150 + oy], "west": [103 + ox, 151 + oy], "east": [153 + ox, 151 + oy], "north": [128 + ox, 146 + oy]},
                "head": [128 + ox, 67 + oy], "torso": [128 + ox, 123 + oy], "feet": [128 + ox, 220 + oy], "back": [128 + ox, 116 + oy]
            }
    manifest = {"id": "equipment-runtime-v1", "status": "complete", "weapons": weapon_records, "wearables": wearable_records, "dropped_loot": loot_records, "anchor_profiles": profiles}
    ensure(ROOT / "equipment/manifests")
    (ROOT / "equipment/manifests/equipment-runtime-v1.json").write_text(json.dumps(manifest, indent=2) + "\n", encoding="utf-8")
    return {"weapon_frames": len(weapon_records), "wearable_frames": len(wearable_records), "dropped_loot": len(loot_records), "anchor_profiles": len(profiles)}


def save_gif(paths: list[Path], output: Path, duration=110) -> None:
    frames = [Image.open(path).convert("RGBA") for path in paths]
    frames[0].save(output, save_all=True, append_images=frames[1:], duration=duration, loop=0, disposal=2)


def build_effects() -> dict:
    specs = [
        ("magic-vfx-v1.png", ["fire", "ice", "lightning", "poison", "healing", "dark"]),
        ("combat-vfx-v1.png", ["slash", "projectile", "block", "critical", "dust-smoke", "portal"]),
    ]
    total = 0
    animations = 0
    for source_name, names in specs:
        source = Image.open(ROOT / "effects/source" / source_name).convert("RGBA")
        for column, name in enumerate(names):
            frames = []
            for row in range(4):
                image = cell(source, column, row)
                path = ROOT / "effects/frames" / name / f"{name}-{row:02d}-v1.png"
                ensure(path.parent); image.save(path); frames.append(path); total += 1
            gif_path = ROOT / "effects/animations" / f"{name}-v1.gif"
            ensure(gif_path.parent); save_gif(frames, gif_path); animations += 1

    environment = ["water", "lava", "rain", "snow", "fire", "smoke", "blood", "healing-particles"]
    for effect_index, name in enumerate(environment):
        frames = []
        for frame_index in range(8):
            image = Image.new("RGBA", (128, 128), (0, 0, 0, 0))
            draw = ImageDraw.Draw(image)
            rng = random.Random(9000 + effect_index * 100 + frame_index)
            if name in ("rain", "snow", "smoke", "healing-particles"):
                for _ in range(35):
                    x, y = rng.randrange(8, 120), rng.randrange(8, 120)
                    color = (125, 180, 230, 190) if name == "rain" else (240, 250, 255, 210) if name == "snow" else (125, 125, 130, 120) if name == "smoke" else (93, 235, 145, 210)
                    if name == "rain": draw.line((x, y, x - 4, y + 12), fill=color, width=2)
                    else: draw.ellipse((x - 2, y - 2, x + 2, y + 2), fill=color)
            else:
                color = (42, 145, 220, 210) if name == "water" else (245, 75, 20, 230) if name in ("lava", "fire") else (145, 16, 24, 210)
                for band in range(5):
                    points = []
                    for x in range(0, 129, 8):
                        y = 40 + band * 13 + round(math.sin((x + frame_index * 8 + band * 11) / 13) * 7)
                        points.append((x, y))
                    draw.line(points, fill=color, width=4)
            path = ROOT / "effects/environment" / name / f"{name}-{frame_index:02d}-v1.png"
            ensure(path.parent); image.save(path); frames.append(path); total += 1
        gif_path = ROOT / "effects/animations" / f"{name}-v1.gif"
        save_gif(frames, gif_path, 90); animations += 1
    return {"frames": total, "animations": animations}


def draw_icon(category: str, index: int) -> Image.Image:
    colors = {"subclass": (72, 118, 178), "spell": (125, 67, 176), "ability": (176, 76, 55), "background": (72, 138, 91), "quest": (181, 143, 54)}
    color = colors[category]
    image = Image.new("RGBA", (64, 64), (0, 0, 0, 0)); draw = ImageDraw.Draw(image)
    draw.rounded_rectangle((3, 3, 60, 60), radius=7, fill=(20, 22, 28, 255), outline=(*color, 255), width=3)
    cx, cy = 32, 32
    sides = 3 + index % 6; radius = 12 + (index % 4) * 3
    points = [(cx + math.cos(-math.pi / 2 + i * 2 * math.pi / sides) * radius, cy + math.sin(-math.pi / 2 + i * 2 * math.pi / sides) * radius) for i in range(sides)]
    draw.polygon(points, outline=(235, 220, 166, 255), fill=(*color, 150))
    draw.line((18, 46, 46, 18), fill=(245, 235, 200, 220), width=2)
    return image


def panel(draw: ImageDraw.ImageDraw, box, title: str) -> None:
    x0, y0, x1, y1 = box
    draw.rounded_rectangle(box, radius=9, fill=(31, 29, 37, 238), outline=(169, 135, 71, 255), width=3)
    draw.rectangle((x0 + 3, y0 + 3, x1 - 3, y0 + 40), fill=(58, 42, 66, 255))
    draw.text((x0 + 14, y0 + 9), title, font=font(19, True), fill=(242, 220, 165, 255))


def button(draw: ImageDraw.ImageDraw, xy, text: str, active=False) -> None:
    x, y, w, h = xy
    fill = (104, 62, 118, 255) if active else (61, 51, 70, 255)
    draw.rounded_rectangle((x, y, x + w, y + h), radius=6, fill=fill, outline=(196, 157, 79, 255), width=2)
    bbox = draw.textbbox((0, 0), text, font=font(18, True)); tw = bbox[2] - bbox[0]; th = bbox[3] - bbox[1]
    draw.text((x + (w - tw) / 2, y + (h - th) / 2 - 2), text, font=font(18, True), fill=(245, 235, 205, 255))


def build_ui() -> dict:
    logo = Image.new("RGBA", (512, 256), (0, 0, 0, 0)); draw = ImageDraw.Draw(logo)
    hexagon = [(256 + math.cos(-math.pi / 2 + i * math.pi / 3) * 92, 124 + math.sin(-math.pi / 2 + i * math.pi / 3) * 92) for i in range(6)]
    draw.polygon(hexagon, fill=(42, 31, 58, 250), outline=(215, 156, 60, 255), width=8)
    draw.ellipse((202, 84, 310, 164), fill=(213, 130, 42, 255), outline=(20, 14, 25, 255), width=5)
    draw.ellipse((239, 91, 273, 159), fill=(55, 20, 72, 255)); draw.ellipse((250, 108, 262, 145), fill=(245, 208, 75, 255))
    ensure(ROOT / "ui/logo"); logo.save(ROOT / "ui/logo/game-logo-mark-v1.png")

    screen_specs = [
        ("main-menu", ["NEW GAME", "CONTINUE", "LOAD GAME", "SETTINGS", "EXIT"]),
        ("character-identity", ["NAME", "BODY", "VOICE", "PORTRAIT", "CONFIRM"]),
        ("race-selection", ["RACE", "TRAITS", "PREVIEW", "BACK", "NEXT"]),
        ("class-subclass-selection", ["CLASS", "SUBCLASS", "FEATURES", "BACK", "NEXT"]),
        ("background-selection", ["ORIGIN", "SKILLS", "CONTACTS", "BACK", "BEGIN"]),
        ("shop-trade", ["BUY", "SELL", "COMPARE", "GOLD", "CLOSE"]),
        ("loot", ["TAKE", "TAKE ALL", "INSPECT", "WEIGHT", "CLOSE"]),
        ("crafting", ["RECIPE", "MATERIALS", "RESULT", "CRAFT", "CLOSE"]),
        ("spellbook-abilities", ["SPELLS", "ABILITIES", "SLOTS", "PREPARE", "CLOSE"]),
        ("settings", ["AUDIO", "VIDEO", "CONTROLS", "ACCESSIBILITY", "APPLY"]),
        ("save-load", ["SAVE", "LOAD", "DELETE", "SLOTS", "CLOSE"]),
        ("pause", ["RESUME", "SAVE", "LOAD", "SETTINGS", "MAIN MENU"]),
        ("death", ["RETRY", "LOAD GAME", "MAIN MENU"]),
        ("victory", ["CONTINUE", "STATISTICS", "MAIN MENU"]),
        ("tutorial-help", ["MOVEMENT", "COMBAT", "INVENTORY", "MAP", "CLOSE"]),
    ]
    screens_dir = ROOT / "ui/screens/runtime-800"; ensure(screens_dir)
    for screen_index, (name, actions) in enumerate(screen_specs):
        image = Image.new("RGBA", (800, 800), (17, 16, 22, 255)); d = ImageDraw.Draw(image)
        for y in range(0, 800, 16):
            d.line((0, y, 800, y), fill=(25, 22, 31, 255))
        d.rectangle((20, 20, 780, 780), outline=(164, 127, 67, 255), width=5)
        d.text((44, 38), name.replace("-", " ").upper(), font=font(30, True), fill=(238, 211, 150, 255))
        if name == "main-menu":
            emblem = logo.resize((256, 128), Image.Resampling.LANCZOS); image.alpha_composite(emblem, (272, 110))
            for i, action in enumerate(actions): button(d, (230, 280 + i * 72, 340, 54), action, i == 0)
        else:
            panel(d, (40, 100, 248, 720), "OPTIONS")
            panel(d, (264, 100, 760, 470), "PREVIEW")
            panel(d, (264, 486, 760, 720), "DETAILS")
            for i, action in enumerate(actions): button(d, (58, 160 + i * 82, 172, 50), action, i == min(3, len(actions)-1))
            for row in range(3):
                for col in range(5):
                    x, y = 292 + col * 88, 155 + row * 88
                    d.rounded_rectangle((x, y, x + 64, y + 64), radius=5, fill=(44, 43, 53, 255), outline=(105, 92, 112, 255), width=2)
                    d.ellipse((x + 18, y + 18, x + 46, y + 46), fill=(78 + screen_index * 7 % 120, 72, 122, 255))
        image.convert("RGB").save(screens_dir / f"{name}-screen-v1.png")

    icon_records = []
    for category in ("subclass", "spell", "ability", "background", "quest"):
        atlas = Image.new("RGBA", (6 * 64, 4 * 64), (0, 0, 0, 0))
        for index in range(24):
            icon = draw_icon(category, index)
            path = ROOT / "ui/icons" / category / f"{category}-{index + 1:02d}-v1.png"
            ensure(path.parent); icon.save(path)
            atlas.alpha_composite(icon, ((index % 6) * 64, (index // 6) * 64))
            icon_records.append(path.relative_to(ROOT).as_posix())
        atlas_dir = ROOT / "ui/atlases"; ensure(atlas_dir); atlas.save(atlas_dir / f"{category}-icons-atlas-v1.png")
    return {"screens": len(screen_specs), "icons": len(icon_records), "logo": 1}


def write_inventory(summary: dict) -> dict:
    files = sorted(path for path in ROOT.rglob("*") if path.is_file())
    visual = [path for path in files if path.suffix.lower() in {".png", ".gif", ".webp"}]
    rows = []
    for path in visual:
        with Image.open(path) as image:
            rows.append({"file": path.relative_to(ROOT).as_posix(), "width": image.width, "height": image.height, "mode": image.mode, "bytes": path.stat().st_size})
    with (ROOT / "asset-index-v1.csv").open("w", newline="", encoding="utf-8-sig") as handle:
        writer = csv.DictWriter(handle, fieldnames=["file", "width", "height", "mode", "bytes"]); writer.writeheader(); writer.writerows(rows)
    errors = []
    for row in rows:
        if row["width"] <= 0 or row["height"] <= 0: errors.append(f"Invalid size: {row['file']}")
    runtime_screens = list((ROOT / "ui/screens/runtime-800").glob("*.png"))
    for path in runtime_screens:
        if Image.open(path).size != (800, 800): errors.append(f"Runtime screen not 800x800: {path.name}")
    manifest = {"id": "final-production-v1", "status": "complete" if not errors else "qa_failed", "story_assets_included": False, "summary": summary, "visual_file_count": len(rows), "qa": {"status": "pass" if not errors else "fail", "errors": errors}}
    ensure(ROOT / "manifests")
    (ROOT / "manifests/final-production-v1.json").write_text(json.dumps(manifest, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    (ROOT / "manifests/qa-report-v1.json").write_text(json.dumps(manifest["qa"], ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    return manifest


def main() -> None:
    summary = {
        "equipment": build_equipment(),
        "scenes": build_routes_and_scenes(),
        "effects": build_effects(),
        "ui": build_ui(),
        "maps": {"isometric_world_map": 1},
    }
    manifest = write_inventory(summary)
    print(json.dumps(manifest, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
