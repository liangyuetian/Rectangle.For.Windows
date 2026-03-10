using System.Drawing;
using System.Drawing.Drawing2D;
using Rectangle.Windows.Core;

namespace Rectangle.Windows.Views;

/// <summary>
/// 动态生成菜单图标
/// </summary>
public static class MenuIconGenerator
{
    private const int IconSize = 16;
    private static readonly Color IconColor = Color.FromArgb(220, 220, 220);
    private static readonly Color AccentColor = Color.FromArgb(0, 120, 212);

    public static Image? GenerateIcon(WindowAction action)
    {
        var bmp = new Bitmap(IconSize, IconSize);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.Transparent);

        using var pen = new Pen(IconColor, 1.5f);
        using var brush = new SolidBrush(Color.FromArgb(80, IconColor));
        using var accentBrush = new SolidBrush(Color.FromArgb(120, AccentColor));

        var rect = new System.Drawing.Rectangle(1, 1, IconSize - 3, IconSize - 3);

        switch (action)
        {
            // 半屏
            case WindowAction.LeftHalf:
                DrawWindowOutline(g, pen);
                g.FillRectangle(accentBrush, 2, 2, 6, 11);
                break;
            case WindowAction.RightHalf:
                DrawWindowOutline(g, pen);
                g.FillRectangle(accentBrush, 8, 2, 6, 11);
                break;
            case WindowAction.CenterHalf:
                DrawWindowOutline(g, pen);
                g.FillRectangle(accentBrush, 4, 2, 8, 11);
                break;
            case WindowAction.TopHalf:
                DrawWindowOutline(g, pen);
                g.FillRectangle(accentBrush, 2, 2, 12, 5);
                break;
            case WindowAction.BottomHalf:
                DrawWindowOutline(g, pen);
                g.FillRectangle(accentBrush, 2, 8, 12, 5);
                break;

            // 四角
            case WindowAction.TopLeft:
                DrawWindowOutline(g, pen);
                g.FillRectangle(accentBrush, 2, 2, 6, 5);
                break;
            case WindowAction.TopRight:
                DrawWindowOutline(g, pen);
                g.FillRectangle(accentBrush, 8, 2, 6, 5);
                break;
            case WindowAction.BottomLeft:
                DrawWindowOutline(g, pen);
                g.FillRectangle(accentBrush, 2, 8, 6, 5);
                break;
            case WindowAction.BottomRight:
                DrawWindowOutline(g, pen);
                g.FillRectangle(accentBrush, 8, 8, 6, 5);
                break;

            // 三分之一
            case WindowAction.FirstThird:
                DrawWindowOutline(g, pen);
                g.FillRectangle(accentBrush, 2, 2, 4, 11);
                break;
            case WindowAction.CenterThird:
                DrawWindowOutline(g, pen);
                g.FillRectangle(accentBrush, 6, 2, 4, 11);
                break;
            case WindowAction.LastThird:
                DrawWindowOutline(g, pen);
                g.FillRectangle(accentBrush, 10, 2, 4, 11);
                break;

            // 三分之二
            case WindowAction.FirstTwoThirds:
                DrawWindowOutline(g, pen);
                g.FillRectangle(accentBrush, 2, 2, 8, 11);
                break;
            case WindowAction.CenterTwoThirds:
                DrawWindowOutline(g, pen);
                g.FillRectangle(accentBrush, 4, 2, 8, 11);
                break;
            case WindowAction.LastTwoThirds:
                DrawWindowOutline(g, pen);
                g.FillRectangle(accentBrush, 6, 2, 8, 11);
                break;

            // 最大化相关
            case WindowAction.Maximize:
                DrawWindowOutline(g, pen);
                g.FillRectangle(accentBrush, 2, 2, 12, 11);
                break;
            case WindowAction.AlmostMaximize:
                DrawWindowOutline(g, pen);
                g.FillRectangle(accentBrush, 3, 3, 10, 9);
                break;
            case WindowAction.MaximizeHeight:
                DrawWindowOutline(g, pen);
                g.FillRectangle(accentBrush, 5, 2, 6, 11);
                break;
            case WindowAction.Center:
                DrawWindowOutline(g, pen);
                g.FillRectangle(accentBrush, 4, 4, 8, 7);
                break;
            case WindowAction.Restore:
                DrawWindowOutline(g, pen);
                g.DrawRectangle(pen, 4, 4, 7, 6);
                break;

            // 放大缩小
            case WindowAction.Larger:
                DrawWindowOutline(g, pen);
                g.DrawLine(pen, 8, 5, 8, 10);
                g.DrawLine(pen, 5, 7, 11, 7);
                break;
            case WindowAction.Smaller:
                DrawWindowOutline(g, pen);
                g.DrawLine(pen, 5, 7, 11, 7);
                break;

            // 显示器
            case WindowAction.NextDisplay:
                DrawWindowOutline(g, pen);
                DrawArrowRight(g, pen, 6, 7);
                break;
            case WindowAction.PreviousDisplay:
                DrawWindowOutline(g, pen);
                DrawArrowLeft(g, pen, 6, 7);
                break;

            default:
                DrawWindowOutline(g, pen);
                break;
        }

        return bmp;
    }

    private static void DrawWindowOutline(Graphics g, Pen pen)
    {
        g.DrawRectangle(pen, 1, 1, 13, 12);
    }

    private static void DrawArrowRight(Graphics g, Pen pen, int x, int y)
    {
        g.DrawLine(pen, x, y, x + 4, y);
        g.DrawLine(pen, x + 2, y - 2, x + 4, y);
        g.DrawLine(pen, x + 2, y + 2, x + 4, y);
    }

    private static void DrawArrowLeft(Graphics g, Pen pen, int x, int y)
    {
        g.DrawLine(pen, x, y, x + 4, y);
        g.DrawLine(pen, x, y, x + 2, y - 2);
        g.DrawLine(pen, x, y, x + 2, y + 2);
    }
}
