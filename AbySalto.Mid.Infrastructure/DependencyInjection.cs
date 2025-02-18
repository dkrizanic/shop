using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AbySalto.Mid.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructur(this IServiceCollection services, IConfiguration configuration)
        {
            return services;
        }

        private static IServiceCollection AddServices(this IServiceCollection services)
        {
            return services;
        }

        private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            return services;
        }
    }
}
