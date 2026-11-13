# Retro RPG — Geliştirici Rehberi

Son güncelleme: 1 Kasım 2026

Bu belge, projeyi devralan geliştiricinin oyunu bozmadan nasıl derleyeceğini, yeni içerik ekleyeceğini ve ana hikâyeyi koda bağlayacağını anlatır. İlk kez gelen biri şu sırayla okumalıdır:

1. `DEVIR.md`
2. Bu dosya
3. `documents/STORY_IMPLEMENTATION_PLAN.md`
4. Değiştirilecek sistemin kaynak kodu ve ilgili testler

## 1. Projenin amacı ve değişmez kurallar

Bu proje C# ve .NET 9 ile yazılmış, Windows Forms üzerinde çalışan 800x800 mantıksal çözünürlüklü retro bir CRPG'dir. Aynı zamanda bir sınıf/kalıtım ödevidir. Kodun öğretici, basit ve okunabilir olması; gereksiz mimari karmaşıklıktan daha önemlidir.

Değişmez kurallar:

- Oyun kuralları `RetroRpg.Core`, çizim ve giriş işlemleri `RetroRpg.Game` içinde kalır.
- Kalıtım, gerçek bir “türdür” ilişkisi olduğunda kullanılır. Sadece kod tekrarını azaltmak için yapay sınıf zinciri kurulmaz.
- Tek sorumluluklu, kısa sınıflar tercih edilir. Karmaşık DI konteyneri, event bus, reflection tabanlı kayıt sistemi veya derin generic yapı eklenmez.
- Bütün görsel dosyalar final oyun assetidir; placeholder veya konsept kabul edilmez.
- UI metni PNG'lere gömülmez. Metin oyun tarafından piksel fontuyla çizilir.
- Görseller ölçeklenirken piksel karakteri korunur; büyütmede nearest-neighbor kullanılır.
- Harita tamamen açıktır. Fog of war yoktur; düşman ve gizli tehlikeler haritada gösterilmez.
- Oyuncu ve düşman sprite'larının üstünde can barı çizilmez. Can HUD ve sıra inceleme alanında görünür.
- D20 animasyonunda ekranda görülen ham zar değeri ile kurala uygulanan değer aynı `DiceRoll` nesnesinden gelmelidir.

## 2. Gereksinimler

- Windows 10/11
- .NET 9 SDK
- Çalıştırmak için .NET 9 Desktop Runtime
- İsteğe bağlı: ffmpeg/ffprobe (asset doğrulama ve GIF üretimi)

Çözüm dosyası: `RetroRpg.sln`.

## 3. Derleme, test ve çalıştırma

Komutları `C:\Users\VECTUS\Documents\pc\game_design` içinde çalıştır:

```powershell
dotnet build RetroRpg.sln
dotnet run --project tests/RetroRpg.Tests
dotnet run --project src/RetroRpg.Game
```

Release exe üretmek için:

```powershell
dotnet publish src/RetroRpg.Game/RetroRpg.Game.csproj -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -p:DebugType=none -o dist
```

Ekranları otomatik render edip 800x800 QA yapmak için:

```powershell
dist\RetroRpg.Game.exe --screenshots C:\temp\retro-rpg-qa
```

Başarı ölçütü: derleme 0 hata/0 uyarı, test uygulaması 0 çıkış kodu, ekran görüntüsü modunda eksik asset veya taşan UI bulunmaması.

## 4. Çözüm yapısı

```text
game_design/
├── src/
│   ├── RetroRpg.Core/       Oyun kuralları, modeller, kalıtım aileleri
│   └── RetroRpg.Game/       Windows Forms, ekranlar, çizim, giriş
├── tests/RetroRpg.Tests/    Haricî paket kullanmayan hızlı test uygulaması
├── tools/                   Atlas ve üretim araçları
├── Content/                 JSON tanımları, şemalar ve asset kataloğu
├── assets/                  UI, sahne, efekt ve hikâye assetleri
├── characters/              Portre, sprite, token ve runtime atlasları
├── items/                   Eşya ikonları
├── documents/               Tasarım, araştırma ve hikâye belgeleri
├── qa/                      Otomatik render ve görsel kontrol çıktıları
└── dist/                    Yayınlanan exe
```

Asset kökü çalışma sırasında otomatik bulunur. Oyun farklı bir klasörden çalıştırılacaksa `RETRO_RPG_ASSET_ROOT` ortam değişkeni `game_design` kökünü göstermelidir.

## 5. Temel mimari ve kalıtım

Ana sınıf aileleri:

