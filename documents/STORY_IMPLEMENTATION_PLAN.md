# Kader Yolları: Göz Kulesi — Hikâye Uygulama Planı

**Uygulama durumu (1 Kasım 2026):** Plan koda bağlandı ve prologdan dört finale kadar oynanabilir. Uygulanan koordinatlar ve plandan küçük farklar `DEVIR.md` içindeki "1 Kasım — Ana hikâye koda bağlandı" bölümündedir (mor kapı `(-5,-3)`; kule içi sahnelerde mevcut v1/v5 görselleri; Vaelor ve Kaderbozan için geçici görseller).

## 1. Temel hedef

Oyuncunun karakter oluşturma sırasında seçtiği ırk, sınıf, alt sınıf ve yedi aşamalı geçmiş; tek bir ana hikâyenin farklı çözüm yollarını açacaktır. Haritadaki yolculuk, d20 kontrolleri, yerleşimler, savaş dışı encounter'lar ve Göz Kulesi aynı ana çatışmaya bağlanacaktır.

Ana tema: **Kader mi, özgür irade mi?**

Ana hikâye adı: **Kader Yolları: Göz Kulesi**

Ana düşman: **Baş Haritacı Vaelor**

Vaelor'un amacı dünyayı yok etmek değildir. Savaşların ve salgınların yollar üzerinden yayıldığına inanır; Kader Merceği ile yolları ve bu yollara ait anıları silerek toplumları birbirinden ayırmaya çalışır. Yöntemi dünyayı mor yozlaşmaya sürükler.

## 2. Tasarım kuralları

- İlk sürümde yalnız `story-slot-1` açılacaktır. Diğer iki hikâye yuvası kilitli kalacaktır.
- Hikâye tek oyunculu ve tek kahramanlı olacaktır; zorunlu parti sistemi eklenmeyecektir.
- Büyük dallanma yerine aynı hedefe giden savaş, beceri, sosyal ve keşif çözümleri kullanılacaktır.
- Sınıflar farklı hikâyeler yaratmayacak; aynı sahnede farklı çözüm seçenekleri açacaktır.
- Irk tepkileri kısa diyalog ve küçük bonuslarla sınırlı tutulacaktır.
- Önemli finaller yalnız tek bir rastgele zara bağlı olmayacaktır. Önceki kararlar final seçeneklerini açacak; d20 yalnız bedeli veya uygulama başarısını etkileyecektir.
- Mevcut 50 random encounter korunacaktır. Ana hikâye encounter'ları belirli koordinatlarda, bir kez çalışan ayrı olaylar olacaktır.
- Harita tamamen açık kalacaktır. Görev hedefleri gösterilecek; düşman ve gizli tehlikeler gösterilmeyecektir.

## 3. Karakter geçmişinin ana hikâyeye bağlanması

Oyuncu, dünyanın geri kalanının unuttuğu bir kişiyi veya yeri hatırlar. Hatırlanan bağ, geçmiş seçimlerinden otomatik çıkarılır.

Öncelik sırası:

1. `turning-loss`: kaybolan yakın veya çocukluk arkadaşı.
2. `turning-oath` ya da `goal-protect`: korunacağına yemin edilen kişi.
3. `family-soldiers` ya da `profession-guard`: unutulan birlik ve komutan.
4. `family-nobles`: kayıp aile elçisi veya kardeş.
5. `education-academy`, `profession-scribe` ya da `goal-knowledge`: kayıp hoca.
6. `family-merchants`, `profession-trader` ya da `childhood-caravan`: kayıp kervan lideri.
7. Varsayılan: oyuncunun doğduğu bölgeden kaybolan bir yolcu.

Bu seçim şu bayraklarla saklanacaktır:

- `story:remembered-type`
- `story:remembered-name`
- `story:remembered-relation`

Karakter sınıfı çözüm yaklaşımını belirler:

