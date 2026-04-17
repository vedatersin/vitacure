# VITACURE SHOWCASE BACKEND PIPELINE

Bu dosya vitrin modulunun backend omurgasini takip eder.
Kapsam: vitrin entity yapisi, admin CRUD, kategori havuzu, one cikan urunler, detay sayfasi veri modeli ve home gorunurluk kurallari.

## Modul Rolü

- Kategori modulu taksonomi ve SEO yapisini tasir.
- Vitrin modulu ise storefront landing deneyimini tasir.
- Bir vitrin, bir veya daha fazla kategori ve alt kategoriye baglanabilir.
- Vitrin, one cikan urun seti ve genis urun havuzu ile ayri bir landing sayfasi olarak davranir.

## Tamamlananlar

- [x] `Showcase` entity yapisi eklendi
- [x] `ShowcaseCategory` iliski tablosu eklendi
- [x] `ShowcaseFeaturedProduct` iliski tablosu eklendi
- [x] `Showcase.IconClass` alani eklendi
- [x] Admin vitrin CRUD servisi olusturuldu
- [x] Vitrin create/edit/list controller omurgasi eklendi
- [x] `ShowOnHome` ve `IsActive` bayraklari eklendi
- [x] Ilk 6 vitrin icin default seed mantigi yazildi
- [x] Arkaplan gorselleri `wwwroot/img` icinden slug bazli eslesti
- [x] Varsayilan 6 vitrin icin ayri slug mimarisi acildi
- [x] Storefront slug resolver vitrinleri de cozer hale geldi
- [x] Vitrin detay veri akisinin ilk SSR omurgasi eklendi

## Acik Isler

### Vitrin detay backend'i

- [x] `IStorefrontContentService` icine vitrin detay veri akisi eklendi
- [x] Vitrin detay sayfasi icin ayri view model tasarlandi
- [x] Sol filtre alaninin ilk kategori havuzu versiyonu eklendi
- [ ] Alt kategori filtrelerini ana kategori secimine gore hiyerarsiklestir
- [ ] Urun havuzu icin dedupe ve sira kurallarini guclendir
- [x] Vitrin one cikan urunleri icin 7 slot/fallback kurali eklendi

### Admin yonetimi

- [x] Featured product siralama deneyimi checkbox'tan slot mantigina cekildi
- [x] 7 urunluk slot modeli eklendi
- [x] Slot secimi icin popup urun havuzu ilk versiyonu eklendi
- [x] Slotlarda surukle-birak siralama ilk versiyonu eklendi
- [ ] Arkaplan secimi icin media picker veya secimli galeri ekle
- [x] Ikon sinifi ve vitrin basligi admin duzenleme alanina eklendi
- [ ] Vitrin silme veya arsivleme akislarini ekle
- [ ] Vitrin bazli preview aksiyonu ekle

### Home entegrasyonu

- [x] Home'da kategori odakli giris yerine vitrin odakli giris modeli baslatildi
- [x] `ShowOnHome` aktif vitrinler home servisinde donduruluyor
- [ ] Home vitrin section secim/siralama kurallarini netlestir

## Bir Sonraki Sira

1. Storefront vitrin detail veri modelini ekle
2. Home servisine home'da gosterilecek vitrin listesini bagla
3. Vitrin urun havuzu filtre mantigini yaz
4. Featured product siralama deneyimini admin tarafinda guclendir
