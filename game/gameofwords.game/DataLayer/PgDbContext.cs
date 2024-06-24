using gameofwords.game.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;


namespace gameofwords.game.DataLayer
{
    public class PgDbContext : DbContext
    {
        public PgDbContext( DbContextOptions<PgDbContext> options ) : base( options )
        {
        }


        public DbSet<Users> Users { get; set; }
        public DbSet<Games> Games { get; set; }
        public DbSet<GamesWords> GamesWords { get; set; }
        public DbSet<GamesWordsTmp> GamesWordsTmp { get; set; }
        public DbSet<Words> Words { get; set; }
        public DbSet<WordsLinks> WordsLinks { get; set; }
        public DbSet<GamesDifficulty> GamesDifficulty { get; set; }
        public DbSet<TopWords> TopWords { get; set; }
        public DbSet<TopContainsWords> TopContainsWords { get; set; }
        public DbSet<TopGames> TopGames { get; set; }
        public DbSet<TopWins> TopWins { get; set; }
        public DbSet<TopPercent> TopPercent { get; set; }
    }
}
