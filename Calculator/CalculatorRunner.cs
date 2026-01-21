using Calculator.Library.Contracts.Services;
using Calculator.Library.Exceptions;
using Calculator.Library.Models;
using Microsoft.Extensions.Logging;

namespace Calculator;

public class CalculatorRunner(
    ICalculatorService calculator, 
    ILogger<CalculatorRunner> logger)
{
    
    public void RunInteractive()
    {
        logger.LogInformation("Запуск интерактивного режима...");
        Console.WriteLine("=== Интерактивный калькулятор ===");

        while (true)
        {
            var operation = GetOperation();
            if (operation == null) break;

            var a = GetNumber("первое число");
            var b = operation.IsUnary
                ? double.NaN
                : GetNumber("второе число");

            ExecuteCalculatorCommand(operation.Handler, a, b);
            Console.WriteLine(); 
        }
    }
    
    private OperationInfo? GetOperation()
    {
        var operations = new Dictionary<string, OperationInfo>
        {
            { "+", new("Сложение", calculator.Add) },
            { "-", new("Вычитание", calculator.Subtract) },
            { "*", new("Умножение", calculator.Multiply) },
            { "/", new("Деление", calculator.Divide) },
            { "^", new("Возведение в степень", calculator.Power) },
            { "s", new("Квадратный корень", (x, _) => calculator.SquareRoot(x), IsUnary: true) }
        };

        Console.WriteLine("\nДоступные операции:");
        
        var menuLines = operations.Select(keyValuePair => $"  {keyValuePair.Key} — {keyValuePair.Value.Name}");
        Console.WriteLine(string.Join("\n", menuLines));
        
        Console.WriteLine("  q/[empty]/[incorrect] — выход");

        Console.Write("\nВыберите операцию: ");
        var input = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(input) || input.Equals("q", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return operations.GetValueOrDefault(input);
    }
    
    private static double GetNumber(string numberNaming)
    {
        Console.WriteLine($"Введите {numberNaming}: ");
    
        var input = Console.ReadLine();

        return double.TryParse(input, out var number)
            ? number
            : double.NaN;
    }
    
    public static void ExecuteCalculatorCommand(
        Func<double, double, CalculationResult> operation,
        double a,
        double b)
    {
        try
        {
            var result = operation(a, b);
            Console.WriteLine($"Результат операции: {result}");
        }
        catch (CalculationException ex)
        {
            Console.WriteLine($"Ошибка калькулятора: {ex.Message}");
        }
    }
}
