namespace AutoRent.Backend.Shared.Validation;

public static class Guard
{
    public static T NotNull<T>(T? value, string parameterName)
        where T : class
    {
        return value ?? throw new ArgumentNullException(parameterName);
    }

    public static string NotBlank(string? value, string parameterName)
    {
        return string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("Value cannot be empty.", parameterName)
            : value.Trim();
    }

    public static Guid NotEmpty(Guid value, string parameterName)
    {
        return value == Guid.Empty
            ? throw new ArgumentException("Value cannot be empty.", parameterName)
            : value;
    }

    public static int InRange(int value, int min, int max, string parameterName)
    {
        if (min > max)
        {
            throw new ArgumentException("Minimum value cannot be greater than maximum value.", nameof(min));
        }

        return value < min || value > max
            ? throw new ArgumentOutOfRangeException(parameterName, value, $"Value must be between {min} and {max}.")
            : value;
    }

    public static decimal NonNegative(decimal value, string parameterName)
    {
        return value < 0
            ? throw new ArgumentOutOfRangeException(parameterName, value, "Value cannot be negative.")
            : value;
    }
}
