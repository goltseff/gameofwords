using System.ComponentModel.DataAnnotations.Schema;

namespace gameofwords.game.Models
{
    [Table( "games_difficulty" )]
    public class GamesDifficulty
    {
        [Column( "id" )]
        public int Id { get; set; }
        [Column( "percent" )]
        public int Percent { get; set; }
        [Column( "name" )]
        public string Name { get; set; }
    }
}