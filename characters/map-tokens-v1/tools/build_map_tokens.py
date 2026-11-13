from __future__ import annotations

import json
import math
from pathlib import Path

from PIL import Image, ImageDraw


TOKEN = 96
INNER = 72
BIOMES = ["forest", "swamp", "desert", "snow", "mountain", "tropical", "volcanic", "corrupted", "underground", "plains"]
FRAME_COLORS = {
    "player": ((230, 190, 72, 255), (54, 88, 128, 255)),
    "npc": ((185, 142, 82, 255), (72, 60, 50, 255)),
    "enemy": ((172, 54, 48, 255), (67, 38, 38, 255)),
    "mini-boss": ((171, 78, 193, 255), (66, 35, 78, 255)),
    "main-boss": ((242, 126, 47, 255), (83, 30, 27, 255)),
}


def fit_sprite(source: Image.Image) -> Image.Image:
    source = source.convert("RGBA")
    bbox = source.getchannel("A").getbbox()
    if bbox is None:
        return Image.new("RGBA", (1, 1), (0, 0, 0, 0))
    crop = source.crop(bbox)
    scale = min(INNER / crop.width, INNER / crop.height)
    size = (max(1, round(crop.width * scale)), max(1, round(crop.height * scale)))
    return crop.resize(size, Image.Resampling.NEAREST)


def make_token(source: Image.Image, token_class: str) -> Image.Image:
    outer, inner = FRAME_COLORS[token_class]
    token = Image.new("RGBA", (TOKEN, TOKEN), (0, 0, 0, 0))
    draw = ImageDraw.Draw(token)
    draw.ellipse((2, 2, TOKEN - 3, TOKEN - 3), fill=(13, 15, 19, 245), outline=outer, width=4)
    draw.ellipse((8, 8, TOKEN - 9, TOKEN - 9), fill=inner, outline=(232, 221, 184, 150), width=2)
    fitted = fit_sprite(source)
    x = (TOKEN - fitted.width) // 2
    y = 10 + max(0, INNER - fitted.height)
    token.alpha_composite(fitted, (x, y))
    draw.ellipse((2, 2, TOKEN - 3, TOKEN - 3), outline=outer, width=4)
    return token


