using AsyncLocal.ExecutionContext.Abstractions;

namespace AsyncLocal_demo.Core.Context;

/// <summary>
/// Extensions spécifiques à cette solution pour les propriétés métier.
/// </summary>
public static class ExecutionContextExtensions
{
    private const string UserIdKey = "UserId";
    private const string TenantIdKey = "TenantId";

    public static string? GetUserId(this IExecutionContext ctx) => ctx.Get<string>(UserIdKey);
    public static void SetUserId(this IExecutionContext ctx, string? value)
    {
        if (value is not null) ctx.Set(UserIdKey, value);
    }

    public static string? GetTenantId(this IExecutionContext ctx) => ctx.Get<string>(TenantIdKey);
    public static void SetTenantId(this IExecutionContext ctx, string? value)
    {
        if (value is not null) ctx.Set(TenantIdKey, value);
    }
}