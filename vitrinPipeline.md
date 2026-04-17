# VITRIN PIPELINE

Bu dosya son vitrin kapsamlarini tek yerde toplamak icin tutulur.
Amaç: kota veya pencere degisiminde son konusulan vitrin islerinin kaybolmamasini engellemek.

## Tamamlanan Son Isler

- [x] Vitrin slug mimarisi ayrildi ve storefront slug resolver'a baglandi
- [x] Varsayilan 6 vitrin icin uygun arkaplanlar `wwwroot/img/*Bg.png` icinden eslesti
- [x] Home'da 6 vitrinlik giris bandi eklendi
- [x] Vitrin detail sayfasinin ilk SSR versiyonu acildi
- [x] Admin vitrin formuna ikon duzenleme alani eklendi
- [x] Admin vitrin formuna 7 urunluk slot yapisi eklendi
- [x] Slot bazli popup urun secimi eklendi
- [x] Slotlar icin surukle-birak siralama eklendi
- [x] `ShowOnHome` ve `IsActive` uzerinden yayin kontrolu korunuyor

## Bu Turdaki Veri Alanlari

### Vitrin temel alanlari

- `Name`
- `Slug`
- `IconClass`
- `Title`
- `Description`
- `TagsContent`
- `BackgroundImageUrl`
- `SeoTitle`
- `MetaDescription`
- `ShowOnHome`
- `IsActive`
- `SortOrder`

### Vitrin urun sahnesi

- 7 slotluk one cikan urun listesi
- Slot sirasi storefront sahnesindeki onceligi temsil eder
- 1 numarali slot ayni zamanda vitrindeki merkez/ilk urun mantigina hizmet eder
- Urunler popup pencereden secilir
- Popup listesi secili kategori havuzundaki urunleri oncelikli gosterir
- Slotlar surukle-birak ile yer degistirir

## Son Istenenler Icinde Hala Acik Olanlar

### Storefront

- [ ] Vitrin detay sayfasinda kategori + alt kategori filtrelerini daha hiyerarsik hale getir
- [ ] Vitrin hero aciklama alanini gorsel 1'deki layouta daha da yaklastir
- [ ] Vitrin sahnesinde 7 urun slotunun tam kurallariyla admin sirasini birebir esle
- [ ] Vitrin detail sayfasina preview CTA ve ek vitrin etiket stili ekle

### Admin

- [ ] Popup urun secicide secili urunleri ayri rozet olarak goster
- [ ] Slottan urun kaldirma butonu ekle
- [ ] Vitrin listesinde arkaplan thumbnail + ikon + title hiyerarsisini daha belirgin yap
- [ ] Vitrinler icin preview butonu ekle
- [ ] Varsayilan ikon seti secici ekle

### Backend

- [ ] Vitrin urun havuzunda alt kategorileri otomatik dahil etme kuralini netlestir
- [ ] Vitrin bazli SEO ve canonical kontrolunu genislet
- [ ] Vitrin silme/arsivleme akislarini yaz

## Referans Kural

- Vitrin modulu kategori modulu ile karismayacak
- Kategori taksonomi ve SEO siniflandirmasini tasir
- Vitrin landing ve sahne kurgusunu tasir
- Home yuzeyinde ilk gorulen 6 giris alani kategori degil, vitrin olarak devam edecek
