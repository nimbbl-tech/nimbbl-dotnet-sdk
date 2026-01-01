using Nimbbl.Sdk.Rest.Common;

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
    private static bool _loggingEnabled = false;

    private readonly string? _logFilePath;
    private readonly object _sync = new();

    private Logger(string? logFilePath = null, bool alreadyHasDateSuffix = false)
    {
        _logFilePath = alreadyHasDateSuffix ? logFilePath : AddDateSuffixToLogFile(logFilePath);
    }

    private static string? AddDateSuffixToLogFile(string? logFilePath)
    {
        if (string.IsNullOrWhiteSpace(logFilePath)) return logFilePath;
        
        var directory = Path.GetDirectoryName(logFilePath);
        var fileName = Path.GetFileNameWithoutExtension(logFilePath);
        var extension = Path.GetExtension(logFilePath);
        var dateSuffix = DateTime.Now.ToString("ddMMyyyy");
        
        var fileNameWithDate = $"{fileName}_{dateSuffix}{extension}";
        return string.IsNullOrWhiteSpace(directory) 
            ? fileNameWithDate 
            : Path.Combine(directory, fileNameWithDate);
    }

    public static Logger GetInstance(string? logFilePath = null)
    {
        var filePathWithDate = AddDateSuffixToLogFile(logFilePath);
        // If instance exists with different file, reset
        if (_instance != null && filePathWithDate != null && _instance._logFilePath != filePathWithDate)
        {
            _instance = null;
        }
        // Pass already-calculated path with date suffix to avoid recalculating
        _instance ??= new Logger(filePathWithDate, alreadyHasDateSuffix: true);
        return _instance;
    }

    public static void EnableDebug() => _enableDebugLogging = true;
    public static void EnableLogging() => _loggingEnabled = true;
    public static bool IsDebugEnabled() => _enableDebugLogging;

    /// <summary>
    /// Log INFO with optional caller info (gets caller info from logger if not provided)
    /// </summary>
    public void InfoWithCaller(string message, (string Module, string Function, int Line)? callerInfo = null)
    {
        if (!_loggingEnabled) return;
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
    /// Log DEBUG with optional caller info (gets caller info from logger if not provided)
    /// </summary>
    public void DebugWithCaller(string message, (string Module, string Function, int Line)? callerInfo = null)
    {
        if (!_loggingEnabled || !_enableDebugLogging) return;
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
        // Debug logging check (caller methods handle _loggingEnabled check)
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
                    var dir = Path.GetDirectoryName(_logFilePath);
                    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                    File.AppendAllText(_logFilePath!, logLine + Environment.NewLine);
                }
            }
            catch
            {
                // Ignore logging failures
            }
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
        "Start", "MoveNext", "ExecutionContextCallback", "Run", "AwaitUnsafeOnCompleted", "SetResult", "SetException"
    };

    private static readonly HashSet<string> SkipFiles = new(StringComparer.OrdinalIgnoreCase)
    {
        "Logger.cs", "ApiClient.cs"
    };

    private static readonly HashSet<string> SkipTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "AsyncMethodBuilderCore", "AsyncStateMachineBox", "AsyncTaskMethodBuilder", "TaskAwaiter", "ConfiguredTaskAwaiter"
    };

    internal static (string Module, string Function, int Line) GetCaller()
    {
        var stack = new System.Diagnostics.StackTrace(true);
        string? fallbackModuleName = null;
        string? fallbackFunctionName = null;

        // Search through stack frames to find first non-logger/non-ApiClient frame
        for (int i = 0; i < stack.FrameCount; i++)
        {
            var frame = stack.GetFrame(i);
            if (frame == null) continue;

            var method = frame.GetMethod();
            var methodName = method?.Name ?? "";
            var file = frame.GetFileName();
            var fileName = !string.IsNullOrWhiteSpace(file) ? Path.GetFileName(file) : null;
            var declaringType = method?.DeclaringType;

            // Skip frames from logger or ApiClient files
            if (fileName != null && SkipFiles.Contains(fileName))
            {
                continue;
            }

            // Skip async infrastructure types
            if (declaringType != null)
            {
                var typeName = declaringType.Name;
                if (SkipTypes.Any(skipType => typeName.Contains(skipType, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }
            }

            // Skip wrapper functions
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
                var stateMachineType = method?.DeclaringType;
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

            // If we have file info, use it immediately (preferred)
            if (hasFileInfo)
            {
                return (fileName!, functionName, frame.GetFileLineNumber());
            }
            
            // Store as fallback if we don't have file info yet
            if (fallbackFunctionName == null)
            {
                fallbackModuleName = "unknown";
                fallbackFunctionName = functionName;
            }
        }

        // Return fallback if found, otherwise return defaults
        return fallbackFunctionName != null 
            ? (fallbackModuleName!, fallbackFunctionName, 0)
            : ("unknown", "-", 0);
    }
}


