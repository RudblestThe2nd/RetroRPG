# Kader Yolları — Hikâye Animasyon Durumu

## Tamamlanan final sekanslar

1. `tower-fall-v1` — Vaelor'un kader darbesi ve oyuncunun kuleden düşüşü — 6 kare.
2. `rider-revival-v1` — Kalpaklı Süvari'nin gelişi, diriltme ve zırhı sunması — 4 kare.
3. `fatebreaker-return-v1` — Oyuncunun dönüşü ve zırhın kader saldırısını kırması — 4 kare.

Toplam: **14 adet 800x800 final PNG kare**, 3 kaynak contact sheet ve 3 GIF önizleme.

Ortak kural: Ana karakter, kişiselleştirmeden bağımsız kullanılması için saf siyah ve özelliksiz humanoid silüettir.

## Tamamlanan ana hikâye sekansları

1. `caravan-prologue-v1` — parçalanmış kervan, Kara Harita Parçası ve üç yol — 4 kare.
2. `root-waystone-v1` — Kök Taşı etkinleşmesi — 4 kare.
3. `sky-waystone-v1` — Gök Taşı etkinleşmesi — 4 kare.
4. `sun-waystone-v1` — Güneş Taşı etkinleşmesi — 4 kare.
5. `archive-gate-v1` — Kırık Arşiv mesajı ve mor bölgenin tek girişinin açılması — 4 kare.
6. `corrupted-region-v1` — unutulan mülteciler, silinmiş devriye, Göz Piskoposu ve anı yankısı — 4 kare.
7. `fate-lens-v1` — Kader Merceği ve Vaelor'un ikinci formu — 4 kare.
8. `endings-v1` — dört ayrı final epilogu — 4 kare.

Yeni paket toplamı: **32 adet 800x800 final PNG kare**, 8 kaynak contact sheet ve 7 GIF önizleme.

Final boss paketiyle birlikte genel toplam: **46 adet 800x800 final PNG kare**, 11 kaynak contact sheet ve 10 GIF önizleme.

Ana hikâye kökü: `assets/final-production-v6/story/cinematics/main-story/`.
Olay kimlikleri ve dosya eşlemesi: `assets/final-production-v6/story/cinematics/main-story/manifest.json`.

## Teknik kullanım

- Sekanslar loop etmez.
- Kareler sırayla gösterilir; sahne metni görsellere gömülmez.
- Metin ve seçimler oyun tarafından ayrı UI katmanında çizilir.
- GIF dosyaları yalnız hızlı önizleme içindir; oyun PNG karelerini kullanır.
- Kaderbozan Zırhı'nın şeffaf ekipman katmanları ayrı asset paketidir; sinematiklerdeki zırh çizimi yalnız hikâye karesidir.

