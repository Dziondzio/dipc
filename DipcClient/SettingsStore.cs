using System.Text.Json;

namespace DipcClient;

public static class SettingsStore
{
    public static AppSettings Load()
    {
        try
        {
            var settingsPath = GetSettingsPath();
            if (!File.Exists(settingsPath))
            {
                return new AppSettings();
            }

            var json = File.ReadAllText(settingsPath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json, AppSettings.SettingsJsonOptions) ?? new AppSettings();

            if (string.IsNullOrWhiteSpace(settings.ServerUrl))
            {
                settings.ServerUrl = new AppSettings().ServerUrl;
            }

            if (settings.EventLookbackDays <= 0)
            {
                settings.EventLookbackDays = new AppSettings().EventLookbackDays;
            }

            if (settings.MaxEvents <= 0)
            {
                settings.MaxEvents = new AppSettings().MaxEvents;
            }

            return settings;
        }
        catch
        {
            return new AppSettings();
        }
    }

    public static void Save(AppSettings settings)
    {
        var (settingsDir, settingsPath) = GetSettingsLocation();
        Directory.CreateDirectory(settingsDir);
        var json = JsonSerializer.Serialize(settings, AppSettings.SettingsJsonOptions);
        File.WriteAllText(settingsPath, json);
    }

    private static string GetSettingsPath()
    {
        var (_, path) = GetSettingsLocation();
        return path;
    }

    private static (string dir, string path) GetSettingsLocation()
    {
        var baseDir = AppContext.BaseDirectory;
        var portableFlag = Path.Combine(baseDir, "portable.flag");
        var portableEnv = Environment.GetEnvironmentVariable("DIPC_PORTABLE");
        var isRemovable = IsBaseDirectoryOnRemovableDrive(baseDir);

        if (isRemovable || File.Exists(portableFlag) || string.Equals(portableEnv, "1", StringComparison.OrdinalIgnoreCase))
        {
            var localPath = Path.Combine(baseDir, "settings.json");
            return (baseDir, localPath);
        }

        var settingsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DipcClient");
        var settingsPath = Path.Combine(settingsDir, "settings.json");
        return (settingsDir, settingsPath);
    }

    private static bool IsBaseDirectoryOnRemovableDrive(string baseDir)
    {
        try
        {
            var root = Path.GetPathRoot(baseDir);
            if (string.IsNullOrWhiteSpace(root))
            {
                return false;
            }

            var drive = new DriveInfo(root);
            return drive.DriveType == DriveType.Removable;
        }
        catch
        {
            return false;
        }
    }
}
