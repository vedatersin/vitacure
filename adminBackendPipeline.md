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
- [x] Showcase CRUD servisleri ve entity omurgasi baslatildi
- [x] Showcase ikon, 7 urun slotu ve popup secim omurgasi eklendi
- [x] Order listeleme modulu acildi
- [x] Admin CRUD sonrasi output cache invalidation eklendi
- [x] Cache gozlemlenebilirligi dashboard'a baglandi
- [x] Admin bildirim entity, service ve migration omurgasi eklendi
- [x] Read/unread state, liste, detay ve sayac endpoint'leri gercek veriye baglandi
- [x] Siparis, favori, sepet, uye kaydi, e-posta dogrulama ve sifre sifirlama akislarindan bildirim uretimi eklendi

## Devam Eden ve Acik Isler

### Yetki ve operator akislari

- [ ] User create/reset/role assignment islemleri
- [ ] 2FA hazirligi
- [ ] Admin profil ve hesap ayarlari backend omurgasi

### Katalog ve icerik

- [ ] Category delete ve guclu validasyon
- [ ] Showcase delete/archive ve preview akislarina gec
- [ ] Product delete, gorsel yukleme ve guclu validasyon
- [ ] Tag delete
- [ ] Admin tarafi toplu islemler

### Bildirim backend'i

- [x] Admin bildirim entity yapisi
- [x] Read/unread state
- [x] Liste, detay ve sayac endpoint'leri
- [x] Yeni siparis, favori, sepete ekleme, yeni uye kaydi gibi eventlerden bildirim uretimi

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

1. User role assignment ve reset akislari
2. Audit trail ihtiyacinin netlestirilmesi
3. Admin CRUD aksiyonlarini da notification feed ve audit trail'e baglamak
