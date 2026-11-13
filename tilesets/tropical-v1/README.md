# Tropical Hex Tileset V1

Fantastik tropik adalar, lagünler, kıyılar ve yoğun ormanlar için oynanabilir pointy-top hex paketidir. Diğer biyom paketleriyle aynı odd-row grid standardını kullanır.

## İçerik

- `tiles/base`: 24 plaj, su, mercan, orman, uçurum ve geçiş arazisi
- `tiles/connections`: 24 orman patikası, tropik nehir ve halat köprüsü bağlantısı
- `tiles/poi`: 24 yerleşim, harabe, görev ve tehlike noktası
- `atlases`: üç 1536x1024 PNG atlası ve sprite koordinat JSON dosyaları
- `manifests/tropical-hex-tileset-v1.json`: oyun verileri ve dosya yolları
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

Manifestte Türkçe ad, kategori, hareket maliyeti, geçilebilirlik, tehlike ve özel geçiş şartı bulunur. Lagün ve mercan resifi `wading_or_boat`, derin tropik su `boat` gerektirir. Kıyı uçurumu ve orman kanyonu normal yürüyüşle geçilemez.

## Yeniden derleme

```powershell
python .\source\scripts\build_tropical_tileset.py
```

Paket ortak `tilesets/tools/hex_pack_builder.py` derleyicisini kullanır.
