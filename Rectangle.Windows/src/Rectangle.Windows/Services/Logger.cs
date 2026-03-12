using System;
using System.IO;
using System.Text;

namespace Rectangle.Windows.Services;

/// <summary>
/// 日志级别
/// </summary>
public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error
}

/// <summary>
/// 结构化日志服务
/// 支持多级别日志、格式化输出、文件写入
/// </summary>
public class Logger : IDisposable
{
    private static Logger? _instance;
    private static readonly object _lock = new();

    private readonly string _logPath;
    private readonly LogLevel _minLevel;
    private readonly int _maxFileSizeMB;
    private readonly int _maxLogFiles;
    private readonly object _fileLock = new();
    private bool _disposed;

    /// <summary>
    /// 获取日志服务单例
    /// </summary>
    public static Logger Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new Logger();
                }
            }
            return _instance;
        }
    }

    /// <summary>
    /// 是否启用控制台输出
    /// </summary>
    public bool EnableConsoleOutput { get; set; } = true;

    /// <summary>
    /// 是否启用文件输出
    /// </summary>
    public bool EnableFileOutput { get; set; } = true;

    private Logger(LogLevel minLevel = LogLevel.Debug, int maxFileSizeMB = 10, int maxLogFiles = 5)
    {
        _minLevel = minLevel;
        _maxFileSizeMB = maxFileSizeMB;
        _maxLogFiles = maxLogFiles;

        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var logDir = Path.Combine(appData, "Rectangle", "Logs");
        Directory.CreateDirectory(logDir);
        _logPath = Path.Combine(logDir, $"rectangle_{DateTime.Now:yyyyMMdd}.log");
    }

    /// <summary>
    /// 记录调试日志
    /// </summary>
    public void Debug(string message, string? category = null)
    {
        Log(LogLevel.Debug, message, category);
    }

    /// <summary>
    /// 记录信息日志
    /// </summary>
    public void Info(string message, string? category = null)
    {
        Log(LogLevel.Info, message, category);
    }

    /// <summary>
    /// 记录警告日志
    /// </summary>
    public void Warning(string message, string? category = null)
    {
        Log(LogLevel.Warning, message, category);
    }

    /// <summary>
    /// 记录错误日志
    /// </summary>
    public void Error(string message, string? category = null, Exception? exception = null)
    {
        var fullMessage = exception != null
            ? $"{message}\nException: {exception.Message}\nStackTrace: {exception.StackTrace}"
            : message;
        Log(LogLevel.Error, fullMessage, category);
    }

    /// <summary>
    /// 记录日志
    /// </summary>
    private void Log(LogLevel level, string message, string? category)
    {
        if (level < _minLevel) return;

        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var levelStr = level.ToString().ToUpper().PadRight(7);
        var categoryStr = !string.IsNullOrEmpty(category) ? $"[{category}] " : "";
        var logLine = $"{timestamp} | {levelStr} | {categoryStr}{message}";

        if (EnableConsoleOutput)
        {
            WriteToConsole(level, logLine);
        }

        if (EnableFileOutput)
        {
            WriteToFile(logLine);
        }
    }

    /// <summary>
    /// 写入控制台（带颜色）
    /// </summary>
    private void WriteToConsole(LogLevel level, string message)
    {
        var originalColor = Console.ForegroundColor;
        try
        {
            Console.ForegroundColor = level switch
            {
                LogLevel.Debug => ConsoleColor.Gray,
                LogLevel.Info => ConsoleColor.White,
                LogLevel.Warning => ConsoleColor.Yellow,
                LogLevel.Error => ConsoleColor.Red,
                _ => ConsoleColor.White
            };
            Console.WriteLine(message);
        }
        finally
        {
            Console.ForegroundColor = originalColor;
        }
    }

    /// <summary>
    /// 写入文件
    /// </summary>
    private void WriteToFile(string message)
    {
        lock (_fileLock)
        {
            try
            {
                CheckLogFileSize();
                File.AppendAllText(_logPath, message + "\n", Encoding.UTF8);
            }
            catch { }
        }
    }

    /// <summary>
    /// 检查日志文件大小，超过限制则归档
    /// </summary>
    private void CheckLogFileSize()
    {
        try
        {
            if (!File.Exists(_logPath)) return;

            var fileInfo = new FileInfo(_logPath);
            if (fileInfo.Length >= _maxFileSizeMB * 1024 * 1024)
            {
                ArchiveLogFile();
                CleanOldLogs();
            }
        }
        catch { }
    }

    /// <summary>
    /// 归档日志文件
    /// </summary>
    private void ArchiveLogFile()
    {
        try
        {
            var archivePath = _logPath.Replace(".log", $"_{DateTime.Now:HHmmss}.log");
            File.Move(_logPath, archivePath);
        }
        catch { }
    }

    /// <summary>
    /// 清理旧日志文件
    /// </summary>
    private void CleanOldLogs()
    {
        try
        {
            var logDir = Path.GetDirectoryName(_logPath);
            if (logDir == null) return;

            var logFiles = Directory.GetFiles(logDir, "rectangle_*.log");
            if (logFiles.Length <= _maxLogFiles) return;

            Array.Sort(logFiles);
            var filesToDelete = logFiles.Length - _maxLogFiles;
            for (int i = 0; i < filesToDelete; i++)
            {
                File.Delete(logFiles[i]);
            }
        }
        catch { }
    }

    /// <summary>
    /// 获取日志文件路径
    /// </summary>
    public string GetLogFilePath() => _logPath;

    /// <summary>
    /// 获取所有日志内容
    /// </summary>
    public string GetAllLogs()
    {
        try
        {
            return File.Exists(_logPath) ? File.ReadAllText(_logPath) : "";
        }
        catch
        {
            return "";
        }
    }

    /// <summary>
    /// 清空日志文件
    /// </summary>
    public void ClearLogs()
    {
        try
        {
            if (File.Exists(_logPath))
            {
                File.Delete(_logPath);
            }
        }
        catch { }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
    }
}

/// <summary>
/// 日志扩展方法
/// </summary>
public static class LoggerExtensions
{
    /// <summary>
    /// 记录窗口操作日志
    /// </summary>
    public static void LogWindowAction(this Logger logger, string action, nint hwnd, string details = "")
    {
        logger.Info($"Window Action: {action}, HWND: {hwnd}, {details}", "WindowManager");
    }

    /// <summary>
    /// 记录快捷键日志
    /// </summary>
    public static void LogShortcut(this Logger logger, string shortcut, string action)
    {
        logger.Info($"Shortcut: {shortcut} -> {action}", "Hotkey");
    }

    /// <summary>
    /// 记录拖拽吸附日志
    /// </summary>
    public static void LogSnap(this Logger logger, string action, int x, int y)
    {
        logger.Info($"Snap: {action} at ({x}, {y})", "Snapping");
    }

    /// <summary>
    /// 记录配置变更日志
    /// </summary>
    public static void LogConfig(this Logger logger, string key, object? oldValue, object? newValue)
    {
        logger.Info($"Config Changed: {key} = {newValue} (was {oldValue})", "Config");
    }
}