using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace gameofwords.game.Models
{
    [Keyless]
    [Table( "v_top_wins" )]
    public class TopWins
    {
        [Column( "count" )]
        public int Count { get; set; }
        [Column( "name" )]
        public string Name { get; set; }
    }
}