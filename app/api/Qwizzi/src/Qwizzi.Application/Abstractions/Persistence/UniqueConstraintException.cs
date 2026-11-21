namespace Qwizzi.Application.Abstractions.Persistence;

public sealed class UniqueConstraintException : Exception
{
    public UniqueConstraintException(Exception innerException)
        : base(
            "The operation violates a unique constraint.",
            innerException)
    {
    }
}