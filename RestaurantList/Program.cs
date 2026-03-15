using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using RestaurantList.Data;
using System.Globalization;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information() // Set default minimum logging level
    //.WriteTo.Console() // Write logs to the console
    //.WriteTo.File("Logs/RestaurantList-Logs.txt", rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true) // Optional: write to a file
    .WriteTo.Seq("http://localhost:5341") // save the log into the SEQ database
    .CreateLogger();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<RestaurantListContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnectionString")));


var app = builder.Build();

// --- Global Culture Configuration ---
var defaultCulture = new CultureInfo("en-PK");
defaultCulture.NumberFormat.CurrencySymbol = "Rs."; // Sets the symbol specifically to Rs.


var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(defaultCulture),
    SupportedCultures = new List<CultureInfo> { defaultCulture },
    SupportedUICultures = new List<CultureInfo> { defaultCulture }
};
app.UseRequestLocalization(localizationOptions);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=RestaurantList}/{action=Index}/{id?}");

app.Run();
