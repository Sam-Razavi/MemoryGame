namespace MemoryGame.Models
{
    public class MoveVm
    {
        public int TurnNumber { get; set; }
        public int UserID { get; set; }
        public string Username { get; set; } = "";
        public bool IsMatch { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
