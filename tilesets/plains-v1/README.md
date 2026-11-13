# Plains / Grassland Hex Tileset V1

Ovalar, tarım alanları, kır yerleşimleri ve açık arazi maceraları için oynanabilir pointy-top hex paketidir. Diğer biyom paketleriyle aynı odd-row grid standardını kullanır.

## İçerik

- `tiles/base`: 24 çayır, tarla, tepe, akarsu ve biyom geçişi
- `tiles/connections`: 24 toprak yol, dere, köprü ve kavşak bağlantısı
- `tiles/poi`: 24 köy, çiftlik, kale, kamp, harabe, görev ve boss noktası
- `atlases`: üç 1536x1024 PNG atlası ve sprite koordinat JSON dosyaları
- `manifests/plains-hex-tileset-v1.json`: oyun verileri ve dosya yolları
- `manifests/qa-report-v1.json`: otomatik kalite kontrol sonucu
- `previews`: genel paket görünümü ve odd-row birleşme testi

## Grid standardı

- Yönelim: pointy-top
- Tile canvas: 256x256 px
- Görsel hex: 222x256 px
- Yatay adım: 222 px
- Dikey adım: 192 px
- Tek numaralı satır X ofseti: 111 px
- Hex köşeleri: `(128,0) (239,64) (239,192) (128,255) (17,192) (17,64)`

## Oynanış verileri

Manifestte Türkçe ad, hareket maliyeti, geçilebilirlik, tehlike ve özel geçiş şartı bulunur. Gölet ve sığ dereler `shallow_water_travel`; bağlantı dereleri `bridge_or_shallow_water_travel` gerektirir. Açık ovalar genel olarak hızlı ve düşük tehlikelidir.

`golden-griffin-boss-nest`, paketin tehlike seviyesi 5 olan final boss noktasıdır.

## Yeniden derleme

```powershell
python .\source\scripts\build_plains_tileset.py
```

Paket ortak `tilesets/tools/hex_pack_builder.py` derleyicisini kullanır.
