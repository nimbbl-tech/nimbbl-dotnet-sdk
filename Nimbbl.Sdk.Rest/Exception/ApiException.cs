namespace Nimbbl.Sdk.Rest.Exception;

public class ApiException : NimbblException
{
    public ApiException(string message, int statusCode = 0, string? errorCode = null)
        : base(message, statusCode, errorCode) { }
}

