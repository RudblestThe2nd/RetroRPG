# Desert Hex Tileset V1

Fantastik çöl biyomu için oynanabilir pointy-top hex paketidir. Forest ve Swamp paketleriyle aynı odd-row grid standardını kullanır.

## İçerik

- `tiles/base`: 24 temel çöl arazisi
- `tiles/connections`: 24 kervan yolu, vadi ve taş köprü bağlantısı
- `tiles/poi`: 24 yerleşim, harabe, görev ve tehlike noktası
- `atlases`: üç 1536x1024 PNG atlası ve sprite koordinat JSON dosyaları
- `manifests/desert-hex-tileset-v1.json`: oyun verileri ve dosya yolları
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

Manifestte her tile için Türkçe ad, kategori, hareket maliyeti, geçilebilirlik, tehlike seviyesi ve gerekiyorsa geçiş şartı bulunur. Batak kum ve uçurum platosu normal yürüyüşte geçilemez; yollar ve köprüler hareket maliyetini düşürür.

## Yeniden derleme

```powershell
python .\source\scripts\build_desert_tileset.py
```

Bu paket ortak `tilesets/tools/hex_pack_builder.py` derleyicisini kullanır.
