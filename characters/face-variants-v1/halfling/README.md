# Halfling Face Variants V1

Karakter oluşturma ekranı için hazırlanmış 24 farklı Halfling portresi paketidir.

## İçerik

- 12 maskülen ve 12 feminen yetişkin kimlik
- Kompakt yüz oranları, kısa sivri kulaklar ve sıcak ifade yapısı
- Kıvırcık, dalgalı, örgülü, kısa ve kırlaşmış saç seçenekleri
- Maskülen portrelerde temiz, favorili, bıyıklı ve kısa sakallı seçenekler
- Farklı yaş, ten tonu, göz ve saç renkleri
- 24 ayrı 256x256 RGBA PNG, 6x4 şeffaf atlas, manifest ve QA önizlemesi

## Yeniden derleme

```powershell
python ..\tools\build_face_variant_pack.py --root . --race halfling --title "Halfling Face Variants V1" --source "halfling-face-variants-source-v1.png"
```