```text
GameEntity
├── PlayerCharacter
└── Npc
    ├── FriendlyNpc
    └── EnemyNpc
        ├── BiomeEnemyNpc
        ├── MiniBossNpc
        └── MainBossNpc

CharacterClass
├── FighterClass
│   ├── ChampionClass
│   └── BattleMasterClass
├── WizardClass
└── ... 12 ana sınıf / 24 alt sınıf

Item
├── Weapon
├── Armor
├── Consumable
└── QuestItem

WorldLocation
├── VillageLocation
├── TownLocation
└── CastleLocation

WorldEncounter
├── DiscoveryEncounter
├── TravelerEncounter
└── HazardEncounter

DiceRoll
├── D20Roll
│   ├── AttackRoll
│   └── InitiativeRoll

GameScreen
├── MainMenuScreen
├── CharacterCreationScreen
├── MapScreen
├── CombatScreen
└── ...
```

`PlayerCharacter` ve `EnemyNpc`, savaşta `ICombatant` üzerinden polimorfik kullanılır. `EnemyNpc` bir karakter sınıfından türemez; seçilmiş `CharacterClass` nesnesini özellik olarak taşır. Bu ayrım korunmalıdır.

Yeni hikâye kodu şu sade aileleri kullanmalıdır:

```text
StoryEvent
├── DialogueStoryEvent
├── SkillCheckStoryEvent
├── CombatStoryEvent
├── ScriptedDefeatStoryEvent
└── EndingStoryEvent

Quest
├── MainQuest
├── WaystoneQuest
└── SideQuest

StoryConsequence
├── FlagConsequence
├── ItemConsequence
├── ReputationConsequence
├── RouteConsequence
└── EndingConsequence
```

Her alt sınıf yalnız gerekli veriyi ve bir `CanTrigger`/`Apply` davranışını taşımalıdır. Çok katmanlı soyutlama eklenmemelidir.

## 6. Ekran yaşam döngüsü

`MainGameForm` tek pencere ve ekran yöneticisidir. Bütün ekranlar `GameScreen` tabanından türemeli, kendi çizim ve giriş alanlarını yönetmelidir. WinForms düğmeleri veya label'ları eklemek yerine mevcut çizim sistemi kullanılmalıdır.

Yeni ekran ekleme adımları:

1. `src/RetroRpg.Game/Screens/` altında `GameScreen` türevi oluştur.
2. 800x800 mantıksal koordinatlar kullan.
3. Düğmeler için `PixelButton`, metin için `UiTheme` kullan.
4. Görselleri `UiImages` üzerinden yükle; dosya yolu yoksa sessizce gizleme, eksik asset logunu koru.
5. `MainGameForm` içine yalnız gerekli geçiş metodunu ekle.
6. Ekran görüntüsü moduna yeni ekranı ekle.
7. 800x800 ve tam ekran renderını kontrol et.

Metinler küçük kutuya sığmıyorsa fontu rastgele küçültme. Önce kısa metin, satır kırma, iç boşluk ve kutu boyutu düzenlenmelidir. Metin çizimi tam piksel koordinatına oturmalıdır.

## 7. Dünya haritası ve hareket

Çalışma zamanı haritası `assets/final-production-v4/maps/world-map-parchment-2d-v1.png` üzerindedir.

- 15 sütun x 12 satır, toplam 180 küçük hex.
- Hex ağı varsayılan görünmez; `H` veya `HEX: AÇIK/KAPALI` ile açılır.
- Oyuncu hedefi seçer, BFS rotası önizlenir, onaydan sonra token komşu hexler arasında yürür.
- Seçim, rota ve görev işaretleri ayrı çalışma zamanı katmanlarıdır.
- Düşman ve gizli tehlike tokenı çizilmez.
- Geçilmez alanlar görsel ve kural olarak dağ, lav, uçurum, bataklık çukuru, kanyon veya çökme ile temsil edilir; düz X/şerit kullanılmaz.
- Mor bölgenin tek girişi güney kapısıdır. Üç Yol Taşı tamamlanmadan açılmaz.
- Göz Kulesi hexinde rastgele encounter yoktur; hikâye/boss olayı zorunludur.

Yeni engel veya geçit eklerken `HexMap` içindeki kilit durumu, BFS testi, harita görseli ve görev bayrağı birlikte güncellenmelidir.

## 8. D20 ve savaş kuralları

Oyun tam D&D kopyası değildir; basit d20 çekirdeği kullanır:

- Saldırı: `d20 + saldırı bonusu >= ArmorClass` ise isabet.
- Doğal 20 kritik, doğal 1 kesin başarısızlık.
- Hasar, seçilen saldırının zar ifadesinden ayrı atılır.
- Saldırı seçilmeden önce isabet bonusu ve hasar aralığı görünür.
- Kaçış Dayanıklılık kontrolüdür; başarısızlıkta düşman serbest saldırı yapar.
- Scripted yenilgi normal ölüm değildir ve `DeathScreen` açmamalıdır.

Yeni zar kuralı eklerken üretim kodu `IDiceRoller`, test kodu `FixedDiceRoller` kullanmalıdır. UI kendi başına ikinci bir rastgele sayı üretmemelidir.

## 9. Kayıt, yükleme ve sürümleme

Kayıt yolu: `%LocalAppData%\RetroRpg\Saves\slot-1.json`.

Mevcut kayıt sürümü 3'tür (`GameSession.SaveVersion`). `GameSession.Flags` küçük dünya durumları içindir; ana hikâye `GameSaveData.Story` (`StoryProgressSaveData`) alanına yazılır. Eski sürüm dosyasının üzerine ilk kez yazılmadan önce `slot-1.v{N}-backup.json` yedeği alınır.

Önerilen `StoryProgress` alanları:

- Aktif bölüm ve aktif düğüm kimliği.
- Başlatılan/tamamlanan görev kimlikleri.
- Kök, Gök ve Güneş Taşı karar modu.
- Mor kapı ve Göz Kulesi kilit durumu.
- İlk Vaelor scripted yenilgisinin oynanıp oynanmadığı.
- Kaderbozan Zırhı'nın verilip verilmediği.
- Hatırlanan kişi tipi ve kurtarılma durumu.
- Yerleşim güvenleri.
- Uygun final kimlikleri ve seçilmiş final.

Sürüm 1 ve 2 kayıtları açılmaya devam etmelidir. Eksik hikâye alanları güvenli varsayılanlarla oluşturulmalı; eski save dosyası doğrudan değiştirilmemelidir.

Autosave şu noktalarda alınır: rota bitimi, encounter sonucu, yerleşim işlemi, hikâye düğümü sonucu, boss öncesi ve boss sonrası. Sinematik karesinin ortasında kayıt alınmaz.

## 10. Ana hikâyeyi koda bağlama

Ana tasarım: `documents/STORY_IMPLEMENTATION_PLAN.md`.

Uygulama sırası:

1. `StoryEvent`, `Quest`, `StoryConsequence` ve `StoryProgress` sınıflarını ekle.
2. `StoryCatalog` içinde bütün olayları sabit kimliklerle tanımla.
3. `StorySceneScreen` ile resim, anlatım, seçenek ve d20 akışını oluştur.
4. Karakter özetinden sonra doğrudan haritaya değil prologa geç.
5. Yeşilgeçit ve Kızılburç görevlerini bağla.
6. Kök Taşı görevini uçtan uca bitir ve test et; ardından Gök ve Güneş taşlarını ekle.
7. Üç taş tamamlanınca mor bölgenin tek kapısını aç.
8. Mor bölgedeki sabit olayları ve Göz Piskoposu'nu bağla.
9. Kule içi, ilk Vaelor savaşı, scripted yenilgi, kuleden düşüş, Süvari diriltmesi ve Kaderbozan dönüşünü bağla.
10. Gerçek final savaşını ve dört `EndingStoryEvent` varyantını ekle.

Sinematik manifestleri:

- Genel hikâye: `assets/final-production-v6/story/cinematics/main-story/manifest.json`
- Final boss: `assets/final-production-v6/story/cinematics/final-boss/manifest.json`

Olay eşlemesi:

| Hikâye olayı | Asset kimliği |
|---|---|
| Kervan prologu ve Kara Harita Parçası | `caravan-prologue-v1` |
| Kök Taşı | `root-waystone-v1` |
| Gök Taşı | `sky-waystone-v1` |
| Güneş Taşı | `sun-waystone-v1` |
| Kırık Arşiv ve mor kapı | `archive-gate-v1` |
| Mülteciler/devriye/Piskopos/anı | `corrupted-region-v1` |
| Kader Merceği ve Vaelor ikinci form | `fate-lens-v1` |
| Scripted düşüş | `tower-fall-v1` |
| Kalpaklı Süvari diriltmesi | `rider-revival-v1` |
| Zırhlı dönüş | `fatebreaker-return-v1` |
| Dört epilog | `endings-v1` |

Kare gösterici PNG'leri sırayla oynatmalı, son karede beklemeli ve kendiliğinden loop etmemelidir. GIF yalnız geliştirici önizlemesidir. Diyalog ve seçenek metni görselin üzerine gömülmemeli, ayrı UI katmanında çizilmelidir.