- Savaşçı, Barbar, Paladin: güç, tehdit, savunma.
- Düzenbaz, Korucu: gizlilik, iz sürme, kilit ve tuzak.
- Sihirbaz, Büyücü, Cadı Avcısı: rün, büyü ve Kader Merceği bilgisi.
- Ruhban, Druid, Keşiş: ruh, yozlaşma ve arındırma.
- Ozan ve yüksek Karizma karakterleri: ikna, yalan, uzlaşma.

Alt sınıflar seçenek metnini ve küçük bonusu değiştirecek; ayrı görev zinciri üretmeyecektir.

## 4. Baştan sona hikâye akışı

### Prolog — Haritadan Silinen Yol

Karakter oluşturma ve hikâye seçiminden sonra doğrudan haritaya geçilmez. Yeni `StorySceneScreen` açılır.

Sahne 1: Oyuncu Yeşilgeçit yakınında parçalanmış bir kervan bulur. Haritada yol görünürken çevredeki insanlar bu yolun hiç var olmadığını söyler.

Sahne 2: Oyuncu enkazda göz işaretli siyah harita parçasını ve ölen bir yol haritacısının yarım kalmış günlüğünü bulur. **Kalpaklı Süvari prologda görünmez.**

İlk seçimler:

- Enkazdaki son yolcuyu kurtar: Şifa/Bilgelik kontrolü; Yeşilgeçit'te tanık ifadesi açılır.
- Harita parçasını incele: Zekâ kontrolü; Vaelor ve Yol Taşları hakkında erken bilgi alınır.
- Kervanın izini takip et: Bilgelik/Çeviklik kontrolü; ilk görev eşyası ve kısa yol bulunur.

Kesin sonuçlar:

- Oyuncu `Kara Harita Parçası` görev eşyasını alır.
- `main-01-silinen-yol` görevi başlar.
- Karakterin hatırladığı kişi belirlenir.
- Oyuncu Yeşilgeçit'te haritaya bırakılır.

### Bölüm I — Silinen Kervan

Amaç: Kara harita parçasının kaynağını bulmak ve ilk Yol Taşı'nı keşfetmek.

Ana duraklar:

1. Yeşilgeçit hanında tanıklarla konuşma.
2. Kervan enkazını `ÇEVREYİ ARA` ile inceleme.
3. Kızılburç Kalesi arşivine erişme.
4. Muhafız veya arşiv görevlisini sosyal, kimlik ya da gizlilik yöntemiyle aşma.
5. Üç Yol Taşı'nın konumu öğrenilir.

Bölüm sonu:

- Günlüğe üç ana görev eklenir.
- Bataklık, dağ ve çöl hedefleri haritada görev işareti alır.
- Göz Kulesi görünür kalır fakat `Üç Yol Taşı etkinleşmeden kuleye giden kader yolu kapalı.` gerekçesiyle erişilemez olur.

### Bölüm II — Üç Yol Taşı

Üç görev istenen sırada tamamlanabilir.

#### A. Kök Taşı — Orman ve Bataklık

Konum: Batmış tapınak.

Çatışma: Bataklık köylüleri taşı yok etmek, yerel cadı taşı bağlamak, tapınak ruhları taşı arındırmak ister.

Çözümler:

- Ruhları yenip taşı zorla almak.
- Bilgelik veya din bilgisiyle taşı arındırmak.
- Cadıyla anlaşarak taşı bağlamak.
- Köylüleri ikna edip tapınağı birlikte boşaltmak.

Kaydedilen karar: `waystone:root:mode = cleanse | bind | seize`

Ödül: `Kök Taşı Parçası`, bataklık güveni ve/veya şifa avantajı.

#### B. Gök Taşı — Dağ ve Kar

Konum: Çökmüş geçidin arkasındaki zirve manastırı.

Çatışma: Dağ keşişleri taşı uyandırmanın çığ ve fırtına yaratacağını düşünür; yozlaşmış muhafız taşı ele geçirmek ister.

