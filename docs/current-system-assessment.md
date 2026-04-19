# Mevcut Sistem Tespit Raporu

## Executive Summary

- Repo, tek bir `ASP.NET Core MVC (.NET 8)` monoliti. Storefront ve admin aynı uygulama içinde; admin `Areas/Admin` ile ayrılmış. Giriş noktası [Program.cs](C:/Users/vedat/Documents/GitHub/vitacure/Program.cs), proje dosyası [vitacure.csproj](C:/Users/vedat/Documents/GitHub/vitacure/vitacure.csproj).
- Gerçekten çalışan çekirdek domainler: `Product`, `Category`, `Tag`, `Showcase`, `User/Role`, `Cart`, `Favorite`, `Address`, `Order`, `HomeContentSettings`.
- Hedef mimaride istenen birçok modül repo içinde yok veya yarım: `Brand`, `Attribute/Specification/Property`, `Variant`, `Collection`, `Review`, gerçek `Notification` backend'i, gerçek `AI chat`, `Agent`, `Automation`, `Audit`, `Analytics`.
- En kritik veri model riskleri:
  - Ürün sadece `tek category FK` ile bağlı; çoklu kategori yok. [Product.cs](C:/Users/vedat/Documents/GitHub/vitacure/Domain/Entities/Product.cs), [AppDbContext.cs](C:/Users/vedat/Documents/GitHub/vitacure/Infrastructure/Persistence/AppDbContext.cs)
  - Ürün medyası normalize değil; `ImageUrl + GalleryImageUrls` string alanı kullanılıyor.
  - Vitrin etiketleri relation değil, `TagsContent` text blob.
  - Home içerikleri de kısmen relation değil, çok satırlı text blob'lar üzerinden tutuluyor.
  - Bildirim ekranı DB'siz, controller içinde seed/mock.
  - Chat widget yalnızca UI davranışı; model çağrısı, oturum kaydı ve AI persistence yok.
- Sonuç: yeni admin + yeni veritabanı dönüşümü için bu repo "başlangıç commerce iskeleti" olarak iyi; ama katalog/AI/otomasyon hedefi için mevcut model yeterli değil.

## Current Architecture

- Stack:
  - Backend/UI: `ASP.NET Core MVC + Razor Views`
  - ORM: `EF Core 8 + SQL Server`
  - Auth: `ASP.NET Identity`
  - Cache: `OutputCache + optional Redis`
  - Frontend: `Bootstrap`, `jQuery`, vanilla JS
- Katmanlar:
  - `Controllers/`: storefront MVC action'ları
  - `Areas/Admin/Controllers/`: admin action'ları
  - `Infrastructure/Services/`: iş kuralları + data access
  - `Services/Content/`: storefront view composition
  - `Domain/Entities/`: veri modeli
  - `Models/ViewModels/`: Razor view modelleri
  - `Migrations/`: EF migration'ları
- Monolith mi: monolith. Ayrı API, worker, queue consumer, cron app, microservice yok.
- Admin panel: ayrı deploy değil; aynı projede `Area("Admin")`.
- Veritabanı erişimi: tamamen `AppDbContext` üzerinden `EF Core`. Repository katmanı yok. Raw SQL, stored procedure, query builder izi görünmedi.
- API yapısı: klasik JSON API değil. Çoğu akış MVC post/get action. Az sayıda AJAX endpoint var:
  - `/cart/items`
  - `/account/favorites/toggle`
  - `/admin/products/upload-image`
- Worker/cron/background service: bulunamadı.
- Testler var: [vitacure.Tests](C:/Users/vedat/Documents/GitHub/vitacure/vitacure.Tests). Bu ortamda `dotnet` kurulu olmadığı için test çalıştırılamadı.

## Existing Domains

### Ürünler

- Entity/table: `Product` / `Products`
- Dosyalar: [Product.cs](C:/Users/vedat/Documents/GitHub/vitacure/Domain/Entities/Product.cs), [AdminProductService.cs](C:/Users/vedat/Documents/GitHub/vitacure/Infrastructure/Services/AdminProductService.cs), [ProductService.cs](C:/Users/vedat/Documents/GitHub/vitacure/Infrastructure/Services/ProductService.cs)
- UI: [Areas/Admin/Views/Products](C:/Users/vedat/Documents/GitHub/vitacure/Areas/Admin/Views/Products)
- Endpointler: `/admin/products`, `/admin/products/create`, `/admin/products/edit/{id}`, `/urun/{slug}`, `/{slug}`
- İlişkiler: `CategoryId`, `ProductTags`, `ShowcaseFeaturedProducts`