def write_atlas(records: list[dict], root: Path, atlas_id: str, cols: int) -> str:
    rows = math.ceil(len(records) / cols)
    atlas = Image.new("RGBA", (cols * TOKEN, rows * TOKEN), (0, 0, 0, 0))
    for index, record in enumerate(records):
        token = Image.open(root / record["file"]).convert("RGBA")
        atlas.alpha_composite(token, ((index % cols) * TOKEN, (index // cols) * TOKEN))
        record["atlas"] = f"atlases/{atlas_id}.png"
        record["atlas_cell"] = [index % cols, index // cols]
    path = root / "atlases" / f"{atlas_id}.png"
    path.parent.mkdir(parents=True, exist_ok=True)
    atlas.save(path)
    return path.relative_to(root).as_posix()


def main() -> None:
    characters = Path(__file__).resolve().parents[2]
    root = Path(__file__).resolve().parents[1]
    tokens_dir = root / "tokens"
    manifests_dir = root / "manifests"
    previews_dir = root / "previews"
    for folder in (tokens_dir, manifests_dir, previews_dir):
        folder.mkdir(parents=True, exist_ok=True)

    records: list[dict] = []
    errors: list[str] = []

    def add(source_path: Path, token_id: str, category: str, token_class: str, biome: str | None = None, source_crop: bool = False) -> None:
        source = Image.open(source_path).convert("RGBA")
        if source_crop:
            source = source.crop((0, 0, 256, 256))
        token = make_token(source, token_class)
        category_dir = tokens_dir / category
        category_dir.mkdir(parents=True, exist_ok=True)
        path = category_dir / f"{token_id}-token-v1.png"
        token.save(path)
        if token.size != (TOKEN, TOKEN) or token.getchannel("A").getbbox() is None:
            errors.append(f"Invalid token: {path.relative_to(root).as_posix()}")
        records.append({
            "id": token_id,
            "category": category,
            "token_class": token_class,
            "biome": biome,
            "file": path.relative_to(root).as_posix(),
            "source": source_path.relative_to(characters).as_posix(),
            "size": [TOKEN, TOKEN],
            "pivot": [48, 48],
        })

    for race_dir in sorted((characters / "face-variants-v1").iterdir()):
        if not race_dir.is_dir():
            continue
        for gender in ("female", "male"):
            portraits = sorted((race_dir / "portraits" / gender).glob("*-face-*-v1.png"))
            if portraits:
                add(portraits[0], f"player-{race_dir.name}-{gender}", "players", "player")

    npc_paths = sorted((characters / "npc-sprites-v1" / "sprites").glob("*/*/*-npc-v1.png"))
    for path in npc_paths:
        add(path, path.stem.removesuffix("-npc-v1"), "npcs", "npc")

    for biome in BIOMES:
        enemy_paths = sorted((characters / "biome-enemies-v1" / biome / "sprites").glob("*/*-idle-v1.png"))
        for path in enemy_paths:
            enemy_id = path.parent.name
            add(path, f"enemy-{biome}-{enemy_id}", f"enemies/{biome}", "enemy", biome=biome)

    boss_class_by_file: dict[str, str] = {}
    for manifest_path in sorted((characters / "boss-sprites-v1" / "manifests").glob("boss-pack-group-*-v1.json")):
        data = json.loads(manifest_path.read_text(encoding="utf-8"))
        for sprite in data["sprites"]:
            if sprite["pose"] == "idle":
                boss_class_by_file[sprite["file"]] = sprite["boss_class"]
    boss_paths = sorted((characters / "boss-sprites-v1" / "sprites").glob("*/*/*-idle-v1.png"))
    boss_root = characters / "boss-sprites-v1"
    for path in boss_paths:
        relative = path.relative_to(boss_root).as_posix()
        boss_class = boss_class_by_file[relative]
        biome = path.parents[1].name
        boss_id = path.parent.name
        add(path, f"boss-{biome}-{boss_id}", "bosses", f"{boss_class}-boss", biome=biome)

    groups = {
        "players": [r for r in records if r["category"] == "players"],
        "npcs": [r for r in records if r["category"] == "npcs"],
        "bosses": [r for r in records if r["category"] == "bosses"],
    }
    atlases = [
        write_atlas(groups["players"], root, "player-tokens-atlas-v1", 6),
        write_atlas(groups["npcs"], root, "npc-tokens-atlas-v1", 8),
        write_atlas(groups["bosses"], root, "boss-tokens-atlas-v1", 5),
    ]
    for biome in BIOMES:
        group = [r for r in records if r["category"] == f"enemies/{biome}"]
        atlases.append(write_atlas(group, root, f"enemy-tokens-{biome}-atlas-v1", 4))

    preview_cols = 15
    preview_rows = math.ceil(len(records) / preview_cols)
    preview = Image.new("RGBA", (preview_cols * TOKEN, preview_rows * TOKEN), (27, 29, 35, 255))
    for index, record in enumerate(records):
        token = Image.open(root / record["file"]).convert("RGBA")
        preview.alpha_composite(token, ((index % preview_cols) * TOKEN, (index // preview_cols) * TOKEN))
    preview_path = previews_dir / "map-tokens-v1-preview.png"
    preview.convert("RGB").save(preview_path)

    counts = {
        "players": len(groups["players"]),
        "npcs": len(groups["npcs"]),
        "enemies": sum(1 for r in records if r["token_class"] == "enemy"),
        "mini_bosses": sum(1 for r in records if r["token_class"] == "mini-boss"),
        "main_bosses": sum(1 for r in records if r["token_class"] == "main-boss"),
    }
    manifest = {
        "id": "map-tokens-v1",
        "status": "complete" if not errors else "qa_failed",
        "token_size": [TOKEN, TOKEN],
        "pivot": [48, 48],
        "counts": counts,
        "total_tokens": len(records),
        "atlases": atlases,
        "preview": preview_path.relative_to(root).as_posix(),
        "tokens": records,
    }
    (manifests_dir / "map-tokens-v1.json").write_text(
        json.dumps(manifest, ensure_ascii=False, indent=2) + "\n", encoding="utf-8"
    )
    qa = {"status": "pass" if not errors else "fail", "checked": len(records), "errors": errors}
    (manifests_dir / "qa-report-v1.json").write_text(
        json.dumps(qa, ensure_ascii=False, indent=2) + "\n", encoding="utf-8"
    )
    print(json.dumps({"qa": qa, "counts": counts, "atlases": len(atlases)}, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
