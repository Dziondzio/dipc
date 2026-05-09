using System.Text.Json;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace DipcClient;

public static class ExcelReportFile
{
    public static void Export(string filePath, PcReport report)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        var json = JsonSerializer.Serialize(report, AppSettings.ReportJsonOptions);
        var summary = BuildSummary(report);

        using var doc = SpreadsheetDocument.Create(filePath, SpreadsheetDocumentType.Workbook);
        var workbookPart = doc.AddWorkbookPart();
        workbookPart.Workbook = new Workbook();

        var sheets = workbookPart.Workbook.AppendChild(new Sheets());

        AddSheet(workbookPart, sheets, "Podsumowanie", new[] { new[] { "Pole", "Wartość" } }.Concat(summary));
        AddSheet(workbookPart, sheets, "CPU", KeyValueTable(report.Cpu));
        AddSheet(workbookPart, sheets, "GPU", GpuTable(report.Gpus));
        AddSheet(workbookPart, sheets, "RAM", KeyValueTable(report.Ram));
        AddSheet(workbookPart, sheets, "RAM_Moduly", RamModulesTable(report.Ram.Modules));
        AddSheet(workbookPart, sheets, "Plyta", KeyValueTable(report.Motherboard));
        AddSheet(workbookPart, sheets, "BIOS", KeyValueTable(report.Bios, report.Security));
        AddSheet(workbookPart, sheets, "System", KeyValueTable(report.Os, report.Performance));
        AddSheet(workbookPart, sheets, "Dyski_Fizyczne", DiskDrivesTable(report.DiskDrives));
        AddSheet(workbookPart, sheets, "Dyski_Zdrowie", DiskSensorsTable(report.DiskSensors));
        AddSheet(workbookPart, sheets, "Dyski_Logiczne", LogicalDisksTable(report.LogicalDisks));
        AddSheet(workbookPart, sheets, "Siec", NetworkTable(report.Network.Adapters));
        AddSheet(workbookPart, sheets, "Ekrany", ScreensTable(report.Displays.Screens));
        AddSheet(workbookPart, sheets, "Temperatury", TemperaturesTable(report.Temperatures.Sensors));
        AddSheet(workbookPart, sheets, "Zdarzenia", EventsTable(report.Events.Items));
        AddSheet(workbookPart, sheets, "JSON", JsonSheet(json));

