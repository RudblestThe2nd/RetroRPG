from __future__ import annotations

import json
from pathlib import Path

from PIL import Image


GAME = Path(r"C:\Users\VECTUS\Documents\pc\game_design")
ROOT = GAME / "assets" / "final-production-v2"


def ensure(path: Path) -> Path:
    path.mkdir(parents=True, exist_ok=True)
    return path


def grid_bounds(length: int, count: int) -> list[int]:
    return [round(i * length / count) for i in range(count + 1)]


def split_grid(source: Path, cols: int, rows: int, output_size: tuple[int, int], targets: list[Path]) -> None:
    image = Image.open(source).convert("RGBA")
    xs = grid_bounds(image.width, cols)
    ys = grid_bounds(image.height, rows)
    if len(targets) != cols * rows:
        raise ValueError(f"Target count mismatch for {source}")
    for index, target in enumerate(targets):
        row, col = divmod(index, cols)
        crop = image.crop((xs[col], ys[row], xs[col + 1], ys[row + 1]))
        crop = crop.resize(output_size, Image.Resampling.LANCZOS)
        ensure(target.parent)
        crop.save(target, optimize=True)


def write_json(path: Path, data: dict) -> None:
    ensure(path.parent)
    path.write_text(json.dumps(data, ensure_ascii=False, indent=2), encoding="utf-8")


def make_gif(frames: list[Path], output: Path, duration: int) -> None:
    images = [Image.open(path).convert("RGBA") for path in frames]
    ensure(output.parent)
    images[0].save(
        output,
        save_all=True,
        append_images=images[1:],
        duration=duration,
        loop=0,
        disposal=2,
        transparency=0,
    )


def build_dice() -> dict:
    source_dir = ROOT / "combat" / "dice" / "source"
    runtime_dir = ensure(ROOT / "combat" / "dice" / "runtime-256")
    dice_names = ["d4", "d6", "d8", "d10", "d12", "d20"]
    dice_targets = [runtime_dir / f"{name}-icon-v1.png" for name in dice_names]
    split_grid(source_dir / "dice-set-source-v1.png", 3, 2, (256, 256), dice_targets)

    animation_dir = ensure(ROOT / "combat" / "dice" / "d20-roll-v1")
    animation_targets = [animation_dir / f"d20-roll-{index:02d}-v1.png" for index in range(8)]
    split_grid(source_dir / "d20-roll-animation-source-v1.png", 4, 2, (256, 256), animation_targets)
    gif_path = ROOT / "combat" / "dice" / "animations" / "d20-roll-v1.gif"
    make_gif(animation_targets, gif_path, 90)

    manifest = {
        "id": "dice-runtime-v1",
        "status": "complete",
        "dice": [
            {"id": name, "sides": int(name[1:]), "file": str(path.relative_to(ROOT)).replace("\\", "/")}
            for name, path in zip(dice_names, dice_targets)
        ],
        "d20_roll": {
            "frames": [str(path.relative_to(ROOT)).replace("\\", "/") for path in animation_targets],
            "gif": str(gif_path.relative_to(ROOT)).replace("\\", "/"),
            "fps": 11,
            "loop": False,
        },
    }
    write_json(ROOT / "combat" / "dice" / "manifests" / "dice-runtime-v1.json", manifest)
    return {"icons": len(dice_targets), "d20_frames": len(animation_targets), "animations": 1}


def build_indicators() -> dict:
    source = ROOT / "ui" / "combat-indicators" / "source" / "combat-indicators-source-v1.png"
    names = [
        "movement", "action", "bonus-action", "reaction",
        "advantage", "disadvantage", "armor-class", "saving-throw",
        "concentration", "death-save-success", "death-save-failure", "initiative",
    ]
    runtime_dir = ensure(ROOT / "ui" / "combat-indicators" / "runtime-128")
    targets = [runtime_dir / f"{name}-icon-v1.png" for name in names]
    split_grid(source, 4, 3, (128, 128), targets)
    manifest = {
        "id": "combat-indicators-v1",
        "status": "complete",
        "icons": [
            {"id": name, "file": str(path.relative_to(ROOT)).replace("\\", "/")}
            for name, path in zip(names, targets)
        ],
    }
    write_json(ROOT / "ui" / "combat-indicators" / "manifests" / "combat-indicators-v1.json", manifest)
    return {"icons": len(targets)}