Çözümler:

- `unlock:mountain-pass` sonucuyla geçidi açmak.
- Eski madenden gizli rota bulmak.
- Keşişi ikna etmek.
- Muhafız mini-boss ile savaşmak.

Kaydedilen karar: `waystone:sky:mode = protect | awaken | seize`

Ödül: `Gök Taşı Parçası`, dağ geçidi ve yolculuk bonusu.

#### C. Güneş Taşı — Çöl ve Tropik Kıyı

Konum: İki ticaret şehri arasındaki eski gözlemevi.

Çatışma: İki şehir taşı ticaret yollarını kontrol etmek için ister.

Çözümler:

- Şehirlerden birini desteklemek.
- Tarafları uzlaştırmak.
- Taşı çalmak.
- Gözlemevi muhafızını yenmek.

Kaydedilen karar: `waystone:sun:mode = share | faction-a | faction-b | steal`

Ödül: `Güneş Taşı Parçası`, ticaret indirimi ve/veya altın.

### Ara bölüm — Kırık Arşivin Mesajı

İki Yol Taşı tamamlandığında sabit bir hikâye encounter'ı çalışır.

- Etkinleşen taşlar, Kızılburç arşivindeki göz mühürlü levhinin gizli metnini görünür kılar.
- Bilgi, oyuncunun önceki tercihlerine göre arşiv görevlisi, göz işaretli bir düşman subayı veya doğrudan levhinin anı yankısından alınır.
- Oyuncunun hatırladığı kişinin Göz Kulesi'nde tutulduğu veya Kader Merceği içinde bir anı yankısına dönüştürüldüğü anlaşılır.
- **Kalpaklı Süvari bu ara bölümde de görünmez ve hiçbir ana görev vermez.**

### Bölüm III — Mor Bölgeye Giriş

Üç Yol Taşı tamamlanınca:

- Kuleye giden tek geçit açılır.
- `main-03-goz-kulesi` görevi başlar.
- Mor bölgedeki random encounter sıklığı düşer; el yapımı hikâye encounter'ları devreye girer.

Sabit olaylar:

1. Adını unutmuş mülteciler.
2. Unutulan asker devriyesi.
3. Oyuncunun geçmişinden bir anı yankısı.
4. Göz Piskoposu mini-boss'u.
5. Kule kapısı ve son dinlenme noktası.

Bu olayların bir kısmı savaşmadan çözülebilir. Başarısızlık ilerlemeyi tamamen durdurmaz; can, eşya, yol veya NPC kaybı yaratır.

### Bölüm IV — Göz Kulesi

Kule dört aşamalı olacaktır:

1. **Arşiv:** Vaelor'un neden yolları silmeye başladığı öğrenilir.
2. **Unutulanlar Hapishanesi:** Oyuncunun hatırladığı kişi veya anı yankısı bulunur.
3. **Kader Merceği Odası:** Üç Yol Taşı'nın nasıl kullanılacağı seçilir.
4. **Vaelor ile ilk yüzleşme:** Kaçınılmaz scripted yenilgi ve kuleden düşüş.

#### Scripted ilk boss karşılaşması

- Oyuncu Vaelor'un salonuna girmeden hemen önce autosave alınır.
- Oyuncuya normal boss ekranı gösterilir ve en az bir gerçek eylem yapmasına izin verilir; böylece sahne doğrudan bir ara video gibi başlamaz.
- Vaelor ilk turun sonunda `Kaderin Hükmü` adlı özel saldırıyı kullanır.
- Bu saldırı normal zırhı, AC'yi ve hasar azaltmayı yok sayar. Oyuncu öldürülmüş gibi görünür.
- Normal ölüm ekranı açılmaz. Bu sonuç hikâye tarafından kontrollü `ScriptedDefeatStoryEvent` olarak işlenir.
- Vaelor oyuncunun bedenini kulenin tepesinden aşağı atar.

