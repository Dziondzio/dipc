using System.Management;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Windows.Forms;
using Microsoft.Win32;

namespace DipcClient;

public static class PcInfoCollector
{
    public static PcReport Collect(CollectOptions? options = null)
    {
        options ??= new CollectOptions();

        var now = DateTimeOffset.UtcNow;
        var (os, performance) = GetOsAndPerformance(now);
        var disks = GetDiskDrives(now, options.CollectSmartDiskInfo);
        var diskSensors = options.CollectSmartDiskInfo ? DiskSensorCollector.GetDiskSensors() : [];

        var temperatures = options.CollectTemperatures
            ? new TemperatureInfo { Sensors = TemperatureCollector.GetTemperatures() }
            : new TemperatureInfo();

        var events = options.CollectEvents
            ? new EventLogInfo
            {
                Items = WindowsEventLogCollector.GetCriticalErrorWarningEvents(
                    TimeSpan.FromDays(Math.Clamp(options.EventLookbackDays, 1, 60)),
                    Math.Clamp(options.MaxEvents, 50, 2000))
            }
            : new EventLogInfo();

        var report = new PcReport
        {
            CollectedAtUtc = now,
            MachineId = GetMachineId() ?? Environment.MachineName,
            ComputerName = Environment.MachineName,
            UserName = Environment.UserName,
            Cpu = GetCpu(),
            Gpus = GetGpus(),
            Ram = GetRam(),
            Motherboard = GetMotherboard(),
            Bios = GetBios(),
            Os = os,
            Security = GetSecurity(),
            Performance = performance,
            Displays = GetDisplays(),
            Temperatures = temperatures,
            Events = events,
            DiskDrives = disks,
            DiskSensors = diskSensors,
            LogicalDisks = GetLogicalDisks(),
            Network = GetNetwork()
        };

        return report;
    }

