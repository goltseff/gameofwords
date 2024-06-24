using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace gameofwords.auth.Models
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
    }
}
