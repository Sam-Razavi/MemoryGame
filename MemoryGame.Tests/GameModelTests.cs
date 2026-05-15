using MemoryGame.Models;

namespace MemoryGame.Tests;

public class GameModelTests
{
    [Fact]
    public void Game_DefaultStatus_IsWaiting()
    {
        var game = new Game();
        Assert.Equal("Waiting", game.Status);
    }

    [Fact]
    public void Game_EndedAt_IsNullByDefault()
    {
        var game = new Game();
        Assert.Null(game.EndedAt);
    }

    [Fact]
    public void Game_WinnerGamePlayerID_IsNullByDefault()
    {
        var game = new Game();
        Assert.Null(game.WinnerGamePlayerID);
    }

    [Fact]
    public void Card_DefaultName_IsEmptyString()
    {
        var card = new Card();
        Assert.Equal(string.Empty, card.Name);
    }

    [Fact]
    public void Card_ImagePath_IsNullByDefault()
    {
        var card = new Card();
        Assert.Null(card.ImagePath);
    }

    [Fact]
    public void Card_CanAssignProperties()
    {
        var card = new Card { CardID = 7, Name = "Apple", PairKey = "FRUIT_A" };
        Assert.Equal(7, card.CardID);
        Assert.Equal("Apple", card.Name);
        Assert.Equal("FRUIT_A", card.PairKey);
    }

    [Fact]
    public void Tile_IsMatched_IsFalseByDefault()
    {
        var tile = new Tile();
        Assert.False(tile.IsMatched);
    }
}
