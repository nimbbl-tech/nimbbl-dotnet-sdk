namespace Nimbbl.Sdk.Rest;

public class NimbblClientException : ApplicationException
{
    public int HttpStatusCode { get; }
    public NimbblClientException(string message, int statusCode) : base(message)
    {
        HttpStatusCode = statusCode;
    }
}