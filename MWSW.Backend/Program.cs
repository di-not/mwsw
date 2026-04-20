using MWSW.Backend.Services;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Настройка кодировки JSON для поддержки русского языка
        options.JsonSerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.WriteIndented = true;
        options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add database service
builder.Services.AddSingleton<DatabaseService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Добавляем middleware для правильной кодировки
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("Content-Type", "application/json; charset=utf-8");
    context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
    context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
    context.Response.Headers.Append("Access-Control-Allow-Headers", "Content-Type");
    
    if (context.Request.Method == "OPTIONS")
    {
        context.Response.StatusCode = 200;
        await context.Response.CompleteAsync();
        return;
    }
    
    await next();
});

app.UseRouting();
app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

// Test endpoint
app.MapGet("/", () => "MWSW API is running!");

// Test database connection endpoint
app.MapGet("/api/test-db", async (DatabaseService dbService) =>
{
    try
    {
        var planets = await dbService.GetAllPlanetsAsync();
        return Results.Ok(new { 
            status = "success", 
            message = $"Database connection successful. Found {planets.Count} planets.",
            count = planets.Count,
            firstPlanet = planets.FirstOrDefault()?.Name
        });
    }
    catch (Exception ex)
    {
        return Results.Ok(new { 
            status = "error", 
            message = ex.Message,
            innerError = ex.InnerException?.Message
        });
    }
});

app.Run();