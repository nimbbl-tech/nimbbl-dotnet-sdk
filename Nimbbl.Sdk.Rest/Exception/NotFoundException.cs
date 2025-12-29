namespace Nimbbl.Sdk.Rest.Exception;

public class NotFoundException : NimbblException
{
    public NotFoundException(string message, int statusCode = 0, string? errorCode = null)
        : base(message, statusCode, errorCode) { }
}

