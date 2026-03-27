$filePath = "c:\Users\vedat\Documents\GitHub\vitacure\Views\Home\Index.cshtml"
$text = [System.IO.File]::ReadAllText($filePath, [System.Text.Encoding]::UTF8)

$replacements = @{
    'Ä±' = 'ı'
    'Ä°' = 'İ'
    'ÄŸ' = 'ğ'
    'Äž' = 'Ğ'
    'Ã¼' = 'ü'
    'Ãœ' = 'Ü'
    'ÅŸ' = 'ş'
    'Åž' = 'Ş'
    'Ã§' = 'ç'
    'Ã‡' = 'Ç'
    'Ã¶' = 'ö'
    'Ã–' = 'Ö'
    'â˜…' = '★'
    'â‚º' = '₺'
    'â€¢' = '•'
    'â€“' = '–'
    'â€”' = '—'
    'â€œ' = '“'
    'â€ ' = '”'
    'â€˜' = '‘'
    'â€™' = '’'
}

foreach ($key in $replacements.Keys) {
    $text = $text.Replace($key, $replacements[$key])
}

[System.IO.File]::WriteAllText($filePath, $text, [System.Text.Encoding]::UTF8)
Write-Output "Done replacing."
