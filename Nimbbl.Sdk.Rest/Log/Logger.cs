using System;
using Nimbbl.Sdk.Rest.Common;
using Nimbbl.Sdk.Rest.RestClient;

namespace Nimbbl.Sdk.Rest.Log;

/// <summary>
/// Logger with always-on format, optional debug gating, file + stdout + callback.
/// Format: [timestamp][sdk sdkVersion][level][module:line][function]: message
/// </summary>
public class Logger
{
    private const string LogLevelInfo = "INFO";
    private const string LogLevelError = "ERROR";
    private const string LogLevelDebug = "DEBUG";
    private const string LogLevelWarning = "WARNING";
    private const string LogLevelException = "EXCEPTION";

    private static Logger? _instance;
    private static bool _enableDebugLogging;
    // Logging is always enabled (INFO, WARNING, ERROR logs are always printed)
    // Only DEBUG logs are controlled by _enableDebugLogging flag

    private readonly string? _logFilePath;
    private readonly object _sync = new();

    private Logger(string? logFilePath = null, bool alreadyHasDateSuffix = false)
    {
        var pathWithDate = alreadyHasDateSuffix ? logFilePath : AddDateSuffixToLogFile(logFilePath);
        // Resolve relative paths to absolute paths
        _logFilePath = string.IsNullOrWhiteSpace(pathWithDate) 
            ? null 
            : Path.IsPathRooted(pathWithDate) 
                ? pathWithDate 
                : Path.Combine(Directory.GetCurrentDirectory(), pathWithDate);
    }

    /// <summary>
    /// Resolve the effective log file path used by the SDK (includes the date suffix).
    /// This is idempotent: if the input already ends with _ddMMyyyy before the extension, it will not add another suffix.
    /// </summary>
    public static string? ResolveLogFilePath(string? logFilePath) => AddDateSuffixToLogFile(logFilePath);

    private static string? AddDateSuffixToLogFile(string? logFilePath)
    {
        if (string.IsNullOrWhiteSpace(logFilePath)) return logFilePath;
        
        var directory = Path.GetDirectoryName(logFilePath);
        var fileName = Path.GetFileNameWithoutExtension(logFilePath);
        var extension = Path.GetExtension(logFilePath);
        var dateSuffix = DateTime.Now.ToString("ddMMyyyy");

        // If filename already ends with _ddMMyyyy, don't append again
        // Example: nimbbl_debug_06012026.log
        if (fileName.Length >= 9)
        {
            var maybeSuffix = fileName.Substring(fileName.Length - 9); // _ + 8 digits
            if (maybeSuffix[0] == '_' && maybeSuffix.Skip(1).All(char.IsDigit))
            {
                var fileNameAlreadyDated = $"{fileName}{extension}";
                return string.IsNullOrWhiteSpace(directory)
                    ? fileNameAlreadyDated
                    : Path.Combine(directory, fileNameAlreadyDated);
            }
        }
        
        var fileNameWithDate = $"{fileName}_{dateSuffix}{extension}";
        return string.IsNullOrWhiteSpace(directory) 
            ? fileNameWithDate 
            : Path.Combine(directory, fileNameWithDate);
    }

