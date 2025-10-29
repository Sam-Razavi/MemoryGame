namespace MemoryGame.Models
{
    public class Move
    {
        public int MoveID { get; set; }
        public int GameID { get; set; }
        public int UserID { get; set; }
        public int FirstTileID { get; set; }
        public int SecondTileID { get; set; }
        public bool IsMatch { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TurnNumber { get; set; }
    }
}
