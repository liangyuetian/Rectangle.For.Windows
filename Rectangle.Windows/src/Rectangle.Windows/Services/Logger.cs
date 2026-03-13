using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace Rectangle.Windows.Services;

/// <summary>
/// 日志级别
/// </summary>
public enum LogLevel
{
    Debug = 0,
    Info = 1,
    Warning = 2,
    Error = 3
}

/// <summary>
/// 结构化日志记录器
/// </summary>
public static class Logger
{
    private static readonly object _lock = new();
    private static LogLevel _minLevel = LogLevel.Info;
    private static bool _logToFile = false;
    private static string _logFilePath = "";
    private static int _maxFileSize = 10; // MB

    /// <summary>
    /// 初始化日志系统
    /// </summary>
    public static void Initialize(LogLevel minLevel, bool logToFile, string logFilePath, int maxFileSizeMB)
    {
        _minLevel = minLevel;
        _logToFile = logToFile;
        _logFilePath = logFilePath;
        _maxFileSize = maxFileSizeMB;

        // 如果没有指定日志路径，使用默认路径
        if (_logToFile && string.IsNullOrEmpty(_logFilePath))
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _logFilePath = Path.Combine(appData, "Rectangle", "logs", "rectangle.log");
        }

        Info("Logger", "日志系统初始化完成");
    }

    /// <summary>
    /// 记录 Debug 级别日志
    /// </summary>
    public static void Debug(string category, string message, object? data = null)
    {
        Log(LogLevel.Debug, category, message, data);
    }

    /// <summary>
    /// 记录 Info 级别日志
    /// </summary>
    public static void Info(string category, string message, object? data = null)
    {
        Log(LogLevel.Info, category, message, data);
    }

    /// <summary>
    /// 记录 Warning 级别日志
    /// </summary>
    public static void Warning(string category, string message, object? data = null)
    {
        Log(LogLevel.Warning, category, message, data);
    }

    /// <summary>
    /// 记录 Error 级别日志
    /// </summary>
    public static void Error(string category, string message, Exception? exception = null, object? data = null)
    {
        Log(LogLevel.Error, category, message, data, exception);
    }

    /// <summary>
    /// 核心日志记录方法
    /// </summary>
    private static void Log(LogLevel level, string category, string message, object? data = null, Exception? exception = null)
    {
        if (level < _minLevel) return;

        var entry = new LogEntry
        {
            Timestamp = DateTime.Now,
            Level = level,
            Category = category,
            Message = message,
            Data = data,
            Exception = exception?.ToString()
        };

        var formatted = FormatLogEntry(entry);

        // 输出到控制台
        Console.WriteLine(formatted);

        // 输出到文件
        if (_logToFile)
        {
            WriteToFile(formatted);
        }
    }

    /// <summary>
    /// 格式化日志条目
    /// </summary>
    private static string FormatLogEntry(LogEntry entry)
    {
        var sb = new StringBuilder();
        sb.Append(entry.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff"));
        sb.Append(" [");
        sb.Append(entry.Level.ToString().ToUpper());
        sb.Append("] [");
        sb.Append(entry.Category);
        sb.Append("] ");
        sb.Append(entry.Message);

        if (entry.Data != null)
        {
            sb.Append(" | Data: ");
            sb.Append(JsonSerializer.Serialize(entry.Data));
        }

        if (!string.IsNullOrEmpty(entry.Exception))
        {
            sb.Append(" | Exception: ");
            sb.Append(entry.Exception);
        }

        return sb.ToString();
    }

    /// <summary>
    /// 写入日志到文件
    /// </summary>
    private static void WriteToFile(string formattedLog)
    {
        lock (_lock)
        {
            try
            {
                // 确保目录存在
                var dir = Path.GetDirectoryName(_logFilePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                // 检查文件大小，如果超过限制则轮转
                RotateLogFileIfNeeded();

                // 追加写入
                File.AppendAllText(_logFilePath, formattedLog + Environment.NewLine);
            }
            catch
            {
                // 忽略文件写入错误
            }
        }
    }

    /// <summary>
    /// 轮转日志文件（如果超过大小限制）
    /// </summary>
    private static void RotateLogFileIfNeeded()
    {
        if (!File.Exists(_logFilePath)) return;

        var fileInfo = new FileInfo(_logFilePath);
        if (fileInfo.Length > _maxFileSize * 1024 * 1024)
        {
            // 备份旧日志
            var backupPath = _logFilePath + "." + DateTime.Now.ToString("yyyyMMddHHmmss") + ".bak";
            File.Move(_logFilePath, backupPath);

            // 清理旧备份（只保留最近 5 个）
            CleanupOldBackups();
        }
    }

    /// <summary>
    /// 清理旧日志备份
    /// </summary>
    private static void CleanupOldBackups()
    {
        try
        {
            var dir = Path.GetDirectoryName(_logFilePath);
            if (string.IsNullOrEmpty(dir)) return;

            var pattern = Path.GetFileName(_logFilePath) + ".*.bak";
            var backups = Directory.GetFiles(dir, pattern);

            // 按时间排序，删除旧的
            if (backups.Length > 5)
            {
                Array.Sort(backups);
                for (int i = 0; i < backups.Length - 5; i++)
                {
                    File.Delete(backups[i]);
                }
            }
        }
        catch
        {
            // 忽略清理错误
        }
    }

    /// <summary>
    /// 获取当前日志级别
    /// </summary>
    public static LogLevel GetCurrentLevel() => _minLevel;

    /// <summary>
    /// 设置日志级别
    /// </summary>
    public static void SetLevel(LogLevel level) => _minLevel = level;
}

/// <summary>
/// 日志条目
/// </summary>
public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public LogLevel Level { get; set; }
    public string Category { get; set; } = "";
    public string Message { get; set; } = "";
    public object? Data { get; set; }
    public string? Exception { get; set; }
}
