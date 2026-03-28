# Mock Data Inventory

## A. Genel Özet

- Bu envanter, projedeki mock/demo/hardcoded verileri kodu değiştirmeden ayıklamak için hazırlandı.
- Ana veri yoğunluğu açık ara [Index.cshtml](/Users/vedat/Documents/GitHub/vitacure/Views/Home/Index.cshtml) içindedir.
- İkinci yoğun kaynak [\_Layout.cshtml](/Users/vedat/Documents/GitHub/vitacure/Views/Shared/_Layout.cshtml) dosyasıdır.
- Veriler üç ana tipe ayrılıyor:
  - global UI metinleri
  - kategori/chat/prompt verileri
  - ürün/kampanya/banner mock verileri
- Scaffold metinler de koruma amacıyla envantere dahil edildi:
  - [Privacy.cshtml](/Users/vedat/Documents/GitHub/vitacure/Views/Home/Privacy.cshtml)
  - [Error.cshtml](/Users/vedat/Documents/GitHub/vitacure/Views/Shared/Error.cshtml)

## B. Veri Kaynakları

| Kaynak | Veri Türleri | Not |
| --- | --- | --- |
| `/Views/Home/Index.cshtml` | kategori adları, section başlıkları, ürün kartları, slider içerikleri, placeholder havuzları, filtreler, tag/chip metinleri, CTA metinleri | birincil mock veri kaynağı |
| `/Views/Shared/_Layout.cshtml` | navbar/footer metinleri, kurumsal linkler, global kategori tekrarları | global UI metin kaynağı |
| `/Views/Home/Privacy.cshtml` | scaffold privacy metni | demo/scaffold |
| `/Views/Shared/Error.cshtml` | scaffold error metni | demo/scaffold |
| `/PROJECT_ANALYSIS_REPORT.md` | kaynak referans ve doğrulama | yardımcı kaynak |

## C. Global Metinler

| Metin | Tip | Kaynak | Kullanım | Tekrar | Etiketler |
| --- | --- | --- | --- | --- | --- |
| VitaCure | brand | `/Views/Shared/_Layout.cshtml:21` | logo alt ve footer | evet | `global`, `seo-candidate`, `critical-preserve` |
| Biz Kimiz? | nav-label | `/Views/Shared/_Layout.cshtml:30`, `:97` | navbar + footer hızlı link | evet | `global`, `duplicated`, `seo-candidate` |
| Tüm Ürünler | nav-label | `/Views/Shared/_Layout.cshtml:33`, `/Views/Home/Index.cshtml:1302`, `:2016` | navbar + CTA | evet | `global`, `duplicated`, `seo-candidate` |
| Kategoriler | heading/nav-label | `/Views/Shared/_Layout.cshtml:36`, `:83` | navbar + footer heading | evet | `global`, `duplicated` |
| Markalar | nav-label | `/Views/Shared/_Layout.cshtml:39`, `:99` | navbar + footer link | evet | `global`, `duplicated` |
| Vitacure AI | nav-label | `/Views/Shared/_Layout.cshtml:42`, `:100`, `/Views/Home/Index.cshtml:1973` | navbar + footer + banner hedefi | evet | `global`, `duplicated`, `seo-candidate` |
| yeni | badge | `/Views/Shared/_Layout.cshtml:44` | navbar badge | hayır | `ui-label` |
| Favoriler | icon label | `/Views/Shared/_Layout.cshtml:50` | navbar icon title | hayır | `ui-label` |
| Sepetim | icon label | `/Views/Shared/_Layout.cshtml:54` | navbar cart title | hayır | `ui-label` |
| Giriş Yap | CTA | `/Views/Shared/_Layout.cshtml:61` | navbar auth CTA | hayır | `ui-label`, `admin-manageable` |
| Footer marka paragrafı | body copy | `/Views/Shared/_Layout.cshtml:77-79` | footer açıklaması | hayır | `seo-candidate`, `admin-manageable` |
| Mesafeli Satış Sözleşmesi | footer-link | `/Views/Shared/_Layout.cshtml:107` | kurumsal link | hayır | `seo-candidate`, `admin-manageable` |
| Gizlilik ve Güvenlik Politikası | footer-link | `/Views/Shared/_Layout.cshtml:108` | kurumsal link | hayır | `seo-candidate`, `admin-manageable` |
| İade ve İptal Koşulları | footer-link | `/Views/Shared/_Layout.cshtml:109` | kurumsal link | hayır | `seo-candidate`, `admin-manageable` |
| KVKK Aydınlatma Metni | footer-link | `/Views/Shared/_Layout.cshtml:110` | kurumsal link | hayır | `seo-candidate`, `admin-manageable` |
| İletişim | footer-link | `/Views/Shared/_Layout.cshtml:111` | kurumsal link | hayır | `seo-candidate`, `admin-manageable` |
| © 2026 VitaCure. Tüm Hakları Saklıdır. | footer-copy | `/Views/Shared/_Layout.cshtml:117` | footer alt satır | hayır | `global`, `critical-preserve` |

