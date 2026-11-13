# Retro RPG — C#/.NET Kod Başlangıcı

Bu çözüm bir eğitim projesidir. Ana hedef, temel ve okunabilir C# koduyla `class`, `inheritance`, `encapsulation` ve `polymorphism` kullanımını açıkça göstermektir.

## Projeler

- `src/RetroRpg.Core`: Motor bağımsız oyun kuralları ve sınıf hiyerarşileri.
- `src/RetroRpg.Game`: 800 × 800 Windows Forms prototipi.
- `tools/RetroRpg.AtlasTool`: 22 kaynak karakter atlasını 256 × 256 runtime karelerine dönüştürür.
- `tests/RetroRpg.Tests`: Haricî test paketi gerektirmeyen hızlı doğrulama uygulaması.
- `Content/definitions`: Irk, sınıf, alt sınıf, geçmiş, biyom ve eşya verileri.
- `Content/manifests`: Merkezi asset kataloğu.
- `Content/schemas`: JSON içerik şeması.

## Çalıştırma

```powershell
dotnet build RetroRpg.sln
dotnet run --project tests/RetroRpg.Tests
dotnet run --project tools/RetroRpg.AtlasTool -- "C:\Users\VECTUS\Documents\pc\game_design"
dotnet run --project src/RetroRpg.Game
```

## Kalıtım örnekleri

- `GameEntity → Npc → FriendlyNpc / EnemyNpc`
- `EnemyNpc → BiomeEnemyNpc / MiniBossNpc / MainBossNpc`
- `GameEntity → PlayerCharacter`
- `CharacterRace → HumanRace / ElfRace / DwarfRace / ...`
- `CharacterClass → FighterClass → ChampionClass / BattleMasterClass` (12 ana sınıfın her biri iki alt sınıfla)
- `Item → Weapon / Armor / Consumable / QuestItem`; `Armor → FatebreakerArmor` (Kaderbozan Zırhı)
- `StoryEvent → DialogueStoryEvent / SkillCheckStoryEvent / CombatStoryEvent / ScriptedDefeatStoryEvent / EndingStoryEvent`
- `Quest → MainQuest / WaystoneQuest / SideQuest`
- `StoryConsequence → FlagConsequence / ItemConsequence / ReputationConsequence / RouteConsequence / EndingConsequence / QuestConsequence / CostConsequence`
- `DiceRoll → D20Roll → AttackRoll / InitiativeRoll`
- `SceneNode → TravelScene / CombatScene / CampScene / ShopScene`
- `GameScreen → MainMenuScreen / CharacterCreationScreen / MapScreen / CombatScreen / StorySceneScreen / EndingScreen`

`EnemyNpc` bir `CharacterClass` değildir; sınıfını `Class` özelliğinde taşır. `PlayerCharacter` ve `EnemyNpc` savaşa `ICombatant` arayüzüyle girer. Bu arayüzün iki gerçek uygulaması vardır; `IDiceRoller` için de aynısı geçerlidir (`DiceRoller` ve testlerdeki `FixedDiceRoller`).

## Basit d20 kuralı

Saldırı `d20 + saldırı bonusu` ile yapılır. Toplam hedefin `ArmorClass` değerine eşit veya büyükse saldırı isabet eder. Doğal 20 kritik, doğal 1 kesin başarısızlıktır. Tam masaüstü D&D kuralları uygulanmaz.

## Asset yolu

Oyun çalışırken `game_design` kökünü otomatik bulur. Farklı bir konum kullanılacaksa `RETRO_RPG_ASSET_ROOT` ortam değişkeni ayarlanabilir. Mevcut üretim assetleri taşınmaz ve yeniden adlandırılmaz.

Atlas aracı çalıştırıldığında yalnızca `characters/runtime-v1` altındaki, yeniden üretilebilen runtime çıktıları yeniler. Kaynak atlaslara dokunmaz. Üretilen her manifest animasyon, yön, süre, döngü, kaynak dikdörtgeni, ayak pivotu ve ekipman ankrajlarını taşır.