### Kategoriler

- Entity/table: `Category` / `Categories`
- Parent-child var; ürünlerle `1-N`
- Dosyalar: [Category.cs](C:/Users/vedat/Documents/GitHub/vitacure/Domain/Entities/Category.cs), [AdminCategoryService.cs](C:/Users/vedat/Documents/GitHub/vitacure/Infrastructure/Services/AdminCategoryService.cs)
- UI: [Areas/Admin/Views/Categories](C:/Users/vedat/Documents/GitHub/vitacure/Areas/Admin/Views/Categories)

### Markalar

- Belirgin entity/service/controller yok
- Sadece content/mock metinlerinde "Markalar" label'ı geçiyor; gerçek domain değil. [docs/mock-data.json](C:/Users/vedat/Documents/GitHub/vitacure/docs/mock-data.json)

### Özellikler / attribute / specification / property

- Domain modeli bulunamadı

### Etiketler

- Entity/table: `Tag`, join: `ProductTag`
- Dosyalar: [Tag.cs](C:/Users/vedat/Documents/GitHub/vitacure/Domain/Entities/Tag.cs), [ProductTag.cs](C:/Users/vedat/Documents/GitHub/vitacure/Domain/Entities/ProductTag.cs), [AdminTagService.cs](C:/Users/vedat/Documents/GitHub/vitacure/Infrastructure/Services/AdminTagService.cs)
- UI: [Areas/Admin/Views/Tags](C:/Users/vedat/Documents/GitHub/vitacure/Areas/Admin/Views/Tags)

### Koleksiyonlar

- Ayrı model bulunamadı
- Vitrine kısmen benzeyen yapı `Showcase`

### Vitrinler

- Entity/table: `Showcase`, joins: `ShowcaseCategory`, `ShowcaseFeaturedProduct`
- Dosyalar: [Showcase.cs](C:/Users/vedat/Documents/GitHub/vitacure/Domain/Entities/Showcase.cs), [AdminShowcaseService.cs](C:/Users/vedat/Documents/GitHub/vitacure/Infrastructure/Services/AdminShowcaseService.cs), [StorefrontContentService.cs](C:/Users/vedat/Documents/GitHub/vitacure/Services/Content/StorefrontContentService.cs)
- UI: [Areas/Admin/Views/Showcases](C:/Users/vedat/Documents/GitHub/vitacure/Areas/Admin/Views/Showcases)

### Medya

- Ayrı media table yok
- Ürün: `ImageUrl`, `GalleryImageUrls`
- Vitrin: `BackgroundImageUrl`
- Upload: dosya sistemi `wwwroot/img/...`

### Siparişler

- Entity/table: `Order`, `OrderItem`
- Service: [OrderService.cs](C:/Users/vedat/Documents/GitHub/vitacure/Infrastructure/Services/OrderService.cs), [AdminOrderService.cs](C:/Users/vedat/Documents/GitHub/vitacure/Infrastructure/Services/AdminOrderService.cs)
- Admin UI: [Areas/Admin/Views/Orders](C:/Users/vedat/Documents/GitHub/vitacure/Areas/Admin/Views/Orders)

### Sepet

- Entity/table: `CustomerCartItem`
- Service: [CartService.cs](C:/Users/vedat/Documents/GitHub/vitacure/Infrastructure/Services/CartService.cs), guest için [GuestSessionService.cs](C:/Users/vedat/Documents/GitHub/vitacure/Infrastructure/Services/GuestSessionService.cs)

### Favoriler

- Entity/table: `CustomerFavorite`
- Customer + guest cookie hibriti

### Müşteriler

- `AppUser` + `AccountType.Customer`
- Adresler: `CustomerAddress`

### Yorumlar / değerlendirmeler

- Ayrı review modeli yok; ürün rating'i doğrudan `Product.Rating`

### Bildirimler

- Gerçek persistence yok
- UI-only modül: [NotificationsController.cs](C:/Users/vedat/Documents/GitHub/vitacure/Areas/Admin/Controllers/NotificationsController.cs)

### AI sohbetleri

