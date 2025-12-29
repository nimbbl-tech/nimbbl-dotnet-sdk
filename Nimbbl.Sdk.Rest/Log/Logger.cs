using Nimbbl.Sdk.Rest.Common;

namespace Nimbbl.Sdk.Rest.Log;

/// <summary>
/// Logger with always-on format, optional debug gating, file + stdout + callback.
/// Format: [timestamp][sdk sdkVersion][level][module:line][function]: message
/// </summary>
public class Logger
{
    private static Logger? _instance;
    private static bool _enableDebugLogging;
    private static bool _loggingEnabled = true;

    private readonly string? _logFilePath;
    private readonly Action<string, string>? _logAction;
    private readonly object _sync = new();

    private Logger(string? logFilePath = null, Action<string, string>? logAction = null)
    {
        _logFilePath = logFilePath;
        _logAction = logAction;
    }

    public static Logger GetInstance(string? logFilePath = null, Action<string, string>? logAction = null)
    {
        // If instance exists with different file, reset
        if (_instance != null && logFilePath != null && _instance._logFilePath != logFilePath)
        {
            _instance = null;
        }
        _instance ??= new Logger(logFilePath, logAction);
        return _instance;
    }

    public static void EnableDebug() => _enableDebugLogging = true;
    public static void DisableDebug() => _enableDebugLogging = false;
    public static void EnableLogging() => _loggingEnabled = true;
    public static void DisableLogging() => _loggingEnabled = false;
    public static bool IsDebugEnabled() => _enableDebugLogging;
    public static bool IsLoggingEnabled() => _loggingEnabled;

    public void Info(string message)
    {
        var caller = GetCaller();
        var formattedMessage = FormatMessage(message, null);
        Write("INFO", formattedMessage, caller.Module, caller.Line, caller.Function);
    }
    
    /// <summary>
    /// Log INFO with explicit caller info (used for response logs to match request log caller)
    /// </summary>
    public void InfoWithCaller(string message, string module, int line, string function)
    {
        var formattedMessage = FormatMessage(message, null);
        Write("INFO", formattedMessage, module, line, function);
    }
    
    /// <summary>
    /// Get caller info without logging (used to store caller for response logs)
    /// </summary>
    public (string Module, string Function, int Line) GetCallerInfo()
    {
        return GetCaller();
    }
    
    public void Error(string message)
    {
        var caller = GetCaller();
        var formattedMessage = FormatMessage(message, null);
        Write("ERROR", formattedMessage, caller.Module, caller.Line, caller.Function);
    }
    
    public void Debug(string message)
    {
        var caller = GetCaller();
        var formattedMessage = FormatMessage(message, null);
        Write("DEBUG", formattedMessage, caller.Module, caller.Line, caller.Function);
    }
    
    /// <summary>
    /// Log DEBUG with explicit caller info (used for response logs to match request log caller)
    /// </summary>
    public void DebugWithCaller(string message, string module, int line, string function)
    {
        var formattedMessage = FormatMessage(message, null);
        Write("DEBUG", formattedMessage, module, line, function);
    }
    
    public void Warning(string message)
    {
        var caller = GetCaller();
        var formattedMessage = FormatMessage(message, null);
        Write("WARNING", formattedMessage, caller.Module, caller.Line, caller.Function);
    }
    
    public void Critical(string message)
    {
        var caller = GetCaller();
        var formattedMessage = FormatMessage(message, null);
        Write("CRITICAL", formattedMessage, caller.Module, caller.Line, caller.Function);
    }
    
    public void Exception(string message, System.Exception ex)
    {
        var caller = GetCaller();
        var formattedMessage = FormatMessage(message, ex);
        Write("ERROR", formattedMessage, caller.Module, caller.Line, caller.Function);
    }
    
    /// <summary>
    /// Log EXCEPTION with explicit caller info (used for exception logs to match request log caller)
    /// </summary>
    public void ExceptionWithCaller(string message, System.Exception ex, string module, int line, string function)
    {
        var formattedMessage = FormatMessage(message, ex);
        Write("ERROR", formattedMessage, module, line, function);
    }

