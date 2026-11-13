# Drow Face Variants V1

Karakter oluşturma ekranı için hazırlanmış 24 farklı Drow portresi paketidir.

## İçerik

- 12 maskülen ve 12 feminen yetişkin kimlik
- Uzun sivri kulaklar ve zarif, köşeli yüz oranları
- Arduvaz, kömür, soğuk mor, lavanta ve mavi-gri ten çeşitleri
- Beyaz, gümüş, soluk lavanta saçlar; düz, dalgalı, kıvırcık, örgülü ve kısa stiller
- Kırmızı, mor, mavi ve amber göz çeşitleri
- 24 ayrı 256x256 RGBA PNG, 6x4 şeffaf atlas, manifest ve QA önizlemesi

## Yeniden derleme

```powershell
python ..\tools\build_face_variant_pack.py --root . --race drow --title "Drow Face Variants V1" --source "drow-face-variants-source-v1.png"
```
