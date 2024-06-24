using System.ComponentModel.DataAnnotations.Schema;

namespace gameofwords.game.Models
{
    [Table( "users" )]
    public class Users
    {
        [Column( "id")]
        public int Id { get; set; }
        [Column( "email" )]
        public string Email { get; set; }
        [Column( "login" )]
        public string Login { get; set; }
        [Column( "password" )]
        public string Password { get; set; }
        [Column( "nick_name" )]
        public string Nickname { get; set; }
        [Column( "is_admin" )]
        public bool IsAdmin { get; set; }
        [Column( "is_bot" )]
        public bool IsBot { get; set; }
        [Column( "updated_by" )]
        public string UpdatedBy { get; set; }
    }
}
