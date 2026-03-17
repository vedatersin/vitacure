import re

file_path = r"c:\Users\vedat\Documents\GitHub\vitacure\Views\Home\Index.cshtml"
with open(file_path, "r", encoding="utf-8") as f:
    content = f.read()

# Remove CSS Part 1: From "Glow Dots" comment up to ".glow-circle { ... }"
pattern1 = r'\s*/\* Glow Dots / Molecules - Base setup.*?\.glow-circle \{\s*fill: #ffffff;\s*filter: url\(#circle-glow\);\s*\}\s*'
content = re.sub(pattern1, '\n', content, flags=re.DOTALL)

# Remove CSS Part 2: Node Positions
pattern2 = r'\s*/\* Node Positions \(Percentage based on 100% width/height of Hero\).*?\.n-sm4 \{ top: 12%; left: 65%; \}\s*'
content = re.sub(pattern2, '\n', content, flags=re.DOTALL)

# Remove HTML Part 3: Vitamin Network Background
pattern3 = r'\s*<!-- Vitamin Network Background \(Z-index: 1\) -->.*?</div>\s*(?=<h1 class="ag-title)'
content = re.sub(pattern3, '\n        ', content, flags=re.DOTALL)

# Clean up any leftover vitamin-network-container class rules
pattern4 = r'\s*/\* === Vitamin & Molecule Network Background === \*/.*?pointer-events: none;\s*/\* Üzerine tıklanmasını engelle \*/\s*\}\s*'
content = re.sub(pattern4, '\n', content, flags=re.DOTALL)

with open(file_path, "w", encoding="utf-8") as f:
    f.write(content)

print("Vitamin network removed successfully.")
