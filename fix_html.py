import pathlib

p = pathlib.Path('c:/Users/vedat/Documents/GitHub/vitacure/Views/Home/Index.cshtml')
text = p.read_text(encoding='utf-8')

# The text has double-encoded UTF-8 since powershell read utf-8 bytes as windows-1254 and saved as utf-8.
# We just need to fix the specific block.

bad_block = """                        <h2 style="color: rgba(255,255,255,0.9); font-weight: 800; font-size: 4.8rem; text-shadow: 1px 2px 5px rgba(0,0,0,0.4); margin-bottom: 25px; line-height: 1.1;">Uyku SaÄŸlÄ±ÄŸÄ±</h2>
                        <p style="color: rgba(255,255,255,0.8); font-size: 1.6rem; font-weight: 400; text-shadow: 1px 2px 4px rgba(0,0,0,0.4); margin-bottom: 40px; line-height: 1.5;">Derin ve rahat bir uyku iÃ§in doÄŸanÄ±n ve bilimin en iyi destekleri.</p>
                        
                        <!-- Etiket ButonlarÄ± Havuzu (Åžeffaf) -->
                        <div class="uyku-tag-cloud">
                    <!-- RIGHT SIDE: Product Coverflow Carousel -->
                    <div class="col-lg-7 col-xl-8 position-relative mt-4 mt-lg-0 d-flex flex-column align-items-center justify-content-center" style="min-height: 520px; padding-left: 80px; padding-right: 80px;">
                        <!-- YÃ¶n OklarÄ± -->"""

good_block = """                        <h2 style="color: rgba(255,255,255,0.9); font-weight: 800; font-size: 4.8rem; text-shadow: 1px 2px 5px rgba(0,0,0,0.4); margin-bottom: 25px; line-height: 1.1;">Uyku Sağlığı</h2>
                        <p style="color: rgba(255,255,255,0.8); font-size: 1.6rem; font-weight: 400; text-shadow: 1px 2px 4px rgba(0,0,0,0.4); margin-bottom: 40px; line-height: 1.5;">Derin ve rahat bir uyku için doğanın ve bilimin en iyi destekleri.</p>
                        
                        <!-- Etiket Butonları Havuzu (Şeffaf) -->
                        <div class="uyku-tag-cloud">
                            <button type="button" class="uyku-tag-btn active"><i class="fa-solid fa-layer-group text-warning"></i> Tümü</button>
                            <button type="button" class="uyku-tag-btn"><i class="fa-solid fa-moon text-primary"></i> Melatonin</button>
                            <button type="button" class="uyku-tag-btn"><i class="fa-solid fa-leaf text-success"></i> Bitkisel Ekstreler</button>
                            <button type="button" class="uyku-tag-btn"><i class="fa-solid fa-child text-info"></i> Çocuklar İçin</button>
                        </div>
                    </div>

                    <!-- RIGHT SIDE: Product Coverflow Carousel -->
                    <div class="col-lg-7 col-xl-8 position-relative mt-4 mt-lg-0 d-flex flex-column align-items-center justify-content-center" style="min-height: 520px; padding-left: 80px; padding-right: 80px;">
                        <!-- Yön Okları -->"""

# We'll do a simple replace matching lines, ignoring line ending differences.
import re

bad_regex = re.compile(bad_block.replace('\\n', '\\n\\s*').replace('Ä', '.'), re.DOTALL)

if bad_regex.search(text):
    print("Found it with regex!")
    new_text = bad_regex.sub(good_block, text)
    p.write_text(new_text, encoding='utf-8')
    print("Fixed!")
else:
    # Fallback, just replace parts
    text = text.replace("Uyku SaÄŸlÄ±ÄŸÄ±", "Uyku Sağlığı")
    text = text.replace("iÃ§in doÄŸanÄ±n", "için doğanın")
    text = text.replace("Etiket ButonlarÄ± Havuzu (Åžeffaf)", "Etiket Butonları Havuzu (Şeffaf)")
    text = text.replace("YÃ¶n OklarÄ±", "Yön Okları")
    
    # insert the missing items
    target_insert = '<div class="uyku-tag-cloud">\\n                    <!-- RIGHT SIDE: Product Coverflow Carousel -->'
    if target_insert in text:
        text = text.replace(target_insert, '<div class="uyku-tag-cloud">\\n                            <button type="button" class="uyku-tag-btn active"><i class="fa-solid fa-layer-group text-warning"></i> Tümü</button>\\n                            <button type="button" class="uyku-tag-btn"><i class="fa-solid fa-moon text-primary"></i> Melatonin</button>\\n                            <button type="button" class="uyku-tag-btn"><i class="fa-solid fa-leaf text-success"></i> Bitkisel Ekstreler</button>\\n                            <button type="button" class="uyku-tag-btn"><i class="fa-solid fa-child text-info"></i> Çocuklar İçin</button>\\n                        </div>\\n                    </div>\\n\\n                    <!-- RIGHT SIDE: Product Coverflow Carousel -->')
    
    # also try with \r\n
    target_insert2 = '<div class="uyku-tag-cloud">\\r\\n                    <!-- RIGHT SIDE: Product Coverflow Carousel -->'
    if target_insert2 in text:
        text = text.replace(target_insert2, '<div class="uyku-tag-cloud">\\r\\n                            <button type="button" class="uyku-tag-btn active"><i class="fa-solid fa-layer-group text-warning"></i> Tümü</button>\\r\\n                            <button type="button" class="uyku-tag-btn"><i class="fa-solid fa-moon text-primary"></i> Melatonin</button>\\r\\n                            <button type="button" class="uyku-tag-btn"><i class="fa-solid fa-leaf text-success"></i> Bitkisel Ekstreler</button>\\r\\n                            <button type="button" class="uyku-tag-btn"><i class="fa-solid fa-child text-info"></i> Çocuklar İçin</button>\\r\\n                        </div>\\r\\n                    </div>\\r\\n\\r\\n                    <!-- RIGHT SIDE: Product Coverflow Carousel -->')
        
    p.write_text(text, encoding='utf-8')
    print("Fixed with fallback!")
