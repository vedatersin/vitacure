# Mock Data Normalized Working Draft

Bu dosya refactor için hazırlık amaçlıdır. Veriler silinmeden, kaynağı korunarak daha düzenli kümelere ayrılmıştır.

## 1. Global UI Dictionary

### Brand

- `brand.name`: `VitaCure`
- `brand.footerDescription`: `Vitacure, sağlığınıza değer veren, bilim ve doğanın gücüyle tasarlanmış, kişiselleştirilmiş en kaliteli vitamin ve gıda takviyelerini sunan %100 güvenilir platformdur.`

### Navigation

- `nav.about`: `Biz Kimiz?`
- `nav.allProducts`: `Tüm Ürünler`
- `nav.categories`: `Kategoriler`
- `nav.brands`: `Markalar`
- `nav.ai`: `Vitacure AI`
- `nav.aiBadge`: `yeni`
- `nav.favorites`: `Favoriler`
- `nav.cart`: `Sepetim`
- `nav.login`: `Giriş Yap`

### Footer

- `footer.heading.categories`: `Kategoriler`
- `footer.heading.quickLinks`: `Hızlı Linkler`
- `footer.heading.corporate`: `Kurumsal`
- `footer.link.distanceSales`: `Mesafeli Satış Sözleşmesi`
- `footer.link.privacySecurity`: `Gizlilik ve Güvenlik Politikası`
- `footer.link.returnCancel`: `İade ve İptal Koşulları`
- `footer.link.kvkk`: `KVKK Aydınlatma Metni`
- `footer.link.contact`: `İletişim`
- `footer.copyright`: `© 2026 VitaCure. Tüm Hakları Saklıdır.`

## 2. Sections

- `home.hero.title`: `Bir Dünya Sağlık`
- `home.hero.subtitle`: `Peki senin ihtiyacin ne?`
- `home.sections.featured`: `Öne Çıkan Ürünler`
- `home.sections.sleep`: `Uyku Sağlığı`
- `home.sections.popularSupplements`: `Popüler Takviyeler`
- `home.sections.campaigns`: `Kampanyalar`
- `home.sections.deals`: `Fırsat Ürünleri`

## 3. Categories

### Canonical category list

1. `uyku-sagligi` -> `Uyku Sağlığı`
2. `multivitamin-enerji` -> `Multivitamin & Enerji`
3. `zihin-hafiza-guclendirme` -> `Zihin & Hafıza Güçlendirme`
4. `hastaliklara-karsi-koruma` -> `Hastalıklara Karşı Koruma`
5. `kas-ve-iskelet-sagligi` -> `Kas ve İskelet Sağlığı`
6. `zayiflama-destegi` -> `Zayıflama Desteği`

### Category-specific content

- `uyku-sagligi.description`
  - `Melatonin, magnezyum, bitkisel ekstreler, gece rutini destekleri ve çocuklara uygun uyku takviyeleri ile daha derin, kesintisiz ve dinlendirici uyku deneyimi için özenle seçilmiş ürünleri keşfedin; rahatlama, gevşeme, uykuya geçiş ve sabah zindeliğini destekleyen güçlü formüller bu kategoride sizi bekliyor.`
- `uyku-sagligi.tags`
  - `Tümü`
  - `Melatonin`
  - `Bitkisel Ekstreler`
  - `Çocuklar İçin`

## 4. Chat Widget

### Core labels

- `chat.back`: `Geri`
- `chat.compactCategory`: `Kategori`
- `chat.searchFilterLabel`: `Kategori`
- `chat.mode.chat`: `Sohbet Modu`
- `chat.mode.search`: `Ürün Arama Modu`
- `chat.button.fullscreen`: `Tam Ekran`
- `chat.button.addFile`: `Dosya Ekle`
- `chat.menu.document`: `Dosya Yukleyin`
- `chat.menu.image`: `Fotograflar`

### Placeholders

- `chat.placeholder.default`: `Hedefini seç, yapay zeka ile sana uygun ürünleri ve rutinleri birlikte planlayalım.`
- `chat.placeholder.searchGlobal`: `Katalogda aramak istediğiniz ürünü yazın...`
- `chat.placeholder.searchInCategory`: `İçinde ara...`

### Dynamic patterns

- `chat.aria.addToCart`: `{productName} ürününü sepete ekle`
- `chat.aria.favoriteActive`: `{productName} favorilerde`
- `chat.aria.favoriteInactive`: `{productName} favorilere ekle`

## 5. Example Prompt Pools

### `uyku-sagligi`

