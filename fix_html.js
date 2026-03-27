const fs = require('fs');
const filepath = 'c:/Users/vedat/Documents/GitHub/vitacure/Views/Home/Index.cshtml';
let data = fs.readFileSync(filepath, 'utf-8');

const regex = /<div class="uyku-tag-cloud">\s*<!-- RIGHT SIDE: Product Coverflow Carousel -->/s;

const replacement = `<div class="uyku-tag-cloud">
                            <button type="button" class="uyku-tag-btn active"><i class="fa-solid fa-layer-group text-warning"></i> Tümü</button>
                            <button type="button" class="uyku-tag-btn"><i class="fa-solid fa-moon text-primary"></i> Melatonin</button>
                            <button type="button" class="uyku-tag-btn"><i class="fa-solid fa-leaf text-success"></i> Bitkisel Ekstreler</button>
                            <button type="button" class="uyku-tag-btn"><i class="fa-solid fa-child text-info"></i> Çocuklar İçin</button>
                        </div>
                    </div>

                    <!-- RIGHT SIDE: Product Coverflow Carousel -->`;

if (regex.test(data)) {
    data = data.replace(regex, replacement);
    fs.writeFileSync(filepath, data, 'utf-8');
    console.log("SUCCESS");
} else {
    console.log("REGEX_FAILED");
}
