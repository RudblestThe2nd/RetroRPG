# DEVIR — Retro RPG

Son güncelleme: 1 Kasım 2026

Bu dosya projeye yeni başlayan biri (insan ya da yapay zekâ) için durum belgesidir. Önce bunu, sonra kökteki `DEVELOPER_GUIDE.md` dosyasını oku.

## Durum özeti

- C# / .NET 9 Windows Forms, 800 x 800 pencere, sınıf ve kalıtım ödevi olarak yazılan retro piksel RPG.
- Oynanabilir akış: ana menü → kimlik → ırk → sınıf/alt sınıf → 7 aşamalı geçmiş → 3 puanlık özellik dağıtımı → karakter özeti → 3 yuvalı hikâye seçimi → **Kader Yolları: Göz Kulesi prologu** → hex dünya haritası → Bölüm I → üç Yol Taşı → mor bölge → Göz Kulesi → scripted yenilgi → Kalpaklı Süvari → gerçek Vaelor savaşı → dört finalden biri.
- Bütün ekranlar hazır assetlerle çiziliyor. Windows Forms kontrolü kullanılmıyor; her şey 800 x 800 alana çiziliyor.
- Derleme 0 hata, 0 uyarı. 56 testin hepsi geçiyor (33 eski + 23 hikâye, biri prologdan finale uçtan uca smoke testi). Ekran görüntüsü modu 80 PNG üretir; eksik asset logu ve metin taşma raporu boş.
- Pencere başlığı `Retro RPG`; ana menüdeki merkez ve flama göz logolarının tamamı kaldırıldı.
- `world-map-parchment-2d-v1.png` üstten bakışlı 2D dünya resmi gerçekçi piksel-art parşömen üzerinde gösterilir. Parşömenin iç harita alanına hizalanmış 15 x 12, toplam 180 tıklanabilir hex varsayılan olarak görünmez; `HEX: KAPALI/AÇIK` düğmesi veya `H` tuşu çizgileri açıp kapatır. Seçim, rota ve görev ayrı katmanlarda çizilir; tehlike ve düşman haritada gösterilmez. Oyuncu hedefi seçer, BFS rotasını görür, onaylar ve token küçük komşu hexler arasında yumuşak animasyonla yürür. Gizli tehlikeli hexe girerken d20 + Bilgelik encounter kontrolü oynar.
- İki düz bariyer kaldırıldı. Engeller mor bölge sınırı, dağ kayalıkları, lav gölleri, bataklık çukurları, çöl kanyonları, buz uçurumları ve mağara çökmeleri biçiminde düzensiz kümelerdir. Mor bölgenin tek girişi `(-5,-3)` güney kapısıdır (`(-3,-4)` editör engelidir); kapı üç Yol Taşı tamamlanınca açılır. Keşiş/halat köprü kısa dağ geçidini; maden asansörü yeraltı kısayolunu açar.
- **Hiç commit yok.** Git deposunun kökü `game_design` değil, bir üst klasör olan `C:\Users\VECTUS\Documents\pc`. Bu depoda başka dosyalar da var.

## 1 Kasım — Ana hikâye koda bağlandı

