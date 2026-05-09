using System.Management;
using LibreHardwareMonitor.Hardware;

namespace DipcClient;

public static class DiskSensorCollector
{
    public static List<DiskSensorInfo> GetDiskSensors()
    {
        var list = new List<DiskSensorInfo>();

        var wmiStatusByModel = ReadWmiDiskStatusByModel();

        try
        {
            var computer = new Computer
            {
                IsStorageEnabled = true,
                IsCpuEnabled = false,
                IsGpuEnabled = false,
                IsMotherboardEnabled = false,
                IsMemoryEnabled = false,
                IsNetworkEnabled = false
            };

            try
            {
                computer.Open();

                foreach (var hw in computer.Hardware)
                {
                    Collect(hw, list, wmiStatusByModel);
                }
            }
            finally
            {
                try
                {
                    computer.Close();
                }
                catch
                {
                }
            }
        }
        catch
        {
        }

        return list
            .Where(s => !string.IsNullOrWhiteSpace(s.DiskName) && !string.IsNullOrWhiteSpace(s.SensorName))
            .OrderBy(s => s.DiskName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(s => s.SensorType, StringComparer.OrdinalIgnoreCase)
            .ThenBy(s => s.SensorName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static void Collect(IHardware hardware, List<DiskSensorInfo> list, Dictionary<string, string?> wmiStatusByModel)
    {
        try
        {
            if (hardware.HardwareType == HardwareType.Storage)
            {
                hardware.Update();

                var diskName = hardware.Name;
                var status = TryMatchStatus(diskName, wmiStatusByModel);
                if (!string.IsNullOrWhiteSpace(status))
                {
                    list.Add(new DiskSensorInfo
                    {
                        DiskName = diskName,
                        SensorType = "Status",
                        SensorName = "WMI",
                        TextValue = status
                    });
                }

                foreach (var s in hardware.Sensors)
                {
                    if (s.Value is null)
                    {
                        continue;
                    }

                    var include =
                        s.SensorType == SensorType.Temperature ||
                        s.SensorType == SensorType.Data ||
                        s.SensorType == SensorType.Level ||
                        s.SensorType == SensorType.Factor ||
                        s.SensorType == SensorType.SmallData ||
                        s.SensorType == SensorType.Throughput;

                    if (!include)
                    {
                        continue;
                    }

                    list.Add(new DiskSensorInfo
                    {
                        DiskName = diskName,
                        SensorType = s.SensorType.ToString(),
                        SensorName = s.Name,
                        Value = s.Value,
                        Unit = GuessUnit(s.SensorType)
                    });
                }
            }

            foreach (var sub in hardware.SubHardware)
            {
                Collect(sub, list, wmiStatusByModel);
            }
        }
        catch
        {
        }
    }

    private static string? GuessUnit(SensorType type)
    {
        return type switch
        {
            SensorType.Temperature => "°C",
            SensorType.Level => "%",
            _ => null
        };
    }

    private static Dictionary<string, string?> ReadWmiDiskStatusByModel()
    {
        var dict = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Model, Status FROM Win32_DiskDrive");
            foreach (var obj in searcher.Get().Cast<ManagementObject>())
            {
                var model = AsString(obj["Model"]);
                if (string.IsNullOrWhiteSpace(model))
                {
                    continue;
                }

                dict[NormalizeKey(model)] = AsString(obj["Status"]);
            }
        }
        catch
        {
        }

        return dict;
    }

    private static string? TryMatchStatus(string? diskName, Dictionary<string, string?> statusByModel)
    {
        if (string.IsNullOrWhiteSpace(diskName) || statusByModel.Count == 0)
        {
            return null;
        }

        var key = NormalizeKey(diskName);
        if (statusByModel.TryGetValue(key, out var status))
        {
            return status;
        }

        var bestScore = 0;
        string? best = null;
        foreach (var kv in statusByModel)
        {
            var score = MatchScore(key, kv.Key);
            if (score > bestScore)
            {
                bestScore = score;
                best = kv.Value;
            }
        }

        return best;
    }

    private static int MatchScore(string a, string b)
    {
        if (string.Equals(a, b, StringComparison.OrdinalIgnoreCase))
        {
            return 100;
        }

        if (a.Contains(b, StringComparison.OrdinalIgnoreCase) || b.Contains(a, StringComparison.OrdinalIgnoreCase))
        {
            return 60;
        }

        var tokensA = a.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var tokensB = b.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var hits = tokensA.Count(t => tokensB.Any(x => string.Equals(x, t, StringComparison.OrdinalIgnoreCase)));
        return hits;
    }

    private static string NormalizeKey(string value)
    {
        return value.Trim().Replace("  ", " ", StringComparison.Ordinal);
    }

    private static string? AsString(object? value)
    {
        return value?.ToString()?.Trim();
    }
}

