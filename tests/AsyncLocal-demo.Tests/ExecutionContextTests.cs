using AsyncLocal.ExecutionContext;
using AsyncLocal.ExecutionContext.Abstractions;
using AsyncLocal_demo.Core.Context;
using FluentAssertions;

namespace AsyncLocal_demo.Tests;

public sealed class ExecutionContextTests
{
    private readonly IExecutionContextAccessor _accessor = new ExecutionContextAccessor(new AsyncLocalExecutionContext());

    [Fact]
    public void Devrait_Stocker_Et_Recuperer_Les_Valeurs()
    {
        var context = _accessor.Current;

        context.CorrelationId = "test-correlation";
        context.SetTenantId("test-tenant");
        context.SetUserId("test-user");

        context.CorrelationId.Should().Be("test-correlation");
        context.GetTenantId().Should().Be("test-tenant");
        context.GetUserId().Should().Be("test-user");

        context.Clear();
    }

    [Fact]
    public void Devrait_Stocker_Les_Donnees_Personnalisees()
    {
        var context = _accessor.Current;

        context.Set("key1", "value1");
        context.Set("key2", 42);

        context.Get<string>("key1").Should().Be("value1");
        context.Get<int>("key2").Should().Be(42);
        context.Get<string>("unknown").Should().BeNull();

        context.Clear();
    }

    [Fact]
    public async Task Devrait_Se_Propager_A_Travers_Les_Appels_Asynchrones()
    {
        var context = _accessor.Current;
        context.SetTenantId("async-tenant");

        var result = await ObtenirTenantAsync();

        result.Should().Be("async-tenant");

        context.Clear();
    }

    [Fact]
    public async Task Devrait_Isoler_Les_Contextes_Paralleles()
    {
        var tasks = Enumerable.Range(1, 50).Select(async i =>
        {
            var ctx = _accessor.Current;
            ctx.SetTenantId($"tenant-{i}");

            await Task.Delay(Random.Shared.Next(5, 20));

            return ctx.GetTenantId() == $"tenant-{i}";
        });

        var results = await Task.WhenAll(tasks);

        results.Should().AllSatisfy(r => r.Should().BeTrue());
    }

    [Fact]
    public void Devrait_Nettoyer_Le_Contexte()
    {
        var context = _accessor.Current;

        context.CorrelationId = "to-clear";
        context.SetTenantId("to-clear");
        context.Set("custom", "value");

        context.Clear();

        context.CorrelationId.Should().BeNull();
        context.GetTenantId().Should().BeNull();
        context.Get<string>("custom").Should().BeNull();
    }

    private async Task<string?> ObtenirTenantAsync()
    {
        await Task.Delay(10);
        return _accessor.Current.GetTenantId();
    }
}