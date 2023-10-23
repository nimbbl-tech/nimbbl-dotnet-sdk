namespace Nimbbl.Sdk.Rest;

public struct TokenResponse
{
    public string Token { get; set; }

    public DateTime ExpiresAt { get; set; }

    public AuthPrincipal AuthPrincipal { get; set; }

    public bool Valid => !string.IsNullOrEmpty(Token) && DateTime.UtcNow < ExpiresAt;
    public void Dispose()
    {
        ExpiresAt = DateTime.MinValue;
        Token = "";
    }
}
public class AuthPrincipal
{
    public long Id { get; set; }

    public string AccessKey { get; set; }
    public bool Active { get; set; }
    public string Type { get; set; }

    public long SubMerchantId { get; set; }

    public long NumberOfTokenPerMinute { get; set; }

    public bool SkipDeviceVerification { get; set; }
}