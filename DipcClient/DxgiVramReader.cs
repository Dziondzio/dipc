using System.Runtime.InteropServices;

namespace DipcClient;

public static class DxgiVramReader
{
    public static IReadOnlyList<(string description, ulong dedicatedVideoMemoryBytes)> GetAdapters()
    {
        var results = new List<(string description, ulong dedicatedVideoMemoryBytes)>();

        var iidFactory1 = typeof(IDXGIFactory1).GUID;
        var hr = CreateDXGIFactory1(ref iidFactory1, out var factoryPtr);
        if (hr != 0 || factoryPtr == IntPtr.Zero)
        {
            return results;
        }

        var factory = (IDXGIFactory1)Marshal.GetObjectForIUnknown(factoryPtr);
        try
        {
            uint index = 0;
            while (true)
            {
                hr = factory.EnumAdapters1(index, out var adapter);
                if (hr != 0 || adapter is null)
                {
                    break;
                }

                try
                {
                    adapter.GetDesc1(out var desc);
                    var description = desc.Description?.Trim() ?? "";
                    var dedicated = (ulong)desc.DedicatedVideoMemory;

                    if (!string.IsNullOrWhiteSpace(description))
                    {
                        results.Add((description, dedicated));
                    }
                }
                finally
                {
                    Marshal.ReleaseComObject(adapter);
                }

                index++;
            }
        }
        finally
        {
            Marshal.ReleaseComObject(factory);
            Marshal.Release(factoryPtr);
        }

        return results;
    }

    public static ulong? FindDedicatedMemoryForName(string? gpuName)
    {
        if (string.IsNullOrWhiteSpace(gpuName))
        {
            return null;
        }

        var adapters = GetAdapters();
        if (adapters.Count == 0)
        {
            return null;
        }

        var name = gpuName.Trim();

        ulong best = 0;
        foreach (var (description, bytes) in adapters)
        {
            if (bytes == 0)
            {
                continue;
            }

            if (string.Equals(description, name, StringComparison.OrdinalIgnoreCase)
                || description.Contains(name, StringComparison.OrdinalIgnoreCase)
                || name.Contains(description, StringComparison.OrdinalIgnoreCase))
            {
                if (bytes > best)
                {
                    best = bytes;
                }
            }
        }

        return best == 0 ? null : best;
    }

    [DllImport("dxgi.dll", CallingConvention = CallingConvention.StdCall)]
    private static extern int CreateDXGIFactory1(ref Guid riid, out IntPtr ppFactory);

    [ComImport]
    [Guid("770aae78-f26f-4dba-a829-253c83d1b387")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IDXGIFactory1
    {
        int SetPrivateData();
        int SetPrivateDataInterface();
        int GetPrivateData();
        int GetParent();
        int EnumAdapters(uint adapter, out IDXGIAdapter ppAdapter);
        int MakeWindowAssociation();
        int GetWindowAssociation();
        int CreateSwapChain();
        int CreateSoftwareAdapter();
        int EnumAdapters1(uint adapter, out IDXGIAdapter1 ppAdapter);
        bool IsCurrent();
    }

    [ComImport]
    [Guid("2411e7e1-12ac-4ccf-bd14-9798e8534dc0")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IDXGIAdapter
    {
        int SetPrivateData();
        int SetPrivateDataInterface();
        int GetPrivateData();
        int GetParent();
        int EnumOutputs();
        int GetDesc(out DXGI_ADAPTER_DESC desc);
        int CheckInterfaceSupport();
    }

    [ComImport]
    [Guid("29038f61-3839-4626-91fd-086879011a05")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IDXGIAdapter1 : IDXGIAdapter
    {
        new int SetPrivateData();
        new int SetPrivateDataInterface();
        new int GetPrivateData();
        new int GetParent();
        new int EnumOutputs();
        new int GetDesc(out DXGI_ADAPTER_DESC desc);
        new int CheckInterfaceSupport();
        int GetDesc1(out DXGI_ADAPTER_DESC1 desc);
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct DXGI_ADAPTER_DESC
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string Description;
        public uint VendorId;
        public uint DeviceId;
        public uint SubSysId;
        public uint Revision;
        public nuint DedicatedVideoMemory;
        public nuint DedicatedSystemMemory;
        public nuint SharedSystemMemory;
        public LUID AdapterLuid;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct DXGI_ADAPTER_DESC1
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string Description;
        public uint VendorId;
        public uint DeviceId;
        public uint SubSysId;
        public uint Revision;
        public nuint DedicatedVideoMemory;
        public nuint DedicatedSystemMemory;
        public nuint SharedSystemMemory;
        public LUID AdapterLuid;
        public uint Flags;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct LUID
    {
        public uint LowPart;
        public int HighPart;
    }
}

