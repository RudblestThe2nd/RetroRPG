from __future__ import annotations

import sys
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
TILESETS_ROOT = Path(__file__).resolve().parents[3]
sys.path.insert(0, str(TILESETS_ROOT))

from tools.hex_pack_builder import build_hex_pack


BASE = [
    ("limestone-cavern-floor", "Kireçtaşı Mağara Zemini"), ("dark-stone-cavern-floor", "Karanlık Taş Mağara Zemini"),
    ("wet-cave-floor", "Islak Mağara Zemini"), ("mossy-cavern", "Yosunlu Mağara"),
    ("blue-crystal-ground", "Mavi Kristal Alanı"), ("purple-crystal-ground", "Mor Kristal Alanı"),
    ("stalagmite-field", "Dikit Tarlası"), ("giant-mushroom-cavern", "Dev Mantar Mağarası"),
    ("fungus-grove", "Yeraltı Mantar Korusu"), ("bone-strewn-cave", "Kemikli Mağara"),
    ("ancient-carved-floor", "Kadim Oyma Zemin"), ("collapsed-rubble-chamber", "Çökmüş Taş Odası"),
    ("underground-lake", "Yeraltı Gölü"), ("black-abyss-pool", "Kara Uçurum Havuzu"),
    ("lava-cave-floor", "Lavlı Mağara Zemini"), ("sulfur-cavern", "Kükürtlü Mağara"),
    ("crystal-ridge", "Kristal Sırtı"), ("subterranean-canyon", "Yeraltı Kanyonu"),
    ("bottomless-chasm", "Dipsiz Yarık"), ("underground-waterfall-basin", "Yeraltı Şelalesi Havzası"),
    ("misty-cavern", "Sisli Mağara"), ("magical-geode-chamber", "Büyülü Jeot Odası"),
    ("mountain-cave-transition", "Dağ Mağara Geçişi"), ("corrupted-cave-transition", "Lanetli Mağara Geçişi"),
]

CONNECTIONS = [
    ("road-straight-n-s", "Mağara Yolu Düz N-S"), ("road-straight-ne-sw", "Mağara Yolu Düz NE-SW"),
    ("road-straight-nw-se", "Mağara Yolu Düz NW-SE"), ("road-curve-n-se", "Mağara Yolu Dönüş N-SE"),
    ("road-curve-n-sw", "Mağara Yolu Dönüş N-SW"), ("road-three-way", "Mağara Yolu Üçlü Kavşak"),
    ("road-four-way", "Mağara Yolu Dörtlü Kavşak"), ("rail-straight-n-s", "Maden Rayı Düz N-S"),
    ("rail-straight-ne-sw", "Maden Rayı Düz NE-SW"), ("rail-straight-nw-se", "Maden Rayı Düz NW-SE"),
    ("rail-curve", "Maden Rayı Dönüş"), ("rail-junction", "Maden Rayı Kavşağı"),
    ("river-straight-n-s", "Yeraltı Nehri Düz N-S"), ("river-straight-ne-sw", "Yeraltı Nehri Düz NE-SW"),
    ("river-straight-nw-se", "Yeraltı Nehri Düz NW-SE"), ("river-bend", "Yeraltı Nehri Dönüş"),
    ("river-fork", "Yeraltı Nehri Çatalı"), ("river-confluence", "Yeraltı Nehri Birleşimi"),
    ("stone-river-bridge", "Taş Nehir Köprüsü"), ("rope-chasm-bridge", "Halat Uçurum Köprüsü"),
    ("ancient-rune-bridge", "Kadim Rün Köprüsü"), ("wooden-mine-bridge", "Ahşap Maden Köprüsü"),
    ("lava-bridge", "Lav Köprüsü"), ("crystal-bridge", "Kristal Köprü"),
]

