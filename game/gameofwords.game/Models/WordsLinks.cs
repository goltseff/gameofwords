using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace gameofwords.game.Models
{
    [Table( "words_links" )]
    [PrimaryKey( nameof( WordId ), nameof( ContainsWordId ) )]
    public class WordsLinks
    {
        [Column( "word_id" )]
        public int WordId { get; set; }
        [Column( "contains_word_id" )]
        public int ContainsWordId { get; set; }
    }
}
