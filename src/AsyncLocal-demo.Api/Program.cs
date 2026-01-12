using AsyncLocal_demo.Api;
using AsyncLocal_demo.Application;
using AsyncLocal_demo.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddInfrastructure()
    .AddApplication()
    .AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwagger();
}

app.UseExecutionContext();
app.MapControllers();

await app.RunAsync();