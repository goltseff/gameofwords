using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace gameofwords.game.Models
{
    [PrimaryKey( nameof( GameId ), nameof( WordId ) )]
    [Table( "games_words_tmp" )]
    public class GamesWordsTmp
    {
        [Column( "game_id" )]
        public int GameId { get; set; }
        [Column( "word_id" )]
        public int WordId { get; set; }
    }
}
