from __future__ import annotations

import sys
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
TILESETS_ROOT = Path(__file__).resolve().parents[3]
sys.path.insert(0, str(TILESETS_ROOT))

from tools.hex_pack_builder import build_hex_pack


BASE = [
    ("grassy-foothills", "Çimenli Dağ Etekleri"), ("rocky-foothills", "Kayalık Dağ Etekleri"),
    ("low-granite-ridge", "Alçak Granit Sırt"), ("high-jagged-ridge", "Yüksek Sivri Sırt"),
    ("steep-granite-slope", "Dik Granit Yamaç"), ("loose-scree-field", "Gevşek Moloz Yamaç"),
    ("pine-mountains", "Çam Kaplı Dağlar"), ("alpine-meadow", "Alp Çayırı"),
    ("cloud-forest-ridge", "Sisli Dağ Ormanı"), ("snow-capped-peaks", "Karlı Zirveler"),
    ("bare-stone-peaks", "Çıplak Taş Zirveler"), ("twin-peaks", "İkiz Zirveler"),
    ("mountain-pass", "Dağ Geçidi"), ("red-rock-canyon", "Kızıl Kaya Kanyonu"),
    ("sheer-cliff-top", "Dik Uçurum Tepesi"), ("dark-ravine", "Karanlık Yar"),
    ("waterfall-cliff", "Şelaleli Uçurum"), ("alpine-lake", "Alp Gölü"),
    ("collapsed-landslide", "Heyelan Kalıntıları"), ("avalanche-path", "Çığ Yolu"),
    ("windswept-summit", "Rüzgârlı Zirve"), ("black-obsidian-ridge", "Kara Obsidyen Sırtı"),
    ("mountain-forest-edge", "Dağ Orman Geçişi"), ("mountain-snow-edge", "Dağ Kar Geçişi"),
]

CONNECTIONS = [
    ("road-straight-e-w", "Dağ Yolu Düz E-W"), ("road-straight-ne-sw", "Dağ Yolu Düz NE-SW"),
    ("road-straight-nw-se", "Dağ Yolu Düz NW-SE"), ("road-curve-e-se", "Dağ Yolu Dönüş E-SE"),
    ("road-curve-se-sw", "Dağ Yolu Dönüş SE-SW"), ("road-curve-sw-w", "Dağ Yolu Dönüş SW-W"),
    ("road-curve-w-nw", "Dağ Yolu Dönüş W-NW"), ("road-curve-nw-ne", "Dağ Yolu Dönüş NW-NE"),
    ("road-curve-ne-e", "Dağ Yolu Dönüş NE-E"), ("road-three-way-e-sw-nw", "Dağ Yolu Üçlü E-SW-NW"),
    ("road-three-way-w-ne-se", "Dağ Yolu Üçlü W-NE-SE"), ("road-six-way", "Dağ Yolu Altılı Kavşak"),
    ("river-straight-e-w", "Dağ Nehri Düz E-W"), ("river-straight-ne-sw", "Dağ Nehri Düz NE-SW"),
    ("river-straight-nw-se", "Dağ Nehri Düz NW-SE"), ("river-curve-e-se", "Dağ Nehri Dönüş E-SE"),
    ("river-curve-se-sw", "Dağ Nehri Dönüş SE-SW"), ("river-curve-sw-w", "Dağ Nehri Dönüş SW-W"),
    ("river-curve-w-nw", "Dağ Nehri Dönüş W-NW"), ("river-curve-nw-ne", "Dağ Nehri Dönüş NW-NE"),
    ("river-curve-ne-e", "Dağ Nehri Dönüş NE-E"), ("bridge-e-w", "Dağ Köprüsü E-W"),
    ("bridge-ne-sw", "Dağ Köprüsü NE-SW"), ("bridge-nw-se", "Dağ Köprüsü NW-SE"),
]

