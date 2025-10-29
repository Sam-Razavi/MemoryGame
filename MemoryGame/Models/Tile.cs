namespace MemoryGame.Models
{
    public class Tile
    {
        public int TileID { get; set; }
        public int GameID { get; set; }
        public int CardID { get; set; }
        public int Position { get; set; }   // 0..15 for 4x4
        public bool IsMatched { get; set; }
        // convenience for rendering
        public string CardName { get; set; } = "";
        public string? ImagePath { get; set; }
    }
}
