# Corrupted / Dark Hex Tileset V1

Bozulmuş topraklar, gölge diyarları ve ana kötünün hüküm sürdüğü karanlık bölgeler için oynanabilir pointy-top hex paketidir. Diğer biyom paketleriyle aynı odd-row grid standardını kullanır.

## İçerik

- `tiles/base`: 24 çorak arazi, yozlaşmış orman, kristal sırt, uçurum, yarık ve biyom geçişi
- `tiles/connections`: 24 lanetli yol, gölge nehri ve rün köprüsü bağlantısı
- `tiles/poi`: 24 yerleşim, harabe, görev, tehlike ve ana hikâye noktası
- `atlases`: üç 1536x1024 PNG atlası ve sprite koordinat JSON dosyaları
- `manifests/corrupted-hex-tileset-v1.json`: oyun verileri ve dosya yolları
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

Manifestte Türkçe ad, hareket maliyeti, geçilebilirlik, tehlike ve özel geçiş şartı bulunur. Yozlaşmış bölgelerde `corruption_resistance`; boşluk havuzu, gölge kanyonu, uçurum ve yarıklarda `void_travel`; gölge nehri geçişlerinde `rune_bridge_or_void_travel` şartları kullanılır.

`watching-eye-tower`, ana kötünün yaşadığı dev göz kuleli gökdelen ve hemen arkasındaki uçurum için hazırlanmış final hikâye noktasıdır. Tehlike seviyesi 5'tir.

## Yeniden derleme

```powershell
python .\source\scripts\build_corrupted_tileset.py
```

Paket ortak `tilesets/tools/hex_pack_builder.py` derleyicisini kullanır.
