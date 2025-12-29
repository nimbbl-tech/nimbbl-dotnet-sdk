namespace Nimbbl.Sdk.Rest.Exception;

public class RateLimitException : NimbblException
{
    public RateLimitException(string message, int statusCode = 0, string? errorCode = null)
        : base(message, statusCode, errorCode) { }
}

