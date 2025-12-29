namespace Nimbbl.Sdk.Rest.Exception;

public class NimbblException : ApplicationException
{
    public int StatusCode { get; }
    public string? ErrorCode { get; }

    public NimbblException(string message, int statusCode = 0, string? errorCode = null) : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }
}

