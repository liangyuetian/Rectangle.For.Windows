using Xunit;
using Rectangle.Windows.Core;

namespace Rectangle.Windows.Tests;

/// <summary>
/// WindowHistory 单元测试
/// </summary>
public class WindowHistoryTests
{
    [Fact]
    public void SaveRestoreRect_ShouldSaveAndRetrieve()
    {
        var history = new WindowHistory();
        var hwnd = (nint)12345;

        history.SaveRestoreRect(hwnd, 100, 200, 800, 600);

        Assert.True(history.TryGetRestoreRect(hwnd, out var rect));
        Assert.Equal(100, rect.X);
        Assert.Equal(200, rect.Y);
        Assert.Equal(800, rect.W);
        Assert.Equal(600, rect.H);
    }

    [Fact]
    public void SaveRestoreRectIfNotExists_ShouldNotOverwrite()
    {
        var history = new WindowHistory();
        var hwnd = (nint)12345;

        history.SaveRestoreRectIfNotExists(hwnd, 100, 200, 800, 600);
        history.SaveRestoreRectIfNotExists(hwnd, 300, 400, 1024, 768);

        Assert.True(history.TryGetRestoreRect(hwnd, out var rect));
        Assert.Equal(100, rect.X);  // 应该保持第一次的值
        Assert.Equal(200, rect.Y);
        Assert.Equal(800, rect.W);
        Assert.Equal(600, rect.H);
    }

    [Fact]
    public void RecordAction_ShouldRecordAction()
    {
        var history = new WindowHistory();
        var hwnd = (nint)12345;

        history.RecordAction(hwnd, WindowAction.LeftHalf, 0, 0, 960, 1080);

        Assert.True(history.TryGetLastAction(hwnd, out var action));
        Assert.Equal(WindowAction.LeftHalf, action.Action);
        Assert.Equal(1, action.Count);
    }

    [Fact]
    public void RecordAction_ShouldIncrementCountForSameAction()
    {
        var history = new WindowHistory();
        var hwnd = (nint)12345;

        history.RecordAction(hwnd, WindowAction.LeftHalf, 0, 0, 960, 1080);
        history.RecordAction(hwnd, WindowAction.LeftHalf, 0, 0, 960, 1080);

        Assert.True(history.TryGetLastAction(hwnd, out var action));
        Assert.Equal(WindowAction.LeftHalf, action.Action);
        Assert.Equal(2, action.Count);
    }

    [Fact]
    public void RecordAction_ShouldResetCountForDifferentAction()
    {
        var history = new WindowHistory();
        var hwnd = (nint)12345;

        history.RecordAction(hwnd, WindowAction.LeftHalf, 0, 0, 960, 1080);
        history.RecordAction(hwnd, WindowAction.LeftHalf, 0, 0, 960, 1080);
        history.RecordAction(hwnd, WindowAction.RightHalf, 960, 0, 960, 1080);

        Assert.True(history.TryGetLastAction(hwnd, out var action));
        Assert.Equal(WindowAction.RightHalf, action.Action);
        Assert.Equal(1, action.Count);  // 应该重置为 1
    }

    [Fact]
    public void MarkAsProgramAdjusted_ShouldMarkWindow()
    {
        var history = new WindowHistory();
        var hwnd = (nint)12345;

        history.MarkAsProgramAdjusted(hwnd);

        Assert.True(history.IsProgramAdjusted(hwnd));
    }

    [Fact]
    public void ClearProgramAdjustedMark_ShouldClearMark()
    {
        var history = new WindowHistory();
        var hwnd = (nint)12345;

        history.MarkAsProgramAdjusted(hwnd);
        history.ClearProgramAdjustedMark(hwnd);

        Assert.False(history.IsProgramAdjusted(hwnd));
    }

    [Fact]
    public void RemoveWindow_ShouldRemoveAllRecords()
    {
        var history = new WindowHistory();
        var hwnd = (nint)12345;

        history.SaveRestoreRect(hwnd, 100, 200, 800, 600);
        history.RecordAction(hwnd, WindowAction.LeftHalf, 0, 0, 960, 1080);
        history.MarkAsProgramAdjusted(hwnd);

        history.RemoveWindow(hwnd);

        Assert.False(history.TryGetRestoreRect(hwnd, out _));
        Assert.False(history.TryGetLastAction(hwnd, out _));
        Assert.False(history.IsProgramAdjusted(hwnd));
    }

    [Fact]
    public void Clear_ShouldRemoveAllRecords()
    {
        var history = new WindowHistory();
        var hwnd1 = (nint)12345;
        var hwnd2 = (nint)67890;

        history.SaveRestoreRect(hwnd1, 100, 200, 800, 600);
        history.SaveRestoreRect(hwnd2, 300, 400, 1024, 768);

        history.Clear();

        Assert.False(history.TryGetRestoreRect(hwnd1, out _));
        Assert.False(history.TryGetRestoreRect(hwnd2, out _));
    }

    [Fact]
    public void IsWindowMovedExternally_ShouldDetectMovement()
    {
        var history = new WindowHistory();
        var hwnd = (nint)12345;

        history.RecordAction(hwnd, WindowAction.LeftHalf, 0, 0, 960, 1080);

        // 相同位置，应该返回 false
        Assert.False(history.IsWindowMovedExternally(hwnd, 0, 0, 960, 1080));

        // 移动超过 2 像素，应该返回 true
        Assert.True(history.IsWindowMovedExternally(hwnd, 10, 10, 960, 1080));
    }

    [Fact]
    public void SaveUserAdjustment_ShouldNotSaveForProgramAdjusted()
    {
        var history = new WindowHistory();
        var hwnd = (nint)12345;

        history.MarkAsProgramAdjusted(hwnd);
        history.SaveUserAdjustment(hwnd, 100, 200, 800, 600);

        Assert.False(history.TryGetRestoreRect(hwnd, out _));
    }

    [Fact]
    public void SaveUserAdjustment_ShouldSaveForUserAdjusted()
    {
        var history = new WindowHistory();
        var hwnd = (nint)12345;

        history.SaveUserAdjustment(hwnd, 100, 200, 800, 600);

        Assert.True(history.TryGetRestoreRect(hwnd, out var rect));
        Assert.Equal(100, rect.X);
        Assert.Equal(200, rect.Y);
    }

    [Fact]
    public void GetHistoryCount_ShouldReturnCorrectCount()
    {
        var history = new WindowHistory();

        Assert.Equal(0, history.GetHistoryCount());

        history.SaveRestoreRect((nint)1, 0, 0, 100, 100);
        Assert.Equal(1, history.GetHistoryCount());

        history.SaveRestoreRect((nint)2, 0, 0, 100, 100);
        Assert.Equal(2, history.GetHistoryCount());
    }

    [Fact]
    public void GetStats_ShouldReturnCorrectStats()
    {
        var history = new WindowHistory();
        var hwnd = (nint)12345;

        history.SaveRestoreRect(hwnd, 100, 200, 800, 600);
        history.RecordAction(hwnd, WindowAction.LeftHalf, 0, 0, 960, 1080);
        history.MarkAsProgramAdjusted(hwnd);

        var stats = history.GetStats();

        Assert.Equal(1, stats.RestoreRects);
        Assert.Equal(1, stats.LastActions);
        Assert.Equal(1, stats.ProgramAdjusted);
        Assert.Equal(1, stats.AccessTimes);
    }
}
