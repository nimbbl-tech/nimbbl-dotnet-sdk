namespace Nimbbl.Sdk.Rest.Exception;

public class NimbblException(string message, int statusCode = 0, string? errorCode = null) : ApplicationException(message)
{
    public int StatusCode { get; } = statusCode;
    public string? ErrorCode { get; } = errorCode;
}

