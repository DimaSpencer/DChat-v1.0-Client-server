using Microsoft.EntityFrameworkCore;
namespace Server
{
    public partial class DarkChatDbContext : DbContext
    {
        public virtual DbSet<User> Users { get; set; }
        public DarkChatDbContext()
        {
            //Database.EnsureDeleted();
            //Database.EnsureCreated();
        }
        public DarkChatDbContext(DbContextOptions<DarkChatDbContext> options)
            : base(options)
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=DarkChatDb;Trusted_Connection=True;");
            }
        }
    }
}
