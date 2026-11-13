# Proje Asset Envanteri

Güncelleme tarihi: 2026-10-31

Bu dosya yalnızca gerçek oyun asset klasörlerini kapsar. Word/PDF renderları ve QA sayfaları dahil edilmez.

## Genel toplam

| Grup | PNG | GIF |
|---|---:|---:|
| maps | 8 | 0 |
| scenes | 15 | 0 |
| characters | 3120 | 0 |
| items | 165 | 0 |
| ui | 535 | 42 |
| tilesets | 946 | 6 |
| assets | 1335 | 21 |
| **Toplam** | **6124** | **69** |

## Haritalar

Toplam 8 harita görseli:

- `maps/00-selected-main-world-map.png`
- `maps/01-ana-kita-coklu-biyom.png`
- `maps/02-buz-kitası.png`
- `maps/03-tropik-takimada.png`
- `maps/04-col-ve-ic-deniz.png`
- `maps/05-mantar-ve-curume-kitası.png`
- `maps/06-parcalanmis-gokyuzu-dunyasi.png`
- `maps/hex/world-map-hex-2d-v1.png`

## Biyom ve rota sahneleri

- 8 ana biyom sahnesi
- 7 tropik fırtına kıyısı rota sahnesi

- `scenes/biomes/01-ancient-forest-crossroads.png`
- `scenes/biomes/02-haunted-swamp-settlement.png`
- `scenes/biomes/03-desert-canyon-city.png`
- `scenes/biomes/04-shattered-glacier-temple.png`
- `scenes/biomes/05-tropical-storm-coast.png`
- `scenes/biomes/06-mushroom-crystal-wilds.png`
- `scenes/biomes/07-volcanic-badlands-approach.png`
- `scenes/biomes/08-eye-tower-abyss.png`
- `scenes/routes/tropical-storm-coast/lighthouse-route/00-jungle-descent.png`
- `scenes/routes/tropical-storm-coast/lighthouse-route/01-route-selection.png`
- `scenes/routes/tropical-storm-coast/lighthouse-route/02-shallow-lagoon-causeway.png`
- `scenes/routes/tropical-storm-coast/lighthouse-route/03-tidal-reef-crossing.png`
- `scenes/routes/tropical-storm-coast/lighthouse-route/04-broken-pier.png`
- `scenes/routes/tropical-storm-coast/lighthouse-route/05-lighthouse-island-ascent.png`
- `scenes/routes/tropical-storm-coast/lighthouse-route/06-lighthouse-courtyard.png`

## Oynanabilir hex tilesetleri

### Forest Hex Tileset V1

- 72 adet şeffaf arka planlı 256x256 tekil pointy-top hex
- 24 temel arazi, 24 yol/nehir/köprü bağlantısı, 24 yerleşim ve ilgi noktası
- 3 oyun atlası ve atlas koordinat JSON dosyaları
- Türkçe ad, hareket maliyeti, geçilebilirlik ve tehlike seviyesi içeren manifest
- Odd-row dizilim için birleşme testi ve otomatik QA raporu
- Grid standardı: 222x256 görsel hex, 222 yatay adım, 192 dikey adım, tek satırda 111 px ofset

### Swamp Hex Tileset V1

- 72 adet şeffaf arka planlı 256x256 tekil pointy-top hex
- 24 temel arazi, 24 patika/kanal/ahşap geçit, 24 yerleşim ve ilgi noktası
- 3 oyun atlası ve atlas koordinat JSON dosyaları
- Hareket maliyeti, geçilebilirlik, tehlike ve tekne gereksinimi içeren manifest
- Forest Hex Tileset V1 ile aynı odd-row grid standardı
- Birleşme testi ve 72/72 başarılı otomatik QA raporu

### Desert Hex Tileset V1

- 72 adet şeffaf arka planlı 256x256 tekil pointy-top hex
- 24 temel arazi, 24 kervan yolu/vadi/taş köprü, 24 yerleşim ve ilgi noktası
- 3 oyun atlası ve atlas koordinat JSON dosyaları
- Hareket maliyeti, geçilebilirlik, tehlike ve geçiş şartları içeren manifest
- Forest ve Swamp paketleriyle aynı odd-row grid standardı
- Birleşme testi ve 72/72 başarılı otomatik QA raporu