#### Kuleden düşüş ve Kalpaklı Süvari

Oyuncu kulenin altındaki yıkık taşlıkta gözlerini açar. Karşısında daha önce oyunda hiç görülmemiş olan **Kalpaklı Süvari** vardır.

- Süvari ana görev zincirinin parçası değildir.
- Kimliği, nereden geldiği ve Vaelor'u neden tanıdığı tam olarak açıklanmaz.
- Oyuncuyu büyülü bir yöntemle hayata döndürür.
- Oyuncuya **Kaderbozan Zırhı** verir.
- Kısa ve tek bir konuşmadan sonra bölgeden ayrılır; görev veren sürekli bir NPC'ye dönüşmez.

Süvarinin temel cümlesi kısa tutulabilir:

> “Seni öldüren güç bedenine değil, kaderine vurdu. Bu zırh aynı darbeyi ikinci kez kabul etmez.”

#### Kaderbozan Zırhı

- Final oyun aseti ve gerçek ekipman sınıfı olacaktır; geçici konsept değildir.
- Bütün ırk ve beden tiplerinde kullanılabilir.
- Final savaşı sırasında mevcut göğüs zırhının üstüne hikâye katmanı olarak uygulanır; oyuncunun önceki ekipman seçimini silmez.
- `Kaderin Hükmü` saldırısını etkisizleştirir.
- Oyuncuya bir kez ölümden dönme veya kritik hasarı engelleme özelliği verir.
- Normal düşmanlara karşı oyunu bozacak kadar yüksek ham AC sağlamaz; asıl gücü Vaelor'un kader saldırısına karşı bağışıklıktır.
- Finalden sonra envanterde efsanevi görev ödülü olarak kalabilir.

#### İkinci boss karşılaşması

- Oyuncu kuleye kısa, düşmansız bir dönüş rotasıyla yeniden girer; daha önce temizlenen katlar tekrar doldurulmaz.
- Vaelor aynı scripted saldırıyı kullanmaya çalışır fakat Kaderbozan Zırhı darbeyi kırar.
- Bundan sonra gerçek, kazanılabilir boss savaşı başlar.
- Oyuncu bu ikinci safhada normal saldırı, büyü, eşya ve d20 sistemini kullanır.
- Oyuncu bu aşamada ölürse normal ölüm ekranı ve son autosave'den devam etme sistemi çalışır.
- Vaelor sıfır cana ulaştığında hemen öldürülmez; final kararı ekranı açılır.

### Finaller

#### Final 1 — Eski Yolları Geri Getir

Mercek onarılır ve bütün yollar eski hâline döner. Kaybolan insanlar hatırlanır. Eski siyasi çatışmaların geri döneceği ima edilir.

#### Final 2 — Yolları Halklara Paylaştır

Her bölge kendi Yol Taşı'nı yönetir. En olumlu finaldir. En az iki bölgeyle yüksek güven ve uzlaşmacı taş kararları gerektirir.

#### Final 3 — Kader Merceği'ni Yok Et

Büyülü yol ağı çöker. Dünya özgürleşir fakat yolculuk ve ticaret zorlaşır. Oyuncu hatırladığı kişiyi kurtarabilir ama bazı silinen anılar geri gelmez.

#### Final 4 — Yeni Gözcü Ol

Oyuncu kuleyi ve Mercek'i ele geçirir. Sınıf, amaç ve geçmişe göre koruyucu, hükümdar veya yozlaşmış Gözcü epilogu gösterilir.

Final seçimi şu sonuçları birleştirir:

- Hatırlanan kişinin kurtarılıp kurtarılmadığı.
- Üç Yol Taşı kararı.
- Yerleşim güveni.
- Vaelor'un öldürülmesi, ikna edilmesi veya hapsedilmesi.

Kalpaklı Süvari epilog hesaplamasına bağlanmaz. Son sahnelerde uzaktan görülebilir veya tamamen kaybolabilir; gizemi korunur.

