namespace Nimbbl.Sdk.Rest.Exception;

public class BadRequestException : NimbblException
{
    public BadRequestException(string message, int statusCode = 0, string? errorCode = null)
        : base(message, statusCode, errorCode) { }
}