    private void Write(string level, string message, string module, int line, string function)
    {
        if (!_loggingEnabled) return;
        if (level.Equals("DEBUG", StringComparison.OrdinalIgnoreCase) && !_enableDebugLogging) return;

        // Format: [timestamp][sdk sdkVersion][level][module:line][function]: message
        var moduleName = !string.IsNullOrWhiteSpace(module) && module != "unknown" ? module : "unknown";
        var functionName = !string.IsNullOrWhiteSpace(function) ? function : "-";
        var lineNumber = line;
        var logLine = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}][{SdkConstants.SdkName} {SdkConstants.SdkVersion}][{level}][{moduleName}:{lineNumber}][{functionName}]: {message}";

        _logAction?.Invoke(level, logLine);

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
    
    private string FormatMessage(string message, System.Exception? exception)
    {
        if (exception != null)
        {
            return $"{message} Exception: {exception.Message}\nTrace: {exception.StackTrace}";
        }
        return message;
    }

    private (string Module, string Function, int Line) GetCaller()
    {
        var moduleName = "unknown";
        var functionName = "-";
        var lineNumber = 0;

        var stack = new System.Diagnostics.StackTrace(true);
        // Skip logger internal methods and get actual caller
        // Skip: 'LogRequestAsync', 'LogResponseAsync', 'SendWithAuthGuardAsync', etc.
        // Also skip async infrastructure: AsyncMethodBuilderCore, AsyncStateMachineBox, etc.
        var skipFunctions = new[] { "GetCaller", "Write", "Info", "Error", "Debug", "Warning", "Critical", "Exception", "FormatMessage", 
            "LogRequestAsync", "LogResponseAsync", "SendWithAuthGuardAsync", "SendResilientRequestAsync", "WithRetry", "HandleErrorResponseAsync",
            "Start", "MoveNext", "ExecutionContextCallback", "Run", "AwaitUnsafeOnCompleted", "SetResult", "SetException" };
        var skipFiles = new[] { "Logger.cs", "ApiClient.cs" };
        var skipTypes = new[] { "AsyncMethodBuilderCore", "AsyncStateMachineBox", "AsyncTaskMethodBuilder", "TaskAwaiter", "ConfiguredTaskAwaiter" };

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
            if (fileName != null)
            {
                bool shouldSkip = false;
                foreach (var skipFile in skipFiles)
                {
                    if (fileName.Equals(skipFile, StringComparison.OrdinalIgnoreCase))
                    {
                        shouldSkip = true;
                        break;
                    }
                }
                if (shouldSkip) continue;
            }

            // Skip async infrastructure types (AsyncMethodBuilderCore, AsyncStateMachineBox, etc.)
            if (declaringType != null)
            {
                bool shouldSkip = false;
                foreach (var skipType in skipTypes)
                {
                    if (declaringType.Name.Contains(skipType, StringComparison.OrdinalIgnoreCase))
                    {
                        shouldSkip = true;
                        break;
                    }
                }
                if (shouldSkip) continue;
            }

            // Skip wrapper functions
            if (!string.IsNullOrEmpty(methodName) && Array.IndexOf(skipFunctions, methodName) >= 0)
            {
                continue;
            }

            // For response logs, we might need to be more lenient about frames without file info
            // Prefer frames with file information
            // Only skip frames without file info if we haven't found a valid frame yet
            // This allows us to find the caller even when called from deep async stacks
            bool hasFileInfo = !string.IsNullOrWhiteSpace(file) && fileName != null;
            
            // If this frame doesn't have file info, but we can extract method info, continue searching
            // but don't skip it yet - we might use it as a fallback
            if (!hasFileInfo)
            {
                // If we can't extract method info either, definitely skip
                if (methodName == "MoveNext" || methodName.Contains("d__") || string.IsNullOrEmpty(methodName))
                {
                    continue;
                }
                // Otherwise, continue to see if we can find a better frame with file info
                // But we'll use this as fallback if nothing better is found
            }

            // Handle async state machine methods (MoveNext, etc.)
            // Async methods compile to nested state machine types like "ClassName.<MethodName>d__N"
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
                        var startIdx = 1;
                        var endIdx = typeName.IndexOf(">d__");
                        if (endIdx > startIdx)
                        {
                            extractedMethodName = typeName.Substring(startIdx, endIdx - startIdx);
                            // Get the actual declaring class (state machine is nested)
                            actualDeclaringType = stateMachineType.DeclaringType;
                        }
                    }
                }
                
                // If we couldn't extract from this frame, continue to next
                if (extractedMethodName == null)
                {
                    continue;
                }
                
                methodName = extractedMethodName;
            }
            else
            {
                // Regular (non-async) method
                actualDeclaringType = declaringType;
            }

            // Build function name with class
            if (actualDeclaringType != null)
            {
                functionName = $"{actualDeclaringType.Name}.{methodName}";
            }
            else
            {
                functionName = methodName;
            }

            // If we have file info, use it immediately (preferred)
            if (hasFileInfo)
            {
                moduleName = fileName!;
                lineNumber = frame.GetFileLineNumber();
                return (moduleName, functionName, lineNumber);
            }
            
            // If no file info but we have method info, store as fallback and continue searching
            // We'll use this if we don't find anything better
            if (moduleName == "unknown" && functionName != "-")
            {
                moduleName = "unknown";
                lineNumber = 0;
                // Don't return yet - continue to see if we can find a frame with file info
            }
        }

        // If we found a fallback (method info but no file info), use it
        if (moduleName == "unknown" && functionName != "-")
        {
            return (moduleName, functionName, lineNumber);
        }

        return (moduleName, functionName, lineNumber);
    }
}