### Ortak hex üretim aracı

- `tilesets/tools/hex_pack_builder.py` yeni biyomları aynı geometriyle derler
- Tekil PNG, atlas, koordinat JSON, manifest, önizleme ve QA raporu otomatik oluşturulur

### Snow Hex Tileset V1

- 72 adet şeffaf arka planlı 256x256 tekil pointy-top hex
- 24 temel arazi, 24 kar yolu/donmuş akarsu/ahşap köprü, 24 yerleşim ve ilgi noktası
- 3 oyun atlası ve atlas koordinat JSON dosyaları
- Hareket maliyeti, geçilebilirlik, tehlike ve buz yolculuğu şartları içeren manifest
- Diğer biyom paketleriyle aynı odd-row grid standardı
- Birleşme testi ve 72/72 başarılı otomatik QA raporu

### Mountain Hex Tileset V1

- 72 adet şeffaf arka planlı 256x256 tekil pointy-top hex
- 24 temel arazi, 24 dağ yolu/nehir/köprü, 24 yerleşim ve ilgi noktası
- 3 oyun atlası ve atlas koordinat JSON dosyaları
- Hareket maliyeti, geçilebilirlik, tehlike ve tırmanış şartları içeren manifest
- Diğer biyom paketleriyle aynı odd-row grid standardı
- Birleşme testi ve 72/72 başarılı otomatik QA raporu

### Tropical Hex Tileset V1

- 72 adet şeffaf arka planlı 256x256 tekil pointy-top hex
- 24 temel arazi, 24 orman patikası/nehir/halat köprüsü, 24 yerleşim ve ilgi noktası
- 3 oyun atlası ve atlas koordinat JSON dosyaları
- Hareket maliyeti, geçilebilirlik, tehlike ve tekne şartları içeren manifest
- Diğer biyom paketleriyle aynı odd-row grid standardı
- Birleşme testi ve 72/72 başarılı otomatik QA raporu

### Volcanic Hex Tileset V1

- 72 adet şeffaf arka planlı 256x256 tekil pointy-top hex
- 24 temel arazi, 24 bazalt yol/lav nehri/demir köprü, 24 yerleşim ve ilgi noktası
- 3 oyun atlası ve atlas koordinat JSON dosyaları
- Hareket maliyeti, geçilebilirlik, tehlike, ısı koruması ve ateş bağışıklığı şartları
- Diğer biyom paketleriyle aynı odd-row grid standardı
- Birleşme testi ve 72/72 başarılı otomatik QA raporu

### Corrupted / Dark Hex Tileset V1

- 72 adet şeffaf arka planlı 256x256 tekil pointy-top hex
- 24 temel arazi, 24 lanetli yol/gölge nehri/rün köprüsü, 24 yerleşim ve ilgi noktası
- 3 oyun atlası ve atlas koordinat JSON dosyaları
- Hareket maliyeti, geçilebilirlik, tehlike, yozlaşma direnci ve boşluk yolculuğu şartları
- Ana hikâye için uçurum önündeki dev Watching Eye Tower final noktası
- Diğer biyom paketleriyle aynı odd-row grid standardı
- Birleşme testi ve 72/72 başarılı otomatik QA raporu

### Underground / Cave Hex Tileset V1

- 72 adet şeffaf arka planlı 256x256 tekil pointy-top hex
- 24 mağara arazisi, 24 yol/ray/nehir/köprü, 24 yerleşim ve ilgi noktası
- 3 oyun atlası ve atlas koordinat JSON dosyaları
- Hareket maliyeti, geçilebilirlik, tehlike, ışık, tekne, köprü ve ateş bağışıklığı şartları
- Kristal Ejder final boss odası
- Diğer biyom paketleriyle aynı odd-row grid standardı
- Birleşme testi ve 72/72 başarılı otomatik QA raporu

### Plains / Grassland Hex Tileset V1

