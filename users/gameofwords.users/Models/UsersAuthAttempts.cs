using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace gameofwords.users.Models
{
    [Table( "users_auth_attempts" )]
    [PrimaryKey( nameof( UserId ), nameof( Datetime ) )]
    public class UsersAuthAttempts
    {
        [Column( "user_id")]
        public int UserId { get; set; }
        [Column( "datetime" )]
        public DateTime Datetime { get; set; }
        [Column( "is_success" )]
        public bool IsSuccess { get; set; }
    }
}