- Sadece chat widget UI/viewmodel var: [Views/Shared/Partials/_ChatWidget.cshtml](C:/Users/vedat/Documents/GitHub/vitacure/Views/Shared/Partials/_ChatWidget.cshtml), [chat-widget.js](C:/Users/vedat/Documents/GitHub/vitacure/wwwroot/js/components/chat-widget.js)
- Kayıt, prompt log, provider integration yok

### Agent / otomasyon

- Bulunamadı

### Kullanıcı / rol / yetki

- `AppUser`, `AppRole`, Identity tabloları
- Roller: `Customer`, `Admin`, `Editor`
- UI sadece listeleme; rol/yetki yönetim ekranı yok

### İçerik sayfaları

- `HomeContentSettings` var
- Privacy vb. statik sayfalar var

### Analitik / log / audit

- Uygulama loglaması var
- Kalıcı audit/log modeli yok

## Database Findings

- Tablolar:
  - Identity: `Users`, `Roles`, `UserRoles`, `UserClaims`, `UserLogins`, `UserTokens`, `RoleClaims`
  - Commerce: `Products`, `Categories`, `Tags`, `ProductTags`
  - Customer: `CustomerFavorites`, `CustomerAddresses`, `CustomerCartItems`
  - Order: `Orders`, `OrderItems`
  - Content: `HomeContentSettings`
  - Showcase: `Showcases`, `ShowcaseCategories`, `ShowcaseFeaturedProducts`
- PK/FK:
  - `Products.CategoryId -> Categories.Id`
  - `ProductTags.ProductId -> Products.Id`, `TagId -> Tags.Id`
  - `CustomerFavorites.AppUserId -> Users.Id`, `ProductId -> Products.Id`
  - `CustomerCartItems.AppUserId -> Users.Id`, `ProductId -> Products.Id`
  - `CustomerAddresses.AppUserId -> Users.Id`
  - `Orders.AppUserId -> Users.Id`
  - `OrderItems.OrderId -> Orders.Id`, `ProductId -> Products.Id (nullable)`
  - `ShowcaseCategories.ShowcaseId -> Showcases.Id`, `CategoryId -> Categories.Id`
  - `ShowcaseFeaturedProducts.ShowcaseId -> Showcases.Id`, `ProductId -> Products.Id`
- Join tabloları:
  - `ProductTags`
  - `ShowcaseCategories`
  - `ShowcaseFeaturedProducts`
- Enum/status:
  - `Users.AccountType` string enum
  - `Orders.Status` string enum
- Soft delete: yok
- Audit alanları:
  - Kısmi var: `CreatedAt`, `UpdatedAt`, `IsActive`
  - Tam audit yok: `CreatedBy`, `UpdatedBy`, `DeletedAt`, `DeletedBy` yok
- Migration yapısı:
  - [Migrations](C:/Users/vedat/Documents/GitHub/vitacure/Migrations) altında EF migration zinciri var
  - `Database.MigrateAsync()` startup'ta otomatik çalışıyor
- Seed:
  - Commerce seed: [AppDbSeeder.cs](C:/Users/vedat/Documents/GitHub/vitacure/Infrastructure/Persistence/AppDbSeeder.cs)
  - Identity seed: [IdentitySeeder.cs](C:/Users/vedat/Documents/GitHub/vitacure/Infrastructure/Identity/IdentitySeeder.cs)
  - Kaynak veri: `docs/mock-data.json`
- Kritik tablo notları:
  - `Products`: brand yok, variant yok, SKU yok, barcode yok, SEO yok, product type yok
  - `Categories`: SEO alanı var, parent var, ama display/media config zayıf
  - `Tags`: çok basit
  - `ProductTags`: düzgün normalize
  - `Product media`: normalize değil; relation table yok
  - `Showcases`: iyi başlangıç ama `TagsContent` free text
  - `HomeContentSettings`: structured içerik yerine serialized text blob kullanıyor

## Admin UI Findings

- Menü:
  - Genel: Dashboard, Siparişler, Kullanıcılar
  - Katalog: Kategoriler, Vitrinler, Ürünler, Etiketler
  - Storefront: Home İçeriği
  - Planlanan: Bildirimler, Yetkilendirme, Raporlar
  - Kaynak: [Areas/Admin/Views/Shared/_Layout.cshtml](C:/Users/vedat/Documents/GitHub/vitacure/Areas/Admin/Views/Shared/_Layout.cshtml)
