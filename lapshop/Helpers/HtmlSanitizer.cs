using System.Text.RegularExpressions;

namespace lapshop.Helpers
{
    public static class HtmlSanitizer
    {
        public static string Sanitize(string html)
        {
            if (string.IsNullOrEmpty(html)) return string.Empty;

            // Remove script tags and their content
            string sanitized = Regex.Replace(html, @"<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>", "", RegexOptions.IgnoreCase);

            // Remove inline event handlers (e.g., onload, onclick, onerror, etc.)
            sanitized = Regex.Replace(sanitized, @"\son\w+\s*=\s*""[^""]*""", "", RegexOptions.IgnoreCase);
            sanitized = Regex.Replace(sanitized, @"\son\w+\s*=\s*'[^']*'", "", RegexOptions.IgnoreCase);

            // Remove javascript: pseudo-protocol links
            sanitized = Regex.Replace(sanitized, @"href\s*=\s*""\s*javascript:[^""]*""", "href=\"#\"", RegexOptions.IgnoreCase);
            sanitized = Regex.Replace(sanitized, @"href\s*=\s*'\s*javascript:[^']*'", "href='#'", RegexOptions.IgnoreCase);

            return sanitized;
        }
    }
}
