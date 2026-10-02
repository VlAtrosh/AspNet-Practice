using Microsoft.EntityFrameworkCore;
using WebApplication3.Application.Common.Abstractions;
using WebApplication3.Application.Orders.CancelOrder;
using WebApplication3.Application.Orders.CancleOrder;
using WebApplication3.Application.Orders.CreateOrder;
using WebApplication3.Application.Orders.GetOrder;
using WebApplication3.Infrastructure;
using WebApplication3.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = Path.Combine(Directory.GetCurrentDirectory(), "Web")
});



// 1. БД — SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=app.db"));

// 2. Репозитории и UnitOfWork
builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// 3. Use Cases — ВСЕ три
builder.Services.AddScoped<CreateOrderUseCase>();
builder.Services.AddScoped<GetOrderUseCase>();
builder.Services.AddScoped<CancelOrderUseCase>();

// 4. MVC + ProblemDetails
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();
builder.Services.AddProblemDetails();


var app = builder.Build();

// 5. Автосоздание БД (вместо миграций)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

// 6. Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapControllers();

// Посмотреть список товаров с их Id
app.MapGet("/seed", async (AppDbContext db) =>
{
    if (!db.Products.Any())
    {
        db.Products.Add(new WebApplication3.Domain.Product("Ноутбук", 79990, 10));
        db.Products.Add(new WebApplication3.Domain.Product("Мышь", 1990, 50));
        db.Products.Add(new WebApplication3.Domain.Product("Кресло", 15990, 5));
    }
    if (!db.Customers.Any())
    {
        db.Customers.Add(new WebApplication3.Domain.Customer("vladosva99@gmail.com", "Влад"));
    }
    await db.SaveChangesAsync();
    return Results.Ok("Seed выполнен");
});

// Посмотреть список товаров с их Id
app.MapGet("/products-list", async (AppDbContext db) =>
    await db.Products.Select(p => new { p.Id, p.Name, p.Price, p.Stock }).ToListAsync());
app.Run();