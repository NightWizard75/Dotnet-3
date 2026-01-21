using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Calculator.Library.Configuration;
using Calculator.Library.Contracts.Services;
using Calculator.Library.Exceptions;
using Calculator.Library.Models;

namespace Calculator.Library.Services;

public class CalculatorService(
    IOptions<CalculatorOptions> options,
    ILogger<CalculatorService> logger) : ICalculatorService
{
    private readonly CalculatorOptions _options = options.Value 
        ?? throw new ArgumentNullException(nameof(options));

    private readonly ILogger<CalculatorService> _logger = logger 
        ?? throw new ArgumentNullException(nameof(logger));

    public CalculationResult Add(double a, double b) =>
        ExecuteOperation(() => a + b, args: [a, b]);

    public CalculationResult Subtract(double a, double b) =>
        ExecuteOperation(() => a - b, args: [a, b]);

    public CalculationResult Multiply(double a, double b) =>
        ExecuteOperation(() => a * b, args: [a, b]);

    public CalculationResult Divide(double a, double b)
    {
        if (b == 0)
        {
            var ex = CalculationException.DivisionByZero();
            LogError(ex, nameof(Divide), a, b);
            throw ex;
        }
        
        return ExecuteOperation(() => a / b, args: [a, b]);
    }

    public CalculationResult Power(double baseValue, double exponent)
    {
        if (baseValue == 0 && exponent < 0)
            throw CalculationException.InvalidResult("возведение нуля в отрицательную степень");
        
        if (double.IsNaN(baseValue) || double.IsNaN(exponent) ||
            double.IsInfinity(baseValue) || double.IsInfinity(exponent))
            throw CalculationException.InvalidResult("операнды содержат NaN или Infinity");

        return ExecuteOperation(() => Math.Pow(baseValue, exponent), args: [baseValue, exponent]);
    }

    public CalculationResult SquareRoot(double value)
    {
        if (value < 0)
        {
            var ex = CalculationException.NegativeSquareRoot();
            LogError(ex, nameof(SquareRoot), value);
            throw ex;
        }
        return ExecuteOperation(() => Math.Sqrt(value), [value]);
    }

    private CalculationResult ExecuteOperation(
        Func<double> operation,
        double[] args,
        [CallerMemberName] string operationName = null!
        )
    {
        try
        {
            var result = ValidateAndRound(operation());
            
            LogDebug(operationName, result, args);
            
            return new CalculationResult(result);
        }
        catch (Exception ex)
        {
            LogError(ex, operationName, args);
            throw; // нужно пробросить выше, чтобы условный контроллер мог вернуть нормальную ошибку 400.
        }
    }

    private double ValidateAndRound(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
            throw CalculationException.InvalidResult("результат содержит NaN или Infinity");

        if (Math.Abs(value) > _options.MaxValue)
            throw CalculationException.ResultTooLarge(_options.MaxValue);

        if (!_options.AllowNegativeResults && value < 0)
            throw CalculationException.NegativeResultNotAllowed();

        return Math.Round(value, _options.Precision, MidpointRounding.AwayFromZero);
    }

    private void LogError(Exception ex, string operation, params double[] args)
    {
        var errorCode = ex is CalculationException cex ? cex.ErrorCode : "UNKNOWN";
        _logger.LogError(ex, "Ошибка в операции {Operation}({Args}): [{ErrorCode}] {Message}",
            operation, 
            string.Join(", ", args.Select(a => a.ToString(CultureInfo.InvariantCulture))), 
            errorCode, 
            ex.Message
            );
    }
    
    private void LogDebug(string operation, double result, params double[] args)
    {
        _logger.LogDebug("Операция {Operation}({Args}) = {Result}",
            operation, 
            string.Join(", ", args.Select(a => a.ToString(CultureInfo.InvariantCulture))), 
            result);
    }
}
