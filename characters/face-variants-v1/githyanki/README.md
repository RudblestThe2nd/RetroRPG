# Githyanki Face Variants V1

Karakter oluşturma ekranı için hazırlanmış 24 farklı Githyanki portresi paketidir.

## İçerik

- 12 maskülen ve 12 feminen yetişkin kimlik
- Sarı, hardal, koyu sarı, zeytin-sarı ve soluk açık yeşil ten çeşitleri
- Uzun dışa açılan kulaklar, dar/basık burun köprüsü ve keskin yüz oranları
- Farklı saç dokuları, kazınmış stiller, örgüler, yara ve benek seçenekleri
- 24 ayrı 256x256 RGBA PNG, 6x4 şeffaf atlas, manifest ve QA önizlemesi

## Kaynak ve üretim özeti

- Kaynak: `source/imagegen/githyanki-face-variants-source-v1.png`
- İstem özeti: 6x4 düzende 12 maskülen ve 12 feminen yetişkin Githyanki; sarı-zeytin ten, uzun sivri kulak ve Githyanki'ye özgü basık burun; çeşitli saç, yaş, yara ve benekler; ortak koyu tunik; şeffaf arka plan.

## Yeniden derleme

```powershell
python ..\tools\build_face_variant_pack.py --root . --race githyanki --title "Githyanki" --source "source\imagegen\githyanki-face-variants-source-v1.png"
```
