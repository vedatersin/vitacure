using vitacure.Application.Utilities;

namespace vitacure.Tests;

public class HtmlContentSanitizerTests
{
    [Fact]
    public void Sanitize_Removes_Script_And_Unsafe_Attributes()
    {
        var rawHtml = """
            <p onclick="alert('x')" style="color:red">Merhaba <strong>Dunya</strong></p>
            <script>alert('x')</script>
            <a href="javascript:alert('x')" onmouseover="alert('x')">Tikla</a>
            """;

        var result = HtmlContentSanitizer.Sanitize(rawHtml);

        Assert.Contains("<p>Merhaba <strong>Dunya</strong></p>", result, StringComparison.Ordinal);
        Assert.Contains("<a href=\"#\" rel=\"noopener noreferrer\">Tikla</a>", result, StringComparison.Ordinal);
        Assert.DoesNotContain("script", result, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("onclick", result, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("style=", result, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("javascript:", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void StripHtml_Returns_Decoded_Plain_Text()
    {
        var result = HtmlContentSanitizer.StripHtml("<p>Vitamin &amp; Mineral <strong>Dengesi</strong></p>");

        Assert.Equal("Vitamin & Mineral Dengesi", result);
    }
}
