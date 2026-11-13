# Class Outfit Baselines V1

On iki temel fantastik RPG sınıfı için kadın ve erkek kıyafet kimliği baseline paketidir.

## İçerik

- 12 sınıf x 2 beden sunumu = 24 adet 256x256 şeffaf PNG
- `sheets/class-outfits-transparent-atlas-v1.png`: 6x4 oyun atlası
- `source/imagegen/class-outfits-source-v1.png`: ImageGen kaynak atlası
- `manifests/class-outfits-v1.json`: sınıf, beden sunumu, hücre ve dosya yolları
- `manifests/qa-report-v1.json`: alfa ve dosya kalite kontrolü
- `previews/class-outfits-v1-preview.png`: şeffaflık denetim görünümü

## Sınıflar

- Barbarian
- Bard
- Cleric
- Druid
- Fighter
- Monk
- Paladin
- Ranger
- Rogue
- Sorcerer
- Warlock
- Wizard

## Kullanım notu

Bu görseller sınıf kimliği ve karakter oluşturma önizlemesi için baseline'dır. Yürüme, saldırı ve büyü animasyonlarında kullanılacak gerçek donanım katmanları; karakter atlasındaki ayak, el, baş ve gövde ankrajlarına göre ayrıca hizalanmalıdır.

## Yeniden derleme

```powershell
python .\source\build_class_outfits.py
```
