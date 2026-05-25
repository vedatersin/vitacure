# Ürün Kataloğu Yeniden Tasarım ve Entegrasyon Planı

## Amaç

Bu doküman, admin panelindeki ürün modülünü paylaşılan referans görsellere mümkün olduğunca yakın bir deneyime taşımak için hazırlanmıştır.

Hedef:

- Ürün liste ekranını kart ve tablo görünümü ile yeniden tasarlamak
- Sütun göster/gizle, içe/dışa aktarma ve ürün ekleme başlangıç modallarını sisteme entegre etmek
- Ürün detay ekranını basamaklı, sekmeli ve daha ölçeklenebilir bir editör yapısına taşımak
- Mevcut backend’in desteklediği alanları doğrudan kullanmak
- Eksik backend alanları için net bir faz planı hazırlamak

Bu plan, görsellerin birebir kopyası olmak yerine, mevcut VitaCure kod tabanına uyarlanmış ve gerçekçi bir uygulama yol haritasıdır. Tasarım dili referans görsellere çok yakın tutulacaktır.

## Referans Ekranlar

Bu plan aşağıdaki referanslara göre hazırlanmıştır:

- `Görsel 1`: ürün kart görünümü
- `Görsel 2`: ürün liste/tablo görünümü
- `Görsel 3`: sütun göster/gizle menüsü
- `Görsel 4`: dışa aktarma ilk adımı
- `Görsel 5`: içe/dışa aktarma ikinci adımı
- `Görsel 6`: ürün eklemeden önce ürün tipi seçimi
- `Görsel 7-10`: sekmeli ürün detay/edit yapısı

## Mevcut Sistem Gerçekliği

Şu an ürün modülü ağırlıklı olarak aşağıdaki dosyalar üzerinden çalışıyor:

- Liste:
  [Areas/Admin/Views/Products/Index.cshtml](C:/Users/vedat/Documents/GitHub/vitacure/Areas/Admin/Views/Products/Index.cshtml)
  [Areas/Admin/Views/Products/_ListContent.cshtml](C:/Users/vedat/Documents/GitHub/vitacure/Areas/Admin/Views/Products/_ListContent.cshtml)
- Form:
  [Areas/Admin/Views/Products/Create.cshtml](C:/Users/vedat/Documents/GitHub/vitacure/Areas/Admin/Views/Products/Create.cshtml)
  [Areas/Admin/Views/Products/Edit.cshtml](C:/Users/vedat/Documents/GitHub/vitacure/Areas/Admin/Views/Products/Edit.cshtml)
  [Areas/Admin/Views/Products/_Form.cshtml](C:/Users/vedat/Documents/GitHub/vitacure/Areas/Admin/Views/Products/_Form.cshtml)
- Controller:
  [Areas/Admin/Controllers/ProductsController.cs](C:/Users/vedat/Documents/GitHub/vitacure/Areas/Admin/Controllers/ProductsController.cs)
- Service:
  [Infrastructure/Services/AdminProductService.cs](C:/Users/vedat/Documents/GitHub/vitacure/Infrastructure/Services/AdminProductService.cs)
- ViewModel:
  [Models/ViewModels/Admin/ProductFormViewModel.cs](C:/Users/vedat/Documents/GitHub/vitacure/Models/ViewModels/Admin/ProductFormViewModel.cs)
  [Models/ViewModels/Admin/ProductListViewModel.cs](C:/Users/vedat/Documents/GitHub/vitacure/Models/ViewModels/Admin/ProductListViewModel.cs)
- JS:
  [wwwroot/js/pages/admin-product-form.js](C:/Users/vedat/Documents/GitHub/vitacure/wwwroot/js/pages/admin-product-form.js)
  [wwwroot/js/pages/admin-product-gallery.js](C:/Users/vedat/Documents/GitHub/vitacure/wwwroot/js/pages/admin-product-gallery.js)
  [wwwroot/js/pages/admin-product-variants.js](C:/Users/vedat/Documents/GitHub/vitacure/wwwroot/js/pages/admin-product-variants.js)
- Stil:
  [wwwroot/css/pages/admin.css](C:/Users/vedat/Documents/GitHub/vitacure/wwwroot/css/pages/admin.css)

Veri modeli tarafında bugün gerçekten elimizde olan temel ürün yapısı:

