# Karakter Görsel Sistemi — Üretim Manifestosu

Sürüm: `v1`  
Üretim tarihi: `2026-10-28`  
Üretim yöntemi: Codex yerleşik ImageGen, referans görsel tabanlı raster üretim  
Biçim: PNG / RGBA / şeffaf arka plan

## Kapsam

- 11 oynanabilir ırk
- Her ırk için erkek ve kadın temel beden: toplam 22 animasyon atlası
- Her ırk için erkek ve kadın yüzlerini içeren 1 ifade sayfası: toplam 11 portre sayfası
- Toplam 33 üretim görseli

Her animasyon atlasında şu hareket grupları bulunur:

- Ön, arka, sol ve sağ bekleme görünümü
- Dört yönde yürüme ana kareleri
- Dört yönde silahsız yakın dövüş saldırısı
- Dört yönde büyü yapma hareketi
- Savunma, hasar alma ve dört aşamalı ölüm
- Konuşma, oturma ve eşya kullanma pozu

Her portre sayfasında aynı erkek ve kadın karakter için altı ifade bulunur:

- Nötr
- Dostça
- Öfkeli
- Korkmuş
- Yaralı / bitkin
- Kararlı

## Irk ve dosya tablosu

| Irk | Erkek atlası | Kadın atlası | Portre sayfası |
|---|---|---|---|
| İnsan | `atlases/human/male/human-male-animation-atlas-v1.png` | `atlases/human/female/human-female-animation-atlas-v1.png` | `portraits/human/human-portrait-expressions-v1.png` |
| Elf | `atlases/elf/male/elf-male-animation-atlas-v1.png` | `atlases/elf/female/elf-female-animation-atlas-v1.png` | `portraits/elf/elf-portrait-expressions-v1.png` |
| Cüce | `atlases/dwarf/male/dwarf-male-animation-atlas-v1.png` | `atlases/dwarf/female/dwarf-female-animation-atlas-v1.png` | `portraits/dwarf/dwarf-portrait-expressions-v1.png` |
| Buçukluk | `atlases/halfling/male/halfling-male-animation-atlas-v1.png` | `atlases/halfling/female/halfling-female-animation-atlas-v1.png` | `portraits/halfling/halfling-portrait-expressions-v1.png` |
| Gnom | `atlases/gnome/male/gnome-male-animation-atlas-v1.png` | `atlases/gnome/female/gnome-female-animation-atlas-v1.png` | `portraits/gnome/gnome-portrait-expressions-v1.png` |
| Yarı Elf | `atlases/half-elf/male/half-elf-male-animation-atlas-v1.png` | `atlases/half-elf/female/half-elf-female-animation-atlas-v1.png` | `portraits/half-elf/half-elf-portrait-expressions-v1.png` |
| Drow | `atlases/drow/male/drow-male-animation-atlas-v1.png` | `atlases/drow/female/drow-female-animation-atlas-v1.png` | `portraits/drow/drow-portrait-expressions-v1.png` |
| Yarı Ork | `atlases/half-orc/male/half-orc-male-animation-atlas-v1.png` | `atlases/half-orc/female/half-orc-female-animation-atlas-v1.png` | `portraits/half-orc/half-orc-portrait-expressions-v1.png` |
| Tiefling | `atlases/tiefling/male/tiefling-male-animation-atlas-v1.png` | `atlases/tiefling/female/tiefling-female-animation-atlas-v1.png` | `portraits/tiefling/tiefling-portrait-expressions-v1.png` |
| Dragonborn | `atlases/dragonborn/male/dragonborn-male-animation-atlas-v1.png` | `atlases/dragonborn/female/dragonborn-female-animation-atlas-v1.png` | `portraits/dragonborn/dragonborn-portrait-expressions-v1.png` |
| Githyanki | `atlases/githyanki/male/githyanki-male-animation-atlas-v1.png` | `atlases/githyanki/female/githyanki-female-animation-atlas-v1.png` | `portraits/githyanki/githyanki-portrait-expressions-v1.png` |

## Ana görsel referansları

- `../../baselines/01-human-elf-dwarf-halfling.png`
- `../../baselines/02-gnome-halfelf-drow-halforc.png`
- `../../baselines/03-tiefling-dragonborn-githyanki.png`

Irk kimliği ilk olarak ilgili temel sayfadan alınmıştır. Atlas düzeni ve poz yoğunluğu, onaylanan insan atlaslarıyla kilitlenmiş; kadın/erkek eşleri aynı ırkın tamamlanan ilk atlasını ek referans olarak kullanmıştır. Portreler doğrudan tamamlanan erkek ve kadın atlaslarından türetilmiştir.

## Ortak üretim istemi özeti

Atlaslar için istem; 1990'ların sonu fantastik RPG piksel sanatı, sert piksel kümeleri, sınırlı toprak paleti, koyu kontur, tek ve tutarlı karakter, tam beden, şeffaf RGBA arka plan, dört yön ve bütün temel aksiyon gruplarını şart koşar. Silah, zırh, büyü efekti, sahne, zemin, gölge, yazı, etiket ve ızgara özellikle yasaklanmıştır.

Portre istemi; aynı kimliği ve kıyafet yakasını koruyan iki sıra hâlinde erkek/kadın yüzleri, aynı kamera açısı ve ışık, altı okunaklı ifade ve şeffaf RGBA arka planı şart koşar. Yazı, çerçeve, ızgara, ilave karakter ve kesilmiş kafa/kulak/boynuz yasaklanmıştır.

## Teknik kalite denetimi

- PNG sayısı: 33
- Animasyon atlası: 22
- Portre sayfası: 11
- Renk modu: 33/33 RGBA
- Şeffaflık: 33/33 dosyada gerçek alfa kanalı ve en az bir şeffaf piksel
- Atlas çözünürlüğü: 22/22 dosyada 1536 x 1024
- Portre çözünürlükleri: 1536 x 1024, 1774 x 887 veya Dragonborn için 2172 x 724
- Görsel denetim: ırk silueti, cinsiyet eşleşmesi, yön çeşitliliği, hareket grupları, portre ifade sırası ve istenmeyen yazı/ızgara açısından incelendi

## Motor entegrasyonu notu

Bu dosyalar `v1` kaynak atlaslarıdır. Oyun motoruna alınırken otomatik dikdörtgen dilimleme yerine karakterleri alfa sınırına göre ayıran bir içe aktarma aracı kullanılmalıdır. Karelerin piksel boyutları görsel üretimde eşit hücre ızgarasına zorlanmadığı için her parçanın pivotu ayak tabanının orta noktası olarak normalize edilmelidir. Ekipmanlar temel bedenin üstüne ayrı katman olarak bindirilmelidir; ırka göre tutuş noktaları kod tarafındaki ankraj profillerinden okunmalıdır.

