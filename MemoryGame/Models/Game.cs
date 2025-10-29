namespace MemoryGame.Models
{
    public class Game
    {
        public int GameID { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public string Status { get; set; } = "Waiting";
        public int? WinnerGamePlayerID { get; set; }
    }
}