## 5. Teknik uygulama

### Mevcut durum

- `StoryModels.cs` içinde temel `StoryDefinition`, `StoryNode`, `Choice` ve `QuestState` vardır; kullanılmamaktadır.
- `StorySelectionScreen` ilk hikâyeyi seçer fakat giriş sahnesi göstermeden oyunu başlatır.
- `JournalScreen` üç sabit metin gösterir.
- `GameSession.Flags` ve kayıt sistemi hikâye bayraklarını zaten saklar.
- `MapScreen` sabit hikâye tetikleyicisi kullanmaz; Göz Kulesi başlangıçtan zorunlu savaş bölgesidir.

### Basit kalıtım yapısı

Kod, ödev amacına uygun biçimde aşağıdaki sade hiyerarşiyi kullanacaktır:

```text
StoryEvent
├── DialogueStoryEvent
├── ExplorationStoryEvent
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
├── RouteConsequence
├── ReputationConsequence
└── EndingConsequence
```

Her sınıf az sayıda okunabilir özellik ve bir `Apply`/`CanTrigger` metodu taşıyacaktır. Derin ve karmaşık generic/DI/event-bus yapıları kullanılmayacaktır.

### Yeni veya değişecek temel dosyalar

- `Core/Story/StoryEvent.cs`: hikâye olaylarının tabanı ve alt sınıfları.
- `Core/Story/Quest.cs`: görev tabanı, ana görev ve Yol Taşı görevleri.
- `Core/Story/StoryConsequence.cs`: sonuç alt sınıfları.
- `Core/Story/StoryCatalog.cs`: bütün bölüm, düğüm ve final tanımları.
- `Core/Story/StoryProgress.cs`: aktif bölüm, tamamlanan taşlar ve final uygunluğu.
- `Game/Screens/StorySceneScreen.cs`: resim, anlatım, seçenek ve d20 akışı.
- `Game/Screens/EndingScreen.cs`: dinamik final ve epilog ekranı.
- `Core/Items/LegendaryArmor.cs`: Kaderbozan Zırhı ve Vaelor saldırısına özel koruma.
- `JournalScreen.cs`: sabit diziler yerine gerçek görev listesini gösterir.
- `StorySelectionScreen.cs`: ilk hikâyeyi `Kader Yolları` adıyla gösterir ve prologu açar.
- `MapScreen.cs`: koordinat ve bayrak tabanlı, bir kez çalışan hikâye tetikleyicileri.
- `HexMap.cs`: Göz Kulesi başlangıçta kilitli; üç taş tamamlanınca açılır.
- `GameSession.cs`: `StoryProgress` ve görev güncelleme metotları.
- `SaveModels.cs`: kayıt sürümü 3; görev ilerlemesi eklenir. Sürüm 1 ve 2 kayıtları açılmaya devam eder.

### Önerilen görev durumları

- `NotStarted`
- `Active`
- `Completed`
- `Failed`

Bir ana görev başarısız olduğunda oyun kilitlenmez. Alternatif hedef açılır. Örneğin arşiv görevlisi ölürse bilgi göz işaretli düşman subayından veya anı levhasından alınır.

### Hikâye koordinatları

Kesin koordinatlar, harita editöründeki bütün biyom engelleri tamamlandıktan sonra son kez doğrulanacaktır. Başlangıç taslağı:

- Yeşilgeçit: `(0,0)`
- Kızılburç Kalesi: `(2,-1)`
- Kök Taşı: bataklık merkezine yakın sabit hex
- Gök Taşı: `(4,-3)` çevresi
- Güneş Taşı: `(3,3)` çevresi
- Göz Kulesi: `(-3,-5)`

## 6. Görsel ve ara sahne planı

Tam video ara sahne önerilmez. Oyunun estetiğine uygun yöntem:

