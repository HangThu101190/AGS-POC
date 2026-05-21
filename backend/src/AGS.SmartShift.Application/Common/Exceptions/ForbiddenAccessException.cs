namespace AGS.SmartShift.Application.Common.Exceptions;

public sealed class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException(string? message = null)
        : base(message ?? "Access to this resource is not allowed.")
    {
    }
}
