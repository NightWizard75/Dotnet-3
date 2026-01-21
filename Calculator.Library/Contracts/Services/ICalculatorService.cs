using Calculator.Library.Models;

namespace Calculator.Library.Contracts.Services;

public interface ICalculatorService
{
    CalculationResult Add(double a, double b);
    CalculationResult Subtract(double a, double b);
    CalculationResult Multiply(double a, double b);
    CalculationResult Divide(double a, double b);
    CalculationResult Power(double baseValue, double exponent);
    CalculationResult SquareRoot(double value);
}