- 72 adet şeffaf arka planlı 256x256 tekil pointy-top hex
- 24 ova arazisi, 24 toprak yol/dere/köprü, 24 yerleşim ve ilgi noktası
- 3 oyun atlası ve atlas koordinat JSON dosyaları
- Hareket maliyeti, geçilebilirlik, tehlike ve sığ su yolculuğu şartları
- Altın Griffin final boss yuvası
- Diğer biyom paketleriyle aynı odd-row grid standardı
- Birleşme testi ve 72/72 başarılı otomatik QA raporu

### Biome Transition Masks V1

- Altı komşu yönün bütün kombinasyonlarını kapsayan 64 RGBA geçiş maskesi
- 10 tamamlanmış biyom arasındaki 45 benzersiz ikili geçiş örneği
- 8x8 maske atlası, yön-bit manifesti ve iki QA önizlemesi
- Yeni biyomlara da yeniden çizim gerektirmeden uygulanabilir
- 64/64 maske ve 45/45 biyom çifti başarılı otomatik QA raporu

### Hex Exploration and State Overlays V1

- Keşif, sis, rota, seçim, kilit, tehlike, görev ve birim durumları için 32 RGBA overlay
- Altı komşu yön için ayrı sis sınırları
- Seçim, görev, tehlike, oyuncu ve rota için 5 nabız GIF'i; 1 hareketli sis GIF'i
- 8x4 statik atlas, oyun manifesti ve bütün durumları gösteren QA önizlemesi
- 32/32 statik ve 6/6 animasyon başarılı otomatik QA raporu

## Karakter sistemi

- 3 ırk baseline sayfası
- 11 oynanabilir ırk
- Kadın ve erkek olmak üzere 22 animasyon atlası
- 11 portre ve ifade sayfası
- Irklar: human, elf, dwarf, halfling, gnome, half-elf, drow, half-orc, tiefling, dragonborn, githyanki

### Class Outfit Baselines V1

- 12 temel sınıf için kadın ve erkek kıyafet baseline'ları: 24 şeffaf PNG
- Barbarian, Bard, Cleric, Druid, Fighter, Monk, Paladin, Ranger, Rogue, Sorcerer, Warlock ve Wizard
- 6x4 şeffaf atlas, sınıf manifesti ve şeffaflık QA önizlemesi
- Karakter oluşturma önizlemesi ve ilerideki animasyon ekipman katmanları için görsel referans

### Class Outfit Animation Layers V1 — Tamamlandı

- On iki temel sınıfın tamamı için yürüyüş, saldırı ve büyü kıyafet katmanları
- Her sınıfta kadın/erkek, dört yön ve üç hareket: 24 kare; toplam 288 adet 256x256 RGBA PNG
- Sınıf başına 6x4 şeffaf atlas, kare manifesti, ortak [128,236] pivotu ve QA önizlemesi
- 288/288 başarılı otomatik QA; sınıf paketinin genel durumu 12/12 tamamlandı

### Human Face Variants V1

- Karakter oluşturma ekranı için 12 maskülen ve 12 feminen insan portresi
- Yüz yapısı, yaş, ten tonu, saç rengi, saç dokusu ve sakal çeşitliliği
- 24 ayrı 256x256 RGBA portre, 6x4 atlas, manifest ve QA önizlemesi

### Elf Face Variants V1

- Karakter oluşturma ekranı için 12 maskülen ve 12 feminen Elf portresi
- Uzun sivri kulak, yüz yapısı, ten, göz, saç rengi ve saç dokusu çeşitliliği
- 24 ayrı 256x256 RGBA portre, 6x4 şeffaf atlas, manifest ve QA önizlemesi

### Dwarf Face Variants V1

- Karakter oluşturma ekranı için 12 maskülen ve 12 feminen Cüce portresi
- Güçlü yüz oranları; çeşitli örgülü, halkalı, kısa ve uzun sakal/saç biçimleri
- 24 ayrı 256x256 RGBA portre, 6x4 şeffaf atlas, manifest ve QA önizlemesi

### Halfling Face Variants V1

- Karakter oluşturma ekranı için 12 maskülen ve 12 feminen yetişkin Halfling portresi
- Kısa sivri kulaklar; çeşitli kıvırcık, örgülü, kısa saç ve ölçülü sakal biçimleri
- 24 ayrı 256x256 RGBA portre, 6x4 şeffaf atlas, manifest ve QA önizlemesi

