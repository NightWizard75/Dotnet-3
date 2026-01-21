using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Calculator.Library.Configuration;
using Calculator.Library.Exceptions;
using Calculator.Library.Services;

namespace Calculator.Library.Tests;

public class CalculationServiceTest
{
    /// <summary>
    /// Вспомогательный метод: создаёт экземпляр "CalculatorService" с заданной конфигурацией.
    /// Использует мок-логгер, чтобы избежать побочных эффектов.
    /// Нельзя использовать общий мок (как new() в поле класса), потому что каждый тест требует изолированного мока.
    /// </summary>
    private static CalculatorService CreateService(CalculatorOptions options)
    {
        var optionsWrapper = Options.Create(options);
        var loggerMock = new Mock<ILogger<CalculatorService>>();
        return new CalculatorService(optionsWrapper, loggerMock.Object);
    }

    // ========================================================================
    // Тесты метода Add
    // ========================================================================

    [Fact]
    public void Add_PositiveNumbers_ReturnsRoundedResult()
    {
        // Arrange
        var options = new CalculatorOptions { Precision = 2 };
        var service = CreateService(options);

        // Act
        var result = service.Add(2.555, 3.777);

        // Assert
        Assert.Equal(6.33, result.Value);
    }

    [Fact]
    public void Add_NegativeResult_AllowedWhenConfigured()
    {
        // Arrange
        var options = new CalculatorOptions { AllowNegativeResults = true };
        var service = CreateService(options);

        // Act
        var result = service.Add(-5, 2);

        // Assert
        Assert.Equal(-3, result.Value);
    }

    [Fact]
    public void Add_NegativeResult_ThrowsWhenNotAllowed()
    {
        // Arrange
        var options = new CalculatorOptions { AllowNegativeResults = false };
        var service = CreateService(options);

        // Act & Assert
        var ex = Assert.Throws<CalculationException>(() => service.Add(-5, 2));
        Assert.Contains("Отрицательные результаты запрещены", ex.Message);
    }

    // ========================================================================
    // Тесты метода Divide
    // ========================================================================

    [Fact]
    public void Divide_ByZero_ThrowsClearException()
    {
        // Arrange
        var service = CreateService(new CalculatorOptions());

        // Act & Assert
        var ex = Assert.Throws<CalculationException>(() => service.Divide(10, 0));
        Assert.Equal("Деление на ноль запрещено.", ex.Message);
    }

    [Fact]
    public void Divide_ValidNumbers_ReturnsRoundedResult()
    {
        // Arrange
        var options = new CalculatorOptions { Precision = 3 };
        var service = CreateService(options);

        // Act
        var result = service.Divide(10, 3);

        // Assert
        Assert.Equal(3.333, result.Value);
    }

    // ========================================================================
    // Тесты метода SquareRoot
    // ========================================================================

    [Fact]
    public void SquareRoot_NegativeValue_ThrowsException()
    {
        // Arrange
        var service = CreateService(new CalculatorOptions());

        // Act & Assert
        var ex = Assert.Throws<CalculationException>(() => service.SquareRoot(-4));
        Assert.Equal("Извлечение квадратного корня из отрицательного числа запрещено.", ex.Message);
    }

    [Fact]
    public void SquareRoot_PositiveValue_ReturnsCorrectResult()
    {
        // Arrange
        var options = new CalculatorOptions { Precision = 4 };
        var service = CreateService(options);

        // Act
        var result = service.SquareRoot(2);

        // Assert
        Assert.Equal(1.4142, result.Value);
    }

    // ========================================================================
    // Тесты возведения в степень (Power)
    // ========================================================================

    [Fact]
    public void Power_ZeroToNegative_ThrowsException()
    {
        // Arrange
        var service = CreateService(new CalculatorOptions());

        // Act & Assert
        var ex = Assert.Throws<CalculationException>(() => service.Power(0, -2));
        Assert.Equal("Результат вычисления недопустим: возведение нуля в отрицательную степень.", ex.Message);
    }

    [Fact]
    public void Power_ValidBaseAndExponent_ReturnsRoundedResult()
    {
        // Arrange
        var options = new CalculatorOptions { Precision = 2 };
        var service = CreateService(options);

        // Act
        var result = service.Power(2, 3);

        // Assert
        Assert.Equal(8, result.Value);
    }

    // ========================================================================
    // Тест валидации по MaxValue
    // ========================================================================
    
    [Fact]
    public void Add_ResultExceedsMaxValue_ThrowsException()
    {
        // Arrange
        var options = new CalculatorOptions { MaxValue = 100 };
        var service = CreateService(options);

        // Act & Assert
        var ex = Assert.Throws<CalculationException>(() => service.Add(90, 20));
        Assert.Contains("превышает допустимое значение: 100", ex.Message);
    }

    // ========================================================================
    // Тест специальных значений (NaN, Infinity)
    // ========================================================================

    [Fact]
    public void Add_NaNInput_ThrowsException()
    {
        // Arrange
        var service = CreateService(new CalculatorOptions());

        // Act & Assert
        var ex = Assert.Throws<CalculationException>(() => service.Add(double.NaN, 5));
        Assert.Equal("Результат вычисления недопустим: результат содержит NaN или Infinity.", ex.Message);
    }

    // ========================================================================
    // Тест логирования через мок
    // ========================================================================
    
    [Fact]
    public void Add_ValidNumbers_LogsDebugMessage()
    {
        // Arrange
        var options = new CalculatorOptions();
        var loggerMock = new Mock<ILogger<CalculatorService>>();
        var service = new CalculatorService(Options.Create(options), loggerMock.Object);

        // Act
        _ = service.Add(1, 2);

        
        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                // Если делать используя лямбду то выдается ошибка
                // An expression tree lambda cannot contain conditional access expressions
                It.Is<It.IsAnyType>((v, t) => LogMessageContainsMethodName(v, "Add")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
    
    // Вспомогательный метод — НЕ внутри лямбды!
    private static bool LogMessageContainsMethodName(object? value, string methodName)
    {
        return value?.ToString()?.Contains(methodName) == true;
    }
}
