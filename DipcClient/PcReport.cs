using System.Text.Json.Serialization;

namespace DipcClient;

public sealed class PcReport
{
    public string ReportVersion { get; init; } = "4";
    public DateTimeOffset CollectedAtUtc { get; init; }
    public string MachineId { get; init; } = "";
    public string ComputerName { get; init; } = "";
    public string? UserName { get; init; }

    public CpuInfo Cpu { get; init; } = new();
    public List<GpuInfo> Gpus { get; init; } = [];
    public RamInfo Ram { get; init; } = new();
    public MotherboardInfo Motherboard { get; init; } = new();
    public BiosInfo Bios { get; init; } = new();
    public OsInfo Os { get; init; } = new();
    public SecurityInfo Security { get; init; } = new();
    public PerformanceInfo Performance { get; init; } = new();
    public DisplayInfo Displays { get; init; } = new();
    public TemperatureInfo Temperatures { get; init; } = new();
    public EventLogInfo Events { get; init; } = new();
    public List<DiskDriveInfo> DiskDrives { get; init; } = [];
    public List<DiskSensorInfo> DiskSensors { get; init; } = [];
    public List<LogicalDiskInfo> LogicalDisks { get; init; } = [];
    public NetworkInfo Network { get; init; } = new();
}

public sealed class CpuInfo
{
    public string? Name { get; init; }
    public uint? Cores { get; init; }
    public uint? LogicalProcessors { get; init; }
    public uint? MaxClockMHz { get; init; }
    public uint? CurrentClockMHz { get; init; }
}

public sealed class GpuInfo
{
    public string? Name { get; init; }
    public string? DriverVersion { get; init; }
    public string? VideoProcessor { get; init; }
    public ulong? AdapterRamBytes { get; init; }
}

public sealed class RamInfo
{
    public ulong? TotalBytes { get; init; }
    public List<RamModuleInfo> Modules { get; init; } = [];
}

public sealed class RamModuleInfo
{
    public string? Manufacturer { get; init; }
    public string? PartNumber { get; init; }
    public ulong? CapacityBytes { get; init; }
    public uint? SpeedMHz { get; init; }
    public string? SerialNumber { get; init; }
}

public sealed class MotherboardInfo
{
    public string? Manufacturer { get; init; }
    public string? Product { get; init; }
    public string? SerialNumber { get; init; }
    public string? SystemManufacturer { get; init; }
    public string? SystemModel { get; init; }
}

public sealed class BiosInfo
{
    public string? Manufacturer { get; init; }
    public string? Version { get; init; }
    public DateTimeOffset? ReleaseDateUtc { get; init; }
    public string? SerialNumber { get; init; }
}

public sealed class OsInfo
{
    public string? Caption { get; init; }
    public string? Version { get; init; }
    public string? BuildNumber { get; init; }
    public string? DisplayVersion { get; init; }
    public string? Architecture { get; init; }
    public DateTimeOffset? InstallDateUtc { get; init; }
    public DateTimeOffset? LastBootUpTimeUtc { get; init; }
    public string? WindowsProductName { get; init; }
}

public sealed class SecurityInfo
{
    public string? FirmwareType { get; init; }
    public bool? SecureBootEnabled { get; init; }
    public bool? TpmPresent { get; init; }
    public string? TpmSpecVersion { get; init; }
    public string? TpmManufacturerId { get; init; }
    public string? TpmManufacturerVersion { get; init; }
}

public sealed class PerformanceInfo
{
    public string? Uptime { get; init; }
    public ulong? TotalVisibleMemoryBytes { get; init; }
    public ulong? FreePhysicalMemoryBytes { get; init; }
}

public sealed class DisplayInfo
{
    public List<ScreenInfo> Screens { get; init; } = [];
}

public sealed class ScreenInfo
{
    public string? DeviceName { get; init; }
    public string? Bounds { get; init; }
    public bool IsPrimary { get; init; }
}

public sealed class DiskDriveInfo
{
    public string? Model { get; init; }
    public string? InterfaceType { get; init; }
    public string? MediaType { get; init; }
    public ulong? SizeBytes { get; init; }
    public string? SerialNumber { get; init; }
    public uint? PowerOnHours { get; init; }
    public DateTimeOffset? FirstPowerOnUtcEstimated { get; init; }
    public uint? PowerCycleCount { get; init; }
}

public sealed class DiskSensorInfo
{
    public string? DiskName { get; init; }
    public string? SensorType { get; init; }
    public string? SensorName { get; init; }
    public double? Value { get; init; }
    public string? Unit { get; init; }
    public string? TextValue { get; init; }
}

public sealed class LogicalDiskInfo
{
    public string? DeviceId { get; init; }
    public string? VolumeName { get; init; }
    public string? FileSystem { get; init; }
    public ulong? SizeBytes { get; init; }
    public ulong? FreeBytes { get; init; }
}

public sealed class NetworkInfo
{
    public List<NetworkAdapterInfo> Adapters { get; init; } = [];
}

public sealed class NetworkAdapterInfo
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public string? MacAddress { get; init; }
    public List<string> IpAddresses { get; init; } = [];
}

public sealed class TemperatureInfo
{
    public List<TemperatureSensorInfo> Sensors { get; init; } = [];
}

public sealed class TemperatureSensorInfo
{
    public string? Name { get; init; }
    public double? Celsius { get; init; }
}

public sealed class EventLogInfo
{
    public List<EventItem> Items { get; init; } = [];
}

public sealed class EventItem
{
    public DateTimeOffset? TimeCreatedUtc { get; init; }
    public string? LogName { get; init; }
    public string? Level { get; init; }
    public int? LevelNumber { get; init; }
    public string? Provider { get; init; }
    public int? EventId { get; init; }
    public string? Message { get; init; }
}

[JsonSerializable(typeof(PcReport))]
public sealed partial class PcReportJsonContext : JsonSerializerContext
{
}