- Modüller:
  - Gerçek CRUD: products, categories, tags, showcases, home-content
  - Read-only list: orders, users
  - UI-only: notifications
- Form alanları:
  - Product: ad, slug, açıklama, fiyat, eski fiyat, puan, stok, kategori, aktif, ana görsel, galeri, tag'ler
  - Category: ad, slug, açıklama, üst kategori, SEO title, meta description, aktif
  - Tag: ad, slug
  - Showcase: ad, slug, ikon, title, açıklama, tag text, bg görsel, mode, SEO, home visibility, aktif, sıra, kategori havuzu, featured products
  - HomeContent: hero, placeholders, section title'ları, CTA'lar, banner ve blob içerikler
- Eksikler:
  - Marka ekranı yok
  - Attribute/specification ekranı yok
  - Variant ekranı yok
  - Review, notification backend, AI history, agent, automation ekranı yok
  - Role/permission yönetimi yok
  - Bulk action yok
  - Pagination yok
  - Delete/archive aksiyonu yok
- Reusable component yapısı:
  - Ortak admin hero/filter/toast partial'ları var
  - Async liste yenileme JS var
- UI library:
  - `Bootstrap + Font Awesome + jQuery`
- Validation:
  - Server-side `DataAnnotations`
  - Razor validation partial'ı
  - Bazı custom validation: `ShowcaseFormViewModel : IValidatableObject`
- State management:
  - Global frontend state yok; form/local DOM JS
- Upload:
  - Product image upload: `/admin/products/upload-image`, filesystem
  - Showcase background upload: form submit içinde filesystem
- Liste yetenekleri:
  - Filtre var
  - Sıralama UI anlamında yok
  - Pagination yok
  - Toplu işlem yok
- UI-DB uyumu:
  - Product form ile DB büyük ölçüde uyumlu
  - Showcase formunda "etiketler" relation gibi görünüyor ama DB'de tek text alan
  - HomeContent ekranı structured görünse de DB'de blob alanlar var
  - Orders/Users ekranları liste-only; operasyon eksik

## API / Workflow Findings

- Ürün oluşturma/güncelleme:
  - Giriş: `Areas/Admin/Controllers/ProductsController`
  - Service: `AdminProductService`
  - Tablolar: `Products`, `ProductTags`
  - Yetki: `Authorize(Roles = "Admin,Editor")`
  - Validasyon: `DataAnnotations + SlugService`
- Kategori bağlama:
  - Ürün-kategori `single FK`
  - Çoklu kategori workflow yok
- Görsel yükleme:
  - Product upload endpoint ayrı
  - Showcase upload service içinde
  - DB'ye media record değil path string yazılıyor
- Fiyat/stok güncelleme:
  - Product edit form üzerinden aynı update akışında
  - Ayrı inventory/price history yok
- Sipariş oluşumu:
  - Giriş: `/cart/checkout`
  - Service: `OrderService.PlaceOrderFromCartAsync`
  - Tablolar: `Orders`, `OrderItems`, sonra `CustomerCartItems` temizleniyor
  - Queue/event/webhook yok
- Sepete ekleme:
  - Giriş: `/cart/items`
  - Auth user: `CustomerCartItems`
  - Guest: cookie
- Favoriye ekleme:
  - Giriş: `/account/favorites/toggle`
  - Auth user: `CustomerFavorites`
  - Guest: cookie
- Yorum gönderme: yok
- Bildirim oluşturma: gerçek akış yok
- AI sohbet kaydı: yok
- Otomasyon tetikleme: yok
- Agent çalışma mantığı: yok

## AI / Automation Findings

- Chat UI var:
  - View: [Views/Shared/Partials/_ChatWidget.cshtml](C:/Users/vedat/Documents/GitHub/vitacure/Views/Shared/Partials/_ChatWidget.cshtml)
  - JS: [wwwroot/js/components/chat-widget.js](C:/Users/vedat/Documents/GitHub/vitacure/wwwroot/js/components/chat-widget.js)
- Gerçek AI entegrasyonu yok:
  - OpenAI SDK, provider client, outbound API çağrısı bulunamadı
  - Chat send action, conversation table, prompt log, moderation, retry, queue yok
- Mevcut "AI" aslında:
  - home/category/showcase sayfalarında prompt rotasyonu yapan bir UI bileşeni
  - içerik kaynağı: `docs/mock-data.json` ve `HomeContentSettings`
