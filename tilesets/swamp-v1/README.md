# Swamp Hex Tileset V1

Karanlık fantezi bataklık biyomu için oynanabilir pointy-top hex paketidir. Forest Hex Tileset V1 ile aynı grid standardını kullanır.

## İçerik

- `tiles/base`: 24 temel arazi hex'i
- `tiles/connections`: 24 patika, kanal ve ahşap geçit hex'i
- `tiles/poi`: 24 yerleşim, harabe, görev ve tehlike noktası
- `atlases`: üç 1536x1024 PNG atlası ve sprite koordinat JSON dosyaları
- `manifests/swamp-hex-tileset-v1.json`: oyun verileri ve dosya yolları
- `manifests/qa-report-v1.json`: otomatik kalite kontrol sonucu
- `previews`: genel paket görünümü ve gerçek odd-row birleşme testi

## Grid standardı

- Yönelim: pointy-top
- Tile canvas: 256x256 px
- Görsel hex genişliği: 222 px
- Görsel hex yüksekliği: 256 px
- Yatay adım: 222 px
- Dikey adım: 192 px
- Tek numaralı satır X ofseti: 111 px
- Hex köşeleri: `(128,0) (239,64) (239,192) (128,255) (17,192) (17,64)`

## Oynanış verileri

Manifestte her tile için Türkçe ad, kategori, hareket maliyeti, geçilebilirlik, tehlike seviyesi ve gerekiyorsa `boat` veya `wading_or_boat` şartı bulunur.

## Yeniden derleme

```powershell
python .\source\scripts\build_swamp_tileset.py
```

Derleyici kaynak atlasları 72 ayrı şeffaf PNG'ye böler, oyun atlaslarını ve manifestleri yeniden oluşturur, köşe şeffaflığını kontrol eder.
