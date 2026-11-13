# Orman Hex Tileset V1

Oynanabilir pointy-top hex dünya haritası için hazırlanmış ilk biyom paketidir.

## İçerik

- 24 taban arazi hex'i
- 12 yol bağlantısı
- 9 nehir bağlantısı
- 3 köprü yönü
- 24 yerleşim, zindan ve özel nokta
- Toplam 72 tekil şeffaf PNG
- 3 adet 6x4 atlas ve koordinat JSON'u
- Hareket maliyeti, geçilebilirlik ve tehlike manifesti

## Geometri

- Canvas: 256x256 piksel
- Yön: pointy-top
- Görsel hex genişliği: yaklaşık 222 piksel
- Hex yüksekliği: 256 piksel
- Dikey satır aralığı: 192 piksel
- Tek satır yatay ofseti: 111 piksel

## Klasörler

```text
forest-v1/
|-- tiles/{base,connections,poi}/
|-- atlases/
|-- manifests/
|-- previews/
`-- source/{imagegen,scripts}/
```

Paketi yeniden oluşturmak için:

```powershell
python .\source\scripts\build_forest_tileset.py
```
