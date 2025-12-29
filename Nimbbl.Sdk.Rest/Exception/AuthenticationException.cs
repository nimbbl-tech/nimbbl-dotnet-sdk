namespace Nimbbl.Sdk.Rest.Exception;

public class AuthenticationException : NimbblException
{
    public AuthenticationException(string message, int statusCode = 0, string? errorCode = null)
        : base(message, statusCode, errorCode) { }
}

