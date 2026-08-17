using System;
using System.Collections.Generic;
using System.Text.Json;
using Nimbbl.Sdk.Rest.Common;
using NimbblException = Nimbbl.Sdk.Rest.Exception.NimbblException;
using Xunit;

namespace Nimbbl.Sdk.Rest.Test;

/// <summary>
/// Credential-free unit tests for <see cref="Encryption"/> (AES-256-GCM), mirroring the PHP SDK's EncryptionTest.
/// </summary>
public class EncryptionTest
{
    private const string Secret = "test_secret_key_12345";

    [Fact]
    public void ShouldRoundTripObject()
    {
        var enc = new Encryption(Secret);
        var hex = enc.Encrypt(new Dictionary<string, object?> { ["message"] = "hello world" });

        Assert.Matches("^[0-9a-fA-F]+$", hex);

        using var doc = JsonDocument.Parse(enc.Decrypt(hex));
        Assert.Equal("hello world", doc.RootElement.GetProperty("message").GetString());
    }

    [Fact]
    public void ShouldRoundTripNestedStructure()
    {
        var enc = new Encryption(Secret);
        var data = new Dictionary<string, object?>
        {
            ["user"] = new Dictionary<string, object?> { ["id"] = 42, ["name"] = "Alice" },
            ["items"] = new[] { "a", "b" },
        };

        using var doc = JsonDocument.Parse(enc.Decrypt(enc.Encrypt(data)));
        Assert.Equal(42, doc.RootElement.GetProperty("user").GetProperty("id").GetInt32());
        Assert.Equal("Alice", doc.RootElement.GetProperty("user").GetProperty("name").GetString());
    }

    [Fact]
    public void ShouldProduceDifferentCiphertextForSameInput()
    {
        var enc = new Encryption(Secret);
        var data = new Dictionary<string, object?> { ["k"] = "v" };
        // Random nonce per call => different ciphertext each time.
        Assert.NotEqual(enc.Encrypt(data), enc.Encrypt(data));
    }

    [Fact]
    public void ShouldFailDecryptWithWrongSecret()
    {
        var hex = new Encryption(Secret).Encrypt(new Dictionary<string, object?> { ["k"] = "v" });
        Assert.ThrowsAny<System.Exception>(() => new Encryption("a_different_secret_value").Decrypt(hex));
    }

    [Fact]
    public void ShouldThrowOnEmptySecret()
    {
        Assert.Throws<NimbblException>(() => new Encryption(""));
    }

    [Fact]
    public void ShouldStripAccessSecretPrefix()
    {
        // "access_secret_<x>" and "<x>" must derive the same key.
        var hex = new Encryption("access_secret_" + Secret).Encrypt(new Dictionary<string, object?> { ["k"] = "v" });
        using var doc = JsonDocument.Parse(new Encryption(Secret).Decrypt(hex));
        Assert.Equal("v", doc.RootElement.GetProperty("k").GetString());
    }

    [Fact]
    public void ShouldThrowOnInvalidHex()
    {
        Assert.ThrowsAny<System.Exception>(() => new Encryption(Secret).Decrypt("not-hex!!"));
    }
}
