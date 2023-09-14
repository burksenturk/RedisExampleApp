using Microsoft.EntityFrameworkCore;

namespace RedisExampleApp.API.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        //bu ctor u inşa etmemin nedeni AppDbContext in configuration ını Program.cs tarafında vermek istiyorum.
        {
            
        }

        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Seed
            modelBuilder.Entity<Product>().HasData(
                new Product()
                {
                    Id = 1,
                    Name = "Kalem 1",
                },
                new Product()
                {
                    Id = 2,
                    Name = "Kalem 2",
                },
                new Product()
                {
                    Id = 3,
                    Name = "Kalem 3",
                });
        }
    }
}
