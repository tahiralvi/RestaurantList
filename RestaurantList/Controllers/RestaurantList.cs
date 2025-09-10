using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantList.Data;

namespace RestaurantList.Controllers
{
    public class RestaurantList : Controller
    {
        private readonly RestaurantListContext _context;

        public RestaurantList(RestaurantListContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(string searchString)
        {
            var restaurants = from r in _context.Restaurants
                              select r;
            if (!string.IsNullOrEmpty(searchString))
            {
                restaurants = restaurants.Where(r =>
                r.Name.Contains(searchString));
                return View(await restaurants.ToListAsync());
            }
            return View(await restaurants.ToListAsync());
        }
        public async Task<IActionResult> Details(int? id)
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
    }
}