## D. Kategori Verileri

| Kategori | Slug Adayı | Kaynak | Sayfa | Tekrar | Admin? | SEO? |
| --- | --- | --- | --- | --- | --- | --- |
| Uyku Sağlığı | `uyku-sagligi` | `/Views/Home/Index.cshtml:1223`, `:1612-1613` | home | evet | evet | evet |
| Multivitamin & Enerji | `multivitamin-enerji` | `/Views/Home/Index.cshtml:1227` | home | evet | evet | evet |
| Zihin & Hafıza Güçlendirme | `zihin-hafiza-guclendirme` | `/Views/Home/Index.cshtml:1231` | home | evet | evet | evet |
| Hastalıklara Karşı Koruma | `hastaliklara-karsi-koruma` | `/Views/Home/Index.cshtml:1235` | home | evet | evet | evet |
| Kas ve İskelet Sağlığı | `kas-ve-iskelet-sagligi` | `/Views/Home/Index.cshtml:1239` | home | evet | evet | evet |
| Zayıflama Desteği | `zayiflama-destegi` | `/Views/Home/Index.cshtml:1243` | home | evet | evet | evet |

Ek kategori verileri:
- Uyku özel açıklaması: `/Views/Home/Index.cshtml:1613`
- Uyku tag/chip metinleri: `/Views/Home/Index.cshtml:1617-1620`
- Chat içi categoryNames array: `/Views/Home/Index.cshtml:2323-2326`

## E. Örnek Soru Havuzları

Kaynak: [Index.cshtml](/Users/vedat/Documents/GitHub/vitacure/Views/Home/Index.cshtml): `2385-2422`

| Kategori | Soru Sayısı | Durum |
| --- | --- | --- |
| Uyku Sağlığı | 4 | kesin |
| Multivitamin ve Enerji | 4 | kesin |
| Zihin ve Hafıza Güçlendirme | 4 | kesin |
| Hastalıklara Karşı Koruma | 4 | kesin |
| Kas ve İskelet Sağlığı | 4 | kesin |
| Zayıflama Desteği | 4 | kesin |
| unassigned | 0 | kesin |

Tüm sorular `docs/mock-data.json` içinde kategori bazlı saklandı; veri kaybı olmaması için tek tek korunmuştur.

## F. Ürün Mock Verileri

Ana ürün kaynakları:
- Featured slider: `/Views/Home/Index.cshtml:1308-1586`
- Uyku coverflow object array: `/Views/Home/Index.cshtml:1671-1678`
- Fırsat ürünleri array: `/Views/Home/Index.cshtml:2021-2031`

Özet:
- Featured ürün sayısı: `10`
- Coverflow ürün objesi: `7`
- Fırsat ürünü: `10`
- Duplicated product identity: `Daily Multivitamin`, `Omega 3`, `Vitamin D3`, `Magnezyum`, `C Vitamini Complex`, `B12 Vitamini Sprey`

Her ürün için şu alanlar JSON’a çıkarıldı:
- `name`
- `price`
- `oldPrice`
- `rating`
- `image`
- `usageSections`
- `categoryRelation`
- `description` varsa
- `duplicate`

## G. Kampanya ve Banner Verileri

