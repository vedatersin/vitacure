# VITACURE ADMIN UI PIPELINE

Bu dosya sadece admin arayuzunun tasarimi, bilgi mimarisi, moduller arasi UI butunlugu ve sonraki UI kararlarini takip etmek icin tutulur.
Backend isleri ve sistemsel altyapi notlari `adminBackendPipeline.md` dosyasina islenir.

## Tasarim Ilkeleri

- Admin arayuzu storefront'tan bagimsiz dusunulecek.
- Bugun path tabanli (`/yonetim`, `/admin/...`), ileride `admin.site.com` gibi subdomain'e tasinabilir olacak.
- Sidebar klasik moduler backoffice omurgasi olacak.
- Sidebar ana rengi mevcut storefront footer tonuna yakin olacak.
- Topbar, sidebar'dan ayrisan acik bir yuzey olarak kullanilacak.
- Tekrarlayan bilgi bloklari azaltilacak.
- Liste ekranlari sade, okunabilir, klasik backoffice mantiginda ilerleyecek.
- Filtre alanlari tabloya yakin konumlanacak.
- Bildirim ve profil aksiyonlari sag ustte toplanacak.

## Tamamlananlar

- [x] Admin area storefront layout'tan ayrildi
- [x] Ayrik admin layout ve auth layout olusturuldu
- [x] `/yonetim` kisayolu admin girisine baglandi
- [x] Sidebar + topbar + kart tabanli admin shell kuruldu
- [x] Dashboard kart bazli moduler yapiya gecirildi
- [x] Liste ekranlarina ortak breadcrumb eklendi
- [x] Liste ekranlarina ortak ikonlu baslik karti eklendi
- [x] Liste ekranlarina tablo ustu filtre bar tasarimi eklendi
- [x] Create/edit ekranlarina breadcrumb ve ikonlu ust kart eklendi
- [x] Sol ust alana sade logo ve `YONETIM PANELI` markasi yerlestirildi
- [x] Topbar beyaz yuzeye cekildi
- [x] Topbar baslik alani sadeleştirildi, sag tarafa kisa manşet tasindi
- [x] Sag ustte profil alani ikon + hesap adi + yetki + cikis aksiyonlariyla dropdown yapisina yaklastirildi
- [x] Sag ustte bildirim zili + badge + acilir onizleme paneli eklendi
- [x] Sidebar altindaki tekrar eden yonetici karti kaldirildi
- [x] Kategori ekraninda ozetler hero alaninin sagina tasindi
- [x] Kategori ekraninda filtre bar tablo ustu toolbar mantigina yaklastirildi
- [x] Ana liste modulleri ortak hero + tablo ustu toolbar diline cekildi
- [x] Create/edit form ekranlarinda hero sag paneliyle ortak ust kurgu olusturuldu
- [x] Bildirimler modulu icin iki kolonlu liste + detay UI omurgasi kuruldu
- [x] Bildirimler modulu kategori bazli filtre yapisiyla UI seviyesinde hazirlandi
- [x] Bildirim dropdown ve bildirim merkezi gercek backend feed'i ile baglandi
- [x] Vitrinler modulu icin liste ve form UI omurgasi eklendi
- [x] Vitrin formunda ikon preview, popup urun secimi ve surukle-birak slot yapisi eklendi

## Guncel UI Kararlari

### Sol Ust Marka

- Cerceveli ekstra kutu olmadan sade logo kullanilacak.
- Logonun altinda buyuk karakterle `YONETIM PANELI` ifadesi yer alacak.
- Marka alani kurumsal admin kimligi gibi davranacak.

### Topbar

- Sayfa basligi topbar'da tekrar etmeyecek.
- Sag ustte iki ana alan olacak:
  - Bildirim ikonu + sayac
  - Profil acilir paneli
- Sol tarafta sayfayi tekrar etmeyen kisa bir manşet bilgi kullanilabilir.

### Profil Paneli

- Profil ikonu
- Hesap adi
- Yetki bilgisi (`Admin`, `Editor` vb.)
- Acilir menude:
  - Profil
  - Cikis

### Bildirim Paneli

