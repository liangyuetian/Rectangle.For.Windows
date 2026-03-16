using System.Collections.Generic;
using System.Text.Json.Serialization;
using Rectangle.Windows.WinUI.Services;

namespace Rectangle.Windows.WinUI.Services;

[JsonSerializable(typeof(AppConfig))]
[JsonSerializable(typeof(SnapAreaConfig))]
[JsonSerializable(typeof(WindowLayout))]
[JsonSerializable(typeof(List<WindowLayout>))]
[JsonSerializable(typeof(StatisticsData))]
[JsonSerializable(typeof(StatisticsReport))]
[JsonSerializable(typeof(LogEntry))]
[JsonSerializable(typeof(object))]
internal partial class AppJsonContext : JsonSerializerContext
{
}