Kaynaklar:
- Popüler Takviyeler data array: `/Views/Home/Index.cshtml:1948-1957`
- Kampanya banner dosya listesi: `/Views/Home/Index.cshtml:1988-1996`
- Vitacure AI banner: `/Views/Home/Index.cshtml:1972-1973`
- Uyku banner: `/Views/Home/Index.cshtml:1602`

Tespit:
- Popüler takviye kartı: `9`
- Kampanya banner dosyası: `8`
- Özel banner: `2`

## H. Chat Widget Metinleri

Kaynaklar:
- Hero/chat markup: `/Views/Home/Index.cshtml:1208-1288`
- Placeholder değişimleri: `/Views/Home/Index.cshtml:2336`, `2354`, `2377`, `2381`, `2510`, `2523`
- Dynamic aria-label kalıpları: `/Views/Home/Index.cshtml:1752`, `1758`, `2127`

Çıkarılan başlıca alanlar:
- hero title
- hero subtitle
- compact back label
- compact category label
- search filter label
- main placeholder
- search placeholder
- search-with-category placeholder
- fullscreen title
- add file title
- chat/search mode label
- file menu labels
- dynamic aria kalıpları

## I. Filtre ve UI Seçenekleri

Kaynak: `/Views/Home/Index.cshtml:1831-1905`

- Filtre başlığı: `Filtrele`
- Temizle linki: `Temizle`
- Marka seçenekleri:
  - Solgar 4
  - Now Foods 3
  - Ocean 2
  - Dynavit 1
- Form seçenekleri:
  - Kapsül 6
  - Tablet 4
  - Damla 2
- Sonuç sayısı etiketi:
  - `12 ürün bulundu`
- Sort etiketi:
  - `Sırala:`
- Sort seçenekleri:
  - `Önerilen`
  - `En Düşük Fiyat`
  - `En Yüksek Fiyat`

## J. Tekrar Eden Veriler

- Kategori adları hem hero içinde hem footer’da tekrar ediyor
- `Tüm Ürünler` hem navbar’da hem CTA’larda kullanılıyor
- `Vitacure AI` hem navbar/footer linki hem banner hedefinde geçiyor
- Bazı ürünler featured + coverflow veya coverflow + fırsat ürünleri arasında tekrar ediyor
- `Favorilere Ekle` ve `Sepete Ekle` çok sayıda kartta tekrarlı

## K. Admin’den Yönetilmesi Gereken Veriler

- kategori adları
- kategori açıklamaları
- örnek soru havuzları
- tüm ürün isim/fiyat/görsel/rating alanları
- popüler takviye kartları
- kampanya banner dosyaları
- footer kurumsal linkleri
- footer marka açıklaması
- chat widget CTA/placeholder metinleri
- filtre seçenekleri
- section başlıkları

## L. SEO İçin Ayrıştırılması Gereken Veriler

- `Bir Dünya Sağlık`
- `Peki senin ihtiyacin ne?`
- `Öne Çıkan Ürünler`
- `Popüler Takviyeler`
- `Kampanyalar`
- `Fırsat Ürünleri`
- `Uyku Sağlığı`
- footer marka paragrafı
- uyku kategori açıklama paragrafı
- kurumsal link başlıkları

## M. Dosya Bazlı Veri Haritası

- `/Views/Home/Index.cshtml`
  - hero copy
  - chat widget labels
  - kategori etiketleri
  - featured ürün slider verisi
  - uyku kategori metinleri
  - coverflow ürün objeleri
  - filtre seçenekleri
  - popüler takviye mock verisi
  - kampanya banner dosya adları
  - fırsat ürünü mock verisi
  - categoryNames ve placeholderCategories array’leri
- `/Views/Shared/_Layout.cshtml`
  - navbar metinleri
  - footer metinleri
  - kurumsal linkler
  - tekrar eden kategori listesi
- `/Views/Home/Privacy.cshtml`
  - scaffold privacy title/body
- `/Views/Shared/Error.cshtml`
  - scaffold error title/body
- `/PROJECT_ANALYSIS_REPORT.md`
  - doğrulama ve yardımcı referans