- Bell ikonu yaninda okunmamis bildirim sayaci
- Acilir panelde son bildirimler listelenecek
- Altta `Tum Bildirimleri Gor` aksiyonu olacak
- Bildirime tiklaninca bildirim modulu veya detay sayfasina gidilecek

### Liste Ekranlari

- Breadcrumb ustte kalacak
- Ikonlu modul baslik karti korunacak
- Onemli sayisal ozetler hero alaninin saginda veya tablo ustu bolgede toplanacak
- Filtreleme, arama ve yeni kayit ekleme butonlari tabloya yakin tek satirlik bir toolbar olarak calisacak
- Tablo stili gorsel referanslardaki gibi daha kurumsal ve daha sade hale getirilecek
- Renkli etiketler ve fazla dekoratif ozetler azaltilacak

## Acik Isler

### Oncelik 1

- [x] Bildirim dropdown icindeki oge tiplerini ve aksiyon ikonlarini gercek senaryolara gore zenginlestirmek
- [ ] Vitrin formundaki arkaplan secimini gorsellestirmek
- [ ] Profil dropdown'ina hesap ayarlari veya profil sayfasi girisi eklemek
- [ ] Form ekranlarindaki operator kartlarini yeni ust kurguya gore daha da sadeleştirmek
- [x] Bildirim detay paneline aksiyon gecmisi ve okunma durumu bloklari eklemek

### Oncelik 2

- [ ] Tablo satir yogunlugunu ve spacing'i gorsel 3'e daha da yaklastirmak
- [ ] Durum badge tasarimini daha sade kurumsal tona cekmek
- [ ] Modul bazli sayfa ust kartlari icin ortak spacing sistemi oturtmak
- [ ] Sidebar aktif ve alt modul durumlari icin acilir grup mantigi eklemek
- [ ] Mobil admin shell davranisini ayri optimize etmek

### Oncelik 3

- [ ] Admin profil sayfasi ve hesap ayarlari ekrani tasarlamak
- [ ] Bildirim merkezi sayfasini gorsel 5 mantigina yaklastirmak
- [ ] Koyu veya acik tema ihtiyaci olup olmadigini netlestirmek

## Bildirim UI Senaryolari

Admin bildirim paneline dusmesi beklenen aksiyonlar:

- [x] Yeni siparis olusturuldu
- [x] Sepete urun eklendi
- [x] Urun favorilendi
- [x] Sisteme yeni uye kaydi oldu
- [x] E-posta dogrulama tamamlandi
- [x] Parola sifirlama talebi olustu
- [ ] Admin CRUD sonrasi kritik katalog degisikligi yapildi

Her biri icin planlanan UI alanlari:

- ikon
- kisa baslik
- kaynak kullanici veya sistem bilgisi
- zaman
- okunma durumu
- detay linki

## Backend Bagimliliklari

Bu UI islerinin backend bagimliliklari `adminBackendPipeline.md` dosyasina da not dusulmelidir:

- Gercek admin bildirim entity ve tablosu
- Bildirim uretim servisleri
- Kullanici aksiyonlarindan event ve bildirim akisi
- Okundu ve okunmadi durumu
- Bildirim detay sayfasi ve listeleme endpoint'leri
- Admin subdomain gecisinde cookie, domain ve auth ayarlari

## Bir Sonraki UI Sira

1. Sidebar alt moduller icin acilir grup mantigi
2. Profil ayarlari ekranini tasarlama
3. Vitrin formundaki arkaplan secimini gorsellestirme
4. Admin CRUD aksiyonlarini da bildirim feed'ine dusurme

## Notlar

- Gorsel referanslarda begenilen kisimlar:
  - klasik sidebar moduler yapi
  - tablonun net okunmasi
  - filtre toolbar'in tabloya yakin olmasi
  - profil ve bildirim aksiyonlarinin sag ustte toplanmasi

- Guncel tasarimdan zayif bulunan kisimlar:
  - tekrar eden bilgiler
  - filtre ve ozet alanlarinin tabloyla yeterince butunlesmemesi
  - fazla kartlasma nedeniyle dikkatin dagilmasi
