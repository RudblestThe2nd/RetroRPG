param(
    [string]$OutputPath = ""
)

$ErrorActionPreference = "Stop"
$ProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $ProjectRoot "documents\diagrams\RetroRpg-Tum-Proje-Sinif-Diyagrami.drawio"
}

function New-Node(
    [string]$Id,
    [string]$Name,
    [int]$X,
    [int]$Y,
    [int]$Width = 210,
    [int]$Height = 70,
    [string]$Kind = "class",
    [string[]]$Members = @()
) {
    $stereotype = switch ($Kind) {
        "interface" { "&lt;&lt;interface&gt;&gt;<br/>" }
        "static"    { "&lt;&lt;utility&gt;&gt;<br/>" }
        "record"    { "&lt;&lt;record&gt;&gt;<br/>" }
        "enum"      { "&lt;&lt;enumeration&gt;&gt;<br/>" }
        "screen"    { "&lt;&lt;screen&gt;&gt;<br/>" }
        default      { "" }
    }
    $displayName = if ($Kind -eq "abstract") { "<i>$Name</i>" } else { $Name }
    $attributes = [System.Collections.Generic.List[string]]::new()
    $methods = [System.Collections.Generic.List[string]]::new()
    foreach ($member in $Members) {
        $line = if ($member.StartsWith("+") -or $member.StartsWith("-") -or $member.StartsWith("#")) {
            $member
        } else {
            "+ $member"
        }
        if ($member.Contains("(")) { $methods.Add($line) } else { $attributes.Add($line) }
    }
    $attributeText = if ($attributes.Count -gt 0) { $attributes -join "<br/>" } else { "&nbsp;" }
    $methodText = if ($methods.Count -gt 0) { $methods -join "<br/>" } else { "&nbsp;" }
    $label = "<table cellpadding=`"0`" cellspacing=`"0`" style=`"width:100%;height:100%;border-collapse:collapse;font-family:Arial;font-size:12px;`">" +
        "<tr><td style=`"background-color:#d5e8d4;text-align:center;border-bottom:1px solid #000000;padding:5px;`">$stereotype<b>$displayName</b></td></tr>" +
        "<tr><td style=`"background-color:#ffffff;text-align:left;border-bottom:1px solid #000000;padding:5px;vertical-align:top;`">$attributeText</td></tr>" +
        "<tr><td style=`"background-color:#ffffff;text-align:left;padding:5px;vertical-align:top;`">$methodText</td></tr></table>"
    [pscustomobject]@{ Id=$Id; Label=$label; X=$X; Y=$Y; W=$Width; H=$Height; Kind=$Kind }
}

function New-Edge(
    [string]$From,
    [string]$To,
    [string]$Kind = "uses",
    [string]$Label = ""
) {
    [pscustomobject]@{ From=$From; To=$To; Kind=$Kind; Label=$Label }
}

function Get-NodeStyle([string]$Kind) {
    "rounded=0;whiteSpace=wrap;html=1;overflow=hidden;align=left;verticalAlign=top;spacing=0;fillColor=#ffffff;strokeColor=#000000;strokeWidth=1;fontSize=12;fontFamily=Arial;shadow=0;"
}

function Get-EdgeStyle([string]$Kind) {
    switch ($Kind) {
        "inherit"   { "edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;endArrow=block;endFill=0;strokeWidth=1.5;strokeColor=#000000;" }
        "implements"{ "edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;endArrow=block;endFill=0;dashed=1;strokeWidth=1.5;strokeColor=#000000;" }
        "contains"  { "edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;startArrow=diamond;startFill=1;endArrow=open;endFill=0;strokeColor=#000000;" }
        default      { "edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;endArrow=open;endFill=0;dashed=1;strokeColor=#666666;" }
    }
}

function Convert-ToXmlText([string]$Text) {
    if ($null -eq $Text) { return "" }
    return [System.Security.SecurityElement]::Escape($Text)
}

