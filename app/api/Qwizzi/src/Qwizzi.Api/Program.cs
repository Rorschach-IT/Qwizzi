using Microsoft.EntityFrameworkCore;
using Qwizzi.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// OpenAPI / Swagger
builder.Services.AddOpenApi();

// PostgreSQL + EF Core
var connectionString =
    builder.Configuration.GetConnectionString("Database")
    ?? throw new InvalidOperationException(
        "Brak connection stringa: ConnectionStrings:Database.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Qwizzi API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

app.Run();