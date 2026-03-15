using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Rectangle.Windows.WinUI.Core;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Windows.Graphics;
using Windows.Graphics.Imaging;
using Windows.Foundation;
using Windows.UI;
using Microsoft.UI.Xaml.Media.Imaging;
using PathShape = Microsoft.UI.Xaml.Shapes.Path;
using RectShape = Microsoft.UI.Xaml.Shapes.Rectangle;

namespace Rectangle.Windows.WinUI.Views;

/// <summary>
/// 动态生成菜单图标 - WinUI 版本
/// 使用 Path 和 Shape 几何图形生成图标
/// </summary>
public static class MenuIconGenerator
{
    private const int IconSize = 16;
    private static readonly Color IconColor = ColorHelper.FromArgb(255, 220, 220, 220);
    private static readonly Color AccentColor = ColorHelper.FromArgb(255, 0, 120, 212);

    /// <summary>
    /// 为指定窗口动作生成图标
    /// </summary>
    public static UIElement? GenerateIcon(WindowAction action)
    {
        return GenerateIconGeometry(action);
    }

    /// <summary>
    /// 生成图标并渲染为 SoftwareBitmap
    /// </summary>
    public static async Task<SoftwareBitmap?> GenerateIconBitmapAsync(WindowAction action)
    {
        var element = GenerateIconGeometry(action);
        if (element == null) return null;

        // 创建 RenderTargetBitmap
        var renderTarget = new RenderTargetBitmap();
        await renderTarget.RenderAsync(element);

        // 获取像素数据
        var pixelBuffer = await renderTarget.GetPixelsAsync();

        // 创建 SoftwareBitmap
        var bitmap = new SoftwareBitmap(BitmapPixelFormat.Bgra8, IconSize, IconSize, BitmapAlphaMode.Premultiplied);
        bitmap.CopyFromBuffer(pixelBuffer);

        return bitmap;
    }

    /// <summary>
    /// 生成图标几何图形
    /// </summary>
    private static UIElement? GenerateIconGeometry(WindowAction action)
    {
        var grid = new Grid
        {
            Width = IconSize,
            Height = IconSize,
            Background = new SolidColorBrush(ColorHelper.FromArgb(0, 0, 0, 0))
        };

        // 创建画布容器
        var canvas = new Canvas
        {
            Width = IconSize,
            Height = IconSize
        };
        grid.Children.Add(canvas);

        // 绘制窗口轮廓（背景）
        var outlinePath = CreateWindowOutline();
        canvas.Children.Add(outlinePath);

        // 根据动作类型绘制填充区域
        var fillShape = CreateFillShape(action);
        if (fillShape != null)
        {
            canvas.Children.Add(fillShape);
        }

        return grid;
    }

    /// <summary>
    /// 创建窗口轮廓
    /// </summary>
    private static PathShape CreateWindowOutline()
    {
        var path = new PathShape
        {
            Stroke = new SolidColorBrush(IconColor),
            StrokeThickness = 1,
            Fill = new SolidColorBrush(ColorHelper.FromArgb(80, IconColor.R, IconColor.G, IconColor.B)),
            Data = new RectangleGeometry
            {
                Rect = new Rect(1, 1, 13, 12)
            }
        };
        return path;
    }

