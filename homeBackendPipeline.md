# VITACURE HOME BACKEND PIPELINE

Bu dosya storefront ve musteri deneyiminin backend omurgasini takip eder.
Kapsam: home content, category/product render verileri, musteri auth, hesabim, adresler, favoriler, sepet ve siparis akislarinin backend'i.

## Kapsam

- Home ve storefront content servisleri
- Category ve product detail veri akislari
- Musteri login/register/logout
- E-posta dogrulama ve sifre sifirlama
- Hesabim dashboard backend'i
- Favori, adres, sepet ve siparis backend'i
- Guest session merge akislari
- Home content modeli ve storefront cache politikasi

## Tamamlananlar

### Storefront veri akislari

- [x] `IStorefrontContentService` olusturuldu
- [x] `StorefrontContentService` yazildi
- [x] Home, category ve product detail SSR veri akislari kuruldu
- [x] Kategori tag query akisi eklendi
- [x] Product detail related product akisi eklendi

### Musteri auth

- [x] Musteri login/register/logout akislari
- [x] E-posta dogrulama akisi
- [x] Sifre sifirlama akisi
- [x] Auth rate limiting
- [x] Guest session merge akisi

### Musteri hesap islemleri

- [x] Hesabim dashboard backend modeli
- [x] Profil guncelleme
- [x] Adres ekleme, guncelleme, silme
- [x] Favori toggle backend'i
- [x] Siparis gecmisi akisi
- [x] Kalici sepet ve siparis olusturma akisi

### Performans

- [x] Home output cache policy
- [x] Category cache policy
- [x] Product detail cache policy
- [x] Redis baglanti hazirligi

## Mevcut Aciklar

### Home content backend'i

- [ ] `docs/mock-data.json` bagimliligini azaltacak `HomeContent` benzeri bir model tasarla
- [ ] Hero copy, CTA, trust bloklari ve banner hedeflerini veri modeli ile temsil et
- [ ] Popular supplement alanini kategori/tag landing mantigina bagla
- [ ] Home section secim kurallarini servis seviyesine tasi
- [ ] Home content degisince cache invalidation tetikle

### Musteri hesap deneyimi

- [ ] Hesabim ekranini storefront ile daha sikı butunlestirecek backend sozlesmesi
- [ ] Section bazli navigation state'i view model seviyesinde daha acik temsil etme
- [ ] Siparis detay sayfasi veya siparis satiri drill-down backend'i
- [ ] Adres dogrulama/il-ilce veri kaynagi entegrasyonu
- [ ] Account ve home ortak telemetry event adlari

### Auth ve guvenlik

- [ ] 2FA hazirligi
- [ ] Musteri tarafi kritik aksiyonlar icin ek gozlemlenebilirlik

## UI Ile Ortak Kararlar

- Hesabim, login ve musteri islem alanlari storefront ust menu ve marka diliyle uyumlu kalacak.
- Hesabim ekrani ayrik admin panel hissi vermeyecek.
- Sidebar/tab navigation kullanilabilir ama storefront header ve logo korunacak.
- Home UI tarafinda yapilacak CTA ve hero gelistirmeleri bu dosyadaki content modeliyle uyumlu olacak.

## Bir Sonraki Sira

1. Home content modeli
2. Hesabim backend sozlesmesini sidebar/tab yapisina gore netlestirme
3. Siparis detay ve account telemetry
4. 2FA hazirligi
