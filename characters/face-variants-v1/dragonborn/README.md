# Dragonborn Face Variants V1

Karakter oluşturma ekranı için hazırlanmış 24 farklı Dragonborn portresi paketidir.

## İçerik

- 12 maskülen ve 12 feminen yetişkin kimlik
- Kırmızı, bakır, bronz, altın, yeşil, mavi, beyaz, siyah ve gümüş pul çeşitleri
- Farklı ağız, kaş, çene plakası, boynuz, ibik ve boyun fırfırı siluetleri
- Tamamen ejderimsi/sürüngen baş yapısı; insan saçı kullanılmaz
- 24 ayrı 256x256 RGBA PNG, 6x4 şeffaf atlas, manifest ve QA önizlemesi

## Kaynak ve üretim özeti

- Kaynak: `source/imagegen/dragonborn-face-variants-source-v1.png`
- İstem özeti: 6x4 düzende 12 maskülen ve 12 feminen yetişkin Dragonborn; farklı metalik/kromatik pul renkleri, boynuz ve ibik biçimleri; ortak koyu tunik; şeffaf arka plan.

## Yeniden derleme

```powershell
python ..\tools\build_face_variant_pack.py --root . --race dragonborn --title "Dragonborn" --source "source\imagegen\dragonborn-face-variants-source-v1.png"
```
