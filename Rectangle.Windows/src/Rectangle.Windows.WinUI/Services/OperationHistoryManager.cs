using Rectangle.Windows.WinUI.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rectangle.Windows.WinUI.Services
{
    /// <summary>
    /// 操作历史记录项
    /// </summary>
    public class OperationHistoryItem
    {
        /// <summary>
        /// 唯一标识
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 操作时间
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;

        /// <summary>
        /// 执行的窗口操作
        /// </summary>
        public WindowAction Action { get; set; }

        /// <summary>
        /// 窗口句柄
        /// </summary>
        public nint WindowHandle { get; set; }

        /// <summary>
        /// 窗口标题
        /// </summary>
        public string WindowTitle { get; set; } = "";

        /// <summary>
        /// 窗口类名
        /// </summary>
        public string WindowClass { get; set; } = "";

        /// <summary>
        /// 操作前的窗口位置
        /// </summary>
        public WindowRect RectBefore { get; set; }

        /// <summary>
        /// 操作后的窗口位置
        /// </summary>
        public WindowRect RectAfter { get; set; }

        /// <summary>
        /// 是否已撤销
        /// </summary>
        public bool IsUndone { get; set; } = false;

        /// <summary>
        /// 关联的操作（用于撤销后记录重做信息）
        /// </summary>
        public Guid? RelatedOperationId { get; set; }

        /// <summary>
        /// 操作描述
        /// </summary>
        public string Description => $"{Action} - {WindowTitle}";
    }

    /// <summary>
    /// 操作历史管理器 - 支持撤销/重做功能
    /// </summary>
    public class OperationHistoryManager
    {
        private readonly List<OperationHistoryItem> _history = new();
        private readonly object _lock = new();
        private readonly Logger _logger;

        /// <summary>
        /// 最大历史记录数量
        /// </summary>
        public int MaxHistoryCount { get; set; } = 50;

        /// <summary>
        /// 历史记录变更事件
        /// </summary>
        public event EventHandler<OperationHistoryItem>? HistoryAdded;

        /// <summary>
        /// 撤销事件
        /// </summary>
        public event EventHandler<OperationHistoryItem>? Undone;

        /// <summary>
        /// 重做事件
        /// </summary>
        public event EventHandler<OperationHistoryItem>? Redone;

        /// <summary>
        /// 历史记录清空事件
        /// </summary>
        public event EventHandler? HistoryCleared;

        public OperationHistoryManager(Logger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 记录操作历史
        /// </summary>
        public void RecordOperation(
            WindowAction action,
            nint windowHandle,
            string windowTitle,
            string windowClass,
            WindowRect rectBefore,
            WindowRect rectAfter)
        {
            lock (_lock)
            {
                // 如果位置没有变化，不记录
                if (rectBefore == rectAfter)
                {
                    return;
                }

                var item = new OperationHistoryItem
                {
                    Action = action,
                    WindowHandle = windowHandle,
                    WindowTitle = windowTitle,
                    WindowClass = windowClass,
                    RectBefore = rectBefore,
                    RectAfter = rectAfter
                };

                // 移除已撤销的操作（新的操作会覆盖重做栈）
                _history.RemoveAll(h => h.IsUndone);

                // 添加到历史记录
                _history.Add(item);

                // 限制历史记录数量
                TrimHistory();

                _logger?.LogDebug($"[OperationHistory] 记录操作: {item.Description}");
                HistoryAdded?.Invoke(this, item);
            }
        }

        /// <summary>
        /// 撤销上一次操作
        /// </summary>
        /// <returns>被撤销的操作项，如果没有可撤销的操作则返回null</returns>
        public OperationHistoryItem? Undo()
        {
            lock (_lock)
            {
                // 找到最后一个未撤销的操作
                var item = _history.LastOrDefault(h => !h.IsUndone);
                if (item == null)
                {
                    _logger?.LogDebug("[OperationHistory] 没有可撤销的操作");
                    return null;
                }

                item.IsUndone = true;
                _logger?.LogInfo($"[OperationHistory] 撤销操作: {item.Description}");
                Undone?.Invoke(this, item);
                return item;
            }
        }

        /// <summary>
        /// 重做上一次撤销的操作
        /// </summary>
        /// <returns>被重做的操作项，如果没有可重做的操作则返回null</returns>
        public OperationHistoryItem? Redo()
        {
            lock (_lock)
            {
                // 找到第一个已撤销的操作（按时间顺序）
                var item = _history.FirstOrDefault(h => h.IsUndone);
                if (item == null)
                {
                    _logger?.LogDebug("[OperationHistory] 没有可重做的操作");
                    return null;
                }

                item.IsUndone = false;
                _logger?.LogInfo($"[OperationHistory] 重做操作: {item.Description}");
                Redone?.Invoke(this, item);
                return item;
            }
        }

        /// <summary>
        /// 获取所有历史记录
        /// </summary>
        public IReadOnlyList<OperationHistoryItem> GetAllHistory()
        {
            lock (_lock)
            {
                return _history.ToList().AsReadOnly();
            }
        }

        /// <summary>
        /// 获取最近的历史记录
        /// </summary>
        public IReadOnlyList<OperationHistoryItem> GetRecentHistory(int count)
        {
            lock (_lock)
            {
                return _history.TakeLast(count).ToList().AsReadOnly();
            }
        }

        /// <summary>
        /// 获取可撤销的操作
        /// </summary>
        public IReadOnlyList<OperationHistoryItem> GetUndoableOperations()
        {
            lock (_lock)
            {
                return _history.Where(h => !h.IsUndone).ToList().AsReadOnly();
            }
        }

        /// <summary>
        /// 获取可重做的操作
        /// </summary>
        public IReadOnlyList<OperationHistoryItem> GetRedoableOperations()
        {
            lock (_lock)
            {
                return _history.Where(h => h.IsUndone).ToList().AsReadOnly();
            }
        }

        /// <summary>
        /// 检查是否可以撤销
        /// </summary>
        public bool CanUndo => _history.Any(h => !h.IsUndone);

        /// <summary>
        /// 检查是否可以重做
        /// </summary>
        public bool CanRedo => _history.Any(h => h.IsUndone);

        /// <summary>
        /// 清空历史记录
        /// </summary>
        public void ClearHistory()
        {
            lock (_lock)
            {
                _history.Clear();
                _logger?.LogInfo("[OperationHistory] 历史记录已清空");
                HistoryCleared?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 移除指定窗口的历史记录
        /// </summary>
        public void RemoveHistoryForWindow(nint windowHandle)
        {
            lock (_lock)
            {
                _history.RemoveAll(h => h.WindowHandle == windowHandle);
                _logger?.LogDebug($"[OperationHistory] 移除窗口 {windowHandle} 的历史记录");
            }
        }

        /// <summary>
        /// 获取统计信息
        /// </summary>
        public HistoryStatistics GetStatistics()
        {
            lock (_lock)
            {
                return new HistoryStatistics
                {
                    TotalOperations = _history.Count,
                    UndoableOperations = _history.Count(h => !h.IsUndone),
                    RedoableOperations = _history.Count(h => h.IsUndone),
                    MostUsedAction = _history.GroupBy(h => h.Action)
                        .OrderByDescending(g => g.Count())
                        .Select(g => g.Key)
                        .FirstOrDefault(),
                    MostUsedActionCount = _history.GroupBy(h => h.Action)
                        .Max(g => (int?)g.Count()) ?? 0,
                    FirstOperationTime = _history.FirstOrDefault()?.Timestamp,
                    LastOperationTime = _history.LastOrDefault()?.Timestamp
                };
            }
        }

        /// <summary>
        /// 限制历史记录数量
        /// </summary>
        private void TrimHistory()
        {
            while (_history.Count > MaxHistoryCount)
            {
                _history.RemoveAt(0);
            }
        }
    }

    /// <summary>
    /// 历史统计信息
    /// </summary>
    public class HistoryStatistics
    {
        public int TotalOperations { get; set; }
        public int UndoableOperations { get; set; }
        public int RedoableOperations { get; set; }
        public WindowAction MostUsedAction { get; set; }
        public int MostUsedActionCount { get; set; }
        public DateTime? FirstOperationTime { get; set; }
        public DateTime? LastOperationTime { get; set; }
    }
}
