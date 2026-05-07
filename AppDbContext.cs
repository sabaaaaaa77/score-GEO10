using Microsoft.EntityFrameworkCore;
using SCORE.Models;

namespace SCORE
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // ჩვენი ცხრილები ბაზაში
        public DbSet<League> Leagues { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Match> Matches { get; set; }

        // მხოლოდ ეს ერთი ხაზი დატოვე Standings-ისთვის:
        public DbSet<Standing> Standings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Match>()
                .HasOne(m => m.HomeTeam)
                .WithMany()
                .HasForeignKey(m => m.HomeTeamId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Match>()
                .HasOne(m => m.AwayTeam)
                .WithMany()
                .HasForeignKey(m => m.AwayTeamId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
