using Rectangle.Windows.WinUI.Core;
using System;
using System.Collections.Generic;

namespace Rectangle.Windows.WinUI.Services;

/// <summary>
/// 按应用自动执行动作规则（窗口激活后自动吸附）。
/// </summary>
public sealed class AutoRuleService : IDisposable
{
    private readonly ConfigService _configService;
    private readonly WindowManager _windowManager;
    private readonly LastActiveWindowService _lastActiveService;
    private readonly Dictionary<nint, DateTime> _lastAppliedAt = new();
    private bool _disposed;

    public AutoRuleService(ConfigService configService, WindowManager windowManager, LastActiveWindowService lastActiveService)
    {
        _configService = configService;
        _windowManager = windowManager;
        _lastActiveService = lastActiveService;
        _lastActiveService.ActiveWindowChanged += OnActiveWindowChanged;
    }

    private void OnActiveWindowChanged(nint hwnd, string processName)
    {
        if (hwnd == 0 || string.IsNullOrWhiteSpace(processName)) return;

        try
        {
            var config = _configService.Load();
            if (config.AppRules.Count == 0) return;

            // 同一窗口短时间内只应用一次，避免焦点抖动反复触发
            var now = DateTime.Now;
            if (_lastAppliedAt.TryGetValue(hwnd, out var last) && (now - last).TotalSeconds < 2)
                return;

            foreach (var rule in config.AppRules)
            {
                if (!rule.Enabled || string.IsNullOrWhiteSpace(rule.ProcessName) || string.IsNullOrWhiteSpace(rule.ActionTag))
                    continue;

                var matched = rule.MatchExact
                    ? processName.Equals(rule.ProcessName, StringComparison.OrdinalIgnoreCase)
                    : processName.Contains(rule.ProcessName, StringComparison.OrdinalIgnoreCase);

                if (!matched) continue;

                if (!Enum.TryParse<WindowAction>(rule.ActionTag, ignoreCase: true, out var action))
                    continue;

                _windowManager.Execute(action, hwnd, forceDirectAction: true);
                _lastAppliedAt[hwnd] = now;
                Logger.Info("AutoRuleService", $"规则命中: {processName} -> {action}");
                break;
            }
        }
        catch (Exception ex)
        {
            Logger.Warning("AutoRuleService", $"应用规则失败: {ex.Message}");
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _lastActiveService.ActiveWindowChanged -= OnActiveWindowChanged;
        _lastAppliedAt.Clear();
        _disposed = true;
    }
}

