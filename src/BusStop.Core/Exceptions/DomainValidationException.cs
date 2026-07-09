namespace BusStop.Core.Exceptions;

public sealed class DomainValidationException : Exception
{
    public string? ParameterName { get; }

    public DomainValidationException(string message, string? parameterName = null)
        : base(message)
    {
        ParameterName = parameterName;
    }
}
