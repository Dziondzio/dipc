using System.Text.Json;

namespace DipcClient;

public sealed class AppSettings
{
    public string ServerUrl { get; set; } = "http://localhost:5077/";
    public string ApiKey { get; set; } = "";
    public bool CollectEvents { get; set; } = true;
    public int EventLookbackDays { get; set; } = 7;
    public int MaxEvents { get; set; } = 200;
    public bool CollectTemperatures { get; set; } = true;
    public bool CollectSmartDiskInfo { get; set; } = true;

    public static JsonSerializerOptions ReportJsonOptions { get; } = new()
    {
        WriteIndented = true,
        TypeInfoResolver = PcReportJsonContext.Default
    };

    public static JsonSerializerOptions SettingsJsonOptions { get; } = new()
    {
        WriteIndented = true
    };
}
