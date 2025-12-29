namespace Nimbbl.Sdk.Rest.Exception;

public class ServerException : NimbblException
{
    public ServerException(string message, int statusCode = 0, string? errorCode = null)
        : base(message, statusCode, errorCode) { }
}

