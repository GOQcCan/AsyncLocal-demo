using AsyncLocal.ExecutionContext;
using AsyncLocal.ExecutionContext.Abstractions;
using AsyncLocal_demo.Core.Context;
using AsyncLocal_demo.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace AsyncLocal_demo.Tests;


public sealed class HttpContextAccessorIntegrationTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;

    public HttpContextAccessorIntegrationTests()
    {
        var services = new ServiceCollection();
        services.AddInfrastructure();
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public void DI_Devrait_Resoudre_CompositeHttpContextAccessor()
    {
        // Act
        var accessor = _serviceProvider.GetRequiredService<IHttpContextAccessor>();

        // Assert
        accessor.Should().BeOfType<CompositeHttpContextAccessor>();
    }

    [Fact]
    public void DI_Devrait_Enregistrer_Tous_Les_Providers()
    {
        // Act
        var providers = _serviceProvider.GetServices<IHttpContextProvider>().ToList();

        // Assert
        providers.Should().HaveCount(2);
        providers.Should().ContainSingle(p => p is DefaultHttpContextProvider);
        providers.Should().ContainSingle(p => p is ExecutionContextHttpContextProvider);
    }

    [Fact]
    public async Task Integration_BackgroundService_Devrait_Propager_Context()
    {
        // Arrange
        var executionContextAccessor = _serviceProvider.GetRequiredService<IExecutionContextAccessor>();
        var httpContextAccessor = _serviceProvider.GetRequiredService<IHttpContextAccessor>();

        // Simule ce que fait le BackgroundService
        executionContextAccessor.Current.SetUserId("integration-user");
        executionContextAccessor.Current.SetTenantId("integration-tenant");
        executionContextAccessor.Current.CorrelationId = "integration-corr";

        // Act - Simule un appel à une librairie externe utilisant IHttpContextAccessor
        var (UserId, TenantId, CorrelationId) = await SimulerAppelLibrairieExterneAsync(httpContextAccessor);

        // Assert
        UserId.Should().Be("integration-user");
        TenantId.Should().Be("integration-tenant");
        CorrelationId.Should().Be("integration-corr");

        executionContextAccessor.Current.Clear();
    }

    private static async Task<(string? UserId, string? TenantId, string? CorrelationId)> 
        SimulerAppelLibrairieExterneAsync(IHttpContextAccessor accessor)
    {
        await Task.Delay(10); // Simule une opération async

        var httpContext = accessor.HttpContext;
        
        return (
            httpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            httpContext?.User.FindFirst("tenant_id")?.Value,
            httpContext?.TraceIdentifier
        );
    }

    public void Dispose() => _serviceProvider.Dispose();
}