### Gnome Face Variants V1

- Karakter oluşturma ekranı için 12 maskülen ve 12 feminen yetişkin Gnome portresi
- Büyük kulaklar; genç yetişkinden yaşlıya yaş, saç, sakal, ten ve göz çeşitliliği
- 24 ayrı 256x256 RGBA portre, 6x4 şeffaf atlas, manifest ve QA önizlemesi

### Half-Elf Face Variants V1

- Karakter oluşturma ekranı için 12 maskülen ve 12 feminen Half-Elf portresi
- İnsan yüz çeşitliliği, zarif Elf oranları ve ölçülü sivri kulak birleşimi
- 24 ayrı 256x256 RGBA portre, 6x4 şeffaf atlas, manifest ve QA önizlemesi

### Drow Face Variants V1

- Karakter oluşturma ekranı için 12 maskülen ve 12 feminen Drow portresi
- Koyu arduvaz/mor ten, gümüş-beyaz saç, sivri kulak ve yeraltı göz paleti
- 24 ayrı 256x256 RGBA portre, 6x4 şeffaf atlas, manifest ve QA önizlemesi

### Half-Orc Face Variants V1

- Karakter oluşturma ekranı için 12 maskülen ve 12 feminen Half-Orc portresi
- Yeşil ten tonları, sivri kulaklar, ölçülü alt çene dişleri, saç, yara ve yaş çeşitliliği
- 24 ayrı 256x256 RGBA portre, 6x4 şeffaf atlas, manifest ve QA önizlemesi

### Tiefling Face Variants V1

- Karakter oluşturma ekranı için 12 maskülen ve 12 feminen Tiefling portresi
- Kırmızı/mor/gri tenler; sivri kulak, cehennemî göz, saç ve boynuz silueti çeşitliliği
- 24 ayrı 256x256 RGBA portre, 6x4 şeffaf atlas, manifest ve QA önizlemesi

### Dragonborn Face Variants V1

- Karakter oluşturma ekranı için 12 maskülen ve 12 feminen Dragonborn portresi
- Kromatik/metalik pul renkleri; farklı boynuz, ibik, çene ve sürüngen yüz yapıları
- 24 ayrı 256x256 RGBA portre, 6x4 şeffaf atlas, manifest ve QA önizlemesi

### Githyanki Face Variants V1

- Karakter oluşturma ekranı için 12 maskülen ve 12 feminen Githyanki portresi
- Sarı/zeytin ten, dışa uzanan sivri kulak, basık burun; saç, yara, benek ve yaş çeşitliliği
- 24 ayrı 256x256 RGBA portre, 6x4 şeffaf atlas, manifest ve QA önizlemesi

### Character Customization Icons V1

- Saç, sakal, ten tonu, boynuz ve işaret kategorilerinde toplam 120 seçim ikonu
- Her kategoride 24 adet 128x128 RGBA PNG, 6x4 atlas, manifest, önizleme ve QA raporu
- Irka göre seçenek filtreleme tablosu ve 24 kesin HEX ten rengi
- 120/120 başarılı otomatik QA raporu

### Civilian and Profession NPC Sprites V1

- 12 sivil rolün kadın/erkek eşleri: 24 tam beden NPC
- 12 meslek rolünün kadın/erkek eşleri: 24 tam beden NPC
- Toplam 48 adet 256x256 RGBA sprite, 2 şeffaf atlas, rol manifestleri ve QA önizlemeleri
- İnsan ve fantastik soy çeşitliliği; yerleşim, dükkân, görev veren ve kalabalık kullanımları
- 48/48 başarılı otomatik QA raporu

### Biome Enemy Packs V1

- Tamamlanan biyomlar: Orman, Bataklık, Çöl, Kar, Dağ, Tropik, Volkanik, Corrupted, Yeraltı ve Ova (10/10)
- Her biyomda 12 düşman ve düşman başına idle/attack çifti
- Toplam 120 düşman, 240 adet 256x256 RGBA sprite, 10 şeffaf atlas ve 10 QA önizlemesi
- Ortak pivot [128, 238]; 240/240 başarılı otomatik QA raporu

