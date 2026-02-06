using AsyncLocal.ExecutionContext.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace AsyncLocal.ExecutionContext;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddExecutionContext<TBuilder>(this IServiceCollection services)
        where TBuilder : class, ISyntheticHttpContextBuilder
    {
        services.AddSingleton<AsyncLocalExecutionContext>();
        services.AddSingleton<IExecutionContext>(sp => sp.GetRequiredService<AsyncLocalExecutionContext>());
        services.AddSingleton<IExecutionContextAccessor, ExecutionContextAccessor>();

        services.AddSingleton<HttpContextAccessor>();
        services.AddSingleton<ISyntheticHttpContextBuilder, TBuilder>();

        services.AddSingleton<IHttpContextProvider, DefaultHttpContextProvider>();
        services.AddSingleton<IHttpContextProvider, ExecutionContextHttpContextProvider>();
        services.AddSingleton<IHttpContextAccessor, CompositeHttpContextAccessor>();

        return services;
    }
}