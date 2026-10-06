using Microsoft.EntityFrameworkCore;

namespace SPCoEdit.Database
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<SPCoEditSessions> SPCoEditSessions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}
