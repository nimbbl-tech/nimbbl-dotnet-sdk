namespace Nimbbl.Sdk.Rest.Exception;

public class NotFoundException(string message, int statusCode = 0, string? errorCode = null) 
    : NimbblException(message, statusCode, errorCode);