## 11. Final boss için zorunlu akış

1. Vaelor salonundan önce autosave.
2. İlk savaş normal savaş ekranında başlar; oyuncu en az bir gerçek eylem yapar.
3. Vaelor ilk turun sonunda `Kaderin Hükmü` kullanır. Bu, AC ve normal savunmayı yok sayan `ScriptedDefeatStoryEvent` sonucudur.
4. Normal ölüm ekranı açılmaz; `tower-fall-v1` oynar.
5. `rider-revival-v1` oynar; Kalpaklı Süvari oyuncuyu diriltir ve Kaderbozan Zırhı verir.
6. Temizlenen kule yeniden düşmanla doldurulmaz.
7. Dönüşte `fatebreaker-return-v1` oynar; zırh ikinci kader saldırısını kırar.
8. Gerçek, kazanılabilir d20 savaşı başlar. Bu savaşta ölüm normal `DeathScreen` akışıdır.
9. Vaelor sıfır cana geldiğinde doğrudan yok edilmez; final kararı açılır.

Kalpaklı Süvari daha önceki bölümlere eklenmez, sürekli görev NPC'sine dönüştürülmez ve epilog hesabını değiştirmez.

## 12. Yeni içerik ekleme reçeteleri

### Yeni sınıf veya alt sınıf

1. `CharacterClass` veya uygun ana sınıftan türet.
2. Kısa, okunabilir özellikleri override et.
3. `ClassCatalog` kaydı ekle.
4. Başlangıç ekipman paketini belirle.
5. İkon/portre yolunu asset kataloğuna ekle.
6. Katalog ve başlangıç kiti testi ekle.

### Yeni eşya

1. `Weapon`, `Armor`, `Consumable` veya `QuestItem` türünü seç.
2. `ItemCatalog`/mağaza kataloğuna tanımla.
3. İkonu doğru final asset klasörüne koy.
4. Gerekirse `ItemSaveMapper` uyumluluğunu kontrol et.
5. Alış, satış, kuşanma/çıkarma ve save/load testi ekle.

### Yeni düşman veya boss

1. Uygun `EnemyNpc` alt türünü seç.
2. `Content/definitions/enemies-v1.json` kaydı ekle.
3. Sprite ve token yollarını asset kataloğuna ekle.
4. Can, AC, saldırı bonusu ve hasarı mevcut tier aralığında tut.
5. Savaş, ödül, ölüm ve deterministic seçim testi ekle.

### Yeni random encounter

1. Uygun `WorldEncounter` alt türünü seç.
2. İki anlamlı seçenek tanımla.
3. Her seçeneğe stat, DC, başarı ödülü ve başarısızlık cezası ver.
4. Ödül/ceza olayın bağlamıyla mantıklı olmalı; koşmak sebepsiz altın vermemeli, öğüt dinlemek sebepsiz para üretmemelidir.
5. Sahne assetini ekle ve katalog/test sayısını güncelle.

### Yeni yerleşim hizmeti

1. `WorldLocation` üzerinden hizmeti erişilebilir yap.
2. Ücret girişte değil gerçek işlem sırasında alınır.
3. Kısıtlı alanlar `AccessApproach` ailesiyle İkna/Gözdağı/Kimlik kontrolü yapar.
4. Pazar ve demirci satın alma/satma ekranına gider; doğrudan altın düşmez.

### Yeni hikâye olayı

1. Kararlı bir kebab-case olay kimliği seç.
2. Uygun `StoryEvent` alt sınıfını kullan.
3. Tetik koşulunu ve tek-sefer bayrağını tanımla.
4. Sonucu `StoryConsequence` alt sınıflarıyla uygula.
5. Görsel asset kimliğini manifestten seç.
6. Sonuçtan sonra autosave al.
7. Başarı, başarısızlık, save/load ve tekrar tetiklenmeme testi ekle.

## 13. Asset üretim ve entegrasyon standardı

- Tam ekran sahne ve sinematik kare: 800x800 PNG.
- Sprite/ikon: gerçek alfa; renkli veya siyah sahte arka plan yok.
- Dosya adı: küçük harf, kebab-case, son ek `-v1`, `-v2`.
- Mevcut dosya yeniden adlandırılmaz; değişiklik yeni sürüm olarak eklenir.
- Her paket `manifest.json`/`manifest.csv` ve üretim notu taşımalıdır.
- Animasyon karesinde pivot ve hücre boyutu bütün karelerde sabit olmalıdır.
- UI yazısı asset içine çizilmez.
- Ana hikâye sinematiklerinde oyuncu saf siyah silüettir; böylece bütün ırk, beden ve sınıflar aynı sahneyi kullanır.
- Oyun GIF oynatmaz; PNG kare dizisini oynatır.

