namespace Calculator.Library.Exceptions;

public class CalculationException(string message, string? errorCode = null)
    : Exception(message)
{
    public string ErrorCode { get; } = errorCode ?? "CALC_GENERAL";

    // Статические фабрики
    public static CalculationException DivisionByZero() =>
        new("Деление на ноль запрещено.", "DIV_BY_ZERO");

    public static CalculationException NegativeSquareRoot() =>
        new("Извлечение квадратного корня из отрицательного числа запрещено.", "NEGATIVE_SQRT");

    public static CalculationException InvalidResult(string reason) =>
        new($"Результат вычисления недопустим: {reason}.", "INVALID_RESULT");

    public static CalculationException ResultTooLarge(double maxValue) =>
        new($"Результат превышает допустимое значение: {maxValue}.", "RESULT_TOO_LARGE");

    public static CalculationException NegativeResultNotAllowed() =>
        new("Отрицательные результаты запрещены в текущей конфигурации.", "NEGATIVE_RESULT");
}
