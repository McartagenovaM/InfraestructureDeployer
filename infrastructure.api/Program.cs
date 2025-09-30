using infrastructure.api.Data;
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
        // If you later add enums, consider: o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
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
