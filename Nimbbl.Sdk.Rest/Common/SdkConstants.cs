using System.Reflection;

namespace Nimbbl.Sdk.Rest.Common;

public static class SdkConstants
{
    private static readonly Assembly Assembly = typeof(SdkConstants).Assembly;
    
    /// <summary>
    /// SDK name - matches Product name from assembly
    /// </summary>
    public static readonly string SdkName = Assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product 
        ?? Assembly.GetName().Name ?? "Nimbbl .NET SDK";
    
    /// <summary>
    /// SDK version - read from assembly version (git commit hash removed for cleaner display)
    /// </summary>
    public static readonly string SdkVersion = GetCleanVersion(
        Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? Assembly.GetName().Version?.ToString() 
        ?? "1.0.0");
    
    /// <summary>
    /// Removes git commit hash from version string (everything after '+' sign)
    /// Example: "1.3.5-rc6+02033c7b..." becomes "1.3.5-rc6"
    /// </summary>
    private static string GetCleanVersion(string version)
    {
        if (string.IsNullOrWhiteSpace(version))
            return version;
        
        // Remove git commit hash (everything after '+' sign)
        var plusIndex = version.IndexOf('+');
        return plusIndex >= 0 ? version.Substring(0, plusIndex) : version;
    }

    // Signature version values
    public const string SignatureVersionV3 = "v3";
}

