using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Calculator.Library.Configuration;
using Calculator.Library.Contracts.Services;
using Calculator.Library.Services;

namespace Calculator;

public static class Bootstrap
{
    public static IServiceProvider Initialize()
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

    public static void Shutdown()
    {
        Log.CloseAndFlush();
    }
}
