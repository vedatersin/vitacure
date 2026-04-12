# VITACURE PIPELINE

Bu dosya proje için yaşayan yol haritasıdır.
Yeni bir thread acildiginda once bu dosya okunur.
Amac: ne yapildi, ne sirada, hangi teknik kurallar gecerli, hangi moduller sonraki contextte ele alinacak net olsun.

## Calisma Kurallari

- Kod degisiklikleri UTF-8 BOM ile kaydedilecek.
- Turkce karakterlerde encoding bozulmasina izin verilmeyecek.
- UI ve mevcut Razor gorunumu korunacak.
- SSR ana yaklasim olmaya devam edecek.
- Her faz sonrasinda `dotnet build` ve uygun yerde `dotnet test` calistirilacak.
- Yeni moduller eklenirken test kapsami birlikte buyutulecek.
- Musteri hesaplari ile backoffice hesaplari ayri tutulacak.
- Bu dosya her buyuk adim sonunda guncellenecek.

## Genel Durum

- Proje: ASP.NET Core MVC SSR storefront
- Veritabani: MSSQL `localhost\\MSSQL2022` / `VitacureDB`
- Auth: ASP.NET Core Identity
- Test projesi: `vitacure.Tests`
- Mevcut hedef: moduler, test edilebilir, okunabilir, temiz ilerleyen e-ticaret omurgasi

## Faz 0 - Temizlik ve Zemin

### Tamamlananlar

- [x] Gereksiz kodlar ayiklandi
- [x] Kullanilmayan logger ve alanlar temizlendi
- [x] Kullanilmayan script importlari kaldirildi
- [x] Proje yapisi dokumani olusturuldu
- [x] UTF-8 BOM hassasiyeti calisma kurali olarak benimsendi

### Notlar

- Ilk temizlikte UI bozulmadan sadece güvenli sadeleştirme yapildi.

## Faz 1 - Veri Katmani ve EF Core Omurgasi

### Tamamlananlar

- [x] EF Core SQL Server paketleri eklendi
- [x] `AppDbContext` kuruldu
- [x] Design-time `AppDbContextFactory` eklendi
- [x] Connection string MSSQL 2022 icin tanimlandi
- [x] Ilk migration olusturuldu
- [x] `VitacureDB` veritabanina baglanti saglandi
- [x] Temel entityler eklendi

### Tamamlanan Entityler

- [x] `Category`
- [x] `Product`
- [x] `Tag`
- [x] `ProductTag`
- [x] `AppUser`
- [x] `AppRole`
- [x] `CustomerFavorite`
- [x] `CustomerAddress`

### Notlar

- Veritabani artik storefront, auth ve account genislemeleri icin yeterli taban yapiya sahip.

## Faz 2 - Seed ve Mock'tan Gercek Veriye Gecis

### Tamamlananlar

- [x] `docs/mock-data.json` seed mantigina tasindi
- [x] `AppDbSeeder` yazildi
- [x] Seed ile kategori ve urun verileri DB'ye tasindi
- [x] Mock veriden kontrollu gecis stratejisi kuruldu

### Durum

- [x] Kategori verileri DB'den geliyor
- [x] Urun verileri DB'den geliyor
- [ ] Dekoratif UI metinlerinin tamaminin mock bagimliligindan temizlenmesi

### Notlar

- Bazi chat/filter/banner metinleri halen kontrollu sekilde mock dokumandan geliyor.

## Faz 3 - Storefront Servislesme ve Routing

### Tamamlananlar

- [x] `IStorefrontContentService` olusturuldu
- [x] `StorefrontContentService` yazildi
- [x] `HomeController` DB destekli servis kullaniyor
- [x] `CategoryController` DB destekli servis kullaniyor
- [x] Dinamik kategori slug routing'e gecildi
- [x] Regex tabanli kategori route bagimliligi kaldirildi
- [x] `/urun/{slug}` urun detay route'u eklendi
- [x] `ProductController` eklendi
- [x] Urun detay SSR sayfasi acildi

### Storefront Tamamlananlar

- [x] Ana sayfa DB tabanli icerik uretiyor
- [x] Kategori sayfasi DB tabanli icerik uretiyor
- [x] Urun detay sayfasi DB tabanli icerik uretiyor
- [x] Kategori tag filtre query akisi eklendi
- [x] Urun detay sayfasinda tag gosterimi eklendi

### Eksikler

- [ ] Chat/filter/banner iceriklerinin tamamen gercek veri modeline tasinmasi
- [ ] Storefront filtrelerinin tam dinamik veri tabanli hale gelmesi
- [x] Sepet davranisinin kalici hale gelmesi

## Faz 4 - Test Omurgasi

### Tamamlananlar

- [x] Ayrik test projesi olusturuldu
- [x] Ana proje icinden test klasoru compile disina alindi
- [x] InMemory EF tabanli servis testleri yazildi
- [x] Storefront servis testleri yazildi
- [x] Admin servis testleri yazildi
- [x] Customer account servis testleri yazildi

### Mevcut Test Alanlari

- [x] `ProductService`
- [x] `CategoryService`
- [x] `StorefrontContentService`
- [x] `AdminDashboardService`
- [x] `AccountAccessService`
- [x] `AdminUserService`
- [x] `AdminCategoryService`
- [x] `AdminProductService`
- [x] `AdminTagService`
- [x] `CustomerAccountService`

### Kurallar

- Her yeni servis veya kritik akista test eklenmeli.
- Build ve test tercihen tekil calistirilmali.
- Paralel build/test bazen dosya kilidi olusturuyor; bu bilinen teknik durum.

## Faz 5 - Authentication ve Hesap Ayrimi

### Tamamlananlar

