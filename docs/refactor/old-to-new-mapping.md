# Old-to-New Mapping

| Eski kaynak dosya | Eski bölüm | Yeni dosya | Yeni sorumluluk |
| --- | --- | --- | --- |
| `/Views/Home/Index.cshtml` | Hero başlık + subtitle + chat widget | `/Views/Home/Partials/_HeroSection.cshtml` | Anasayfa hero composition |
| `/Views/Home/Index.cshtml` | Hero chat widget markup | `/Views/Shared/Partials/_ChatWidget.cshtml` | Home/category ortak chat widget markup |
| `/Views/Home/Index.cshtml` | Kategori pill markup | `/Views/Shared/Partials/_CategoryPill.cshtml` | Tekrarlı kategori giriş kartı |
| `/Views/Home/Index.cshtml` | Featured ürün kart markup | `/Views/Shared/Partials/_ProductCard.cshtml` | Ortak ürün kartı |
| `/Views/Home/Index.cshtml` | Featured ürün slider bölümü | `/Views/Home/Partials/_FeaturedProducts.cshtml` | Featured section composition |
| `/Views/Home/Index.cshtml` | Popüler takviyeler kartları | `/Views/Home/Partials/_PopularSupplements.cshtml` | Popüler takviyeler section composition |
| `/Views/Home/Index.cshtml` | Banner kart markup | `/Views/Shared/Partials/_BannerCard.cshtml` | Ortak banner kartı |
| `/Views/Home/Index.cshtml` | Kampanyalar bölümü | `/Views/Home/Partials/_Campaigns.cshtml` | Kampanya grid composition |
| `/Views/Home/Index.cshtml` | Fırsat ürünleri bölümü | `/Views/Home/Partials/_OpportunityProducts.cshtml` | Fırsat ürünleri composition |
| `/Views/Home/Index.cshtml` | Filtre sidebar markup | `/Views/Shared/Partials/_FilterSidebar.cshtml` | Ortak filtre alanı |
| `/Views/Home/Index.cshtml` | Breadcrumb/back navigation mantığı | `/Views/Shared/Partials/_Breadcrumb.cshtml` | Category breadcrumb markup |
| `/Views/Home/Index.cshtml` | Uyku category detail embedded page | `/Views/Category/Detail.cshtml` | Gerçek route’lu kategori detail sayfası |
| `/Views/Home/Index.cshtml` | Uyku category coverflow script | `/wwwroot/js/pages/category.js` | Category coverflow davranışı |
| `/Views/Home/Index.cshtml` | Chat widget inline script | `/wwwroot/js/components/chat-widget.js` | Tek merkezli reusable widget davranışı |
| `/Views/Home/Index.cshtml` | Featured swiper inline script | `/wwwroot/js/pages/home.js` | Home page slider davranışı |
| `/Views/Home/Index.cshtml` | Heart/add button inline davranışı | `/wwwroot/js/components/product-card.js` | Ortak ürün kartı etkileşimi |
| `/Views/Home/Index.cshtml` | Filter interactions | `/wwwroot/js/components/filter-sidebar.js` | Ortak filtre davranışı |
| `/Views/Home/Index.cshtml` | Inline style block | `/wwwroot/css/pages/home.css` | Home page düzen ve spacing |
| `/Views/Home/Index.cshtml` | Category embedded page styles | `/wwwroot/css/pages/category.css` | Category sayfası düzeni ve background’lar |
| `/Views/Home/Index.cshtml` | Chat widget stilleri | `/wwwroot/css/components/chat-widget.css` | Ortak chat widget component stilleri |
| `/Views/Home/Index.cshtml` | Product glass card stilleri | `/wwwroot/css/components/product-card.css` | Ortak ürün kartı stilleri |
| `/Views/Home/Index.cshtml` | Banner hover stilleri | `/wwwroot/css/components/banner-card.css` | Ortak banner kart stilleri |
| `/Views/Home/Index.cshtml` | Filter sidebar stilleri | `/wwwroot/css/components/filter-sidebar.css` | Ortak filtre sidebar stilleri |
| `/Views/Home/Index.cshtml` | Category pill stilleri | `/wwwroot/css/components/category-pill.css` | Ortak kategori pill stilleri |
| `/Views/Home/Index.cshtml` | Section heading stilleri | `/wwwroot/css/components/section-header.css` | Ortak section header stilleri |
| `/Views/Home/Index.cshtml` + `/docs/mock-data.json` | Hardcoded ürün/kategori/banner/prompt verileri | `/Services/Content/MockContentService.cs` | Merkezi mock content erişimi |
| `/Views/Home/Index.cshtml` + `/docs/mock-data*.md` | View içi veri kümeleri | `/Models/ViewModels/*.cs` | Taşınmış, strongly typed view models |
| `/Views/Shared/_Layout.cshtml` | Statik title/meta | `/Views/Shared/_Layout.cshtml` | Dinamik SEO title/meta/canonical desteği |
