using Microsoft.EntityFrameworkCore;

namespace Shop.ReadModel.Context
{
    public class ShopDbContext : DbContext
    {
        public ShopDbContext(DbContextOptions<ShopDbContext> options)
            :base(options)
        {
            
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>().HasKey(o => o.Id);
            modelBuilder.Entity<Order>().HasIndex(o => o.Number);
            modelBuilder.Entity<OrderItem>().HasKey(o => new { o.OrderId, o.NumberInOrder});
            modelBuilder.Entity<User>().HasKey(o =>  o.Id);
            modelBuilder.Entity<User>().HasIndex(o =>  o.Login);
            modelBuilder.Entity<Good>().HasIndex(o =>  o.Id);
            modelBuilder.Entity<Good>().HasIndex(o =>  o.Name);
        }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Good> Goods { get; set; }
    }
}