- `Product`: ad, slug, açıklama, fiyat, eski fiyat, rating, ana görsel, stok, ana kategori, marka, aktiflik
- `ProductVariant`: varyant ekseni, seçenek, SKU, fiyat, eski fiyat, stok, sıra, aktiflik
- `ProductMedia`: medya URL, primary flag, sıralama, media asset ilişkisi
- `ProductCategory`: ek kategori ilişkisi
- `ProductFeature`: özellik ve değer ilişkisi
- `ProductCollection`: koleksiyon ilişkisi

İlgili entity dosyaları:

- [Domain/Entities/Product.cs](C:/Users/vedat/Documents/GitHub/vitacure/Domain/Entities/Product.cs)
- [Domain/Entities/ProductVariant.cs](C:/Users/vedat/Documents/GitHub/vitacure/Domain/Entities/ProductVariant.cs)
- [Domain/Entities/ProductMedia.cs](C:/Users/vedat/Documents/GitHub/vitacure/Domain/Entities/ProductMedia.cs)
- [Domain/Entities/ProductCategory.cs](C:/Users/vedat/Documents/GitHub/vitacure/Domain/Entities/ProductCategory.cs)
- [Domain/Entities/ProductFeature.cs](C:/Users/vedat/Documents/GitHub/vitacure/Domain/Entities/ProductFeature.cs)
- [Domain/Entities/ProductCollection.cs](C:/Users/vedat/Documents/GitHub/vitacure/Domain/Entities/ProductCollection.cs)

## Ana Tespitler

### UI tarafındaki ana açıklar

- Liste ekranı veri yoğun ama yönetim ergonomisi düşük
- Kart ve tablo görünümü arasında geçiş yok
- Sütun göster/gizle altyapısı yok
- İçe/dışa aktarma akışı yok
- Ürün ekleme başlangıç modalı yok
- Ürün detay ekranı sekmeli değil, tek uzun form
- Form içinde bilgi mimarisi çok yoğun
- Liste ekranında toplu seçim davranışı yok
- Tablo kolonları referans görsellerdeki ticari dil ile eşleşmiyor

### Backend tarafındaki ana açıklar

- Liste görünümü için referans görsellerdeki tüm kolonlar desteklenmiyor
- Alış fiyatı, barkod, tedarikçi, ürün tipi, satış kanalı, Google ürün kategorisi, metadata başlık/açıklama, birim fiyat, desi, HS kodu, stok lokasyonu gibi alanlar modelde yok
- İçe/dışa aktarma servisleri yok
- Sütun görünürlük tercihlerini saklayan kullanıcı bazlı ayar yapısı yok
- Ürün tipi seçiminin backend karşılığı yok
- Paket ürün modeli yok
- Tekil ürün ile varyantlı ürün arasında ilk yaratım adımında ayrı akış yok

## Tasarım Hedefi

Ürün modülü 3 ana yüzeye ayrılacak:

1. Ürün listeleme alanı
2. Ürün ekleme başlangıç alanı
3. Ürün detay/edit alanı

Bu alanlar tek bir görsel dil kullanacak:

- açık arka plan
- yumuşak gri yüzeyler
- mor/violet aksiyon rengi
- sade ikonografi
- geniş boşluk kullanımı
- tablo ve kart görünümünde aynı veri çekirdeği

## Hedef Bilgi Mimarisi

### 1. Ürün Liste Ekranı

Referans:
`Görsel 1`, `Görsel 2`, `Görsel 3`, `Görsel 4`, `Görsel 5`, `Görsel 6`

Üst aksiyon alanı:

- Başlık: `Ürünler`
- Bilgi ikonu
- `Dışa Aktar`
- `İçe Aktar`
- `Ürün Ekle`

Araç çubuğu:

- arama input
- filtre butonu
- sıralama butonu veya kontrolü
- sütun ayarı butonu
- tablo görünümü butonu
- kart görünümü butonu

Liste görünüm modları:

- `Grid/Card View`
- `Table/List View`

### 2. Ürün Ekle Başlangıç Modalı

Referans:
`Görsel 6`

Seçenekler:

- `Basit Ürün`
- `Varyantlı Ürün`
- `Paket Ürün`

Bu seçim create akışını yönetecek.