- Notification/automation tarafı:
  - `NotificationsController` tam anlamıyla mock seed data render ediyor
  - Agent/cron/worker/background job kodu bulunamadı

## Showcase/Vitrin Findings

- Vitrin bu repoda gerçekten ayrı domain:
  - `Showcase`
  - `ShowcaseCategory`
  - `ShowcaseFeaturedProduct`
- Alanlar:
  - ad, slug, ikon, başlık, açıklama, etiket blob'u, background image, dark/light, SEO, aktiflik, home görünürlüğü, sıra
- İlişkiler:
  - kategori havuzu var
  - öne çıkan ürün ilişkisi var
- Admin ekranı güçlü:
  - kategori havuzu seçimi
  - featured product slot mantığı
  - drag/drop benzeri vitrin ürün yerleşimi
  - background upload
- Eksikler:
  - showcase SEO dışında landing/page-builder yapısı yok
  - template/layout tipi yok
  - versioning yok
  - schedule/publish window yok
  - preview/publish state yok
  - showcase tags relation değil
- Bu modül yeni "vitrin" mimarisi için en değerli yeniden kullanılabilir çekirdek.

## Gaps vs Target Architecture

- Eksik domainler:
  - brand
  - attribute/specification/property
  - variant
  - collection
  - review/rating source records
  - notification backend
  - AI session/history/provider integration
  - agent
  - automation
  - audit trail
  - analytics/reporting
  - media library
- Yetersiz modelleme:
  - product-category many-to-many yok
  - product media normalize değil
  - tag taxonomy yok
  - showcase tags normalize değil
  - home content blob bazlı
  - user-role management UI eksik
- Admin hedefinize göre mevcut panel "v1 CRUD admin", yeni architecture için yeterli değil.

## Refactor Risks

- Domain karışmaları:
  - `Tag` ve `Showcase.TagsContent` farklı anlamlarla kullanılıyor
  - `Product.Rating` review olmadan saklanıyor
  - chat/AI kavramı UI'da var, backend'de yok
- Veri taşıma riski:
  - `GalleryImageUrls`, `TagsContent`, `PopularSupplementsContent`, `CampaignBannersContent` text blob'ları migration'da parçalanmalı
- İsimlendirme tutarsızlığı:
  - category/showcase slug eşleştirmeleri ve legacy slug tamirleri seed/service içine gömülü
- UI içinde gömülü iş kuralları:
  - showcase background önerme/eşleme
  - product size label mapping
  - notification seed
- Frontend-backend uyumsuz alanlar:
  - "Markalar" nav/dokümanlarda var, backend domain yok
  - "AI" label var, AI backend yok
  - bildirim merkezi var, event store yok
- Migration kırılgan alanları:
  - startup'ta otomatik migrate
  - seed logic showcase'leri onarıyor/değiştiriyor
- Legacy izleri:
  - `legacy_product` route
  - `uncategorized` teknik kategori
  - slug fix/repair mantıkları

## Reusable Parts

### Olduğu gibi korunabilir

- Identity temeli
- basic order/cart/favorite/account akışları
- admin shell, hero/filter/toast partial yapısı
- showcase temel entity üçlüsü

### Refactor ile korunabilir

- product CRUD servisleri
- category CRUD
- tag sistemi
- home content yönetimi
- storefront content composition
- slug conflict service

### Baştan yazılması daha doğru

- product data model
- media modeli
- brand/attribute/variant/collection ekseni
- notification sistemi
- AI sohbet backend'i
- agent/automation
- role/permission admin
- analytics/audit

## Recommended Migration Order

1. Yeni hedef domain sözlüğünü netleştirin: `category / tag / attribute / variant / collection / showcase / brand`.
2. Yeni katalog çekirdeğini tasarlayın: `product`, `brand`, `product_variant`, `attribute`, `attribute_value`, `product_media`, `product_category`, `product_tag`.
3. Vitrin domain'ini ayrı güçlendirin: `showcase`, `showcase_section`, `showcase_product`, `showcase_category`, `showcase_tag`.
4. Blob alanları normalize edin: home content ve showcase tags/media.
5. Admin ortak altyapısını koruyup yeni modül bazlı admin screens ekleyin.
6. Notification/audit/event omurgasını kurun.
7. AI session + message + run + prompt/template + moderation şemasını ekleyin.
8. Agent/automation modüllerini en son ekleyin; çünkü şu an repo içinde bunlara temel oluşturacak runtime yok.
9. Geçiş boyunca storefront adapter katmanı kurup eski view modelleri yeni şemadan besleyin.

