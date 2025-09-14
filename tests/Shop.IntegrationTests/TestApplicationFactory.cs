using Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Shop.IntegrationTests;

public class TestApplicationFactory<TProgram> : WebApplicationFactory<TProgram>
    where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Override the connection string to force in-memory
        builder.UseSetting("ConnectionStrings:DefaultConnection", "");

        builder.ConfigureServices(services =>
        {
            // Remove all Entity Framework related services
            var efServices = services.Where(s =>
                s.ServiceType.FullName?.Contains("EntityFramework") == true ||
                s.ServiceType == typeof(ApplicationDbContext) ||
                s.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
                s.ServiceType == typeof(DbContextOptions) ||
                (s.ServiceType.IsGenericType && s.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>))
            ).ToList();

            foreach (var service in efServices)
            {
                services.Remove(service);
            }

            // Add clean InMemory database - use a static name per test run
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDatabase");
                options.EnableSensitiveDataLogging();
            });
        });
    }
}