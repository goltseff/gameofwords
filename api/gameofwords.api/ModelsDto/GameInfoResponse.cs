using gameofwords.service;

namespace gameofwords.api.ModelsDto
{
    public class GameInfoResponse
    {
        public int WordId { get; set; }
        public int UserId { get; set; }
        public string Word { get; set; }
        public string WordDescription { get; set; }
        public string Difficulty { get; set; }
        public string UserNickName { get; set; }
        public bool IsFinished { get; set; }
        public bool IsUserWin { get; set; }
        public bool CanContinue { get; set; }
        public DateTime CreateDate { get; set; }
        public List<GameMoveItem> Moves { get; set; }
    }
    public class GameMoveItem
    {
        public int WordId { get; set; }
        public string Word { get; set; }
        public string WordDescription { get; set; }
        public bool isFromUser { get; set; }
        public DateTime CreateDate { get; set; }

    }
}