### 3. Ürün Detay/Edit Alanı

Referans:
`Görsel 7`, `Görsel 8`, `Görsel 9`, `Görsel 10`

Sekmeler:

- `Temel Bilgi`
- `Medya`
- `Ürün Detayı`
- `Envanter`
- `SEO`
- `Özel Alanlar`
- `Ürün Özelleştirmesi`
- `Varyant`

Not:
Bugünkü backend ile birebir her sekmenin tüm alanını doldurmak zorunda değiliz. Öncelik, mevcut sistemi bozmadan sekmeli editör iskeletini kurmaktır.

## Fazlara Bölünmüş Uygulama Planı

## Faz 1 - Liste Ekranı Yenileme

Amaç:
Mevcut ürün listesini görsel referansa yakın hale getirmek.

### Faz 1A - Liste üst barı

Eklenecekler:

- arama alanı yeniden tasarlanacak
- `Filtre` butonu eklenecek
- `Dışa Aktar`, `İçe Aktar`, `Ürün Ekle` aksiyonları üst sağ alana taşınacak
- görünüm geçiş butonları eklenecek

Backend gereksinimi:

- yok
- mevcut `Index` action kullanılabilir

### Faz 1B - Card/Grid görünümü

Eklenecekler:

- ürün kartları
- ürün görseli
- ürün adı
- satış fiyatı
- varsa eski fiyat
- stok özeti
- basit aksiyon alanı

Not:

- Görseldeki dünya ikonu benzeri küçük aksiyon, ilk fazda placeholder olabilir
- kart görünümde sadece desteklenen alanlar gösterilecek

Kullanılacak veri:

- `ImageUrl`
- `Name`
- `Price`
- `OldPrice`
- `Stock`
- `IsActive`
- `BrandName`
- `CategoryName`

Gerekli ViewModel genişlemesi:

- `OldPrice`
- `DisplayPrice`
- `DisplayOldPrice`
- `HasDiscount`
- `PrimaryBadge`
- `StatusLabel`

### Faz 1C - Table/List görünümü

Referans kolonları:

- Ürün
- Satış Fiyatı
- Alış Fiyatı
- Envanter
- Satış Kanalları

Bugünkü sistemde desteklenen gerçek kolon karşılıkları:

- `Ürün`
- `Satış Fiyatı`
- `Envanter`

İlk faz için önerilen tablo kolonları:

- seçim kutusu
- ürün
- satış fiyatı
- eski fiyat
- stok
- varyant
- marka
- kategori
- durum
- aksiyon

Sonraki backend fazında eklenebilir:

- alış fiyatı
- satış kanalları
- barkod
- oluşturulma tarihi
- güncellenme tarihi

### Faz 1D - Sütun göster/gizle menüsü

Referans:
`Görsel 3`

İlk faz davranışı:

- sadece UI tarafında çalışır
- tablo kolonları `data-column-key` ile işaretlenir
- kullanıcı görünür kolonları local storage’da saklanır

İkinci faz davranışı:

- kullanıcı bazlı kalıcı tercihler backend’e yazılır

İlk fazda desteklenecek kolon anahtarları:

- `product`
- `price`
- `old-price`
- `stock`
- `variant`
- `brand`
- `category`
- `status`
- `feature-count`
- `tag-count`

### Faz 1E - Filtre paneli

İlk fazda:

- durum
- stok
- marka
- kategori
- varyantlı / varyantsız

Bugün backend’de kolay desteklenebilecekler:

- durum
- stok
- marka
- kategori
- varyant sayısı

Not:
Şu an controller içindeki filtre mantığı sınırlı:
[Areas/Admin/Controllers/ProductsController.cs](C:/Users/vedat/Documents/GitHub/vitacure/Areas/Admin/Controllers/ProductsController.cs)

Bu kısım service tarafına taşınmalı.

## Faz 2 - İçe / Dışa Aktarma Akışı

Referans:
`Görsel 4`, `Görsel 5`

## Faz 2A - Modal step 1

İki akış olacak:

- `Dışa Aktar`
- `İçe Aktar`

İlk adım:

- dosya tipi seçimi
- kapsam seçimi

Desteklenecek tipler:

- `csv`
- `xlsx`

İlk teslimde gerçekçi seçenekler:

- `Ürünler`
- `Varyantlar`
- `Ürün Özellikleri`

### Faz 2B - Modal step 2

Referans mantığı korunacak:

- ikinci ekranda alan seçimi listesi
- “Tüm seçimleri kaldır”
- onay butonu

İlk fazda backend tarafından gerçekten dışa aktarılabilecek alanlar:

- ad
- açıklama
- slug
- satış fiyatı
- indirimli fiyat
- marka
- kategori
- ek kategoriler
- etiketler
- stok
- görsel URL
- aktiflik
- varyantlar

Backend eklendikten sonra açılacak alanlar:

- alış fiyatı
- barkod
- SKU ana ürün seviyesi
- metadata başlık/açıklama
- ürün tipi
- tedarikçi
- Google ürün kategori ID
- desi
- HS kodu
- satış kanalı
- minimum / maksimum sepet adedi
- stok bitince satışa devam et

### Faz 2C - Teknik yaklaşım

İlk teslim için:

- UI modalları tamamlanır
- export gerçek çalışır
- import UI placeholder veya sınırlı CSV template import olarak açılır

Öneri:

- Export önce gerçekten çalışsın
- Import ikinci adımda validasyonlu olarak eklensin

Gerekli yeni backend bileşenleri:

- `IAdminProductImportExportService`
- ürün export request modeli
- ürün import preview modeli
- CSV/XLSX üretim helper’ı

## Faz 3 - Ürün Ekle Başlangıç Modalı

Referans:
`Görsel 6`

Amaç:

- create akışını daha anlaşılır hale getirmek
- ürün tipine göre editör davranışını ön hazırlıkla başlatmak

İlk faz davranışı:

- `Basit Ürün` seçilirse create ekranı simple mod ile açılır
- `Varyantlı Ürün` seçilirse create ekranı variant sekmesi açık başlar ve en az bir varyant satırı hazır gelir
- `Paket Ürün` seçeneği UI’da görünür ama backend hazır değilse disabled veya “yakında” etiketli olur

Gerekli query param önerisi:

- `/admin/products/create?mode=simple`
- `/admin/products/create?mode=variant`
- `/admin/products/create?mode=bundle`

Gerekli ViewModel alanı:

- `CreateMode`

Olası enum:

```csharp
public enum ProductCreateMode
{
    Simple,
    Variant,
    Bundle
}
```

## Faz 4 - Sekmeli Ürün Detay/Edit Ekranı

Referans:
`Görsel 7-10`

Amaç:

- tek uzun formdan sekmeli düzenleyiciye geçmek
- bilgi mimarisini sadeleştirmek
- daha sonra yeni alanlar eklendiğinde formun dağılmasını önlemek

## Önerilen sekme mimarisi

### Sekme 1 - Temel Bilgi

UI içeriği:

- ürün adı
- ürün tipi
- satış fiyatı
- indirimli fiyat
- alış fiyatı
- birim fiyat göster
- fiyat listesi ekle linki veya placeholder

Bugün backend’de doğrudan desteklenen alanlar:

- ürün adı
- satış fiyatı
- indirimli fiyat

Bugün kısmi veya eksik olanlar:

- ürün tipi
- alış fiyatı
- birim fiyat
- fiyat listesi

İlk faz kararı:

- desteklenmeyen alanlar ya disabled gösterilir ya da “yakında” bilgi etiketi ile sunulur

### Sekme 2 - Medya

UI içeriği:

- kapak görseli
- galeri sıralama
- medya kütüphanesinden seçme
- sürükle bırak
- primary görsel işareti

Bugünkü sistem:

- güçlü temel var
- [wwwroot/js/pages/admin-product-gallery.js](C:/Users/vedat/Documents/GitHub/vitacure/wwwroot/js/pages/admin-product-gallery.js) ve medya kütüphanesi entegrasyonu hazır

Karar:

- medya sekmesi ilk teslimde gerçek çalışan sekme olacak

### Sekme 3 - Ürün Detayı

UI içeriği:

- marka
- etiketler
- Google ürün kategorisi
- tedarikçi
- kategori kartları
- açıklama rich text

Bugün desteklenen alanlar:

- marka
- etiket
- kategori
- açıklama

Eksik alanlar:

- Google ürün kategorisi
- tedarikçi

Karar:

- desteklenen alanlar gerçek form elemanı olacak
- eksik alanlar taslak veya disabled görünümde açılacak

### Sekme 4 - Envanter

UI içeriği:

- SKU
- barkod
- desi
- HS kodu
- stok lokasyon tablosu
- stok tükenince satışa devam et

Bugün desteklenen alanlar:

- stok
- varyant stokları

Eksik alanlar:

- SKU ana ürün seviyesi
- barkod
- desi
- HS kodu
- lokasyon bazlı stok
- backorder benzeri flag

Karar:

- ilk teslimde bu sekme “mevcut stok yönetimi” odaklı sade versiyonla açılır
- ana stok
- varyant stokları özeti
- ileride açılacak alanlar için placeholder blokları

### Sekme 5 - SEO

UI içeriği:

- SEO başlık
- meta açıklama
- slug
- önizleme kartı
- gelişmiş SEO ayarları

Bugünkü sistem:

- ürün entity’de yalnızca `Slug` var
- ayrı SEO alanları yok

Karar:

- ilk teslimde `Slug` aktif alan olur
- SEO title / meta alanları UI placeholder olabilir
- ikinci backend fazında product SEO alanları eklenir

### Sekme 6 - Özel Alanlar

Bugünkü sistemde doğrudan desteklenmiyor.

İlk teslim:

- boş state + “yakında”
- ya da mevcut `Feature` yapısına yönlendiren bilgi alanı

### Sekme 7 - Ürün Özelleştirmesi

Bugünkü sistemde doğrudan desteklenmiyor.

İlk teslim:

- boş state
- daha sonra personalizasyon veya configurator alanı için ayrılmış alan

### Sekme 8 - Varyant

Bugünkü sistemde destekleniyor.

Bu sekme ilk teslimde gerçek çalışan alanlardan biri olmalı:

- varyant satırları
- eksen
- seçenek
- SKU
- fiyat
- indirimli fiyat
- stok
- sıra
- aktiflik

Mevcut kaynak:

- [wwwroot/js/pages/admin-product-variants.js](C:/Users/vedat/Documents/GitHub/vitacure/wwwroot/js/pages/admin-product-variants.js)

## Mevcut Formun Yeni Parçalara Bölünmesi

Bugünkü dev partial:

- [Areas/Admin/Views/Products/_Form.cshtml](C:/Users/vedat/Documents/GitHub/vitacure/Areas/Admin/Views/Products/_Form.cshtml)

Yeni hedef yapı:

- `Areas/Admin/Views/Products/_EditorShell.cshtml`
- `Areas/Admin/Views/Products/_EditorTabs.cshtml`
- `Areas/Admin/Views/Products/Tabs/_BasicTab.cshtml`
- `Areas/Admin/Views/Products/Tabs/_MediaTab.cshtml`
- `Areas/Admin/Views/Products/Tabs/_DetailsTab.cshtml`
- `Areas/Admin/Views/Products/Tabs/_InventoryTab.cshtml`
- `Areas/Admin/Views/Products/Tabs/_SeoTab.cshtml`
- `Areas/Admin/Views/Products/Tabs/_CustomFieldsTab.cshtml`
- `Areas/Admin/Views/Products/Tabs/_PersonalizationTab.cshtml`
- `Areas/Admin/Views/Products/Tabs/_VariantsTab.cshtml`
- `Areas/Admin/Views/Products/Modals/_CreateModeModal.cshtml`
- `Areas/Admin/Views/Products/Modals/_ImportExportStepOneModal.cshtml`
- `Areas/Admin/Views/Products/Modals/_ImportExportStepTwoModal.cshtml`

## Yeni ViewModel Planı

Bugünkü ViewModel:

- [Models/ViewModels/Admin/ProductFormViewModel.cs](C:/Users/vedat/Documents/GitHub/vitacure/Models/ViewModels/Admin/ProductFormViewModel.cs)

İlk büyük adımda tamamen parçalamak yerine tek ana ViewModel korunmalı.

Ana modele eklenecek olası alanlar:

- `CreateMode`
- `ProductType`
- `CostPrice`
- `SupplierName`
- `GoogleProductCategory`
- `Barcode`
- `Sku`
- `MetadataTitle`
- `MetadataDescription`
- `UnitPriceEnabled`
- `AllowBackorder`
- `WeightDesi`
- `HsCode`