## Yeni Sistem Tasarımı İçin Benden Hâlâ Öğrenilmesi Gerekenler

- Ürün tipi tek mi, çok mu: simple / variant / bundle / subscription?
- Varyant ekseni ne olacak: boyut, aroma, form, paket?
- Marka zorunlu mu, opsiyonel mi?
- Kategori tekil sınıflama mı, çoklu taxonomy mi?
- Etiketler serbest mi, tipli mi: campaign tag, symptom tag, ingredient tag?
- Attribute ile tag sınırı tam olarak nasıl çizilecek?
- Showcase ürün seçimi manuel mi, kural bazlı mı, hibrit mi?
- Showcase kategori havuzu zorunlu mu?
- Showcase içinde section bazlı layout gerekiyor mu?
- Medya için DAM benzeri merkezi kütüphane gerekiyor mu?
- AI oturum tipleri neler: shopper chat, admin copilot, support?
- AI kayıt retention ve moderation gereksinimleri neler?
- Agent yetkileri hangi scope'larda çalışacak?
- Otomasyonlar event-driven mi, schedule-driven mı?
- Bildirim kanalları neler: admin inbox, email, push, sms, webhook?
- Sipariş statüleri ve fulfillment akışı ne kadar detaylı olacak?
- Review gerçek kullanıcı yorumu olacak mı, moderasyon olacak mı?
- Audit trail hangi modüllerde zorunlu?
- Backoffice role modeli basit role mi, permission matrix mi?

## Alan Bazlı Özet

| Alan | Mevcut mu | Nerede | Ne kadar tamam | Sorunlar | Önerilen aksiyon |
|---|---|---|---|---|---|
| Ürünler | Evet | `Domain/Entities/Product.cs`, `Areas/Admin/Views/Products` | Orta | brand/variant/media normalize değil, tek kategori | Refactor ile koru, modeli yeniden kur |
| Kategoriler | Evet | `Category.cs`, `AdminCategoryService.cs` | Orta | çoklu taxonomy yok | Korunabilir, relation'ları genişlet |
| Markalar | Hayır | sadece UI/mock label | Çok düşük | gerçek domain yok | Baştan yaz |
| Özellikler | Hayır | bulunamadı | Yok | attribute/spec sistemi yok | Baştan yaz |
| Etiketler | Evet | `Tag.cs`, `ProductTag.cs` | Orta | type/taxonomy yok | Refactor ile koru |
| Koleksiyonlar | Hayır | bulunamadı | Yok | showcase ile karışabilir | Baştan yaz |
| Vitrinler | Evet | `Showcase*`, admin showcase ekranları | Orta-iyi | tags free text, page builder yok | Güçlendirip koru |
| Medya | Kısmen | string URL alanları + filesystem | Düşük | normalize değil, media library yok | Baştan yaz |
| Siparişler | Evet | `Order`, `OrderItem`, admin orders | Orta | status akışı sığ | Refactor ile koru |
| Sepetler | Evet | `CustomerCartItem`, `GuestSessionService` | Orta | guest/auth ikili model, promotion yok | Refactor ile koru |
| Favoriler | Evet | `CustomerFavorite` | Orta | basit | Korunabilir |
| Müşteriler | Evet | `AppUser`, `CustomerAddress` | Orta | CRM seviyesi yok | Refactor ile koru |
| Yorumlar | Hayır | sadece `Product.Rating` | Çok düşük | review entity yok | Baştan yaz |
| Bildirimler | Kısmen | admin mock UI | Çok düşük | DB/event yok | Baştan yaz |
| AI | Kısmen | chat widget UI | Çok düşük | gerçek AI backend yok | Baştan yaz |
| Agentlar | Hayır | bulunamadı | Yok | runtime yok | Baştan yaz |
| Otomasyonlar | Hayır | bulunamadı | Yok | cron/job modeli yok | Baştan yaz |
| İçerik | Evet | `HomeContentSettings`, Razor sayfalar | Orta | blob alanlar | Refactor ile koru |
| Yetki sistemi | Kısmen | Identity role tabanı var | Düşük-orta | permission UI/matrix yok | Temeli koru, modeli genişlet |