### Mini-Boss and Main Boss Sprites V1

- 10 biyom için 10 mini-boss ve 10 ana boss: toplam 20 boss
- Her boss için idle/attack çifti; toplam 40 adet 256x256 RGBA sprite
- Ortak pivot [128, 240], large/huge çarpışma profilleri ve iki şeffaf atlas
- 40/40 başarılı otomatik QA raporu

### Map Tokens V1

- 22 oyuncu, 48 NPC, 120 düşman, 10 mini-boss ve 10 ana boss tokenı
- Toplam 210 adet 96x96 RGBA token ve 13 kategori/biyom atlası
- Ortak pivot [48, 48]; sınıfa göre renk kodlu çerçeveler
- 210/210 başarılı otomatik QA raporu

## Final Production V1

- Kalan hikâye dışı ekipman, sahne, efekt, UI ve 3D harita assetleri
- 96 silah karesi, 384 kuşanılabilir parça karesi, 150 düşürülmüş ganimet sprite'ı ve 22 ankraj profili
- 54 rota sahnesi, 12 iç mekân, 10 savaş arka planı, 50 hava varyantı, 24 prop ve 4 kamp
- 112 efekt karesi, 20 animasyon GIF'i, 15 adet 800x800 UI ekranı, 120 ikon ve 1 isometrik dünya haritası
- Paket manifest durumu complete; QA pass
- Konum: assets/final-production-v1/

## Final Production V5 — oynanabilir kapanış paketi

- 42 adet 128 x 128 geçmiş/köken ikonu: köken, aile, çocukluk, eğitim, meslek, dönüm noktası ve amaç için altışar seçenek
- 7 adet 800 x 800 runtime ekranı: özellik dağıtımı, karakter özeti, envanter, karakter kaydı, günlük, kamp ve ölüm
- 6 adet savaş/harita modalı ve 1 autosave sistem ikonu
- Köy, kasaba, kale dış görünüşleri ile han, pazar, demirci, şifacı, tapınak, kışla, savaş odası ve zindan: 11 yerleşim sahnesi
- Bütün biyomlara dağıtılmış 50 adet 800 x 800 savaş dışı encounter sahnesi
- Final dosyalar çalışma zamanında bağlıdır; konsept/placeholder değildir
- Dosya manifesti: `assets/final-production-v5/manifest.csv`
- 29 ekranlık eski final Release çalışma zamanı QA: `qa/archive/history/screenshots-final-release-v3/`

## Eşya sistemi

### Item Pack 01

- 24 silah ve kalkan ikonu
- 24 zırh ve aksesuar ikonu
- 24 tüketilebilir eşya ve ganimet ikonu
- 3 kaynak sprite sheet
- 2 kuşanılmış karakter karşılaştırma sayfası

### Item Pack 02

- 24 üretim malzemesi ikonu
- 24 kamp ve keşif aracı ikonu
- 24 görev eşyası ve büyülü kalıntı ikonu
- 6 nadirlik çerçevesi
- 3 temiz sprite sheet ve atlas koordinatları
- Eşya ağırlığı, değeri, istif sınırı ve görev durumu manifesti

## UI sistemi

- 508 tekil UI PNG asseti
- 112 adet normal/hover/pressed/disabled etkileşim grubu
- 42 GIF animasyon önizlemesi
- Düğmeler, sekmeler, scrollbarlar ve slotlar
- Panel, modal ve tooltip çerçeveleri
- Sistem, kaynak, sınıf, ırk ve durum etkisi ikonları
- 12 animasyonlu imleç
- Klavye ve fare giriş göstergeleri
- UI atlasları ve koordinat JSON dosyaları

## Tamamlanan ekran tasarımları

- Hex dünya haritası/hareket ekranı
- Savaş ekranı
- Envanter ve ekipman ekranı
- Karakter ekranı
- Günlük ve görev ekranı
- Kamp ekranı
- Beş ana ekranın hem yüksek çözünürlüklü hem 800x800 runtime sürümleri

## Tam dosya listesi

Bütün güncel görsel dosyaların yolu ve dosya boyutu `asset-files-v5.csv` içinde bulunur.
