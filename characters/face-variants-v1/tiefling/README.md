# Tiefling Face Variants V1

Karakter oluşturma ekranı için hazırlanmış 24 farklı Tiefling portresi paketidir.

## İçerik

- 12 maskülen ve 12 feminen yetişkin kimlik
- Kırmızı, kızıl, mor, lavanta, gri ve mavi-gri ten çeşitleri
- Farklı koç, kıvrımlı, kısa ve geriye uzanan boynuz çiftleri
- Sivri kulaklar, cehennemî göz paleti, çeşitli saç dokuları ve yüz oranları
- 24 ayrı 256x256 RGBA PNG, 6x4 şeffaf atlas, manifest ve QA önizlemesi

## Kaynak ve üretim özeti

- Kaynak: `source/imagegen/tiefling-face-variants-source-v1.png`
- İstem özeti: 6x4 düzende 12 maskülen ve 12 feminen yetişkin Tiefling; farklı ten, saç, göz ve boynuz siluetleri; tüm boynuzlar hücre içinde; ortak koyu tunik ve şeffaf arka plan.

## Yeniden derleme

```powershell
python ..\tools\build_face_variant_pack.py --root . --race tiefling --title "Tiefling" --source "source\imagegen\tiefling-face-variants-source-v1.png"
```