    private static string? GetMachineId()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT UUID FROM Win32_ComputerSystemProduct");
            foreach (var obj in searcher.Get().Cast<ManagementObject>())
            {
                var uuid = obj["UUID"]?.ToString();
                if (!string.IsNullOrWhiteSpace(uuid) && !string.Equals(uuid, "FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF", StringComparison.OrdinalIgnoreCase))
                {
                    return uuid.Trim();
                }
            }
        }
        catch
        {
        }

        return null;
    }

    private static CpuInfo GetCpu()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Name, NumberOfCores, NumberOfLogicalProcessors, MaxClockSpeed, CurrentClockSpeed FROM Win32_Processor");
            var obj = searcher.Get().Cast<ManagementObject>().FirstOrDefault();
            if (obj is null)
            {
                return new CpuInfo();
            }

            return new CpuInfo
            {
                Name = AsString(obj["Name"]),
                Cores = AsUInt(obj["NumberOfCores"]),
                LogicalProcessors = AsUInt(obj["NumberOfLogicalProcessors"]),
                MaxClockMHz = AsUInt(obj["MaxClockSpeed"]),
                CurrentClockMHz = AsUInt(obj["CurrentClockSpeed"])
            };
        }
        catch
        {
            return new CpuInfo();
        }
    }

    private static List<GpuInfo> GetGpus()
    {
        var list = new List<GpuInfo>();
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Name, DriverVersion, VideoProcessor, AdapterRAM, PNPDeviceID FROM Win32_VideoController");
            foreach (var obj in searcher.Get().Cast<ManagementObject>())
            {
                var name = AsString(obj["Name"]);
                var pnpDeviceId = AsString(obj["PNPDeviceID"]);
                var registryVram = TryGetGpuMemoryBytesFromRegistry(pnpDeviceId);

                list.Add(new GpuInfo
                {
                    Name = name,
                    DriverVersion = AsString(obj["DriverVersion"]),
                    VideoProcessor = AsString(obj["VideoProcessor"]),
                    AdapterRamBytes = registryVram ?? AsULong(obj["AdapterRAM"])
                });
            }
        }
        catch
        {
        }

        return list;
    }

    private static RamInfo GetRam()
    {
        var modules = new List<RamModuleInfo>();
        try
        {
            using var moduleSearcher = new ManagementObjectSearcher("SELECT Manufacturer, PartNumber, Capacity, Speed, SerialNumber FROM Win32_PhysicalMemory");
            foreach (var obj in moduleSearcher.Get().Cast<ManagementObject>())
            {
                modules.Add(new RamModuleInfo
                {
                    Manufacturer = AsString(obj["Manufacturer"]),
                    PartNumber = AsString(obj["PartNumber"]),
                    CapacityBytes = AsULong(obj["Capacity"]),
                    SpeedMHz = AsUInt(obj["Speed"]),
                    SerialNumber = AsString(obj["SerialNumber"])
                });
            }
        }
        catch
        {
        }

        ulong? totalBytes = null;
        try
        {
            using var csSearcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem");
            var obj = csSearcher.Get().Cast<ManagementObject>().FirstOrDefault();
            totalBytes = AsULong(obj?["TotalPhysicalMemory"]);
        }
        catch
        {
        }

        return new RamInfo
        {
            TotalBytes = totalBytes,
            Modules = modules
        };
    }

    private static MotherboardInfo GetMotherboard()
    {
        string? sysManufacturer = null;
        string? sysModel = null;
        try
        {
            using var sysSearcher = new ManagementObjectSearcher("SELECT Manufacturer, Model FROM Win32_ComputerSystem");
            var obj = sysSearcher.Get().Cast<ManagementObject>().FirstOrDefault();
            sysManufacturer = AsString(obj?["Manufacturer"]);
            sysModel = AsString(obj?["Model"]);
        }
        catch
        {
        }

        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Manufacturer, Product, SerialNumber FROM Win32_BaseBoard");
            var obj = searcher.Get().Cast<ManagementObject>().FirstOrDefault();
            if (obj is null)
            {
                return new MotherboardInfo { SystemManufacturer = sysManufacturer, SystemModel = sysModel };
            }

            return new MotherboardInfo
            {
                Manufacturer = AsString(obj["Manufacturer"]),
                Product = AsString(obj["Product"]),
                SerialNumber = AsString(obj["SerialNumber"]),
                SystemManufacturer = sysManufacturer,
                SystemModel = sysModel
            };
        }
        catch
        {
            return new MotherboardInfo { SystemManufacturer = sysManufacturer, SystemModel = sysModel };
        }
    }

    private static BiosInfo GetBios()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Manufacturer, SMBIOSBIOSVersion, ReleaseDate, SerialNumber FROM Win32_BIOS");
            var obj = searcher.Get().Cast<ManagementObject>().FirstOrDefault();
            if (obj is null)
            {
                return new BiosInfo();
            }

            return new BiosInfo
            {
                Manufacturer = AsString(obj["Manufacturer"]),
                Version = AsString(obj["SMBIOSBIOSVersion"]),
                ReleaseDateUtc = AsCimDateTimeUtc(obj["ReleaseDate"]),
                SerialNumber = AsString(obj["SerialNumber"])
            };
        }
        catch
        {
            return new BiosInfo();
        }
    }

    private static (OsInfo os, PerformanceInfo performance) GetOsAndPerformance(DateTimeOffset nowUtc)
    {
        string? productName = null;
        string? displayVersion = null;
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            productName = key?.GetValue("ProductName")?.ToString();
            displayVersion = key?.GetValue("DisplayVersion")?.ToString()
                ?? key?.GetValue("ReleaseId")?.ToString();
        }
        catch
        {
        }

        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Caption, Version, BuildNumber, OSArchitecture, InstallDate, LastBootUpTime, TotalVisibleMemorySize, FreePhysicalMemory FROM Win32_OperatingSystem");
            var obj = searcher.Get().Cast<ManagementObject>().FirstOrDefault();
            if (obj is null)
            {
                return (new OsInfo { WindowsProductName = NormalizeWindowsName(productName, null), DisplayVersion = displayVersion }, new PerformanceInfo());
            }

            var build = AsString(obj["BuildNumber"]);
            var lastBoot = AsCimDateTimeUtc(obj["LastBootUpTime"]);

            var totalVisibleKb = AsULong(obj["TotalVisibleMemorySize"]);
            var freeKb = AsULong(obj["FreePhysicalMemory"]);
            var perf = new PerformanceInfo
            {
                Uptime = lastBoot is null ? null : FormatUptime(nowUtc - lastBoot.Value),
                TotalVisibleMemoryBytes = totalVisibleKb is null ? null : totalVisibleKb.Value * 1024,
                FreePhysicalMemoryBytes = freeKb is null ? null : freeKb.Value * 1024
            };

            return (new OsInfo
            {
                Caption = AsString(obj["Caption"]),
                Version = AsString(obj["Version"]),
                BuildNumber = build,
                DisplayVersion = displayVersion,
                Architecture = AsString(obj["OSArchitecture"]),
                InstallDateUtc = AsCimDateTimeUtc(obj["InstallDate"]),
                LastBootUpTimeUtc = lastBoot,
                WindowsProductName = NormalizeWindowsName(productName, build)
            }, perf);
        }
        catch
        {
            return (new OsInfo { WindowsProductName = NormalizeWindowsName(productName, null), DisplayVersion = displayVersion }, new PerformanceInfo());
        }
    }

    private static DisplayInfo GetDisplays()
    {
        var screens = new List<ScreenInfo>();
        try
        {
            foreach (var s in Screen.AllScreens)
            {
                screens.Add(new ScreenInfo
                {
                    DeviceName = s.DeviceName,
                    Bounds = $"{s.Bounds.Width}x{s.Bounds.Height} @ {s.Bounds.X},{s.Bounds.Y}",
                    IsPrimary = s.Primary
                });
            }
        }
        catch
        {
        }

        return new DisplayInfo { Screens = screens };
    }

    private static SecurityInfo GetSecurity()
    {
        string? firmwareType = null;
        bool? secureBootEnabled = null;

        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control");
            var v = key?.GetValue("PEFirmwareType");
            if (v is int i)
            {
                firmwareType = i switch
                {
                    1 => "BIOS (Legacy)",
                    2 => "UEFI",
                    _ => i.ToString()
                };
            }
            else if (v is byte[] bytes && bytes.Length >= 4)
            {
                var n = BitConverter.ToInt32(bytes, 0);
                firmwareType = n switch
                {
                    1 => "BIOS (Legacy)",
                    2 => "UEFI",
                    _ => n.ToString()
                };
            }
        }
        catch
        {
        }

        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\SecureBoot\State");
            var v = key?.GetValue("UEFISecureBootEnabled");
            if (v is int i)
            {
                secureBootEnabled = i != 0;
            }
            else if (v is byte[] bytes && bytes.Length >= 4)
            {
                secureBootEnabled = BitConverter.ToInt32(bytes, 0) != 0;
            }
        }
        catch
        {
        }

        bool? tpmPresent = null;
        string? tpmSpecVersion = null;
        string? tpmManufacturerId = null;
        string? tpmManufacturerVersion = null;

        try
        {
            var scope = new ManagementScope(@"\\.\root\CIMV2\Security\MicrosoftTpm");
            scope.Connect();

            using var searcher = new ManagementObjectSearcher(scope, new ObjectQuery("SELECT SpecVersion, ManufacturerId, ManufacturerVersion FROM Win32_Tpm"));
            var obj = searcher.Get().Cast<ManagementObject>().FirstOrDefault();
            if (obj is not null)
            {
                tpmPresent = true;
                tpmSpecVersion = AsString(obj["SpecVersion"]);
                tpmManufacturerId = obj["ManufacturerId"]?.ToString();
                tpmManufacturerVersion = AsString(obj["ManufacturerVersion"]);
            }
            else
            {
                tpmPresent = false;
            }
        }
        catch
        {
        }

        return new SecurityInfo
        {
            FirmwareType = firmwareType,
            SecureBootEnabled = secureBootEnabled,
            TpmPresent = tpmPresent,
            TpmSpecVersion = tpmSpecVersion,
            TpmManufacturerId = tpmManufacturerId,
            TpmManufacturerVersion = tpmManufacturerVersion
        };
    }

    private static List<DiskDriveInfo> GetDiskDrives(DateTimeOffset nowUtc, bool includeSmart)
    {
        var list = new List<DiskDriveInfo>();
        var smartFromLhm = includeSmart ? LibreHardwareMonitorStorageReader.ReadStorageSmart() : Array.Empty<StorageSmartInfo>();
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Model, InterfaceType, MediaType, Size, SerialNumber, PNPDeviceID FROM Win32_DiskDrive");
            foreach (var obj in searcher.Get().Cast<ManagementObject>())
            {
                var pnp = AsString(obj["PNPDeviceID"]);
                var model = AsString(obj["Model"]);
                var (powerOnHours, powerCycles) = includeSmart ? SmartInfoReader.TryGetPowerOnHoursAndCycles(pnp) : (null, null);
                if (includeSmart && powerOnHours is null && powerCycles is null)
                {
                    (powerOnHours, powerCycles) = LibreHardwareMonitorStorageReader.TryMatchByModel(model, smartFromLhm);
                }
                DateTimeOffset? firstPowerOn = null;
                if (powerOnHours is not null)
                {
                    firstPowerOn = nowUtc - TimeSpan.FromHours(powerOnHours.Value);
                }

                list.Add(new DiskDriveInfo
                {
                    Model = model,
                    InterfaceType = AsString(obj["InterfaceType"]),
                    MediaType = AsString(obj["MediaType"]),
                    SizeBytes = AsULong(obj["Size"]),
                    SerialNumber = AsString(obj["SerialNumber"]),
                    PowerOnHours = powerOnHours,
                    PowerCycleCount = powerCycles,
                    FirstPowerOnUtcEstimated = firstPowerOn
                });
            }
        }
        catch
        {
        }

        return list;
    }

    private static List<LogicalDiskInfo> GetLogicalDisks()
    {
        var list = new List<LogicalDiskInfo>();
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT DeviceID, VolumeName, FileSystem, Size, FreeSpace FROM Win32_LogicalDisk WHERE DriveType=3");
            foreach (var obj in searcher.Get().Cast<ManagementObject>())
            {
                list.Add(new LogicalDiskInfo
                {
                    DeviceId = AsString(obj["DeviceID"]),
                    VolumeName = AsString(obj["VolumeName"]),
                    FileSystem = AsString(obj["FileSystem"]),
                    SizeBytes = AsULong(obj["Size"]),
                    FreeBytes = AsULong(obj["FreeSpace"])
                });
            }
        }
        catch
        {
        }

        return list;
    }

    private static NetworkInfo GetNetwork()
    {
        var adapters = new List<NetworkAdapterInfo>();

        try
        {
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != OperationalStatus.Up)
                {
                    continue;
                }

                var ipAddresses = ni
                    .GetIPProperties()
                    .UnicastAddresses
                    .Where(a => a.Address.AddressFamily is AddressFamily.InterNetwork or AddressFamily.InterNetworkV6)
                    .Select(a => a.Address.ToString())
                    .Distinct()
                    .ToList();

                if (ipAddresses.Count == 0)
                {
                    continue;
                }

                adapters.Add(new NetworkAdapterInfo
                {
                    Name = ni.Name,
                    Description = ni.Description,
                    MacAddress = ni.GetPhysicalAddress().ToString(),
                    IpAddresses = ipAddresses
                });
            }
        }
        catch
        {
        }

        return new NetworkInfo { Adapters = adapters };
    }

    private static string? AsString(object? value)
    {
        var s = value?.ToString();
        return string.IsNullOrWhiteSpace(s) ? null : s.Trim();
    }

    private static uint? AsUInt(object? value)
    {
        if (value is null)
        {
            return null;
        }

        try
        {
            return Convert.ToUInt32(value);
        }
        catch
        {
            return null;
        }
    }

    private static ulong? AsULong(object? value)
    {
        if (value is null)
        {
            return null;
        }

        try
        {
            return Convert.ToUInt64(value);
        }
        catch
        {
            return null;
        }
    }

    private static DateTimeOffset? AsCimDateTimeUtc(object? value)
    {
        var s = value?.ToString();
        if (string.IsNullOrWhiteSpace(s))
        {
            return null;
        }

        try
        {
            var dt = ManagementDateTimeConverter.ToDateTime(s);
            return new DateTimeOffset(DateTime.SpecifyKind(dt, DateTimeKind.Local)).ToUniversalTime();
        }
        catch
        {
            return null;
        }
    }

    private static string? NormalizeWindowsName(string? productName, string? buildNumber)
    {
        if (string.IsNullOrWhiteSpace(productName))
        {
            return null;
        }

        var normalized = productName.Trim();
        if (!normalized.Contains("Windows 10", StringComparison.OrdinalIgnoreCase))
        {
            return normalized;
        }

        if (string.IsNullOrWhiteSpace(buildNumber))
        {
            return normalized;
        }

        if (!int.TryParse(buildNumber.Trim(), out var build))
        {
            return normalized;
        }

        return build >= 22000
            ? normalized.Replace("Windows 10", "Windows 11", StringComparison.OrdinalIgnoreCase)
            : normalized;
    }

    private static string? FormatUptime(TimeSpan uptime)
    {
        if (uptime < TimeSpan.Zero)
        {
            return null;
        }

        var days = (int)uptime.TotalDays;
        var hours = uptime.Hours;
        var minutes = uptime.Minutes;
        if (days > 0)
        {
            return $"{days}d {hours}h {minutes}m";
        }

        return $"{hours}h {minutes}m";
    }

    private static ulong? TryGetGpuMemoryBytesFromRegistry(string? pnpDeviceId)
    {
        if (string.IsNullOrWhiteSpace(pnpDeviceId))
        {
            return null;
        }

        try
        {
            var regPath = @"SYSTEM\CurrentControlSet\Enum\" + pnpDeviceId.Trim() + @"\Device Parameters";
            using var key = Registry.LocalMachine.OpenSubKey(regPath);
            if (key is null)
            {
                return null;
            }

            var value = key.GetValue("HardwareInformation.qwMemorySize");
            if (value is long l && l > 0)
            {
                return (ulong)l;
            }

            if (value is ulong ul && ul > 0)
            {
                return ul;
            }

            if (value is byte[] bytes && bytes.Length >= 8)
            {
                var n = BitConverter.ToUInt64(bytes, 0);
                return n == 0 ? null : n;
            }
        }
        catch
        {
        }

        return null;
    }
}
