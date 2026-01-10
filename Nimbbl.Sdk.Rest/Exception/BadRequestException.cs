namespace Nimbbl.Sdk.Rest.Exception;

public class BadRequestException(string message, int statusCode = 0, string? errorCode = null) 
    : NimbblException(message, statusCode, errorCode);

