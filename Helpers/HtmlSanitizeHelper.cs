using Ganss.Xss;

namespace ProjectManagement.App.Helpers
{
    public static class HtmlSanitizeHelper
    {
        private static readonly HtmlSanitizer _sanitizer;

        static HtmlSanitizeHelper()
        {
            _sanitizer = new HtmlSanitizer();

            // Allowed tags (RTE-friendly)
            _sanitizer.AllowedTags.Add("p");
            _sanitizer.AllowedTags.Add("br");
            _sanitizer.AllowedTags.Add("strong");
            _sanitizer.AllowedTags.Add("em");
            _sanitizer.AllowedTags.Add("u");
            _sanitizer.AllowedTags.Add("s");
            _sanitizer.AllowedTags.Add("ul");
            _sanitizer.AllowedTags.Add("ol");
            _sanitizer.AllowedTags.Add("li");
            _sanitizer.AllowedTags.Add("a");
            _sanitizer.AllowedTags.Add("img");
            _sanitizer.AllowedTags.Add("span");
            _sanitizer.AllowedTags.Add("div");

            // Allowed attributes
            _sanitizer.AllowedAttributes.Add("href");
            _sanitizer.AllowedAttributes.Add("src");
            _sanitizer.AllowedAttributes.Add("alt");
            _sanitizer.AllowedAttributes.Add("title");
            _sanitizer.AllowedAttributes.Add("style"); // optional

            // Allowed CSS (OPTIONAL but recommended)
            _sanitizer.AllowedCssProperties.Add("color");
            _sanitizer.AllowedCssProperties.Add("font-weight");
            _sanitizer.AllowedCssProperties.Add("font-style");
            _sanitizer.AllowedCssProperties.Add("text-decoration");
            _sanitizer.AllowedCssProperties.Add("text-align");

            // Block javascript: links
            _sanitizer.AllowedSchemes.Add("http");
            _sanitizer.AllowedSchemes.Add("https");
            _sanitizer.AllowedSchemes.Add("data"); // for base64 images (optional)
        }

        public static string Sanitize(string html)
        {
            return string.IsNullOrWhiteSpace(html)
                ? string.Empty
                : _sanitizer.Sanitize(html);
        }
    }
}
