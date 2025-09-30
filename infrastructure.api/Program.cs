using infrastructure.api.Data;
using infrastructure.api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc; // for ProblemDetails
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// EF Core InMemory
builder.Services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase("infra-db"));

// Controllers + JSON config
builder.Services
    .AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        // o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// ProblemDetails middleware
builder.Services.AddProblemDetails();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS (allow Web on https://7200 and http://5200)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWeb", policy =>
        policy.WithOrigins("https://localhost:7200", "http://localhost:5200")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// ===== Dynamic seed for demo data =====
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (!db.Components.Any())
    {
        db.Components.AddRange(
            new InfrastructureComponent
            {
                Id = Guid.NewGuid(),
                Name = "Demo VM",
                Type = "vm",
                Environment = "dev",
                Status = "provisioned",
                CreatedUtc = DateTime.UtcNow
            },
            new InfrastructureComponent
            {
                Id = Guid.NewGuid(),
                Name = "Demo SQL Database",
                Type = "sql",
                Environment = "prod",
                Status = "provisioned",
                CreatedUtc = DateTime.UtcNow
            }
        );

        db.SaveChanges();
    }
}
// =====================================

// HSTS in non-dev
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Centralized exception handling + RFC7807
app.UseExceptionHandler();

// CORS before endpoints
app.UseCors("AllowWeb");

app.MapControllers();

// Small health check and root redirect to Swagger
app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();
