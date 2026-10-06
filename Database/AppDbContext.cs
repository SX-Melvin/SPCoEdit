using Microsoft.EntityFrameworkCore;

namespace SPCoEdit.Database
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<SPCoEditSessions> SPCoEditSessions { get; set; }
        public DbSet<DTreeCore> DTreeCore { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DTreeCore>().HasNoKey();
            modelBuilder.Entity<DTreeCore>().ToTable(nameof(DTreeCore), t => t.ExcludeFromMigrations());
        }
    }
}
