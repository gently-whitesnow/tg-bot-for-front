using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using ATI.Services.Common.Behaviors;
using ATI.Services.Common.Extensions;
using ATI.Services.Common.Initializers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using Sorcer.Bot;
using Sorcer.Bot.CallbackHandler;
using Sorcer.Bot.MessageHandler;
using Sorcer.DataAccess.Helpers;
using Sorcer.DataAccess.Managers;
using Sorcer.DataAccess.Repositories;
using Sorcer.Models.Options;
using ConfigurationManager = ATI.Services.Common.Behaviors.ConfigurationManager;

namespace Sorcer.Api;

public static class Startup
{
    public static WebApplicationBuilder SetUpHost(this WebApplicationBuilder builder)
    {
        builder.Host.UseContentRoot(Directory.GetCurrentDirectory());

        builder.WebHost.UseKestrel(options =>
        {
            options.Listen(IPAddress.Any, ConfigurationManager.GetApplicationPort());
            options.AllowSynchronousIO = true;
        }).ConfigureLogging((context, loggingBuilder) =>
        {
            loggingBuilder
                .ClearProviders()
                .AddConsole();
        }).UseDefaultServiceProvider((context, options) =>
        {
            var environmentName = context.HostingEnvironment.EnvironmentName;
            //Scoped services aren't directly or indirectly resolved from the root service provider.
            //Scoped services aren't directly or indirectly injected into singletons.
            options.ValidateScopes = context.HostingEnvironment.IsDevelopment() || environmentName == "dev";
            //Validate DI tree on startup    
            options.ValidateOnBuild = context.HostingEnvironment.IsDevelopment() || environmentName is "dev" or "staging";
        });

        var env = builder.Environment;

        builder.Configuration
            .SetBasePath(env.ContentRootPath)
            .AddJsonFile("appsettings.json", true, true)
            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true)
            .AddEnvironmentVariables()
            .Build();

        ConfigurationManager.ConfigurationRoot = builder.Configuration;
        var botSettings = new TelegramBotOptions();
        builder.Configuration.GetSection(nameof(TelegramBotOptions)).Bind(botSettings);
        botSettings.TOKEN = Environment.GetEnvironmentVariable(nameof(botSettings.TOKEN));
        botSettings.PASSWORD = Environment.GetEnvironmentVariable(nameof(botSettings.PASSWORD));
        if (string.IsNullOrEmpty(botSettings.TOKEN) || string.IsNullOrEmpty(botSettings.PASSWORD))
            throw new KeyNotFoundException("TOKEN or PASSWORD is empty");
        builder.Services.AddSingleton(botSettings);
        return builder;
    }

    public static IServiceCollection WithOptions(this IServiceCollection services)
    {
        
        services.ConfigureByName<FileSystemOptions>();
        return services;
    }
    public static IServiceCollection WithServices(this IServiceCollection services)
    {
        services.AddHealthChecks();

        services
            .AddControllers(options =>
            {
                options.SuppressInputFormatterBuffering = true;
                options.SuppressOutputFormatterBuffering = true;
            })
            .AddNewtonsoftJson(
                options =>
                {
                    options.SerializerSettings.ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new SnakeCaseNamingStrategy()
                    };
                });

        services.AddSingleton<UpdateHandler>();
        services.AddSingleton<CallbackReceiver>();
        services.AddSingleton<MessageReceiver>();
        
        services.AddSingleton<FileSystemHelper>();
        services.AddSingleton<InlineSender>();
        
        services.AddSingleton<AuthorizationManager>();
        services.AddSingleton<EventsManager>();
        
        services.AddSingleton<AuthorizationRepository>();
        services.AddSingleton<UserStateRepository>();
        services.AddSingleton<EventsRepository>();

        services.AddSingleton<TelegramBotInitializer>();
        services.AddInitializers();
        return services;
    }

    public static Task UseAppAsync(this WebApplication app)
    {
        const string healthCheckRoute = "/_internal/healthcheck";
        
        app.UseRouting();
        app.UseCors(CommonBehavior.AllowAllOriginsCorsPolicyName);
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapHealthChecks(healthCheckRoute);
        });
        
        var notify = () => Console.WriteLine(@"Application Port - " + ConfigurationManager.GetApplicationPort());

        var services = app.Services;
        
        using var scope = services.CreateScope();
        var initTask = app.Services.GetRequiredService<StartupInitializer>()
            .InitializeAsync().ContinueWith(_ => notify());
        
        return initTask;
    }
}