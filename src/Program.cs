using Microsoft.EntityFrameworkCore;
using Infrastructure.Data;
using Infrastructure.Services;
using Infrastructure.Repositories;
using Domain.Repositories;
using FluentValidation;
using Domain.Validators;
using Application.Middleware;
using Domain.Models;

var builder = WebApplication.CreateBuilder(args);

// Add Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add HttpClient for DummyJSON API
builder.Services.AddHttpClient<IDummyJsonService, DummyJsonService>();

// Register repositories and services
builder.Services.AddScoped<IUserFavoriteRepository, UserFavoriteRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<ProductQueryParametersValidator>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Add global exception handling middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }