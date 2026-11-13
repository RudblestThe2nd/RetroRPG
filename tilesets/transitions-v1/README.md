# Biome Transition Masks V1

Mevcut bütün pointy-top biyom hexlerini birbirine bağlayan, kod tarafından yeniden kullanılabilir geçiş maskesi paketidir.

## İçerik

- `tiles/masks`: altı komşu yönün bütün kombinasyonlarını kapsayan 64 RGBA maske
- `samples`: 10 biyom arasındaki 45 benzersiz ikili geçiş örneği
- `atlases/biome-transition-mask-atlas-v1.png`: 8x8 maske atlası
- `manifests/biome-transition-masks-v1.json`: yön bitleri ve dosya yolları
- `manifests/qa-report-v1.json`: sayı ve dosya kalite kontrolü
- `previews`: maske matrisi ve 45 biyom çifti önizlemesi

## Kullanım

Kaynak biyom tile'ı altta, hedef biyom tile'ı üstte çizilir. Seçilen maske PNG'sinin alfa kanalı hedef biyomun karışım miktarı olarak kullanılır.

Yön bit sırası:

```text
bit 0: E
bit 1: SE
bit 2: SW
bit 3: W
bit 4: NW
bit 5: NE
```

Komşu biyomlardan oluşturulan 0–63 arası bit değeri doğrudan doğru maske dosyasını seçer. Böylece 10 biyom için ayrı ayrı binlerce geçiş resmi gerekmez.

## Yeniden derleme

```powershell
python .\source\scripts\build_transition_masks.py
```
