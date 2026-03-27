const fs = require('fs');
const path = require('path');

const filePath = path.join('c:', 'Users', 'vedat', 'Documents', 'GitHub', 'vitacure', 'Views', 'Home', 'Index.cshtml');
let text = fs.readFileSync(filePath, 'utf8');

const replacements = {
    'Ä±': 'ı',
    'Ä°': 'İ',
    'ÄŸ': 'ğ',
    'Äž': 'Ğ',
    'Ã¼': 'ü',
    'Ãœ': 'Ü',
    'ÅŸ': 'ş',
    'Åž': 'Ş',
    'Ã§': 'ç',
    'Ã‡': 'Ç',
    'Ã¶': 'ö',
    'Ã–': 'Ö',
    'â˜…': '★',
    'â‚º': '₺',
    'â€¢': '•',
    'â€“': '–',
    'â€”': '—',
    'â€œ': '“',
    'â€ ': '”',
    'â€˜': '‘',
    'â€™': '’'
};

for (const [key, value] of Object.entries(replacements)) {
    text = text.split(key).join(value);
}

fs.writeFileSync(filePath, text, 'utf8');
console.log('Done replacing via Node.js.');
