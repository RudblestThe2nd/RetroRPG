# Biyom Düşman Paketleri V1

Bu klasör, oyunun 10 biyomu için tamamlanmış temel düşman sprite paketlerini içerir.

## Ortak teknik standart

- Her biyomda 12 farklı düşman bulunur.
- Her düşmanın `idle` ve `attack` olmak üzere iki pozu vardır.
- Her sprite 256x256 RGBA PNG'dir.
- Ortak pivot: `[128, 238]`.
- Kaynak atlas 1536x1024, 6 sütun x 4 satırdır.
- İlk iki satır ilk altı düşmanın idle/attack çiftleridir.
- Son iki satır kalan altı düşmanın idle/attack çiftleridir.
- Biyom klasörlerinde kaynak, ayrılmış sprite'lar, şeffaf atlas, manifest, önizleme ve QA raporu birlikte tutulur.

## Durum

- Tamamlandı: `forest`, `swamp`, `desert`, `snow`, `mountain`, `tropical`, `volcanic`, `corrupted`, `underground`, `plains`
- Mevcut toplam: 120 düşman, 240 sprite
- QA: 240/240 başarılı
- Hedef toplam: 120 düşman, 240 sprite

Hikâye illüstrasyonları bu üretim grubunun kapsamı dışındadır.