Liste modeli için eklenecek olası alanlar:

- `OldPrice`
- `CostPrice`
- `SalesChannelCount`
- `CreatedAt`
- `UpdatedAt`
- `ColumnVisibility`
- `ViewMode`

İlk teslimde bu alanların bir kısmı sadece UI amaçlı olabilir.

## Backend Genişleme Planı

## A. Hemen yapılabilecek backend işleri

- ürün liste filtrelerini service katmanına taşımak
- liste için card/table ortak veri modeli üretmek
- export endpoint hazırlamak
- create mode query param desteklemek
- sütun görünürlük ayarını local storage ile başlatmak

## B. Kısa vadeli migration isteyen işler

- `Product` tablosuna:
  - `CostPrice`
  - `Sku`
  - `Barcode`
  - `ProductType`
  - `SeoTitle`
  - `SeoDescription`
  - `GoogleProductCategory`
  - `SupplierName`
  - `AllowBackorder`
  - `HsCode`
  - `Desi`
- `ProductVariant` tablosuna:
  - `Barcode`
  - gerekirse `SupplierSku`

## C. Orta vadeli yeni tablo gerektiren işler

- ürün satış kanalları
- ürün metadata / custom field records
- stok lokasyon tablosu
- import job / audit kayıtları
- kullanıcı bazlı admin liste tercihleri
- paket ürün ilişkileri

## Uygulama Sırası

Önerilen sıra:

1. Liste ekranı modernizasyonu
2. kart / tablo görünüm geçişi
3. sütun göster/gizle
4. ürün ekleme başlangıç modalı
5. sekmeli editör shell
6. medya ve varyant sekmelerinin yeni yapıya taşınması
7. ürün detayı, envanter ve SEO sekmeleri
8. export modülü
9. import modülü
10. eksik backend alanlarının migration ile tamamlanması

## İlk Teslim İçin Kırpılmış Kapsam

Eğer hızlı ama etkili bir ilk sürüm istiyorsak:

- kart görünüm
- tablo görünüm
- sütun göster/gizle
- ürün ekleme tipi modalı
- sekmeli editör iskeleti
- gerçek çalışan sekmeler:
  - temel bilgi
  - medya
  - ürün detayı
  - varyant
- sınırlı envanter sekmesi
- placeholder SEO / özel alanlar / ürün özelleştirmesi

Bu yaklaşım, görsel referansa çok yaklaşır ve mevcut backend’i zorlamadan sistemi büyütür.

## Riskler

- Tek seferde tüm ekranı dönüştürmek yüksek kırılma riski taşır
- `_Form.cshtml` doğrudan büyük çapta parçalanırken isim binding’ler bozulabilir
- variant ve media JS davranışları yeni DOM yapısında yeniden bağlanmalıdır
- import modülü UI ile sınırlı kalırsa kullanıcı beklentisi oluşabilir
- eksik backend alanları gerçekmiş gibi sunulursa veri tutarsızlığı yaratır

## Uygulama Esasları

- Görsel referansa mümkün olduğunca yakın gidilecek
- Ancak backend’de olmayan alanlar “çalışıyor gibi” gösterilmeyecek
- İlk aşamada çalışan alanlar öne alınacak
- UI dili sade ve ticari yönetim odaklı olacak
- Sekmeli yapı kalıcı temel olacak
- Liste ekranı kart + tablo arasında tek veri kaynağını paylaşacak

## Sonuç

Bu planla ürün modülü:

- daha profesyonel bir katalog yönetim yüzüne kavuşur
- mevcut kod tabanına uyumlu kalır
- kademeli teslimata uygun hale gelir
- görsel olarak referans tasarıma çok yaklaşır
- backend eksikleri net biçimde ayrıştırıldığı için ileride büyütülmesi kolaylaşır

## Önerilen Sonraki Adım

Bu doküman onaylandıktan sonra bir sonraki gerçek uygulama işi şu olmalı:

1. ürün liste ekranının card/list çift görünümünü implement etmek
2. `Ürün Ekle` başlangıç modalını eklemek
3. ürün formunu sekmeli shell yapıya bölmek

Bu üç adım tamamlandığında ürün modülünün omurgası kurulmuş olur.
