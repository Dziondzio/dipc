using System.Management;

namespace DipcClient;

public static class SmartInfoReader
{
    public static (uint? powerOnHours, uint? powerCycles) TryGetPowerOnHoursAndCycles(string? diskPnpDeviceId)
    {
        if (string.IsNullOrWhiteSpace(diskPnpDeviceId))
        {
            return (null, null);
        }

        try
        {
            var pnp = diskPnpDeviceId.Trim();

            using var searcher = new ManagementObjectSearcher(@"root\wmi", "SELECT InstanceName, VendorSpecific FROM MSStorageDriver_FailurePredictData");
            foreach (var obj in searcher.Get().Cast<ManagementObject>())
            {
                var instanceName = obj["InstanceName"]?.ToString();
                if (string.IsNullOrWhiteSpace(instanceName))
                {
                    continue;
                }

                if (!InstanceMatchesDisk(instanceName, pnp))
                {
                    continue;
                }

                var vendorSpecific = obj["VendorSpecific"] as byte[];
                if (vendorSpecific is null || vendorSpecific.Length < 362)
                {
                    continue;
                }

                return ParseAtaSmart(vendorSpecific);
            }
        }
        catch
        {
        }

        return (null, null);
    }

    private static bool InstanceMatchesDisk(string instanceName, string diskPnpDeviceId)
    {
        var a = instanceName.Replace("_", "\\", StringComparison.OrdinalIgnoreCase);
        if (a.Contains(diskPnpDeviceId, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var b = instanceName.Replace("\\", "\\\\", StringComparison.OrdinalIgnoreCase);
        if (b.Contains(diskPnpDeviceId, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return instanceName.Contains(diskPnpDeviceId, StringComparison.OrdinalIgnoreCase);
    }

    private static (uint? powerOnHours, uint? powerCycles) ParseAtaSmart(byte[] vendorSpecific)
    {
        uint? poh = null;
        uint? pcc = null;

        const int attributesStart = 2;
        const int entrySize = 12;
        const int entryCount = 30;

        for (var i = 0; i < entryCount; i++)
        {
            var offset = attributesStart + (i * entrySize);
            if (offset + entrySize > vendorSpecific.Length)
            {
                break;
            }

            var id = vendorSpecific[offset];
            if (id == 0)
            {
                continue;
            }

            var raw0 = vendorSpecific[offset + 5];
            var raw1 = vendorSpecific[offset + 6];
            var raw2 = vendorSpecific[offset + 7];
            var raw3 = vendorSpecific[offset + 8];
            var raw4 = vendorSpecific[offset + 9];
            var raw5 = vendorSpecific[offset + 10];

            var raw = (ulong)raw0
                      | ((ulong)raw1 << 8)
                      | ((ulong)raw2 << 16)
                      | ((ulong)raw3 << 24)
                      | ((ulong)raw4 << 32)
                      | ((ulong)raw5 << 40);

            if (id == 9)
            {
                poh = raw > uint.MaxValue ? uint.MaxValue : (uint)raw;
            }
            else if (id == 12)
            {
                pcc = raw > uint.MaxValue ? uint.MaxValue : (uint)raw;
            }
        }

        return (poh, pcc);
    }
}

