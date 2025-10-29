namespace MemoryGame.Models
{
    public class PlayerVm
    {
        public int GamePlayerID { get; set; }
        public int UserID { get; set; }
        public int PlayerOrder { get; set; }
        public string Username { get; set; } = "";
    }
}
