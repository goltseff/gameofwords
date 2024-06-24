using System.ComponentModel.DataAnnotations.Schema;

namespace gameofwords.users.Models
{
    [Table( "users_history" )]
    public class UsersHistory
    {
        [Column( "user_id")]
        public int UserId { get; set; }
        [Column( "user_login" )]
        public string UserLogin { get; set; }
        [Column( "action" )]
        public string Action { get; set; }
        [Column( "message" )]
        public string Message { get; set; }
        [Column( "who" )]
        public string Who { get; set; }
        [Column( "datetime" )]
        public DateTime Datetime { get; set; }
    }
}
