from __future__ import annotations

from pathlib import Path

from PIL import Image, ImageDraw


ROOT = Path(__file__).resolve().parents[1]
OUTPUT = ROOT / "source" / "imagegen" / "skin-tone-icons-source-v1.png"
CELL = 256

TONES = [
    ("porcelain", "#F7D9C4"),
    ("fair-rose", "#F0C6AF"),
    ("peach", "#EAB18F"),
    ("light-tan", "#D99A70"),
    ("golden", "#C9864F"),
    ("warm-tan", "#B8754D"),
    ("olive", "#A47C4D"),
    ("medium-brown", "#8C5A3C"),
    ("deep-brown", "#643D2C"),
    ("ebony", "#3B2725"),
    ("orc-sage", "#7E8B57"),
    ("orc-olive", "#667343"),
    ("orc-green", "#4E6A45"),
    ("gith-yellow", "#C6A33C"),
    ("tiefling-crimson", "#9E3B37"),
    ("tiefling-red", "#B95243"),
    ("tiefling-purple", "#76517F"),
    ("infernal-gray", "#6E6470"),
    ("drow-slate", "#4C5267"),
    ("drow-violet", "#66537B"),
    ("lavender", "#88759D"),
    ("ice-blue", "#8DA9B7"),
    ("dragon-gold", "#B98735"),
    ("dragon-obsidian", "#34343B"),
]


def shift(rgb: tuple[int, int, int], amount: int) -> tuple[int, int, int, int]:
    return tuple(max(0, min(255, channel + amount)) for channel in rgb) + (255,)


def main() -> None:
    OUTPUT.parent.mkdir(parents=True, exist_ok=True)
    canvas = Image.new("RGBA", (CELL * 6, CELL * 4), (0, 0, 0, 0))
    draw = ImageDraw.Draw(canvas)
    for index, (_, hex_color) in enumerate(TONES):
        column = index % 6
        row = index // 6
        x0 = column * CELL
        y0 = row * CELL
        rgb = tuple(bytes.fromhex(hex_color[1:]))
        outer = (x0 + 40, y0 + 40, x0 + 216, y0 + 216)
        inner = (x0 + 50, y0 + 50, x0 + 206, y0 + 206)
        draw.rounded_rectangle(outer, radius=44, fill=(22, 20, 24, 255))
        draw.rounded_rectangle(inner, radius=38, fill=rgb + (255,))
        draw.polygon(
            [
                (x0 + 66, y0 + 70),
                (x0 + 126, y0 + 54),
                (x0 + 98, y0 + 86),
                (x0 + 62, y0 + 110),
            ],
            fill=shift(rgb, 28),
        )
        draw.polygon(
            [
                (x0 + 198, y0 + 150),
                (x0 + 198, y0 + 198),
                (x0 + 126, y0 + 206),
                (x0 + 168, y0 + 184),
            ],
            fill=shift(rgb, -26),
        )
        for offset in range(0, 40, 8):
            draw.rectangle(
                (x0 + 74 + offset, y0 + 166, x0 + 77 + offset, y0 + 169),
                fill=shift(rgb, -12),
            )
    canvas.save(OUTPUT)
    print(OUTPUT)


if __name__ == "__main__":
    main()
