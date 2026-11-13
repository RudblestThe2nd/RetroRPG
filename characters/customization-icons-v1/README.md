# Character Customization Icons V1

Karakter oluşturma ekranındaki seçim listeleri için hazırlanmış 120 adet şeffaf pixel-art ikondur.

## İçerik

- 24 saç stili
- 24 sakal ve bıyık stili
- 24 kesin renk değerli ten tonu
- 24 boynuz çifti
- 24 yüz ve beden işareti
- Her kategori için 128x128 tekil RGBA PNG, 6x4 atlas, manifest, QA raporu ve önizleme

## Kullanım ilkeleri

- Görsel ikonlar birer seçim önizlemesidir; karakter sprite katmanına doğrudan yapıştırılmaz.
- C# tarafı `customization-icons-v1.json` içindeki `allowed_races` alanına göre seçenekleri filtrelemelidir.
- Ten rengi seçimi `skin-tone-icons-v1.json` kimliğine bağlanır; gerçek sprite rengi aynı kimliğin palet/tint değeriyle uygulanır.
- Boynuz seçenekleri yalnız Tiefling ve Dragonborn için açılır.
- Dragonborn için saç ve sakal kapalıdır; pul/ibik özelleştirmesi ayrı bir sonraki katman paketinde ele alınır.
- İşaretler bütün ırklara açıktır; görünmeyen veya anatomik olarak uyumsuz bölgeler C# tarafındaki ırk ankraj profiliyle filtrelenir.

## ImageGen kaynak özeti

- `hair-icons-source-v1.png`: 24 farklı saç-only silueti; doğal renkler; şeffaf 6x4 atlas.
- `beard-icons-source-v1.png`: 24 farklı sakal/bıyık-only silueti; şeffaf 6x4 atlas.
- `horn-icons-source-v1.png`: 24 farklı çift boynuz; kemik, metalik, volkanik ve infernal malzeme çeşitleri.
- `marking-icons-source-v1.png`: yara, savaş boyası, doğa, göksel ve mistik motiflerden 24 özgün işaret.
- Ten tonu kaynağı kesin HEX değerlerini korumak için deterministik Python çizimiyle üretilir.

## Yeniden derleme

```powershell
python .\tools\build_skin_tone_source.py
python .\tools\build_customization_icon_category.py --root . --category hair --title "Hair Styles" --source "hair-icons-source-v1.png"
python .\tools\build_customization_icon_category.py --root . --category beard --title "Facial Hair" --source "beard-icons-source-v1.png"
python .\tools\build_customization_icon_category.py --root . --category skin-tone --title "Skin Tones" --source "skin-tone-icons-source-v1.png"
python .\tools\build_customization_icon_category.py --root . --category horn --title "Horn Styles" --source "horn-icons-source-v1.png"
python .\tools\build_customization_icon_category.py --root . --category marking --title "Character Markings" --source "marking-icons-source-v1.png"
```
