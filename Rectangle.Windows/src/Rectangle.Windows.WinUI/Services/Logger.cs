using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Rectangle.Windows.WinUI.Services
{
    public enum LogLevel
    {
        Debug = 0,
        Info = 1,
        Warning = 2,
        Error = 3
    }

    public static class Logger
    {
        private static readonly object _lock = new();
        private static LogLevel _minLevel = LogLevel.Info;
        private static bool _logToFile = false;
        private static string _logFilePath = "";
        private static int _maxFileSize = 10;

        public static void Initialize(LogLevel minLevel, bool logToFile, string logFilePath, int maxFileSizeMB)
        {
            _minLevel = minLevel;
            _logToFile = logToFile;
            _logFilePath = logFilePath;
            _maxFileSize = maxFileSizeMB;

            if (_logToFile && string.IsNullOrEmpty(_logFilePath))
            {
                var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                _logFilePath = Path.Combine(appData, "Rectangle", "logs", "rectangle.log");
            }

            Info("Logger", "日志系统初始化完成");
        }

        public static void InitializeFromConfig(ConfigService configService)
        {
            var config = configService.Load();
            Initialize(
                (LogLevel)config.LogLevel,
                config.LogToFile,
                config.LogFilePath,
                config.MaxLogFileSize);
        }

        public static void Debug(string category, string message, object? data = null)
            => Log(LogLevel.Debug, category, message, data);

        public static void Info(string category, string message, object? data = null)
            => Log(LogLevel.Info, category, message, data);

        public static void Warning(string category, string message, object? data = null)
            => Log(LogLevel.Warning, category, message, data);

        public static void Error(string category, string message, Exception? exception = null, object? data = null)
            => Log(LogLevel.Error, category, message, data, exception);

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
            System.Diagnostics.Debug.WriteLine(formatted);

            // 同时输出到控制台（如果可用）
            try
            {
                Console.WriteLine(formatted);
            }
            catch { /* 控制台可能不可用 */ }

            if (_logToFile)
                WriteToFile(formatted);
        }

        private static string FormatLogEntry(LogEntry entry)
        {
            var sb = new StringBuilder();
            sb.Append(entry.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            sb.Append($" [{entry.Level.ToString().ToUpper()}] [{entry.Category}] {entry.Message}");

            if (entry.Data != null)
                sb.Append($" | Data: {JsonSerializer.Serialize(entry.Data)}");

            if (!string.IsNullOrEmpty(entry.Exception))
                sb.Append($" | Exception: {entry.Exception}");

            return sb.ToString();
        }

        private static void WriteToFile(string formattedLog)
        {
            lock (_lock)
            {
                try
                {
                    var dir = Path.GetDirectoryName(_logFilePath);
                    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    RotateLogFileIfNeeded();
                    File.AppendAllText(_logFilePath, formattedLog + Environment.NewLine);
                }
                catch { }
            }
        }

        private static void RotateLogFileIfNeeded()
        {
            if (!File.Exists(_logFilePath)) return;

            var fileInfo = new FileInfo(_logFilePath);
            if (fileInfo.Length > _maxFileSize * 1024 * 1024)
            {
                var backupPath = _logFilePath + "." + DateTime.Now.ToString("yyyyMMddHHmmss") + ".bak";
                File.Move(_logFilePath, backupPath);
                CleanupOldBackups();
            }
        }

        private static void CleanupOldBackups()
        {
            try
            {
                var dir = Path.GetDirectoryName(_logFilePath);
                if (string.IsNullOrEmpty(dir)) return;

                var pattern = Path.GetFileName(_logFilePath) + ".*.bak";
                var backups = Directory.GetFiles(dir, pattern);

                if (backups.Length > 5)
                {
                    Array.Sort(backups);
                    for (int i = 0; i < backups.Length - 5; i++)
                        File.Delete(backups[i]);
                }
            }
            catch { }
        }

        public static LogLevel GetCurrentLevel() => _minLevel;
        public static void SetLevel(LogLevel level) => _minLevel = level;
    }

    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public LogLevel Level { get; set; }
        public string Category { get; set; } = "";
        public string Message { get; set; } = "";
        public object? Data { get; set; }
        public string? Exception { get; set; }
    }
}
