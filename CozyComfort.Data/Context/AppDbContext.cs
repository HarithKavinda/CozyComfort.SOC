using Microsoft.EntityFrameworkCore;
using CozyComfort.Data.Models;

namespace CozyComfort.Data.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Blanket> Blankets { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Inventory Price precision
            modelBuilder.Entity<Inventory>()
                .Property(i => i.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Blanket>()
                .Property(b => b.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.UnitPrice)
                .HasPrecision(18, 2);
        }


    }
}
