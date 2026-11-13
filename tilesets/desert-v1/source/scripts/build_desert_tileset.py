from __future__ import annotations

import sys
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
TILESETS_ROOT = Path(__file__).resolve().parents[3]
sys.path.insert(0, str(TILESETS_ROOT))

from tools.hex_pack_builder import build_hex_pack


BASE = [
    ("golden-dunes", "Altın Kum Tepeleri"), ("pale-dunes", "Soluk Kum Tepeleri"),
    ("red-dunes", "Kızıl Kum Tepeleri"), ("wind-rippled-dunes", "Rüzgâr İzli Kumlar"),
    ("gravel-plain", "Çakıllı Düzlük"), ("cracked-earth", "Çatlamış Toprak"),
    ("dry-scrubland", "Kuru Çalılık"), ("cactus-field", "Kaktüs Tarlası"),
    ("thorn-scrub", "Dikenli Çalılık"), ("acacia-grove", "Akasya Korusu"),
    ("palm-oasis", "Palmiye Vahası"), ("white-salt-flat", "Beyaz Tuz Düzlüğü"),
    ("rocky-mesa", "Kayalık Masa Dağı"), ("sandstone-outcrop", "Kumtaşı Çıkıntısı"),
    ("red-canyon", "Kızıl Kanyon"), ("desert-hill", "Çöl Tepesi"),
    ("cliff-plateau", "Uçurum Platosu"), ("dry-riverbed", "Kurumuş Nehir Yatağı"),
    ("quicksand", "Batak Kum"), ("dusty-basin", "Tozlu Çanak"),
    ("scorched-earth", "Kavrulmuş Toprak"), ("obsidian-field", "Obsidyen Tarlası"),
    ("desert-coast-edge", "Çöl Kıyı Geçişi"), ("desert-mountain-edge", "Çöl Dağ Geçişi"),
]

CONNECTIONS = [
    ("road-straight-e-w", "Kervan Yolu Düz E-W"), ("road-straight-ne-sw", "Kervan Yolu Düz NE-SW"),
    ("road-straight-nw-se", "Kervan Yolu Düz NW-SE"), ("road-curve-e-se", "Kervan Yolu Dönüş E-SE"),
    ("road-curve-se-sw", "Kervan Yolu Dönüş SE-SW"), ("road-curve-sw-w", "Kervan Yolu Dönüş SW-W"),
    ("road-curve-w-nw", "Kervan Yolu Dönüş W-NW"), ("road-curve-nw-ne", "Kervan Yolu Dönüş NW-NE"),
    ("road-curve-ne-e", "Kervan Yolu Dönüş NE-E"), ("road-three-way-e-sw-nw", "Kervan Yolu Üçlü E-SW-NW"),
    ("road-three-way-w-ne-se", "Kervan Yolu Üçlü W-NE-SE"), ("road-six-way", "Kervan Yolu Altılı Kavşak"),
    ("wadi-straight-e-w", "Vadi Düz E-W"), ("wadi-straight-ne-sw", "Vadi Düz NE-SW"),
    ("wadi-straight-nw-se", "Vadi Düz NW-SE"), ("wadi-curve-e-se", "Vadi Dönüş E-SE"),
    ("wadi-curve-se-sw", "Vadi Dönüş SE-SW"), ("wadi-curve-sw-w", "Vadi Dönüş SW-W"),
    ("wadi-curve-w-nw", "Vadi Dönüş W-NW"), ("wadi-curve-nw-ne", "Vadi Dönüş NW-NE"),
    ("wadi-curve-ne-e", "Vadi Dönüş NE-E"), ("stone-bridge-e-w", "Taş Köprü E-W"),
    ("stone-bridge-ne-sw", "Taş Köprü NE-SW"), ("stone-bridge-nw-se", "Taş Köprü NW-SE"),
]

POI = [
    ("adobe-village", "Kerpiç Köy"), ("desert-trading-city", "Çöl Ticaret Şehri"),
    ("caravanserai", "Kervansaray"), ("nomad-camp", "Göçebe Kampı"),
    ("oasis-camp", "Vaha Kampı"), ("date-farmstead", "Hurma Çiftliği"),
    ("buried-ruins", "Gömülü Harabeler"), ("ruined-watchtower", "Yıkık Çöl Kulesi"),
    ("sun-temple", "Güneş Tapınağı"), ("desert-shrine", "Çöl Mabedi"),
    ("ancient-obelisks", "Kadim Dikilitaşlar"), ("desert-graveyard", "Çöl Mezarlığı"),
    ("cave-entrance", "Kumtaşı Mağarası"), ("desert-mine", "Çöl Madeni"),
    ("sealed-royal-tomb", "Mühürlü Kraliyet Mezarı"), ("hidden-spring-grotto", "Gizli Kaynak Mağarası"),
    ("ancient-well", "Kadim Kuyu"), ("colossal-fossil", "Dev Fosil"),
    ("bandit-camp", "Haydut Kampı"), ("sandworm-nest", "Kum Kurdu Yuvası"),
    ("hermit-hut", "Çöl Münzevisi Kulübesi"), ("frontier-watchtower", "Sınır Gözetleme Kulesi"),
    ("magic-portal", "Çöl Büyü Geçidi"), ("boss-arena", "Patron Arenası"),
]

CATEGORIES = [
    ("base", "desert-base-source-v1.png", BASE),
    ("connections", "desert-connections-source-v1.png", CONNECTIONS),
    ("poi", "desert-poi-source-v1.png", POI),
]


def metadata(category: str, tile_id: str, name_tr: str, index: int) -> dict:
    movement_cost = 2
    passable = True
    danger = 2
    requires = None

    if category == "base":
        if tile_id in {"gravel-plain", "cracked-earth", "dry-scrubland", "dry-riverbed"}:
            movement_cost = 1
            danger = 1
        if tile_id in {"golden-dunes", "pale-dunes", "red-dunes", "wind-rippled-dunes", "cactus-field"}:
            movement_cost = 2
        if tile_id in {"rocky-mesa", "red-canyon", "cliff-plateau", "desert-mountain-edge"}:
            movement_cost = 3
        if tile_id in {"quicksand", "cliff-plateau"}:
            passable = False
            movement_cost = 99
        if tile_id in {"scorched-earth", "obsidian-field", "quicksand"}:
            danger = 3
        if tile_id == "desert-coast-edge":
            requires = "coastal_crossing"
    elif category == "connections":
        if tile_id.startswith("road-") or tile_id.startswith("stone-bridge-"):
            movement_cost = 1
            danger = 1
        else:
            movement_cost = 3
            passable = False
            requires = "bridge_or_wading"
    else:
        movement_cost = 1 if tile_id in {
            "adobe-village", "desert-trading-city", "caravanserai", "oasis-camp", "date-farmstead"
        } else 2
        danger = 3 if tile_id in {
            "sun-temple", "sealed-royal-tomb", "bandit-camp", "sandworm-nest", "magic-portal", "boss-arena"
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
        pack_slug="desert",
        pack_title="DESERT HEX TILESET V1",
        categories=CATEGORIES,
        metadata_fn=metadata,
        preview_labels=("BASE TERRAIN", "CARAVAN ROADS / WADIS / BRIDGES", "POINTS OF INTEREST"),
        preview_background=(30, 21, 11),
        preview_accent=(245, 186, 61),
    )
