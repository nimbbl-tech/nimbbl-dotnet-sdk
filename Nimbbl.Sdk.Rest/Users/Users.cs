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

}