        workbookPart.Workbook.Save();
    }

    public static PcReport Import(string filePath)
    {
        using var doc = SpreadsheetDocument.Open(filePath, false);
        var workbookPart = doc.WorkbookPart ?? throw new InvalidOperationException("Brak WorkbookPart");
        var sheet = workbookPart.Workbook.Sheets?.Elements<Sheet>().FirstOrDefault(s => string.Equals(s.Name?.Value, "JSON", StringComparison.OrdinalIgnoreCase));
        if (sheet is null)
        {
            throw new InvalidOperationException("Brak arkusza JSON");
        }

        if (sheet.Id is null)
        {
            throw new InvalidOperationException("Błędny arkusz JSON");
        }

        var wsPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id!);
        var sheetData = wsPart.Worksheet.Elements<SheetData>().FirstOrDefault();
        if (sheetData is null)
        {
            throw new InvalidOperationException("Pusty arkusz JSON");
        }

        var lines = new List<string>();
        foreach (var row in sheetData.Elements<Row>())
        {
            var cell = row.Elements<Cell>().FirstOrDefault();
            if (cell is null)
            {
                continue;
            }

            var text = GetCellText(workbookPart, cell);
            if (!string.IsNullOrEmpty(text))
            {
                lines.Add(text);
            }
        }

        var json = string.Join("", lines);
        var report = JsonSerializer.Deserialize<PcReport>(json, AppSettings.ReportJsonOptions);
        if (report is null)
        {
            throw new InvalidOperationException("Nie udało się wczytać raportu");
        }

        return report;
    }

    private static IEnumerable<string?[]> BuildSummary(PcReport report)
    {
        return new[]
        {
            new[] { "Komputer", report.ComputerName },
            new[] { "Użytkownik", report.UserName },
            new[] { "Machine ID", report.MachineId },
            new[] { "Windows", $"{report.Os.WindowsProductName ?? report.Os.Caption} {report.Os.DisplayVersion}".Trim() },
            new[] { "CPU", report.Cpu.Name },
            new[] { "GPU", string.Join(", ", report.Gpus.Select(g => g.Name).Where(s => !string.IsNullOrWhiteSpace(s))) },
            new[] { "RAM", report.Ram.TotalBytes is null ? null : FormatBytes(report.Ram.TotalBytes.Value) },
            new[] { "Płyta", $"{report.Motherboard.Manufacturer} {report.Motherboard.Product}".Trim() },
            new[] { "Model", $"{report.Motherboard.SystemManufacturer} {report.Motherboard.SystemModel}".Trim() },
            new[] { "BIOS Mode", report.Security.FirmwareType },
            new[] { "Secure Boot", report.Security.SecureBootEnabled is null ? "Nieznane" : (report.Security.SecureBootEnabled.Value ? "Włączone" : "Wyłączone") },
            new[] { "TPM", report.Security.TpmPresent is null ? "Nieznane" : (report.Security.TpmPresent.Value ? "Jest" : "Brak") }
        };
    }

    private static IEnumerable<string?[]> KeyValueTable(CpuInfo cpu)
    {
        return new[]
        {
            new[] { "Nazwa", cpu.Name },
            new[] { "Rdzenie", cpu.Cores?.ToString() },
            new[] { "Wątki", cpu.LogicalProcessors?.ToString() },
            new[] { "Max MHz", cpu.MaxClockMHz?.ToString() },
            new[] { "Aktualne MHz", cpu.CurrentClockMHz?.ToString() }
        }.Prepend(new[] { "Pole", "Wartość" });
    }

    private static IEnumerable<string?[]> KeyValueTable(RamInfo ram)
    {
        return new[]
        {
            new[] { "Suma", ram.TotalBytes is null ? null : FormatBytes(ram.TotalBytes.Value) },
            new[] { "Moduły", ram.Modules.Count.ToString() }
        }.Prepend(new[] { "Pole", "Wartość" });
    }

    private static IEnumerable<string?[]> KeyValueTable(MotherboardInfo mb)
    {
        return new[]
        {
            new[] { "Płyta producent", mb.Manufacturer },
            new[] { "Płyta produkt", mb.Product },
            new[] { "Płyta serial", mb.SerialNumber },
            new[] { "System producent", mb.SystemManufacturer },
            new[] { "System model", mb.SystemModel }
        }.Prepend(new[] { "Pole", "Wartość" });
    }

    private static IEnumerable<string?[]> KeyValueTable(BiosInfo bios, SecurityInfo security)
    {
        return new[]
        {
            new[] { "Producent", bios.Manufacturer },
            new[] { "Wersja", bios.Version },
            new[] { "Data", bios.ReleaseDateUtc?.ToString("yyyy-MM-dd") },
            new[] { "Serial", bios.SerialNumber },
            new[] { "BIOS Mode", security.FirmwareType },
            new[] { "Secure Boot", security.SecureBootEnabled is null ? "Nieznane" : (security.SecureBootEnabled.Value ? "Włączone" : "Wyłączone") },
            new[] { "TPM", security.TpmPresent is null ? "Nieznane" : (security.TpmPresent.Value ? "Jest" : "Brak") },
            new[] { "TPM Spec", security.TpmSpecVersion },
            new[] { "TPM Producent ID", security.TpmManufacturerId },
            new[] { "TPM Wersja", security.TpmManufacturerVersion }
        }.Prepend(new[] { "Pole", "Wartość" });
    }

    private static IEnumerable<string?[]> KeyValueTable(OsInfo os, PerformanceInfo perf)
    {
        return new[]
        {
            new[] { "Windows", os.WindowsProductName ?? os.Caption },
            new[] { "DisplayVersion", os.DisplayVersion },
            new[] { "Wersja", os.Version },
            new[] { "Build", os.BuildNumber },
            new[] { "Architektura", os.Architecture },
            new[] { "Instalacja (UTC)", os.InstallDateUtc?.ToString("yyyy-MM-dd HH:mm:ss") },
            new[] { "Ostatni start (UTC)", os.LastBootUpTimeUtc?.ToString("yyyy-MM-dd HH:mm:ss") },
            new[] { "Uptime", perf.Uptime },
            new[] { "RAM całkowity", perf.TotalVisibleMemoryBytes is null ? null : FormatBytes(perf.TotalVisibleMemoryBytes.Value) },
            new[] { "RAM wolny", perf.FreePhysicalMemoryBytes is null ? null : FormatBytes(perf.FreePhysicalMemoryBytes.Value) }
        }.Prepend(new[] { "Pole", "Wartość" });
    }

    private static IEnumerable<string?[]> GpuTable(IEnumerable<GpuInfo> gpus)
    {
        var rows = new List<string?[]> { new[] { "Nazwa", "Sterownik", "Procesor", "Pamięć" } };
        foreach (var g in gpus)
        {
            rows.Add(new[]
            {
                g.Name,
                g.DriverVersion,
                g.VideoProcessor,
                g.AdapterRamBytes is null ? null : FormatBytes(g.AdapterRamBytes.Value)
            });
        }

        return rows;
    }

    private static IEnumerable<string?[]> RamModulesTable(IEnumerable<RamModuleInfo> modules)
    {
        var rows = new List<string?[]> { new[] { "Producent", "Model", "Pojemność", "MHz", "Serial" } };
        foreach (var m in modules)
        {
            rows.Add(new[]
            {
                m.Manufacturer,
                m.PartNumber,
                m.CapacityBytes is null ? null : FormatBytes(m.CapacityBytes.Value),
                m.SpeedMHz?.ToString(),
                m.SerialNumber
            });
        }

        return rows;
    }

    private static IEnumerable<string?[]> DiskDrivesTable(IEnumerable<DiskDriveInfo> disks)
    {
        var rows = new List<string?[]> { new[] { "Model", "Interfejs", "Typ", "Rozmiar", "Godziny", "Pierwsze uruchomienie (szac.)", "Cycles", "Serial" } };
        foreach (var d in disks)
        {
            rows.Add(new[]
            {
                d.Model,
                d.InterfaceType,
                d.MediaType,
                d.SizeBytes is null ? null : FormatBytes(d.SizeBytes.Value),
                d.PowerOnHours?.ToString(),
                d.FirstPowerOnUtcEstimated?.ToString("yyyy-MM-dd HH:mm:ss"),
                d.PowerCycleCount?.ToString(),
                d.SerialNumber
            });
        }

        return rows;
    }

    private static IEnumerable<string?[]> DiskSensorsTable(IEnumerable<DiskSensorInfo> sensors)
    {
        var rows = new List<string?[]> { new[] { "Dysk", "Typ", "Czujnik", "Wartość", "Jednostka", "Tekst" } };
        foreach (var s in sensors)
        {
            rows.Add(new[]
            {
                s.DiskName,
                s.SensorType,
                s.SensorName,
                s.Value?.ToString("0.###"),
                s.Unit,
                s.TextValue
            });
        }

        return rows;
    }

    private static IEnumerable<string?[]> LogicalDisksTable(IEnumerable<LogicalDiskInfo> disks)
    {
        var rows = new List<string?[]> { new[] { "Dysk", "Nazwa", "System", "Rozmiar", "Wolne" } };
        foreach (var d in disks)
        {
            rows.Add(new[]
            {
                d.DeviceId,
                d.VolumeName,
                d.FileSystem,
                d.SizeBytes is null ? null : FormatBytes(d.SizeBytes.Value),
                d.FreeBytes is null ? null : FormatBytes(d.FreeBytes.Value)
            });
        }

        return rows;
    }

    private static IEnumerable<string?[]> NetworkTable(IEnumerable<NetworkAdapterInfo> adapters)
    {
        var rows = new List<string?[]> { new[] { "Nazwa", "Opis", "MAC", "IP" } };
        foreach (var a in adapters)
        {
            rows.Add(new[]
            {
                a.Name,
                a.Description,
                a.MacAddress,
                string.Join(", ", a.IpAddresses)
            });
        }

        return rows;
    }

    private static IEnumerable<string?[]> ScreensTable(IEnumerable<ScreenInfo> screens)
    {
        var rows = new List<string?[]> { new[] { "Nazwa", "Rozdzielczość", "Primary" } };
        foreach (var s in screens)
        {
            rows.Add(new[]
            {
                s.DeviceName,
                s.Bounds,
                s.IsPrimary ? "Tak" : "Nie"
            });
        }

        return rows;
    }

    private static IEnumerable<string?[]> TemperaturesTable(IEnumerable<TemperatureSensorInfo> sensors)
    {
        var rows = new List<string?[]> { new[] { "Czujnik", "Celsius" } };
        foreach (var t in sensors)
        {
            rows.Add(new[]
            {
                t.Name,
                t.Celsius is null ? null : $"{t.Celsius:0.#}"
            });
        }

        return rows;
    }

    private static IEnumerable<string?[]> EventsTable(IEnumerable<EventItem> items)
    {
        var rows = new List<string?[]> { new[] { "CzasUTC", "Log", "Poziom", "Provider", "Id", "Wiadomość" } };
        foreach (var e in items)
        {
            rows.Add(new[]
            {
                e.TimeCreatedUtc?.ToString("yyyy-MM-dd HH:mm:ss"),
                e.LogName,
                e.Level,
                e.Provider,
                e.EventId?.ToString(),
                e.Message
            });
        }

        return rows;
    }

    private static IEnumerable<string?[]> JsonSheet(string json)
    {
        const int chunkSize = 30000;
        var rows = new List<string?[]>();
        for (var i = 0; i < json.Length; i += chunkSize)
        {
            var len = Math.Min(chunkSize, json.Length - i);
            rows.Add(new[] { json.Substring(i, len) });
        }

        return rows;
    }

    private static void AddSheet(WorkbookPart workbookPart, Sheets sheets, string name, IEnumerable<string?[]> rows)
    {
        var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
        var sheetData = new SheetData();
        worksheetPart.Worksheet = new Worksheet(sheetData);

        uint rowIndex = 1;
        foreach (var row in rows)
        {
            var r = new Row { RowIndex = rowIndex++ };
            foreach (var value in row)
            {
                r.AppendChild(new Cell
                {
                    DataType = CellValues.InlineString,
                    InlineString = new InlineString(new Text(value ?? ""))
                });
            }

            sheetData.AppendChild(r);
        }

        worksheetPart.Worksheet.Save();

        var sheetId = (uint)(sheets.Elements<Sheet>().Select(s => (int?)s.SheetId?.Value).Max() ?? 0) + 1;
        var relationshipId = workbookPart.GetIdOfPart(worksheetPart);
        sheets.Append(new Sheet { Id = relationshipId, SheetId = sheetId, Name = name });
    }

    private static string GetCellText(WorkbookPart workbookPart, Cell cell)
    {
        if (cell.DataType?.Value == CellValues.InlineString)
        {
            return cell.InlineString?.Text?.Text ?? cell.InlineString?.InnerText ?? "";
        }

        var value = cell.CellValue?.Text ?? cell.InnerText ?? "";
        if (cell.DataType?.Value == CellValues.SharedString)
        {
            if (int.TryParse(value, out var idx))
            {
                var sst = workbookPart.SharedStringTablePart?.SharedStringTable;
                var item = sst?.Elements<SharedStringItem>().ElementAtOrDefault(idx);
                return item?.InnerText ?? "";
            }
        }

        return value;
    }

    private static string FormatBytes(ulong bytes)
    {
        const double k = 1024.0;
        var b = (double)bytes;
        if (b < k) return $"{bytes} B";
        if (b < k * k) return $"{b / k:0.##} KB";
        if (b < k * k * k) return $"{b / (k * k):0.##} MB";
        if (b < k * k * k * k) return $"{b / (k * k * k):0.##} GB";
        return $"{b / (k * k * k * k):0.##} TB";
    }
}
