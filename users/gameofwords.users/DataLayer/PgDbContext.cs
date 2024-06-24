using gameofwords.users.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;


namespace gameofwords.users.DataLayer
{
    public class PgDbContext : DbContext
    {
        public PgDbContext( DbContextOptions<PgDbContext> options ) : base( options )
        {
        }


        public DbSet<Users> Users { get; set; }
        public DbSet<UsersHistory> UsersHistory { get; set; }

        protected override void OnModelCreating( ModelBuilder modelBuilder )
        {
            base.OnModelCreating( modelBuilder );

            try
            {
                modelBuilder.Entity<UsersHistory>( ).HasNoKey( );
            }
            catch(Exception)
            {
                throw;
            }
        }

    }
}