- **Çekirdek (`src\RetroRpg.Core\Story\`):** `StoryEvent` → `DialogueStoryEvent` / `SkillCheckStoryEvent` / `CombatStoryEvent` / `ScriptedDefeatStoryEvent` / `EndingStoryEvent`; `Quest` → `MainQuest` / `WaystoneQuest` / `SideQuest`; `StoryConsequence` → `FlagConsequence` / `ItemConsequence` / `ReputationConsequence` / `RouteConsequence` / `EndingConsequence` (+ `QuestConsequence`, `CostConsequence`). Bütün olay ve görevler `StoryCatalog.cs` içinde sabit kimliklerle; tetik ve zincir seçimi `StoryDirector.cs`; finaller `EndingRules.cs`; manifest okuyucu `CinematicLibrary.cs`; durum `StoryProgress.cs`.
- **Tetikler:** `Start` (prolog), `Arrive` (rota o hexte biterse veya ÇEVREYİ ARA), `Pass` (geçerken, mor bölge ve kule), `Search` (kervan enkazı), `Inn` (Yeşilgeçit hanı, `TANIKLARI DİNLE`), `Chain` (önceki olayın ardından). Yarım kalan zincir `StoryProgress.ActiveNodeId` ile kayda yazılır; autosave'den yükleyince aynı düğümden sürer.
- **Koordinatlar:** Yeşilgeçit `(0,0)`, kervan enkazı `(1,0)`, Kızılburç `(2,-1)`, Kök Taşı `(-4,-1)`, Gök Taşı `(4,-3)`, Güneş Taşı `(3,3)`, mor kapı `(-5,-3)`, devriye `(-5,-4)`, köprü/anı/Piskopos `(-4,-5)`, Göz Kulesi `(-3,-5)`.
- **Mor bölge:** Eski kodda mor bölgeye üç yerden girilebiliyordu. Artık editör engelleri (8 hex, test sabit) dışındaki bütün mor hexler `HexTile.IsSealed` ile mühürlü. Üç taş tamamlanınca `HexMap.OpenPurpleGate()` yalnız `(-5,-3) → (-5,-4) → (-4,-5)` yolunu açar; diğer mor sınırlar kapalı kalır, kapı yolunda rastgele savaş yoktur.
- **Kayıt sürüm 3:** `GameSaveData.Story` (`StoryProgressSaveData`). Sürüm 1/2 kayıtlar güvenli varsayılanlarla açılır ve prologdan başlar. Eski sürüm dosyasının üzerine ilk kez yazılmadan önce `slot-1.v{N}-backup.json` yedeği alınır; eski dosya içeriği hiç değiştirilmez.
- **Ekranlar:** `StorySceneScreen` (PNG kare oynatıcı, metin kutusu, seçenek, d20, sonuç; tıklama/Enter sonraki kare), `EndingScreen` (Vaelor'un kaderi → dört final → epilog), gerçek görevleri gösteren `JournalScreen`. `CombatScreen` hikâye savaşlarını `CombatStoryEvent` ile oynatır; ilk Vaelor savaşında oyuncu ilk eylemi yapar, sonra `Kaderin Hükmü` modalı açılır ve ölüm ekranı açılmaz.
- **Gök Taşı savaş dalı:** `Muhafızla savaş` seçimi uyanış karelerini oynatmaz (`StoryChoice.PlaysAfterFrames = false`). Uyanış yalnız muhafız yenilince `sky-waystone-seized` olayında oynar; kaçışta oynamaz ve taş seçenekleri yeniden açılır.
- **Paylaştırma finali:** Yüksek güven yalnız `EndingRules.TrustRegions` içindeki taş bölgelerinde sayılır (`swamp`, `mountain`, `desert`). Yeşilgeçit ve mülteci güveni tutulur ama bölge sayılmaz.
- **Kaderbozan Zırhı:** `Core\Items\LegendaryArmor.cs`, `Armor` alt sınıfı, yeni `EquipmentSlot.StoryLayer` yuvası (göğüs zırhı yerinde kalır), +1 AC, Kaderin Hükmü'nü kırar, bir kez ölümcül darbeyi 1 CAN'da durdurur, satılamaz.
- **Final hikâye görselleri:** Kaderbozan artık envanterde kendine ait ikonu kullanır (`items\item-pack-01\icons\armor\25-fatebreaker-armor-v1.png`); güney/idle ekipman katmanı da gelecek katman birleştiricisi için hazırdır. Mevcut çalışma zamanı karakter çizicisi henüz giyilebilir katmanları ana gövdeye birleştirmez. Vaelor için iki ayrı final sprite çifti vardır: ilk karşılaşmadaki iki kollu Baş Haritacı formu ile gerçek final savaşındaki çok kollu Kader Gözü formu (`characters\story-boss-sprites-v1`). Göz Piskoposu kendi `eye-bishop` sprite'ını kullanır. Kule arşivi/hapishanesi/kapısı için mevcut v1/v5 sahneleri kullanılmaya devam eder.
- **Ekran görüntüsü modu** artık kendi kayıt klasörünü kullanır (`<çıktı>\qa-saves`); oyuncunun gerçek kaydına yazmaz. `text-overflow.log` iki yazı boyutuna da sığmayan metinleri listeler.

## 31 Ekim tamamlanma güncellemesi

- Final Production V5 tamamlandı: 42 anlamlı geçmiş ikonu, 7 runtime ekranı, 6 modal, 11 yerleşim/iç mekân ve 50 savaş dışı encounter sahnesi.
- Karakter oluşturma sonunda üç özellik puanı dağıtılır, karakter özeti görülür; haritaya doğrudan atlanmaz.
- Harita menüsünden envanter, karakter kaydı, günlük ve kamp açılır. `ÇEVREYİ ARA` mevcut hexte yerleşim bulursa giriş onayı, aksi hâlde biyoma uygun savaş dışı encounter açar.
- Köy, kasaba ve kale `WorldLocation` tabanından; keşif, gezgin ve tehlike encounterları `WorldEncounter` tabanından türetilmiştir. Bu yapı ödevin kalıtım hedefini görünür ve okunabilir tutar.
- Savaşta saldırı türü seçilmeden önce isabet bonusu ve hasar aralığı görünür. D20 ara kareleri `?` ile maskelenir; ekrandaki ve uygulanan ham değer aynı nesneden gelir. Kaçış Dayanıklılık d20 kontrolü, yorgunluk hasarı ve başarısızlıkta serbest saldırı kullanır. Sprite üstü can barları kaldırılmıştır.
- Göz Kulesi hexinde encounter zarı yoktur. Ana hikâyede kuleye girişi hikâye olayları yönetir; mor kapı açılınca kule rastgele savaştan temizlenir. Haritada düşman/tehlike görünmez.
- Yenilgi gerçek ölüm ekranına gider; `AUTOSAVE YÜKLE` son otomatik kaydı geri açar. Rota bitimi, savaş/encounter ve yerleşim işlemleri autosave üretir.
- Bütün 50 savaş dışı encounter iki seçenek taşır. Seçeneklerin statı, zorluğu, başarı ödülü ve başarısızlık cezası ayrıdır; tam liste `documents\RANDOM_ENCOUNTERS.md` içindedir.
- Pazar ve demirci satın alma/satma sekmeleri açar. Kuşanılmış eşya satılırken otomatik çıkarılır; görev/kimlik eşyaları satılmaz. Envanterde `KUŞAN/ÇIKAR` geçişi vardır.
- Hana girmek ücretsizdir; ücret yalnız oyuncu dinlenmeyi seçince alınır. Han söylentileri hikâye encounterları için kanca bırakır. Kışla ve savaş odası muhafızlıdır; İkna, Güç/Gözdağı veya uygun geçmiş kimliği gerekir.
- Başlangıç ekipmanı sınıf + geçmiş + hikâye seçimine göre kurulur. Kamp yemeği altın değil gerçek malzeme tüketir.
- Gereksiz tam-vücut gösterimleri karakter oluşturma/özellik/özet/envanterden kaldırıldı. Saç ve sakal/yüz detayı varyantları aktiftir; savaş sprite'ı daha sert pikselleştirilir.
- Ortak merkezleme rutini ve ekran bazlı koordinatlar yenilendi; özellik, özet, envanter, karakter, günlük, POI, encounter ve mağaza metinleri 800 x 800 QA ile düzeltildi.
- Güncel görsel dosya listesi `asset-files-v5.csv`; V5 manifesti `assets\final-production-v5\manifest.csv`; düzensiz engelleri ve mor bölgenin tek girişini içeren son QA renderları `qa\screenshots-organic-final-v9\`. Araştırma ve kararlar `documents\MECHANICS_RESEARCH.md` içindedir.

## Dosya konumları

| Ne | Nerede |
|---|---|
| Proje kökü (asset kökü) | `C:\Users\VECTUS\Documents\pc\game_design` |
| Çözüm | `RetroRpg.sln` |
| Rehber belgesi (hedef) | `documents\word\Retro_RPG_Mega_Prompt_Asset_Rehberi_v9.docx` (önceki sürümler silinmedi) |
| Kod notları | `README-CODE.md` |
| Tam geliştirici rehberi | `DEVELOPER_GUIDE.md` |
| Asset envanteri | `ASSET_INVENTORY.md`, `asset-files-v1.csv`, `VISUAL_PRODUCTION_PROGRESS.md` |
| Yayınlanmış exe | `dist\RetroRpg.Game.exe` |
| Masaüstü kısayolu | `C:\Users\VECTUS\Desktop\Göz Kulesi (Retro RPG).lnk` → `dist\RetroRpg.Game.exe` |
| Kayıt dosyası | `%LocalAppData%\RetroRpg\Saves\slot-1.json` |
| Eksik asset logu | `%LocalAppData%\RetroRpg\missing-assets.log` |
| İçerik verisi | `Content\definitions\game-data-v1.json` (ırk, sınıf, alt sınıf, geçmiş, biyom adları, eşya) |
| Düşman verisi | `Content\definitions\enemies-v1.json` (120 düşman, Türkçe ad, tier, sınıf) |
| Merkezi asset kataloğu | `Content\manifests\asset-catalog-v1.json` |
| İçerik şeması | `Content\schemas\game-data-v1.schema.json` |

## Çalıştırma

`game_design` klasöründe:

```powershell
dotnet build RetroRpg.sln
dotnet run --project tests/RetroRpg.Tests
dotnet run --project src/RetroRpg.Game
```

Exe'yi yeniden üretmek için:

```powershell
dotnet publish src/RetroRpg.Game/RetroRpg.Game.csproj -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -p:DebugType=none -o dist
```

Ekran görüntüsü modu, ekranları tıklamadan kontrol etmek için. Her ekranı 800 x 800 PNG olarak kaydeder:

```powershell
dist\RetroRpg.Game.exe --screenshots C:\bir\klasor
```

Çıktı 80 PNG'dir: eski 33 ekran (ana menü/ayarlar/tam ekran, karakter oluşturma, özellik, özet, hikâye seçimi, harita, envanter, karakter, günlük, kamp, encounter, kasaba, mağaza, han, muhafız, savaş, zar, ölüm) ve `22-` ile `42-` arası ana hikâye: prolog (anlatım, seçim, zar, sonuç), han tanığı, kervan enkazı, Kızılburç arşivi, görev günlüğü, üç Yol Taşı, zirve muhafızı savaşı, Kırık Arşiv, mor kapı, mor bölge olayları, Göz Piskoposu, kule katları, ilk Vaelor savaşı ve Kaderin Hükmü, kuleden düşüş, Süvari, Kaderbozan envanteri, zırhlı dönüş, Vaelor ikinci form ve final savaşı, final kararı ve dört epilog. `01c-tam-ekran.png` bilinçli olarak tam ekran çözünürlüğündedir.

**Exe neden yerinden taşınmamalı:** Exe `game_design` klasörünün içinde durmalı. Açılırken üst klasörlerde `assets` ve `characters` klasörlerini arar. Başka yerde çalışacaksa `RETRO_RPG_ASSET_ROOT` ortam değişkeni proje kökünü göstermeli. Exe .NET 9 runtime ister.

## Kod yapısı

### `src\RetroRpg.Core` — oyun kuralları (ekrandan bağımsız)

**`Entities\GameEntity.cs`** — Her şeyin atası. `Id` ve `Name` burada.

**`Characters\`**
- `PlayerCharacter.cs` — `GameEntity` ve `ICombatant`'tan türüyor. Irk, sınıf, geçmiş, stat, envanter ve ekipman tutuyor. `DefenseBonus`, "Savun" seçildiğinde bir tur için AC'ye eklenen değer.
- Karakter, ırk, sınıf, alt sınıf, geçmiş ve NPC dosyalarının tamamı yeni başlayanların kolay bulabilmesi için doğrudan bu klasördedir. Hepsi `RetroRpg.Core.Characters` namespace'ini kullanır; alt klasör yoktur.
- `Npc.cs`, `FriendlyNpc.cs`, `EnemyNpc.cs`, `BiomeEnemyNpc.cs`, `MiniBossNpc.cs`, `MainBossNpc.cs` — NPC kalıtım zinciri.
- `EnemyCatalog.cs` — `enemies-v1.json` dosyasını okuyor. Hangi hexte hangi düşmanın çıkacağını seçiyor; aynı hexte hep aynı düşman çıkıyor ve güç merkezden uzaklaştıkça artıyor. Düşmanın sprite ve token yollarını veriyor, tier'a göre can, AC ve hasar değeri olan düşman üretiyor.
- Karakter sınıfı kalıtımı:
  - Soyut `CharacterClass` taban sınıf.
  - Her ana sınıf ayrı bir dosyada. Örneğin `FighterClass.cs` içinde iki alt sınıfı (`ChampionClass` ve `BattleMasterClass`) da duruyor.
  - `ClassCatalog` 12 ana sınıfı ve 24 alt sınıfı tutuyor.
- `CharacterRace.cs` — 11 ırk ve `RaceCatalog`.
- `CharacterBackground.cs` — köken, aile, çocukluk, eğitim, meslek, dönüm noktası ve amaç olmak üzere 7 aşama; her aşamada 6 seçenek. Seçimler stat ve başlangıç altınını etkiler. `CompositeBackground`, seçilen yolu tek nesnede ve tek kayıt kimliğinde birleştirir; eski 6 geçmiş kaydı açılmaya devam eder.

**`Combat\`**
- `ICombatant` — oyuncu ve düşman bunun üzerinden savaşıyor.
- `CombatService` — saldırı ve inisiyatif zarlarını atıyor, savaş günlüğünü tutuyor.
- `DiceRoller` (gerçek zar) ve `FixedDiceRoller` (testlerde sabit sonuç).
- `DiceRoll → D20Roll → AttackRoll / InitiativeRoll`

**`World\`**
- `HexMap.CreateDemoMap()` — dünya görselinin küçük hücre ölçeğine göre 15 sütun x 12 satırdan oluşan 180 hexlik harita; biyom merkezleri resimdeki bölgelerle hizalıdır. `IsDangerous`, `HasQuest` ve `IsLocked` burada belirlenir. Harita `RevealAll()` ile tamamen açık başlar.
- `HexMap.FindPath()` — kilitli hücreleri dışarıda bırakan sade BFS ile başlangıçtan hedefe en kısa yürüyüş rotasını verir.
- `HexTile.Reveal()` eski kayıt uyumluluğu için duruyor; çalışma zamanı haritasında fog of war çizilmiyor.
- `WorldEncounter` → `DiscoveryEncounter` / `TravelerEncounter` / `HazardEncounter`; katalogda 50 savaş dışı encounter bulunur.
- `WorldLocation` → `VillageLocation` / `TownLocation` / `CastleLocation`; hizmetler han, pazar, demirci, şifacı, tapınak, kışla, savaş odası ve zindandır.
- `AccessApproach` → `PersuasionApproach` / `IntimidationApproach` / `CredentialApproach`; muhafız erişimi bu basit kalıtım ailesiyle çözülür.

**`Game\GameSession.cs`**
- Hareketi yönetiyor ve oturumu kayda çeviriyor.
- `PortraitIndex` (seçilen portre) ve `ClearDanger` (temizlenen hex) bilgileri kayıttaki `Flags` sözlüğünde tutuluyor.

**`Story\`** — ana hikâye; ayrıntı yukarıdaki 1 Kasım bölümünde.

**`Persistence\`**
- Kayıt modeli sürüm 3 (hikâye ilerlemesi `StoryProgressSaveData`); sürüm 1/2 açılır.
- `ItemSaveMapper` envanteri ve takılı eşyayı kayda yazıyor.
- Eşya bilgisi olmayan sürüm 1 kayıtlar yüklenince karakter başlangıç eşyalarını alıyor.

**`Items\`**
- `Item → Weapon / Armor / Consumable / QuestItem`
- `StartingKitPackage` → savaşçı/uzman/büyücü paketleri. `ItemCatalog.GiveStartingKit` sınıf, geçmiş ve hikâye seçimini birleştiriyor.

**`Rendering\`**
- `CharacterAtlasRuntime` — `characters\runtime-v1` manifestleri.
- `HexTilesetCatalog` — tileset manifestlerinden biyom karosunu seçiyor.
- `EquipmentAnchorCatalog` — ekipman ankrajları.

### `src\RetroRpg.Game` — ekranlar

| Dosya | Ne yapar |
|---|---|
| `MainGameForm.cs` | 800 x 800 mantıksal pencere; ekran değiştirme; katalogları yükleme ve `F11` tam ekran geçişi. Tam ekranda kare oyun alanı oranı bozulmadan büyür ve siyah yan boşluklarla ortalanır. |
| `Program.cs` | Giriş noktası; `--screenshots` modu. |
| `UiImages.cs` | Görsel önbelleği. Eksik dosyayı loglar ve ekranda gösterir. Şeffaf kenardaki kırıntıları kırpar. İmleci oluşturur. |
| `PixelButton.cs` | Çizilen düğme: normal, hover, pressed ve disabled durumları. Ana menüde Final Production V4 ahşap-kırmızı setini kullanır. |
| `UiTheme.cs` | Bütün çalışma zamanı metinlerini `ui\assets\fonts\PixelifySans-Variable.ttf` piksel fontuyla çizer. Renkler ve dokuz parçalı panel yardımcıları da buradadır. |
| `CharacterArt.cs` | Oyuncu portresi, token ve runtime animasyon karesi yolları. |
| `Screens\GameScreen.cs` | Bütün ekranların tabanı; fare ve düğme durumu, modal pencere desteği. |
| `Screens\MainMenuScreen.cs` | Ana menü. |
| `Screens\SettingsScreen.cs` | AYARLAR menüsündeki tam ekran aç/kapat düğmesi ve F11 bilgisini gösterir. |
| `Screens\CharacterCreationScreen.cs` | Kimlik → saç/sakal-yüz detayı → ırk → sınıf → 7 aşamalı geçmiş. Ana menüyle aynı V4 ahşap, demir, altın ve kırmızı kumaş çerçevesini kullanır; ayrıntı panellerinde portre gösterir. |
| `Screens\MapScreen.cs` | Tek parça dünya görseli üstünde görünmez/toggle edilebilir 180 küçük hex; rota ve smoothstep yürüyüş; envanter/karakter/günlük/kamp/çevre araştırma. Düşman ve tehlike işareti çizmez. |
| `Screens\CombatScreen.cs` | Önceden görünen isabet/hasar, maskeli d20 animasyonu, savaş envanteri, savunma ve Dayanıklılık kontrollü kaçış. Oyuncu ve düşman sprite'ının üstünde can barı yoktur; can HUD/sıra incelemesindedir. |
| `Screens\EncounterScreen.cs` | 50 savaş dışı encounterı oynatır; her olayda iki stat/zorluk seçeneği, ayrı ödül/ceza, d20 animasyonu ve autosave vardır. |
| `Screens\LocationScreen.cs` | Köy/kasaba/kale ve hizmet sahneleri; han, mağaza, iyileştirme ve muhafızlı alanlara yönlendirme. `InnScreen`, `ShopScreen` ve `GuardDialogueScreen` ayrı sorumluluk taşır. |
| `Screens\ShopScreen.cs` | Pazar ve demirci ürünlerini fiyat, ikon ve açıklamayla gösterir; satın alınanı envantere ekler. |
| `Screens\InventoryScreen.cs` / `CharacterRecordScreen.cs` / `JournalScreen.cs` / `CampScreen.cs` | Haritadan açılan çalışan RPG menüleri. |
| `Screens\AttributeAllocationScreen.cs` / `CharacterSummaryScreen.cs` / `StorySelectionScreen.cs` | Karakter oluşturma kapanış adımları. |
| `Screens\DeathScreen.cs` | Ölüm, autosave yükleme, ana menü ve çıkış. |

### Diğer projeler

- **`tests\RetroRpg.Tests\Program.cs`:** 56 test (23'ü ana hikâye: prologdan finale uçtan uca smoke testi, prolog, hatırlanan kişi, taş sırası, Gök Taşı savaş dalı, mülteci güveni, arşiv mesajı, mor kapı, kilitlenmeme, Piskopos, scripted yenilgi, Süvari, Kaderbozan, gerçek boss, kayıt göçü, tek-sefer olaylar, dört final, manifest kareleri). Kalıtım, açık harita/BFS, zorunlu geçitler, kayıt, atlaslar, 50 encounter seçeneği/sonucu, alış/satış, eşya çıkarma, başlangıç paketleri, muhafız yaklaşımı, rota açılması, yerleşimler, Göz Kulesi ve saldırı varyantları doğrulanır.
- **`tools\RetroRpg.AtlasTool`:** 22 karakter atlasını `characters\runtime-v1` altında 256 x 256 karelere bölen araç.

## Ekran ↔ asset eşleşmesi

| Ekran | Kullanılan assetler |
|---|---|
| Ana menü | `assets\final-production-v4\ui\screens\runtime-800\main-menu-background-wood-red-v1.png`, `assets\final-production-v4\ui\controls\buttons\large\button-large_{state}.png`, imleç `ui\assets\cursors\cursor-default_f00.png`. Merkezde ve flamalarda göz logosu kullanılmaz. |
| Karakter oluşturma | `assets\final-production-v4\ui\screens\runtime-800\character-creation-wood-red-v1.png`; portreler `characters\face-variants-v1\{ırk}\portraits\{beden}\`; ikonlar `ui\assets\icons\races\`, `ui\assets\icons\classes\`, `assets\final-production-v1\ui\icons\background\`; sprite `characters\runtime-v1\` |
| Hex harita | 2D parşömen dünya zemini `assets\final-production-v4\maps\world-map-parchment-2d-v1.png`; seçim, rota, görev ve isteğe bağlı hex çizgileri çalışma zamanında ayrı katman; oyuncu tokenı `characters\map-tokens-v1\tokens\players\`; encounter zarı `assets\final-production-v2\combat\dice\d20-roll-v1\`; çerçeve `ui\assets\frames\tooltips\tooltip-black-gold-small.png`; düğmeler `ui\assets\controls\buttons\medium\`. Düşman ve tehlike tokenı haritada çizilmez. |
| Savaş | çerçeve `assets\final-production-v2\ui\screens\runtime-800\combat-screen-simple-d20-v1.png`; orman sahnesi `assets\final-production-v4\scenes\combat-backgrounds\forest-combat-background-pixel-v2.png`, diğer biyomlar `assets\final-production-v1\scenes\combat-backgrounds\{biyom}-combat-background-v1.png`; düşman `characters\biome-enemies-v1\{biyom}\sprites\{id}\` (kurt v2, diğerleri v1); zar `assets\final-production-v2\combat\dice\`; ölüm efekti `assets\final-production-v2\effects\combat-reactions\enemy-death-dissolve-v1\`; sonuç penceresi `ui\assets\frames\panels\panel-purple-header.png` |

### Henüz oyunda kullanılmayan asset klasörleri

- `ui\mockups\screens\runtime-800\`: envanter, karakter, günlük ve kamp ekranları. Savaş ekranının ilk sürümü de burada ama savaşta v2 kullanılıyor.
- `assets\final-production-v1\ui\screens\runtime-800\`: dükkân, ganimet, crafting, büyü kitabı, kayıt/yükleme, duraklatma, ölüm, zafer ve yardım ekranları. Tam ekran ayarı çalışan kodla bağlandı; hazır ayarlar mockup'ı kullanılmıyor.
- **Karakterler:** `characters\boss-sprites-v1`, `characters\npc-sprites-v1`, `characters\class-outfits-v1`, `characters\class-animation-layers-v1`.
- **Eşyalar:** `items\item-pack-01`, `items\item-pack-02` (eşya ikonları).
- **Ekipman ve efekt:** `assets\final-production-v1\equipment` ve `assets\final-production-v2\equipment\weapons-extended` (karakter üstüne silah katmanı), `assets\final-production-v1\effects`.
- **Sahne ve harita:** `assets\final-production-v1\scenes\` (rota, iç mekân, kamp, hava durumu); `tilesets\transitions-v1` (biyom geçişleri) ve tilesetlerin `connections` / `poi` karoları. Eski `maps\hex\world-map-hex-2d-v1.png` ile izometrik dünya haritası artık çalışma zamanı zemini değildir; güncel zemin Final Production V4 içindeki parşömen haritasıdır.
- **Encounter:** `assets\final-production-v3\scenes\encounters\kalpakli-suvari-encounter-v1.png`.
- **Final Production V4:** Ortaçağ ahşap panolu ana menü, aynı görsel dilde karakter oluşturma çerçevesi ve ahşap-kırmızı dört durumlu ana menü düğmeleri.
- **Piksel-art savaş yenilemesi:** Orman savaş arka planı ile kurdun bekleme/saldırı kareleri gerçek blok piksel kümeleriyle v2 olarak üretildi. Oyun çiziminde tüm küçültmeler nearest-neighbor kullanır; eski karakter kareleri savaşta renk paleti ve piksel ızgarası bakımından normalize edilir.

## Belgeden bilinçli sapmalar

- **`Name`:** Belge onu `Npc`'ye koyuyor, kodda `GameEntity`'de duruyor. Eşyalar ve dünya nesneleri de ad kullanıyor.
- **`RewardGold`:** Belge onu yalnız `MiniBossNpc`'ye koyuyor, kodda `EnemyNpc`'de. Bütün düşmanlar altın veriyor.
- **Karakter oluşturma çerçevesi:** Yeni V4 görselinde yazı gömülü değildir. Başlıklar, seçenekler ve ayrıntılar çalışma zamanında Türkçe piksel fontuyla çizilir.
- **Savaştaki BİTİR düğmesi:** Kaçış onayı açar; düşman bir serbest saldırı yaptıktan sonra hayattaysa oyuncu haritaya döner. BÜYÜ düğmesi pasif.
- **Yenilgi:** Can sıfıra indiğinde ölüm ekranı açılır; otomatik iyileştirme yapılmaz.

## Bilinçli kapsam sınırı

1. Ana hikâye prologdan dört finale kadar oynanabilir. Hikâye II ve III yuvaları kilitlidir. Kaderbozan Zırhı'nın ve Vaelor'un özel runtime görselleri üretilmediği için yukarıdaki geçici görseller kullanılır.
2. BÜYÜ düğmesi için sınıfa özel büyü kuralları tasarlanmadığından pasiftir; mevcut ödev kapsamındaki d20 saldırı sistemi tam çalışır.
3. Yerelleştirme/string tablosu zorunlu oynanış eksiği değildir; çalışma zamanı metinleri Türkçedir.
4. Kod, görsel ve test tarafında bilinen kritik eksik yoktur. İlk git commit'i kullanıcı kararı olarak bırakılmıştır; üst depodaki diğer dosyalar karıştırılmamalıdır.

## Tuzaklar

- **Ekran koordinatları elle ölçüldü.** Savaş ekranında görselin üstüne yazılan değerlerin yerleri (`CombatScreen.cs` içindeki dikdörtgenler) ve karakter oluşturmadaki yuva ve düğme yerleri (`CharacterCreationScreen.Option` / `Slot`) görsellerden ölçüldü. Görsel değişirse bunlar da güncellenmeli.
- **Bazı UI görsellerinde kırıntı var.** Şeffaf kenarlarda komşu parçalardan kalan kırıntılar bulunuyor. Çerçeve ve düğmeler `Images.Get(path, trim: true)` ile yüklenmeli. Küçük düğme (`small`), sekme (`tab-primary`) ve slot görselleri kırpınca bozuluyor, kullanılmadı.
- **İmleç sıcak noktası:** `cursor-animations-v1.json` içindeki değerler görselle uyuşmuyor. Kod sıcak noktayı ilk opak pikselden hesaplıyor.
- **Düşman spriteları sağa bakıyor.** Savaşta oyuncuya dönmeleri için yatay aynalanıyor.
- **Ölçekleme:** Görsel büyütülürken nearest neighbor, küçültülürken yumuşak örnekleme kullanılıyor (`UiTheme.DrawImageFit`).
- **Bash ters eğik çizgi:** Bash ile `sed` kullanırken Windows yolundaki ters eğik çizgiler kayboluyor. Dosya düzenlemelerinde Edit aracını kullan.
- **Word belgesi render:** Makinede LibreOffice ve pdftoppm yok. Word COM ile PDF, Windows.Data.Pdf ile PNG üretilebiliyor.
