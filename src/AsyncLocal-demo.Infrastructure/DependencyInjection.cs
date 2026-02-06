using AsyncLocal.ExecutionContext;
using AsyncLocal.ExecutionContext.Abstractions;
using AsyncLocal_demo.Application.Orders;
using AsyncLocal_demo.Core.BackgroundProcessing;
using AsyncLocal_demo.Infrastructure.BackgroundProcessing;
using AsyncLocal_demo.Infrastructure.Context;
using AsyncLocal_demo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AsyncLocal_demo.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Contexte d'exécution via le package réutilisable
        services.AddExecutionContext<DemoSyntheticHttpContextBuilder>();

        // Base de données
        services.AddDbContext<AppDbContext>(opt =>
            opt.UseInMemoryDatabase("ExecutionContextDemo"));

        services.AddScoped<IOrderRepository, OrderRepository>();

        // Traitement en arrière-plan
        services.AddSingleton<IBackgroundTaskQueue<Guid>>(sp =>
            new BackgroundTaskQueue<Guid>(
                sp.GetRequiredService<IExecutionContext>(),
                capacity: 100));

        services.AddSingleton<OrderProcessingBackgroundService>();
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<Microsoft.Extensions.Hosting.IHostedService, OrderProcessingBackgroundService>(
                sp => sp.GetRequiredService<OrderProcessingBackgroundService>()));

        return services;
    }
}