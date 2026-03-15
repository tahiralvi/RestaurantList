using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantList.Data;
using RestaurantList.Models;
using Serilog;

namespace RestaurantList.Controllers
{
    public class DishesController : Controller
    {
        private readonly RestaurantListContext _context;

        public DishesController(RestaurantListContext context)
        {
            _context = context;
        }

        // GET: Dishes
        public async Task<IActionResult> Index()
        {
            Log.Information($"DishesController-->Index");
            return View(await _context.Dishes.ToListAsync());
        }

        // GET: Dishes/Create
        public IActionResult Create()
        {
            Log.Information($"DishesController-->Create");
            return View();
        }

        // POST: Dishes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Price")] Dish dish)
        {
            if (ModelState.IsValid)
            {
                Log.Information($"DishesController-->Create: {dish.Name +" , "+ dish.Price }");
                _context.Add(dish);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(dish);
        }

        // GET: Dishes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            Log.Information($"DishesController-->Edit: {id}");
            if (id == null) return NotFound();

            var dish = await _context.Dishes.FindAsync(id);
            Log.Information($"DishesController-->Edit: {dish.Id + " , " + dish.Name +", "+ dish.Price}");
            if (dish == null) return NotFound();

            return View(dish);
        }

        // POST: Dishes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Price")] Dish dish)
        {
            if (id != dish.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(dish);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Dishes.Any(e => e.Id == dish.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(dish);
        }
    }
}