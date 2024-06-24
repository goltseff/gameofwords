using System.ComponentModel.DataAnnotations.Schema;

namespace gameofwords.game.Models
{
    [Table( "games" )]
    public class Games
    {
        [Column( "id" )]
        public int Id { get; set; }
        [Column( "word_id" )]
        public int WordId { get; set; }
        [Column( "difficulty_id" )]
        public int DifficultyId { get; set; }
        [Column( "is_finished" )]
        public bool IsFinished { get; set; }
        [Column( "is_user_win" )]
        public bool IsUserWin { get; set; }
        [Column( "user_id" )]
        public int UserId { get; set; }
        [Column( "create_date" )]
        public DateTime CreateDate { get; set; }
    }
}