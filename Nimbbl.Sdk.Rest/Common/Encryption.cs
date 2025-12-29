using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Nimbbl.Sdk.Rest.Exception;
using Nimbbl.Sdk.Rest.Log;

namespace Nimbbl.Sdk.Rest.Common;

/// <summary>
/// Encryption helper implementing AES-GCM Encryption/Decryption as per Nimbbl API documentation.
/// </summary>
public class Encryption
{
    private const int GCM_TAG_LENGTH = 16;
    private const int GCM_NONCE_LENGTH = 16;
    
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
        var logger = Log.Logger.GetInstance();
        try
        {
            logger.Debug($"Encryption::decrypt() called - Input length: {encryptedData.Length}, returnAsArray: {returnAsArray}");
            
            // Convert hex string to bytes
            byte[] encryptedBytes;
            try
            {
                encryptedBytes = Convert.FromHexString(encryptedData);
            }
            catch
            {
                logger.Error("Encryption::decrypt() - Invalid hex string provided");
                throw new NimbblException(ErrorMessages.InvalidHexString, 400, ErrorCodes.InvalidHexString);
            }
            
            logger.Debug($"Encryption::decrypt() - Hex conversion successful, bytes length: {encryptedBytes.Length}");

            // Verify minimum length (nonce + tag = 32 bytes minimum)
            var minLength = GCM_NONCE_LENGTH + GCM_TAG_LENGTH;
            if (encryptedBytes.Length < minLength)
            {
                logger.Error($"Encryption::decrypt() - Encrypted data too short. Expected: {minLength}, Got: {encryptedBytes.Length}");
                throw new NimbblException(
                    $"Encrypted data too short. Expected at least {minLength} bytes, got {encryptedBytes.Length}",
                    400,
                    "INVALID_ENCRYPTED_DATA"
                );
            }

            // Extract nonce (first 16 bytes)
            var nonce = new byte[GCM_NONCE_LENGTH];
            Array.Copy(encryptedBytes, 0, nonce, 0, GCM_NONCE_LENGTH);
            logger.Debug($"Encryption::decrypt() - Extracted nonce, length: {nonce.Length}");

            // Extract tag (last 16 bytes)
            var tag = new byte[GCM_TAG_LENGTH];
            Array.Copy(encryptedBytes, encryptedBytes.Length - GCM_TAG_LENGTH, tag, 0, GCM_TAG_LENGTH);
            logger.Debug($"Encryption::decrypt() - Extracted tag, length: {tag.Length}");

            // Extract ciphertext (middle bytes)
            var ciphertextLength = encryptedBytes.Length - GCM_NONCE_LENGTH - GCM_TAG_LENGTH;
            var ciphertext = new byte[ciphertextLength];
            Array.Copy(encryptedBytes, GCM_NONCE_LENGTH, ciphertext, 0, ciphertextLength);
            logger.Debug($"Encryption::decrypt() - Extracted ciphertext, length: {ciphertext.Length}");

            // Decrypt with AES-256-GCM
            logger.Debug("Encryption::decrypt() - Decrypting with AES-256-GCM");
            byte[] plaintext;
            using (var aesGcm = new AesGcm(_encryptionKey, GCM_TAG_LENGTH))
            {
                plaintext = new byte[ciphertextLength];
                aesGcm.Decrypt(nonce, ciphertext, tag, plaintext);
            }

            logger.Debug($"Encryption::decrypt() - Decryption successful, plaintext length: {plaintext.Length}");

            var plaintextString = Encoding.UTF8.GetString(plaintext);

            // Return as array if requested and data is valid JSON
            if (returnAsArray)
            {
                try
                {
                    var decoded = JsonSerializer.Deserialize<Dictionary<string, object?>>(plaintextString);
                    if (decoded != null)
                    {
                        logger.Debug("Encryption::decrypt() - JSON decode successful, returning array");
                        return JsonSerializer.Serialize(decoded);
                    }
                }
                catch
                {
                    logger.Debug("Encryption::decrypt() - JSON decode failed, returning plaintext");
                }
            }

            logger.Debug("Encryption::decrypt() - Returning plaintext");
            return plaintextString;
        }
        catch (NimbblException)
        {
            throw;
        }
        catch (System.Exception ex)
        {
            logger.Exception($"Decryption error: {ex.Message}", ex);
            throw new NimbblException(string.Format(ErrorMessages.DecryptionError, ex.Message), 500, ErrorCodes.DecryptionError);
        }
    }

    /// <summary>
    /// Encrypt data using AES-GCM
    /// </summary>
    public string Encrypt(object data)
    {
        var logger = Log.Logger.GetInstance();
        try
        {
            logger.Debug($"Encryption::encrypt() called - Input type: {data.GetType().Name}");
            
            // Convert data to string
            string plaintext;
            if (data is string str)
            {
                plaintext = str;
            }
            else
            {
                plaintext = JsonSerializer.Serialize(data);
                logger.Debug($"Encryption::encrypt() - Converted to JSON, length: {plaintext.Length}");
            }

            var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);

            // Generate random nonce (IV)
            var nonce = new byte[GCM_NONCE_LENGTH];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(nonce);
            }
            logger.Debug($"Encryption::encrypt() - Generated random nonce, length: {nonce.Length}");

            // Encrypt with AES-256-GCM
            logger.Debug($"Encryption::encrypt() - Encrypting with AES-256-GCM, plaintext length: {plaintextBytes.Length}");
            var ciphertext = new byte[plaintextBytes.Length];
            var tag = new byte[GCM_TAG_LENGTH];
            
            using (var aesGcm = new AesGcm(_encryptionKey, GCM_TAG_LENGTH))
            {
                aesGcm.Encrypt(nonce, plaintextBytes, ciphertext, tag);
            }

            logger.Debug($"Encryption::encrypt() - Encryption successful, ciphertext length: {ciphertext.Length}");

            // Concatenate: nonce + ciphertext + tag
            var encryptedData = new byte[nonce.Length + ciphertext.Length + tag.Length];
            Array.Copy(nonce, 0, encryptedData, 0, nonce.Length);
            Array.Copy(ciphertext, 0, encryptedData, nonce.Length, ciphertext.Length);
            Array.Copy(tag, 0, encryptedData, nonce.Length + ciphertext.Length, tag.Length);
            
            logger.Debug($"Encryption::encrypt() - Concatenated encrypted data, total length: {encryptedData.Length} bytes");

            // Convert to hex string
            var hexResult = Convert.ToHexString(encryptedData).ToLowerInvariant();
            logger.Debug($"Encryption::encrypt() - Converted to hex string, length: {hexResult.Length}");
            
            return hexResult;
        }
        catch (NimbblException)
        {
            throw;
        }
        catch (System.Exception ex)
        {
            logger.Exception($"Encryption error: {ex.Message}", ex);
            throw new NimbblException(string.Format(ErrorMessages.EncryptionError, ex.Message), 500, ErrorCodes.EncryptionError);
        }
    }
}
