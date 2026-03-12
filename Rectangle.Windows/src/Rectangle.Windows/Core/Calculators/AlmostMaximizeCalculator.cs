using Rectangle.Windows.Core;
using Rectangle.Windows.Services;

namespace Rectangle.Windows.Core.Calculators;

public class AlmostMaximizeCalculator : IRectCalculator
{
    private readonly ConfigService _configService;

    public AlmostMaximizeCalculator(ConfigService configService)
    {
        _configService = configService;
    }

    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action)
    {
        var config = _configService?.Load();
        
        // 获取配置的比例，默认 0.9，范围 0.5 - 1.0
        float widthRatio = config?.AlmostMaximizeWidth ?? 0.9f;
        float heightRatio = config?.AlmostMaximizeHeight ?? 0.9f;
        
        // 验证比例范围
        widthRatio = Math.Clamp(widthRatio, 0.5f, 1.0f);
        heightRatio = Math.Clamp(heightRatio, 0.5f, 1.0f);
        
        // 计算目标尺寸
        int targetWidth = (int)(workArea.Width * widthRatio);
        int targetHeight = (int)(workArea.Height * heightRatio);
        
        // 居中放置
        int x = workArea.Left + (workArea.Width - targetWidth) / 2;
        int y = workArea.Top + (workArea.Height - targetHeight) / 2;
        
        return new WindowRect(x, y, targetWidth, targetHeight);
    }
}
