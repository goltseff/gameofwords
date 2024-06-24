using gameofwords.auth.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;


namespace gameofwords.auth.DataLayer
{
    public class PgDbContext : DbContext
    {
        public PgDbContext( DbContextOptions<PgDbContext> options ) : base( options )
        {
        }


        public DbSet<Users> Users { get; set; }
 
    }
}
