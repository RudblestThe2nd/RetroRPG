# Retro RPG — Mekanik Araştırması ve Uygulama Kararları

Bu belge 31 Ekim 2026 tarihinde, hikâye yazımına geçmeden önce yapılan mekanik düzeltmeleri ve dayandıkları tasarım ilkelerini açıklar.

## 1. D20 ve görünür sonuç

D20 testi üç ayrı bilgiyi tek kayıttan göstermelidir:

1. Ham zar (`RawValue`)
2. Nitelik/sınıf bonusu (`Bonus`)
3. Toplam (`RawValue + Bonus`)

Animasyon kareleri sonuç değildir. Animasyon sırasında zarın merkezinde `?` görünür; yalnız son aşamada gerçek ham değer gösterilir. Böylece animasyonda 20 görünüp sistemin 1 uygulaması gibi algılanan hata ortadan kalkar. Doğal 20 otomatik başarı/kritik, doğal 1 otomatik başarısızlıktır.

Kaynak: [D&D 2024 Free Rules — Playing the Game](https://www.dndbeyond.com/sources/dnd/br-2024/playing-the-game)

## 2. Sosyal konuşma ve muhafız erişimi

Kışla ve savaş odası artık serbest hizmet düğmeleri değildir. Muhafız konuşması üç yaklaşım sunar:

- **İkna:** Karizma d20 kontrolü.
- **Gözdağı:** Güç d20 kontrolü.
- **Kimlik:** Soylu aile geçmişi savaş odasında; muhafız mesleği kışlada otomatik geçiş sağlar.

Kışla zorluğu 12, savaş odası zorluğu 15'tir. Sistem `AccessApproach` taban sınıfından `PersuasionApproach`, `IntimidationApproach` ve `CredentialApproach` alt sınıflarını kullanır. Bu yapı hem ödevin inheritance hedefini gösterir hem de ekran kodunu basit tutar.

D&D sosyal etkileşim kuralında NPC tutumu ve oyuncunun yaklaşımı önemlidir; tereddütlü NPC için İkna, Aldatma veya Gözdağı gibi kontroller kullanılır. Oyundaki sade sistem bu ilkenin küçük ölçekli uyarlamasıdır.

Kaynaklar:

- [D&D 2024 Free Rules — Social Interaction](https://www.dndbeyond.com/sources/dnd/br-2024/playing-the-game)
- [D&D 2024 Free Rules — Influence](https://www.dndbeyond.com/sources/dnd/br-2024/rules-glossary)
- [Kingdom Come: Deliverance II — seçimlerin ilişkilere etkisi](https://www.deepsilver.com/games/kingdom-come-deliverance-ii/news/kingdom-come-deliverance-the-story-so-far)

## 3. Başlangıç ekipmanı

Başlangıç envanteri artık üç katmandan oluşur:

1. **Sınıf paketi:** Ana silah, zırh veya sınıfa uygun tüketilebilir/araç.
2. **Geçmiş paketi:** Soylu mührü, nöbetçi rozeti, şifacı merhemi, avcı azığı veya tüccar defteri gibi seçimle ilişkili eşya.
3. **Hikâye paketi:** Seçilen hikâye yuvasına özel anahtar eşya ve yol azığı.

### Sınıf paketleri

| Sınıf | Ana ekipman | Ek ekipman |
|---|---|---|
| Barbar | Büyük Kılıç | Yol Azığı |
| Bard | İnce Kılıç | İyileştirme İksiri |
| Ruhban | Topuz | İyileştirme İksiri |
| Druid | Topuz | Yol Azığı |
| Savaşçı | Büyük Kılıç | Dolgulu Yelek |
| Keşiş | Topuz | Yol Azığı |
| Paladin | Savaş Çekici | Zincir Gömlek, İyileştirme İksiri |
| Korucu | Hafif Arbalet | Yol Azığı |
| Düzenbaz | Hançer | Maymuncuk Takımı |
| Büyücü (Sorcerer) | Hançer | İyileştirme İksiri |
| Warlock | Hançer | Maceracı Meşalesi |
| Wizard | Hançer | Büyü Notları |

Bu paketler bire bir D&D kopyası değildir; oyunun mevcut eşya kataloğuna ve basit kod hedefine uyarlanmıştır. Resmî kurallarda başlangıç ekipmanının sınıf ve geçmiş seçimleriyle belirlenmesi temel alınmıştır.

Kaynaklar:

- [D&D 2024 Free Rules — Creating a Character](https://www.dndbeyond.com/sources/dnd/br-2024/creating-a-character)
- [D&D 2024 Free Rules — Equipment](https://www.dndbeyond.com/sources/dnd/br-2024/equipment)
- [D&D Adventurers League Player's Guide v14](https://media.dndbeyond.com/compendium-images/ddal/reference-docs/03-2024/d%26d-adventurers-league-player%27s-guide-v14.0.pdf)

## 4. Encounter ödül ilkesi

Ödül, yapılan eylemin doğal sonucu olmalıdır:

- Bilgi/öğüt: Bilgelik, bilgi bayrağı veya yeni konuşma seçeneği.
- Tehlikeli geçit: Rota açılması; altın verilmez.
- Hazine araması: Altın veya eşya olabilir.
- Yardım edilen yolcu: Rota bilgisi, erzak veya ilişki bilgisi.
- Başarısız fiziksel tehlike: CAN kaybı ve geçidin kapalı kalması.

Örnek: Dağ Keşişi öğüdü artık altın değil `Bilgelik +1` verir. Halat köprüyü koşarak geçmek altın vermez; rota açar. Başarısızlıkta karakter başlangıç tarafında kalır ve CAN kaybeder.

Tüm 50 encounter'ın seçenek ve sonuç listesi `documents/RANDOM_ENCOUNTERS.md` dosyasında koddan otomatik üretilir.

## 5. Han, satış, yemek ve kaçış

- Hana girmek ücretsizdir. Ücret yalnız oyuncu **Dinlen** seçeneğini onaylayınca alınır.
- Han içinde söylenti/olay kancası vardır; hikâye yazıldığında göreve veya özel encounter'a bağlanabilir.
- Pazar ve demirci hem satın alma hem satış ekranına sahiptir. Satış fiyatı eşyanın temel değerinin yarısıdır.
- Görev/kimlik eşyaları satılamaz. Kuşanılmış eşya satılırsa önce otomatik çıkarılır.
- Envanterde kuşanılmış eşya için düğme `ÇIKAR` olur.
- Kamp yemeği altınla yoktan üretilmez; Yol Azığı, yabani bal veya şifalı ot tüketir.
- Kaçış Dayanıklılık d20, zorluk 12 ile çözülür. Başarıda 1 CAN yorgunluk; başarısızlıkta d4 CAN ve düşmanın serbest saldırısı uygulanır.

## 6. Kilitli hexlerin anlamı

Kilitli hexler arıza değildir. Önceki iki düz şerit kaldırıldı; engeller biyom biçimlerini izleyen düzensiz kümelerdir:

- Mor/yozlaşmış bölgenin sınırındaki karanlık yarıklar yalnız `(-3,-4)` güney girişini açık bırakır. Göz Kulesi'ne giden her rota bu kapıdan geçer.
- Dağlarda kırık bir kayalık çember, volkan çevresinde ayrı lav gölleri, bataklıkta iki dipsiz çamur kümesi bulunur.
- Çölde birbirinden ayrı üç kanyon/mesa kümesi, karlı bölgede parçalı buz uçurumları ve yeraltında çökmüş mağara ağızları vardır.

İki geçit keşifle açılabilir:

- `(3,-2)`: Çığla kapanmış kısa dağ geçidi. Dağ Keşişi veya halat köprü sonucu açar; dağ yolunu ciddi biçimde kısaltır.
- `(-6,0)`: Çökmüş yeraltı geçidi. Maden asansörünü onarma sonucu açar.

Kalıcı engeller açılmaz; açılabilir geçitler encounter sonucu `ApplyUnlock` ile açılır. Harita imleçle üzerine gelindiğinde ve rota seçimi başarısız olduğunda nedeni açıkça yazar. Hex katmanı açıldığında geçilemeyen arazi kırmızı-kahverengi çizgi, koyu dolgu ve `×`; açılabilir geçitler altın `◆` ile görünür.

## 7. Karakter görsel kullanımı

Tam vücut karakter karesi karakter oluşturma, özellik, özet ve envanter ekranlarından kaldırıldı. Bu ekranlar portre kullanır. Tam vücut sprite yalnız hareket ve savaş gibi gerçekten gerekli sahnelerde kalır ve daha sert piksel ızgarası/renk indirgemesiyle çizilir.

Kimlik ekranında saç ve sakal/yüz detayı varyant seçimi aktiftir. Seçimler oturum bayraklarına kaydedilir; sonraki aşamada ayrı katman atlaslarına bağlanabilecek şekilde veri olarak korunur.
