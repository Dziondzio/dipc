using LibreHardwareMonitor.Hardware;

namespace DipcClient;

public static class LibreHardwareMonitorStorageReader
{
    public static IReadOnlyList<StorageSmartInfo> ReadStorageSmart()
    {
        var list = new List<StorageSmartInfo>();

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
                    Collect(hw, list);
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

        return list;
    }

    private static void Collect(IHardware hardware, List<StorageSmartInfo> list)
    {
        try
        {
            if (hardware.HardwareType == HardwareType.Storage)
            {
                hardware.Update();

                uint? powerOnHours = null;
                uint? powerCycleCount = null;

                foreach (var s in hardware.Sensors)
                {
                    if (s.SensorType != SensorType.Data || s.Value is null)
                    {
                        continue;
                    }

                    var name = s.Name ?? "";
                    if (name.Contains("Power-On Hours", StringComparison.OrdinalIgnoreCase) ||
                        name.Contains("Power On Hours", StringComparison.OrdinalIgnoreCase) ||
                        name.Contains("Poweron Hours", StringComparison.OrdinalIgnoreCase))
                    {
                        var v = (uint)Math.Max(0, Math.Round(s.Value.Value));
                        powerOnHours = v;
                    }
                    else if (name.Contains("Power Cycle Count", StringComparison.OrdinalIgnoreCase) ||
                             name.Contains("Power-Cycle Count", StringComparison.OrdinalIgnoreCase))
                    {
                        var v = (uint)Math.Max(0, Math.Round(s.Value.Value));
                        powerCycleCount = v;
                    }
                }

                list.Add(new StorageSmartInfo
                {
                    DeviceName = hardware.Name,
                    PowerOnHours = powerOnHours,
                    PowerCycleCount = powerCycleCount
                });
            }

            foreach (var sub in hardware.SubHardware)
            {
                Collect(sub, list);
            }
        }
        catch
        {
        }
    }

    public static (uint? powerOnHours, uint? powerCycles) TryMatchByModel(string? model, IReadOnlyList<StorageSmartInfo> smart)
    {
        if (string.IsNullOrWhiteSpace(model) || smart.Count == 0)
        {
            return (null, null);
        }

        var m = Normalize(model);

        uint? bestHours = null;
        uint? bestCycles = null;
        var bestScore = 0;

        foreach (var s in smart)
        {
            var name = Normalize(s.DeviceName ?? "");
            var score = MatchScore(m, name);
            if (score <= bestScore)
            {
                continue;
            }

            bestScore = score;
            bestHours = s.PowerOnHours;
            bestCycles = s.PowerCycleCount;
        }

        return (bestHours, bestCycles);
    }

    private static string Normalize(string value)
    {
        return value.Trim().Replace("  ", " ", StringComparison.Ordinal);
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
}

public sealed class StorageSmartInfo
{
    public string? DeviceName { get; init; }
    public uint? PowerOnHours { get; init; }
    public uint? PowerCycleCount { get; init; }
}
