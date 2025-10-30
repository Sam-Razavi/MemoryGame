namespace MemoryGame.Models
{
    public class LobbyRowVm
    {
        public int GameID { get; set; }
        public string Status { get; set; } = "Waiting";
        public DateTime CreatedAt { get; set; }

        public int PlayerCount { get; set; }
        public bool IsParticipant { get; set; }   // current user is in this game
        public bool IsOwner { get; set; }         // current user is PlayerOrder=1

        // Convenience flags
        public bool CanJoin => Status != "Completed" && PlayerCount < 2 && !IsParticipant;
        public bool CanPlay => IsParticipant;     // show Play only to participants
    }
}
