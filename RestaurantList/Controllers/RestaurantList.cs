using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantList.Data;
using RestaurantList.Models;

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
                .FirstOrDefaultAsync(x => x.Id == id); //

            if (restaurant == null)
            {
                return NotFound();
            }

            // Get list of dishes NOT already assigned to this restaurant for the dropdown
            var assignedDishIds = restaurant.RestaurantDishes.Select(rd => rd.DishId).ToList();
            ViewBag.AvailableDishes = await _context.Dishes
                .Where(d => !assignedDishIds.Contains(d.Id))
                .ToListAsync();

            return View(restaurant);
        }

        // POST: RestaurantList/AddDish
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddDish(int restaurantId, int dishId)
        {
            var restaurantDish = new RestaurantDish //
            {
                RestaurantId = restaurantId,
                DishId = dishId
            };

            _context.RestaurantDishes.Add(restaurantDish); //
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = restaurantId });
        }

        // GET: RestaurantList/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: RestaurantList/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,ImageUrl,Address,Email,PhoneNumber,Description,CuisineType,Rating,OpeningTime,ClosingTime,IsOpenNow,PriceRange")] Restaurant restaurant)
        {
            if (ModelState.IsValid)
            {
                _context.Add(restaurant);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(restaurant);
        }

        // GET: RestaurantList/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var restaurant = await _context.Restaurants.FindAsync(id);
            if (restaurant == null)
            {
                return NotFound();
            }
            return View(restaurant);
        }

        // POST: RestaurantList/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,ImageUrl,Address,Email,PhoneNumber,Description,CuisineType,Rating,OpeningTime,ClosingTime,IsOpenNow,PriceRange")] Restaurant restaurant)
        {
            if (id != restaurant.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(restaurant);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RestaurantExists(restaurant.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(restaurant);
        }

        private bool RestaurantExists(int id)
        {
            return _context.Restaurants.Any(e => e.Id == id);
        }

        // GET: RestaurantList/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Find the restaurant to display its details before confirming deletion
            var restaurant = await _context.Restaurants
                .FirstOrDefaultAsync(m => m.Id == id);

            if (restaurant == null)
            {
                return NotFound();
            }

            return View(restaurant);
        }

        // POST: RestaurantList/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var restaurant = await _context.Restaurants.FindAsync(id);
            if (restaurant != null)
            {
                _context.Restaurants.Remove(restaurant);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}