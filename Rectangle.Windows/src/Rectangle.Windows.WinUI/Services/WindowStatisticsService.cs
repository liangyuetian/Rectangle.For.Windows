using Rectangle.Windows.WinUI.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Rectangle.Windows.WinUI.Services
{
    /// <summary>
    /// 窗口使用统计项
    /// </summary>
    public class WindowUsageStats
    {
        /// <summary>
        /// 窗口类名
        /// </summary>
        public string WindowClass { get; set; } = "";

        /// <summary>
        /// 应用程序名称
        /// </summary>
        public string ApplicationName { get; set; } = "";

        /// <summary>
        /// 应用程序可执行文件名
        /// </summary>
        public string ExecutableName { get; set; } = "";

        /// <summary>
        /// 总使用时长（秒）
        /// </summary>
        public double TotalUsageSeconds { get; set; }

        /// <summary>
        /// 活跃使用时长（窗口在前台的时长，秒）
        /// </summary>
        public double ActiveUsageSeconds { get; set; }

        /// <summary>
        /// 窗口被激活次数
        /// </summary>
        public int ActivationCount { get; set; }

        /// <summary>
        /// 窗口被布局调整次数
        /// </summary>
        public int LayoutAdjustmentCount { get; set; }

        /// <summary>
        /// 最常用的布局操作
        /// </summary>
        public Dictionary<WindowAction, int> LayoutUsage { get; set; } = new();

        /// <summary>
        /// 首次使用时间
        /// </summary>
        public DateTime FirstUsed { get; set; }

        /// <summary>
        /// 最后使用时间
        /// </summary>
        public DateTime LastUsed { get; set; }

        /// <summary>
        /// 平均窗口尺寸
        /// </summary>
        public Size AverageWindowSize { get; set; }

        /// <summary>
        /// 最常使用的显示器
        /// </summary>
        public string MostUsedDisplay { get; set; } = "";

        /// <summary>
        /// 使用时段分布（24小时制，每小时的使用次数）
        /// </summary>
        public int[] HourlyUsage { get; set; } = new int[24];

        /// <summary>
        /// 使用星期分布（0=周日，6=周六）
        /// </summary>
        public int[] DailyUsage { get; set; } = new int[7];
    }

    /// <summary>
    /// 布局使用统计
    /// </summary>
    public class LayoutUsageStats
    {
        /// <summary>
        /// 布局操作
        /// </summary>
        public WindowAction Action { get; set; }

        /// <summary>
        /// 使用次数
        /// </summary>
        public int UsageCount { get; set; }

        /// <summary>
        /// 占总操作的百分比
        /// </summary>
        public double UsagePercentage { get; set; }

        /// <summary>
        /// 平均执行时间（毫秒）
        /// </summary>
        public double AverageExecutionTimeMs { get; set; }

        /// <summary>
        /// 成功次数
        /// </summary>
        public int SuccessCount { get; set; }

        /// <summary>
        /// 失败次数
        /// </summary>
        public int FailureCount { get; set; }

        /// <summary>
        /// 最常用于的应用程序
        /// </summary>
        public string MostUsedWithApp { get; set; } = "";
    }

    /// <summary>
    /// 时间段统计
    /// </summary>
    public class TimeRangeStats
    {
        /// <summary>
        /// 时间段开始
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 时间段结束
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// 总操作次数
        /// </summary>
        public int TotalOperations { get; set; }

        /// <summary>
        /// 最常用的操作
        /// </summary>
        public WindowAction MostUsedAction { get; set; }

        /// <summary>
        /// 操作最多的应用程序
        /// </summary>
        public string MostActiveApp { get; set; } = "";
    }

    /// <summary>
    /// 热力图数据点
    /// </summary>
    public class HeatmapDataPoint
    {
        /// <summary>
        /// X坐标（归一化0-1）
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Y坐标（归一化0-1）
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// 强度值
        /// </summary>
        public double Intensity { get; set; }

        /// <summary>
        /// 操作类型
        /// </summary>
        public WindowAction Action { get; set; }

        /// <summary>
        /// 应用程序
        /// </summary>
        public string Application { get; set; } = "";

        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// 综合统计报告
    /// </summary>
    public class StatisticsReport
    {
        /// <summary>
        /// 报告生成时间
        /// </summary>
        public DateTime GeneratedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 统计时间段
        /// </summary>
        public TimeRangeStats TimeRange { get; set; } = new();

        /// <summary>
        /// 应用程序使用排行
        /// </summary>
        public List<WindowUsageStats> TopApplications { get; set; } = new();

        /// <summary>
        /// 布局使用排行
        /// </summary>
        public List<LayoutUsageStats> TopLayouts { get; set; } = new();

        /// <summary>
        /// 每日使用趋势
        /// </summary>
        public List<DailyStats> DailyTrends { get; set; } = new();

        /// <summary>
        /// 小时使用分布
        /// </summary>
        public int[] HourlyDistribution { get; set; } = new int[24];

        /// <summary>
        /// 热力图数据
        /// </summary>
        public List<HeatmapDataPoint> HeatmapData { get; set; } = new();

        /// <summary>
        /// 效率指标
        /// </summary>
        public EfficiencyMetrics Efficiency { get; set; } = new();
    }

    /// <summary>
    /// 每日统计
    /// </summary>
    public class DailyStats
    {
        public DateTime Date { get; set; }
        public int OperationCount { get; set; }
        public int UniqueAppsUsed { get; set; }
        public double ActiveUsageHours { get; set; }
    }

    /// <summary>
    /// 效率指标
    /// </summary>
    public class EfficiencyMetrics
    {
        /// <summary>
        /// 平均每次布局节省时间（估算，秒）
        /// </summary>
        public double AvgTimeSavedPerOperation { get; set; }

        /// <summary>
        /// 总节省时间（小时）
        /// </summary>
        public double TotalTimeSavedHours { get; set; }

        /// <summary>
        /// 最常用的快捷键组合
        /// </summary>
        public string MostUsedShortcut { get; set; } = "";

        /// <summary>
        /// 生产力评分（0-100）
        /// </summary>
        public int ProductivityScore { get; set; }
    }

    /// <summary>
    /// 窗口统计服务 - 记录和分析窗口使用数据
    /// </summary>
    public class WindowStatisticsService
    {
        private readonly string _dataDirectory;
        private readonly Dictionary<string, WindowUsageStats> _windowStats = new();
        private readonly Dictionary<WindowAction, LayoutUsageStats> _layoutStats = new();
        private readonly List<HeatmapDataPoint> _heatmapData = new();
        private readonly object _lock = new();

        private DateTime _sessionStart = DateTime.Now;
        private nint _currentActiveWindow = 0;
        private DateTime _currentWindowActivatedAt = DateTime.Now;

        /// <summary>
        /// 统计数据更新事件
        /// </summary>
        public event EventHandler? StatisticsUpdated;

        /// <summary>
        /// 是否启用统计
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 最大保留天数
        /// </summary>
        public int MaxRetentionDays { get; set; } = 90;

        /// <summary>
        /// 最大热力图数据点
        /// </summary>
        public int MaxHeatmapPoints { get; set; } = 10000;

        public WindowStatisticsService()
        {
            _dataDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Rectangle",
                "Statistics");

            if (!Directory.Exists(_dataDirectory))
            {
                Directory.CreateDirectory(_dataDirectory);
            }

            // 加载历史数据
            _ = LoadStatisticsAsync();
        }

        /// <summary>
        /// 记录窗口激活
        /// </summary>
        public void RecordWindowActivation(nint hwnd, string windowClass, string applicationName, string executableName)
        {
            if (!IsEnabled) return;

            lock (_lock)
            {
                // 记录之前窗口的使用时长
                if (_currentActiveWindow != 0 && _currentActiveWindow != hwnd)
                {
                    var elapsed = DateTime.Now - _currentWindowActivatedAt;
                    UpdateWindowUsageTime(_currentActiveWindow, elapsed.TotalSeconds);
                }

                _currentActiveWindow = hwnd;
                _currentWindowActivatedAt = DateTime.Now;

                var key = $"{executableName}:{windowClass}";
                if (!_windowStats.TryGetValue(key, out var stats))
                {
                    stats = new WindowUsageStats
                    {
                        WindowClass = windowClass,
                        ApplicationName = applicationName,
                        ExecutableName = executableName,
                        FirstUsed = DateTime.Now
                    };
                    _windowStats[key] = stats;
                }

                stats.ActivationCount++;
                stats.LastUsed = DateTime.Now;

                // 更新时段统计
                var hour = DateTime.Now.Hour;
                var day = (int)DateTime.Now.DayOfWeek;
                stats.HourlyUsage[hour]++;
                stats.DailyUsage[day]++;

                Logger.Debug("Statistics", $"[Statistics] 窗口激活: {applicationName}");
                StatisticsUpdated?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 记录布局操作
        /// </summary>
        public void RecordLayoutOperation(
            WindowAction action,
            nint hwnd,
            string windowClass,
            string applicationName,
            WindowRect rect,
            bool success = true,
            double executionTimeMs = 0)
        {
            if (!IsEnabled) return;

            lock (_lock)
            {
                // 更新布局统计
                if (!_layoutStats.TryGetValue(action, out var layoutStat))
                {
                    layoutStat = new LayoutUsageStats
                    {
                        Action = action
                    };
                    _layoutStats[action] = layoutStat;
                }

                layoutStat.UsageCount++;
                if (success)
                {
                    layoutStat.SuccessCount++;
                }
                else
                {
                    layoutStat.FailureCount++;
                }

                // 更新平均执行时间
                if (executionTimeMs > 0)
                {
                    layoutStat.AverageExecutionTimeMs =
                        (layoutStat.AverageExecutionTimeMs * (layoutStat.UsageCount - 1) + executionTimeMs)
                        / layoutStat.UsageCount;
                }

                // 更新窗口统计
                var key = $"{applicationName}:{windowClass}";
                if (_windowStats.TryGetValue(key, out var windowStat))
                {
                    windowStat.LayoutAdjustmentCount++;

                    if (!windowStat.LayoutUsage.ContainsKey(action))
                    {
                        windowStat.LayoutUsage[action] = 0;
                    }
                    windowStat.LayoutUsage[action]++;
                }

                // 记录热力图数据
                if (_heatmapData.Count < MaxHeatmapPoints)
                {
                    int screenW = GetSystemMetrics(0); // SM_CXSCREEN
                    int screenH = GetSystemMetrics(1); // SM_CYSCREEN
                    _heatmapData.Add(new HeatmapDataPoint
                    {
                        X = screenW > 0 ? (double)rect.X / screenW : 0,
                        Y = screenH > 0 ? (double)rect.Y / screenH : 0,
                        Intensity = 1.0,
                        Action = action,
                        Application = applicationName,
                        Timestamp = DateTime.Now
                    });
                }

                StatisticsUpdated?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 更新窗口使用时长
        /// </summary>
        private void UpdateWindowUsageTime(nint hwnd, double seconds)
        {
            // 这里简化处理，实际应该根据窗口句柄查找对应的统计项
            // 由于窗口可能已经关闭，我们使用最后已知的窗口信息
        }

        /// <summary>
        /// 获取应用程序使用排行
        /// </summary>
        public List<WindowUsageStats> GetTopApplications(int count = 10)
        {
            lock (_lock)
            {
                return _windowStats.Values
                    .OrderByDescending(s => s.ActiveUsageSeconds)
                    .Take(count)
                    .ToList();
            }
        }

        /// <summary>
        /// 获取布局使用统计
        /// </summary>
        public List<LayoutUsageStats> GetLayoutStatistics()
        {
            lock (_lock)
            {
                var totalUsage = _layoutStats.Values.Sum(s => s.UsageCount);
                if (totalUsage > 0)
                {
                    foreach (var stat in _layoutStats.Values)
                    {
                        stat.UsagePercentage = (double)stat.UsageCount / totalUsage * 100;
                    }
                }

                return _layoutStats.Values
                    .OrderByDescending(s => s.UsageCount)
                    .ToList();
            }
        }

        /// <summary>
        /// 获取热力图数据
        /// </summary>
        public List<HeatmapDataPoint> GetHeatmapData(DateTime? startTime = null, DateTime? endTime = null)
        {
            lock (_lock)
            {
                var query = _heatmapData.AsEnumerable();

                if (startTime.HasValue)
                {
                    query = query.Where(h => h.Timestamp >= startTime.Value);
                }
                if (endTime.HasValue)
                {
                    query = query.Where(h => h.Timestamp <= endTime.Value);
                }

                return query.ToList();
            }
        }

        /// <summary>
        /// 生成统计报告
        /// </summary>
        public StatisticsReport GenerateReport(DateTime? startDate = null, DateTime? endDate = null)
        {
            var start = startDate ?? DateTime.Now.AddDays(-7);
            var end = endDate ?? DateTime.Now;

            lock (_lock)
            {
                var report = new StatisticsReport
                {
                    TimeRange = new TimeRangeStats
                    {
                        StartTime = start,
                        EndTime = end,
                        TotalOperations = _layoutStats.Values.Sum(s => s.UsageCount),
                        MostUsedAction = _layoutStats.OrderByDescending(kv => kv.Value.UsageCount).FirstOrDefault().Key,
                        MostActiveApp = _windowStats.OrderByDescending(kv => kv.Value.LayoutAdjustmentCount).FirstOrDefault().Value?.ApplicationName ?? ""
                    },
                    TopApplications = GetTopApplications(10),
                    TopLayouts = GetLayoutStatistics().Take(10).ToList(),
                    HourlyDistribution = CalculateHourlyDistribution(),
                    HeatmapData = GetHeatmapData(start, end),
                    Efficiency = CalculateEfficiencyMetrics()
                };

                // 计算每日趋势
                report.DailyTrends = CalculateDailyTrends(start, end);

                return report;
            }
        }

        /// <summary>
        /// 计算小时分布
        /// </summary>
        private int[] CalculateHourlyDistribution()
        {
            var distribution = new int[24];
            foreach (var stats in _windowStats.Values)
            {
                for (int i = 0; i < 24; i++)
                {
                    distribution[i] += stats.HourlyUsage[i];
                }
            }
            return distribution;
        }

        /// <summary>
        /// 计算每日趋势
        /// </summary>
        private List<DailyStats> CalculateDailyTrends(DateTime start, DateTime end)
        {
            var trends = new List<DailyStats>();
            var current = start.Date;

            while (current <= end.Date)
            {
                // 这里简化处理，实际应该按日期聚合数据
                trends.Add(new DailyStats
                {
                    Date = current,
                    OperationCount = 0,
                    UniqueAppsUsed = 0,
                    ActiveUsageHours = 0
                });
                current = current.AddDays(1);
            }

            return trends;
        }

        /// <summary>
        /// 计算效率指标
        /// </summary>
        private EfficiencyMetrics CalculateEfficiencyMetrics()
        {
            var totalOperations = _layoutStats.Values.Sum(s => s.UsageCount);
            var estimatedTimeSaved = totalOperations * 3.0; // 假设每次操作节省3秒

            return new EfficiencyMetrics
            {
                AvgTimeSavedPerOperation = 3.0,
                TotalTimeSavedHours = estimatedTimeSaved / 3600.0,
                MostUsedShortcut = _layoutStats.OrderByDescending(kv => kv.Value.UsageCount).FirstOrDefault().Value?.Action.ToString() ?? "",
                ProductivityScore = Math.Min(100, (int)(totalOperations / 10.0)) // 简化评分
            };
        }

        /// <summary>
        /// 保存统计数据到文件
        /// </summary>
        public async Task SaveStatisticsAsync()
        {
            try
            {
                var filePath = Path.Combine(_dataDirectory, $"stats_{DateTime.Now:yyyyMM}.json");

                var data = new StatisticsData
                {
                    WindowStats = _windowStats,
                    LayoutStats = _layoutStats,
                    HeatmapData = _heatmapData,
                    SavedAt = DateTime.Now
                };

                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                await File.WriteAllTextAsync(filePath, json);
                Logger.Info("Statistics", $"[Statistics] 统计数据已保存: {filePath}");
            }
            catch (Exception ex)
            {
                Logger.Error("Statistics", $"[Statistics] 保存统计数据失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 加载统计数据
        /// </summary>
        public async Task LoadStatisticsAsync()
        {
            try
            {
                var filePath = Path.Combine(_dataDirectory, $"stats_{DateTime.Now:yyyyMM}.json");
                if (!File.Exists(filePath))
                {
                    return;
                }

                var json = await File.ReadAllTextAsync(filePath);
                var data = JsonSerializer.Deserialize<StatisticsData>(json);

                if (data != null)
                {
                    lock (_lock)
                    {
                        // 合并数据而不是完全替换
                        foreach (var kvp in data.WindowStats)
                        {
                            if (!_windowStats.ContainsKey(kvp.Key))
                            {
                                _windowStats[kvp.Key] = kvp.Value;
                            }
                        }

                        foreach (var kvp in data.LayoutStats)
                        {
                            if (!_layoutStats.ContainsKey(kvp.Key))
                            {
                                _layoutStats[kvp.Key] = kvp.Value;
                            }
                        }

                        _heatmapData.AddRange(data.HeatmapData);
                    }

                    Logger.Info("Statistics", $"[Statistics] 已加载历史统计数据");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Statistics", $"[Statistics] 加载统计数据失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 清除统计数据
        /// </summary>
        public void ClearStatistics()
        {
            lock (_lock)
            {
                _windowStats.Clear();
                _layoutStats.Clear();
                _heatmapData.Clear();
                _sessionStart = DateTime.Now;
            }

            Logger.Info("Statistics", "[Statistics] 统计数据已清除");
            StatisticsUpdated?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 清理过期数据
        /// </summary>
        public void CleanupOldData()
        {
            var cutoffDate = DateTime.Now.AddDays(-MaxRetentionDays);

            // 清理旧的统计文件
            try
            {
                var files = Directory.GetFiles(_dataDirectory, "stats_*.json");
                foreach (var file in files)
                {
                    var fileDate = File.GetCreationTime(file);
                    if (fileDate < cutoffDate)
                    {
                        File.Delete(file);
                        Logger.Debug("Statistics", $"[Statistics] 删除过期统计文件: {file}");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Statistics", $"[Statistics] 清理过期数据失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 导出统计数据
        /// </summary>
        public async Task<string> ExportStatisticsAsync(string format = "json")
        {
            var report = GenerateReport();
            var fileName = $"Rectangle_Statistics_{DateTime.Now:yyyyMMdd_HHmmss}.{format}";
            var filePath = Path.Combine(_dataDirectory, fileName);

            if (format.ToLower() == "json")
            {
                var json = JsonSerializer.Serialize(report, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                await File.WriteAllTextAsync(filePath, json);
            }
            else if (format.ToLower() == "csv")
            {
                // 简化CSV导出
                var csv = new System.Text.StringBuilder();
                csv.AppendLine("Application,Usage Seconds,Activation Count,Layout Count");
                foreach (var app in report.TopApplications)
                {
                    csv.AppendLine($"{app.ApplicationName},{app.ActiveUsageSeconds},{app.ActivationCount},{app.LayoutAdjustmentCount}");
                }
                await File.WriteAllTextAsync(filePath, csv.ToString());
            }

            Logger.Info("Statistics", $"[Statistics] 统计数据已导出: {filePath}");
            return filePath;
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);
    }

    /// <summary>
    /// 统计数据存储结构
    /// </summary>
    internal class StatisticsData
    {
        public Dictionary<string, WindowUsageStats> WindowStats { get; set; } = new();
        public Dictionary<WindowAction, LayoutUsageStats> LayoutStats { get; set; } = new();
        public List<HeatmapDataPoint> HeatmapData { get; set; } = new();
        public DateTime SavedAt { get; set; }
    }
}
