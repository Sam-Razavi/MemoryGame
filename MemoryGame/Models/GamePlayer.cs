namespace MemoryGame.Models
{
    public class GamePlayer
    {
        public int GamePlayerID { get; set; }
        public int GameID { get; set; }
        public int UserID { get; set; }
        public int PlayerOrder { get; set; } // 1 or 2
    }
}
