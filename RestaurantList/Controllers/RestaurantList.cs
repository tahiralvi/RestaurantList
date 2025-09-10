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
        public async Task<IActionResult> Index()
        {
            return View(await _context.Restaurants.ToListAsync());
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
