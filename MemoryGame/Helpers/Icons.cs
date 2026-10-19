using Microsoft.AspNetCore.Html;

namespace MemoryGame.Helpers
{
    // Inline SVG icons (Lucide, ISC license) so the UI needs no icon font or CDN.
    public static class Icons
    {
        private static readonly Dictionary<string, string> Paths = new()
        {
            ["plus"] = "<path d=\"M5 12h14\"/><path d=\"M12 5v14\"/>",
            ["arrow-left"] = "<path d=\"m12 19-7-7 7-7\"/><path d=\"M19 12H5\"/>",
            ["arrow-right"] = "<path d=\"M5 12h14\"/><path d=\"m12 5 7 7-7 7\"/>",
            ["users"] = "<path d=\"M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2\"/><circle cx=\"9\" cy=\"7\" r=\"4\"/><path d=\"M22 21v-2a4 4 0 0 0-3-3.87\"/><path d=\"M16 3.13a4 4 0 0 1 0 7.75\"/>",
            ["user-plus"] = "<path d=\"M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2\"/><circle cx=\"9\" cy=\"7\" r=\"4\"/><path d=\"M19 8v6\"/><path d=\"M22 11h-6\"/>",
            ["clock"] = "<circle cx=\"12\" cy=\"12\" r=\"10\"/><path d=\"M12 6v6l4 2\"/>",
            ["trophy"] = "<path d=\"M6 9H4.5a2.5 2.5 0 0 1 0-5H6\"/><path d=\"M18 9h1.5a2.5 2.5 0 0 0 0-5H18\"/><path d=\"M4 22h16\"/><path d=\"M10 14.66V17c0 .55-.47.98-.97 1.21C7.85 18.75 7 20.24 7 22\"/><path d=\"M14 14.66V17c0 .55.47.98.97 1.21C16.15 18.75 17 20.24 17 22\"/><path d=\"M18 2H6v7a6 6 0 0 0 12 0V2Z\"/>",
            ["zap"] = "<path d=\"M13 2 3 14h9l-1 8 10-12h-9l1-8z\"/>",
            ["layers"] = "<path d=\"m12 2 10 5-10 5L2 7l10-5z\"/><path d=\"m2 17 10 5 10-5\"/><path d=\"m2 12 10 5 10-5\"/>",
            ["grid"] = "<rect width=\"7\" height=\"7\" x=\"3\" y=\"3\" rx=\"1\"/><rect width=\"7\" height=\"7\" x=\"14\" y=\"3\" rx=\"1\"/><rect width=\"7\" height=\"7\" x=\"14\" y=\"14\" rx=\"1\"/><rect width=\"7\" height=\"7\" x=\"3\" y=\"14\" rx=\"1\"/>",
            ["log-in"] = "<path d=\"M15 3h4a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2h-4\"/><path d=\"m10 17 5-5-5-5\"/><path d=\"M15 12H3\"/>",
            ["log-out"] = "<path d=\"M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4\"/><path d=\"m16 17 5-5-5-5\"/><path d=\"M21 12H9\"/>",
            ["pencil"] = "<path d=\"M17 3a2.85 2.83 0 1 1 4 4L7.5 20.5 2 22l1.5-5.5Z\"/>",
            ["trash"] = "<path d=\"M3 6h18\"/><path d=\"M19 6v14c0 1-1 2-2 2H7c-1 0-2-1-2-2V6\"/><path d=\"M8 6V4c0-1 1-2 2-2h4c1 0 2 1 2 2v2\"/>",
            ["eye"] = "<path d=\"M2 12s3-7 10-7 10 7 10 7-3 7-10 7-10-7-10-7Z\"/><circle cx=\"12\" cy=\"12\" r=\"3\"/>",
            ["play"] = "<polygon points=\"6 3 20 12 6 21 6 3\"/>",
            ["check"] = "<path d=\"M20 6 9 17l-5-5\"/>",
            ["x"] = "<path d=\"M18 6 6 18\"/><path d=\"m6 6 12 12\"/>",
            ["history"] = "<path d=\"M3 12a9 9 0 1 0 9-9 9.75 9.75 0 0 0-6.74 2.74L3 8\"/><path d=\"M3 3v5h5\"/><path d=\"M12 7v5l4 2\"/>",
            ["menu"] = "<path d=\"M4 6h16\"/><path d=\"M4 12h16\"/><path d=\"M4 18h16\"/>",
            ["sparkles"] = "<path d=\"m12 3-1.9 5.8a2 2 0 0 1-1.3 1.3L3 12l5.8 1.9a2 2 0 0 1 1.3 1.3L12 21l1.9-5.8a2 2 0 0 1 1.3-1.3L21 12l-5.8-1.9a2 2 0 0 1-1.3-1.3Z\"/>",
            ["radio"] = "<circle cx=\"12\" cy=\"12\" r=\"2\"/><path d=\"M16.24 7.76a6 6 0 0 1 0 8.49\"/><path d=\"M7.76 16.24a6 6 0 0 1 0-8.49\"/><path d=\"M19.07 4.93a10 10 0 0 1 0 14.14\"/><path d=\"M4.93 19.07a10 10 0 0 1 0-14.14\"/>",
            ["shield"] = "<path d=\"M20 13c0 5-3.5 7.5-7.66 8.95a1 1 0 0 1-.67-.01C7.5 20.5 4 18 4 13V6a1 1 0 0 1 1-1c2 0 4.5-1.2 6.24-2.72a1.17 1.17 0 0 1 1.52 0C14.51 3.81 17 5 19 5a1 1 0 0 1 1 1z\"/><path d=\"m9 12 2 2 4-4\"/>",
            ["alert"] = "<circle cx=\"12\" cy=\"12\" r=\"10\"/><path d=\"M12 8v4\"/><path d=\"M12 16h.01\"/>",
            ["info"] = "<circle cx=\"12\" cy=\"12\" r=\"10\"/><path d=\"M12 16v-4\"/><path d=\"M12 8h.01\"/>",
            ["inbox"] = "<path d=\"M22 12h-6l-2 3h-4l-2-3H2\"/><path d=\"M5.45 5.11 2 12v6a2 2 0 0 0 2 2h16a2 2 0 0 0 2-2v-6l-3.45-6.89A2 2 0 0 0 16.76 4H7.24a2 2 0 0 0-1.79 1.11z\"/>",
        };

        public static IHtmlContent Get(string name, int size = 18)
        {
            var paths = Paths.TryGetValue(name, out var p) ? p : "";
            return new HtmlString(
                $"<svg class=\"icon\" width=\"{size}\" height=\"{size}\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" " +
                $"stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\" aria-hidden=\"true\">{paths}</svg>");
        }
    }
}