Asset ekledikten sonra dosyanın gerçekten yüklendiğini `%LocalAppData%\RetroRpg\missing-assets.log` ve ekran görüntüsü moduyla doğrula.

## 14. Test ve QA kontrol listesi

Her anlamlı değişiklikten sonra:

- [ ] `dotnet build RetroRpg.sln` 0 hata/0 uyarı.
- [ ] `dotnet run --project tests/RetroRpg.Tests` başarılı.
- [ ] Yeni sınıfın kalıtım testi var.
- [ ] Eski sürüm 1/2 save dosyası açılıyor.
- [ ] Autosave doğru anda alınıyor.
- [ ] D20'nin görünen ve uygulanan ham değeri aynı.
- [ ] 800x800 ekranda yazı taşmıyor, kaymıyor ve butonlara ortalanıyor.
- [ ] Tam ekranda kare oyun alanı oranı bozulmuyor.
- [ ] Eksik asset logu boş.
- [ ] Haritada fog of war ve görünür düşman/tehlike yok.
- [ ] Sprite üstü can barı yok.
- [ ] Envanterde kuşan/çıkar, satma ve save/load çalışıyor.
- [ ] Hikâye olayı ikinci kez istenmeden tetiklenmiyor.
- [ ] Başarısız kontrol ana hikâyeyi kalıcı olarak kilitlemiyor; yalnız bedel veya alternatif yol üretiyor.
- [ ] Dört finalin her biri test verisiyle ulaşılabiliyor.

## 15. Önerilen geliştirme sırası

1. Hikâye çekirdek sınıfları ve kayıt sürümü 3.
2. `StorySceneScreen` ve PNG kare oynatıcı.
3. Kervan prologu ve Kara Harita Parçası.
4. Yeşilgeçit/Kızılburç görev zinciri.
5. Kök Taşı uçtan uca; sonra Gök ve Güneş taşları.
6. Mor bölge kapısı ve sabit hikâye encounterları.
7. Göz Kulesi ve Kader Merceği.
8. İki aşamalı Vaelor akışı, scripted yenilgi ve Kaderbozan Zırhı.
9. Dört final ve dinamik epilog.
10. Save migration, ekran görüntüsü QA, performans ve son dengeleme.

Bir aşama kabul ölçütlerini geçmeden sonraki aşamaya geçilmemelidir. En kritik hedef, önce tek bir dar akışı uçtan uca oynanabilir hâle getirmek; sonra aynı basit kalıpla yeni içerik eklemektir.

## 16. Bilinen mevcut durum

- Karakter oluşturma, ırk/sınıf/geçmiş, özellik puanı, özet ve hikâye seçimi çalışır.
- 2D parşömen dünya haritası, küçük görünmez hexler, rota seçimi ve yürüyüş çalışır.
- Savaş, d20 animasyonu, saldırı türleri, kaçış, ölüm ve autosave çalışır.
- 50 savaş dışı encounter, yerleşimler, mağaza, han ve muhafız erişimi çalışır.
- Kader Yolları: Göz Kulesi prologdan dört finale kadar oynanabilir; 46 sinematik karenin tamamı `StorySceneScreen`/`EndingScreen` içinde kullanılır.
- Hikâye kodu `src/RetroRpg.Core/Story/` altındadır: olaylar ve görevler `StoryCatalog`, tetik/zincir/işaretler `StoryDirector`, final kuralları `EndingRules`, durum `StoryProgress`. Yeni olay eklerken `StoryCatalog` içine aynı kalıpla ekle; aynı hexteki olaylarda liste sırası önceliği belirler.
- `CombatScreen` hikâye savaşlarını `CombatStoryEvent` ile oynatır. Scripted yenilgi `EnemyTurnResult.OpensDeathScreen == false` döner.
- Kaderbozan Zırhı mantıksal olarak tamdır (`EquipmentSlot.StoryLayer`); ayrı final ikon ve karakter ekipman katmanı henüz üretilmemiştir. Vaelor savaşta `abyssal-crown` boss sprite'ını kullanır.
- `StoryModels.cs` artık yalnız hikâye yuvalarını (`StorySlotCatalog`) tutar.

Kod değişikliğine başlamadan önce `git status --short` ile çalışma ağacını kontrol et. Depo kökü bir üst klasördür ve kullanıcının başka dosyalarını da içerir; alakasız dosyalara dokunma.
