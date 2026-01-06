using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Nimbbl.Sdk.Rest.Exception;
using Nimbbl.Sdk.Rest.Log;

namespace Nimbbl.Sdk.Rest.Common;

/// <summary>
/// Encryption helper implementing AES-GCM Encryption/Decryption as per Nimbbl API documentation.
/// Uses BouncyCastle library for full 16-byte nonce support (required by Nimbbl specification).
/// </summary>
public class Encryption
{
    private const int GCM_TAG_LENGTH = 16; // 16 bytes = 128 bits (authentication tag)
    private const int GCM_NONCE_LENGTH = 16; // Nimbbl spec uses 16 bytes for nonce
    
    private readonly byte[] _encryptionKey;
    private readonly int _keyIterations;

    /// <summary>
    /// Constructor
    /// </summary>
    public Encryption(string accessSecret, int keyIterations = 1)
    {
        if (string.IsNullOrWhiteSpace(accessSecret))
        {
            throw new NimbblException(ErrorMessages.AccessSecretRequired, 400, ErrorCodes.InvalidAccessSecret);
        }

        _keyIterations = keyIterations;
        _encryptionKey = GenerateKey(accessSecret);
    }

    private byte[] GenerateKey(string accessSecret)
    {
        // Remove "access_secret_" prefix
        var keyString = accessSecret.Replace("access_secret_", "", StringComparison.Ordinal);
        
        // Generate SHA256 hash (with iterations)
        byte[] byteKey = Encoding.UTF8.GetBytes(keyString);
        for (int i = 0; i < _keyIterations; i++)
        {
            using var sha256 = SHA256.Create();
            byteKey = sha256.ComputeHash(byteKey);
        }
        
        return byteKey;
    }

    /// <summary>
    /// Decrypt data using AES-GCM
    /// </summary>
    public string Decrypt(string encryptedData, bool returnAsArray = false)
    {
        var logger = Logger.GetInstance();
        try
        {
            logger.DebugWithCaller($"Encryption::decrypt() called - Input length: {encryptedData.Length}, returnAsArray: {returnAsArray}");
            
            // Convert hex string to bytes
            byte[] encryptedBytes;
            try
            {
                encryptedBytes = Convert.FromHexString(encryptedData);
            }
            catch
            {
                logger.ErrorWithCaller("Encryption::decrypt() - Invalid hex string provided");
                throw new NimbblException(ErrorMessages.InvalidHexString, 400, ErrorCodes.InvalidHexString);
            }
            
            logger.DebugWithCaller($"Encryption::decrypt() - Hex conversion successful, bytes length: {encryptedBytes.Length}");

            // Verify minimum length (nonce + tag = 32 bytes minimum)
            var minLength = GCM_NONCE_LENGTH + GCM_TAG_LENGTH;
            if (encryptedBytes.Length < minLength)
            {
                logger.ErrorWithCaller($"Encryption::decrypt() - Encrypted data too short. Expected: {minLength}, Got: {encryptedBytes.Length}");
                throw new NimbblException(
                    $"Encrypted data too short. Expected at least {minLength} bytes, got {encryptedBytes.Length}",
                    400,
                    "INVALID_ENCRYPTED_DATA"
                );
            }

            // Extract nonce (first 16 bytes per Nimbbl spec)
            var nonce = new byte[GCM_NONCE_LENGTH];
            Array.Copy(encryptedBytes, 0, nonce, 0, GCM_NONCE_LENGTH);
            logger.DebugWithCaller($"Encryption::decrypt() - Extracted nonce, length: {nonce.Length}");

            // Extract tag (last 16 bytes)
            var tag = new byte[GCM_TAG_LENGTH];
            Array.Copy(encryptedBytes, encryptedBytes.Length - GCM_TAG_LENGTH, tag, 0, GCM_TAG_LENGTH);
            logger.DebugWithCaller($"Encryption::decrypt() - Extracted tag, length: {tag.Length}");

            // Extract ciphertext (middle bytes)
            var ciphertextLength = encryptedBytes.Length - GCM_NONCE_LENGTH - GCM_TAG_LENGTH;
            var ciphertext = new byte[ciphertextLength];
            Array.Copy(encryptedBytes, GCM_NONCE_LENGTH, ciphertext, 0, ciphertextLength);
            logger.DebugWithCaller($"Encryption::decrypt() - Extracted ciphertext, length: {ciphertext.Length}");

            // Decrypt with AES-256-GCM using BouncyCastle (supports 16-byte nonce)
            logger.DebugWithCaller("Encryption::decrypt() - Decrypting with AES-256-GCM using BouncyCastle");
            byte[] plaintext;
            
            var cipher = new GcmBlockCipher(new AesEngine());
            var keyParam = new KeyParameter(_encryptionKey);
            var parameters = new AeadParameters(keyParam, GCM_TAG_LENGTH * 8, nonce);
            cipher.Init(false, parameters); // false = decrypt mode
            
            // Combine ciphertext and tag for BouncyCastle (it expects them together)
            var ciphertextWithTag = new byte[ciphertext.Length + tag.Length];
            Array.Copy(ciphertext, 0, ciphertextWithTag, 0, ciphertext.Length);
            Array.Copy(tag, 0, ciphertextWithTag, ciphertext.Length, tag.Length);
            
            plaintext = new byte[cipher.GetOutputSize(ciphertextWithTag.Length)];
            var len = cipher.ProcessBytes(ciphertextWithTag, 0, ciphertextWithTag.Length, plaintext, 0);
            cipher.DoFinal(plaintext, len);

            logger.DebugWithCaller($"Encryption::decrypt() - Decryption successful, plaintext length: {plaintext.Length}");

            var plaintextString = Encoding.UTF8.GetString(plaintext);

            // Return as array if requested and data is valid JSON
            if (returnAsArray)
            {
                try
                {
                    var decoded = JsonSerializer.Deserialize<Dictionary<string, object?>>(plaintextString);
                    if (decoded != null)
                    {
                        logger.DebugWithCaller("Encryption::decrypt() - JSON decode successful, returning array");
                        return JsonSerializer.Serialize(decoded);
                    }
                }
                catch
                {
                    logger.DebugWithCaller("Encryption::decrypt() - JSON decode failed, returning plaintext");
                }
            }

            logger.DebugWithCaller("Encryption::decrypt() - Returning plaintext");
            return plaintextString;
        }
        catch (NimbblException)
        {
            throw;
        }
        catch (System.Exception ex)
        {
            logger.ExceptionWithCaller($"Decryption error: {ex.Message}", ex);
            throw new NimbblException(string.Format(ErrorMessages.DecryptionError, ex.Message), 500, ErrorCodes.DecryptionError);
        }
    }

