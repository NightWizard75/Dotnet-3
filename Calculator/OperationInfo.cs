using Calculator.Library.Models;

namespace Calculator;

public record OperationInfo(
    string Name,
    Func<double, double, CalculationResult> Handler,
    bool IsUnary = false
    );
    