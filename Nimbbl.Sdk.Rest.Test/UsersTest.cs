using System.Threading.Tasks;
using Xunit;
namespace Nimbbl.Sdk.Rest.Test;

public class UsersTest
{
    private readonly IUsers _users;

    public UsersTest()
    {
        var config = new Config("https://api.nimbbl.tech/api/", "access_key_1MwvMkKkweorz0ry", "access_secret_81x7ByYkRpB4g05N");
        _users = new NimbblClient(config).Users;
    }

    [Fact]
    public async Task ShouldGetUserById()
    {
        var transaction = await _users.GetAsync("137");
        AssertTransactionsMatch(ExpectedUser(), transaction);
    }

    private static void AssertTransactionsMatch(UserResponse expected, UserResponse transaction)
    {
        Assert.Equal(expected.Id, transaction.Id);
        Assert.Equal(expected.Email, transaction.Email);
        Assert.Equal(expected.FirstName, transaction.FirstName);
        Assert.Equal(expected.LastName, transaction.LastName);
        Assert.Equal(expected.MobileNumber, transaction.MobileNumber);
        Assert.Equal(expected.CountryCode, transaction.CountryCode);
        Assert.Equal(expected.UserId, transaction.UserId);
    }

    private UserResponse ExpectedUser()
    {
        return new()
        {
            Id = 137,
            Email = "anurag.theprophet@gmail.com",
            FirstName = "Anurag",
            LastName = "Pandey",
            CountryCode = "+91",
            MobileNumber = "09769507476",
            UserId = "user_BrQv9DQAgVQKG7zg"
        };
    }
}