# Restaurant Catalog Project

A comprehensive web application for browsing restaurants and their menus, built with modern .NET technologies.

## 📋 Project Overview

In this project, we develop a restaurant catalog where users can:
- Browse a list of restaurants
- View detailed information about each restaurant
- See which dishes each restaurant offers
- Search for restaurants by name using a search bar

## 🛠️ Technology Stack

- **ASP.NET Core 8** - Web framework
- **Razor Pages** - UI generation
- **MVC Architecture** - Design pattern
- **Entity Framework Core** - ORM for database operations
- **SQL Server** - Database management system
- **CSS** - Styling and responsive design

## 📖 Table of Contents

### Chapter 1: Introduction
Overview of the project requirements, architecture decisions, and technology selection. Sets up the development environment and project structure.

### Chapter 2: Restaurant List Application
Implementation of the main restaurant listing page with pagination and basic UI components.

### Chapter 3: Models
Definition of the data models:

```csharp
public class Restaurant
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public string PhoneNumber { get; set; }
    public ICollection<RestaurantDish> RestaurantDishes { get; set; }
}

public class Dish
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public ICollection<RestaurantDish> RestaurantDishes { get; set; }
}

public class RestaurantDish
{
    public int RestaurantId { get; set; }
    public Restaurant Restaurant { get; set; }
    public int DishId { get; set; }
    public Dish Dish { get; set; }
}
```

### Chapter 4: Context
DbContext configuration with relationships and model constraints:

```csharp
public class RestaurantContext : DbContext
{
    public DbSet<Restaurant> Restaurants { get; set; }
    public DbSet<Dish> Dishes { get; set; }
    public DbSet<RestaurantDish> RestaurantDishes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
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
    }
}
```

### Chapter 5: Connecting to SQL Server Database
Database configuration in `appsettings.json` and service registration:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=RestaurantCatalog;Trusted_Connection=True;TrustServerCertificate=true;"
  }
}
```

Service registration in `Program.cs`:
```csharp
builder.Services.AddDbContext<RestaurantContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

### Chapter 6: Controller
Implementation of the RestaurantController with actions for listing and details:

```csharp
public class RestaurantController : Controller
{
    private readonly RestaurantContext _context;

    public RestaurantController(RestaurantContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var restaurants = await _context.Restaurants.ToListAsync();
        return View(restaurants);
    }
}
```

### Chapter 7: Restaurant Details Page
Implementation of the details page with related dishes using EF Core eager loading:

**C# Code:**
```csharp
public async Task<IActionResult> Details(int id)
{
    var restaurant = await _context.Restaurants
        .Include(rd => rd.RestaurantDishes)
        .ThenInclude(d => d.Dish)
        .FirstOrDefaultAsync(x => x.Id == id);
        
    if (restaurant == null)
    {
        return NotFound();
    }
    
    return View(restaurant);
}
```

**Equivalent SQL Query:**
```sql
SELECT r.*, rd.*, d.*
FROM Restaurants AS r
LEFT JOIN RestaurantDishes AS rd ON r.Id = rd.RestaurantId
LEFT JOIN Dishes AS d ON rd.DishId = d.Id
WHERE r.Id = @id
```

**Razor View Example:**
```html
<div class="restaurant-details">
    <h2>@Model.Name</h2>
    <p>Address: @Model.Address</p>
    <p>Phone: @Model.PhoneNumber</p>
    
    <h3>Menu</h3>
    @foreach (var restaurantDish in Model.RestaurantDishes)
    {
        <div class="dish-item">
            <h4>@restaurantDish.Dish.Name</h4>
            <p>@restaurantDish.Dish.Description</p>
            <span class="price">$@restaurantDish.Dish.Price</span>
        </div>
    }
</div>
```

### Chapter 8: Sample Data in the Database
Database seeding with sample restaurants and dishes:

```csharp
public static void Initialize(IServiceProvider serviceProvider)
{
    using (var context = new RestaurantContext(
        serviceProvider.GetRequiredService<DbContextOptions<RestaurantContext>>()))
    {
        if (!context.Restaurants.Any())
        {
            context.Restaurants.AddRange(
                new Restaurant { Name = "Italian Bistro", Address = "123 Main St", PhoneNumber = "555-0123" },
                new Restaurant { Name = "Sushi Palace", Address = "456 Oak Ave", PhoneNumber = "555-0456" }
            );
            context.SaveChanges();
        }
    }
}
```

### Chapter 9: Search Bar Filter
Implementation of the search functionality:

**Controller Action:**
```csharp
public async Task<IActionResult> Index(string searchString)
{
    var restaurants = from r in _context.Restaurants
                     select r;

    if (!string.IsNullOrEmpty(searchString))
    {
        restaurants = restaurants.Where(r => r.Name.Contains(searchString));
    }

    return View(await restaurants.ToListAsync());
}
```

**Razor Page Search Form:**
```html
<form asp-controller="Restaurant" asp-action="Index" method="get">
    <div class="search-bar">
        <input type="text" name="searchString" placeholder="Search restaurants..." 
               value="@ViewData["CurrentFilter"]" />
        <button type="submit">Search</button>
    </div>
</form>
```

## 🚀 Getting Started

1. **Clone the repository**
2. **Update connection string** in `appsettings.json`
3. **Run migrations**: `dotnet ef database update`
4. **Run the application**: `dotnet run`

## 📊 Database Schema

The application uses a many-to-many relationship between Restaurants and Dishes through the junction table RestaurantDishes, allowing dishes to be associated with multiple restaurants and vice versa.

## 🔍 Features

- Complete CRUD operations for restaurants and dishes
- Efficient data loading with EF Core Include/ThenInclude
- Responsive design with CSS
- Search and filter functionality
- Clean MVC architecture with separation of concerns