    public static Logger GetInstance(string? logFilePath = null)
    {
        var filePathWithDate = AddDateSuffixToLogFile(logFilePath);
        // Resolve to absolute path for comparison
        var absoluteFilePathWithDate = string.IsNullOrWhiteSpace(filePathWithDate) 
            ? null 
            : Path.IsPathRooted(filePathWithDate) 
                ? filePathWithDate 
                : Path.Combine(Directory.GetCurrentDirectory(), filePathWithDate);
        
        // If instance exists, check if we need to reset due to date change
        if (_instance != null)
        {
            // Check if file path is different (including date change) - compare absolute paths
            if (absoluteFilePathWithDate != null && _instance._logFilePath != absoluteFilePathWithDate)
            {
                _instance = null;
            }
            // Also check if the existing instance's file path has an old date suffix
            else if (_instance._logFilePath != null && absoluteFilePathWithDate != null)
            {
                var existingPath = _instance._logFilePath;
                var todaySuffix = DateTime.Now.ToString("ddMMyyyy");
                
                // Extract date suffix from existing path if it has one
                var existingFileName = Path.GetFileNameWithoutExtension(existingPath);
                var existingHasDateSuffix = false;
                string? existingDateSuffix = null;
                
                if (existingFileName.Length >= 9)
                {
                    var maybeSuffix = existingFileName.Substring(existingFileName.Length - 9);
                    if (maybeSuffix[0] == '_' && maybeSuffix.Skip(1).All(char.IsDigit))
                    {
                        existingHasDateSuffix = true;
                        existingDateSuffix = maybeSuffix.Substring(1); // Remove the underscore
                    }
                }
                
                // Check if new path has date suffix
                var newFileName = Path.GetFileNameWithoutExtension(absoluteFilePathWithDate);
                var newHasDateSuffix = false;
                if (newFileName.Length >= 9)
                {
                    var maybeSuffix = newFileName.Substring(newFileName.Length - 9);
                    if (maybeSuffix[0] == '_' && maybeSuffix.Skip(1).All(char.IsDigit))
                    {
                        newHasDateSuffix = true;
                    }
                }
                
                // Reset if: old path has date suffix but doesn't match today, OR old path has no date suffix but new one does
                if (existingHasDateSuffix && existingDateSuffix != todaySuffix)
                {
                    _instance = null;
                }
                else if (!existingHasDateSuffix && newHasDateSuffix)
                {
                    // Old file doesn't have date suffix, but new one should - reset to create new file
                    _instance = null;
                }
            }
        }
        
        // Pass already-calculated path with date suffix to avoid recalculating
        if (_instance == null)
        {
            _instance = new Logger(filePathWithDate, alreadyHasDateSuffix: true);
            // Debug: Log the resolved log file path (only on first initialization to avoid spam)
            if (!string.IsNullOrWhiteSpace(_instance._logFilePath))
            {
                Console.WriteLine($"[LOGGER INFO] Log file: {_instance._logFilePath}");
            }
            else
            {
                Console.WriteLine("[LOGGER WARNING] Log file path is null or empty - logging to console only");
            }
        }
        return _instance;
    }

    public static void EnableDebug() => _enableDebugLogging = true;
    public static void DisableDebug() => _enableDebugLogging = false;
    public static bool IsDebugEnabled() => _enableDebugLogging;

    /// <summary>
    /// Log INFO with optional caller info (always printed, debug logging is controlled separately)
    /// </summary>
    public void InfoWithCaller(string message, (string Module, string Function, int Line)? callerInfo = null)
    {
        var (module, function, line) = callerInfo ?? GetCaller();
        var formattedMessage = FormatMessage(message, null);
        Write(LogLevelInfo, formattedMessage, module, line, function);
    }
    
    /// <summary>
    /// Log ERROR with optional caller info (always printed, regardless of logging settings)
    /// </summary>
    public void ErrorWithCaller(string message, (string Module, string Function, int Line)? callerInfo = null)
    {
        var (module, function, line) = callerInfo ?? GetCaller();
        var formattedMessage = FormatMessage(message, null);
        Write(LogLevelError, formattedMessage, module, line, function);
    }
    
    /// <summary>
    /// Log DEBUG with optional caller info (only printed when debug logging is enabled)
    /// </summary>
    public void DebugWithCaller(string message, (string Module, string Function, int Line)? callerInfo = null)
    {
        if (!_enableDebugLogging) return;
        var (module, function, line) = callerInfo ?? GetCaller();
        var formattedMessage = FormatMessage(message, null);
        Write(LogLevelDebug, formattedMessage, module, line, function);
    }
    
    /// <summary>
    /// Log WARNING with optional caller info (always printed, regardless of logging settings)
    /// </summary>
    public void WarningWithCaller(string message, (string Module, string Function, int Line)? callerInfo = null)
    {
        var (module, function, line) = callerInfo ?? GetCaller();
        var formattedMessage = FormatMessage(message, null);
        Write(LogLevelWarning, formattedMessage, module, line, function);
    }
    
    /// <summary>
    /// Log EXCEPTION with optional caller info (always printed, regardless of logging settings)
    /// </summary>
    public void ExceptionWithCaller(string message, System.Exception ex, (string Module, string Function, int Line)? callerInfo = null)
    {
        var (module, function, line) = callerInfo ?? GetCaller();
        var formattedMessage = FormatMessage(message, ex);
        Write(LogLevelException, formattedMessage, module, line, function);
    }

