using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);


// 1. Swagger + OpenAPI Info

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Todo Management API",
        Version = "v1",
        Description = "API для управления задачами. Поддерживает фильтрацию и создание.",
        Contact = new OpenApiContact
        {
            Name = "API Support",
            Email = "support@example.com"
        },
        License = new OpenApiLicense
        {
            Name = "MIT",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });
});

var app = builder.Build();


// 2. Swagger UI

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Todo API v1");
        c.RoutePrefix = "swagger";
    });
}


// 3. In-memory хранилище

var todos = new List<TodoItem>
{
    new(1, "Купить хлеб", false),
    new(2, "Сделать дз", true),
    new(3, "Позвонить маме", false)
};


// 4. CRUD эндпоинты


// GET /api/todos?completed=true
app.MapGet("/api/todos", (bool? completed) =>
{
    var result = completed.HasValue
        ? todos.Where(t => t.IsCompleted == completed.Value)
        : todos;
    return Results.Ok(result);
})
.WithName("GetAllTodos")
.WithOpenApi();

// GET /api/todos/{id}
app.MapGet("/api/todos/{id:int}", (int id) =>
{
    var todo = todos.FirstOrDefault(t => t.Id == id);
    return todo is null ? Results.NotFound() : Results.Ok(todo);
})
.WithName("GetTodoById")
.WithOpenApi();

// POST /api/todos
app.MapPost("/api/todos", (TodoItem input) =>
{
    var newId = todos.Count == 0 ? 1 : todos.Max(t => t.Id) + 1;
    var todo = new TodoItem(newId, input.Title, false);
    todos.Add(todo);
    return Results.Created($"/api/todos/{todo.Id}", todo);
})
.WithName("CreateTodo")
.WithOpenApi();

// PUT /api/todos/{id}
app.MapPut("/api/todos/{id:int}", (int id, TodoItem input) =>
{
    var todo = todos.FirstOrDefault(t => t.Id == id);
    if (todo is null) return Results.NotFound();

    todo.Title = input.Title;
    todo.IsCompleted = input.IsCompleted;
    return Results.Ok(todo);
})
.WithName("UpdateTodo")
.WithOpenApi();

// DELETE /api/todos/{id}
app.MapDelete("/api/todos/{id:int}", (int id) =>
{
    var todo = todos.FirstOrDefault(t => t.Id == id);
    if (todo is null) return Results.NotFound();

    todos.Remove(todo);
    return Results.NoContent();
})
.WithName("DeleteTodo")
.WithOpenApi();

app.Run();


public class TodoItem
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public bool IsCompleted { get; set; }

    public TodoItem() { }

    public TodoItem(int id, string title, bool isCompleted)
    {
        Id = id;
        Title = title;
        IsCompleted = isCompleted;
    }
}