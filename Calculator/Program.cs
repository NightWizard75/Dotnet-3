// В Program.cs настройте:
// - ConfigurationBuilder для чтения appsettings.json
// - Serilog для логирования
// - DI контейнер для регистрации ICalculatorService
// - Вызов методов библиотеки

using Calculator;
using Microsoft.Extensions.DependencyInjection;
using Calculator.Library.Contracts.Services;
using Microsoft.Extensions.Logging;
using Serilog;


try
{
    // Настройка вынесена в Bootstrap
    using var host = Bootstrap.Initialize(args);
    var runner = host.Services.GetRequiredService<CalculatorRunner>();
    
    var logger = host.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Запуск приложения...");
    
    // Ручные запуски
    CalculatorManualCalls(host.Services);
    
    // Интерактивный режим
    runner.RunInteractive();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Критическая ошибка приложения");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}

return 0;

static void CalculatorManualCalls(IServiceProvider serviceProvider)
{
    Console.WriteLine("=== Демонстрационные вызовы ===");
    var calculator = serviceProvider.GetRequiredService<ICalculatorService>();

    CalculatorRunner.ExecuteCalculatorCommand(calculator.Add, 2.6, 3.7);
    CalculatorRunner.ExecuteCalculatorCommand(calculator.Subtract, 5.0, 2.0);
    CalculatorRunner.ExecuteCalculatorCommand(calculator.Multiply, 2.6, 3.7);
    CalculatorRunner.ExecuteCalculatorCommand(calculator.Divide, 2.6, 0.0);      // деление на ноль
    CalculatorRunner.ExecuteCalculatorCommand(calculator.Divide, 0.0 / 0.0, 1);  // NaN
    CalculatorRunner.ExecuteCalculatorCommand(calculator.Power, 2, 10);
    
    // SquareRoot принимает 1 параметр, ExecuteCalculatorCommand - 2 параметра
    // Обмануть можно с помощью лямбды с 1-м необязательным параметром 
    CalculatorRunner.ExecuteCalculatorCommand((x, _) => calculator.SquareRoot(x), 256, 0);

    Console.WriteLine("\n" + new string('=', 80) + "\n");
}
