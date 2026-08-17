namespace Nimbbl.Sdk.Rest.Common;

/// <summary>
/// HTTP status code constants
/// </summary>
public static class HttpStatusCodes
{
    // SDK-level error (non-API errors like encryption/decryption failures)
    public const int Unknown = 0;

    // HTTP status codes — success
    public const int Ok = 200;
    public const int Created = 201;
    public const int Accepted = 202;
    public const int NoContent = 204;

    // HTTP status codes — client/server errors
    public const int BadRequest = 400;
    public const int Unauthorized = 401;
    public const int Forbidden = 403;
    public const int NotFound = 404;
    public const int Conflict = 409;
    public const int UnprocessableEntity = 422;
    public const int TooManyRequests = 429;
    public const int InternalServerError = 500;
    public const int BadGateway = 502;
    public const int ServiceUnavailable = 503;
    public const int GatewayTimeout = 504;
}