- Son zamanlarda uykum çok düzensiz, ne yapabilirim?
- Gece sık sık uyanıyorum, öneriniz var mı?
- Daha kaliteli uyumak için ne kullanmalıyım?
- Sabah yorgun kalkıyorum, sebebi ne olabilir?

### `multivitamin-enerji`

- Gün içinde çok halsiz hissediyorum, ne önerirsin?
- Enerjimi artırmak için hangi vitaminleri almalıyım?
- Yorgunluk için iyi gelen takviyeler neler?
- Bağışıklık ve enerji için ne kullanabilirim?

### `zihin-hafiza-guclendirme`

- Odaklanmakta zorlanıyorum, ne yapmalıyım?
- Hafızamı güçlendirmek için ne önerirsin?
- Ders çalışırken dikkatim dağılıyor, çözüm var mı?
- Beyin performansımı artırmak istiyorum

### `hastaliklara-karsi-koruma`

- Bağışıklığımı güçlendirmek istiyorum, ne önerirsin?
- Sık hasta oluyorum, nasıl korunabilirim?
- Gripten korunmak için ne kullanmalıyım?
- Genel sağlık için hangi takviyeler iyi?

### `kas-ve-iskelet-sagligi`

- Eklem ağrılarım var, ne önerirsin?
- Kemik sağlığımı korumak için ne yapmalıyım?
- Spor yapıyorum, kaslarım için destek önerir misin?
- Bel ve diz ağrılarım için çözüm var mı?

### `zayiflama-destegi`

- Kilo vermek istiyorum, nereden başlamalıyım?
- Yağ yakımını hızlandırmak için ne önerirsin?
- Diyet yapıyorum ama sonuç alamıyorum
- İştahımı kontrol etmek için ne kullanabilirim?

## 6. Product Collections

### Featured collection

- Daily Multivitamin
- Omega 3
- Vitamin D3
- Magnezyum
- C Vitamini Complex
- Kolajen Peptit
- B12 Vitamini
- Çinko Pikolinat
- Probiyotik 10B
- Demir + C Vitamini

### Uyku coverflow collection

- Kalsiyum Kompleks
- Daily Multivitamin
- Omega 3
- Magnezyum
- C Vitamini Complex
- Vitamin D3
- B12 Vitamini Sprey

### Fırsat ürünleri collection

- Vitamin D3 - 3 Al 2 Öde
- Omega 3 Aile Paketi
- C Vitamini Seti
- Magnezyum Enerji Kofre
- Daily Multivitamin Büyük Boy
- Kolajen ve C Vitamini
- Çinko Kompleks
- Probiyotik Bakteri
- B12 Vitamini Sprey
- Demir Takviyesi

### Duplicate-sensitive products

- Daily Multivitamin
- Omega 3
- Vitamin D3
- Magnezyum
- C Vitamini Complex
- B12 Vitamini Sprey

## 7. Campaign and Banner Collections

### Popüler Takviyeler cards

- Vitamin D
- Demir
- B12
- Folat/Folik asit
- Kalsiyum
- İyot
- Magnezyum
- Potasyum
- Çinko

### Banner assets

- aspinatura-banner-mobil.jpg
- corega-banner-mobile.jpg
- dynavit-mobil-banner.jpg
- easyfishoil-banner-mobile.jpg
- laroche-mobil-banner.jpg
- newlife-mobil-banner.jpg
- nutraxin-banner-mobile.jpg
- pharmaton-mobil-banner.jpg
- vitacureai.png
- uyku.png

## 8. Filters and UI Options

### Brand filters

- Solgar 4
- Now Foods 3
- Ocean 2
- Dynavit 1

### Form filters

- Kapsül 6
- Tablet 4
- Damla 2

### Sort options

- Önerilen
- En Düşük Fiyat
- En Yüksek Fiyat

## 9. Scaffold / Preserve-as-Is Texts

- `Privacy Policy`
- `Use this page to detail your site's privacy policy.`
- `Error`
- `An error occurred while processing your request.`
- `Request ID:`
- `Development Mode`

## 10. Suggested Future Storage Buckets

Bu bölüm refactor yapmaz; sadece ileride taşınabilecek güvenli hedef kümeleri gösterir.

- `HomePageContent`
  - hero metinleri
  - section başlıkları
  - CTA etiketleri
- `CategoryContent`
  - kategori adları
  - slug
  - icon
  - açıklama
  - örnek prompt havuzları
- `ProductCatalogMock`
  - featured
  - deals
  - coverflow
- `CampaignContent`
  - banner dosyaları
  - popüler takviye kartları
- `ChatWidgetContent`
  - placeholder
  - mode labels
  - button/menu labels
  - aria kalıpları
- `FooterContent`
  - link metinleri
  - açıklama
  - copyright
