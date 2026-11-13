# Class Outfit Animation Layers V1

Temel karakter bedeninin üstüne bindirilmek üzere hazırlanmış şeffaf sınıf kıyafeti animasyon katmanlarıdır.

## Tamamlanan sınıflar

- Barbarian / Barbar — 24 kare
- Bard / Ozan — 24 kare
- Cleric / Ruhban — 24 kare
- Druid — 24 kare
- Fighter / Savaşçı — 24 kare
- Monk / Keşiş — 24 kare
- Paladin — 24 kare
- Ranger / Korucu — 24 kare
- Rogue / Haydut — 24 kare
- Sorcerer / Büyü Kaynaklı — 24 kare
- Warlock / Antlaşmalı Büyücü — 24 kare
- Wizard / Sihirbaz — 24 kare
- Toplam: 288 ayrı 256x256 RGBA katman karesi

Her sınıf için iki beden sunumu, üç hareket ve dört yön bulunur:

- Male ve Female
- Walk, Attack ve Cast
- Front, Right, Back ve Left

## Atlas düzeni

- Satır 1: male walk front/right/back/left, attack front/right
- Satır 2: male attack back/left, cast front/right/back/left
- Satır 3: female walk front/right/back/left, attack front/right
- Satır 4: female attack back/left, cast front/right/back/left

Her hücre 256x256'dır. Ortak ayak/pivot ankrajı `[128, 236]` olarak tanımlanmıştır.

## Motor birleştirme sırası

1. Base body
2. Body outfit
3. Armor/accessory additions
4. Held weapon/shield
5. Spell or combat VFX
6. UI state overlays

Bu paketteki kareler `body_outfit` katmanıdır. Kaynak atlaslardaki kıyafetler boş manken mantığıyla çizildiği için yüz, ten, saç veya beden kimliği taşımaz.

## ImageGen üretim özeti

Yerleşik ImageGen aracı kullanıldı. Her sınıfın erkek/kadın baseline PNG'leri tasarım referansı olarak verildi. İstem seti; referans renk ve malzemelerini koruyan, vücut/ten/baş/saç içermeyen, dört yönlü walk/attack/cast pozlarında 24 boş kıyafet karesi ve gerçek şeffaf alfa istedi.

## Yeniden derleme

```powershell
python .\tools\build_class_animation_layer.py --root . --class-slug barbarian --class-name-tr Barbar --source barbarian-outfit-animation-layers-source-v1.png
python .\tools\build_class_animation_layer.py --root . --class-slug bard --class-name-tr Ozan --source bard-outfit-animation-layers-source-v1.png
python .\tools\build_class_animation_layer.py --root . --class-slug cleric --class-name-tr Ruhban --source cleric-outfit-animation-layers-source-v1.png
python .\tools\build_class_animation_layer.py --root . --class-slug druid --class-name-tr Druid --source druid-outfit-animation-layers-source-v1.png
python .\tools\build_class_animation_layer.py --root . --class-slug fighter --class-name-tr Savaşçı --source fighter-outfit-animation-layers-source-v1.png
python .\tools\build_class_animation_layer.py --root . --class-slug monk --class-name-tr Keşiş --source monk-outfit-animation-layers-source-v1.png
python .\tools\build_class_animation_layer.py --root . --class-slug paladin --class-name-tr Paladin --source paladin-outfit-animation-layers-source-v1.png
python .\tools\build_class_animation_layer.py --root . --class-slug ranger --class-name-tr Korucu --source ranger-outfit-animation-layers-source-v1.png
python .\tools\build_class_animation_layer.py --root . --class-slug rogue --class-name-tr Haydut --source rogue-outfit-animation-layers-source-v1.png
python .\tools\build_class_animation_layer.py --root . --class-slug sorcerer --class-name-tr "Büyü Kaynaklı" --source sorcerer-outfit-animation-layers-source-v1.png
python .\tools\build_class_animation_layer.py --root . --class-slug warlock --class-name-tr "Antlaşmalı Büyücü" --source warlock-outfit-animation-layers-source-v1.png
python .\tools\build_class_animation_layer.py --root . --class-slug wizard --class-name-tr Sihirbaz --source wizard-outfit-animation-layers-source-v1.png
```

## Durum

On iki temel sınıfın tamamı bitmiş ve otomatik QA'dan geçmiştir.
