# VITACURE ADMIN BACKEND PIPELINE

Bu dosya sadece admin tarafinin backend omurgasini takip eder.
Kapsam: admin auth, dashboard verileri, katalog CRUD, bildirim backend'i, cache invalidation ve ileride subdomain gecisi.

## Kapsam

- Admin girisi ve yetkilendirme
- Admin dashboard veri kaynaklari
- Category/Product/Tag CRUD servisleri
- Admin order listeleme ve operasyonlari
- Admin bildirim backend'i
- Admin kaynakli cache invalidation

## Tamamlananlar

- [x] Admin auth akisi ayrildi
- [x] Admin area controller omurgasi kuruldu
- [x] Dashboard veri servisleri yazildi
- [x] Category CRUD servisleri yazildi
- [x] Product CRUD servisleri yazildi
- [x] Tag CRUD servisleri yazildi
- [x] Order listeleme modulu acildi
- [x] Admin CRUD sonrasi output cache invalidation eklendi
- [x] Cache gozlemlenebilirligi dashboard'a baglandi

## Devam Eden ve Acik Isler

### Yetki ve operator akislari

- [ ] User create/reset/role assignment islemleri
- [ ] 2FA hazirligi
- [ ] Admin profil ve hesap ayarlari backend omurgasi

### Katalog ve icerik

- [ ] Category delete ve guclu validasyon
- [ ] Product delete, gorsel yukleme ve guclu validasyon
- [ ] Tag delete
- [ ] Admin tarafi toplu islemler

### Bildirim backend'i

- [ ] Admin bildirim entity yapisi
- [ ] Read/unread state
- [ ] Liste, detay ve sayac endpoint'leri
- [ ] Yeni siparis, favori, sepete ekleme, yeni uye kaydi gibi eventlerden bildirim uretimi

### Altyapi

- [ ] Admin subdomain gecisi icin cookie/domain/auth plani
- [ ] Kritik admin aksiyonlari icin audit trail

## Home Tarafindan Ayrilan Konular

Bu dosyada artik su konular tutulmaz:

- Musteri login/register/forgot-password/reset-password
- Musteri hesabim ekrani
- Adres yonetimi
- Favori akisi
- Sepet ve siparis gecmisi
- Home content modeli

Bu basliklar `homeBackendPipeline.md` icinde takip edilir.

## Bir Sonraki Sira

1. Admin notification backend omurgasi
2. User role assignment ve reset akislari
3. Audit trail ihtiyacinin netlestirilmesi
