using System.Management;
using LibreHardwareMonitor.Hardware;

namespace DipcClient;

public static class TemperatureCollector
{
    public static List<TemperatureSensorInfo> GetTemperatures()
    {
        var sensors = new List<TemperatureSensorInfo>();
        sensors.AddRange(ReadLibreHardwareMonitor());
        sensors.AddRange(ReadAcpiThermalZones());

        return sensors
            .Where(s => !string.IsNullOrWhiteSpace(s.Name))
            .GroupBy(s => s.Name, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.OrderByDescending(x => x.Celsius ?? double.MinValue).First())
            .OrderBy(s => s.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static IEnumerable<TemperatureSensorInfo> ReadLibreHardwareMonitor()
    {
        var list = new List<TemperatureSensorInfo>();

        try
        {
            var computer = new Computer
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
                IsMotherboardEnabled = true,
                IsStorageEnabled = true,
                IsMemoryEnabled = true,
                IsNetworkEnabled = false
            };
            try
            {
                computer.Open();

                foreach (var hardware in computer.Hardware)
                {
                    CollectHardwareTemps(hardware, list);
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

    private static void CollectHardwareTemps(IHardware hardware, List<TemperatureSensorInfo> list)
    {
        try
        {
            hardware.Update();

            foreach (var sensor in hardware.Sensors)
            {
                if (sensor.SensorType != SensorType.Temperature)
                {
                    continue;
                }

                if (sensor.Value is null)
                {
                    continue;
                }

                list.Add(new TemperatureSensorInfo
                {
                    Name = $"{hardware.HardwareType} {hardware.Name} - {sensor.Name}",
                    Celsius = sensor.Value.Value
                });
            }

            foreach (var sub in hardware.SubHardware)
            {
                CollectHardwareTemps(sub, list);
            }
        }
        catch
        {
        }
    }

    private static IEnumerable<TemperatureSensorInfo> ReadAcpiThermalZones()
    {
        var list = new List<TemperatureSensorInfo>();
        try
        {
            using var searcher = new ManagementObjectSearcher(@"root\wmi", "SELECT InstanceName, CurrentTemperature FROM MSAcpi_ThermalZoneTemperature");
            foreach (var obj in searcher.Get().Cast<ManagementObject>())
            {
                var raw = obj["CurrentTemperature"];
                var c = ConvertAcpiToCelsius(raw);
                list.Add(new TemperatureSensorInfo
                {
                    Name = $"ACPI {obj["InstanceName"]}",
                    Celsius = c
                });
            }
        }
        catch
        {
        }

        return list;
    }

    private static double? ConvertAcpiToCelsius(object? value)
    {
        if (value is null)
        {
            return null;
        }

        try
        {
            var kelvinTimes10 = Convert.ToDouble(value);
            if (kelvinTimes10 <= 0)
            {
                return null;
            }

            return (kelvinTimes10 / 10.0) - 273.15;
        }
        catch
        {
            return null;
        }
    }
}
