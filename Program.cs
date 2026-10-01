using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MyMvcApp.Infrastructure;
using MyMvcApp.Middlewares;
using MyMvcApp.Services;
using MyMvcApp.Validators;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<IProductRepository, JsonProductRepository>();
builder.Services.AddValidatorsFromAssemblyContaining<ProductCreateValidator>();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddProblemDetails();
builder.Services.AddSingleton<ProblemDetailsFactory, CustomProblemDetailsFactory>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{

    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();