- [x] ASP.NET Core Identity entegre edildi
- [x] `AppUser` / `AppRole` ile custom user-role yapisi kuruldu
- [x] Cookie auth etkinlestirildi
- [x] Musteri ve backoffice hesap tipi ayrildi
- [x] Role seed mekanizmasi yazildi
- [x] Varsayilan admin hesabi seed edildi

### Roller

- [x] `Admin`
- [x] `Editor`
- [x] `Customer`

### Akislar

- [x] `/login`
- [x] `/register`
- [x] `/account`
- [x] `/admin/login`
- [x] `/admin/dashboard`

### Eksikler

- [ ] Sifre sifirlama akisi
- [ ] E-posta dogrulama
- [ ] Rate limiting
- [ ] 2FA hazirligi

## Faz 6 - Admin Paneli

### Tamamlanan Moduller

- [x] Dashboard
- [x] Users listeleme
- [x] Categories listeleme
- [x] Categories create/edit
- [x] Products listeleme
- [x] Products create/edit
- [x] Tags listeleme
- [x] Tags create/edit
- [x] Product-tag baglantisi
- [x] Orders listeleme

### Eksikler

- [ ] User create/reset/role assignment islemleri
- [ ] Category delete / daha guclu validasyon
- [ ] Product delete / gorsel yukleme / daha guclu validasyon
- [ ] Tag delete
- [ ] Admin tarafi toplu islemler
- [ ] Admin cache invalidation akislari

## Faz 7 - Musteri Hesabi ve Favoriler

### Tamamlananlar

- [x] Kalici favori tabloları eklendi
- [x] Kalici adres tabloları eklendi
- [x] Kalici sepet tablosu ve servis katmani eklendi
- [x] `CustomerAccountService` yazildi
- [x] Favori toggle endpoint'i eklendi
- [x] Account dashboard modeli yazildi
- [x] `Hesabim` sayfasi favorilerle genislletildi
- [x] `Hesabim` sayfasi adreslerle genislletildi
- [x] `Sepetim` sayfasi acildi
- [x] Urun karti kalp butonlari gercek endpoint'e baglandi
- [x] Urun karti ve urun detay sepet butonlari gercek endpoint'e baglandi
- [x] Header sepet sayaci dinamik hale getirildi
- [x] Girissiz favori denemesinde login redirect akisi eklendi
- [x] Girissiz sepete ekleme denemesinde login redirect akisi eklendi
- [x] Varsayilan veya ilk kayitli adresle siparis olusturma akisi eklendi
- [x] Hesabim sayfasina siparis gecmisi eklendi
- [x] Canli musteri kaydi + favori + adres akisi dogrulandi

### Eksikler

- [ ] Favori sayacinin header'da dinamik gosterimi
- [ ] Adres duzenleme / silme
- [ ] Musteri profil guncelleme
- [x] Siparis gecmisi
- [x] Kalici sepet

## Faz 8 - Performans ve Cache

### Tamamlananlar

- [x] Output cache altyapisi eklendi
- [x] Home icin cache policy eklendi
- [x] Category icin slug ve tag varyasyonlu cache policy eklendi
- [x] Product detail icin cache policy eklendi
- [x] Redis connection string hazirligi eklendi
- [x] Redis baglantisi verilirse devreye girecek cache kaydi hazirlandi

### Eksikler

- [ ] Gercek Redis instance ile canli baglanti testi
- [ ] Admin CRUD sonrasi ilgili output cache temizleme stratejisi
- [ ] Cache invalidation servis katmani
- [ ] Cache hit/miss gozlemlenebilirligi

## Faz 9 - Hemen Siradaki Isler

### Bir Sonraki Oncelik

- [x] Sepeti kalici hale getirmek
- [x] Siparis entity ve servis omurgasini kurmak
- [x] Musteri siparis gecmisi ekranini acmak
- [x] Admin order listeleme modulu
- [ ] Sifre sifirlama akisi

### Onerilen Sira

1. Password reset
2. Redis canli baglanti
3. Cache invalidation

## Faz 10 - Teknik Borclar ve Dikkat Noktalari

- [ ] Mock tabanli dekoratif iceriklerin tam tasfiyesi
- [ ] Bazi dosyalarda gecmisten gelen bozuk karakterlerin sistematik taranmasi
- [ ] `bin/obj/.vs` gibi build artiklarinin repo hijyeni gozden gecirilmesi
- [ ] `OutputCache` invalidation tasarimi admin CRUD ile baglanmali
- [ ] Frontend JS davranislarinda test kapsami yok, ileride dusunulebilir

## Thread Acildiginda Ne Yapilacak

Yeni bir thread acildiginda su sirayla ilerlenir:

1. Bu `pipeline.md` dosyasini oku
2. `Faz 9 - Hemen Siradaki Isler` bolumunu kontrol et
3. Son tamamlanan fazi ve eksikleri baz al
4. Yeni is yapildiginda ilgili kutucuklari guncelle
5. Dosyayi UTF-8 BOM ile kaydet
6. Build/test sonucu bu dosyadaki ilgili faz notuna yansit

## Son Guncelleme Ozeti

Bu dosya guncellendiginde sistem su durumda:

- MSSQL baglantisi aktif
- Identity aktif
- Admin panel temel CRUD omurgasi aktif
- Musteri account, favori ve adres akisi aktif
- Storefront SSR urun/kategori/urun detay akislari aktif
- Output cache aktif
- Redis hazirligi var, canli baglanti henuz yok
- Kalici sepet, sepet sayfasi ve header sepet sayaci aktif
- Siparis entityleri, siparis olusturma akisi, musteri siparis gecmisi ve admin siparis listeleme aktif
- Test suite aktif ve buyuyor