function Add-DiagramPage(
    [System.Text.StringBuilder]$Document,
    [string]$PageId,
    [string]$Name,
    [object[]]$Nodes,
    [object[]]$Edges,
    [int]$PageWidth,
    [int]$PageHeight,
    [string]$Subtitle
) {
    $titleLabel = "<b>$Name</b><br/><font color=`"#666666`">$Subtitle</font>"
    $titleValue = Convert-ToXmlText $titleLabel
    [void]$Document.AppendLine("  <diagram id=`"$PageId`" name=`"$(Convert-ToXmlText $Name)`">")
    [void]$Document.AppendLine("    <mxGraphModel dx=`"1400`" dy=`"900`" grid=`"1`" gridSize=`"10`" guides=`"1`" tooltips=`"1`" connect=`"1`" arrows=`"1`" fold=`"1`" page=`"1`" pageScale=`"1`" pageWidth=`"$PageWidth`" pageHeight=`"$PageHeight`" math=`"0`" shadow=`"0`">")
    [void]$Document.AppendLine("      <root>")
    [void]$Document.AppendLine("        <mxCell id=`"0`"/>")
    [void]$Document.AppendLine("        <mxCell id=`"1`" parent=`"0`"/>")
    [void]$Document.AppendLine("        <mxCell id=`"${PageId}_title`" value=`"$titleValue`" style=`"text;html=1;align=left;verticalAlign=middle;whiteSpace=wrap;rounded=0;fontSize=20;fontFamily=Arial;`" vertex=`"1`" parent=`"1`"><mxGeometry x=`"30`" y=`"20`" width=`"900`" height=`"55`" as=`"geometry`"/></mxCell>")

    foreach ($node in $Nodes) {
        $id = "${PageId}_$($node.Id)"
        $value = Convert-ToXmlText $node.Label
        $style = Get-NodeStyle $node.Kind
        [void]$Document.AppendLine("        <mxCell id=`"$id`" value=`"$value`" style=`"$style`" vertex=`"1`" parent=`"1`"><mxGeometry x=`"$($node.X)`" y=`"$($node.Y)`" width=`"$($node.W)`" height=`"$($node.H)`" as=`"geometry`"/></mxCell>")
    }

    $edgeNumber = 0
    foreach ($edge in $Edges) {
        $edgeNumber++
        $id = "${PageId}_edge_$edgeNumber"
        $source = "${PageId}_$($edge.From)"
        $target = "${PageId}_$($edge.To)"
        $value = Convert-ToXmlText $edge.Label
        $style = Get-EdgeStyle $edge.Kind
        [void]$Document.AppendLine("        <mxCell id=`"$id`" value=`"$value`" style=`"$style`" edge=`"1`" parent=`"1`" source=`"$source`" target=`"$target`"><mxGeometry relative=`"1`" as=`"geometry`"/></mxCell>")
    }

    [void]$Document.AppendLine("      </root>")
    [void]$Document.AppendLine("    </mxGraphModel>")
    [void]$Document.AppendLine("  </diagram>")
}

$document = [System.Text.StringBuilder]::new()
[void]$document.AppendLine('<?xml version="1.0" encoding="UTF-8"?>')
[void]$document.AppendLine('<mxfile host="app.diagrams.net" modified="2026-11-03T00:00:00.000Z" agent="Codex" version="24.7.17" type="device">')

# 1 - Genel bakış
$nodes = @(
    (New-Node "entities" "Entities" 50 150 260 95 "abstract" @("GameEntity", "WorldObject")),
    (New-Node "characters" "Characters" 390 120 300 130 "class" @("PlayerCharacter", "Npc ve düşmanlar", "12 class / 24 subclass", "11 ırk / geçmiş")),
    (New-Node "combat" "Combat" 770 120 260 130 "interface" @("ICombatant", "CombatService", "D20 zar ailesi")),
    (New-Node "items" "Items" 1110 120 260 130 "class" @("Item ailesi", "Inventory / Equipment", "Shop / StartingKit")),
    (New-Node "session" "GameSession" 520 390 300 130 "class" @("Player", "HexMap", "StoryProgress", "SaveVersion")),
    (New-Node "world" "World" 50 650 280 135 "class" @("HexMap / HexTile", "WorldLocation", "WorldEncounter", "AccessApproach")),
    (New-Node "story" "Story" 400 650 300 150 "abstract" @("StoryEvent", "StoryConsequence", "Quest", "StoryProgress / Director")),
    (New-Node "persistence" "Persistence" 780 650 280 135 "record" @("SaveGameService", "Save modelleri", "ItemSaveMapper")),
    (New-Node "rendering" "Assets + Rendering" 1130 650 300 150 "class" @("AssetCatalog", "CharacterAtlasRuntime", "EquipmentAnchorCatalog", "HexTilesetCatalog")),
    (New-Node "form" "MainGameForm" 520 950 300 105 "screen" @("GameSession", "Ekran geçişleri", "UiImages")),
    (New-Node "screens" "GameScreen ve ekranlar" 940 930 360 145 "screen" @("19 oyun ekranı", "PixelButton", "UiTheme", "800 x 800 çizim")),
    (New-Node "content" "Content + Scenes" 50 950 300 125 "record" @("JSON tanımları", "SceneNode ailesi", "Asset manifestleri"))
)
$edges = @(
    (New-Edge "entities" "characters" "inherit"),
    (New-Edge "characters" "combat" "implements"),
    (New-Edge "characters" "items" "contains" "envanter"),
    (New-Edge "session" "characters" "contains" "Player"),
    (New-Edge "session" "world" "contains" "Map"),
    (New-Edge "session" "story" "contains" "Story"),
    (New-Edge "session" "persistence" "uses" "kaydet/yükle"),
    (New-Edge "form" "session" "contains"),
    (New-Edge "form" "screens" "contains"),
    (New-Edge "screens" "rendering" "uses"),
    (New-Edge "content" "rendering" "uses"),
    (New-Edge "story" "combat" "uses"),
    (New-Edge "world" "combat" "uses")
)
Add-DiagramPage $document "overview" "01 - Genel Mimari" $nodes $edges 1600 1200 "Basit katmanlar ve ana sahiplik ilişkileri"

# 2 - Karakter, NPC ve savaş
$nodes = @(
    (New-Node "gameEntity" "GameEntity" 900 100 240 105 "abstract" @("Id", "Name", "IsActive")),
    (New-Node "player" "PlayerCharacter" 280 300 260 150 "class" @("Race / Class / Background", "Stats", "Inventory / Equipment", "Health / ArmorClass")),
    (New-Node "npc" "Npc" 900 300 240 105 "class" @("VisualAssetId", "Interact()")),
    (New-Node "friendly" "FriendlyNpc" 650 520 220 90 "class" @("DialogueKey")),
    (New-Node "enemy" "EnemyNpc" 1000 520 250 135 "class" @("Health / ArmorClass", "CharacterClass", "Weapon / RewardGold")),
    (New-Node "biomeEnemy" "BiomeEnemyNpc" 720 760 220 85 "class" @("Biome")),
    (New-Node "miniBoss" "MiniBossNpc" 1000 760 220 85 "class" @("SpecialAttack")),
    (New-Node "mainBoss" "MainBossNpc" 1280 760 220 100 "class" @("PhaseCount", "CurrentPhase")),
    (New-Node "stats" "CharacterStats" 50 520 220 120 "class" @("STR / DEX / CON", "INT / WIS / CHA", "Modifier()")),
    (New-Node "combatant" "ICombatant" 1500 300 250 150 "interface" @("Health / MaxHealth", "ArmorClass", "AttackBonus", "ReceiveDamage()")),
    (New-Node "combatService" "CombatService" 1500 560 250 115 "class" @("Attack()", "RollInitiative()", "RollDamage()")),
    (New-Node "diceInterface" "IDiceRoller" 1850 300 220 90 "interface" @("Roll()")),
    (New-Node "diceRoller" "DiceRoller" 1800 520 200 80 "class"),
    (New-Node "fixedDice" "FixedDiceRoller" 2050 520 220 80 "class"),
    (New-Node "diceRoll" "DiceRoll" 1850 760 220 105 "abstract" @("Sides", "RawValue", "Bonus / Total")),
    (New-Node "d20" "D20Roll" 1850 940 220 95 "class" @("Natural 1", "Natural 20")),
    (New-Node "attackRoll" "AttackRoll" 1700 1120 220 105 "class" @("TargetArmorClass", "IsHit / IsCritical")),
    (New-Node "initiative" "InitiativeRoll" 2000 1120 220 80 "class"),
    (New-Node "combatLog" "CombatLogEntry" 1450 900 230 90 "record"),
    (New-Node "damage" "DamageResult" 1450 1050 230 90 "record")
)
$edges = @(
    (New-Edge "player" "gameEntity" "inherit"),
    (New-Edge "npc" "gameEntity" "inherit"),
    (New-Edge "friendly" "npc" "inherit"),
    (New-Edge "enemy" "npc" "inherit"),
    (New-Edge "biomeEnemy" "enemy" "inherit"),
    (New-Edge "miniBoss" "enemy" "inherit"),
    (New-Edge "mainBoss" "enemy" "inherit"),
    (New-Edge "player" "combatant" "implements"),
    (New-Edge "enemy" "combatant" "implements"),
    (New-Edge "player" "stats" "contains"),
    (New-Edge "combatService" "combatant" "uses"),
    (New-Edge "combatService" "diceInterface" "contains"),
    (New-Edge "diceRoller" "diceInterface" "implements"),
    (New-Edge "fixedDice" "diceInterface" "implements"),
    (New-Edge "d20" "diceRoll" "inherit"),
    (New-Edge "attackRoll" "d20" "inherit"),
    (New-Edge "initiative" "d20" "inherit")
)
Add-DiagramPage $document "character_combat" "02 - Karakter, NPC ve Savaş" $nodes $edges 2400 1350 "Kalıtım, interface ve temel savaş nesneleri"

# 3 - Class, subclass, ırk ve geçmiş
$nodes = [System.Collections.Generic.List[object]]::new()
$edges = [System.Collections.Generic.List[object]]::new()
$nodes.Add((New-Node "characterClass" "CharacterClass" 1150 90 260 125 "abstract" @("BaseHitPoints", "PrimaryAbility", "AttackBonus()")))
$classFamilies = @(
    @("fighter","FighterClass","ChampionClass","BattleMasterClass"),
    @("barbarian","BarbarianClass","BerserkerClass","TotemWarriorClass"),
    @("bard","BardClass","LoreBardClass","ValorBardClass"),
    @("cleric","ClericClass","LifeClericClass","WarClericClass"),
    @("druid","DruidClass","LandDruidClass","MoonDruidClass"),
    @("monk","MonkClass","OpenHandMonkClass","ShadowMonkClass"),
    @("paladin","PaladinClass","DevotionPaladinClass","VengeancePaladinClass"),
    @("ranger","RangerClass","HunterClass","BeastMasterClass"),
    @("rogue","RogueClass","ThiefClass","AssassinClass"),
    @("sorcerer","SorcererClass","WildMagicSorcererClass","DraconicSorcererClass"),
    @("warlock","WarlockClass","FiendWarlockClass","GreatOldOneWarlockClass"),
    @("wizard","WizardClass","EvocationWizardClass","AbjurationWizardClass")
)
for ($i=0; $i -lt $classFamilies.Count; $i++) {
    $family = $classFamilies[$i]
    $column = $i % 4
    $row = [math]::Floor($i / 4)
    $x = 60 + $column * 620
    $y = 310 + $row * 470
    $mainId = $family[0]
    $nodes.Add((New-Node $mainId $family[1] $x $y 250 78 "class"))
    $nodes.Add((New-Node "${mainId}_a" $family[2] ($x-20) ($y+175) 250 78 "class"))
    $nodes.Add((New-Node "${mainId}_b" $family[3] ($x+280) ($y+175) 270 78 "class"))
    $edges.Add((New-Edge $mainId "characterClass" "inherit"))
    $edges.Add((New-Edge "${mainId}_a" $mainId "inherit"))
    $edges.Add((New-Edge "${mainId}_b" $mainId "inherit"))
}
$nodes.Add((New-Node "classCatalog" "ClassCatalog" 2700 90 240 100 "static" @("All", "AllSubclasses", "FindSubclass()")))
$edges.Add((New-Edge "classCatalog" "characterClass" "uses"))

$nodes.Add((New-Node "race" "CharacterRace" 3000 300 250 110 "abstract" @("BodyType", "Movement", "ApplyBonuses()")))
$raceNames = @("HumanRace","ElfRace","DwarfRace","HalflingRace","GnomeRace","HalfElfRace","DrowRace","HalfOrcRace","TieflingRace","DragonbornRace","GithyankiRace")
for ($i=0; $i -lt $raceNames.Count; $i++) {
    $x = 2650 + ($i % 3) * 330
    $y = 520 + [math]::Floor($i / 3) * 150
    $id = "race_$i"
    $nodes.Add((New-Node $id $raceNames[$i] $x $y 240 70 "class"))
    $edges.Add((New-Edge $id "race" "inherit"))
}
$nodes.Add((New-Node "raceCatalog" "RaceCatalog" 3000 1170 240 80 "static" @("All", "Find()")))
$edges.Add((New-Edge "raceCatalog" "race" "uses"))

$nodes.Add((New-Node "background" "CharacterBackground" 2700 1400 270 120 "abstract" @("StartingGold", "Choices", "ApplyBonuses()")))
$nodes.Add((New-Node "compositeBackground" "CompositeBackground" 2650 1630 270 85 "class"))
$nodes.Add((New-Node "backgroundChoice" "BackgroundChoice" 3000 1630 250 85 "class"))
$nodes.Add((New-Node "stageChoice" "BackgroundStageChoice" 3300 1630 260 85 "record"))
$nodes.Add((New-Node "backgroundCatalog" "BackgroundCatalog" 3000 1400 250 100 "static"))
$nodes.Add((New-Node "backgroundStage" "BackgroundStage" 3300 1400 220 80 "enum"))
$nodes.Add((New-Node "backgroundStat" "BackgroundStat" 3550 1400 220 80 "enum"))
$edges.Add((New-Edge "compositeBackground" "background" "inherit"))
$edges.Add((New-Edge "backgroundChoice" "background" "inherit"))
$edges.Add((New-Edge "backgroundCatalog" "background" "uses"))
$edges.Add((New-Edge "backgroundCatalog" "stageChoice" "uses"))
Add-DiagramPage $document "classes_races" "03 - Sınıflar, Alt Sınıflar, Irklar ve Geçmiş" $nodes.ToArray() $edges.ToArray() 3900 1900 "12 ana class, 24 subclass, 11 ırk ve geçmiş ailesi"

# 4 - Eşya, mağaza, oturum ve kayıt
$nodes = @(
    (New-Node "gameEntity" "GameEntity" 500 80 220 85 "abstract"),
    (New-Node "item" "Item" 500 250 240 115 "abstract" @("Value", "Weight", "CanBeSold")),
    (New-Node "weapon" "Weapon" 50 500 210 100 "class" @("DamageDie", "Slot")),
    (New-Node "armor" "Armor" 330 500 210 100 "class" @("ArmorBonus", "Slot")),
    (New-Node "fatebreaker" "FatebreakerArmor" 310 720 250 110 "class" @("LastStandCharges", "TryUseLastStand()")),
    (New-Node "consumable" "Consumable" 610 500 220 100 "class" @("Healing", "Use()")),
    (New-Node "questItem" "QuestItem" 900 500 220 100 "class" @("QuestKey", "CanBeSold=false")),
    (New-Node "inventory" "Inventory" 50 950 220 95 "class" @("Items", "TotalWeight")),
    (New-Node "equipment" "EquipmentSet" 330 950 230 105 "class" @("Equip()", "Unequip()", "Get()")),
    (New-Node "itemCatalog" "ItemCatalog" 630 950 220 90 "static"),
    (New-Node "kit" "StartingKitPackage" 950 900 260 110 "abstract" @("Weapon", "ExtraItems", "GiveTo()")),
    (New-Node "warriorKit" "WarriorStartingKit" 850 1120 240 80 "class"),
    (New-Node "expertKit" "ExpertStartingKit" 1130 1120 220 80 "class"),
    (New-Node "casterKit" "CasterStartingKit" 1390 1120 220 80 "class"),
    (New-Node "kitCatalog" "StartingKitCatalog" 1250 900 240 90 "static"),
    (New-Node "shopOffer" "ShopOffer" 50 1300 220 85 "record"),
    (New-Node "shopCatalog" "ShopCatalog" 330 1300 220 85 "static"),
    (New-Node "shopService" "ShopService" 610 1300 220 105 "class" @("TryBuy()", "TrySell()")),
    (New-Node "session" "GameSession" 1700 200 260 140 "class" @("Player", "Map", "Story", "ToSaveData()")),
    (New-Node "saveService" "SaveGameService" 1700 500 250 105 "class" @("Save()", "Load()", "Autosave()")),
    (New-Node "gameSave" "GameSaveData" 2050 500 240 85 "record"),
    (New-Node "characterSave" "CharacterSaveData" 2050 680 250 85 "record"),
    (New-Node "itemSave" "ItemSaveData" 2050 850 230 85 "record"),
    (New-Node "questSave" "QuestSaveData" 2050 1020 230 85 "record"),
    (New-Node "storySave" "StoryProgressSaveData" 2050 1190 270 85 "record"),
    (New-Node "saveMapper" "ItemSaveMapper" 1700 850 240 100 "static" @("ToSaveData()", "Restore()"))
)
$edges = @(
    (New-Edge "item" "gameEntity" "inherit"),
    (New-Edge "weapon" "item" "inherit"),
    (New-Edge "armor" "item" "inherit"),
    (New-Edge "fatebreaker" "armor" "inherit"),
    (New-Edge "consumable" "item" "inherit"),
    (New-Edge "questItem" "item" "inherit"),
    (New-Edge "inventory" "item" "contains"),
    (New-Edge "equipment" "item" "contains"),
    (New-Edge "warriorKit" "kit" "inherit"),
    (New-Edge "expertKit" "kit" "inherit"),
    (New-Edge "casterKit" "kit" "inherit"),
    (New-Edge "kitCatalog" "kit" "uses"),
    (New-Edge "shopService" "shopOffer" "uses"),
    (New-Edge "shopCatalog" "shopOffer" "contains"),
    (New-Edge "session" "gameSave" "uses"),
    (New-Edge "saveService" "gameSave" "uses"),
    (New-Edge "gameSave" "characterSave" "contains"),
    (New-Edge "characterSave" "itemSave" "contains"),
    (New-Edge "gameSave" "storySave" "contains"),
    (New-Edge "storySave" "questSave" "contains"),
    (New-Edge "saveMapper" "itemSave" "uses"),
    (New-Edge "saveMapper" "item" "uses")
)
Add-DiagramPage $document "items_save" "04 - Eşya, Mağaza, Oturum ve Kayıt" $nodes $edges 2500 1550 "Eşya kalıtımı, başlangıç paketleri ve JSON kayıt modelleri"

# 5 - Hikâye ve görevler
$nodes = @(
    (New-Node "storyEvent" "StoryEvent" 1050 100 270 140 "abstract" @("Id / Title / Scene", "Trigger / Requirement", "Resolve()")),
    (New-Node "dialogue" "DialogueStoryEvent" 50 400 250 85 "class"),
    (New-Node "skill" "SkillCheckStoryEvent" 350 400 260 95 "class" @("CheckBonus()", "Succeeds()")),
    (New-Node "combat" "CombatStoryEvent" 680 400 260 110 "class" @("CreateEnemy()", "EnemyTurn()", "Victory()")),
    (New-Node "defeat" "ScriptedDefeatStoryEvent" 1010 400 300 100 "class" @("Strike()")),
    (New-Node "ending" "EndingStoryEvent" 1380 400 260 100 "class" @("IsUnlocked()")),
    (New-Node "trigger" "StoryTrigger" 50 650 220 80 "record"),
    (New-Node "scene" "StoryScene" 310 650 220 80 "record"),
    (New-Node "check" "StoryCheck" 570 650 220 80 "record"),
    (New-Node "choice" "StoryChoice" 830 650 220 80 "record"),
    (New-Node "outcome" "StoryOutcome" 1090 650 220 80 "record"),
    (New-Node "enemyTurn" "EnemyTurnResult" 1350 650 230 80 "record"),
    (New-Node "triggerKind" "StoryTriggerKind" 50 800 220 70 "enum"),
    (New-Node "enemyTurnKind" "EnemyTurnKind" 310 800 220 70 "enum"),
    (New-Node "fateResult" "FateStrikeResult" 570 800 220 70 "enum"),
    (New-Node "consequence" "StoryConsequence" 1050 950 270 110 "abstract" @("Apply()", "Summary")),
    (New-Node "flagCon" "FlagConsequence" 50 1180 220 75 "class"),
    (New-Node "itemCon" "ItemConsequence" 310 1180 220 75 "class"),
    (New-Node "repCon" "ReputationConsequence" 570 1180 250 75 "class"),
    (New-Node "routeCon" "RouteConsequence" 860 1180 230 75 "class"),
    (New-Node "endingCon" "EndingConsequence" 1130 1180 230 75 "class"),
    (New-Node "questCon" "QuestConsequence" 1400 1180 230 75 "class"),
    (New-Node "costCon" "CostConsequence" 1670 1180 230 75 "class"),
    (New-Node "quest" "Quest" 2250 100 250 120 "abstract" @("Objectives", "Status", "Start / Complete / Fail")),
    (New-Node "mainQuest" "MainQuest" 2000 350 220 90 "class"),
    (New-Node "wayQuest" "WaystoneQuest" 2280 350 230 90 "class"),
    (New-Node "sideQuest" "SideQuest" 2570 350 220 90 "class"),
    (New-Node "objective" "QuestObjective" 2250 560 230 80 "record"),
    (New-Node "questChange" "QuestChange" 2530 560 220 75 "enum"),
    (New-Node "progress" "StoryProgress" 2200 800 260 130 "class" @("Flags / Trust", "Quests", "UnlockedEndings")),
    (New-Node "storyDirector" "StoryDirector" 2530 800 230 90 "static"),
    (New-Node "storyCatalog" "StoryCatalog" 2820 800 230 90 "static"),
    (New-Node "endingRules" "EndingRules" 2200 1040 230 90 "static"),
    (New-Node "storyFlags" "StoryFlags" 2480 1040 220 80 "static"),
    (New-Node "storyText" "StoryText" 2750 1040 220 80 "static"),
    (New-Node "remembered" "RememberedPerson" 2200 1240 240 80 "record"),
    (New-Node "cinematic" "CinematicLibrary" 2500 1240 240 90 "class"),
    (New-Node "sequence" "CinematicSequence" 2800 1240 250 80 "record"),
    (New-Node "storyDef" "StoryDefinition" 2200 1430 240 80 "record"),
    (New-Node "slotCatalog" "StorySlotCatalog" 2500 1430 230 80 "static"),
    (New-Node "marker" "StoryMarker" 2800 1430 220 80 "record"),
    (New-Node "vaelorState" "VaelorState" 3080 1430 220 75 "enum")
)
$edges = @()
foreach ($child in @("dialogue","skill","combat","defeat","ending")) { $edges += New-Edge $child "storyEvent" "inherit" }
foreach ($child in @("flagCon","itemCon","repCon","routeCon","endingCon","questCon","costCon")) { $edges += New-Edge $child "consequence" "inherit" }
foreach ($child in @("mainQuest","wayQuest","sideQuest")) { $edges += New-Edge $child "quest" "inherit" }
$edges += @(
    (New-Edge "storyEvent" "trigger" "contains"),
    (New-Edge "storyEvent" "scene" "contains"),
    (New-Edge "storyEvent" "choice" "contains"),
    (New-Edge "choice" "check" "contains"),
    (New-Edge "choice" "consequence" "contains"),
    (New-Edge "quest" "objective" "contains"),
    (New-Edge "progress" "quest" "contains"),
    (New-Edge "storyDirector" "storyCatalog" "uses"),
    (New-Edge "storyDirector" "progress" "uses"),
    (New-Edge "endingRules" "progress" "uses"),
    (New-Edge "cinematic" "sequence" "contains"),
    (New-Edge "slotCatalog" "storyDef" "contains")
)
Add-DiagramPage $document "story" "05 - Hikâye ve Görev Sistemi" $nodes $edges 3400 1650 "Olay, sonuç ve görev kalıtım aileleri"

# 6 - Dünya ve harita
$nodes = @(
    (New-Node "gameEntity" "GameEntity" 900 80 220 80 "abstract"),
    (New-Node "coordinate" "HexCoordinate" 50 180 220 90 "record" @("Q / R", "Neighbors()", "DistanceTo()")),
    (New-Node "hexTile" "HexTile" 360 180 240 130 "class" @("Biome", "MovementCost", "IsLocked / IsDangerous")),
    (New-Node "hexMap" "HexMap" 700 180 250 140 "class" @("Tiles", "CanMove()", "FindPath()", "CreateDemoMap()")),
    (New-Node "location" "WorldLocation" 1100 250 260 120 "abstract" @("LocationId", "SceneAsset", "Services")),
    (New-Node "village" "VillageLocation" 1000 520 220 75 "class"),
    (New-Node "town" "TownLocation" 1260 520 220 75 "class"),
    (New-Node "castle" "CastleLocation" 1520 520 220 75 "class"),
    (New-Node "tower" "EyeTowerLocation" 1780 520 220 75 "class"),
    (New-Node "locationService" "LocationService" 1450 250 230 75 "enum"),
    (New-Node "encounter" "WorldEncounter" 350 750 260 120 "abstract" @("Biome", "AssetPath", "Choices")),
    (New-Node "discovery" "DiscoveryEncounter" 50 1000 240 75 "class"),
    (New-Node "traveler" "TravelerEncounter" 350 1000 240 75 "class"),
    (New-Node "hazard" "HazardEncounter" 650 1000 240 75 "class"),
    (New-Node "encounterCatalog" "EncounterCatalog" 50 1210 230 80 "static"),
    (New-Node "encounterChoice" "EncounterChoice" 350 1210 230 80 "record"),
    (New-Node "encounterOutcome" "EncounterOutcome" 650 1210 230 80 "record"),
    (New-Node "choiceCatalog" "EncounterChoiceCatalog" 350 1400 260 80 "static"),
    (New-Node "encounterReport" "EncounterReport" 650 1400 230 80 "static"),
    (New-Node "access" "AccessApproach" 1250 850 260 115 "abstract" @("Stat", "Difficulty()", "Bonus()")),
    (New-Node "persuasion" "PersuasionApproach" 1000 1080 230 75 "class"),
    (New-Node "intimidation" "IntimidationApproach" 1290 1080 230 75 "class"),
    (New-Node "credential" "CredentialApproach" 1580 1080 230 75 "class"),
    (New-Node "accessCatalog" "AccessApproachCatalog" 1250 1290 260 80 "static"),
    (New-Node "worldObject" "WorldObject" 2050 250 230 90 "abstract" @("BlocksMovement")),
    (New-Node "commonEnums" "Ortak enumlar" 2050 550 300 180 "enum" @("Direction / Gender / Biome", "DiscoveryState", "CharacterAnimation", "EquipmentSlot", "QuestStatus / SceneKind"))
)
$edges = @(
    (New-Edge "worldObject" "gameEntity" "inherit"),
    (New-Edge "location" "gameEntity" "inherit"),
    (New-Edge "encounter" "gameEntity" "inherit"),
    (New-Edge "hexMap" "hexTile" "contains"),
    (New-Edge "hexTile" "coordinate" "contains"),
    (New-Edge "village" "location" "inherit"),
    (New-Edge "town" "location" "inherit"),
    (New-Edge "castle" "location" "inherit"),
    (New-Edge "tower" "location" "inherit"),
    (New-Edge "location" "locationService" "contains"),
    (New-Edge "discovery" "encounter" "inherit"),
    (New-Edge "traveler" "encounter" "inherit"),
    (New-Edge "hazard" "encounter" "inherit"),
    (New-Edge "encounter" "encounterChoice" "contains"),
    (New-Edge "encounterChoice" "encounterOutcome" "contains"),
    (New-Edge "choiceCatalog" "encounterChoice" "contains"),
    (New-Edge "persuasion" "access" "inherit"),
    (New-Edge "intimidation" "access" "inherit"),
    (New-Edge "credential" "access" "inherit"),
    (New-Edge "accessCatalog" "access" "uses")
)
Add-DiagramPage $document "world" "06 - Dünya, Harita ve Encounter" $nodes $edges 2500 1600 "Hex haritası, yerleşimler, encounter ve sosyal erişim"

# 7 - Asset, içerik, rendering ve sahneler
$nodes = @(
    (New-Node "assetCatalog" "AssetCatalog" 50 180 240 105 "class" @("Register()", "LoadManifest()", "Resolve()")),
    (New-Node "assetEntry" "AssetEntry" 350 180 220 75 "record"),
    (New-Node "assetManifest" "AssetManifest" 350 330 220 75 "record"),
    (New-Node "defaultAssets" "DefaultAssetCatalog" 50 430 250 85 "static"),
    (New-Node "jsonOptions" "JsonOptions" 350 480 220 75 "static"),
    (New-Node "contentLoader" "ContentDefinitionLoader" 750 180 270 95 "class" @("Load()")),
    (New-Node "contentDocument" "ContentDefinitionDocument" 1080 180 280 80 "record"),
    (New-Node "idName" "IdNameDefinition" 750 380 230 75 "record"),
    (New-Node "subclassDef" "SubclassDefinition" 1040 380 230 75 "record"),
    (New-Node "itemDef" "ItemDefinition" 1330 380 230 75 "record"),
    (New-Node "atlas" "CharacterAtlasRuntime" 50 750 270 105 "class" @("Load()", "SelectFrame()")),
    (New-Node "atlasManifest" "RuntimeAtlasManifest" 380 700 250 80 "record"),
    (New-Node "runtimeFrame" "RuntimeFrame" 380 850 220 75 "record"),
    (New-Node "runtimeRect" "RuntimeRect" 650 850 220 75 "record"),
    (New-Node "runtimePoint" "RuntimePoint" 920 850 220 75 "record"),
    (New-Node "anchorCatalog" "EquipmentAnchorCatalog" 1200 700 280 95 "class"),
    (New-Node "anchorProfile" "EquipmentAnchorProfile" 1200 880 270 80 "record"),
    (New-Node "tilesetCatalog" "HexTilesetCatalog" 1650 700 260 95 "class"),
    (New-Node "tileManifest" "HexTilesetManifest" 1650 880 250 80 "record"),
    (New-Node "tileDefinition" "HexTileDefinition" 1950 880 240 80 "record"),
    (New-Node "sceneNode" "SceneNode" 1750 160 240 105 "abstract" @("Id", "Title", "Kind")),
    (New-Node "travelScene" "TravelScene" 1600 380 220 75 "class"),
    (New-Node "combatScene" "CombatScene" 1870 380 220 75 "class"),
    (New-Node "campScene" "CampScene" 2140 380 220 75 "class"),
    (New-Node "shopScene" "ShopScene" 2410 380 220 75 "class")
)
$edges = @(
    (New-Edge "assetCatalog" "assetEntry" "contains"),
    (New-Edge "assetCatalog" "assetManifest" "uses"),
    (New-Edge "defaultAssets" "assetCatalog" "uses"),
    (New-Edge "contentLoader" "contentDocument" "uses"),
    (New-Edge "contentDocument" "idName" "contains"),
    (New-Edge "contentDocument" "subclassDef" "contains"),
    (New-Edge "contentDocument" "itemDef" "contains"),
    (New-Edge "atlas" "atlasManifest" "contains"),
    (New-Edge "atlasManifest" "runtimeFrame" "contains"),
    (New-Edge "runtimeFrame" "runtimeRect" "contains"),
    (New-Edge "runtimeFrame" "runtimePoint" "contains"),
    (New-Edge "anchorCatalog" "anchorProfile" "contains"),
    (New-Edge "tilesetCatalog" "tileManifest" "contains"),
    (New-Edge "tileManifest" "tileDefinition" "contains"),
    (New-Edge "travelScene" "sceneNode" "inherit"),
    (New-Edge "combatScene" "sceneNode" "inherit"),
    (New-Edge "campScene" "sceneNode" "inherit"),
    (New-Edge "shopScene" "sceneNode" "inherit")
)
Add-DiagramPage $document "assets_rendering" "07 - Asset, İçerik, Rendering ve Sahneler" $nodes $edges 2800 1150 "Manifest, runtime atlas, tile ve sahne tipleri"

# 8 - WinForms UI
$nodes = [System.Collections.Generic.List[object]]::new()
$edges = [System.Collections.Generic.List[object]]::new()
$nodes.Add((New-Node "control" "System.Windows.Forms.Control" 1050 80 300 85 "abstract"))
$nodes.Add((New-Node "gameScreen" "GameScreen" 1050 250 300 135 "abstract" @("Draw()", "AddButton()", "HandleKey()", "800 x 800 ölçekleme")))
$screenNames = @(
    "MainMenuScreen","SettingsScreen","CharacterCreationScreen","AttributeAllocationScreen",
    "CharacterSummaryScreen","CharacterRecordScreen","StorySelectionScreen","StorySceneScreen",
    "MapScreen","EncounterScreen","CombatScreen","InventoryScreen","JournalScreen","CampScreen",
    "LocationScreen","ShopScreen","InnScreen","GuardDialogueScreen","DeathScreen","EndingScreen"
)
for ($i=0; $i -lt $screenNames.Count; $i++) {
    $x = 50 + ($i % 5) * 500
    $y = 520 + [math]::Floor($i / 5) * 190
    $id = "screen_$i"
    $nodes.Add((New-Node $id $screenNames[$i] $x $y 300 75 "screen"))
    $edges.Add((New-Edge $id "gameScreen" "inherit"))
}
$nodes.Add((New-Node "form" "MainGameForm" 2800 180 260 115 "screen" @("ShowScreen()", "GameSession", "UiImages")))
$nodes.Add((New-Node "pixelButton" "PixelButton" 2800 420 240 110 "class" @("Bounds", "State", "OnClick")))
$nodes.Add((New-Node "buttonState" "PixelButtonState" 3100 420 220 70 "enum"))
$nodes.Add((New-Node "uiImages" "UiImages" 2800 650 240 100 "class" @("Get()", "CreateCursor()")))
$nodes.Add((New-Node "uiTheme" "UiTheme" 3100 650 230 90 "static"))
$nodes.Add((New-Node "panelStyle" "PanelStyle" 3100 850 220 70 "enum"))
$nodes.Add((New-Node "characterArt" "CharacterArt" 2800 850 230 90 "static"))
$nodes.Add((New-Node "rootLocator" "GameDesignRootLocator" 2800 1050 270 80 "static"))
$nodes.Add((New-Node "creationDraft" "CharacterCreationDraft" 3100 1050 260 95 "class"))
$nodes.Add((New-Node "program" "Program" 2800 1250 220 80 "static"))
$nodes.Add((New-Node "uiEnums" "Ekran enumları" 3100 1250 260 130 "enum" @("CombatModal", "JournalFilter", "ShopMode", "StoryScenePhase")))
$edges.Add((New-Edge "gameScreen" "control" "inherit"))
$edges.Add((New-Edge "form" "gameScreen" "contains"))
$edges.Add((New-Edge "gameScreen" "pixelButton" "contains"))
$edges.Add((New-Edge "form" "uiImages" "contains"))
$edges.Add((New-Edge "gameScreen" "uiTheme" "uses"))
$edges.Add((New-Edge "characterArt" "uiImages" "uses"))
$edges.Add((New-Edge "program" "form" "uses"))
Add-DiagramPage $document "ui" "08 - WinForms UI ve Oyun Ekranları" $nodes.ToArray() $edges.ToArray() 3500 1500 "Bütün ekranlar tek GameScreen tabanından türer"

# 9 - Atlas aracı ve test projesi
$nodes = @(
    (New-Node "toolProgram" "RetroRpg.AtlasTool.Program" 50 150 300 85 "static"),
    (New-Node "builder" "CharacterAtlasBuilder" 450 150 300 100 "class" @("Build()")),
    (New-Node "detector" "AlphaRegionDetector" 850 150 280 85 "static"),
    (New-Node "manifest" "CharacterAtlasManifest" 450 400 300 80 "record"),
    (New-Node "atlasFrame" "AtlasFrame" 850 400 240 80 "record"),
    (New-Node "intRect" "IntRect" 1150 400 220 75 "record"),
    (New-Node "intPoint" "IntPoint" 1150 550 220 75 "record"),
    (New-Node "tests" "RetroRpg.Tests.Program" 50 750 300 105 "static" @("56 test", "Smoke ve hikâye akışı")),
    (New-Node "core" "RetroRpg.Core" 500 750 300 100 "abstract" @("Test edilen oyun kuralları")),
    (New-Node "game" "RetroRpg.Game" 900 750 300 100 "screen" @("Screenshot QA"))
)
$edges = @(
    (New-Edge "toolProgram" "builder" "uses"),
    (New-Edge "builder" "detector" "uses"),
    (New-Edge "builder" "manifest" "uses"),
    (New-Edge "manifest" "atlasFrame" "contains"),
    (New-Edge "atlasFrame" "intRect" "contains"),
    (New-Edge "atlasFrame" "intPoint" "contains"),
    (New-Edge "tests" "core" "uses"),
    (New-Edge "tests" "game" "uses")
)
Add-DiagramPage $document "tools_tests" "09 - Atlas Aracı ve Test Projesi" $nodes $edges 1500 1000 "Üretim aracı ve test bağımlılıkları"

[void]$document.AppendLine('</mxfile>')

$outputDirectory = Split-Path -Parent $OutputPath
New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
[System.IO.File]::WriteAllText($OutputPath, $document.ToString(), [System.Text.UTF8Encoding]::new($false))

# XML biçimini ayrıca doğrula. Hata varsa bozuk dosya teslim edilmez.
[void][xml]([System.IO.File]::ReadAllText($OutputPath))
Write-Output "DRAWIO_CREATED=$OutputPath"
