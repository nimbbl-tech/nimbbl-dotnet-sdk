using Nimbbl.Sdk.Rest.RestClient;
namespace Nimbbl.Sdk.Rest;

public interface IUsers
{
    Task<UserResponse> GetAsync(string id);
}
internal class Users : IUsers
{
    private readonly ApiClient _apiClient;
    internal Users(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<UserResponse> GetAsync(string id)
    {
        return _apiClient.GetWithAuth<UserResponse>($"users/one/{id}");
    }

    // public Task<UserResponse> ResolveAsync(string id)
    // {
    //     return _apiClient.PostWithAuth(($"users/one/{id}");
    // }
    //resolve user request
    // {
    //     "mobile_number": "9635020816",
    //     "device_verified": null,
    //     "country_code": "+91",
    //     "order_id": "o_DZ0X4AgEwnYNr0bY",
    //     "fingerPrint": "71902f5855d5b4d46326813b24c19b0b"
    // }


    //resolve user response
    // {
    //     "success": true,
    //     "item": {
    //         "id": 83234,
    //         "email": "get.prashant.007+nimbbl@gmail.com",
    //         "first_name": "PC",
    //         "last_name": "",
    //         "country_code": "+91",
    //         "mobile_number": "9635020816",
    //         "user_id": "user_ArL0Od45BZ6jB7zP",
    //         "token": "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJ1c2VyX2lkIjo4MzIzNCwiZXhwIjoxNjUxNjEyMTU0LCJ0b2tlbl90eXBlIjoidXNlciJ9.scya6gBrlSNm3Ukn-Hd3Jf7jeC4nnxzTyRfhSYPC3wM",
    //         "token_expiration": "2022-05-03T21:09:14.356201"
    //     },
    //     "otp_sent": false,
    //     "next_step": "payment_mode"
    // }
}