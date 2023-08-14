using ATI.Services.Common.Serializers.SystemTextJsonSerialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Sorcer.Api;

var builder = WebApplication.CreateBuilder(args)
    .SetUpHost();

var services = builder.Services;

services.AddControllers(options =>
    {
        options.SuppressInputFormatterBuffering = true;
        options.SuppressOutputFormatterBuffering = true;
    })
    .AddControllersAsServices()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = new SnakeCaseNamingPolicy();
    });

services.WithOptions()
    .WithServices();

var app = builder.Build();

await app.UseAppAsync();

app.Run();