    /// <summary>
    /// Encrypt data using AES-GCM
    /// </summary>
    public string Encrypt(object data)
    {
        var logger = Logger.GetInstance();
        try
        {
            logger.DebugWithCaller($"Encryption::encrypt() called - Input type: {data.GetType().Name}");
            
            // Convert data to string
            string plaintext;
            if (data is string str)
            {
                plaintext = str;
            }
            else
            {
                plaintext = JsonSerializer.Serialize(data);
                logger.DebugWithCaller($"Encryption::encrypt() - Converted to JSON, length: {plaintext.Length}");
            }

            var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);

            // Generate random nonce (IV) - 16 bytes as per Nimbbl specification
            var nonce = new byte[GCM_NONCE_LENGTH];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(nonce);
            }
            logger.DebugWithCaller($"Encryption::encrypt() - Generated random nonce, length: {nonce.Length}");

            // Encrypt with AES-256-GCM using BouncyCastle (supports 16-byte nonce)
            logger.DebugWithCaller($"Encryption::encrypt() - Encrypting with AES-256-GCM using BouncyCastle, plaintext length: {plaintextBytes.Length}");
            
            var cipher = new GcmBlockCipher(new AesEngine());
            var keyParam = new KeyParameter(_encryptionKey);
            var parameters = new AeadParameters(keyParam, GCM_TAG_LENGTH * 8, nonce);
            cipher.Init(true, parameters); // true = encrypt mode
            
            var encrypted = new byte[cipher.GetOutputSize(plaintextBytes.Length)];
            var len = cipher.ProcessBytes(plaintextBytes, 0, plaintextBytes.Length, encrypted, 0);
            cipher.DoFinal(encrypted, len);
            
            logger.DebugWithCaller($"Encryption::encrypt() - Encryption successful, encrypted data length: {encrypted.Length}");

            // BouncyCastle returns ciphertext + tag together
            // We need to separate them: ciphertext is all but last 16 bytes, tag is last 16 bytes
            var ciphertextLength = encrypted.Length - GCM_TAG_LENGTH;
            var ciphertext = new byte[ciphertextLength];
            var tag = new byte[GCM_TAG_LENGTH];
            Array.Copy(encrypted, 0, ciphertext, 0, ciphertextLength);
            Array.Copy(encrypted, ciphertextLength, tag, 0, GCM_TAG_LENGTH);

            // Concatenate: nonce (16 bytes) + ciphertext + tag (16 bytes) as per Nimbbl spec
            var encryptedData = new byte[nonce.Length + ciphertext.Length + tag.Length];
            Array.Copy(nonce, 0, encryptedData, 0, nonce.Length);
            Array.Copy(ciphertext, 0, encryptedData, nonce.Length, ciphertext.Length);
            Array.Copy(tag, 0, encryptedData, nonce.Length + ciphertext.Length, tag.Length);
            
            logger.DebugWithCaller($"Encryption::encrypt() - Concatenated encrypted data, total length: {encryptedData.Length} bytes");

            // Convert to hex string
            var hexResult = Convert.ToHexString(encryptedData).ToLowerInvariant();
            logger.DebugWithCaller($"Encryption::encrypt() - Converted to hex string, length: {hexResult.Length}");
            
            return hexResult;
        }
        catch (NimbblException)
        {
            throw;
        }
        catch (System.Exception ex)
        {
            logger.ExceptionWithCaller($"Encryption error: {ex.Message}", ex);
            throw new NimbblException(string.Format(ErrorMessages.EncryptionError, ex.Message), 500, ErrorCodes.EncryptionError);
        }
    }
}
