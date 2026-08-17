using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Nimbbl.Sdk.Rest.Common;
using Xunit;

namespace Nimbbl.Sdk.Rest.Test;

/// <summary>
/// Credential-free unit tests for <see cref="PayloadHelperUtils"/>, mirroring the PHP SDK's PayloadHelperUtilsTest.
/// </summary>
public class PayloadHelperUtilsTest
{
    private const string Secret = "test_secret_key_12345";

    private static string Json(object o) => JsonSerializer.Serialize(o);

    [Fact]
    public void ShouldParseSimpleJson()
    {
        var body = Json(new Dictionary<string, object?> { ["event_type"] = "payment_success", ["status"] = "success" });
        var el = PayloadHelperUtils.Parse(body, Secret);
        Assert.Equal("payment_success", el.GetProperty("event_type").GetString());
    }

    [Fact]
    public void ShouldParseTopLevelEncryptedResponse()
    {
        var inner = new Dictionary<string, object?> { ["event_type"] = "payment_success", ["status"] = "succeeded" };
        var hex = new Encryption(Secret).Encrypt(inner);
        var body = Json(new Dictionary<string, object?> { ["encrypted_response"] = hex });

        var el = PayloadHelperUtils.Parse(body, Secret);
        Assert.Equal("succeeded", el.GetProperty("status").GetString());
    }

    [Fact]
    public void ShouldUnwrapGlobalHandleCheckoutResponse()
    {
        var nested = new Dictionary<string, object?> { ["status"] = "success", ["order_id"] = "o_1" };
        var body = Json(new Dictionary<string, object?> { ["event_type"] = "globalHandleCheckoutResponse", ["payload"] = nested });

        var el = PayloadHelperUtils.Parse(body, Secret);
        Assert.Equal("success", el.GetProperty("status").GetString());
        Assert.Equal("o_1", el.GetProperty("order_id").GetString());
    }

    [Fact]
    public void ShouldParseResponseBase64Encoded()
    {
        var jsonStr = Json(new Dictionary<string, object?> { ["event_type"] = "payment_success" });
        var b64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonStr));

        var el = PayloadHelperUtils.ParseResponse(b64, Secret);
        Assert.Equal("payment_success", el.GetProperty("event_type").GetString());
    }

    [Fact]
    public void ShouldParseResponseDirectJson()
    {
        var jsonStr = Json(new Dictionary<string, object?> { ["event_type"] = "refund_success" });
        var el = PayloadHelperUtils.ParseResponse(jsonStr, Secret);
        Assert.Equal("refund_success", el.GetProperty("event_type").GetString());
    }

    [Fact]
    public void ShouldThrowOnEmptyResponse()
    {
        Assert.ThrowsAny<System.Exception>(() => PayloadHelperUtils.ParseResponse("", Secret));
    }
}
