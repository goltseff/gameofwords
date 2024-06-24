using System.ComponentModel.DataAnnotations.Schema;

namespace gameofwords.game.Models
{
    [Table( "words" )]
    public class Words
    {
        [Column( "id")]
        public int Id { get; set; }
        [Column( "name" )]
        public string Name { get; set; }
        [Column( "description" )]
        public string Description { get; set; }
    }
}
