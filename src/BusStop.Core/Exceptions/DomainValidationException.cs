namespace BusStop.Core.Exceptions;

/// <summary>
/// Domain guard exception reserved for future use when domain invariants move from
/// the Result pattern to exception-driven enforcement.  Currently no code throws this;
/// the <c>DomainExceptionBehavior</c> pipeline in Web serves as a safety net for later
/// adoption.  Deferred item — do not remove.
/// </summary>
public sealed class DomainValidationException : Exception
{
    public string? ParameterName { get; }

    public DomainValidationException(string message, string? parameterName = null)
        : base(message)
    {
        ParameterName = parameterName;
    }
}
