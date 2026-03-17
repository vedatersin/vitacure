const fs = require('fs');

const filePath = 'Views/Home/Index.cshtml';
let content = fs.readFileSync(filePath, 'utf8');

// Remove Vitamin Network CSS 1
content = content.replace(/\s*\/\* === Vitamin & Molecule Network Background === \*\/(.|\n)*?pointer-events: none; \/\* Üzerine tıklanmasını engelle \*\/\s*\}/g, '');
content = content.replace(/\s*\/\* Glow Dots \/ Molecules - Base setup(.|\n)*?\.glow-circle \{\s*fill: #ffffff;\s*filter: url\(#circle-glow\);\s*\}/g, '');

// Remove Node Positions mapped lines
content = content.replace(/\s*\/\* Node Positions \((.|\n)*?\.n-sm4 \{ top: 12%; left: 65%; \}/g, '');

// Remove HTML structure
content = content.replace(/\s*<!-- Vitamin Network Background \(Z-index: 1\) -->(.|\n)*?<\/div>\s*<h1 class="ag-title/g, '\n\n        <h1 class="ag-title');

fs.writeFileSync(filePath, content, 'utf8');
console.log('Cleaned Vitamin Network Successfully');
