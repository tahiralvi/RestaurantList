using System.ComponentModel.DataAnnotations;

namespace RestaurantList.Models
{
    public class Restaurant
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Image URL")]
        public string ImageUrl { get; set; }

        [Required]
        public string Address { get; set; }

        // --- New Business Logic Fields ---

        [EmailAddress]
        public string? Email { get; set; }

        [Phone]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        public string? Description { get; set; }

        [Display(Name = "Cuisine Type")]
        public string? CuisineType { get; set; } // e.g., Italian, Chinese, Pakistani

        [Range(1, 5)]
        public double Rating { get; set; } // Customer rating out of 5

        [Display(Name = "Opening Time")]
        [DataType(DataType.Time)]
        public TimeSpan OpeningTime { get; set; }

        [Display(Name = "Closing Time")]
        [DataType(DataType.Time)]
        public TimeSpan ClosingTime { get; set; }

        public bool IsOpenNow => DateTime.Now.TimeOfDay >= OpeningTime && DateTime.Now.TimeOfDay <= ClosingTime;

        [Display(Name = "Price Range")]
        public string? PriceRange { get; set; } // e.g., $, $$, $$$

        // Relationships
        public List<RestaurantDish>? RestaurantDishes { get; set; }
    }
}