POI = [
    ("mountain-village", "Dağ Köyü"), ("mountain-trading-town", "Dağ Ticaret Kasabası"),
    ("cliffside-monastery", "Uçurum Manastırı"), ("shepherd-camp", "Çoban Kampı"),
    ("stone-quarry-camp", "Taş Ocağı Kampı"), ("alpine-farmstead", "Alp Çiftliği"),
    ("ruined-hill-fort", "Yıkık Tepe Kalesi"), ("ruined-watchtower", "Yıkık Dağ Kulesi"),
    ("sky-temple", "Gökyüzü Tapınağı"), ("summit-shrine", "Zirve Mabedi"),
    ("ancient-standing-stones", "Kadim Dikili Taşlar"), ("mountain-graveyard", "Dağ Mezarlığı"),
    ("granite-cave", "Granit Mağarası"), ("mountain-mine", "Dağ Madeni"),
    ("fortified-tunnel", "Tahkimli Dağ Tüneli"), ("waterfall-grotto", "Şelale Mağarası"),
    ("alpine-lake-dock", "Alp Gölü İskelesi"), ("giant-eagle-eyrie", "Dev Kartal Yuvası"),
    ("bandit-fortress-camp", "Haydut Kale Kampı"), ("wyvern-nest", "Wyvern Yuvası"),
    ("mountain-hermit-hut", "Dağ Münzevisi Kulübesi"), ("beacon-watchtower", "İşaret Kulesi"),
    ("sky-magic-portal", "Gökyüzü Büyü Geçidi"), ("summit-boss-citadel", "Zirve Patron Kalesi"),
]

CATEGORIES = [
    ("base", "mountain-base-source-v1.png", BASE),
    ("connections", "mountain-connections-source-v1.png", CONNECTIONS),
    ("poi", "mountain-poi-source-v1.png", POI),
]


def metadata(category: str, tile_id: str, name_tr: str, index: int) -> dict:
    movement_cost = 2
    passable = True
    danger = 2
    requires = None

    if category == "base":
        if tile_id in {"grassy-foothills", "alpine-meadow", "mountain-pass", "mountain-forest-edge"}:
            movement_cost = 1
            danger = 1
        if tile_id in {"rocky-foothills", "low-granite-ridge", "pine-mountains", "cloud-forest-ridge"}:
            movement_cost = 2
        if tile_id in {
            "high-jagged-ridge", "steep-granite-slope", "loose-scree-field", "snow-capped-peaks",
            "bare-stone-peaks", "twin-peaks", "windswept-summit", "mountain-snow-edge"
        }:
            movement_cost = 3
            requires = "climbing_gear"
        if tile_id in {"sheer-cliff-top", "dark-ravine", "waterfall-cliff"}:
            passable = False
            movement_cost = 99
        if tile_id in {"collapsed-landslide", "avalanche-path", "black-obsidian-ridge"}:
            danger = 3
    elif category == "connections":
        if tile_id.startswith("road-") or tile_id.startswith("bridge-"):
            movement_cost = 1
            danger = 1
        else:
            movement_cost = 3
            passable = False
            requires = "bridge_or_climbing_gear"
    else:
        movement_cost = 1 if tile_id in {
            "mountain-village", "mountain-trading-town", "cliffside-monastery", "alpine-farmstead",
            "fortified-tunnel", "waterfall-grotto"
        } else 2
        danger = 3 if tile_id in {
            "sky-temple", "giant-eagle-eyrie", "bandit-fortress-camp", "wyvern-nest",
            "sky-magic-portal", "summit-boss-citadel"
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


if __name__ == "__main__":
    build_hex_pack(
        root=ROOT,
        pack_slug="mountain",
        pack_title="MOUNTAIN HEX TILESET V1",
        categories=CATEGORIES,
        metadata_fn=metadata,
        preview_labels=("BASE TERRAIN", "MOUNTAIN ROADS / RIVERS / BRIDGES", "POINTS OF INTEREST"),
        preview_background=(17, 21, 17),
        preview_accent=(188, 205, 139),
    )
