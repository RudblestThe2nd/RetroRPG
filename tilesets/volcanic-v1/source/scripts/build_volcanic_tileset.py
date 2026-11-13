from __future__ import annotations

import sys
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
TILESETS_ROOT = Path(__file__).resolve().parents[3]
sys.path.insert(0, str(TILESETS_ROOT))

from tools.hex_pack_builder import build_hex_pack


BASE = [
    ("ash-plain", "Kül Ovası"), ("scorched-cracked-earth", "Kavrulmuş Çatlak Toprak"),
    ("black-basalt-field", "Kara Bazalt Tarlası"), ("magma-crust", "Magma Çatlaklı Kabuk"),
    ("small-lava-pool", "Küçük Lav Havuzu"), ("vast-lava-lake", "Büyük Lav Gölü"),
    ("smoking-vents", "Dumanlı Volkan Bacaları"), ("yellow-sulfur-field", "Sarı Kükürt Alanı"),
    ("obsidian-shard-field", "Obsidyen Parça Tarlası"), ("charred-forest", "Kömürleşmiş Orman"),
    ("dead-black-tree-grove", "Kara Ölü Ağaç Korusu"), ("glowing-ember-field", "Kor Tarlası"),
    ("volcanic-hill", "Volkanik Tepe"), ("basalt-ridge", "Bazalt Sırtı"),
    ("deep-lava-canyon", "Derin Lav Kanyonu"), ("caldera-rim", "Kaldera Kenarı"),
    ("magma-waterfall-cliff", "Magma Şelalesi Uçurumu"), ("volcanic-crater-lake", "Volkanik Krater Gölü"),
    ("eruption-debris", "Patlama Kalıntıları"), ("ash-storm-ground", "Kül Fırtınası Bölgesi"),
    ("collapsed-lava-tubes", "Çökmüş Lav Tünelleri"), ("black-glass-desert", "Kara Cam Çölü"),
    ("volcanic-mountain-edge", "Volkan Dağ Geçişi"), ("volcanic-corrupted-edge", "Volkan Lanetli Geçişi"),
]

CONNECTIONS = [
    ("road-straight-e-w", "Bazalt Yol Düz E-W"), ("road-straight-ne-sw", "Bazalt Yol Düz NE-SW"),
    ("road-straight-nw-se", "Bazalt Yol Düz NW-SE"), ("road-curve-e-se", "Bazalt Yol Dönüş E-SE"),
    ("road-curve-se-sw", "Bazalt Yol Dönüş SE-SW"), ("road-curve-sw-w", "Bazalt Yol Dönüş SW-W"),
    ("road-curve-w-nw", "Bazalt Yol Dönüş W-NW"), ("road-curve-nw-ne", "Bazalt Yol Dönüş NW-NE"),
    ("road-curve-ne-e", "Bazalt Yol Dönüş NE-E"), ("road-three-way-e-sw-nw", "Bazalt Yol Üçlü E-SW-NW"),
    ("road-three-way-w-ne-se", "Bazalt Yol Üçlü W-NE-SE"), ("road-six-way", "Bazalt Yol Altılı Kavşak"),
    ("lava-straight-e-w", "Lav Nehri Düz E-W"), ("lava-straight-ne-sw", "Lav Nehri Düz NE-SW"),
    ("lava-straight-nw-se", "Lav Nehri Düz NW-SE"), ("lava-curve-e-se", "Lav Nehri Dönüş E-SE"),
    ("lava-curve-se-sw", "Lav Nehri Dönüş SE-SW"), ("lava-curve-sw-w", "Lav Nehri Dönüş SW-W"),
    ("lava-curve-w-nw", "Lav Nehri Dönüş W-NW"), ("lava-curve-nw-ne", "Lav Nehri Dönüş NW-NE"),
    ("lava-curve-ne-e", "Lav Nehri Dönüş NE-E"), ("iron-bridge-e-w", "Demir Köprü E-W"),
    ("iron-bridge-ne-sw", "Demir Köprü NE-SW"), ("iron-bridge-nw-se", "Demir Köprü NW-SE"),
]

