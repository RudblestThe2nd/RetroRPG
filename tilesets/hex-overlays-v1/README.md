# Hex Exploration and State Overlays V1

Hex dünya haritasının keşif, rota, görev ve etkileşim durumlarını arazi görselini değiştirmeden göstermek için hazırlanmış şeffaf üst katman paketidir.

## İçerik

- `static`: 32 adet 256x256 RGBA üst katman
- `animated`: 5 nabız animasyonu ve 1 hareketli sis GIF'i
- `atlases/hex-overlays-static-atlas-v1.png`: 8x4 statik overlay atlası
- `manifests/hex-overlays-v1.json`: dosya yolları, kategoriler ve çizim sırası
- `manifests/qa-report-v1.json`: otomatik kalite kontrolü
- `previews/hex-overlays-v1-preview.png`: ova tile'ı üzerindeki bütün durumlar

## Keşif durumları

- Keşfedilmemiş
- Sis altında
- Daha önce görülmüş fakat görüş dışında
- Yeni açılmış
- Altı yön için ayrı sis sınırı

## Harita durumları

- Seçili ve hover
- Kilitli ve geçiş engelli
- Tehlike, ana görev ve yan görev
- Oyuncu konumu
- Rota başlangıcı, adımı ve bitişi
- Erişilebilir, erişilemez ve hareket menzili
- Düşman, dost, rastgele karşılaşma ve boss
- Kamp, yerleşim, ganimet ve tamamlanmış hedef

## Çizim sırası

```text
terrain -> exploration -> route -> state -> player
```

## Yeniden derleme

```powershell
python .\source\scripts\build_hex_overlays.py
```
