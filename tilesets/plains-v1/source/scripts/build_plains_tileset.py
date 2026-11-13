from __future__ import annotations

import sys
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
TILESETS_ROOT = Path(__file__).resolve().parents[3]
sys.path.insert(0, str(TILESETS_ROOT))

from tools.hex_pack_builder import build_hex_pack


BASE = [
    ("short-green-grass", "Kısa Yeşil Çayır"), ("tall-golden-grass", "Uzun Altın Çayır"),
    ("wildflower-meadow", "Yabani Çiçek Çayırı"), ("clover-field", "Yonca Tarlası"),
    ("dry-summer-grassland", "Kuru Yaz Ovası"), ("rain-soaked-grassland", "Yağmurla Islanmış Ova"),
    ("rolling-grassy-hills", "Dalgalı Yeşil Tepeler"), ("rocky-grassland", "Kayalık Çayır"),
    ("scattered-oak-grove", "Seyrek Meşe Korusu"), ("lone-ancient-oak", "Yalnız Kadim Meşe"),
    ("hedgerow-field", "Çitli Tarla"), ("grazing-pasture", "Otlak"),
    ("wheat-field", "Buğday Tarlası"), ("barley-field", "Arpa Tarlası"),
    ("ploughed-farmland", "Sürülmüş Tarla"), ("fallow-field", "Nadas Tarlası"),
    ("irrigation-pond", "Sulama Göleti"), ("shallow-grassland-stream", "Sığ Ova Deresi"),
    ("windswept-plateau", "Rüzgârlı Plato"), ("misty-lowland", "Sisli Alçak Ova"),
    ("battlefield-scarred-grass", "Savaş Yaralı Çayır"), ("standing-stone-field", "Dikili Taş Alanı"),
    ("forest-plains-transition", "Orman Ova Geçişi"), ("desert-plains-transition", "Çöl Ova Geçişi"),
]

CONNECTIONS = [
    ("road-straight-e-w", "Toprak Yol Düz E-W"), ("road-straight-ne-sw", "Toprak Yol Düz NE-SW"),
    ("road-straight-nw-se", "Toprak Yol Düz NW-SE"), ("road-curve-e-se", "Toprak Yol Dönüş E-SE"),
    ("road-curve-se-sw", "Toprak Yol Dönüş SE-SW"), ("road-curve-sw-w", "Toprak Yol Dönüş SW-W"),
    ("road-curve-w-nw", "Toprak Yol Dönüş W-NW"), ("road-curve-nw-ne", "Toprak Yol Dönüş NW-NE"),
    ("road-curve-ne-e", "Toprak Yol Dönüş NE-E"), ("road-three-way", "Toprak Yol Üçlü Kavşak"),
    ("road-four-way", "Toprak Yol Dörtlü Kavşak"), ("road-six-way", "Toprak Yol Altılı Kavşak"),
    ("stream-straight-e-w", "Ova Deresi Düz E-W"), ("stream-straight-ne-sw", "Ova Deresi Düz NE-SW"),
    ("stream-straight-nw-se", "Ova Deresi Düz NW-SE"), ("stream-curve-e-se", "Ova Deresi Dönüş E-SE"),
    ("stream-curve-se-sw", "Ova Deresi Dönüş SE-SW"), ("stream-fork", "Ova Deresi Çatalı"),
    ("stone-bridge-e-w", "Taş Köprü E-W"), ("stone-bridge-ne-sw", "Taş Köprü NE-SW"),
    ("stone-bridge-nw-se", "Taş Köprü NW-SE"), ("wooden-farm-bridge", "Ahşap Çiftlik Köprüsü"),
    ("wagon-ford-crossing", "Araba Geçidi"), ("ancient-kings-road", "Kadim Kral Yolu"),
]

POI = [
    ("farming-village", "Çiftçi Köyü"), ("prosperous-market-town", "Zengin Pazar Kasabası"),
    ("isolated-farmhouse", "Issız Çiftlik Evi"), ("windmill-farm", "Yel Değirmeni Çiftliği"),
    ("shepherd-camp", "Çoban Kampı"), ("roadside-inn", "Yol Kenarı Hanı"),
    ("hilltop-castle", "Tepe Kalesi"), ("wooden-watchtower", "Ahşap Gözetleme Kulesi"),
    ("small-stone-temple", "Küçük Taş Tapınak"), ("wayside-shrine", "Yol Kenarı Mabedi"),
    ("old-graveyard", "Eski Mezarlık"), ("ruined-manor", "Yıkık Malikâne"),
    ("bandit-camp", "Haydut Kampı"), ("military-encampment", "Askerî Kamp"),
    ("tournament-field", "Turnuva Alanı"), ("abandoned-battlefield", "Terk Edilmiş Savaş Alanı"),
    ("hunter-lodge", "Avcı Kulübesi"), ("herbalist-cottage", "Şifacı Kulübesi"),
    ("fairy-ring", "Peri Çemberi"), ("ancient-stone-circle", "Kadim Taş Çemberi"),
    ("giant-sacred-tree", "Dev Kutsal Ağaç"), ("sealed-barrow-mound", "Mühürlü Höyük"),
    ("royal-caravan-rest", "Kraliyet Kervanı Dinlenme Yeri"), ("golden-griffin-boss-nest", "Altın Griffin Patron Yuvası"),
]

CATEGORIES = [
    ("base", "plains-base-source-v1.png", BASE),
    ("connections", "plains-connections-source-v1.png", CONNECTIONS),
    ("poi", "plains-poi-source-v1.png", POI),
]


def metadata(category: str, tile_id: str, name_tr: str, index: int) -> dict:
    movement_cost = 1
    passable = True
    danger = 1
    requires = None

    if category == "base":
        if tile_id in {"tall-golden-grass", "rain-soaked-grassland", "rolling-grassy-hills", "rocky-grassland", "hedgerow-field"}:
            movement_cost = 2
        if tile_id in {"irrigation-pond", "shallow-grassland-stream"}:
            movement_cost = 3
            requires = "shallow_water_travel"
        if tile_id in {"misty-lowland", "battlefield-scarred-grass", "standing-stone-field"}:
            danger = 2
    elif category == "connections":
        if tile_id.startswith("road-") or tile_id.endswith("bridge") or tile_id in {"wagon-ford-crossing", "ancient-kings-road"}:
            movement_cost = 1
        else:
            movement_cost = 99
            passable = False
            requires = "bridge_or_shallow_water_travel"
            danger = 2
    else:
        movement_cost = 1
        danger = 3 if tile_id in {
            "old-graveyard", "ruined-manor", "bandit-camp", "abandoned-battlefield",
            "fairy-ring", "sealed-barrow-mound", "golden-griffin-boss-nest"
        } else 1
        if tile_id in {"hilltop-castle", "military-encampment", "tournament-field", "ancient-stone-circle", "giant-sacred-tree"}:
            danger = 2
        if tile_id == "golden-griffin-boss-nest":
            danger = 5

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
        pack_slug="plains",
        pack_title="PLAINS / GRASSLAND HEX TILESET V1",
        categories=CATEGORIES,
        metadata_fn=metadata,
        preview_labels=("PLAINS TERRAIN", "DIRT ROADS / STREAMS / BRIDGES", "PLAINS POINTS OF INTEREST"),
        preview_background=(12, 18, 10),
        preview_accent=(172, 220, 80),
    )
