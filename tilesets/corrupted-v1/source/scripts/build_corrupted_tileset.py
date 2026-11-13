from __future__ import annotations

import sys
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
TILESETS_ROOT = Path(__file__).resolve().parents[3]
sys.path.insert(0, str(TILESETS_ROOT))

from tools.hex_pack_builder import build_hex_pack


BASE = [
    ("withered-grassland", "Solmuş Çayır"), ("black-dead-soil", "Kara Ölü Toprak"),
    ("purple-corruption-veins", "Mor Yozlaşma Damarları"), ("crimson-corruption-veins", "Kızıl Yozlaşma Damarları"),
    ("shadow-mire", "Gölge Bataklığı"), ("violet-void-pool", "Mor Boşluk Havuzu"),
    ("twisted-black-forest", "Bükülmüş Kara Orman"), ("whispering-dead-forest", "Fısıldayan Ölü Orman"),
    ("thorn-grove", "Diken Korusu"), ("sinister-mushroom-field", "Uğursuz Mantar Tarlası"),
    ("bone-wasteland", "Kemik Çoraklığı"), ("haunted-mist", "Lanetli Sis"),
    ("corrupted-hill", "Yozlaşmış Tepe"), ("black-crystal-ridge", "Kara Kristal Sırtı"),
    ("shadow-canyon", "Gölge Kanyonu"), ("floating-shattered-rocks", "Parçalanmış Uçan Kayalar"),
    ("abyss-cliff", "Uçurum Boşluğu"), ("cursed-dark-lake", "Lanetli Karanlık Göl"),
    ("dark-storm-ground", "Karanlık Fırtına Bölgesi"), ("open-void-rift", "Açık Boşluk Yarığı"),
    ("corruption-spires", "Yozlaşma Kuleleri"), ("eye-growth-field", "Göz Oluşumu Tarlası"),
    ("corrupted-volcanic-edge", "Lanetli Volkan Geçişi"), ("corrupted-plains-edge", "Lanetli Ova Geçişi"),
]

CONNECTIONS = [
    ("road-straight-e-w", "Lanetli Yol Düz E-W"), ("road-straight-ne-sw", "Lanetli Yol Düz NE-SW"),
    ("road-straight-nw-se", "Lanetli Yol Düz NW-SE"), ("road-curve-e-se", "Lanetli Yol Dönüş E-SE"),
    ("road-curve-se-sw", "Lanetli Yol Dönüş SE-SW"), ("road-curve-sw-w", "Lanetli Yol Dönüş SW-W"),
    ("road-curve-w-nw", "Lanetli Yol Dönüş W-NW"), ("road-curve-nw-ne", "Lanetli Yol Dönüş NW-NE"),
    ("road-curve-ne-e", "Lanetli Yol Dönüş NE-E"), ("road-three-way-e-sw-nw", "Lanetli Yol Üçlü E-SW-NW"),
    ("road-three-way-w-ne-se", "Lanetli Yol Üçlü W-NE-SE"), ("road-six-way", "Lanetli Yol Altılı Kavşak"),
    ("shadow-river-straight-e-w", "Gölge Nehri Düz E-W"), ("shadow-river-straight-ne-sw", "Gölge Nehri Düz NE-SW"),
    ("shadow-river-straight-nw-se", "Gölge Nehri Düz NW-SE"), ("shadow-river-curve-e-se", "Gölge Nehri Dönüş E-SE"),
    ("shadow-river-curve-se-sw", "Gölge Nehri Dönüş SE-SW"), ("shadow-river-curve-sw-w", "Gölge Nehri Dönüş SW-W"),
    ("shadow-river-curve-w-nw", "Gölge Nehri Dönüş W-NW"), ("shadow-river-curve-nw-ne", "Gölge Nehri Dönüş NW-NE"),
    ("shadow-river-curve-ne-e", "Gölge Nehri Dönüş NE-E"), ("rune-bridge-e-w", "Rün Köprüsü E-W"),
    ("rune-bridge-ne-sw", "Rün Köprüsü NE-SW"), ("rune-bridge-nw-se", "Rün Köprüsü NW-SE"),
]

POI = [
    ("cursed-village", "Lanetli Köy"), ("fortified-dark-city", "Tahkimli Karanlık Şehir"),
    ("shadow-hunter-lodge", "Gölge Avcısı Kulübesi"), ("survivor-camp", "Hayatta Kalanlar Kampı"),
    ("black-crystal-quarry", "Kara Kristal Ocağı"), ("blighted-farmstead", "Çürümüş Çiftlik"),
    ("ruined-black-citadel", "Yıkık Kara Kale"), ("eye-watchtower", "Göz Gözetleme Kulesi"),
    ("shadow-temple", "Gölge Tapınağı"), ("eye-shrine", "Göz Mabedi"),
    ("floating-dark-monoliths", "Uçan Kara Monolitler"), ("cursed-graveyard", "Lanetli Mezarlık"),
    ("void-cave", "Boşluk Mağarası"), ("black-crystal-mine", "Kara Kristal Madeni"),
    ("flesh-root-tunnel", "Et ve Kök Tüneli"), ("abyss-grotto", "Uçurum Mağarası"),
    ("floating-rock-lift", "Uçan Kaya Asansörü"), ("ancient-eye-tree", "Kadim Göz Ağacı"),
    ("masked-cultist-camp", "Maskeli Tarikat Kampı"), ("nightmare-beast-nest", "Kâbus Yaratığı Yuvası"),
    ("dark-witch-hut", "Kara Cadı Kulübesi"), ("black-signal-tower", "Kara İşaret Kulesi"),
    ("eye-magic-portal", "Göz Büyü Geçidi"), ("watching-eye-tower", "Gözetleyen Göz Kulesi"),
]

CATEGORIES = [
    ("base", "corrupted-base-source-v1.png", BASE),
    ("connections", "corrupted-connections-source-v1.png", CONNECTIONS),
    ("poi", "corrupted-poi-source-v1.png", POI),
]


def metadata(category: str, tile_id: str, name_tr: str, index: int) -> dict:
    movement_cost = 2
    passable = True
    danger = 3
    requires = "corruption_resistance"

    if category == "base":
        if tile_id in {"withered-grassland", "black-dead-soil", "corrupted-plains-edge"}:
            movement_cost = 1
            danger = 2
        if tile_id in {"twisted-black-forest", "whispering-dead-forest", "thorn-grove", "haunted-mist"}:
            movement_cost = 3
        if tile_id in {"violet-void-pool", "shadow-canyon", "abyss-cliff", "open-void-rift"}:
            passable = False
            movement_cost = 99
            requires = "void_travel"
        if tile_id in {"dark-storm-ground", "corruption-spires", "eye-growth-field"}:
            danger = 4
    elif category == "connections":
        if tile_id.startswith("road-") or tile_id.startswith("rune-bridge-"):
            movement_cost = 1
            danger = 2
        else:
            movement_cost = 99
            passable = False
            requires = "rune_bridge_or_void_travel"
            danger = 4
    else:
        movement_cost = 1 if tile_id in {"survivor-camp", "black-crystal-quarry", "floating-rock-lift"} else 2
        danger = 5 if tile_id in {
            "shadow-temple", "void-cave", "flesh-root-tunnel", "nightmare-beast-nest",
            "eye-magic-portal", "watching-eye-tower"
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
        pack_slug="corrupted",
        pack_title="CORRUPTED / DARK HEX TILESET V1",
        categories=CATEGORIES,
        metadata_fn=metadata,
        preview_labels=("BASE TERRAIN", "CURSED ROADS / SHADOW RIVERS / RUNE BRIDGES", "POINTS OF INTEREST"),
        preview_background=(15, 10, 20),
        preview_accent=(221, 78, 255),
    )
