namespace MemoryGame.Helpers
{
    // Picks artwork for a card face from its name: an emoji when we know one,
    // otherwise a monogram. Hue is stable per name so both cards of a pair match.
    public static class CardArt
    {
        private static readonly Dictionary<string, string> Emojis = new(StringComparer.OrdinalIgnoreCase)
        {
            ["apple"] = "🍎", ["banana"] = "🍌", ["cherry"] = "🍒", ["grape"] = "🍇",
            ["lemon"] = "🍋", ["orange"] = "🍊", ["pear"] = "🍐", ["peach"] = "🍑",
            ["pineapple"] = "🍍", ["strawberry"] = "🍓", ["watermelon"] = "🍉", ["kiwi"] = "🥝",
            ["mango"] = "🥭", ["coconut"] = "🥥", ["avocado"] = "🥑", ["carrot"] = "🥕",
            ["cat"] = "🐱", ["dog"] = "🐶", ["mouse"] = "🐭", ["rabbit"] = "🐰",
            ["fox"] = "🦊", ["bear"] = "🐻", ["panda"] = "🐼", ["koala"] = "🐨",
            ["tiger"] = "🐯", ["lion"] = "🦁", ["cow"] = "🐮", ["pig"] = "🐷",
            ["frog"] = "🐸", ["monkey"] = "🐵", ["chicken"] = "🐔", ["penguin"] = "🐧",
            ["owl"] = "🦉", ["unicorn"] = "🦄", ["horse"] = "🐴", ["fish"] = "🐟",
            ["octopus"] = "🐙", ["turtle"] = "🐢", ["snake"] = "🐍", ["butterfly"] = "🦋",
            ["star"] = "⭐", ["heart"] = "❤️", ["moon"] = "🌙", ["sun"] = "☀️",
            ["rocket"] = "🚀", ["car"] = "🚗", ["ball"] = "⚽", ["pizza"] = "🍕",
            ["flower"] = "🌸", ["tree"] = "🌳", ["diamond"] = "💎", ["crown"] = "👑",
        };

        public static string? Emoji(string? name)
        {
            var key = (name ?? "").Trim();
            if (key.Length == 0) return null;
            if (Emojis.TryGetValue(key, out var e)) return e;
            // "Cats" -> "Cat"
            if (key.EndsWith('s') && Emojis.TryGetValue(key[..^1], out e)) return e;
            return null;
        }

        public static string Monogram(string? name)
        {
            var key = (name ?? "").Trim();
            return key.Length == 0 ? "?" : char.ToUpperInvariant(key[0]).ToString();
        }

        public static int Hue(string? name)
        {
            // Deterministic (string.GetHashCode is randomized per process).
            int h = 0;
            foreach (var c in (name ?? "").Trim().ToLowerInvariant())
                h = (h * 31 + c) % 360;
            return h;
        }
    }
}
