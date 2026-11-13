# Underground / Cave Hex Tileset V1

Mağaralar, madenler, yeraltı yerleşimleri ve derin zindan bölgeleri için oynanabilir pointy-top hex paketidir. Diğer biyom paketleriyle aynı odd-row grid standardını kullanır.

## İçerik

- `tiles/base`: 24 mağara zemini, kristal, mantar, yeraltı suyu, uçurum ve biyom geçişi
- `tiles/connections`: 24 mağara yolu, maden rayı, yeraltı nehri ve köprü bağlantısı
- `tiles/poi`: 24 yerleşim, maden, tapınak, zindan, görev ve boss noktası
- `atlases`: üç 1536x1024 PNG atlası ve sprite koordinat JSON dosyaları
- `manifests/underground-hex-tileset-v1.json`: oyun verileri ve dosya yolları
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

Manifestte Türkçe ad, hareket maliyeti, geçilebilirlik, tehlike ve özel geçiş şartı bulunur. Normal mağara keşfi `light_source`; göl, uçurum ve yarıklar `bridge_flight_or_boat`; nehirler `boat_or_bridge`; lav zemini `fire_immunity` gerektirir.

`crystal-dragon-boss-chamber`, paketin tehlike seviyesi 5 olan final boss noktasıdır.

## Yeniden derleme

```powershell
python .\source\scripts\build_underground_tileset.py
```

Paket ortak `tilesets/tools/hex_pack_builder.py` derleyicisini kullanır.