- 800 x 800 piksel-art sabit sahne.
- Alt bölümde ahşap/parşömen metin kutusu.
- Hafif kararma, geçiş ve sınırlı kamera kaydırması.
- Önemli seçimlerde mevcut d20 animasyonu.

### Yeniden kullanılabilecek mevcut görseller

- Kalpaklı Süvari encounter görseli.
- Göz Kulesi dış görünüşü.
- Köy, kasaba, kale ve tapınak sahneleri.
- Mor biyom rota sahneleri.
- Göz mührü, kule anahtarı, dünya haritası parçası ve kalıntı eşya görselleri.
- Mevcut zafer ekranı çerçevesi.

### Üretimine başlanan final animasyonlar

- `tower-fall-v1`: Vaelor'un kader darbesi ve oyuncunun kuleden düşüşü — 6 adet 800x800 kare tamamlandı.
- `rider-revival-v1`: Kalpaklı Süvari'nin gelişi, diriltme ve zırh sunumu — 4 adet 800x800 kare tamamlandı.
- `fatebreaker-return-v1`: Zırhın aynı kader saldırısını kırması — 4 adet 800x800 kare tamamlandı.
- Ana karakter bütün karelerde saf siyah, özelliksiz silüet olarak tutuldu.

### Üretilmesi gereken zorunlu yeni görseller

1. Parçalanmış kervan ve ilk karşılaşma.
2. Kara Harita Parçası yakın plan görev görseli/ikonu.
3. Kök Taşı ve batmış tapınak ana sahnesi.
4. Gök Taşı ve zirve manastırı ana sahnesi.
5. Güneş Taşı ve çöl gözlemevi ana sahnesi.
6. Baş Haritacı Vaelor portresi.
7. Vaelor harita tokenı ve savaş sprite'ı.
8. Kader Merceği kule içi sahnesi.
9. Hatırlanan kişi/anı yankısı için kimliği belirsiz portre varyantı.
10. Vaelor'un tek vuruşluk `Kaderin Hükmü` saldırı efekti.
11. Oyuncunun kuleden düşüşünü gösteren ara sahne.
12. Kulenin dibinde Kalpaklı Süvari'nin oyuncuyu dirilttiği ara sahne.
13. Kaderbozan Zırhı envanter ikonu, yere bırakılmış eşya görseli ve karakter ekipman katmanları.
14. Zırhın Vaelor'un saldırısını kırdığı ikinci karşılaşma sahnesi/efekti.
15. Dört final için dört epilog illüstrasyonu.

### İsteğe bağlı görseller

- Unutulan asker devriyesi.
- Mor bölge mültecileri.
- Üç Yol Taşı'nın etkinleşme animasyon kareleri.
- Kuleye girişte 3 karelik kısa parallax geçişi.

Öncelik yeni görsel üretmek değil, önce hikâye motorunu mevcut görsellerle uçtan uca çalıştırmaktır. Yeni görseller sonra placeholder olmadan final asset olarak eklenir.

## 7. Uygulama sırası

### Aşama 1 — Hikâye omurgası

- Hikâye/quest sınıfları.
- Kayıt sürümü 3 ve eski kayıt uyumluluğu.
- Dinamik günlük.
- Hikâye bayrak testleri.

Kabul ölçütü: Görev başlar, ilerler, kaydedilir, yüklenir ve günlükte doğru görünür.

### Aşama 2 — Prolog

- StorySceneScreen.
- Karakter geçmişinden hatırlanan kişi üretimi.
- Kervan enkazı ve ilk üç seçim.
- Kara Harita Parçası.

Kabul ölçütü: Yeni oyun karakter seçiminden sonra prologu oynar ve Yeşilgeçit'te doğru bayraklarla başlar.

### Aşama 3 — Birinci bölüm ve üç taşın açılması

- Yeşilgeçit, kervan ve Kızılburç görevleri.
- Sabit hikâye koordinatları.
- Göz Kulesi kilidi.