def build_death_effect() -> dict:
    source = ROOT / "effects" / "combat-reactions" / "source" / "enemy-death-dissolve-source-v1.png"
    frame_dir = ensure(ROOT / "effects" / "combat-reactions" / "enemy-death-dissolve-v1")
    targets = [frame_dir / f"enemy-death-dissolve-{index:02d}-v1.png" for index in range(8)]
    split_grid(source, 4, 2, (256, 256), targets)
    gif_path = ROOT / "effects" / "combat-reactions" / "animations" / "enemy-death-dissolve-v1.gif"
    make_gif(targets, gif_path, 110)
    manifest = {
        "id": "enemy-death-dissolve-v1",
        "status": "complete",
        "pivot": [128, 224],
        "fps": 9,
        "loop": False,
        "frames": [str(path.relative_to(ROOT)).replace("\\", "/") for path in targets],
        "gif": str(gif_path.relative_to(ROOT)).replace("\\", "/"),
    }
    write_json(ROOT / "effects" / "combat-reactions" / "manifests" / "enemy-death-dissolve-v1.json", manifest)
    return {"frames": len(targets), "animations": 1}


def build_weapons() -> dict:
    weapons = ["dagger", "rapier", "mace", "warhammer", "greatsword", "light-crossbow"]
    directions = ["south", "west", "east", "north"]
    entries: list[dict] = []
    for weapon in weapons:
        targets: list[Path] = []
        for direction in directions:
            target = ROOT / "equipment" / "weapons-extended" / "runtime-256" / weapon / direction / f"{weapon}-{direction}-v1.png"
            targets.append(target)
            entries.append({
                "id": f"{weapon}-{direction}",
                "weapon": weapon,
                "direction": direction,
                "file": str(target.relative_to(ROOT)).replace("\\", "/"),
                "anchor": [128, 150],
            })
        source = ROOT / "equipment" / "weapons-extended" / "source" / "per-weapon" / f"{weapon}-directions-source-v1.png"
        split_grid(source, 2, 2, (256, 256), targets)
    manifest = {
        "id": "extended-weapons-directions-v1",
        "status": "complete",
        "animation_note": "Direction layers are final assets. Idle and attack motion is applied by code using anchor transforms.",
        "weapons": entries,
    }
    write_json(ROOT / "equipment" / "weapons-extended" / "manifests" / "extended-weapons-directions-v1.json", manifest)
    return {"weapon_types": len(weapons), "direction_layers": len(entries)}


def build_combat_screen() -> dict:
    screens = [
        ROOT / "ui" / "screens" / "runtime-800" / "combat-screen-d20-v1.png",
        ROOT / "ui" / "screens" / "runtime-800" / "combat-screen-simple-d20-v1.png",
    ]
    for screen in screens:
        image = Image.open(screen).convert("RGB")
        image = image.resize((800, 800), Image.Resampling.LANCZOS)
        image.save(screen, optimize=True)
    return {
        "screens": len(screens),
        "selected": "ui/screens/runtime-800/combat-screen-simple-d20-v1.png",
        "resolution": [800, 800],
    }


def audit() -> dict:
    errors: list[str] = []
    checked = 0
    for path in ROOT.rglob("*.png"):
        checked += 1
        image = Image.open(path)
        if "source" not in path.parts and "screens" not in path.parts:
            if image.mode != "RGBA":
                errors.append(f"Missing RGBA: {path.relative_to(ROOT)}")
            elif image.getchannel("A").getextrema()[0] != 0:
                errors.append(f"Missing transparent pixels: {path.relative_to(ROOT)}")
    for screen in (ROOT / "ui" / "screens" / "runtime-800").glob("*.png"):
        if Image.open(screen).size != (800, 800):
            errors.append(f"Combat screen is not 800x800: {screen.name}")
    report = {"status": "pass" if not errors else "fail", "checked_png": checked, "errors": errors}
    write_json(ROOT / "manifests" / "qa-report-v2.json", report)
    return report


def main() -> None:
    summary = {
        "dice": build_dice(),
        "combat_indicators": build_indicators(),
        "enemy_death_effect": build_death_effect(),
        "extended_weapons": build_weapons(),
        "combat_screen": build_combat_screen(),
    }
    report = audit()
    final_manifest = {
        "id": "final-production-v2",
        "status": "complete" if report["status"] == "pass" else "needs-review",
        "asset_policy": "All files are production game assets, not concepts or temporary placeholders.",
        "combat_scope": "Simple d20 roll system, not a complete tabletop rules implementation.",
        "summary": summary,
        "qa": report,
    }
    write_json(ROOT / "manifests" / "final-production-v2.json", final_manifest)
    print(json.dumps(final_manifest, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
