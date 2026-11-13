from __future__ import annotations

import sys
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
TILESETS_ROOT = Path(__file__).resolve().parents[3]
sys.path.insert(0, str(TILESETS_ROOT))

from tools.hex_pack_builder import build_hex_pack


BASE = [
    ("fresh-snowfield", "Taze Kar Ovası"), ("windblown-snow", "Rüzgârlı Kar Ovası"),
    ("deep-snowdrifts", "Derin Kar Yığınları"), ("blue-icy-plain", "Mavi Buz Düzlüğü"),
    ("frozen-lake", "Donmuş Göl"), ("cracked-ice", "Çatlak Buz"),
    ("snowy-pine-grove", "Karlı Çam Korusu"), ("dense-taiga", "Sık Tayga Ormanı"),
    ("winter-birch-grove", "Kış Huş Korusu"), ("dead-frozen-forest", "Donmuş Ölü Orman"),
    ("frosted-meadow", "Kırağılı Çayır"), ("tundra-scrub", "Tundra Çalılığı"),
    ("snowy-hill", "Karlı Tepe"), ("rocky-frozen-ridge", "Donmuş Kayalık Sırt"),
    ("mountain-pass", "Karlı Dağ Geçidi"), ("great-glacier", "Büyük Buzul"),
    ("blue-ice-cliff", "Mavi Buz Uçurumu"), ("frozen-riverbed", "Donmuş Nehir Yatağı"),
    ("blizzard-ground", "Tipi Bölgesi"), ("avalanche-debris", "Çığ Kalıntıları"),
    ("geothermal-spring", "Jeotermal Kaynak"), ("ice-crystal-field", "Buz Kristali Tarlası"),
    ("snow-forest-edge", "Kar Orman Geçişi"), ("snow-coast-edge", "Kar Kıyı Geçişi"),
]

CONNECTIONS = [
    ("road-straight-e-w", "Kar Yolu Düz E-W"), ("road-straight-ne-sw", "Kar Yolu Düz NE-SW"),
    ("road-straight-nw-se", "Kar Yolu Düz NW-SE"), ("road-curve-e-se", "Kar Yolu Dönüş E-SE"),
    ("road-curve-se-sw", "Kar Yolu Dönüş SE-SW"), ("road-curve-sw-w", "Kar Yolu Dönüş SW-W"),
    ("road-curve-w-nw", "Kar Yolu Dönüş W-NW"), ("road-curve-nw-ne", "Kar Yolu Dönüş NW-NE"),
    ("road-curve-ne-e", "Kar Yolu Dönüş NE-E"), ("road-three-way-e-sw-nw", "Kar Yolu Üçlü E-SW-NW"),
    ("road-three-way-w-ne-se", "Kar Yolu Üçlü W-NE-SE"), ("road-six-way", "Kar Yolu Altılı Kavşak"),
    ("stream-straight-e-w", "Donmuş Akarsu Düz E-W"), ("stream-straight-ne-sw", "Donmuş Akarsu Düz NE-SW"),
    ("stream-straight-nw-se", "Donmuş Akarsu Düz NW-SE"), ("stream-curve-e-se", "Donmuş Akarsu Dönüş E-SE"),
    ("stream-curve-se-sw", "Donmuş Akarsu Dönüş SE-SW"), ("stream-curve-sw-w", "Donmuş Akarsu Dönüş SW-W"),
    ("stream-curve-w-nw", "Donmuş Akarsu Dönüş W-NW"), ("stream-curve-nw-ne", "Donmuş Akarsu Dönüş NW-NE"),
    ("stream-curve-ne-e", "Donmuş Akarsu Dönüş NE-E"), ("timber-bridge-e-w", "Ahşap Köprü E-W"),
    ("timber-bridge-ne-sw", "Ahşap Köprü NE-SW"), ("timber-bridge-nw-se", "Ahşap Köprü NW-SE"),
]

POI = [
    ("snow-village", "Karlı Köy"), ("winter-trading-town", "Kış Ticaret Kasabası"),
    ("ranger-lodge", "Korucu Kulübesi"), ("trapper-camp", "Kürk Avcısı Kampı"),
    ("northern-mining-camp", "Kuzey Maden Kampı"), ("winter-farmstead", "Kış Çiftliği"),
    ("snow-buried-ruins", "Kara Gömülü Harabeler"), ("frozen-watchtower", "Donmuş Gözetleme Kulesi"),
    ("blue-ice-temple", "Mavi Buz Tapınağı"), ("snow-shrine", "Kar Mabedi"),
    ("glowing-rune-stones", "Parlayan Rün Taşları"), ("frozen-graveyard", "Donmuş Mezarlık"),
    ("blue-ice-cave", "Mavi Buz Mağarası"), ("winter-mine", "Kış Madeni"),
    ("glacier-crevasse", "Buzul Yarığı"), ("hot-spring-grotto", "Sıcak Su Mağarası"),
    ("frozen-dock", "Donmuş İskele"), ("ancient-frost-tree", "Kadim Kırağı Ağacı"),
    ("northern-raider-camp", "Kuzey Akıncı Kampı"), ("yeti-nest", "Yeti Yuvası"),
    ("frost-witch-hut", "Buz Cadısı Kulübesi"), ("signal-watchtower", "İşaret Gözetleme Kulesi"),
    ("aurora-portal", "Aurora Geçidi"), ("black-ice-fortress", "Kara Buz Kalesi"),
]

CATEGORIES = [
    ("base", "snow-base-source-v1.png", BASE),
    ("connections", "snow-connections-source-v1.png", CONNECTIONS),
    ("poi", "snow-poi-source-v1.png", POI),
]


def metadata(category: str, tile_id: str, name_tr: str, index: int) -> dict:
    movement_cost = 2
    passable = True
    danger = 2
    requires = None

    if category == "base":
        if tile_id in {"fresh-snowfield", "windblown-snow", "frosted-meadow", "frozen-riverbed"}:
            movement_cost = 1
            danger = 1
        if tile_id in {"deep-snowdrifts", "dense-taiga", "rocky-frozen-ridge", "mountain-pass"}:
            movement_cost = 3
        if tile_id in {"frozen-lake", "blue-icy-plain", "great-glacier", "cracked-ice"}:
            requires = "ice_travel"
        if tile_id == "blue-ice-cliff":
            passable = False
            movement_cost = 99
        if tile_id in {"cracked-ice", "blizzard-ground", "avalanche-debris", "ice-crystal-field"}:
            danger = 3
        if tile_id == "snow-coast-edge":
            requires = "coastal_crossing"
    elif category == "connections":
        if tile_id.startswith("road-") or tile_id.startswith("timber-bridge-"):
            movement_cost = 1
            danger = 1
        else:
            movement_cost = 2
            requires = "ice_travel"
    else:
        movement_cost = 1 if tile_id in {
            "snow-village", "winter-trading-town", "ranger-lodge", "winter-farmstead", "hot-spring-grotto"
        } else 2
        danger = 3 if tile_id in {
            "blue-ice-temple", "glacier-crevasse", "northern-raider-camp", "yeti-nest",
            "frost-witch-hut", "aurora-portal", "black-ice-fortress"
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
        pack_slug="snow",
        pack_title="SNOW HEX TILESET V1",
        categories=CATEGORIES,
        metadata_fn=metadata,
        preview_labels=("BASE TERRAIN", "SNOW ROADS / FROZEN STREAMS / BRIDGES", "POINTS OF INTEREST"),
        preview_background=(12, 20, 30),
        preview_accent=(129, 219, 255),
    )
