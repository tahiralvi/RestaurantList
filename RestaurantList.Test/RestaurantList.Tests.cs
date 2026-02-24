csharp RestaurantList.Test\RestaurantListControllerTests.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RestaurantList.Controllers;
using RestaurantList.Data;
using RestaurantList.Models;

namespace RestaurantList.Test
{
    [TestClass]
    public class RestaurantListControllerTests
    {
        private static DbContextOptions<RestaurantListContext> CreateNewContextOptions(string dbName)
        {
            return new DbContextOptionsBuilder<RestaurantListContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
        }

        // Derived context allowing SaveChangesAsync to be forced to throw for concurrency tests
        private class TestRestaurantListContext : RestaurantListContext
        {
            private readonly bool _throwOnSave;

            public TestRestaurantListContext(DbContextOptions<RestaurantListContext> options, bool throwOnSave = false)
                : base(options)
            {
                _throwOnSave = throwOnSave;
            }

            public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            {
                if (_throwOnSave)
                {
                    throw new DbUpdateConcurrencyException();
                }

                return base.SaveChangesAsync(cancellationToken);
            }
        }

        [TestMethod]
        public async Task Index_NoSearch_ReturnsAllRestaurants()
        {
            var options = CreateNewContextOptions(nameof(Index_NoSearch_ReturnsAllRestaurants));
            using (var context = new RestaurantListContext(options))
            {
                context.Restaurants.Add(new Restaurant { Id = 1, Name = "A", ImageUrl = "u", Address = "a" });
                context.Restaurants.Add(new Restaurant { Id = 2, Name = "B", ImageUrl = "u", Address = "a" });
                await context.SaveChangesAsync();
            }

            using (var context = new RestaurantListContext(options))
            {
                var controller = new RestaurantList(context);
                var result = await controller.Index(null) as ViewResult;

                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result.Model, typeof(IEnumerable<Restaurant>));
                var model = (IEnumerable<Restaurant>)result.Model;
                Assert.AreEqual(2, model.Count());
            }
        }

