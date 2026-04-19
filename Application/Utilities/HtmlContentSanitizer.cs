using System.Net;
using System.Text.RegularExpressions;

namespace vitacure.Application.Utilities;

public static partial class HtmlContentSanitizer
{
    private static readonly HashSet<string> AllowedTags = new(StringComparer.OrdinalIgnoreCase)
    {
        "p", "br", "strong", "b", "em", "i", "u", "ul", "ol", "li", "a", "h3", "h4", "blockquote"
    };

    public static string Sanitize(string? rawHtml)
    {
        if (string.IsNullOrWhiteSpace(rawHtml))
        {
            return string.Empty;
        }

        var html = rawHtml.Trim();
        html = ScriptOrStyleRegex().Replace(html, string.Empty);
        html = HtmlCommentRegex().Replace(html, string.Empty);
        html = OnAttributeRegex().Replace(html, string.Empty);
        html = StyleAttributeRegex().Replace(html, string.Empty);
        html = HrefJavascriptRegex().Replace(html, " href=\"#\"");
        html = TagRegex().Replace(html, SanitizeTag);
        html = EmptyParagraphRegex().Replace(html, string.Empty);

        return html.Trim();
    }

    public static string StripHtml(string? rawHtml)
    {
        if (string.IsNullOrWhiteSpace(rawHtml))
        {
            return string.Empty;
        }

        var withoutTags = Regex.Replace(rawHtml, "<[^>]+>", " ");
        var decoded = WebUtility.HtmlDecode(withoutTags);
        return Regex.Replace(decoded, "\\s+", " ").Trim();
    }

    private static string SanitizeTag(Match match)
    {
        var tag = match.Groups["tag"].Value;
        if (!AllowedTags.Contains(tag))
        {
            return string.Empty;
        }

        var isClosing = match.Value.StartsWith("</", StringComparison.Ordinal);
        if (isClosing)
        {
            return $"</{tag.ToLowerInvariant()}>";
        }

        if (string.Equals(tag, "a", StringComparison.OrdinalIgnoreCase))
        {
            var hrefMatch = Regex.Match(match.Value, "href\\s*=\\s*([\"'])(?<href>.*?)\\1", RegexOptions.IgnoreCase);
            var hrefValue = hrefMatch.Success ? hrefMatch.Groups["href"].Value.Trim() : "#";

            if (!(hrefValue.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                  hrefValue.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                  hrefValue.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase) ||
                  hrefValue.StartsWith("/", StringComparison.OrdinalIgnoreCase) ||
                  hrefValue.StartsWith("#", StringComparison.OrdinalIgnoreCase)))
            {
                hrefValue = "#";
            }

            return $"<a href=\"{WebUtility.HtmlEncode(hrefValue)}\" rel=\"noopener noreferrer\">";
        }

        return $"<{tag.ToLowerInvariant()}>";
    }

    [GeneratedRegex("<(script|style)[^>]*?>.*?</\\1>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex ScriptOrStyleRegex();

    [GeneratedRegex("<!--.*?-->", RegexOptions.Singleline)]
    private static partial Regex HtmlCommentRegex();

    [GeneratedRegex("\\son\\w+\\s*=\\s*([\"']).*?\\1", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex OnAttributeRegex();

    [GeneratedRegex("\\sstyle\\s*=\\s*([\"']).*?\\1", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex StyleAttributeRegex();

    [GeneratedRegex("\\shref\\s*=\\s*([\"'])\\s*javascript:.*?\\1", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex HrefJavascriptRegex();

    [GeneratedRegex("<\\s*/?\\s*(?<tag>[a-z0-9]+)(?:\\s[^>]*)?>", RegexOptions.IgnoreCase)]
    private static partial Regex TagRegex();

    [GeneratedRegex("<p>\\s*</p>", RegexOptions.IgnoreCase)]
    private static partial Regex EmptyParagraphRegex();
}
