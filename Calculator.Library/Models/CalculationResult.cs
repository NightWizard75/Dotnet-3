using System.Globalization;

namespace Calculator.Library.Models;

public class CalculationResult(double value)
{
    public double Value { get; } = value;

    public override string ToString() =>
        Value.ToString(CultureInfo.CurrentCulture);
}
