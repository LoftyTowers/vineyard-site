using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace VineyardApi.Services
{
    internal static class RichTextSanitizer
    {
        public static string Sanitize(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return string.Empty;
            }

            var cleaned = StripDisallowedBlocks(html);
            cleaned = StripEventHandlers(cleaned);
            cleaned = SanitizeTags(cleaned);
            return cleaned;
        }

        private static string StripDisallowedBlocks(string html)
        {
            var withoutScripts = Regex.Replace(html, @"<\s*script[^>]*>.*?<\s*/\s*script\s*>", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            return Regex.Replace(withoutScripts, @"<\s*style[^>]*>.*?<\s*/\s*style\s*>", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        }

        private static string StripEventHandlers(string html)
        {
            return Regex.Replace(html, @"\son\w+\s*=\s*(['""]?).*?\1", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        }

        private static string SanitizeTags(string html)
        {
            var allowedTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "p", "br", "strong", "em", "a", "ul", "ol", "li", "h1", "h2", "h3"
            };

            return Regex.Replace(html, @"<\s*/?\s*([a-zA-Z0-9]+)([^>]*)>", match =>
            {
                var tag = match.Groups[1].Value;
                var isClosing = match.Value.StartsWith("</", StringComparison.Ordinal);
                if (!allowedTags.Contains(tag))
                {
                    return string.Empty;
                }

                if (isClosing)
                {
                    return $"</{tag}>";
                }

                if (string.Equals(tag, "br", StringComparison.OrdinalIgnoreCase))
                {
                    return "<br>";
                }

                if (!string.Equals(tag, "a", StringComparison.OrdinalIgnoreCase))
                {
                    return $"<{tag}>";
                }

                var href = ExtractHref(match.Groups[2].Value);
                if (string.IsNullOrWhiteSpace(href))
                {
                    return "<a>";
                }

                return $"<a href=\"{href}\">";
            }, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        }

        private static string? ExtractHref(string attributes)
        {
            var hrefMatch = Regex.Match(attributes, @"href\s*=\s*(['""])(.*?)\1", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (!hrefMatch.Success)
            {
                hrefMatch = Regex.Match(attributes, @"href\s*=\s*([^ \t\r\n""'>]+)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            }

            var href = hrefMatch.Success ? hrefMatch.Groups[hrefMatch.Groups.Count - 1].Value : null;
            if (string.IsNullOrWhiteSpace(href))
            {
                return null;
            }

            if (!Uri.TryCreate(href, UriKind.Absolute, out var uri))
            {
                return null;
            }

            if (uri.Scheme != "http" && uri.Scheme != "https" && uri.Scheme != "mailto")
            {
                return null;
            }

            return uri.ToString();
        }
    }
}
