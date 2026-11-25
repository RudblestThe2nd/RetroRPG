# Kader Yolları: Göz Kulesi

C# / .NET 9 WinForms ile geliştirilen, sade sınıf kalıtımı ve d20 tabanlı mekanikler kullanan retro RPG projesi.
Oyun tam ekranda açılır. Harita ekran genişliğini kullanır. Diğer ekranlar 800×800 mantıksal pikselden
piksel kenarları korunarak ölçeklenir. `Ctrl + / -` pencere boyutunu, `F11` tam ekranı değiştirir.

Batı, merkez ve doğu tek parşömen üzerinde 45×12 tıklanabilir hex oluşturur. Sağ fare tuşuyla haritayı sürükleyebilir, hedef seçip rotayı onaylayabilirsin.

Karakterler savaşlardan XP kazanır, 10. seviyeye kadar gelişir ve seçilen 24 alt sınıfın her biri
kendine özgü saldırı, iyileştirme veya koruma gücü kullanır. Sınıf gücü uzun dinlenmede yenilenir.
Savaşlarda zar sesi, saldırı hamlesi, isabet/ıskalama/blok sesi, ekran sarsıntısı, darbe parlaması
ve yükselen hasar yazısı birlikte çalışır. Sekiz durum etkisi, beş düşman davranışı, silah türüne
özel teknikler ve beş kademeli ganimet/nadirlik sistemi aktiftir. Menü, biyom, savaş, yerleşim,
han, kamp, hikâye ve ölüm için ayrı döngüsel retro müzikler; biyom, yerleşim, han ve kamp için
ayrı düşük sesli ortam katmanları vardır. Müzik/ortam ile efekt sesi AYARLAR ekranından
birbirinden bağımsız değiştirilebilir.

## Hızlı başlangıç

```powershell
dotnet build RetroRpg.sln
dotnet run --project src/RetroRpg.Game/RetroRpg.Game.csproj
dotnet run --project tests/RetroRpg.Tests/RetroRpg.Tests.csproj
```

Kodun nereden okunacağı için [README-CODE.md](README-CODE.md), geliştirme kuralları için
[DEVELOPER_GUIDE.md](DEVELOPER_GUIDE.md), güncel devir durumu için [DEVIR.md](DEVIR.md) kullanılır.

## Klasörler

| Klasör | Amaç |
|---|---|
| `src/RetroRpg.Core/` | Oyun kuralları, karakterler, savaş, hikâye, harita ve kayıt sistemi |
| `src/RetroRpg.Core/Characters/` | Irk, class, subclass, geçmiş, oyuncu ve bütün NPC sınıfları; alt klasör kullanmaz |
| `src/RetroRpg.Game/` | WinForms ekranları, çizim ve kullanıcı girişi |
| `tests/RetroRpg.Tests/` | Otomatik testler ve uçtan uca smoke senaryosu |
| `Content/` | JSON oyun tanımları ve asset kataloğu |
| `assets/`, `characters/`, `items/`, `maps/`, `scenes/`, `tilesets/`, `ui/` | Çalışma zamanında kullanılan oyun görselleri |
| `documents/` | Tasarım, araştırma ve encounter belgeleri, sınıf diyagramı |
| `tools/` | Atlas aracı, harita engel editörü ve asset üretim betikleri |

## Gereksinim

.NET 9 SDK (Windows). Oyun açılırken asset klasörlerini depo kökünde arar; exe başka bir yerden
çalıştırılacaksa `RETRO_RPG_ASSET_ROOT` ortam değişkeni depo kökünü göstermelidir.

Tek dosyalık exe üretmek için:

```powershell
dotnet publish src/RetroRpg.Game/RetroRpg.Game.csproj -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -p:DebugType=none -o dist
```

## Önemli kural

Asset klasörleri sürüm numarası bakımından dağınık görünse de aktif kod hâlâ V1–V6 arasındaki
dosyalara başvurur. Bu klasörler, kod ve manifest yolları birlikte değiştirilmeden silinmemelidir.
