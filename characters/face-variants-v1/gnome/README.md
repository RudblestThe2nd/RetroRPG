# Gnome Face Variants V1

Karakter oluşturma ekranı için hazırlanmış 24 farklı Gnome portresi paketidir.

## İçerik

- 12 maskülen ve 12 feminen yetişkin kimlik
- Büyük dışa açılan kulaklar, ince çene ve canlı göz oranları
- Genç yetişkinden yaşlıya uzanan yaş çeşitliliği
- Kıvırcık, dalgalı, kısa, örgülü, seyrek ve beyaz saç seçenekleri
- Farklı ten, göz ve saç renkleri; ölçülü sakal ve bıyık çeşitleri
- 24 ayrı 256x256 RGBA PNG, 6x4 şeffaf atlas, manifest ve QA önizlemesi

## Yeniden derleme

```powershell
python ..\tools\build_face_variant_pack.py --root . --race gnome --title "Gnome Face Variants V1" --source "gnome-face-variants-source-v1.png"
```
