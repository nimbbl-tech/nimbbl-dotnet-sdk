namespace Nimbbl.Sdk.Rest;

public class Config
{
    public string Url { get; } = "https://api.nimbbl.tech/api/";

    public string Key { get; } = "access_key_1MwvMkKkweorz0ry";

    public string Secret { get; } = "access_secret_81x7ByYkRpB4g05N";

    public Config(string baseUrl, string key, string secret)
    {
        Url = baseUrl;
        Key = key;
        Secret = secret;
    }
}