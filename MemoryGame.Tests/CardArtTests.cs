using Xunit;
using MemoryGame.Helpers;

namespace MemoryGame.Tests;

public class CardArtTests
{
    [Theory]
    [InlineData("Apple", "🍎")]
    [InlineData("  cat ", "🐱")]
    [InlineData("Dogs", "🐶")]
    public void Emoji_KnownNames_ReturnArtwork(string name, string expected)
    {
        Assert.Equal(expected, CardArt.Emoji(name));
    }

    [Theory]
    [InlineData("Zebracorn")]
    [InlineData("")]
    [InlineData(null)]
    public void Emoji_UnknownNames_ReturnNull(string? name)
    {
        Assert.Null(CardArt.Emoji(name));
    }

    [Fact]
    public void Monogram_UsesFirstLetterOrQuestionMark()
    {
        Assert.Equal("Z", CardArt.Monogram("zebra"));
        Assert.Equal("?", CardArt.Monogram("  "));
    }

    [Fact]
    public void Hue_IsStableAndCaseInsensitive()
    {
        Assert.Equal(CardArt.Hue("Banana"), CardArt.Hue(" banana "));
        Assert.InRange(CardArt.Hue("Banana"), 0, 359);
    }
}
