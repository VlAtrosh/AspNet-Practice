using LifecycleDemo.Middlewares;
using Serilog;
using Microsoft.AspNetCore.OpenApi;

var builder = WebApplication.CreateBuilder(args);


builder.Host.UseSerilog((context, config) =>
{
    config
        .MinimumLevel.Information()
        .WriteTo.Console()
        .WriteTo.File("logs/lifecycle-.txt",
            rollingInterval: RollingInterval.Day,
            outputTemplate: "{Timestamp:HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}");
});


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 3.1. Обработка исключений (самый первый — ловит всё)
app.UseExceptionHandler("/error");

// 3.2. Наш кастомный middleware для жизненного цикла
app.UseLifecycleLogger();

// 3.3. Swagger (только в разработке)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 3.4. Статические файлы (HTML, CSS, JS)
app.UseDefaultFiles();   // index.html по умолчанию
app.UseStaticFiles();    // отдаёт файлы из wwwroot


// 4.1. Эндпоинт для приёма анкеты
app.MapPost("/api/anketa", (AnketaRequest request, ILogger<Program> logger) =>
{
    logger.LogInformation("📥 Получена анкета: {FirstName} {LastName}, {Age} лет, {Email}",
        request.FirstName, request.LastName, request.Age, request.Email);

    // Простая валидация
    if (string.IsNullOrWhiteSpace(request.FirstName))
        return Results.BadRequest(new { error = "Имя обязательно" });

    if (request.Age < 1 || request.Age > 120)
        return Results.BadRequest(new { error = "Возраст от 1 до 120" });

    // Здесь можно сохранить в БД
    logger.LogInformation("Анкета успешно обработана");

    return Results.Ok(new
    {
        message = $"Спасибо, {request.FirstName}! Ваша заявка принята.",
        data = request
    });
})
.WithName("SubmitAnketa")
.WithOpenApi();

// 4.2. Эндпоинт для ошибки
app.MapGet("/error", () => Results.Problem("Произошла ошибка"));

// 4.3. Тестовый эндпоинт, который бросает исключение (для демонстрации этапа 5)
app.MapGet("/throw", () =>
{
    throw new InvalidOperationException("Тестовое исключение для демонстрации");
});


app.Run();

public record AnketaRequest(
    string FirstName,
    string LastName,
    int Age,
    string Email,
    string? About
);