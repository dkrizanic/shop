using Microsoft.EntityFrameworkCore;
using Infrastructure.Data;
using Infrastructure.Services;
using Infrastructure.Repositories;
using Domain.Repositories;
using FluentValidation;
using Domain.Validators;
using Application.Middleware;
using Domain.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configure port for Render deployment
if (builder.Environment.IsProduction())
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(int.Parse(port));
    });
}

// Add Entity Framework
if (builder.Environment.EnvironmentName != "Testing")
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    // Fallback to default SQLite path if connection string is empty
    if (string.IsNullOrEmpty(connectionString))
    {
        connectionString = "Data Source=/app/data/shop.db";
        Console.WriteLine($"Using fallback connection string: {connectionString}");
    }

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(connectionString));
}

// Add HttpClient for DummyJSON API
builder.Services.AddHttpClient<IDummyJsonService, DummyJsonService>();

// Register repositories and services
builder.Services.AddScoped<IUserFavoriteRepository, UserFavoriteRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IShoppingCartRepository, ShoppingCartRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IShoppingCartService, ShoppingCartService>();

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<ProductQueryParametersValidator>();

// Add JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        var frontendUrl = builder.Configuration["FRONTEND_URL"] ?? "http://localhost:3000";
        policy.WithOrigins("http://localhost:3000", frontendUrl)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Ensure database is created and migrated
if (app.Environment.IsProduction())
{
    try
    {
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            Console.WriteLine($"Database connection string: {context.Database.GetConnectionString()}");
            context.Database.EnsureCreated();
            Console.WriteLine("Database initialized successfully");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database initialization failed: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        // Continue without database for now
    }
}

// Add global exception handling middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHttpsRedirection();
}

// Serve static files from wwwroot (frontend build)
app.UseStaticFiles();

app.UseCors("FrontendPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Fallback to serve React app for client-side routing
app.MapFallbackToFile("index.html");

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }