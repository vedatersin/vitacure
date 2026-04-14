# HOME UI PIPELINE

Bu dosya `home` yuzeyinin tasarim, UX, veri bagimliligi ve teknik borclarini takip eder.
Amac: ana sayfanin sadece gorunur olmasi degil, backend ile uyumlu, yonetilebilir ve olceklenebilir hale gelmesi.

## Mevcut Durum

- `Views/Home/Index.cshtml` sayfayi partial bazli kuruyor.
- `HomeController` yalnizca `IStorefrontContentService` uzerinden model aliyor.
- `StorefrontContentService` ana sayfa verisini hem DB hem `docs/mock-data.json` kaynaklarindan derliyor.
- Hero/chat, section basliklari, popular supplement kartlari ve campaign banner alanlari halen dekoratif mock bagimliligina sahip.
- Featured ve opportunity alanlari gercek `Product` kayitlarindan geliyor.
- Home UI tek sayfa hissi veriyor ancak kullanici niyetini olcme, dinamik segmentasyon ve admin yonetimi acisindan zayif.

## Tamamlananlar

- [x] Home sayfasi partial parcalara ayrildi
- [x] `HomeController` servis tabanli hale geldi
- [x] Featured ve opportunity urunleri DB destekli hale geldi
- [x] Home output cache policy aktif edildi
- [x] Home CTA metinlerindeki bozuk encoding duzeltildi
- [x] Hesabim ekrani storefront hissine yaklasacak sidebar entegrasyonu baslatildi

## UI Tarafinda Acik Eksikler

- [ ] Hero alaninda trust sinyalleri yok
- [ ] Kullaniciya gore kisitli veya login durumuna gore farkli home akisi yok
- [ ] Bos durum/fallback tasarimlari tanimli degil
- [ ] Popular supplements kartlari tiklanabilir kategori ya da filtre aksiyonuna bagli degil
- [ ] Campaign banner alaninda admin yonetilebilir hedef URL modeli yok
- [ ] Home section CTA'lari kategori, listing ya da kampanya landing'lerine inmeden ayni sayfaya donuyor
- [ ] Analytics/telemetry hook'lari yok
- [ ] SEO icin statik title/meta disinda zenginlestirilmis schema veya landing metni yok
- [ ] Accessibility kontrol listesi tamam degil

## Home UI Icin Yapilabilecekler

### 1. Hero'yu karar destek alanina cevir

- Chat widget altina 3 guven karti eklenmeli: eczaci destekli, ayni gun kargo, guvenli odeme gibi.
- Hero icinde kategori seciminin yanina hedef bazli giris eklenmeli: enerji, uyku, bagisiklik, odak.
- Chat'e girmeden once kullaniciya hizli aksiyon butonlari sunulmali.

### 2. Section'lari niyet bazli kurgula

- `FeaturedProducts` sadece rating ile degil editor secimi, yeni gelen, cok satan gibi segmentlerle desteklenmeli.
- `OpportunityProducts` salt indirim oranina gore degil stok, kampanya bitis tarihi ve kategori cesitliligi ile secilmeli.
- `PopularSupplements` dekoratif kart yerine kategori veya tag landing girisi olmali.

### 3. Donusum alanlarini guclendir

- Tumunu Gor CTA'lari gercek route'a baglanmali.
- Banner alanlarina hedef URL, kampanya etiketi ve opsiyonel tarih siniri eklenmeli.
- Home uzerinden sepete ekleme ve favori aksiyonlarinin gorunur basari geri bildirimi standartlastirilmali.

### 4. Yonetilebilirlik ekle

- Home'daki section title, CTA label, banner target, hero copy ve trust bloklari admin panelinden yonetilebilir olmali.
- Mock JSON sadece gelistirme fallback'i olarak kalmali.

### 5. Hesap alanini storefront ile birlestir

- Login, register ve hesabim akislari storefront layout diliyle ayni ailede kalmali.
- Hesabim sayfasinda ust menu/logo korunup iceride account sidebar veya tab sistemi kullanilmali.
- Siparisler, adresler ve favoriler storefront kart/spacing sistemine daha yakin bir dile cekilmeli.

## Backend Ile Baglanti

Home UI su an bu veri katmanlarina bagli:

- `Categories`: DB
- `FeaturedProducts`: DB
- `OpportunityProducts`: DB
- `PopularSupplements`: mock
- `CampaignBanners`: mock
- `SectionTitles`: mock agirlikli
- `ChatWidget` global/category copy: mock

Bu nedenle UI tarafinda yapilacak hemen her iyilestirme backend tarafinda yeni bir content modeli gerektiriyor.

## Ortaklasilmasi Gereken Isler

### UI + Backend birlikte

- [ ] Home icin ayri bir content/config modeli tasarla
- [ ] Banner hedef URL ve sira bilgisini veri modeline ekle
- [ ] Section bazli urun secim kuralini veri tarafina tasi
- [ ] Hero copy ve trust bloklarini admin yonetimine ac
- [ ] Analytics event adlarini UI ve backend/log tarafinda standardize et

### UI oncelikli

- [ ] Hero trust strip
- [ ] CTA route iyilestirmeleri
- [ ] Responsive spacing ve section hiyerarsisi revizyonu
- [ ] Bos state/fallback komponentleri

### Backend oncelikli

- [ ] `HomeContent` benzeri yonetilebilir aggregate tasarimi
- [ ] Mock JSON bagimliligini parcali azaltma plani
- [ ] Home section karar mekanizmalarini servis seviyesine tasima
- [ ] Cache invalidation'i home content degisimine baglama

## Onerilen Siradaki Sprint

1. Home content modeli ve admin yonetim sinirlarini tanimla
2. Hero copy, banner ve CTA verilerini mock'tan cikar
3. Featured/opportunity secim kurallarini netlestir
4. UI tarafinda gercek route ve bos state duzenini uygula
5. Test ve cache invalidation kapsamini buyut

## Son Inceleme Ozeti

- Ana sayfa teknik olarak calisiyor fakat content-driven storefront seviyesine gelmemis durumda.
- UI guclu bir iskelete sahip ama veri sozlesmesi zayif oldugu icin yonetilebilirlik dusuk.
- Home'un bir sonraki asamasi yeni komponent eklemekten cok, mevcut alanlari gercek veri ve admin akislarina baglamak olmali.
