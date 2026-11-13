# Civilian and Profession NPC Sprites V1

800x800 retro RPG için hazırlanmış 48 adet tam beden, şeffaf NPC sprite paketidir.

## Paketler

### Civilian — 24 sprite

- Young villager
- Adult villager
- Elderly villager
- Traveler
- Refugee
- Minor noble
- Dockworker
- Stablehand
- Street vendor
- Pilgrim
- Entertainer
- Household servant

Her rolün maskülen ve feminen bir sürümü bulunur.

### Profession — 24 sprite

- Merchant
- Blacksmith
- Innkeeper
- Healer / apothecary
- Town guard
- Farmer
- Fisher
- Miner
- Scholar / scribe
- Priest
- Carpenter / artisan
- Courier

Her mesleğin maskülen ve feminen bir sürümü bulunur.

## Teknik standart

- 48 adet 256x256 RGBA PNG
- Paket başına 6x4 şeffaf atlas
- Ortak ayak pivotu: `[128,238]`
- İnsan, Elf, Cüce, Halfling, Gnome, Drow, Half-Orc, Tiefling ve diğer fantastik soyları kapsayan çeşitlilik
- Oyun içi sivil kalabalık, dükkân, görev veren kişi ve yerleşim sahnelerinde kullanıma uygun

## ImageGen üretim özeti

Yerleşik ImageGen aracı kullanıldı. Sivil istemi 12 gündelik rolün kadın/erkek eşlerini; meslek istemi 12 iş rolünün kadın/erkek eşlerini, küçük bir meslek ipucu ve şeffaf arka planla istedi. Büyük silah, savaş pozu, sahne, yazı ve gölge yasaklandı.

## Yeniden derleme

```powershell
python .\tools\build_npc_sprite_pack.py --root . --pack civilian --source civilian-npc-sprites-source-v1.png
python .\tools\build_npc_sprite_pack.py --root . --pack profession --source profession-npc-sprites-source-v1.png
```
