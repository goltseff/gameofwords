using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace gameofwords.game.Models
{
    [Keyless]
    [Table( "v_top_contains_words" )]
    public class TopContainsWords
    {
        [Column( "count" )]
        public int Count { get; set; }
        [Column( "name" )]
        public string Name { get; set; }
        [Column( "description" )]
        public string Description { get; set; }
    }
}