Kabul ölçütü: Oyuncu kuleye erken gidemez; üç taş görevini günlüğünde görür.

### Aşama 4 — Üç Yol Taşı

- Önce Kök Taşı uçtan uca.
- Sonra Gök Taşı.
- Sonra Güneş Taşı.
- Her görevde savaş, beceri ve sosyal çözüm.

Kabul ölçütü: Görevlerin sırası serbesttir ve her çözüm farklı bayrak/ödül üretir.

### Aşama 5 — Mor bölge ve kule

- Sabit mor bölge olayları.
- Göz Piskoposu.
- Kule katları.
- Hatırlanan kişi.
- Vaelor ilk karşılaşması ve scripted yenilgi.
- Kuleden düşüş, Kalpaklı Süvari revive sahnesi ve Kaderbozan Zırhı.
- Vaelor'a dönüş ve gerçek boss savaşı.

Kabul ölçütü: Oyuncu üç taş olmadan kuleye giremez; ilk yenilgi yalnız bir kez çalışır; normal ölüm ekranını açmaz; zırh verildikten sonra ikinci savaş gerçekten kazanılabilir.

### Aşama 6 — Dört final

- Final uygunluk hesaplama.
- EndingScreen.
- Karar bayraklarından epilog oluşturma.
- Ana menüye dönüş ve tamamlanmış kayıt işareti.

Kabul ölçütü: Dört final de test senaryolarıyla ulaşılabilir; en iyi final önceki uzlaşma kararlarını gerektirir.

### Aşama 7 — Görsel üretim ve son QA

- Önce Vaelor, Kader Merceği ve prolog.
- Sonra üç Yol Taşı.
- Son olarak dört epilog.
- 800 x 800 okunabilirlik.
- Bütün seçim yolları, kayıt/yükleme ve ölüm testi.

## 8. Test planı

- Geçmiş seçimi doğru hatırlanan kişi tipini üretir.
- Her ana görev durumu kaydedilip yüklenir.
- Hikâye olayı aynı kayıtta ikinci kez tetiklenmez.
- Üç taş farklı sırada tamamlanabilir.
- Kule üç taş öncesinde kilitlidir.
- Her Yol Taşı'nın en az üç çözümü çalışır.
- Sınıf seçeneği olmayan karakter ana hikâyede kilitlenmez.
- İlk Vaelor savaşı oyuncunun canı, AC'si veya sınıfından bağımsız olarak scripted yenilgiyle biter.
- Scripted yenilgi normal ölüm sayılmaz ve ölüm ekranını açmaz.
- Kuleden düşüşten sonra oyuncu doğru canla dirilir ve Kaderbozan Zırhı'nı alır.
- Kaderbozan Zırhı mevcut ekipmanı silmez ve bütün ırk/bedenlerde çalışır.
- İlk yenilgi kaydet/yükle sonrasında tekrar tekrar tetiklenmez.
- İkinci Vaelor savaşı kazanılabilir; gerçek yenilgi normal ölüm akışını çalıştırır.
- Dört finalin gereksinimleri doğrudur.
- Eski sürüm 1/2 kayıtları açılır.
- Autosave hikâye seçimi ve sonuçtan sonra çalışır.
- Hikâye tamamlanana kadar rastgele d20 sonucu kalıcı çıkmaz yaratmaz.

## 9. Plan sonunda ortaya çıkacak içerik

- 1 tamamlanmış ana hikâye.
- 1 prolog.
- 4 ana bölüm.
- 3 serbest sıralı Yol Taşı görevi.
- 8-12 sabit hikâye encounter'ı.
- 3-5 zorunlu veya isteğe bağlı boss karşılaşması.
- 4 final.
- Geçmiş, sınıf ve istatistiklere bağlı tekrar oynanabilir diyalog seçenekleri.
- Dinamik günlük ve kayıtlı hikâye ilerlemesi.

