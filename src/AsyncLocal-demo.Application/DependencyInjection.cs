using AsyncLocal_demo.Application.Orders;
using Microsoft.Extensions.DependencyInjection;

namespace AsyncLocal_demo.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IOrderProcessor, OrderProcessor>();
        return services;
    }
}