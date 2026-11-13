from __future__ import annotations

import sys
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
TILESETS_ROOT = Path(__file__).resolve().parents[3]
sys.path.insert(0, str(TILESETS_ROOT))

from tools.hex_pack_builder import build_hex_pack


BASE = [
    ("golden-beach", "Altın Kumsal"), ("white-sand-beach", "Beyaz Kum Plajı"),
    ("black-sand-beach", "Siyah Volkanik Plaj"), ("coral-shoreline", "Mercan Kıyısı"),
    ("shallow-lagoon", "Sığ Turkuaz Lagün"), ("deep-tropical-water", "Derin Tropik Su"),
    ("palm-grove", "Palmiye Korusu"), ("dense-tropical-jungle", "Sık Tropik Orman"),
    ("bamboo-grove", "Bambu Korusu"), ("mangrove-roots", "Mangrov Kökleri"),
    ("tropical-flower-field", "Tropik Çiçeklik"), ("jungle-clearing", "Orman Açıklığı"),
    ("jungle-hill", "Orman Tepesi"), ("limestone-karst-hills", "Kireçtaşı Tepeleri"),
    ("coastal-cliff", "Kıyı Uçurumu"), ("jungle-canyon", "Orman Kanyonu"),
    ("waterfall-jungle", "Şelaleli Orman"), ("freshwater-cenote", "Tatlı Su Cenotesi"),
    ("storm-soaked-ground", "Fırtına Islaklığı"), ("flooded-jungle", "Su Basan Orman"),
    ("colorful-coral-reef", "Renkli Mercan Resifi"), ("shipwreck-beach", "Gemi Enkazı Plajı"),
    ("tropical-forest-edge", "Tropik Orman Geçişi"), ("tropical-desert-edge", "Tropik Çöl Geçişi"),
]

CONNECTIONS = [
    ("trail-straight-e-w", "Orman Patikası Düz E-W"), ("trail-straight-ne-sw", "Orman Patikası Düz NE-SW"),
    ("trail-straight-nw-se", "Orman Patikası Düz NW-SE"), ("trail-curve-e-se", "Orman Patikası Dönüş E-SE"),
    ("trail-curve-se-sw", "Orman Patikası Dönüş SE-SW"), ("trail-curve-sw-w", "Orman Patikası Dönüş SW-W"),
    ("trail-curve-w-nw", "Orman Patikası Dönüş W-NW"), ("trail-curve-nw-ne", "Orman Patikası Dönüş NW-NE"),
    ("trail-curve-ne-e", "Orman Patikası Dönüş NE-E"), ("trail-three-way-e-sw-nw", "Orman Patikası Üçlü E-SW-NW"),
    ("trail-three-way-w-ne-se", "Orman Patikası Üçlü W-NE-SE"), ("trail-six-way", "Orman Patikası Altılı Kavşak"),
    ("river-straight-e-w", "Tropik Nehir Düz E-W"), ("river-straight-ne-sw", "Tropik Nehir Düz NE-SW"),
    ("river-straight-nw-se", "Tropik Nehir Düz NW-SE"), ("river-curve-e-se", "Tropik Nehir Dönüş E-SE"),
    ("river-curve-se-sw", "Tropik Nehir Dönüş SE-SW"), ("river-curve-sw-w", "Tropik Nehir Dönüş SW-W"),
    ("river-curve-w-nw", "Tropik Nehir Dönüş W-NW"), ("river-curve-nw-ne", "Tropik Nehir Dönüş NW-NE"),
    ("river-curve-ne-e", "Tropik Nehir Dönüş NE-E"), ("rope-bridge-e-w", "Halat Köprüsü E-W"),
    ("rope-bridge-ne-sw", "Halat Köprüsü NE-SW"), ("rope-bridge-nw-se", "Halat Köprüsü NW-SE"),
]

POI = [
    ("fishing-village", "Balıkçı Köyü"), ("tropical-port-town", "Tropik Liman Şehri"),
    ("jungle-explorer-lodge", "Orman Kâşifi Kulübesi"), ("sailor-beach-camp", "Denizci Sahil Kampı"),
    ("fruit-plantation", "Meyve Plantasyonu"), ("island-farmstead", "Ada Çiftliği"),
    ("overgrown-coastal-ruins", "Bitkilerle Kaplı Kıyı Harabeleri"), ("island-lighthouse", "Ada Deniz Feneri"),
    ("jungle-temple", "Orman Tapınağı"), ("tidal-shrine", "Gelgit Mabedi"),
    ("carved-island-idols", "Oyma Ada Putları"), ("pirate-graveyard", "Korsan Mezarlığı"),
    ("sea-cave", "Deniz Mağarası"), ("gemstone-mine", "Değerli Taş Madeni"),
    ("jungle-tunnel", "Orman Tüneli"), ("waterfall-grotto", "Şelale Mağarası"),
    ("lagoon-dock", "Lagün İskelesi"), ("ancient-banyan", "Kadim Banyan Ağacı"),
    ("pirate-fortress-camp", "Korsan Kale Kampı"), ("sea-serpent-nest", "Deniz Yılanı Yuvası"),
    ("island-witch-hut", "Ada Cadısı Kulübesi"), ("storm-signal-tower", "Fırtına İşaret Kulesi"),
    ("storm-magic-portal", "Fırtına Büyü Geçidi"), ("boss-island-citadel", "Patron Ada Kalesi"),
]

CATEGORIES = [
    ("base", "tropical-base-source-v1.png", BASE),
    ("connections", "tropical-connections-source-v1.png", CONNECTIONS),
    ("poi", "tropical-poi-source-v1.png", POI),
]


def metadata(category: str, tile_id: str, name_tr: str, index: int) -> dict:
    movement_cost = 2
    passable = True
    danger = 2
    requires = None

    if category == "base":
        if tile_id in {"golden-beach", "white-sand-beach", "tropical-flower-field", "jungle-clearing"}:
            movement_cost = 1
            danger = 1
        if tile_id in {"dense-tropical-jungle", "bamboo-grove", "mangrove-roots", "flooded-jungle"}:
            movement_cost = 3
        if tile_id in {"shallow-lagoon", "colorful-coral-reef"}:
            requires = "wading_or_boat"
        if tile_id == "deep-tropical-water":
            passable = False
            movement_cost = 99
            requires = "boat"
        if tile_id in {"coastal-cliff", "jungle-canyon"}:
            passable = False
            movement_cost = 99
        if tile_id in {"storm-soaked-ground", "flooded-jungle", "shipwreck-beach"}:
            danger = 3
    elif category == "connections":
        if tile_id.startswith("trail-") or tile_id.startswith("rope-bridge-"):
            movement_cost = 1
            danger = 1
        else:
            movement_cost = 3
            passable = False
            requires = "boat_or_bridge"
    else:
        movement_cost = 1 if tile_id in {
            "fishing-village", "tropical-port-town", "jungle-explorer-lodge", "fruit-plantation",
            "island-farmstead", "lagoon-dock"
        } else 2
        danger = 3 if tile_id in {
            "jungle-temple", "pirate-graveyard", "pirate-fortress-camp", "sea-serpent-nest",
            "island-witch-hut", "storm-magic-portal", "boss-island-citadel"
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
        pack_slug="tropical",
        pack_title="TROPICAL HEX TILESET V1",
        categories=CATEGORIES,
        metadata_fn=metadata,
        preview_labels=("BASE TERRAIN", "JUNGLE TRAILS / RIVERS / ROPE BRIDGES", "POINTS OF INTEREST"),
        preview_background=(10, 25, 18),
        preview_accent=(77, 226, 165),
    )
