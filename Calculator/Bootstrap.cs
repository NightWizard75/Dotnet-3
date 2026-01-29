using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Calculator.Library.Configuration;
using Calculator.Library.Contracts.Services;
using Calculator.Library.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace Calculator;

public static class Bootstrap
{
    public static IHost Initialize(string[]? args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        // === 1. Создание логгера ===
        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)  // чтение из appsettings.json
            .CreateLogger();

        // === 2. Регистрация опций ===
        builder.Services.Configure<CalculatorOptions>(
            builder.Configuration.GetSection("Calculator")
        );

        // === 3. Регистрация сервисов ===
        builder.Services.AddSingleton<ICalculatorService, CalculatorService>();
        builder.Services.AddSingleton<CalculatorRunner>();
        
        // dispose: true — гарантирует вызов CloseAndFlush() при завершении хоста
        builder.Services.AddSerilog(logger, dispose: true);

        return builder.Build();
    }
    
    public static IServiceProvider InitializeByServiceProvider()
    {
        // === 1. Загрузка конфигурации ===
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        // === 2. Настройка Serilog ===
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .MinimumLevel.Debug()
            //.ReadFrom.Configuration(config)
            .CreateLogger();
        
        // === 3. Получение настроек калькулятора из конфигурационного файла. ===
        var options = config.GetSection("Calculator").Get<CalculatorOptions>()
                      ?? new CalculatorOptions();
                      
        Log.Information("Загружена конфигурация: {@CalculatorOptions}", options);

        // === 4. DI-контейнер ===
        var services = new ServiceCollection(); 
        services.AddSingleton(Options.Create(options));
        services.AddSingleton<ICalculatorService, CalculatorService>();
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(Log.Logger);
        });
        services.AddSingleton<CalculatorRunner>();

        return services.BuildServiceProvider();
    }
    
    public static IServiceProvider InitializeByWebApplication()
    {
        
        var builder = WebApplication.CreateBuilder();
        
        builder.Services.Configure<CalculatorOptions>(builder.Configuration.GetSection("Calculator"));
        builder.Services.AddScoped<ICalculatorService, CalculatorService>();
        builder.Services.AddScoped<CalculatorRunner>();
        builder.Host.UseSerilog((context, services, loggerConfiguration) =>
        {
            loggerConfiguration
                .WriteTo.Console()
                .MinimumLevel.Debug();
        });
        
        return builder.Build().Services;
    }

    /// <summary>
    /// Метод необходим для корректного завершения глобального логгера.
    /// Но он не нужен при использовании
    /// Host.CreateApplicationBuilder()
    /// builder.Services.AddSerilog(logger, dispose: true);
    /// </summary>
    public static void Shutdown()
    {
        Log.CloseAndFlush();
    }
}