        [TestMethod]
        public async Task Index_WithSearch_ReturnsFilteredRestaurants()
        {
            var options = CreateNewContextOptions(nameof(Index_WithSearch_ReturnsFilteredRestaurants));
            using (var context = new RestaurantListContext(options))
            {
                context.Restaurants.Add(new Restaurant { Id = 1, Name = "Pasta Palace", ImageUrl = "u", Address = "a" });
                context.Restaurants.Add(new Restaurant { Id = 2, Name = "Burger Barn", ImageUrl = "u", Address = "a" });
                await context.SaveChangesAsync();
            }

            using (var context = new RestaurantListContext(options))
            {
                var controller = new RestaurantList(context);
                var result = await controller.Index("Pasta") as ViewResult;

                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result.Model, typeof(IEnumerable<Restaurant>));
                var model = (IEnumerable<Restaurant>)result.Model;
                Assert.AreEqual(1, model.Count());
                Assert.AreEqual("Pasta Palace", model.First().Name);
            }
        }

        [TestMethod]
        public async Task Details_NullId_ReturnsNotFound()
        {
            var options = CreateNewContextOptions(nameof(Details_NullId_ReturnsNotFound));
            using var context = new RestaurantListContext(options);
            var controller = new RestaurantList(context);

            var result = await controller.Details(null);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Details_NotFound_ReturnsNotFound()
        {
            var options = CreateNewContextOptions(nameof(Details_NotFound_ReturnsNotFound));
            using var context = new RestaurantListContext(options);
            var controller = new RestaurantList(context);

            var result = await controller.Details(999);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Details_Found_ReturnsViewWithAvailableDishes()
        {
            var options = CreateNewContextOptions(nameof(Details_Found_ReturnsViewWithAvailableDishes));
            using (var context = new RestaurantListContext(options))
            {
                var r = new Restaurant { Id = 1, Name = "R", ImageUrl = "u", Address = "a" };
                var d1 = new Dish { Id = 1, Name = "Pizza", Price = 10 };
                var d2 = new Dish { Id = 2, Name = "Pasta", Price = 9 };
                context.Restaurants.Add(r);
                context.Dishes.AddRange(d1, d2);
                context.RestaurantDishes.Add(new RestaurantDish { RestaurantId = 1, DishId = 1 });
                await context.SaveChangesAsync();
            }

            using (var context = new RestaurantListContext(options))
            {
                var controller = new RestaurantList(context);
                var result = await controller.Details(1) as ViewResult;

                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result.Model, typeof(Restaurant));
                var model = (Restaurant)result.Model;
                var available = controller.ViewBag.AvailableDishes as List<Dish>;
                Assert.IsNotNull(available);
                Assert.AreEqual(1, available.Count);
                Assert.AreEqual(2, available.First().Id);
            }
        }

        [TestMethod]
        public async Task AddDish_AddsRestaurantDishAndRedirects()
        {
            var options = CreateNewContextOptions(nameof(AddDish_AddsRestaurantDishAndRedirects));
            using (var context = new RestaurantListContext(options))
            {
                context.Restaurants.Add(new Restaurant { Id = 1, Name = "R", ImageUrl = "u", Address = "a" });
                context.Dishes.Add(new Dish { Id = 2, Name = "Pasta", Price = 9 });
                await context.SaveChangesAsync();
            }

            using (var context = new RestaurantListContext(options))
            {
                var controller = new RestaurantList(context);
                var result = await controller.AddDish(1, 2) as RedirectToActionResult;

                Assert.IsNotNull(result);
                Assert.AreEqual(nameof(RestaurantList.Details), result.ActionName);
                Assert.AreEqual(1, result.RouteValues["id"]);
                Assert.AreEqual(1, context.RestaurantDishes.Count());
                var rd = context.RestaurantDishes.First();
                Assert.AreEqual(1, rd.RestaurantId);
                Assert.AreEqual(2, rd.DishId);
            }
        }

        [TestMethod]
        public void Create_Get_ReturnsView()
        {
            var options = CreateNewContextOptions(nameof(Create_Get_ReturnsView));
            using var context = new RestaurantListContext(options);
            var controller = new RestaurantList(context);

            var result = controller.Create() as ViewResult;

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task Create_Post_Valid_RedirectsAndAdds()
        {
            var options = CreateNewContextOptions(nameof(Create_Post_Valid_RedirectsAndAdds));
            using (var context = new RestaurantListContext(options))
            {
                var controller = new RestaurantList(context);
                var restaurant = new Restaurant { Id = 10, Name = "New", ImageUrl = "u", Address = "a" };

                var result = await controller.Create(restaurant) as RedirectToActionResult;

                Assert.IsNotNull(result);
                Assert.AreEqual(nameof(RestaurantList.Index), result.ActionName);

                // Confirm added
                Assert.AreEqual(1, context.Restaurants.Count());
                Assert.AreEqual("New", context.Restaurants.First().Name);
            }
        }

        [TestMethod]
        public async Task Create_Post_Invalid_ReturnsViewWithModel()
        {
            var options = CreateNewContextOptions(nameof(Create_Post_Invalid_ReturnsViewWithModel));
            using var context = new RestaurantListContext(options);
            var controller = new RestaurantList(context);
            controller.ModelState.AddModelError("Name", "Required");
            var restaurant = new Restaurant { Id = 11, Name = null, ImageUrl = "u", Address = "a" };

            var result = await controller.Create(restaurant) as ViewResult;

            Assert.IsNotNull(result);
            Assert.AreSame(restaurant, result.Model);
        }

        [TestMethod]
        public async Task Edit_Get_NullId_ReturnsNotFound()
        {
            var options = CreateNewContextOptions(nameof(Edit_Get_NullId_ReturnsNotFound));
            using var context = new RestaurantListContext(options);
            var controller = new RestaurantList(context);

            var result = await controller.Edit(null);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Edit_Get_NotFound_ReturnsNotFound()
        {
            var options = CreateNewContextOptions(nameof(Edit_Get_NotFound_ReturnsNotFound));
            using var context = new RestaurantListContext(options);
            var controller = new RestaurantList(context);

            var result = await controller.Edit(999);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Edit_Get_Found_ReturnsViewWithModel()
        {
            var options = CreateNewContextOptions(nameof(Edit_Get_Found_ReturnsViewWithModel));
            using (var context = new RestaurantListContext(options))
            {
                context.Restaurants.Add(new Restaurant { Id = 5, Name = "ToEdit", ImageUrl = "u", Address = "a" });
                await context.SaveChangesAsync();
            }

            using (var context = new RestaurantListContext(options))
            {
                var controller = new RestaurantList(context);
                var result = await controller.Edit(5) as ViewResult;

                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result.Model, typeof(Restaurant));
                var model = (Restaurant)result.Model;
                Assert.AreEqual(5, model.Id);
            }
        }

        [TestMethod]
        public async Task Edit_Post_IdMismatch_ReturnsNotFound()
        {
            var options = CreateNewContextOptions(nameof(Edit_Post_IdMismatch_ReturnsNotFound));
            using var context = new RestaurantListContext(options);
            var controller = new RestaurantList(context);
            var restaurant = new Restaurant { Id = 20, Name = "X", ImageUrl = "u", Address = "a" };

            var result = await controller.Edit(99, restaurant);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Edit_Post_ModelInvalid_ReturnsViewWithModel()
        {
            var options = CreateNewContextOptions(nameof(Edit_Post_ModelInvalid_ReturnsViewWithModel));
            using var context = new RestaurantListContext(options);
            var controller = new RestaurantList(context);
            controller.ModelState.AddModelError("Name", "Required");
            var restaurant = new Restaurant { Id = 21, Name = "X", ImageUrl = "u", Address = "a" };

            var result = await controller.Edit(21, restaurant) as ViewResult;

            Assert.IsNotNull(result);
            Assert.AreSame(restaurant, result.Model);
        }

        [TestMethod]
        public async Task Edit_Post_Valid_UpdatesAndRedirects()
        {
            var options = CreateNewContextOptions(nameof(Edit_Post_Valid_UpdatesAndRedirects));
            using (var context = new RestaurantListContext(options))
            {
                context.Restaurants.Add(new Restaurant { Id = 30, Name = "Old", ImageUrl = "u", Address = "a" });
                await context.SaveChangesAsync();
            }

            using (var context = new RestaurantListContext(options))
            {
                var controller = new RestaurantList(context);
                var updated = new Restaurant { Id = 30, Name = "Updated", ImageUrl = "u", Address = "a" };

                var result = await controller.Edit(30, updated) as RedirectToActionResult;

                Assert.IsNotNull(result);
                Assert.AreEqual(nameof(RestaurantList.Index), result.ActionName);

                var fromDb = context.Restaurants.Find(30);
                Assert.AreEqual("Updated", fromDb.Name);
            }
        }

        [TestMethod]
        public async Task Edit_Post_ConcurrencyThrows_RestaurantNotExist_ReturnsNotFound()
        {
            var options = CreateNewContextOptions(nameof(Edit_Post_ConcurrencyThrows_RestaurantNotExist_ReturnsNotFound));
            // create a context that will throw on save and that contains no restaurant with id 999
            using (var context = new TestRestaurantListContext(options, throwOnSave: true))
            {
                // intentionally do not add restaurant so RestaurantExists returns false
                var controller = new RestaurantList(context);
                var restaurant = new Restaurant { Id = 999, Name = "X", ImageUrl = "u", Address = "a" };

                var result = await controller.Edit(999, restaurant);

                Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            }
        }

        [TestMethod]
        public async Task Delete_Get_NullId_ReturnsNotFound()
        {
            var options = CreateNewContextOptions(nameof(Delete_Get_NullId_ReturnsNotFound));
            using var context = new RestaurantListContext(options);
            var controller = new RestaurantList(context);

            var result = await controller.Delete(null);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Delete_Get_NotFound_ReturnsNotFound()
        {
            var options = CreateNewContextOptions(nameof(Delete_Get_NotFound_ReturnsNotFound));
            using var context = new RestaurantListContext(options);
            var controller = new RestaurantList(context);

            var result = await controller.Delete(12345);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Delete_Get_Found_ReturnsViewWithModel()
        {
            var options = CreateNewContextOptions(nameof(Delete_Get_Found_ReturnsViewWithModel));
            using (var context = new RestaurantListContext(options))
            {
                context.Restaurants.Add(new Restaurant { Id = 40, Name = "ToDelete", ImageUrl = "u", Address = "a" });
                await context.SaveChangesAsync();
            }

            using (var context = new RestaurantListContext(options))
            {
                var controller = new RestaurantList(context);
                var result = await controller.Delete(40) as ViewResult;

                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result.Model, typeof(Restaurant));
                var model = (Restaurant)result.Model;
                Assert.AreEqual(40, model.Id);
            }
        }

        [TestMethod]
        public async Task DeleteConfirmed_RemovesRestaurantAndRedirects()
        {
            var options = CreateNewContextOptions(nameof(DeleteConfirmed_RemovesRestaurantAndRedirects));
            using (var context = new RestaurantListContext(options))
            {
                context.Restaurants.Add(new Restaurant { Id = 50, Name = "RemoveMe", ImageUrl = "u", Address = "a" });
                await context.SaveChangesAsync();
            }

            using (var context = new RestaurantListContext(options))
            {
                var controller = new RestaurantList(context);
                var result = await controller.DeleteConfirmed(50) as RedirectToActionResult;

                Assert.IsNotNull(result);
                Assert.AreEqual(nameof(RestaurantList.Index), result.ActionName);
                Assert.IsFalse(context.Restaurants.Any(r => r.Id == 50));
            }
        }
    }
}