POI = [
    ("ash-village", "Kül Köyü"), ("volcanic-town", "Volkan Kenti"),
    ("master-forge-lodge", "Usta Demirhane"), ("lava-miner-camp", "Lav Madencisi Kampı"),
    ("obsidian-quarry", "Obsidyen Ocağı"), ("sulfur-outpost", "Kükürt Karakolu"),
    ("ruined-volcanic-citadel", "Yıkık Volkan Kalesi"), ("lava-watchtower", "Lav Gözetleme Kulesi"),
    ("fire-temple", "Ateş Tapınağı"), ("ember-shrine", "Kor Mabedi"),
    ("magma-rune-stones", "Magma Rün Taşları"), ("burned-graveyard", "Yanmış Mezarlık"),
    ("lava-cave", "Lav Mağarası"), ("obsidian-mine", "Obsidyen Madeni"),
    ("basalt-tunnel", "Bazalt Tüneli"), ("magma-grotto", "Magma Mağarası"),
    ("iron-lift-station", "Demir Asansör İstasyonu"), ("ancient-volcanic-forge", "Kadim Volkan Demirhanesi"),
    ("fire-cultist-camp", "Ateş Tarikatı Kampı"), ("fire-drake-nest", "Ateş Ejderi Yuvası"),
    ("flame-witch-hut", "Alev Cadısı Kulübesi"), ("eruption-beacon", "Patlama İşaret Kulesi"),
    ("infernal-portal", "Cehennem Geçidi"), ("volcano-boss-citadel", "Volkan Patron Kalesi"),
]

CATEGORIES = [
    ("base", "volcanic-base-source-v1.png", BASE),
    ("connections", "volcanic-connections-source-v1.png", CONNECTIONS),
    ("poi", "volcanic-poi-source-v1.png", POI),
]


def metadata(category: str, tile_id: str, name_tr: str, index: int) -> dict:
    movement_cost = 2
    passable = True
    danger = 3
    requires = None

    if category == "base":
        if tile_id in {"ash-plain", "black-basalt-field", "dead-black-tree-grove"}:
            movement_cost = 1
            danger = 2
        if tile_id in {"scorched-cracked-earth", "charred-forest", "black-glass-desert"}:
            movement_cost = 2
        if tile_id in {"smoking-vents", "yellow-sulfur-field", "obsidian-shard-field", "caldera-rim"}:
            movement_cost = 3
            requires = "heat_protection"
        if tile_id in {"small-lava-pool", "vast-lava-lake", "deep-lava-canyon", "magma-waterfall-cliff"}:
            passable = False
            movement_cost = 99
            requires = "fire_immunity"
        if tile_id in {"eruption-debris", "ash-storm-ground", "collapsed-lava-tubes", "volcanic-corrupted-edge"}:
            danger = 4
    elif category == "connections":
        if tile_id.startswith("road-") or tile_id.startswith("iron-bridge-"):
            movement_cost = 1
            danger = 2
        else:
            movement_cost = 99
            passable = False
            requires = "bridge_or_fire_immunity"
            danger = 4
    else:
        movement_cost = 1 if tile_id in {
            "ash-village", "volcanic-town", "master-forge-lodge", "lava-miner-camp",
            "obsidian-quarry", "sulfur-outpost", "iron-lift-station"
        } else 2
        danger = 4 if tile_id in {
            "fire-temple", "magma-grotto", "ancient-volcanic-forge", "fire-cultist-camp",
            "fire-drake-nest", "flame-witch-hut", "infernal-portal", "volcano-boss-citadel"
        } else 3

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
        pack_slug="volcanic",
        pack_title="VOLCANIC HEX TILESET V1",
        categories=CATEGORIES,
        metadata_fn=metadata,
        preview_labels=("BASE TERRAIN", "BASALT ROADS / LAVA RIVERS / IRON BRIDGES", "POINTS OF INTEREST"),
        preview_background=(23, 12, 10),
        preview_accent=(255, 105, 32),
    )
