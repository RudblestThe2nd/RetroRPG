# Retro RPG sınıf diyagramı

`RetroRpg-Tum-Proje-Sinif-Diyagrami.drawio` dosyası diagrams.net/draw.io ile açılır.

## Sayfalar

1. Genel Mimari
2. Karakter, NPC ve Savaş
3. Sınıflar, Alt Sınıflar, Irklar ve Geçmiş
4. Eşya, Mağaza, Oturum ve Kayıt
5. Hikâye ve Görev Sistemi
6. Dünya, Harita ve Encounter
7. Asset, İçerik, Rendering ve Sahneler
8. WinForms UI ve Oyun Ekranları
9. Atlas Aracı ve Test Projesi

## Gösterim

- Açık yeşil başlık: sınıf veya tip adı.
- Birinci beyaz bölüm: özellikler/alanlar.
- İkinci beyaz bölüm: metotlar.
- Düz, içi boş üçgen ok: kalıtım.
- Kesik, içi boş üçgen ok: interface uygulaması.
- Siyah elmas: sahiplik/nesne içerme.
- Kesik gri ok: kullanım bağımlılığı.
- İtalik sınıf adı: abstract class.

## Yeniden üretme

Proje kökünde:

```powershell
.\tools\generate_class_diagram.ps1
```
