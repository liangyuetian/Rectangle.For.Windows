using Xunit;
using Rectangle.Windows.Services;
using System.IO;
using System;

namespace Rectangle.Windows.Tests;

/// <summary>
/// Logger 单元测试
/// </summary>
public class LoggerTests : IDisposable
{
    private readonly string _testLogPath;

    public LoggerTests()
    {
        _testLogPath = Path.Combine(Path.GetTempPath(), $"test_rectangle_{Guid.NewGuid()}.log");
    }

    public void Dispose()
    {
        // 清理测试日志文件
        if (File.Exists(_testLogPath))
        {
            File.Delete(_testLogPath);
        }
    }

    [Fact]
    public void Initialize_ShouldSetConfiguration()
    {
        Logger.Initialize(LogLevel.Debug, true, _testLogPath, 10);

        Assert.Equal(LogLevel.Debug, Logger.GetCurrentLevel());
    }

    [Fact]
    public void SetLevel_ShouldChangeLogLevel()
    {
        Logger.Initialize(LogLevel.Info, false, "", 10);
        Logger.SetLevel(LogLevel.Warning);

        Assert.Equal(LogLevel.Warning, Logger.GetCurrentLevel());
    }

    [Fact]
    public void Debug_ShouldLogWhenLevelIsDebug()
    {
        Logger.Initialize(LogLevel.Debug, true, _testLogPath, 10);
        Logger.Debug("TestCategory", "Test debug message");

        // 验证日志文件被创建
        Assert.True(File.Exists(_testLogPath));

        var content = File.ReadAllText(_testLogPath);
        Assert.Contains("[DEBUG]", content);
        Assert.Contains("TestCategory", content);
        Assert.Contains("Test debug message", content);
    }

    [Fact]
    public void Debug_ShouldNotLogWhenLevelIsInfo()
    {
        Logger.Initialize(LogLevel.Info, true, _testLogPath, 10);
        Logger.Debug("TestCategory", "Test debug message");

        // 不应该创建日志文件或文件为空
        if (File.Exists(_testLogPath))
        {
            var content = File.ReadAllText(_testLogPath);
            Assert.DoesNotContain("[DEBUG]", content);
        }
    }

    [Fact]
    public void Info_ShouldLogWhenLevelIsInfo()
    {
        Logger.Initialize(LogLevel.Info, true, _testLogPath, 10);
        Logger.Info("TestCategory", "Test info message");

        var content = File.ReadAllText(_testLogPath);
        Assert.Contains("[INFO]", content);
        Assert.Contains("Test info message", content);
    }

    [Fact]
    public void Warning_ShouldLogWhenLevelIsWarning()
    {
        Logger.Initialize(LogLevel.Warning, true, _testLogPath, 10);
        Logger.Warning("TestCategory", "Test warning message");

        var content = File.ReadAllText(_testLogPath);
        Assert.Contains("[WARNING]", content);
        Assert.Contains("Test warning message", content);
    }

    [Fact]
    public void Error_ShouldLogWhenLevelIsError()
    {
        Logger.Initialize(LogLevel.Error, true, _testLogPath, 10);
        Logger.Error("TestCategory", "Test error message");

        var content = File.ReadAllText(_testLogPath);
        Assert.Contains("[ERROR]", content);
        Assert.Contains("Test error message", content);
    }

    [Fact]
    public void Log_WithException_ShouldIncludeException()
    {
        Logger.Initialize(LogLevel.Debug, true, _testLogPath, 10);
        var exception = new InvalidOperationException("Test exception");
        Logger.Error("TestCategory", "Error with exception", exception);

        var content = File.ReadAllText(_testLogPath);
        Assert.Contains("InvalidOperationException", content);
        Assert.Contains("Test exception", content);
    }

    [Fact]
    public void Log_WithData_ShouldIncludeData()
    {
        Logger.Initialize(LogLevel.Debug, true, _testLogPath, 10);
        var data = new { X = 100, Y = 200 };
        Logger.Info("TestCategory", "Message with data", data);

        var content = File.ReadAllText(_testLogPath);
        Assert.Contains("X", content);
        Assert.Contains("100", content);
        Assert.Contains("Y", content);
        Assert.Contains("200", content);
    }

    [Fact]
    public void Log_ShouldIncludeTimestamp()
    {
        Logger.Initialize(LogLevel.Debug, true, _testLogPath, 10);
        Logger.Info("TestCategory", "Test message");

        var content = File.ReadAllText(_testLogPath);
        // 验证包含时间戳格式
        Assert.Contains(DateTime.Now.ToString("yyyy-MM-dd"), content);
    }
}
