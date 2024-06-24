using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace gameofwords.game.Models
{
    [Keyless]
    [Table( "v_top_percent" )]
    public class TopPercent
    {
        [Column( "count" )]
        public int Count { get; set; }
        [Column( "name" )]
        public string Name { get; set; }
    }
}