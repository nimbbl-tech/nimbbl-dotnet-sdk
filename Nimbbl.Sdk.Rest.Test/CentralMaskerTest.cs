using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using Nimbbl.Sdk.Rest.Common;
using Xunit;

namespace Nimbbl.Sdk.Rest.Test;

/// <summary>
/// Parity tests for <see cref="CentralMasker"/> — mirrors the PHP SDK's CentralMaskerTest.
/// </summary>
public class CentralMaskerTest
{
    private static string? Field(string maskedJson, params string[] path)
    {
        using var doc = JsonDocument.Parse(maskedJson);
        var el = doc.RootElement;
        foreach (var p in path)
        {
            el = el.GetProperty(p);
        }
        return el.GetString();
    }

    [Fact]
    public void ShouldMaskHeaders()
    {
        var req = new HttpRequestMessage();
        req.Headers.TryAddWithoutValidation("Authorization", "Bearer secret_token_12345");
        req.Headers.TryAddWithoutValidation("X-Custom", "public_value");

        var masked = CentralMasker.MaskHeaders(req.Headers);

        Assert.Equal("Bear***2345", masked["Authorization"]);
        Assert.Equal("public_value", masked["X-Custom"]);
    }

    [Fact]
    public void ShouldMaskBodyJson()
    {
        var json = JsonSerializer.Serialize(new Dictionary<string, object?>
        {
            ["access_key"] = "access_key_1234567890",
            ["token"] = "eyJhbGabcdefghijklmng59w",
            ["first_name"] = "John",
            ["mobile_number"] = "9876543210",
            ["email"] = "john.doe@example.com",
            ["card_no"] = "4111111111111111",
            ["cvv"] = "123",
            ["public_field"] = "visible",
        });

        var masked = CentralMasker.MaskBody(json);

        Assert.Equal("acce****7890", Field(masked, "access_key"));
        Assert.Equal("eyJhb***********lmng59w", Field(masked, "token"));
        Assert.Equal("J***", Field(masked, "first_name"));
        Assert.Equal("******3210", Field(masked, "mobile_number"));
        Assert.Equal("jo****oe@example.com", Field(masked, "email"));
        Assert.Equal("**** **** **** 1111", Field(masked, "card_no"));
        Assert.Equal("***", Field(masked, "cvv"));
        Assert.Equal("visible", Field(masked, "public_field"));
    }

    [Fact]
    public void ShouldMaskNestedBodyJson()
    {
        var json = JsonSerializer.Serialize(new Dictionary<string, object?>
        {
            ["user"] = new Dictionary<string, object?>
            {
                ["first_name"] = "Alice",
                ["address"] = new Dictionary<string, object?>
                {
                    ["street"] = "123 Main St",
                    ["city"] = "Metropolis",
                },
            },
        });

        var masked = CentralMasker.MaskBody(json);

        Assert.Equal("A****", Field(masked, "user", "first_name"));
        Assert.Equal("1** M*** S*", Field(masked, "user", "address", "street"));
        Assert.Equal("Me********", Field(masked, "user", "address", "city"));
    }

    [Fact]
    public void ShouldMaskResponseSidePiiKeys()
    {
        // Webhook/callback RESPONSE payloads use short PII field names (name/mobile/card_holder/state).
        var json = JsonSerializer.Serialize(new Dictionary<string, object?>
        {
            ["user"] = new Dictionary<string, object?>
            {
                ["name"] = "John Doe",
                ["mobile"] = "9876543210",
                ["email"] = "customer@example.com",
            },
            ["transaction"] = new Dictionary<string, object?>
            {
                ["sub_payment_mode"] = new Dictionary<string, object?> { ["card_holder"] = "sandeepkumar" },
                ["state"] = "Maharashtra",
            },
        });

        var masked = CentralMasker.MaskBody(json);

        Assert.NotEqual("John Doe", Field(masked, "user", "name"));
        Assert.NotEqual("9876543210", Field(masked, "user", "mobile"));
        Assert.NotEqual("customer@example.com", Field(masked, "user", "email"));
        Assert.NotEqual("sandeepkumar", Field(masked, "transaction", "sub_payment_mode", "card_holder"));
        Assert.NotEqual("Maharashtra", Field(masked, "transaction", "state"));
        // sanity: phone keeps last 4
        Assert.EndsWith("3210", Field(masked, "user", "mobile"));
    }
}
