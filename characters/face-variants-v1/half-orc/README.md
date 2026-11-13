# Half-Orc Face Variants V1

Karakter oluşturma ekranı için hazırlanmış 24 farklı Half-Orc portresi paketidir.

## İçerik

- 12 maskülen ve 12 feminen yetişkin kimlik
- Yeşil, zeytin, adaçayı ve gri-yeşil ten çeşitleri
- Sivri kulak, alt çene dişleri, güçlü yüz oranları, yara ve yaş çeşitliliği
- Kısa, uzun, kıvırcık, örgülü ve kazınmış saç biçimleri
- 24 ayrı 256x256 RGBA PNG, 6x4 şeffaf atlas, manifest ve QA önizlemesi

## Kaynak ve üretim özeti

- Kaynak: `source/imagegen/half-orc-face-variants-source-v1.png`
- İstem özeti: 6x4 düzende 12 maskülen ve 12 feminen yetişkin Half-Orc; çeşitli yeşil tenler, saçlar, yaralar, sivri kulaklar ve ölçülü alt çene dişleri; ortak koyu tunik; şeffaf arka plan.

## Yeniden derleme

```powershell
python ..\tools\build_face_variant_pack.py --root . --race half-orc --title "Half-Orc" --source "source\imagegen\half-orc-face-variants-source-v1.png"
```
