# Eşya Paketi 02

Bu paket, ilk eşya paketinde bulunmayan üretim malzemelerini, kamp araçlarını ve hikâye görev eşyalarını tamamlar.

## İçerik

- 24 üretim malzemesi
- 24 kamp ve keşif aracı
- 24 görev eşyası ve büyülü kalıntı
- 6 nadirlik çerçevesi
- 3 adet 6x4 sprite sheet
- Oyun istatistiklerini içeren JSON manifest
- Atlas koordinat JSON dosyaları

## Klasör düzeni

```text
item-pack-02/
|-- icons/
|   |-- materials/
|   |-- camp-tools/
|   |-- quest-relics/
|   `-- rarity-frames/
|-- sheets/
|-- atlases/
|-- manifests/
|-- previews/
`-- source/
    |-- imagegen/
    `-- scripts/
```

Tekil eşya ikonları 256x256 piksel ve şeffaf arka planlıdır. Dosyalar sayı öneki ve İngilizce `kebab-case` kimlik kullanır:

```text
01-firewood.png
14-lantern.png
24-world-map-fragment.png
```

Paketi yeniden üretmek için:

```powershell
python .\source\scripts\build_item_pack.py
```
