using AsyncLocal_demo.Application.Orders;
using AsyncLocal_demo.Core.Context;
using AsyncLocal_demo.Infrastructure.Context;
using AsyncLocal_demo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AsyncLocal_demo.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IExecutionContextAccessor, ExecutionContextAccessor>();
        services.AddSingleton<IExecutionContext>(sp =>
            sp.GetRequiredService<IExecutionContextAccessor>().Current);

        services.AddDbContext<AppDbContext>(opt =>
            opt.UseInMemoryDatabase("ExecutionContextDemo"));

        services.AddScoped<IOrderRepository, OrderRepository>();

        return services;
    }
}