# Volcanic Hex Tileset V1

Fantastik yanardağlar, lav bölgeleri ve volkanik kötü araziler için oynanabilir pointy-top hex paketidir. Diğer biyom paketleriyle aynı odd-row grid standardını kullanır.

## İçerik

- `tiles/base`: 24 kül, bazalt, lav, kükürt, obsidyen ve geçiş arazisi
- `tiles/connections`: 24 bazalt yol, lav nehri ve demir köprü bağlantısı
- `tiles/poi`: 24 yerleşim, harabe, görev ve tehlike noktası
- `atlases`: üç 1536x1024 PNG atlası ve sprite koordinat JSON dosyaları
- `manifests/volcanic-hex-tileset-v1.json`: oyun verileri ve dosya yolları
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

Manifestte Türkçe ad, hareket maliyeti, geçilebilirlik, tehlike ve özel geçiş şartı bulunur. Lav gölü, lav kanyonu ve magma uçurumu `fire_immunity`; kükürt, obsidyen ve kaldera alanları `heat_protection` gerektirir.

## Yeniden derleme

```powershell
python .\source\scripts\build_volcanic_tileset.py
```

Paket ortak `tilesets/tools/hex_pack_builder.py` derleyicisini kullanır.
