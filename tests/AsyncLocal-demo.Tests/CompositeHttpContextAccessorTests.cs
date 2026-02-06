using AsyncLocal.ExecutionContext;
using AsyncLocal.ExecutionContext.Abstractions;
using AsyncLocal_demo.Core.Context;
using AsyncLocal_demo.Infrastructure.Context;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;

namespace AsyncLocal_demo.Tests;

public sealed class CompositeHttpContextAccessorTests
{
    private readonly IExecutionContextAccessor _executionContextAccessor = new ExecutionContextAccessor(new AsyncLocalExecutionContext());

    [Fact]
    public void Devrait_Retourner_HttpContext_Explicite_En_Priorite()
    {
        // Arrange
        var explicitContext = new DefaultHttpContext();
        var providers = CreateProviders();
        var accessor = new CompositeHttpContextAccessor(providers)
        {
            // Act
            HttpContext = explicitContext
        };

        // Assert
        accessor.HttpContext.Should().BeSameAs(explicitContext);
    }

    [Fact]
    public void Devrait_Utiliser_Provider_Selon_Priorite()
    {
        // Arrange
        var lowPriorityContext = new DefaultHttpContext { TraceIdentifier = "low" };
        var highPriorityContext = new DefaultHttpContext { TraceIdentifier = "high" };

        var lowPriorityProvider = CreateMockProvider(priority: 10, context: lowPriorityContext);
        var highPriorityProvider = CreateMockProvider(priority: 0, context: highPriorityContext);

        var accessor = new CompositeHttpContextAccessor([lowPriorityProvider, highPriorityProvider]);

        // Act
        var result = accessor.HttpContext;

        // Assert
        result.Should().NotBeNull();
        result!.TraceIdentifier.Should().Be("high");
    }

    [Fact]
    public void Devrait_Fallback_Au_Provider_Suivant_Si_Null()
    {
        // Arrange
        var fallbackContext = new DefaultHttpContext { TraceIdentifier = "fallback" };

        var primaryProvider = CreateMockProvider(priority: 0, context: null);
        var fallbackProvider = CreateMockProvider(priority: 10, context: fallbackContext);

        var accessor = new CompositeHttpContextAccessor([primaryProvider, fallbackProvider]);

        // Act
        var result = accessor.HttpContext;

        // Assert
        result.Should().NotBeNull();
        result!.TraceIdentifier.Should().Be("fallback");
    }

    [Fact]
    public void Devrait_Retourner_Null_Si_Aucun_Provider_Ne_Fournit_Context()
    {
        // Arrange
        var providers = new[]
        {
            CreateMockProvider(priority: 0, context: null),
            CreateMockProvider(priority: 10, context: null)
        };

        var accessor = new CompositeHttpContextAccessor(providers);

        // Act
        var result = accessor.HttpContext;

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ExecutionContextProvider_Devrait_Creer_HttpContext_Synthetique()
    {
        // Arrange
        var context = _executionContextAccessor.Current;
        context.SetUserId("user-123");
        context.SetTenantId("tenant-456");
        context.CorrelationId = "corr-789";

        var httpContextBuilder = new DemoSyntheticHttpContextBuilder();
        var provider = new ExecutionContextHttpContextProvider(_executionContextAccessor, httpContextBuilder);

        // Act
        var httpContext = provider.GetHttpContext();

        // Assert
        httpContext.Should().NotBeNull();
        httpContext!.TraceIdentifier.Should().Be("corr-789");
        httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value.Should().Be("user-123");
        httpContext.User.FindFirst("tenant_id")?.Value.Should().Be("tenant-456");

        context.Clear();
    }

    [Fact]
    public void ExecutionContextProvider_Devrait_Retourner_Null_Si_Context_Vide()
    {
        // Arrange
        var context = _executionContextAccessor.Current;
        context.Clear();

        var httpContextBuilder = new DemoSyntheticHttpContextBuilder();
        var provider = new ExecutionContextHttpContextProvider(_executionContextAccessor, httpContextBuilder);

        // Act
        var httpContext = provider.GetHttpContext();

        // Assert
        httpContext.Should().BeNull();
    }

    [Fact]
    public async Task Devrait_Fonctionner_Dans_Contexte_BackgroundService_Simule()
    {
        // Arrange
        var context = _executionContextAccessor.Current;
        context.SetUserId("bg-user");
        context.SetTenantId("bg-tenant");

        var httpContextBuilder = new DemoSyntheticHttpContextBuilder();
        var executionContextProvider = new ExecutionContextHttpContextProvider(_executionContextAccessor, httpContextBuilder);
        var defaultProvider = CreateMockProvider(priority: 0, context: null);

        var accessor = new CompositeHttpContextAccessor([defaultProvider, executionContextProvider]);

        // Act
        var result = await Task.Run(() =>
        {
            var httpContext = accessor.HttpContext;
            return httpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        });

        // Assert
        result.Should().Be("bg-user");

        context.Clear();
    }

    [Fact]
    public async Task Devrait_Isoler_Contextes_Paralleles_Dans_BackgroundService()
    {
        // Arrange
        var httpContextBuilder = new DemoSyntheticHttpContextBuilder();
        var executionContextProvider = new ExecutionContextHttpContextProvider(_executionContextAccessor, httpContextBuilder);
        var defaultProvider = CreateMockProvider(priority: 0, context: null);
        var accessor = new CompositeHttpContextAccessor([defaultProvider, executionContextProvider]);

        // Act
        var tasks = Enumerable.Range(1, 20).Select(async i =>
        {
            var ctx = _executionContextAccessor.Current;
            ctx.SetUserId($"user-{i}");
            ctx.SetTenantId($"tenant-{i}");

            await Task.Delay(Random.Shared.Next(5, 15));

            var httpContext = accessor.HttpContext;
            var userId = httpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var tenantId = httpContext?.User.FindFirst("tenant_id")?.Value;

            return userId == $"user-{i}" && tenantId == $"tenant-{i}";
        });

        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().AllSatisfy(r => r.Should().BeTrue());
    }

    private IHttpContextProvider[] CreateProviders()
    {
        var httpContextBuilder = new DemoSyntheticHttpContextBuilder();
        return
        [
            CreateMockProvider(priority: 0, context: null),
            new ExecutionContextHttpContextProvider(_executionContextAccessor, httpContextBuilder)
        ];
    }

    private static IHttpContextProvider CreateMockProvider(int priority, HttpContext? context)
    {
        var mock = new Mock<IHttpContextProvider>();
        mock.Setup(p => p.Priority).Returns(priority);
        mock.Setup(p => p.GetHttpContext()).Returns(context);
        return mock.Object;
    }
}