    private void Write(string level, string message, string module, int line, string function)
    {
        // Debug logging check (only DEBUG logs are gated)
        if (level.Equals(LogLevelDebug, StringComparison.OrdinalIgnoreCase) && !_enableDebugLogging) return;

        // Format: [timestamp][sdk sdkVersion][level][module:line][function]: message
        var moduleName = !string.IsNullOrWhiteSpace(module) && module != "unknown" ? module : "unknown";
        var functionName = !string.IsNullOrWhiteSpace(function) ? function : "-";
        var lineNumber = line;
        var logLine = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}][{SdkConstants.SdkName} {SdkConstants.SdkVersion}][{level}][{moduleName}:{lineNumber}][{functionName}]: {message}";

        if (!string.IsNullOrWhiteSpace(_logFilePath))
        {
            try
            {
                lock (_sync)
                {
                    // _logFilePath is already absolute (resolved in constructor), but double-check
                    var absoluteLogPath = Path.IsPathRooted(_logFilePath) 
                        ? _logFilePath 
                        : Path.Combine(Directory.GetCurrentDirectory(), _logFilePath);
                    
                    var dir = Path.GetDirectoryName(absoluteLogPath);
                    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                    File.AppendAllText(absoluteLogPath, logLine + Environment.NewLine);
                }
            }
            catch (System.Exception ex)
            {
                // Log to console if file logging fails (but don't throw to avoid breaking the application)
                Console.WriteLine($"[LOGGER ERROR] Failed to write to log file '{_logFilePath}': {ex.Message}");
                Console.WriteLine($"[LOGGER ERROR] Stack trace: {ex.StackTrace}");
            }
        }
        else
        {
            // Debug: Log when _logFilePath is null/empty
            Console.WriteLine($"[LOGGER WARNING] Log file path is null or empty. Logging to console only.");
        }

        // Also write to stdout for CLI parity
        Console.WriteLine(logLine);
    }
    
    private static string FormatMessage(string message, System.Exception? exception)
    {
        if (exception != null)
        {
            return $"{message} Exception: {exception.Message}\nTrace: {exception.StackTrace}";
        }
        return message;
    }

    private static readonly HashSet<string> SkipFunctions = new(StringComparer.OrdinalIgnoreCase)
    {
        "GetCaller", "Write", "Info", "Error", "Debug", "Warning", "Critical", "Exception", "FormatMessage",
        "LogRequestAsync", "LogResponseAsync", "ExecuteRequestAsync", "RetryRequestAsync", "HandleErrorResponseAsync",
        "Post", "Get", "Put", "Delete", "Patch", // ApiClient HTTP method wrappers
        "Start", "MoveNext", "ExecutionContextCallback", "Run", "AwaitUnsafeOnCompleted", "SetResult", "SetException"
    };

    private static readonly HashSet<string> SkipFiles = new(StringComparer.OrdinalIgnoreCase)
    {
        "Logger.cs", "ApiClient.cs"
    };

    private static readonly HashSet<string> SkipTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "AsyncMethodBuilderCore", "AsyncStateMachineBox", "AsyncTaskMethodBuilder", "TaskAwaiter", "ConfiguredTaskAwaiter",
        "ApiClient", "Logger"
    };

    // Type information for skipping internal SDK types from stack traces
    private static readonly string ApiClientTypeName = typeof(ApiClient).Name;
    private static readonly string ApiClientFullName = typeof(ApiClient).FullName ?? "";
    private static readonly string ApiClientNamespace = typeof(ApiClient).Namespace ?? "";
    private static readonly string LoggerTypeName = typeof(Logger).Name;
    private static readonly string LoggerFullName = typeof(Logger).FullName ?? "";
    private static readonly string LoggerNamespace = typeof(Logger).Namespace ?? "";

    internal static (string Module, string Function, int Line) GetCaller()
    {
        var stack = new System.Diagnostics.StackTrace(true);

        // Search through stack frames to find first non-logger/non-ApiClient frame
        for (int i = 0; i < stack.FrameCount; i++)
        {
            var frame = stack.GetFrame(i);
            if (frame == null) continue;

            var method = frame.GetMethod();
            if (method == null) continue;

            var methodName = method.Name;
            var file = frame.GetFileName();
            var fileName = !string.IsNullOrWhiteSpace(file) ? Path.GetFileName(file) : null;
            var declaringType = method.DeclaringType;

            // Skip frames from logger or ApiClient files (if we have file info)
            if (fileName != null && SkipFiles.Contains(fileName))
            {
                continue;
            }

            // Skip ApiClient and Logger types by checking type name and namespace
            // This works even without debug symbols
            if (declaringType != null)
            {
                var typeName = declaringType.Name;
                var fullTypeName = declaringType.FullName ?? "";
                var namespaceName = declaringType.Namespace ?? "";

                // Skip ApiClient type (check by name, full name, or namespace)
                if (typeName.Equals(ApiClientTypeName, StringComparison.OrdinalIgnoreCase) ||
                    fullTypeName.Equals(ApiClientFullName, StringComparison.OrdinalIgnoreCase) ||
                    (namespaceName.Contains(ApiClientNamespace, StringComparison.OrdinalIgnoreCase) && 
                     typeName.Equals(ApiClientTypeName, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                // Skip Logger type (check by name, full name, or namespace)
                if (typeName.Equals(LoggerTypeName, StringComparison.OrdinalIgnoreCase) ||
                    fullTypeName.Equals(LoggerFullName, StringComparison.OrdinalIgnoreCase) ||
                    (namespaceName.Contains(LoggerNamespace, StringComparison.OrdinalIgnoreCase) && 
                     typeName.Equals(LoggerTypeName, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                // Skip async infrastructure types
                if (SkipTypes.Any(skipType => typeName.Contains(skipType, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }
            }

            // Skip wrapper functions (but only if not already skipped by type)
            if (!string.IsNullOrEmpty(methodName) && SkipFunctions.Contains(methodName))
            {
                continue;
            }

            bool hasFileInfo = !string.IsNullOrWhiteSpace(file) && fileName != null;
            
            // Handle async state machine methods (MoveNext, etc.)
            string? extractedMethodName = null;
            System.Type? actualDeclaringType = null;
            
            if (methodName == "MoveNext" || methodName.Contains("d__"))
            {
                var stateMachineType = method.DeclaringType;
                if (stateMachineType != null)
                {
                    var typeName = stateMachineType.Name;
                    // Extract actual method name from state machine type like "<GenerateTokenAsync>d__0"
                    if (typeName.StartsWith("<") && typeName.Contains(">d__"))
                    {
                        var endIdx = typeName.IndexOf(">d__");
                        if (endIdx > 1)
                        {
                            extractedMethodName = typeName.Substring(1, endIdx - 1);
                            actualDeclaringType = stateMachineType.DeclaringType;
                            
                            // If the actual declaring type is ApiClient or Logger, skip this frame
                            if (actualDeclaringType != null)
                            {
                                var actualTypeName = actualDeclaringType.Name;
                                var actualFullTypeName = actualDeclaringType.FullName ?? "";
                                var actualNamespaceName = actualDeclaringType.Namespace ?? "";
                                
                                if (actualTypeName.Equals(ApiClientTypeName, StringComparison.OrdinalIgnoreCase) ||
                                    actualFullTypeName.Equals(ApiClientFullName, StringComparison.OrdinalIgnoreCase) ||
                                    (actualNamespaceName.Contains(ApiClientNamespace, StringComparison.OrdinalIgnoreCase) && 
                                     actualTypeName.Equals(ApiClientTypeName, StringComparison.OrdinalIgnoreCase)) ||
                                    actualTypeName.Equals(LoggerTypeName, StringComparison.OrdinalIgnoreCase) ||
                                    actualFullTypeName.Equals(LoggerFullName, StringComparison.OrdinalIgnoreCase) ||
                                    (actualNamespaceName.Contains(LoggerNamespace, StringComparison.OrdinalIgnoreCase) && 
                                     actualTypeName.Equals(LoggerTypeName, StringComparison.OrdinalIgnoreCase)))
                                {
                                    continue;
                                }
                            }
                        }
                    }
                }
                
                if (extractedMethodName == null) continue;
                methodName = extractedMethodName;
            }
            else
            {
                actualDeclaringType = declaringType;
            }

            // Build function name with class
            var functionName = actualDeclaringType != null 
                ? $"{actualDeclaringType.Name}.{methodName}" 
                : methodName;

            // Determine module name: prefer file name, fallback to type name
            string moduleName;
            if (hasFileInfo)
            {
                moduleName = fileName!;
            }
            else if (actualDeclaringType != null)
            {
                // Use type name as module name when file info is not available
                moduleName = actualDeclaringType.Name;
            }
            else
            {
                moduleName = "unknown";
            }

            // Return the first valid frame (with or without file info)
            var lineNumber = hasFileInfo ? frame.GetFileLineNumber() : 0;
            return (moduleName, functionName, lineNumber);
        }

        // If no valid frame found, return defaults
        return ("unknown", "-", 0);
    }
}


