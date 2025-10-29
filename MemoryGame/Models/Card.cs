namespace MemoryGame.Models
{
    public class Card
    {
        public int CardID { get; set; }
        public string Name { get; set; } = "";
        public string? ImagePath { get; set; }
        public string PairKey { get; set; } = "";
    }
}
