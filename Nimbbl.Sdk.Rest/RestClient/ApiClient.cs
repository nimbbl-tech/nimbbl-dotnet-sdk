using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
[assembly: InternalsVisibleTo("Nimbbl.Sdk.Rest.Test")]
namespace Nimbbl.Sdk.Rest.RestClient;

internal class ApiClient
{
    private readonly JsonSerializerOptions _serializerOptions;
    private readonly HttpClient _client;

    private TokenResponse _token = new();
    private readonly AuthenticationService _authenticationService;
    public ApiClient(Config config)
    {
        _serializerOptions = new(JsonSerializerDefaults.Web)
        {
            AllowTrailingCommas = true,
            PropertyNamingPolicy = new SnakeCaseNamingPolicy(),
        };
        _client = new()
        {
            BaseAddress = new(config.Url)
        };
        _authenticationService = new(_client, config, _serializerOptions);
    }

    public async Task<TResponse> GetWithAuth<TResponse>(string requestUri) where TResponse : class
    {
        return await SendWithAuthGuardAsync<TResponse>(CreateRequest(HttpMethod.Get, requestUri));
    }

    public async Task<TResponse> PostWithAuth<TRequest, TResponse>(string requestUri, TRequest body)
        where TResponse : class where TRequest : class
    {
        return await SendWithAuthGuardAsync<TResponse>(CreateRequest(HttpMethod.Post, requestUri, body));
    }


    private async Task<TResponse> SendWithAuthGuardAsync<TResponse>(HttpRequestMessage message) where TResponse : class
    {
        var httpResponse = await SendResilientRequestAsync();
        var response = await httpResponse.Content.ReadFromJsonAsync<TResponse>(_serializerOptions);
        if (response == null) throw new ApplicationException("Server returned no value");
        return response;

        async Task<HttpResponseMessage> SendResilientRequestAsync()
        {
            return await WithRetry(SendRequestAsync, message, 1);
        }
    }

    private async Task<HttpResponseMessage> WithRetry(Func<HttpRequestMessage, Task<HttpResponseMessage>> request, HttpRequestMessage message, ushort retryCount)
    {
        HttpResponseMessage response;
        do
        {
            response = await request(CloneRequest(message));
            if (response.IsSuccessStatusCode) return response;
            if (IsAuthFailure(response)) _token.Dispose();
        } while (retryCount-- != 0);
        await HandleErrorResponseAsync(response);
        return response;
    }

    private static bool IsAuthFailure(HttpResponseMessage response)
    {
        return response.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.Unauthorized;
    }

    private async Task HandleErrorResponseAsync(HttpResponseMessage httpResponse)
    {
        var errorText = await httpResponse.Content.ReadAsStringAsync();
        if (!IsValidJson(errorText)) throw new NimbblClientException(errorText, (int) httpResponse.StatusCode);
        var (code, message) = JsonSerializer.Deserialize<ErrorResponse>(errorText, _serializerOptions).Error;
        throw new NimbblClientException(message, code);

        static bool IsValidJson(string errorText)
        {
            return errorText.Trim().StartsWith('{');
        }
    }
    private async Task<HttpResponseMessage> SendRequestAsync(HttpRequestMessage request)
    {
        await LazyAuthorizeRequestAsync(request);
        return await _client.SendAsync(request);
    }

    private async Task LazyAuthorizeRequestAsync(HttpRequestMessage request)
    {
        if (!_token.Valid)
        {
            _token = await _authenticationService.Authenticate();
        }
        request.Headers.Authorization = new("Bearer", _token.Token);
    }



    private static HttpRequestMessage CreateRequest(HttpMethod method, string requestUri)
    {
        return new(method, requestUri);
    }

    private HttpRequestMessage CreateRequest<TRequest>(HttpMethod method, string requestUri, TRequest body) where TRequest : class
    {
        var message = CreateRequest(method, requestUri);
        message.Content = JsonContent.Create(body, MediaTypeHeaderValue.Parse("application/json"), _serializerOptions);
        return message;
    }

    private static HttpRequestMessage CloneRequest(HttpRequestMessage message)
    {
        return new(message.Method, message.RequestUri)
        {
            Content = message.Content
        };
    }

    private record struct Error(int Code, string Message);

    private record struct ErrorResponse(Error Error);
}