POI = [
    ("dwarven-mining-camp", "Cüce Madenci Kampı"), ("subterranean-village", "Yeraltı Köyü"),
    ("underground-trading-post", "Yeraltı Ticaret Noktası"), ("mushroom-folk-settlement", "Mantar Halkı Yerleşimi"),
    ("abandoned-miners-camp", "Terk Edilmiş Madenci Kampı"), ("crystal-prospectors-camp", "Kristal Arayıcıları Kampı"),
    ("ancient-dwarven-city-gate", "Kadim Cüce Şehri Kapısı"), ("ruined-underground-fortress", "Yıkık Yeraltı Kalesi"),
    ("deep-temple", "Derin Tapınak"), ("forgotten-shrine", "Unutulmuş Mabet"),
    ("underground-graveyard", "Yeraltı Mezarlığı"), ("prison-dungeon-entrance", "Zindan Hapishanesi Girişi"),
    ("active-mine-entrance", "Etkin Maden Girişi"), ("crystal-mine", "Kristal Madeni"),
    ("flooded-tunnel-entrance", "Su Basmış Tünel"), ("lava-tunnel-entrance", "Lav Tüneli"),
    ("giant-mushroom-hollow", "Dev Mantar Oyuğu"), ("hidden-smuggler-cave", "Gizli Kaçakçı Mağarası"),
    ("spider-nest", "Örümcek Yuvası"), ("goblin-cave-camp", "Goblin Mağara Kampı"),
    ("deep-monster-lair", "Derin Canavar İni"), ("underground-wizard-laboratory", "Yeraltı Büyücü Laboratuvarı"),
    ("sealed-ancient-vault", "Mühürlü Kadim Kasa"), ("crystal-dragon-boss-chamber", "Kristal Ejder Patron Odası"),
]

CATEGORIES = [
    ("base", "underground-base-source-v1.png", BASE),
    ("connections", "underground-connections-source-v1.png", CONNECTIONS),
    ("poi", "underground-poi-source-v1.png", POI),
]


def metadata(category: str, tile_id: str, name_tr: str, index: int) -> dict:
    movement_cost = 2
    passable = True
    danger = 2
    requires = "light_source"

    if category == "base":
        if tile_id in {"limestone-cavern-floor", "dark-stone-cavern-floor", "ancient-carved-floor"}:
            movement_cost = 1
        if tile_id in {"wet-cave-floor", "mossy-cavern", "stalagmite-field", "fungus-grove", "collapsed-rubble-chamber"}:
            movement_cost = 3
        if tile_id in {"underground-lake", "black-abyss-pool", "subterranean-canyon", "bottomless-chasm"}:
            passable = False
            movement_cost = 99
            requires = "bridge_flight_or_boat"
            danger = 4
        if tile_id == "lava-cave-floor":
            passable = False
            movement_cost = 99
            requires = "fire_immunity"
            danger = 4
        if tile_id in {"sulfur-cavern", "misty-cavern", "magical-geode-chamber", "corrupted-cave-transition"}:
            danger = 3
    elif category == "connections":
        if tile_id.startswith("road-") or tile_id.startswith("rail-") or tile_id.endswith("bridge"):
            movement_cost = 1
            requires = "light_source"
        else:
            movement_cost = 99
            passable = False
            requires = "boat_or_bridge"
            danger = 3
    else:
        movement_cost = 1 if tile_id in {
            "dwarven-mining-camp", "subterranean-village", "underground-trading-post",
            "mushroom-folk-settlement", "crystal-prospectors-camp", "active-mine-entrance"
        } else 2
        danger = 4 if tile_id in {
            "prison-dungeon-entrance", "lava-tunnel-entrance", "spider-nest", "goblin-cave-camp",
            "deep-monster-lair", "sealed-ancient-vault", "crystal-dragon-boss-chamber"
        } else 3
        if tile_id == "crystal-dragon-boss-chamber":
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
        pack_slug="underground",
        pack_title="UNDERGROUND / CAVE HEX TILESET V1",
        categories=CATEGORIES,
        metadata_fn=metadata,
        preview_labels=("CAVE TERRAIN", "ROADS / RAILS / RIVERS / BRIDGES", "UNDERGROUND POINTS OF INTEREST"),
        preview_background=(11, 12, 18),
        preview_accent=(87, 191, 255),
    )
