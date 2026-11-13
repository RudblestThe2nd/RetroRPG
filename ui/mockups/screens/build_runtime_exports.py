from __future__ import annotations

import json
from pathlib import Path

from PIL import Image, ImageFilter


ROOT = Path(__file__).resolve().parent
SOURCE = ROOT / "source-highres"
RUNTIME = ROOT / "runtime-800"
MANIFESTS = ROOT / "manifests"


SCREEN_DESCRIPTIONS = {
    "combat-screen-v1.png": "Turn-based combat screen",
    "inventory-screen-v1.png": "Inventory, equipment and item requirement screen",
    "character-screen-v1.png": "Character sheet, attributes and skills screen",
    "journal-screen-v1.png": "Quest journal and world notes screen",
    "camp-screen-v1.png": "Camp management and night event screen",
}


def main() -> None:
    RUNTIME.mkdir(parents=True, exist_ok=True)
    MANIFESTS.mkdir(parents=True, exist_ok=True)
    entries = []

    for filename, description in SCREEN_DESCRIPTIONS.items():
        source_path = SOURCE / filename
        target_path = RUNTIME / filename
        image = Image.open(source_path).convert("RGB")
        original_size = list(image.size)
        runtime = image.resize((800, 800), Image.Resampling.LANCZOS)
        runtime = runtime.filter(ImageFilter.UnsharpMask(radius=0.7, percent=115, threshold=3))
        runtime.save(target_path, optimize=True)
        entries.append(
            {
                "id": filename.removesuffix(".png"),
                "description": description,
                "source": source_path.relative_to(ROOT).as_posix(),
                "runtime": target_path.relative_to(ROOT).as_posix(),
                "source_size": original_size,
                "runtime_size": [800, 800],
            }
        )

    manifest = {
        "pack": "core-game-screens",
        "version": 1,
        "target_resolution": [800, 800],
        "screens": entries,
    }
    (MANIFESTS / "core-game-screens-v1.json").write_text(
        json.dumps(manifest, ensure_ascii=False, indent=2), encoding="utf-8"
    )
    print(f"Exported {len(entries)} screens at 800x800")


if __name__ == "__main__":
    main()
