namespace Calculator.Library.Configuration;

public class CalculatorOptions
{
    public double MaxValue { get; init; } = double.MaxValue;
    public int Precision { get; init; } = 15;
    public bool AllowNegativeResults { get; init; } = true;
}
