using System.Diagnostics;

namespace LifecycleDemo.Middlewares;


public class LifecycleMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LifecycleMiddleware> _logger;

    public LifecycleMiddleware(RequestDelegate next, ILogger<LifecycleMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation("==============================================");
        _logger.LogInformation("[1] ВХОДЯЩИЙ ЗАПРОС");
        _logger.LogInformation("     Метод: {Method}", context.Request.Method);
        _logger.LogInformation("     Путь: {Path}", context.Request.Path);
        _logger.LogInformation("     Query: {Query}", context.Request.QueryString);
        _logger.LogInformation("     Content-Type: {ContentType}", context.Request.ContentType);
        _logger.LogInformation("     IP клиента: {IP}", context.Connection.RemoteIpAddress);
        _logger.LogInformation("     User-Agent: {UA}", context.Request.Headers["User-Agent"].ToString());
        _logger.LogInformation("     TraceIdentifier: {TraceId}", context.TraceIdentifier);

        _logger.LogInformation("[2] MIDDLEWARE PIPELINE — вход в следующий middleware");

        try
        {

            await _next(context);

            _logger.LogInformation("[2] MIDDLEWARE PIPELINE — выход из следующего middleware");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[5] ОШИБКА при обработке запроса");
            throw;
        }

        _logger.LogInformation("[3] ФОРМИРОВАНИЕ ОТВЕТА");
        _logger.LogInformation("     Status Code: {StatusCode}", context.Response.StatusCode);
        _logger.LogInformation("     Content-Type: {ContentType}", context.Response.ContentType);

        stopwatch.Stop();
        _logger.LogInformation("[4] ЗАВЕРШЕНИЕ ЗАПРОСА");
        _logger.LogInformation("     Время обработки: {Elapsed} мс", stopwatch.ElapsedMilliseconds);
        _logger.LogInformation("==============================================");
    }
}


public static class LifecycleMiddlewareExtensions
{
    public static IApplicationBuilder UseLifecycleLogger(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<LifecycleMiddleware>();
    }
}