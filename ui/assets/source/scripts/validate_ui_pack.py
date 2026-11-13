from __future__ import annotations

import json
import re
from collections import defaultdict
from pathlib import Path

import numpy as np
from PIL import Image


ROOT = Path(__file__).resolve().parents[2]
REPORT = ROOT / "manifests" / "ui-assets-qa-report-v1.json"
STATES = ("normal", "hover", "pressed", "disabled")
NAME_PATTERN = re.compile(r"^[a-z0-9]+(?:-[a-z0-9]+)*(?:_(?:normal|hover|pressed|disabled|f\d{2}))?\.png$")


def main() -> None:
    png_files = [
        path for path in ROOT.rglob("*.png")
        if "source/imagegen" not in path.as_posix()
        and "/atlases/" not in path.as_posix()
        and "/previews/" not in path.as_posix()
    ]
    errors: list[str] = []
    warnings: list[str] = []
    dimensions: dict[str, set[tuple[int, int]]] = defaultdict(set)
    state_groups: dict[str, set[str]] = defaultdict(set)

    for path in png_files:
        relative = path.relative_to(ROOT).as_posix()
        if not NAME_PATTERN.match(path.name):
            errors.append(f"Invalid filename: {relative}")
        image = Image.open(path).convert("RGBA")
        alpha = np.asarray(image.getchannel("A"))
        if int(alpha.max()) == 0:
            errors.append(f"Blank asset: {relative}")
        if any(image.getpixel(point)[3] > 8 for point in ((0, 0), (image.width - 1, 0), (0, image.height - 1), (image.width - 1, image.height - 1))):
            warnings.append(f"Opaque corner: {relative}")

        match = re.match(r"(.+)_(normal|hover|pressed|disabled)$", path.stem)
        if match:
            base, state = match.groups()
            state_groups[f"{path.parent.as_posix()}/{base}"].add(state)
            dimensions[f"{path.parent.as_posix()}/{base}"].add(image.size)

    for group, states in state_groups.items():
        missing = sorted(set(STATES) - states)
        if missing:
            errors.append(f"Missing states {missing}: {group}")
        if len(dimensions[group]) != 1:
            errors.append(f"State dimensions differ: {group} -> {sorted(dimensions[group])}")

    report = {
        "status": "pass" if not errors else "fail",
        "checked_png_files": len(png_files),
        "state_groups": len(state_groups),
        "errors": errors,
        "warnings": warnings,
    }
    REPORT.write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding="utf-8")
    print(json.dumps(report, ensure_ascii=False, indent=2))
    raise SystemExit(1 if errors else 0)


if __name__ == "__main__":
    main()
