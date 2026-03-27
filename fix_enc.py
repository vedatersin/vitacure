import sys

def fix_encoding(text):
    # Instead of encoding/decoding which might fail on some undefined characters,
    # we can explicitly map the corrupted sequences back to their original characters.
    replacements = {
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
        'â€': '”',
        'â€˜': '‘',
        'â€™': '’'
    }
    
    for corrupted, original in replacements.items():
        text = text.replace(corrupted, original)
    
    return text

def main():
    path = r"c:\Users\vedat\Documents\GitHub\vitacure\Views\Home\Index.cshtml"
    with open(path, "r", encoding="utf-8") as f:
        content = f.read()

    fixed_content = fix_encoding(content)

    print(f"Fixed 'Ä±' count: {content.count('Ä±')}")
    print(f"Fixed 'â˜…' count: {content.count('â˜…')}")
    print(f"Fixed 'â‚º' count: {content.count('â‚º')}")

    with open(path, "w", encoding="utf-8") as f:
        f.write(fixed_content)
        
    print("Fix completed.")

if __name__ == "__main__":
    main()
