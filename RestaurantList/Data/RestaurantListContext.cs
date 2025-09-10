using Microsoft.EntityFrameworkCore;
using RestaurantList.Models;

namespace RestaurantList.Data
{
    public class RestaurantListContext: DbContext
    {
        public RestaurantListContext(DbContextOptions<RestaurantListContext> options) : base(options)
        {
        }

        protected RestaurantListContext()
        {
        }

        public DbSet<Restaurant> Restaurants { get; set; }
        public DbSet<Dish> Dishes { get; set; }
        public DbSet<RestaurantDish> RestaurantDishes { get; set; }

        override protected void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RestaurantDish>()
                .HasKey(rd => new { rd.RestaurantId, rd.DishId });
            modelBuilder.Entity<RestaurantDish>()
                .HasOne(rd => rd.Restaurant)
                .WithMany(r => r.RestaurantDishes)
                .HasForeignKey(rd => rd.RestaurantId);
            modelBuilder.Entity<RestaurantDish>()
                .HasOne(rd => rd.Dish)
                .WithMany(d => d.RestaurantDishes)
                .HasForeignKey(rd => rd.DishId);
            modelBuilder.Entity<Restaurant>().HasData(
                new Restaurant 
                { 
                    Id = 1, 
                    Name = "Pasta Palace", 
                    ImageUrl = "https://example.com/images/pasta_palace.jpg", 
                    Address = "123 Noodle St, Flavor Town" }
                );
            modelBuilder.Entity<Dish>().HasData(
                new Dish { Id = 1, Name = "Pizza", Price = 10 },
                new Dish { Id = 2, Name = "Pasta", Price = 9 }
                );

            base.OnModelCreating(modelBuilder);
        }
    }
}
