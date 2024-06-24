using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace gameofwords.game.Models
{
    [Table( "games_words" )]
    [PrimaryKey( nameof( GameId ), nameof( WordId ) )]
    public class GamesWords
    {
        [Column( "game_id" )]
        public int GameId { get; set; }
        [Column( "word_id" )]
        public int WordId { get; set; }
        [Column( "is_from_user" )]
        public bool IsFromUser { get; set; }
        [Column( "create_date" )]
        public DateTime CreateDate { get; set; }
    }
}