    /// <summary>
    /// 根据动作创建填充形状
    /// </summary>
    private static UIElement? CreateFillShape(WindowAction action)
    {
        var accentBrush = new SolidColorBrush(ColorHelper.FromArgb(120, AccentColor.R, AccentColor.G, AccentColor.B));

        switch (action)
        {
            // 半屏
            case WindowAction.LeftHalf:
                return new RectShape
                {
                    Fill = accentBrush,
                    Width = 6,
                    Height = 11,
                    Margin = new Thickness(2, 2, 0, 0)
                };
            case WindowAction.RightHalf:
                return new RectShape
                {
                    Fill = accentBrush,
                    Width = 6,
                    Height = 11,
                    Margin = new Thickness(8, 2, 0, 0)
                };
            case WindowAction.CenterHalf:
                return new RectShape
                {
                    Fill = accentBrush,
                    Width = 8,
                    Height = 11,
                    Margin = new Thickness(4, 2, 0, 0)
                };
            case WindowAction.TopHalf:
                return new RectShape
                {
                    Fill = accentBrush,
                    Width = 12,
                    Height = 5,
                    Margin = new Thickness(2, 2, 0, 0)
                };
            case WindowAction.BottomHalf:
                return new RectShape
                {
                    Fill = accentBrush,
                    Width = 12,
                    Height = 5,
                    Margin = new Thickness(2, 8, 0, 0)
                };

            // 四角
            case WindowAction.TopLeft:
                return new RectShape
                {
                    Fill = accentBrush,
                    Width = 6,
                    Height = 5,
                    Margin = new Thickness(2, 2, 0, 0)
                };
            case WindowAction.TopRight:
                return new RectShape
                {
                    Fill = accentBrush,
                    Width = 6,
                    Height = 5,
                    Margin = new Thickness(8, 2, 0, 0)
                };
            case WindowAction.BottomLeft:
                return new RectShape
                {
                    Fill = accentBrush,
                    Width = 6,
                    Height = 5,
                    Margin = new Thickness(2, 8, 0, 0)
                };
            case WindowAction.BottomRight:
                return new RectShape
                {
                    Fill = accentBrush,
                    Width = 6,
                    Height = 5,
                    Margin = new Thickness(8, 8, 0, 0)
                };

            // 三分之一
            case WindowAction.FirstThird:
                return new RectShape
                {
                    Fill = accentBrush,
                    Width = 4,
                    Height = 11,
                    Margin = new Thickness(2, 2, 0, 0)
                };
            case WindowAction.CenterThird:
                return new RectShape
                {
                    Fill = accentBrush,
                    Width = 4,
                    Height = 11,
                    Margin = new Thickness(6, 2, 0, 0)
                };
            case WindowAction.LastThird:
                return new RectShape
                {
                    Fill = accentBrush,
                    Width = 4,
                    Height = 11,
                    Margin = new Thickness(10, 2, 0, 0)
                };

            // 三分之二
            case WindowAction.FirstTwoThirds:
                return new RectShape
                {
                    Fill = accentBrush,
                    Width = 8,
                    Height = 11,
                    Margin = new Thickness(2, 2, 0, 0)
                };
            case WindowAction.CenterTwoThirds:
                return new RectShape
                {
                    Fill = accentBrush,
                    Width = 8,
                    Height = 11,
                    Margin = new Thickness(4, 2, 0, 0)
                };
            case WindowAction.LastTwoThirds:
                return new RectShape
                {
                    Fill = accentBrush,
                    Width = 8,
                    Height = 11,
                    Margin = new Thickness(6, 2, 0, 0)
                };

            // 四等分
            case WindowAction.FirstFourth:
                return new RectShape
                {
                    Fill = accentBrush,
                    Width = 3,
                    Height = 11,
                    Margin = new Thickness(2, 2, 0, 0)
                };
            case WindowAction.SecondFourth:
                return new RectShape
                {
                    Fill = accentBrush,
                    Width = 3,
                    Height = 11,
                    Margin = new Thickness(5, 2, 0, 0)
                };
            case WindowAction.ThirdFourth:
                return new RectShape
                {
                    Fill = accentBrush,
                    Width = 3,
                    Height = 11,
                    Margin = new Thickness(8, 2, 0, 0)
                };
            case WindowAction.LastFourth:
                return new RectShape
                {
                    Fill = accentBrush,
                    Width = 3,
                    Height = 11,
                    Margin = new Thickness(11, 2, 0, 0)
                };

            // 最大化相关
            case WindowAction.Maximize:
                return new RectShape
                {
                    Fill = accentBrush,
                    Width = 12,
                    Height = 11,
                    Margin = new Thickness(2, 2, 0, 0)
                };
            case WindowAction.AlmostMaximize:
                return new RectShape
                {
                    Fill = accentBrush,
                    Width = 10,
                    Height = 9,
                    Margin = new Thickness(3, 3, 0, 0)
                };
            case WindowAction.MaximizeHeight:
                return new RectShape
                {
                    Fill = accentBrush,
                    Width = 6,
                    Height = 11,
                    Margin = new Thickness(5, 2, 0, 0)
                };
            case WindowAction.Center:
                return new RectShape
                {
                    Fill = accentBrush,
                    Width = 8,
                    Height = 7,
                    Margin = new Thickness(4, 4, 0, 0)
                };
            case WindowAction.Restore:
                return CreateRestoreIcon();

            // 放大缩小
            case WindowAction.Larger:
                return CreateLargerIcon();
            case WindowAction.Smaller:
                return CreateSmallerIcon();

            // 显示器
            case WindowAction.NextDisplay:
                return CreateDisplayIcon(true);
            case WindowAction.PreviousDisplay:
                return CreateDisplayIcon(false);

            default:
                return null;
        }
    }

