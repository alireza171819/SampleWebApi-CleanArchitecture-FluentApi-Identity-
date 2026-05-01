
namespace Domain.Exceptions;

/// <summary>
/// Represents an exception that is thrown when a domain rule or invariant is violated in the domain layer.
/// </summary>
/// <remarks>
/// This exception should be used to enforce domain constraints, invariants, and business rules
/// within the domain model. It typically indicates that an operation attempted to put the domain
/// entity or aggregate into an invalid state.
/// </remarks>
public class DomainException : Exception
{
    public DomainException() { }

    public DomainException(string message)
        : base(message)
    {
    }

    public DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
