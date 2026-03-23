using System;
using System.IO;

namespace Nimbbl.Sdk.Rest.Test;

/// <summary>
/// Utility class for loading environment variables from .env files
/// </summary>
public static class EnvLoader
{
    private static bool _isLoaded = false;
    private static readonly object _lock = new object();

    /// <summary>
    /// Loads environment variables from a .env file if it exists.
    /// Looks for .env in the current directory and parent directories up to 5 levels.
    /// Only sets variables that are not already set in the environment.
    /// This method is idempotent - it will only load the .env file once, even if called multiple times.
    /// </summary>
    public static void LoadEnvFile()
    {
        // Early return if already loaded (double-checked locking pattern)
        if (_isLoaded)
        {
            return;
        }

        lock (_lock)
        {
            // Check again inside lock to prevent race conditions
            if (_isLoaded)
            {
                return;
            }

            // Try to find .env file starting from current directory and going up
            var currentDir = Directory.GetCurrentDirectory();
            var envFile = FindEnvFile(currentDir);
            
            if (envFile != null && File.Exists(envFile))
            {
                LoadEnvFile(envFile);
            }

            // Mark as loaded after attempting to load (even if file not found)
            _isLoaded = true;
        }
    }

    /// <summary>
    /// Loads environment variables from a specific .env file path
    /// </summary>
    /// <param name="envFilePath">Path to the .env file</param>
    public static void LoadEnvFile(string envFilePath)
    {
        if (!File.Exists(envFilePath))
        {
            return;
        }

        var lines = File.ReadAllLines(envFilePath);
        foreach (var line in lines)
        {
            // Skip comments and empty lines
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#'))
                continue;
            
            // Parse KEY=VALUE format
            var equalIndex = trimmed.IndexOf('=');
            if (equalIndex > 0)
            {
                var key = trimmed.Substring(0, equalIndex).Trim();
                var value = trimmed.Substring(equalIndex + 1).Trim();
                
                // Remove quotes if present
                if ((value.StartsWith('"') && value.EndsWith('"')) || 
                    (value.StartsWith('\'') && value.EndsWith('\'')))
                {
                    value = value.Substring(1, value.Length - 2);
                }
                
                // Only set if not already set as environment variable
                if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(key)))
                {
                    Environment.SetEnvironmentVariable(key, value);
                }
            }
        }
    }

    /// <summary>
    /// Finds .env file by searching current directory and parent directories (up to 5 levels)
    /// </summary>
    /// <param name="startDirectory">Directory to start searching from</param>
    /// <returns>Path to .env file if found, null otherwise</returns>
    private static string? FindEnvFile(string startDirectory)
    {
        var currentDir = new DirectoryInfo(startDirectory);
        var maxLevels = 5;
        var level = 0;

        while (currentDir != null && level < maxLevels)
        {
            var envFile = Path.Combine(currentDir.FullName, ".env");
            if (File.Exists(envFile))
            {
                return envFile;
            }

            currentDir = currentDir.Parent;
            level++;
        }

        return null;
    }
}
