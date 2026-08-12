using IotPipeline.Platform.Common.Interfaces;
using IotPipeline.Platform.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace IotPipeline.Platform.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient<IRagService, GeminiRagService>();
        services.AddScoped<IEmbeddingService, LocalEmbeddingService>();
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                o => o.UseVector()
            )
        );

        return services;
    }
}
