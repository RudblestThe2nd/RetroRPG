from __future__ import annotations

import json
import math
from pathlib import Path

import cv2
import numpy as np
from PIL import Image, ImageDraw, ImageEnhance, ImageFilter, ImageFont, ImageOps


ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / "source" / "imagegen"
ATLASES = ROOT / "atlases"
MANIFESTS = ROOT / "manifests"
PREVIEWS = ROOT / "previews"
ANIMATION_PREVIEWS = ROOT / "animations" / "previews"

STATES = ("normal", "hover", "pressed", "disabled")
STATE_DURATIONS_MS = {"normal": 420, "hover": 160, "pressed": 110, "disabled": 420}


def ensure_dirs() -> None:
    for path in (
        ATLASES,
        MANIFESTS,
        PREVIEWS,
        ANIMATION_PREVIEWS,
        ROOT / "controls" / "buttons" / "small",
        ROOT / "controls" / "buttons" / "medium",
        ROOT / "controls" / "buttons" / "large",
        ROOT / "controls" / "buttons" / "icon",
        ROOT / "controls" / "tabs",
        ROOT / "controls" / "scrollbars" / "vertical",
        ROOT / "controls" / "scrollbars" / "horizontal",
        ROOT / "controls" / "slots" / "item",
        ROOT / "controls" / "slots" / "equipment",
        ROOT / "controls" / "slots" / "ability",
        ROOT / "controls" / "slots" / "portrait",
        ROOT / "frames" / "panels",
        ROOT / "frames" / "modals",
        ROOT / "frames" / "tooltips",
        ROOT / "icons" / "system",
        ROOT / "icons" / "resources",
        ROOT / "icons" / "classes",
        ROOT / "icons" / "races",
        ROOT / "icons" / "status_effects",
        ROOT / "cursors",
        ROOT / "input" / "keyboard",
        ROOT / "input" / "mouse",
    ):
        path.mkdir(parents=True, exist_ok=True)


def open_rgba(name: str) -> Image.Image:
    return Image.open(SOURCE / name).convert("RGBA")


def crop_grid(image: Image.Image, columns: int, rows: int, column: int, row: int) -> Image.Image:
    left = round(column * image.width / columns)
    right = round((column + 1) * image.width / columns)
    top = round(row * image.height / rows)
    bottom = round((row + 1) * image.height / rows)
    return image.crop((left, top, right, bottom))


def trim_alpha(image: Image.Image, threshold: int = 8, padding: int = 4) -> Image.Image:
    alpha = np.array(image.getchannel("A"))
    ys, xs = np.where(alpha > threshold)
    if len(xs) == 0:
        return image
    left = max(0, int(xs.min()) - padding)
    right = min(image.width, int(xs.max()) + padding + 1)
    top = max(0, int(ys.min()) - padding)
    bottom = min(image.height, int(ys.max()) + padding + 1)
    return image.crop((left, top, right, bottom))


def grabcut_foreground(image: Image.Image) -> Image.Image:
    """Remove soft atlas backdrops while retaining the centered icon and its glow."""
    rgba = np.array(image.convert("RGBA"))
    rgb = cv2.cvtColor(rgba[:, :, :3], cv2.COLOR_RGB2BGR)
    height, width = rgb.shape[:2]
    margin_x = max(3, int(width * 0.055))
    margin_y = max(3, int(height * 0.055))
    mask = np.zeros((height, width), np.uint8)
    bg_model = np.zeros((1, 65), np.float64)
    fg_model = np.zeros((1, 65), np.float64)
    rect = (margin_x, margin_y, width - margin_x * 2, height - margin_y * 2)
    try:
        cv2.grabCut(rgb, mask, rect, bg_model, fg_model, 6, cv2.GC_INIT_WITH_RECT)
        foreground = np.where((mask == cv2.GC_FGD) | (mask == cv2.GC_PR_FGD), 255, 0).astype(np.uint8)
        foreground = cv2.morphologyEx(foreground, cv2.MORPH_CLOSE, np.ones((3, 3), np.uint8))
        foreground = cv2.GaussianBlur(foreground, (3, 3), 0)
        rgba[:, :, 3] = np.minimum(rgba[:, :, 3], foreground)
    except cv2.error:
        pass
    return Image.fromarray(rgba, "RGBA")


def fit_sprite(image: Image.Image, size: tuple[int, int], padding: int = 4) -> Image.Image:
    image = trim_alpha(image)
    max_w = max(1, size[0] - padding * 2)
    max_h = max(1, size[1] - padding * 2)
    scale = min(max_w / image.width, max_h / image.height)
    resized = image.resize(
        (max(1, round(image.width * scale)), max(1, round(image.height * scale))),
        Image.Resampling.LANCZOS,
    )
    canvas = Image.new("RGBA", size, (0, 0, 0, 0))
    canvas.alpha_composite(resized, ((size[0] - resized.width) // 2, (size[1] - resized.height) // 2))
    return canvas


def brightness(image: Image.Image, factor: float) -> Image.Image:
    alpha = image.getchannel("A")
    rgb = ImageEnhance.Brightness(image.convert("RGB")).enhance(factor)
    result = rgb.convert("RGBA")
    result.putalpha(alpha)
    return result


def gold_glow(image: Image.Image) -> Image.Image:
    alpha = image.getchannel("A")
    glow_alpha = alpha.filter(ImageFilter.MaxFilter(7)).filter(ImageFilter.GaussianBlur(2))
    glow_alpha = glow_alpha.point(lambda value: int(value * 0.42))
    glow = Image.new("RGBA", image.size, (255, 181, 24, 0))
    glow.putalpha(glow_alpha)
    glow.alpha_composite(brightness(image, 1.12))
    return glow


def pressed(image: Image.Image, offset: int = 2) -> Image.Image:
    dark = brightness(image, 0.78)
    canvas = Image.new("RGBA", image.size, (0, 0, 0, 0))
    canvas.alpha_composite(dark, (0, offset))
    return canvas


def disabled(image: Image.Image) -> Image.Image:
    alpha = image.getchannel("A").point(lambda value: int(value * 0.62))
    gray = ImageOps.grayscale(image.convert("RGB")).convert("RGBA")
    gray = ImageEnhance.Contrast(gray).enhance(0.82)
    gray.putalpha(alpha)
    return gray


def save_states(base: Image.Image, directory: Path, stem: str) -> list[Path]:
    directory.mkdir(parents=True, exist_ok=True)
    variants = {
        "normal": base,
        "hover": gold_glow(base),
        "pressed": pressed(base),
        "disabled": disabled(base),
    }
    output: list[Path] = []
    for state, image in variants.items():
        path = directory / f"{stem}_{state}.png"
        image.save(path)
        output.append(path)
    return output


def save_gif(paths: list[Path], target: Path) -> None:
    frames = [Image.open(path).convert("RGBA") for path in paths]
    max_w = max(frame.width for frame in frames)
    max_h = max(frame.height for frame in frames)
    normalized = []
    for frame in frames:
        canvas = Image.new("RGBA", (max_w, max_h), (0, 0, 0, 0))
        canvas.alpha_composite(frame, ((max_w - frame.width) // 2, (max_h - frame.height) // 2))
        normalized.append(canvas)
    normalized[0].save(
        target,
        save_all=True,
        append_images=normalized[1:],
        duration=[420, 160, 110, 240][: len(normalized)],
        loop=0,
        disposal=2,
        transparency=0,
    )


def build_core_controls() -> list[Path]:
    source = open_rgba("core-controls-source-v1.png")
    specs = [
        (0, ROOT / "controls" / "buttons" / "large", "button-large", (256, 72)),
        (1, ROOT / "controls" / "buttons" / "medium", "button-medium", (192, 64)),
        (2, ROOT / "controls" / "buttons" / "small", "button-small", (128, 48)),
        (3, ROOT / "controls" / "buttons" / "icon", "button-icon", (72, 72)),
        (4, ROOT / "controls" / "tabs", "tab-primary", (176, 52)),
    ]
    files: list[Path] = []
    for row, directory, stem, size in specs:
        state_files = []
        for column, state in enumerate(STATES):
            cell = fit_sprite(crop_grid(source, 4, 6, column, row), size, padding=1)
            path = directory / f"{stem}_{state}.png"
            cell.save(path)
            files.append(path)
            state_files.append(path)
        save_gif(state_files[:3] + [state_files[0]], ANIMATION_PREVIEWS / f"{stem}-press-preview.gif")

    slot_categories = {
        "item": (84, 72, 115),
        "equipment": (92, 72, 116),
        "ability": (79, 50, 118),
        "portrait": (62, 55, 78),
    }
    for category, tint in slot_categories.items():
        directory = ROOT / "controls" / "slots" / category
        state_files = []
        for column, state in enumerate(STATES):
            cell = fit_sprite(crop_grid(source, 4, 6, column, 5), (80, 80), padding=1)
            overlay = Image.new("RGBA", cell.size, (*tint, 0))
            tint_strength = 26 if state != "disabled" else 10
            overlay.putalpha(cell.getchannel("A").point(lambda value: int(value * tint_strength / 255)))
            cell = Image.alpha_composite(cell, overlay)
            path = directory / f"slot-{category}_{state}.png"
            cell.save(path)
            files.append(path)
            state_files.append(path)
        save_gif(state_files[:3] + [state_files[0]], ANIMATION_PREVIEWS / f"slot-{category}-press-preview.gif")
    return files


def build_frames() -> list[Path]:
    source = open_rgba("frames-source-v1.png")
    grid = [
        (0, 0, "panels", "panel-square-stone", (320, 320)),
        (0, 1, "panels", "panel-wide-stone-wood", (480, 288)),
        (0, 2, "panels", "panel-tall-stone", (288, 480)),
        (0, 3, "panels", "panel-purple-header", (420, 260)),
        (1, 0, "modals", "modal-confirmation-compact", (420, 240)),
        (1, 1, "modals", "modal-dialog-medium", (480, 300)),
        (1, 2, "modals", "modal-choice-large", (560, 360)),
        (1, 3, "modals", "modal-warning", (440, 260)),
        (2, 0, "tooltips", "tooltip-black-gold-small", (300, 144)),
        (2, 1, "tooltips", "tooltip-parchment-medium", (320, 176)),
        (2, 2, "tooltips", "tooltip-item-detail-tall", (280, 400)),
        (2, 3, "tooltips", "tooltip-notification-wide", (420, 112)),
    ]
    files: list[Path] = []
    for column, row, category, stem, size in grid:
        cell = crop_grid(source, 3, 4, column, row)
        cell = fit_sprite(cell, size, padding=2)
        path = ROOT / "frames" / category / f"{stem}.png"
        cell.save(path)
        files.append(path)
    return files


SYSTEM_NAMES = [
    "lock", "unlock", "valid", "invalid", "drag", "pickup",
    "delete", "sort", "info", "warning", "help", "save",
]
RESOURCE_NAMES = [
    "health", "energy", "food", "gold", "weight", "experience",
    "armor", "time", "day", "movement", "danger", "weather",
]
CLASS_NAMES = [
    "fighter", "rogue", "wizard", "cleric", "ranger", "paladin", "barbarian", "bard",
    "druid", "monk", "sorcerer", "warlock", "artificer", "necromancer", "shaman", "knight",
]
RACE_NAMES = [
    "human", "elf", "dwarf", "halfling", "gnome", "half-elf",
    "drow", "half-orc", "tiefling", "dragonborn", "githyanki", "custom",
]
STATUS_NAMES = [
    "bleeding", "burning", "frozen", "poisoned", "stunned", "blinded",
    "frightened", "charmed", "sleeping", "silenced", "slowed", "haste",
    "strengthened", "weakened", "armored", "exposed", "regeneration", "cursed",
    "blessed", "invisible", "rooted", "wet", "shocked", "concentrating",
]
CURSOR_NAMES = [
    "default", "pointer", "inspect", "move", "attack", "ranged",
    "magic", "pickup", "drag", "forbidden", "talk", "wait",
]


def build_icon_grid(
    source_name: str,
    columns: int,
    rows: int,
    names: list[str],
    directory: Path,
    prefix: str,
    size: tuple[int, int] = (64, 64),
    clean_background: bool = False,
) -> list[Path]:
    source = open_rgba(source_name)
    files: list[Path] = []
    for index, name in enumerate(names):
        cell = crop_grid(source, columns, rows, index % columns, index // columns)
        if clean_background:
            cell = grabcut_foreground(cell)
        base = fit_sprite(cell, size, padding=5)
        if name == "invisible" and np.array(base.getchannel("A")).max() < 24:
            base = draw_invisible_icon(size)
        files.extend(save_states(base, directory, f"{prefix}-{name}"))
    return files


def draw_invisible_icon(size: tuple[int, int]) -> Image.Image:
    image = Image.new("RGBA", size, (0, 0, 0, 0))
    draw = ImageDraw.Draw(image)
    w, h = size
    color = (181, 132, 255, 205)
    draw.ellipse((w * 0.2, h * 0.28, w * 0.8, h * 0.66), outline=color, width=max(2, w // 18))
    draw.ellipse((w * 0.43, h * 0.42, w * 0.57, h * 0.56), fill=(223, 205, 255, 225))
    for offset in (-10, 0, 10):
        draw.line((w * 0.23 + offset, h * 0.75, w * 0.38 + offset, h * 0.61), fill=color, width=2)
    return image


def build_icons() -> list[Path]:
    files: list[Path] = []
    files += build_icon_grid(
        "system-icons-source-v1.png", 6, 4, SYSTEM_NAMES,
        ROOT / "icons" / "system", "icon", clean_background=True,
    )
    # Resource icons start at row three in the shared 6x4 source atlas.
    source = open_rgba("system-icons-source-v1.png")
    for index, name in enumerate(RESOURCE_NAMES):
        source_index = index + 12
        cell = crop_grid(source, 6, 4, source_index % 6, source_index // 6)
        base = fit_sprite(grabcut_foreground(cell), (64, 64), padding=5)
        files += save_states(base, ROOT / "icons" / "resources", f"resource-{name}")

    files += build_icon_grid(
        "class-icons-source-v1.png", 4, 4, CLASS_NAMES,
        ROOT / "icons" / "classes", "class", size=(72, 72),
    )
    files += build_icon_grid(
        "race-icons-source-v1.png", 4, 3, RACE_NAMES,
        ROOT / "icons" / "races", "race", size=(80, 80),
    )
    files += build_icon_grid(
        "status-icons-source-v1.png", 6, 4, STATUS_NAMES,
        ROOT / "icons" / "status_effects", "status", clean_background=True,
    )
    return sorted(set(files))


def draw_scrollbar(vertical: bool, state: str) -> Image.Image:
    size = (28, 196) if vertical else (196, 28)
    image = Image.new("RGBA", size, (0, 0, 0, 0))
    draw = ImageDraw.Draw(image)
    stone = (42, 42, 48, 255) if state != "disabled" else (68, 68, 72, 210)
    gold = (216, 145, 24, 255) if state != "disabled" else (110, 110, 110, 200)
    purple = (92, 26, 116, 255)
    draw.rounded_rectangle((1, 1, size[0] - 2, size[1] - 2), radius=4, fill=stone, outline=(11, 11, 14, 255), width=2)
    draw.rounded_rectangle((4, 4, size[0] - 5, size[1] - 5), radius=3, outline=gold, width=2)
    if vertical:
        thumb = (6, 58, size[0] - 7, 118)
    else:
        thumb = (58, 6, 118, size[1] - 7)
    fill = purple if state not in ("pressed", "disabled") else ((60, 18, 74, 255) if state == "pressed" else (80, 80, 85, 210))
    draw.rounded_rectangle(thumb, radius=4, fill=fill, outline=gold, width=2)
    if state == "hover":
        image = gold_glow(image)
        alpha = image.getchannel("A")
        alpha_draw = ImageDraw.Draw(alpha)
        alpha_draw.rectangle((0, 0, size[0] - 1, size[1] - 1), outline=0, width=2)
        image.putalpha(alpha)
    if state == "pressed":
        image = pressed(image, 1)
    return image


def build_scrollbars() -> list[Path]:
    files: list[Path] = []
    for orientation, vertical in (("vertical", True), ("horizontal", False)):
        state_files = []
        for state in STATES:
            image = draw_scrollbar(vertical, state)
            path = ROOT / "controls" / "scrollbars" / orientation / f"scrollbar-{orientation}_{state}.png"
            image.save(path)
            files.append(path)
            state_files.append(path)
        save_gif(state_files[:3] + [state_files[0]], ANIMATION_PREVIEWS / f"scrollbar-{orientation}-press-preview.gif")
    return files


def find_font(size: int) -> ImageFont.FreeTypeFont | ImageFont.ImageFont:
    candidates = [
        Path("C:/Windows/Fonts/consolab.ttf"),
        Path("C:/Windows/Fonts/consola.ttf"),
        Path("C:/Windows/Fonts/lucon.ttf"),
    ]
    for candidate in candidates:
        if candidate.exists():
            return ImageFont.truetype(candidate, size=size)
    return ImageFont.load_default()


def draw_keycap(label: str, width: int, state: str) -> Image.Image:
    image = Image.new("RGBA", (width, 44), (0, 0, 0, 0))
    draw = ImageDraw.Draw(image)
    gold = (228, 160, 34, 255) if state != "disabled" else (112, 112, 118, 220)
    fill = (77, 24, 94, 255) if state == "normal" else (58, 17, 70, 255)
    if state == "disabled":
        fill = (58, 58, 62, 220)
    y = 3 if state == "pressed" else 1
    draw.rounded_rectangle((1, y, width - 2, 40 + y), radius=5, fill=(25, 25, 30, 255), outline=(8, 8, 10, 255), width=2)
    draw.rounded_rectangle((4, 4 + y, width - 5, 36 + y), radius=4, fill=fill, outline=gold, width=2)
    font = find_font(18 if len(label) <= 3 else 13)
    box = draw.textbbox((0, 0), label, font=font)
    tx = (width - (box[2] - box[0])) // 2
    ty = y + (38 - (box[3] - box[1])) // 2 - box[1]
    draw.text((tx, ty), label, font=font, fill=(245, 238, 214, 255))
    return image


def build_input_prompts() -> list[Path]:
    files: list[Path] = []
    keys = {
        "w": 44, "a": 44, "s": 44, "d": 44,
        "up": 44, "down": 44, "left": 44, "right": 44,
        "enter": 86, "escape": 86, "space": 140, "tab": 72,
        "shift": 86, "ctrl": 72, "alt": 64,
        "inventory": 44, "character": 44, "journal": 44, "camp": 44,
    }
    labels = {
        "up": "↑", "down": "↓", "left": "←", "right": "→",
        "enter": "ENTER", "escape": "ESC", "space": "SPACE", "tab": "TAB",
        "shift": "SHIFT", "ctrl": "CTRL", "alt": "ALT",
        "inventory": "I", "character": "C", "journal": "J", "camp": "K",
    }
    for name, width in keys.items():
        state_files = []
        for state in STATES:
            image = draw_keycap(labels.get(name, name.upper()), width, state)
            path = ROOT / "input" / "keyboard" / f"key-{name}_{state}.png"
            image.save(path)
            files.append(path)
            state_files.append(path)
        save_gif(state_files[:3] + [state_files[0]], ANIMATION_PREVIEWS / f"key-{name}-press-preview.gif")

    mouse_actions = ["left-click", "right-click", "middle-click", "wheel-up", "wheel-down", "move"]
    for index, name in enumerate(mouse_actions):
        base = Image.new("RGBA", (56, 72), (0, 0, 0, 0))
        draw = ImageDraw.Draw(base)
        draw.rounded_rectangle((11, 4, 45, 64), radius=15, fill=(42, 42, 48, 255), outline=(220, 151, 28, 255), width=3)
        draw.line((28, 5, 28, 31), fill=(220, 151, 28, 255), width=2)
        draw.line((12, 31, 44, 31), fill=(220, 151, 28, 255), width=2)
        if name == "left-click":
            draw.pieslice((14, 8, 28, 28), 180, 360, fill=(118, 33, 143, 255))
        elif name == "right-click":
            draw.pieslice((28, 8, 42, 28), 180, 360, fill=(118, 33, 143, 255))
        elif name == "middle-click":
            draw.rounded_rectangle((24, 10, 32, 25), radius=3, fill=(118, 33, 143, 255))
        elif name in ("wheel-up", "wheel-down"):
            draw.polygon([(28, 8 if name == "wheel-up" else 26), (21, 18), (35, 18)], fill=(246, 194, 62, 255))
        else:
            draw.line((5, 36, 51, 36), fill=(246, 194, 62, 255), width=3)
            draw.line((28, 13, 28, 59), fill=(246, 194, 62, 255), width=3)
        files += save_states(base, ROOT / "input" / "mouse", f"mouse-{name}")
    return files


def build_cursors() -> list[Path]:
    source = open_rgba("cursor-icons-source-v1.png")
    files: list[Path] = []
    cursor_manifest = {}
    for index, name in enumerate(CURSOR_NAMES):
        cell = crop_grid(source, 4, 3, index % 4, index // 4)
        base = fit_sprite(cell, (48, 48), padding=4)
        frame_paths: list[Path] = []
        for frame_index, (dy, light) in enumerate(((0, 1.0), (1, 1.08), (2, 1.14), (1, 1.06))):
            frame = brightness(base, light)
            canvas = Image.new("RGBA", base.size, (0, 0, 0, 0))
            canvas.alpha_composite(frame, (0, dy))
            path = ROOT / "cursors" / f"cursor-{name}_f{frame_index:02d}.png"
            canvas.save(path)
            files.append(path)
            frame_paths.append(path)
        save_gif(frame_paths, ANIMATION_PREVIEWS / f"cursor-{name}-preview.gif")
        cursor_manifest[name] = {
            "frames": [path.relative_to(ROOT).as_posix() for path in frame_paths],
            "frame_duration_ms": 90,
            "hotspot": [2, 2] if name == "default" else [24, 24],
        }
    (MANIFESTS / "cursor-animations-v1.json").write_text(
        json.dumps(cursor_manifest, ensure_ascii=False, indent=2), encoding="utf-8"
    )
    return files


def pack_atlas(name: str, paths: list[Path], max_width: int = 2048) -> tuple[Path, Path]:
    paths = sorted(set(paths), key=lambda path: path.as_posix())
    padding = 2
    x = padding
    y = padding
    row_height = 0
    placements = []
    for path in paths:
        image = Image.open(path).convert("RGBA")
        if x + image.width + padding > max_width:
            x = padding
            y += row_height + padding
            row_height = 0
        placements.append((path, image, x, y))
        x += image.width + padding
        row_height = max(row_height, image.height)
    height = y + row_height + padding
    atlas = Image.new("RGBA", (max_width, max(8, height)), (0, 0, 0, 0))
    manifest = {"atlas": f"ui-{name}-atlas-v1.png", "sprites": {}}
    for path, image, px, py in placements:
        atlas.alpha_composite(image, (px, py))
        manifest["sprites"][path.stem] = {
            "x": px, "y": py, "width": image.width, "height": image.height,
            "source": path.relative_to(ROOT).as_posix(),
        }
    atlas_path = ATLASES / f"ui-{name}-atlas-v1.png"
    json_path = ATLASES / f"ui-{name}-atlas-v1.json"
    atlas.save(atlas_path)
    json_path.write_text(json.dumps(manifest, ensure_ascii=False, indent=2), encoding="utf-8")
    return atlas_path, json_path


def make_preview(groups: dict[str, list[Path]]) -> Path:
    canvas = Image.new("RGB", (1600, 1200), (18, 16, 22))
    draw = ImageDraw.Draw(canvas)
    title_font = find_font(30)
    label_font = find_font(18)
    draw.text((32, 22), "RETRO FANTASY UI ASSET PACK V1", font=title_font, fill=(244, 192, 61))

    def normal_only(paths: list[Path]) -> list[Path]:
        return [path for path in sorted(set(paths)) if path.stem.endswith("_normal") or path.stem.endswith("_f00")]

    def place_section(title: str, paths: list[Path], origin: tuple[int, int], width: int, limit: int = 24) -> None:
        gx, gy = origin
        draw.text((gx, gy), title, font=label_font, fill=(220, 220, 225))
        x, y = gx, gy + 32
        row_height = 0
        for path in paths[:limit]:
            image = Image.open(path).convert("RGBA")
            if x + image.width > gx + width:
                x = gx
                y += row_height + 10
                row_height = 0
            canvas.paste(image, (x, y), image)
            x += image.width + 8
            row_height = max(row_height, image.height)

    control_showcase = []
    for stem in ("button-large", "button-medium", "button-small", "button-icon", "tab-primary", "slot-item"):
        control_showcase.extend(sorted(path for path in groups["controls"] if path.stem.startswith(stem)))
    place_section("CONTROLS — NORMAL / HOVER / PRESSED / DISABLED", control_showcase, (32, 86), 1530, 24)
    place_section("SYSTEM + RESOURCES", normal_only(groups["system"]), (32, 370), 500, 24)
    place_section("CLASSES", normal_only(groups["classes"]), (560, 370), 500, 16)
    place_section("RACES", normal_only(groups["races"]), (1090, 370), 470, 12)
    place_section("STATUS EFFECTS", normal_only(groups["status"]), (32, 720), 750, 24)
    place_section("CURSORS", normal_only(groups["cursors"]), (810, 720), 350, 12)
    place_section("INPUT", normal_only(groups["input"]), (1190, 720), 370, 18)
    path = PREVIEWS / "ui-asset-pack-v1-preview.png"
    canvas.save(path)
    return path


def write_master_manifest(all_files: list[Path], atlas_files: list[Path]) -> Path:
    entries = []
    for path in sorted(set(all_files)):
        image = Image.open(path)
        stem = path.stem
        state = next((state for state in STATES if stem.endswith(f"_{state}")), None)
        entries.append({
            "id": stem,
            "path": path.relative_to(ROOT).as_posix(),
            "width": image.width,
            "height": image.height,
            "state": state,
        })
    manifest = {
        "pack": "retro-fantasy-ui",
        "version": 1,
        "target_canvas": [800, 800],
        "pixel_scale": 1,
        "naming": "lowercase-kebab-case with _normal/_hover/_pressed/_disabled state suffix",
        "interaction_states": list(STATES),
        "recommended_press_animation_ms": [70, 90, 70],
        "atlases": [path.relative_to(ROOT).as_posix() for path in atlas_files],
        "assets": entries,
    }
    path = MANIFESTS / "ui-assets-manifest-v1.json"
    path.write_text(json.dumps(manifest, ensure_ascii=False, indent=2), encoding="utf-8")
    return path


def main() -> None:
    ensure_dirs()
    controls = build_core_controls() + build_scrollbars()
    frames = build_frames()
    icons = build_icons()
    cursors = build_cursors()
    inputs = build_input_prompts()

    groups = {
        "controls": controls,
        "frames": frames,
        "system": [path for path in icons if "icons/system" in path.as_posix() or "icons/resources" in path.as_posix()],
        "classes": [path for path in icons if "icons/classes" in path.as_posix()],
        "races": [path for path in icons if "icons/races" in path.as_posix()],
        "status": [path for path in icons if "icons/status_effects" in path.as_posix()],
        "cursors": cursors,
        "input": inputs,
    }

    atlas_outputs: list[Path] = []
    for name, paths in groups.items():
        png, metadata = pack_atlas(name, paths)
        atlas_outputs.extend((png, metadata))

    all_files = controls + frames + icons + cursors + inputs
    preview = make_preview(groups)
    manifest = write_master_manifest(all_files, atlas_outputs)
    print(f"Built {len(set(all_files))} PNG assets")
    print(f"Preview: {preview}")
    print(f"Manifest: {manifest}")


if __name__ == "__main__":
    main()