    /// <summary>
    /// 创建恢复图标
    /// </summary>
    private static UIElement CreateRestoreIcon()
    {
        var path = new PathShape
        {
            Stroke = new SolidColorBrush(IconColor),
            StrokeThickness = 1,
            Data = new RectangleGeometry
            {
                Rect = new Rect(4, 4, 7, 6)
            }
        };
        return path;
    }

    /// <summary>
    /// 创建放大图标（+）
    /// </summary>
    private static UIElement CreateLargerIcon()
    {
        var grid = new Grid();

        // 水平线
        var hLine = new Line
        {
            Stroke = new SolidColorBrush(IconColor),
            StrokeThickness = 1.5,
            X1 = 5, Y1 = 7, X2 = 11, Y2 = 7
        };
        grid.Children.Add(hLine);

        // 垂直线
        var vLine = new Line
        {
            Stroke = new SolidColorBrush(IconColor),
            StrokeThickness = 1.5,
            X1 = 8, Y1 = 5, X2 = 8, Y2 = 10
        };
        grid.Children.Add(vLine);

        return grid;
    }

    /// <summary>
    /// 创建缩小图标（-）
    /// </summary>
    private static UIElement CreateSmallerIcon()
    {
        return new Line
        {
            Stroke = new SolidColorBrush(IconColor),
            StrokeThickness = 1.5,
            X1 = 5, Y1 = 7, X2 = 11, Y2 = 7
        };
    }

    /// <summary>
    /// 创建显示器图标（带箭头）
    /// </summary>
    private static UIElement CreateDisplayIcon(bool next)
    {
        var grid = new Grid();

        // 箭头
        var arrow = new PathShape
        {
            Fill = new SolidColorBrush(IconColor),
            Data = next ? CreateRightArrowGeometry(6, 7) : CreateLeftArrowGeometry(6, 7)
        };
        grid.Children.Add(arrow);

        return grid;
    }

    /// <summary>
    /// 创建向右箭头几何
    /// </summary>
    private static Geometry CreateRightArrowGeometry(double x, double y)
    {
        var pathGeometry = new PathGeometry();
        var pathFigure = new PathFigure
        {
            StartPoint = new Point(x, y - 2)
        };

        pathFigure.Segments.Add(new LineSegment { Point = new Point(x + 4, y) });
        pathFigure.Segments.Add(new LineSegment { Point = new Point(x, y + 2) });
        pathFigure.Segments.Add(new LineSegment { Point = new Point(x, y - 2) });

        pathGeometry.Figures.Add(pathFigure);
        return pathGeometry;
    }

    /// <summary>
    /// 创建向左箭头几何
    /// </summary>
    private static Geometry CreateLeftArrowGeometry(double x, double y)
    {
        var pathGeometry = new PathGeometry();
        var pathFigure = new PathFigure
        {
            StartPoint = new Point(x + 4, y - 2)
        };

        pathFigure.Segments.Add(new LineSegment { Point = new Point(x, y) });
        pathFigure.Segments.Add(new LineSegment { Point = new Point(x + 4, y + 2) });
        pathFigure.Segments.Add(new LineSegment { Point = new Point(x + 4, y - 2) });

        pathGeometry.Figures.Add(pathFigure);
        return pathGeometry;
    }

    /// <summary>
    /// 创建通用图标（仅窗口轮廓）
    /// </summary>
    public static UIElement CreateGenericIcon()
    {
        var grid = new Grid
        {
            Width = IconSize,
            Height = IconSize
        };

        var outline = CreateWindowOutline();
        grid.Children.Add(outline);

        return grid;
    }
}
