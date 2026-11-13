# Retro Fantasy UI Asset Pack

Bu klasör, 800x800 çözünürlüklü oyun arayüzünde kullanılacak üretim varlıklarını içerir.

## Klasör düzeni

```text
assets/
|-- controls/
|   |-- buttons/{small,medium,large,icon}/
|   |-- tabs/
|   |-- scrollbars/{vertical,horizontal}/
|   `-- slots/{item,equipment,ability,portrait}/
|-- frames/{panels,modals,tooltips}/
|-- icons/{system,resources,classes,races,status_effects}/
|-- cursors/
|-- input/{keyboard,mouse}/
|-- animations/previews/
|-- atlases/
|-- manifests/
|-- previews/
`-- source/{imagegen,scripts}/
```

## İsimlendirme standardı

- Bütün adlar İngilizce ve küçük harflidir.
- Sözcükler `kebab-case` ile ayrılır.
- Etkileşim durumu dosyanın sonunda alt çizgiyle yazılır:
  - `_normal`
  - `_hover`
  - `_pressed`
  - `_disabled`
- Animasyon kareleri `_f00`, `_f01`, `_f02` biçimindedir.
- Sürümlü toplu dosyalar `-v1`, `-v2` biçiminde adlandırılır.

Örnekler:

```text
button-large_normal.png
button-large_pressed.png
slot-equipment_hover.png
status-poisoned_disabled.png
cursor-attack_f02.png
ui-controls-atlas-v1.png
```

## Etkileşim durumları

Tıklanabilir bütün kontroller dört kare içerir:

1. `normal`: bekleme durumu
2. `hover`: fare üzerindeyken altın parıltı
3. `pressed`: içerik iki piksel aşağı kaymış ve koyulaşmış durum
4. `disabled`: gri ve düşük opaklıklı durum

Önerilen basma sırası:

```text
normal -> hover -> pressed -> hover
```

Önerilen süreler `70 ms -> 90 ms -> 70 ms` şeklindedir. Oyun kodu görsel kareleri değiştirir; ikonun kendisi için ayrıca animasyon sistemi yazılması gerekmez.

## Boyut standardı

| Asset | Boyut |
|---|---:|
| Büyük düğme | 256x72 |
| Orta düğme | 192x64 |
| Küçük düğme | 128x48 |
| İkon düğmesi | 72x72 |
| Sekme | 176x52 |
| Eşya/ekipman yuvası | 80x80 |
| Genel ikon | 64x64 |
| Sınıf ikonu | 72x72 |
| Irk ikonu | 80x80 |
| İmleç karesi | 48x48 |

## Kaynak ve çıktı ayrımı

- `source/imagegen/`: görsel stil için üretilmiş yüksek çözünürlüklü kaynak atlaslar.
- `source/scripts/`: atlasları tekil oyun varlıklarına dönüştüren araçlar.
- Diğer klasörler: oyunun doğrudan yükleyebileceği PNG dosyaları.
- `atlases/`: MonoGame veya benzeri içerik sistemleri için paketlenmiş atlas ve koordinat JSON dosyaları.
- `manifests/`: bütün varlıkların boyut, durum ve yol bilgileri.

Paketi yeniden üretmek için:

```powershell
python .\source\scripts\build_ui_pack.py
```

Üretilmiş dosyalar elle değiştirilmemelidir. Kalıcı değişiklikler kaynak atlaslarda veya derleme betiğinde yapılmalıdır.
