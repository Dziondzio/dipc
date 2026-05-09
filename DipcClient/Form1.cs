namespace DipcClient;

public partial class Form1 : Form
{
    private PcReport? _report;
    private AppSettings _settings = new();
    private bool _loadingOptions;
    private List<EventItem> _eventItems = [];
    private readonly System.Windows.Forms.Timer _tempsTimer = new();
    private readonly Dictionary<string, (double min, double max)> _tempsMinMax = new(StringComparer.OrdinalIgnoreCase);
    private readonly bool _isAdmin = IsAdministrator();
    private TabPage? _tabDiskHealth;
    private DataGridView? _gridDiskSensors;
    private TabPage? _tabJunk;
    private TableLayoutPanel? _layoutJunk;
    private Button? _btnEmptyRecycle;
    private Button? _btnClearTemp;
    private Label? _lblJunkStatus;
    private TabPage? _tabCommands;
    private TableLayoutPanel? _layoutCommands;
    private DataGridView? _gridCommands;
    private Button? _btnCopyCommand;
    private TabPage? _tabPrograms;
    private TableLayoutPanel? _layoutPrograms;
    private LinkLabel? _linkCrystal;
    private LinkLabel? _linkCpuZ;
    private SplitContainer? _navSplit;
    private TreeView? _navTree;
    private bool _navSync;
    private TableLayoutPanel? _layoutDiskSensors;
    private Label? _lblDiskSensors;
    private Button? _btnRunCommand;
    private TextBox? _txtCommandDetails;
    private readonly List<string> _commandActionLines = [];
    private ContextMenuStrip? _cmdContextMenu;
    private ToolStripMenuItem? _cmdMenuRun;
    private ToolStripMenuItem? _cmdMenuRunAdmin;
    private ToolStripMenuItem? _cmdMenuCopy;
    private ContextMenuStrip? _mfgContextMenu;
    private ToolStripMenuItem? _mfgMenuOpen;
    private ToolStripMenuItem? _mfgMenuSearch;

    public Form1()
    {
        InitializeComponent();
    }
    private void Form1_Load(object? sender, EventArgs e)
    {
        _settings = SettingsStore.Load();

        SetupKeyValueGrid(gridSummary);
        SetupKeyValueGrid(gridCpu);
        SetupKeyValueGrid(gridRam);
        SetupKeyValueGrid(gridMotherboard);
        SetupKeyValueGrid(gridBios);
        SetupKeyValueGrid(gridSystem);

        SetupGrid(gridGpus);
        SetupGrid(gridRamModules);
        SetupGrid(gridDiskDrives);
        SetupGrid(gridLogicalDisks);
        SetupGrid(gridNetwork);
        SetupGrid(gridScreens);
        SetupGrid(gridEvents);
        SetupGrid(gridTemps);

        EnsureExtraTabs();
        if (_gridDiskSensors is not null) SetupGrid(_gridDiskSensors);
        if (_gridCommands is not null) SetupGrid(_gridCommands);

        EnsureManufacturerContextMenus();
        EnsureGroupedNavbar();
        ApplyV2Style();

        gridEvents.MultiSelect = true;
        gridEvents.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
        cmbEventLevel.SelectedIndex = 0;
        cmbEventLog.SelectedIndex = 0;

        _loadingOptions = true;
        chkCollectEvents.Checked = _settings.CollectEvents;
        chkCollectTemps.Checked = _settings.CollectTemperatures;
        chkCollectSmart.Checked = _settings.CollectSmartDiskInfo;
        nudEventDays.Value = Math.Clamp(_settings.EventLookbackDays, (int)nudEventDays.Minimum, (int)nudEventDays.Maximum);
        nudMaxEvents.Value = Math.Clamp(_settings.MaxEvents, (int)nudMaxEvents.Minimum, (int)nudMaxEvents.Maximum);
        _loadingOptions = false;

        LoadPowerPlans();
        LoadCommands();

        lblVersion.Text = $"Wersja: {GetAppVersion()}";
        statusVersion.Text = $"v{GetAppVersion()}";

        _tempsTimer.Interval = 5000;
        _tempsTimer.Tick += tempsTimer_Tick;
        tabs.SelectedIndexChanged += tabs_SelectedIndexChanged;

        var mode = MessageBox.Show(
            "Tryb startu:\n\nTAK = sprawdź aktualny komputer\nNIE = wczytaj raport z Excela",
            "DIPC",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (mode == DialogResult.No)
        {
            LoadFromExcel();
        }
        else
        {
            RefreshReport();
        }
    }

    private void ApplyV2Style()
    {
        Font = new Font("Segoe UI", 9F);
        BackColor = Color.FromArgb(245, 246, 250);
        ForeColor = Color.FromArgb(33, 37, 41);

        if (_navTree is null)
        {
            tabs.Alignment = TabAlignment.Left;
            tabs.Multiline = true;
            tabs.Appearance = TabAppearance.Normal;
            tabs.Padding = new Point(8, 8);
            tabs.SizeMode = TabSizeMode.Fixed;
            tabs.ItemSize = new Size(42, 150);
        }
        else
        {
            tabs.Alignment = TabAlignment.Top;
            tabs.Multiline = true;
            tabs.Appearance = TabAppearance.FlatButtons;
            tabs.SizeMode = TabSizeMode.Fixed;
            tabs.ItemSize = new Size(0, 1);
            tabs.Padding = new Point(0, 0);
        }

        foreach (var tp in tabs.TabPages.Cast<TabPage>())
        {
            tp.BackColor = Color.FromArgb(245, 246, 250);
            tp.Padding = new Padding(10);
        }

        StyleButton(btnRefresh, primary: true);
        StyleButton(btnExport, primary: true);
        StyleButton(btnImport, primary: false);
        StyleButton(btnEventCopySelected, primary: false);
        StyleButton(btnEventCopyAll, primary: false);
        StyleButton(btnEventClearLog, primary: false);
        StyleButton(btnCheckUpdates, primary: false);
        StyleButton(btnApplyPowerPlan, primary: false);

        lblStatus.Font = new Font(Font, FontStyle.Bold);

        statusStrip.BackColor = Color.FromArgb(245, 246, 250);
        statusStrip.ForeColor = ForeColor;

        ApplyGridStyle(gridSummary);
        ApplyGridStyle(gridCpu);
        ApplyGridStyle(gridGpus);
        ApplyGridStyle(gridRam);
        ApplyGridStyle(gridRamModules);
        ApplyGridStyle(gridMotherboard);
        ApplyGridStyle(gridDiskDrives);
        ApplyGridStyle(gridLogicalDisks);
        ApplyGridStyle(gridBios);
        ApplyGridStyle(gridSystem);
        ApplyGridStyle(gridNetwork);
        ApplyGridStyle(gridScreens);
        ApplyGridStyle(gridEvents);
        ApplyGridStyle(gridTemps);
        if (_gridDiskSensors is not null) ApplyGridStyle(_gridDiskSensors);
        if (_gridCommands is not null) ApplyGridStyle(_gridCommands);
        if (_btnEmptyRecycle is not null) StyleButton(_btnEmptyRecycle, primary: false);
        if (_btnClearTemp is not null) StyleButton(_btnClearTemp, primary: false);
        if (_btnCopyCommand is not null) StyleButton(_btnCopyCommand, primary: false);
        if (_btnRunCommand is not null) StyleButton(_btnRunCommand, primary: false);

        if (_navTree is not null)
        {
            _navTree.BackColor = Color.FromArgb(245, 246, 250);
            _navTree.ForeColor = ForeColor;
            _navTree.BorderStyle = BorderStyle.None;
            _navTree.FullRowSelect = true;
            _navTree.ShowLines = false;
            _navTree.ShowRootLines = false;
            _navTree.ShowPlusMinus = true;
            _navTree.HideSelection = false;
            _navTree.ItemHeight = 28;
        }
    }

    private void StyleButton(Button button, bool primary)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.Height = 34;
        button.Cursor = Cursors.Hand;

        if (primary)
        {
            button.BackColor = Color.FromArgb(13, 110, 253);
            button.ForeColor = Color.White;
        }
        else
        {
            button.BackColor = Color.FromArgb(233, 236, 239);
            button.ForeColor = Color.FromArgb(33, 37, 41);
        }
    }

    private void ApplyGridStyle(DataGridView grid)
    {
        grid.BackgroundColor = Color.White;
        grid.BorderStyle = BorderStyle.None;
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        grid.GridColor = Color.FromArgb(222, 226, 230);
        grid.EnableHeadersVisualStyles = false;

        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 250);
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(33, 37, 41);
        grid.ColumnHeadersDefaultCellStyle.Font = new Font(Font, FontStyle.Bold);
        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        grid.ColumnHeadersHeight = 36;
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(207, 226, 255);
        grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(33, 37, 41);
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 250);
        grid.RowTemplate.Height = 28;
    }

    private void LoadPowerPlans()
    {
        cmbPowerPlan.Items.Clear();
        cmbPowerPlan.Items.Add(new PowerPlanItem("Najlepsza wydajność", "8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c"));
        cmbPowerPlan.Items.Add(new PowerPlanItem("Zrównoważony", "381b4222-f694-41f0-9685-ff5bb260df2e"));
        cmbPowerPlan.Items.Add(new PowerPlanItem("Najbardziej oszczędny / cichy", "a1841308-3541-4fab-bc81-f71556f20b4a"));

        var active = TryGetActivePowerSchemeGuid();
        if (active is not null)
        {
            for (var i = 0; i < cmbPowerPlan.Items.Count; i++)
            {
                if (cmbPowerPlan.Items[i] is PowerPlanItem item && string.Equals(item.Guid, active, StringComparison.OrdinalIgnoreCase))
                {
                    cmbPowerPlan.SelectedIndex = i;
                    return;
                }
            }
        }

        cmbPowerPlan.SelectedIndex = 0;
    }

    private string? TryGetActivePowerSchemeGuid()
    {
        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo("powercfg", "/getactivescheme")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var p = System.Diagnostics.Process.Start(psi);
            if (p is null)
            {
                return null;
            }

            var output = p.StandardOutput.ReadToEnd();
            p.WaitForExit(2000);

            var m = System.Text.RegularExpressions.Regex.Match(output, @"([0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12})");
            return m.Success ? m.Groups[1].Value : null;
        }
        catch
        {
            return null;
        }
    }

    private void tabs_SelectedIndexChanged(object? sender, EventArgs e)
    {
        var isTempsTab = ReferenceEquals(tabs.SelectedTab, tabTemps);
        if (isTempsTab && _settings.CollectTemperatures)
        {
            _tempsTimer.Start();
            RefreshTemperaturesLive();
        }
        else
        {
            _tempsTimer.Stop();
        }
    }

    private void tempsTimer_Tick(object? sender, EventArgs e)
    {
        if (!_settings.CollectTemperatures)
        {
            _tempsTimer.Stop();
            return;
        }

        if (!ReferenceEquals(tabs.SelectedTab, tabTemps))
        {
            return;
        }

        RefreshTemperaturesLive();
    }

    private void RefreshTemperaturesLive()
    {
        try
        {
            var sensors = TemperatureCollector.GetTemperatures();
            foreach (var s in sensors)
            {
                if (s.Celsius is null || string.IsNullOrWhiteSpace(s.Name))
                {
                    continue;
                }

                if (_tempsMinMax.TryGetValue(s.Name, out var mm))
                {
                    _tempsMinMax[s.Name] = (Math.Min(mm.min, s.Celsius.Value), Math.Max(mm.max, s.Celsius.Value));
                }
                else
                {
                    _tempsMinMax[s.Name] = (s.Celsius.Value, s.Celsius.Value);
                }
            }

            gridTemps.DataSource = sensors.Select(s =>
            {
                _tempsMinMax.TryGetValue(s.Name ?? "", out var mm);
                return new
                {
                    Czujnik = s.Name,
                    Celsius = s.Celsius is null ? null : $"{s.Celsius:0.#}",
                    Min = _tempsMinMax.ContainsKey(s.Name ?? "") ? $"{mm.min:0.#}" : null,
                    Max = _tempsMinMax.ContainsKey(s.Name ?? "") ? $"{mm.max:0.#}" : null
                };
            }).ToList();
        }
        catch
        {
        }
    }

    private void btnRefresh_Click(object? sender, EventArgs e)
    {
        RefreshReport();
    }

    private void btnExport_Click(object? sender, EventArgs e)
    {
        try
        {
            if (_report is null)
            {
                RefreshReport();
            }

            if (_report is null)
            {
                SetStatus("Brak danych do eksportu");
                return;
            }

            using var dialog = new SaveFileDialog
            {
                Filter = "Excel (*.xlsx)|*.xlsx",
                DefaultExt = "xlsx",
                AddExtension = true,
                FileName = $"DIPC_{_report.ComputerName}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            SetStatus("Eksport do Excela...");
            btnExport.Enabled = false;
            btnRefresh.Enabled = false;
            btnImport.Enabled = false;

            ExcelReportFile.Export(dialog.FileName, _report);
            SetStatus("Zapisano Excel");
        }
        catch (Exception ex)
        {
            SetStatus($"Błąd: {ex.Message}");
        }
        finally
        {
            btnExport.Enabled = true;
            btnRefresh.Enabled = true;
            btnImport.Enabled = true;
        }
    }

    private void btnImport_Click(object? sender, EventArgs e)
    {
        LoadFromExcel();
    }

    private void LoadFromExcel()
    {
        try
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "Excel (*.xlsx)|*.xlsx",
                Multiselect = false
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            SetStatus("Wczytywanie z Excela...");
            btnExport.Enabled = false;
            btnRefresh.Enabled = false;
            btnImport.Enabled = false;

            _report = ExcelReportFile.Import(dialog.FileName);
            RenderReport(_report);
            SetStatus("Wczytano Excel");
        }
        catch (Exception ex)
        {
            SetStatus($"Błąd: {ex.Message}");
        }
        finally
        {
            btnExport.Enabled = true;
            btnRefresh.Enabled = true;
            btnImport.Enabled = true;
        }
    }

    private void RefreshReport()
    {
        SetStatus("Zbieranie danych...");
        try
        {
            var options = new CollectOptions
            {
                CollectEvents = _settings.CollectEvents,
                EventLookbackDays = _settings.EventLookbackDays,
                MaxEvents = _settings.MaxEvents,
                CollectTemperatures = _settings.CollectTemperatures,
                CollectSmartDiskInfo = _settings.CollectSmartDiskInfo
            };

            _report = PcInfoCollector.Collect(options);
            RenderReport(_report);
            SetStatus($"Zebrano: {_report.CollectedAtUtc:yyyy-MM-dd HH:mm:ss} UTC");
        }
        catch (Exception ex)
        {
            SetStatus($"Błąd: {ex.Message}");
        }
    }

    private void RenderReport(PcReport report)
    {
        SetKeyValueRows(gridSummary, new (string key, string? value)[]
        {
            ("Komputer", report.ComputerName),
            ("Użytkownik", report.UserName),
            ("Machine ID", report.MachineId),
            ("Windows", $"{report.Os.WindowsProductName ?? report.Os.Caption} {report.Os.DisplayVersion}".Trim()),
            ("CPU", report.Cpu.Name),
            ("GPU", string.Join(", ", report.Gpus.Select(g => g.Name).Where(s => !string.IsNullOrWhiteSpace(s)))),
            ("RAM", report.Ram.TotalBytes is null ? null : FormatBytes(report.Ram.TotalBytes.Value)),
            ("Płyta", $"{report.Motherboard.Manufacturer} {report.Motherboard.Product}".Trim()),
            ("Model", $"{report.Motherboard.SystemManufacturer} {report.Motherboard.SystemModel}".Trim()),
        });

        SetKeyValueRows(gridCpu, new (string key, string? value)[]
        {
            ("Nazwa", report.Cpu.Name),
            ("Rdzenie", report.Cpu.Cores?.ToString()),
            ("Wątki", report.Cpu.LogicalProcessors?.ToString()),
            ("Max MHz", report.Cpu.MaxClockMHz?.ToString()),
            ("Aktualne MHz", report.Cpu.CurrentClockMHz?.ToString())
        });

        gridGpus.DataSource = report.Gpus.Select(g => new
        {
            Nazwa = g.Name,
            Sterownik = g.DriverVersion,
            Procesor = g.VideoProcessor,
            Pamiec = g.AdapterRamBytes is null ? null : FormatBytes(g.AdapterRamBytes.Value)
        }).ToList();

        SetKeyValueRows(gridRam, new (string key, string? value)[]
        {
            ("Suma", report.Ram.TotalBytes is null ? null : FormatBytes(report.Ram.TotalBytes.Value)),
            ("Moduły", report.Ram.Modules.Count.ToString())
        });

        gridRamModules.DataSource = report.Ram.Modules.Select(m => new
        {
            Producent = m.Manufacturer,
            Model = m.PartNumber,
            Pojemnosc = m.CapacityBytes is null ? null : FormatBytes(m.CapacityBytes.Value),
            MHz = m.SpeedMHz,
            Serial = m.SerialNumber
        }).ToList();

        SetKeyValueRows(gridMotherboard, new (string key, string? value)[]
        {
            ("Płyta producent", report.Motherboard.Manufacturer),
            ("Płyta produkt", report.Motherboard.Product),
            ("Płyta serial", report.Motherboard.SerialNumber),
            ("System producent", report.Motherboard.SystemManufacturer),
            ("System model", report.Motherboard.SystemModel)
        });

        gridDiskDrives.DataSource = report.DiskDrives.Select(d => new
        {
            Model = d.Model,
            Interfejs = d.InterfaceType,
            Typ = d.MediaType,
            Rozmiar = d.SizeBytes is null ? null : FormatBytes(d.SizeBytes.Value),
            Godziny = FormatSmartNumber(d.PowerOnHours),
            PierwszeUruchomienie = d.FirstPowerOnUtcEstimated?.ToString("yyyy-MM-dd HH:mm:ss") ?? FormatSmartMissing(),
            Cycles = FormatSmartNumber(d.PowerCycleCount),
            Serial = d.SerialNumber
        }).ToList();

        gridLogicalDisks.DataSource = report.LogicalDisks.Select(d => new
        {
            Dysk = d.DeviceId,
            Nazwa = d.VolumeName,
            System = d.FileSystem,
            Rozmiar = d.SizeBytes is null ? null : FormatBytes(d.SizeBytes.Value),
            Wolne = d.FreeBytes is null ? null : FormatBytes(d.FreeBytes.Value)
        }).ToList();

        if (_gridDiskSensors is not null)
        {
            var sensors = report.DiskSensors ?? [];
            _gridDiskSensors.DataSource = sensors.Select(s => new
            {
                Dysk = s.DiskName,
                Typ = s.SensorType,
                Czujnik = s.SensorName,
                Wartosc = s.Value is null ? null : $"{s.Value:0.###}",
                Jednostka = s.Unit,
                Tekst = s.TextValue
            }).ToList();

            if (_lblDiskSensors is not null)
            {
                if (!_settings.CollectSmartDiskInfo)
                {
                    _lblDiskSensors.Text = "Włącz w Opcje: „Zbieraj SMART”";
                }
                else if (sensors.Count == 0)
                {
                    _lblDiskSensors.Text = "Brak danych. Spróbuj uruchomić jako administrator lub sprawdź czy dysk ma SMART i czujniki.";
                }
                else
                {
                    _lblDiskSensors.Text = $"Dane: {sensors.Count} wierszy";
                }
            }
        }

        SetKeyValueRows(gridBios, new (string key, string? value)[]
        {
            ("Producent", report.Bios.Manufacturer),
            ("Wersja", report.Bios.Version),
            ("Data", report.Bios.ReleaseDateUtc?.ToString("yyyy-MM-dd") ),
            ("Serial", report.Bios.SerialNumber),
            ("BIOS Mode", report.Security.FirmwareType),
            ("Secure Boot", report.Security.SecureBootEnabled is null ? "Nieznane" : (report.Security.SecureBootEnabled.Value ? "Włączone" : "Wyłączone")),
            ("TPM", report.Security.TpmPresent is null ? "Nieznane" : (report.Security.TpmPresent.Value ? "Jest" : "Brak")),
            ("TPM Spec", report.Security.TpmSpecVersion),
            ("TPM Producent ID", report.Security.TpmManufacturerId),
            ("TPM Wersja", report.Security.TpmManufacturerVersion),
        });

        SetKeyValueRows(gridSystem, new (string key, string? value)[]
        {
            ("Windows", report.Os.WindowsProductName ?? report.Os.Caption),
            ("Wersja", report.Os.Version),
            ("Build", report.Os.BuildNumber),
            ("DisplayVersion", report.Os.DisplayVersion),
            ("Architektura", report.Os.Architecture),
            ("Instalacja (UTC)", report.Os.InstallDateUtc?.ToString("yyyy-MM-dd HH:mm:ss")),
            ("Ostatni start (UTC)", report.Os.LastBootUpTimeUtc?.ToString("yyyy-MM-dd HH:mm:ss")),
            ("Uptime", report.Performance.Uptime),
            ("RAM całkowity", report.Performance.TotalVisibleMemoryBytes is null ? null : FormatBytes(report.Performance.TotalVisibleMemoryBytes.Value)),
            ("RAM wolny", report.Performance.FreePhysicalMemoryBytes is null ? null : FormatBytes(report.Performance.FreePhysicalMemoryBytes.Value)),
        });

        gridNetwork.DataSource = report.Network.Adapters.Select(a => new
        {
            Nazwa = a.Name,
            Opis = a.Description,
            MAC = a.MacAddress,
            IP = string.Join(", ", a.IpAddresses)
        }).ToList();

        gridScreens.DataSource = report.Displays.Screens.Select(s => new
        {
            Nazwa = s.DeviceName,
            Rozdzielczosc = s.Bounds,
            Primary = s.IsPrimary ? "Tak" : "Nie"
        }).ToList();

        _eventItems = report.Events.Items ?? [];
        ApplyEventFilter();

        gridTemps.DataSource = report.Temperatures.Sensors.Select(t => new
        {
            Czujnik = t.Name,
            Celsius = t.Celsius is null ? null : $"{t.Celsius:0.#}"
        }).ToList();
    }

    private void linkAuthor_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
    {
        OpenUrl("https://dziondzio.xyz/");
    }

    private void statusAuthor_Click(object? sender, EventArgs e)
    {
        OpenUrl("https://dziondzio.xyz/");
    }

    private async void btnCheckUpdates_Click(object? sender, EventArgs e)
    {
        try
        {
            btnCheckUpdates.Enabled = false;
            SetStatus("Sprawdzanie aktualizacji...");
            var result = await UpdateChecker.CheckAsync(CancellationToken.None);
            if (result is null)
            {
                SetStatus("Brak informacji o aktualizacji");
                return;
            }

            if (result.IsUpdateAvailable)
            {
                SetStatus($"Nowa wersja: {result.LatestVersion}");
                var notes = (result.Notes ?? "").Trim();
                var msg =
                    $"Dostępna jest nowa wersja:\n\n" +
                    $"Aktualna: {result.CurrentVersion}\n" +
                    $"Najnowsza: {result.LatestVersion}\n" +
                    (string.IsNullOrWhiteSpace(notes) ? "" : $"\nZmiany:\n{notes}\n") +
                    "\nChcesz zaktualizować teraz?";

                var ask = MessageBox.Show(this, msg, "Aktualizacja", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (ask != DialogResult.Yes)
                {
                    return;
                }

                var portableMode = IsPortableMode();
                var downloadUrl = portableMode
                    ? (result.PortableUrl ?? result.InstallerUrl)
                    : (result.InstallerUrl ?? result.PortableUrl);
                var expectedSha = portableMode ? result.PortableSha256 : result.InstallerSha256;

                if (string.IsNullOrWhiteSpace(downloadUrl))
                {
                    if (!string.IsNullOrWhiteSpace(result.ReleasePageUrl))
                    {
                        OpenUrl(result.ReleasePageUrl);
                    }
                    else
                    {
                        MessageBox.Show(this, "Brak pliku do pobrania w GitHub Releases", "Aktualizacja", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    return;
                }

                if (portableMode)
                {
                    var updated = await TrySelfUpdatePortableAsync(downloadUrl, expectedSha, result.LatestVersion, CancellationToken.None);
                    if (!updated)
                    {
                        OpenUrl(downloadUrl);
                    }
                }
                else
                {
                    var started = await TryRunInstallerUpdateAsync(downloadUrl, expectedSha, result.LatestVersion, CancellationToken.None);
                    if (!started)
                    {
                        OpenUrl(downloadUrl);
                    }
                }
            }
            else
            {
                SetStatus("Masz najnowszą wersję");
            }
        }
        catch (Exception ex)
        {
            SetStatus($"Błąd: {ex.Message}");
        }
        finally
        {
            btnCheckUpdates.Enabled = true;
        }
    }

    private static bool IsPortableMode()
    {
        try
        {
            var baseDir = AppContext.BaseDirectory;
            var portableFlag = Path.Combine(baseDir, "portable.flag");
            var portableEnv = Environment.GetEnvironmentVariable("DIPC_PORTABLE");
            if (File.Exists(portableFlag) || string.Equals(portableEnv, "1", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

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

    private async Task<bool> TrySelfUpdatePortableAsync(string downloadUrl, string? expectedSha256, string latestVersion, CancellationToken cancellationToken)
    {
        try
        {
            var currentExe = Environment.ProcessPath;
            if (string.IsNullOrWhiteSpace(currentExe) || !File.Exists(currentExe))
            {
                return false;
            }

            var targetDir = Path.GetDirectoryName(currentExe);
            if (string.IsNullOrWhiteSpace(targetDir))
            {
                return false;
            }

            var tmpExe = Path.Combine(Path.GetTempPath(), $"DIPC_Update_{latestVersion}.exe");
            SetStatus("Pobieranie aktualizacji...");

            using (var http = new HttpClient { Timeout = TimeSpan.FromMinutes(3) })
            using (var response = await http.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
            {
                response.EnsureSuccessStatusCode();
                await using var src = await response.Content.ReadAsStreamAsync(cancellationToken);
                await using var dst = new FileStream(tmpExe, FileMode.Create, FileAccess.Write, FileShare.None);
                await src.CopyToAsync(dst, cancellationToken);
            }

            var expectedSha = (expectedSha256 ?? "").Trim();
            if (!string.IsNullOrWhiteSpace(expectedSha))
            {
                SetStatus("Sprawdzanie pliku...");
                var actualSha = ComputeSha256Hex(tmpExe);
                if (!string.Equals(actualSha, expectedSha, StringComparison.OrdinalIgnoreCase))
                {
                    try { File.Delete(tmpExe); } catch { }
                    MessageBox.Show(this, "SHA256 nie pasuje. Aktualizacja anulowana.", "Aktualizacja", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }

            if (!CanWriteToDirectory(targetDir))
            {
                MessageBox.Show(this, "Brak uprawnień do nadpisania pliku EXE. Otwieram link do pobrania.", "Aktualizacja", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            var pid = System.Diagnostics.Process.GetCurrentProcess().Id;
            var tmpCmd = Path.Combine(Path.GetTempPath(), $"DIPC_Update_{latestVersion}.cmd");
            var script = BuildUpdateScript(tmpExe, currentExe, pid);
            File.WriteAllText(tmpCmd, script, System.Text.Encoding.ASCII);

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("cmd.exe", $"/c \"{tmpCmd}\"")
            {
                UseShellExecute = true,
                WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
            });

            SetStatus("Aktualizacja uruchomiona...");
            BeginInvoke(new Action(() => Close()));
            return true;
        }
        catch (Exception ex)
        {
            SetStatus($"Błąd: {ex.Message}");
            return false;
        }
    }

    private async Task<bool> TryRunInstallerUpdateAsync(string downloadUrl, string? expectedSha256, string latestVersion, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(downloadUrl) || !downloadUrl.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var tmpInstaller = Path.Combine(Path.GetTempPath(), $"DIPC_Installer_{latestVersion}.exe");
            SetStatus("Pobieranie instalatora...");

            using (var http = new HttpClient { Timeout = TimeSpan.FromMinutes(5) })
            using (var response = await http.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
            {
                response.EnsureSuccessStatusCode();
                await using var src = await response.Content.ReadAsStreamAsync(cancellationToken);
                await using var dst = new FileStream(tmpInstaller, FileMode.Create, FileAccess.Write, FileShare.None);
                await src.CopyToAsync(dst, cancellationToken);
            }

            var expectedSha = (expectedSha256 ?? "").Trim();
            if (!string.IsNullOrWhiteSpace(expectedSha))
            {
                SetStatus("Sprawdzanie pliku...");
                var actualSha = ComputeSha256Hex(tmpInstaller);
                if (!string.Equals(actualSha, expectedSha, StringComparison.OrdinalIgnoreCase))
                {
                    try { File.Delete(tmpInstaller); } catch { }
                    MessageBox.Show(this, "SHA256 nie pasuje. Aktualizacja anulowana.", "Aktualizacja", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }

            var psi = new System.Diagnostics.ProcessStartInfo(tmpInstaller)
            {
                UseShellExecute = true
            };

            try
            {
                psi.Verb = "runas";
                System.Diagnostics.Process.Start(psi);
            }
            catch
            {
                psi.Verb = "";
                System.Diagnostics.Process.Start(psi);
            }

            SetStatus("Uruchomiono instalator aktualizacji");
            BeginInvoke(new Action(() => Close()));
            return true;
        }
        catch (Exception ex)
        {
            SetStatus($"Błąd: {ex.Message}");
            return false;
        }
    }

    private static bool CanWriteToDirectory(string directory)
    {
        try
        {
            Directory.CreateDirectory(directory);
            var test = Path.Combine(directory, $"._dipc_write_{Guid.NewGuid():N}.tmp");
            File.WriteAllText(test, "x");
            File.Delete(test);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string ComputeSha256Hex(string filePath)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        using var fs = File.OpenRead(filePath);
        var hash = sha.ComputeHash(fs);
        return Convert.ToHexString(hash);
    }

    private static string BuildUpdateScript(string tmpExe, string targetExe, int pid)
    {
        var t = targetExe.Replace("\"", "\"\"");
        var s = tmpExe.Replace("\"", "\"\"");
        return
            "@echo off\r\n" +
            "setlocal\r\n" +
            $"set \"PID={pid}\"\r\n" +
            ":wait\r\n" +
            "tasklist /FI \"PID eq %PID%\" | find \"%PID%\" >nul\r\n" +
            "if %errorlevel%==0 (\r\n" +
            "  timeout /t 1 /nobreak >nul\r\n" +
            "  goto wait\r\n" +
            ")\r\n" +
            $"copy /Y \"{s}\" \"{t}\" >nul\r\n" +
            $"start \"\" \"{t}\"\r\n" +
            $"del /f /q \"{s}\" >nul 2>nul\r\n" +
            "del /f /q \"%~f0\" >nul 2>nul\r\n";
    }

    private void txtEventFilter_TextChanged(object? sender, EventArgs e)
    {
        ApplyEventFilter();
    }

    private void cmbEventLevel_SelectedIndexChanged(object? sender, EventArgs e)
    {
        ApplyEventFilter();
    }

    private void cmbEventLog_SelectedIndexChanged(object? sender, EventArgs e)
    {
        ApplyEventFilter();
    }

    private void btnEventCopySelected_Click(object? sender, EventArgs e)
    {
        CopyEventsToClipboard(onlySelected: true);
    }

    private void btnEventCopyAll_Click(object? sender, EventArgs e)
    {
        CopyEventsToClipboard(onlySelected: false);
    }

    private void btnEventClearLog_Click(object? sender, EventArgs e)
    {
        try
        {
            var logs = cmbEventLog.SelectedIndex switch
            {
                1 => new[] { "System" },
                2 => new[] { "Application" },
                _ => new[] { "System", "Application" }
            };

            var title = "Wyczyść dziennik zdarzeń";
            var question = $"To usunie wszystkie wpisy z logu(ów): {string.Join(", ", logs)}.\n\nOperacja nieodwracalna.\n\nKontynuować?";

            if (MessageBox.Show(this, question, title, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return;
            }

            if (!_isAdmin)
            {
                MessageBox.Show(this, "Ta operacja wymaga uruchomienia programu jako administrator.", title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            btnEventClearLog.Enabled = false;
            SetStatus("Czyszczenie dziennika...");

            foreach (var log in logs)
            {
                ClearEventLog(log);
            }

            ReloadEventsAfterClear();
            SetStatus("Wyczyszczono dziennik");
            MessageBox.Show(this, $"Wyczyszczono logi: {string.Join(", ", logs)}", title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            SetStatus($"Błąd: {ex.Message}");
        }
        finally
        {
            btnEventClearLog.Enabled = true;
        }
    }

    private void chkCollectEvents_CheckedChanged(object? sender, EventArgs e)
    {
        if (_loadingOptions)
        {
            return;
        }

        _settings.CollectEvents = chkCollectEvents.Checked;
        SettingsStore.Save(_settings);
    }

    private void chkCollectTemps_CheckedChanged(object? sender, EventArgs e)
    {
        if (_loadingOptions)
        {
            return;
        }

        _settings.CollectTemperatures = chkCollectTemps.Checked;
        SettingsStore.Save(_settings);
    }

    private void chkCollectSmart_CheckedChanged(object? sender, EventArgs e)
    {
        if (_loadingOptions)
        {
            return;
        }

        _settings.CollectSmartDiskInfo = chkCollectSmart.Checked;
        SettingsStore.Save(_settings);
    }

    private void nudEventDays_ValueChanged(object? sender, EventArgs e)
    {
        if (_loadingOptions)
        {
            return;
        }

        _settings.EventLookbackDays = (int)nudEventDays.Value;
        SettingsStore.Save(_settings);
    }

    private void nudMaxEvents_ValueChanged(object? sender, EventArgs e)
    {
        if (_loadingOptions)
        {
            return;
        }

        _settings.MaxEvents = (int)nudMaxEvents.Value;
        SettingsStore.Save(_settings);
    }

    private void btnApplyPowerPlan_Click(object? sender, EventArgs e)
    {
        try
        {
            if (cmbPowerPlan.SelectedItem is not PowerPlanItem item)
            {
                SetStatus("Wybierz plan zasilania");
                return;
            }

            var psi = new System.Diagnostics.ProcessStartInfo("powercfg", $"/setactive {item.Guid}")
            {
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var p = System.Diagnostics.Process.Start(psi);
            p?.WaitForExit(3000);

            SetStatus($"Ustawiono: {item.Name}");
        }
        catch (Exception ex)
        {
            SetStatus($"Błąd: {ex.Message}");
        }
    }

    private void ApplyEventFilter()
    {
        var q = txtEventFilter.Text.Trim();
        var mode = cmbEventLevel.SelectedIndex;
        var log = cmbEventLog.SelectedIndex;

        IEnumerable<EventItem> filtered = _eventItems;

        filtered = log switch
        {
            1 => filtered.Where(e => string.Equals(e.LogName, "System", StringComparison.OrdinalIgnoreCase)),
            2 => filtered.Where(e => string.Equals(e.LogName, "Application", StringComparison.OrdinalIgnoreCase)),
            _ => filtered
        };

        filtered = mode switch
        {
            1 => filtered.Where(IsCritical),
            2 => filtered.Where(IsError),
            3 => filtered.Where(IsWarning),
            4 => filtered,
            _ => filtered.Where(e => IsCritical(e) || IsError(e))
        };

        if (q.Length > 0)
        {
            filtered = filtered.Where(e =>
                Contains(e.LogName, q) ||
                Contains(e.Level, q) ||
                Contains(e.Provider, q) ||
                (e.EventId?.ToString().Contains(q, StringComparison.OrdinalIgnoreCase) ?? false) ||
                Contains(e.Message, q));
        }

        gridEvents.DataSource = filtered.Select(e => new EventRow
        {
            CzasUTC = e.TimeCreatedUtc?.ToString("yyyy-MM-dd HH:mm:ss"),
            Log = e.LogName,
            Poziom = GetLevelName(e),
            Provider = e.Provider,
            Id = e.EventId,
            Wiadomosc = e.Message
        }).ToList();
    }

    private static void ClearEventLog(string logName)
    {
        var psi = new System.Diagnostics.ProcessStartInfo("wevtutil", $"cl \"{logName}\"")
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using var p = System.Diagnostics.Process.Start(psi);
        if (p is null)
        {
            throw new InvalidOperationException("Nie udało się uruchomić wevtutil");
        }

        var stdout = p.StandardOutput.ReadToEnd();
        var stderr = p.StandardError.ReadToEnd();
        p.WaitForExit(15000);

        if (p.ExitCode != 0)
        {
            var msg = string.IsNullOrWhiteSpace(stderr) ? stdout : stderr;
            msg = (msg ?? "").Trim();
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(msg) ? $"wevtutil zakończył się kodem {p.ExitCode}" : msg);
        }
    }

    private void ReloadEventsAfterClear()
    {
        if (!_settings.CollectEvents)
        {
            _eventItems = [];
            _report?.Events.Items.Clear();
            ApplyEventFilter();
            return;
        }

        var items = WindowsEventLogCollector.GetCriticalErrorWarningEvents(TimeSpan.FromDays(_settings.EventLookbackDays), _settings.MaxEvents);
        _eventItems = items;

        if (_report is not null)
        {
            _report.Events.Items.Clear();
            _report.Events.Items.AddRange(items);
        }

        ApplyEventFilter();
    }

    private void CopyEventsToClipboard(bool onlySelected)
    {
        try
        {
            var rows = new List<DataGridViewRow>();
            if (onlySelected && gridEvents.SelectedRows.Count > 0)
            {
                rows.AddRange(gridEvents.SelectedRows.Cast<DataGridViewRow>().OrderBy(r => r.Index));
            }
            else
            {
                rows.AddRange(gridEvents.Rows.Cast<DataGridViewRow>());
            }

            if (rows.Count == 0)
            {
                SetStatus("Brak zdarzeń do skopiowania");
                return;
            }

            var headers = gridEvents.Columns.Cast<DataGridViewColumn>().Select(c => c.HeaderText);
            var lines = new List<string> { string.Join("\t", headers) };

            foreach (var r in rows)
            {
                if (r.IsNewRow)
                {
                    continue;
                }

                var values = gridEvents.Columns
                    .Cast<DataGridViewColumn>()
                    .Select(c => (r.Cells[c.Index].Value?.ToString() ?? "").Replace("\r", " ").Replace("\n", " "));

                lines.Add(string.Join("\t", values));
            }

            Clipboard.SetText(string.Join(Environment.NewLine, lines));
            SetStatus("Skopiowano do schowka");
        }
        catch (Exception ex)
        {
            SetStatus($"Błąd: {ex.Message}");
        }
    }

    private static bool Contains(string? haystack, string needle)
    {
        return !string.IsNullOrWhiteSpace(haystack) && haystack.Contains(needle, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsCritical(EventItem e)
    {
        return e.LevelNumber == 1 || (e.LevelNumber is null && (e.Level ?? "").Contains("Critical", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsError(EventItem e)
    {
        return e.LevelNumber == 2 || (e.LevelNumber is null && (e.Level ?? "").Contains("Error", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsWarning(EventItem e)
    {
        return e.LevelNumber == 3 || (e.LevelNumber is null && (e.Level ?? "").Contains("Warning", StringComparison.OrdinalIgnoreCase));
    }

    private static string? GetLevelName(EventItem e)
    {
        if (!string.IsNullOrWhiteSpace(e.Level))
        {
            return e.Level;
        }

        return e.LevelNumber switch
        {
            1 => "Critical",
            2 => "Error",
            3 => "Warning",
            4 => "Information",
            5 => "Verbose",
            _ => null
        };
    }

    private sealed class EventRow
    {
        public string? CzasUTC { get; init; }
        public string? Log { get; init; }
        public string? Poziom { get; init; }
        public string? Provider { get; init; }
        public int? Id { get; init; }
        public string? Wiadomosc { get; init; }
    }

    private sealed class PowerPlanItem
    {
        public PowerPlanItem(string name, string guid)
        {
            Name = name;
            Guid = guid;
        }

        public string Name { get; }
        public string Guid { get; }

        public override string ToString() => Name;
    }

    private sealed class CommandItem
    {
        public string? Nazwa { get; init; }
        public string? Komenda { get; init; }
        public string? Opis { get; init; }
        public string? Uwagi { get; init; }
        public string? Przyklad { get; init; }
        public bool WymagaAdmin { get; init; }
    }

    private void EnsureExtraTabs()
    {
        if (_tabDiskHealth is null)
        {
            _layoutDiskSensors = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2
            };
            _layoutDiskSensors.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            _layoutDiskSensors.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            _lblDiskSensors = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Text = "" };
            _gridDiskSensors = new DataGridView { Dock = DockStyle.Fill };

            _layoutDiskSensors.Controls.Add(_lblDiskSensors, 0, 0);
            _layoutDiskSensors.Controls.Add(_gridDiskSensors, 0, 1);

            _tabDiskHealth = new TabPage { Text = "Dysk SMART" };
            _tabDiskHealth.Controls.Add(_layoutDiskSensors);
            tabs.TabPages.Add(_tabDiskHealth);
        }

        if (_tabJunk is null)
        {
            _layoutJunk = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3
            };
            _layoutJunk.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            _layoutJunk.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            _layoutJunk.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            _layoutJunk.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            _layoutJunk.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            _btnEmptyRecycle = new Button { Dock = DockStyle.Fill, Text = "Opróżnij kosz" };
            _btnEmptyRecycle.Click += btnEmptyRecycle_Click;

            _btnClearTemp = new Button { Dock = DockStyle.Fill, Text = "Usuń pliki tymczasowe" };
            _btnClearTemp.Click += btnClearTemp_Click;

            _lblJunkStatus = new Label { Dock = DockStyle.Fill, Text = "", TextAlign = ContentAlignment.TopLeft };

            _layoutJunk.Controls.Add(_btnEmptyRecycle, 0, 0);
            _layoutJunk.SetColumnSpan(_btnEmptyRecycle, 2);
            _layoutJunk.Controls.Add(_btnClearTemp, 0, 1);
            _layoutJunk.SetColumnSpan(_btnClearTemp, 2);
            _layoutJunk.Controls.Add(_lblJunkStatus, 0, 2);
            _layoutJunk.SetColumnSpan(_lblJunkStatus, 2);

            _tabJunk = new TabPage { Text = "Syf" };
            _tabJunk.Controls.Add(_layoutJunk);
            tabs.TabPages.Add(_tabJunk);
        }

        if (_tabCommands is null)
        {
            _layoutCommands = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3
            };
            _layoutCommands.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            _layoutCommands.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            _layoutCommands.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            _layoutCommands.RowStyles.Add(new RowStyle(SizeType.Percent, 65F));
            _layoutCommands.RowStyles.Add(new RowStyle(SizeType.Percent, 35F));

            _btnCopyCommand = new Button { Dock = DockStyle.Fill, Text = "Kopiuj komendę" };
            _btnCopyCommand.Click += btnCopyCommand_Click;

            _btnRunCommand = new Button { Dock = DockStyle.Fill, Text = "Uruchom" };
            _btnRunCommand.Click += btnRunCommand_Click;

            _gridCommands = new DataGridView { Dock = DockStyle.Fill };
            _gridCommands.MultiSelect = false;
            _gridCommands.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _gridCommands.SelectionChanged += gridCommands_SelectionChanged;
            _gridCommands.CellDoubleClick += (_, _) => btnRunCommand_Click(null, EventArgs.Empty);
            _gridCommands.CellMouseDown += gridCommands_CellMouseDown;

            _txtCommandDetails = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical
            };

            _layoutCommands.Controls.Add(_btnCopyCommand, 0, 0);
            _layoutCommands.Controls.Add(_btnRunCommand, 1, 0);
            _layoutCommands.Controls.Add(_gridCommands, 0, 1);
            _layoutCommands.SetColumnSpan(_gridCommands, 2);
            _layoutCommands.Controls.Add(_txtCommandDetails, 0, 2);
            _layoutCommands.SetColumnSpan(_txtCommandDetails, 2);

            _tabCommands = new TabPage { Text = "Komendy" };
            _tabCommands.Controls.Add(_layoutCommands);
            tabs.TabPages.Add(_tabCommands);

            EnsureCommandsContextMenu();
        }

        if (_tabPrograms is null)
        {
            _layoutPrograms = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                Padding = new Padding(10)
            };
            _layoutPrograms.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            _layoutPrograms.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            _layoutPrograms.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            _layoutPrograms.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            _layoutPrograms.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var lbl = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Programy warte uwagi:",
                TextAlign = ContentAlignment.MiddleLeft
            };

            _linkCrystal = new LinkLabel { Dock = DockStyle.Fill, Text = "CrystalDiskInfo", TextAlign = ContentAlignment.MiddleLeft };
            _linkCrystal.LinkClicked += (_, _) => OpenUrl("https://crystalmark.info/en/software/crystaldiskinfo/");

            _linkCpuZ = new LinkLabel { Dock = DockStyle.Fill, Text = "CPU-Z", TextAlign = ContentAlignment.MiddleLeft };
            _linkCpuZ.LinkClicked += (_, _) => OpenUrl("https://www.cpuid.com/softwares/cpu-z.html");

            var lbl2 = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Uwaga: tu będzie kiedyś defender.",
                TextAlign = ContentAlignment.MiddleLeft
            };

            _layoutPrograms.Controls.Add(lbl, 0, 0);
            _layoutPrograms.Controls.Add(_linkCrystal, 0, 1);
            _layoutPrograms.Controls.Add(_linkCpuZ, 0, 2);
            _layoutPrograms.Controls.Add(lbl2, 0, 3);

            _tabPrograms = new TabPage { Text = "Programy" };
            _tabPrograms.Controls.Add(_layoutPrograms);
            tabs.TabPages.Add(_tabPrograms);
        }
    }

    private void EnsureGroupedNavbar()
    {
        if (_navTree is not null || _navSplit is not null)
        {
            return;
        }

        _navTree = new TreeView { Dock = DockStyle.Fill };
        _navTree.AfterSelect += navTree_AfterSelect;

        _navSplit = new SplitContainer
        {
            Dock = DockStyle.Fill,
            FixedPanel = FixedPanel.Panel1,
            SplitterDistance = 210,
            Panel1MinSize = 170
        };

        _navSplit.Panel1.Controls.Add(_navTree);

        if (layoutRoot.Controls.Contains(tabs))
        {
            layoutRoot.Controls.Remove(tabs);
        }

        tabs.Dock = DockStyle.Fill;
        _navSplit.Panel2.Controls.Add(tabs);
        layoutRoot.Controls.Add(_navSplit, 0, 1);

        BuildNavbarTree();
        tabs.SelectedIndexChanged += tabs_SelectedIndexChanged_NavSync;
        SyncNavbarSelectionFromTab();
    }

    private void BuildNavbarTree()
    {
        if (_navTree is null)
        {
            return;
        }

        var nInfo = new TreeNode("Informacje");
        var nDiag = new TreeNode("Diagnostyka");
        var nService = new TreeNode("Serwis");
        var nSettings = new TreeNode("Ustawienia");

        TreeNode GetNavbarGroup(TabPage tp)
        {
            if (ReferenceEquals(tp, tabOptions))
            {
                return nSettings;
            }

            if (ReferenceEquals(tp, tabEvents) || ReferenceEquals(tp, tabTemps) || string.Equals(tp.Text, "Dysk SMART", StringComparison.OrdinalIgnoreCase))
            {
                return nDiag;
            }

            if (string.Equals(tp.Text, "Syf", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(tp.Text, "Komendy", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(tp.Text, "Programy", StringComparison.OrdinalIgnoreCase))
            {
                return nService;
            }

            return nInfo;
        }

        _navTree.BeginUpdate();
        try
        {
            _navTree.Nodes.Clear();

            foreach (var tp in tabs.TabPages.Cast<TabPage>())
            {
                var node = new TreeNode(tp.Text) { Tag = tp };
                GetNavbarGroup(tp).Nodes.Add(node);
            }

            _navTree.Nodes.Add(nInfo);
            _navTree.Nodes.Add(nDiag);
            _navTree.Nodes.Add(nService);
            _navTree.Nodes.Add(nSettings);

            nInfo.Expand();
            nDiag.Expand();
            nService.Expand();
            nSettings.Expand();
        }
        finally
        {
            _navTree.EndUpdate();
        }
    }

    private void navTree_AfterSelect(object? sender, TreeViewEventArgs e)
    {
        if (_navSync)
        {
            return;
        }

        if (e.Node?.Tag is not TabPage tp)
        {
            e.Node?.Expand();
            return;
        }

        _navSync = true;
        try
        {
            tabs.SelectedTab = tp;
        }
        finally
        {
            _navSync = false;
        }
    }

    private void tabs_SelectedIndexChanged_NavSync(object? sender, EventArgs e)
    {
        SyncNavbarSelectionFromTab();
    }

    private void SyncNavbarSelectionFromTab()
    {
        if (_navTree is null || tabs.SelectedTab is null)
        {
            return;
        }

        _navSync = true;
        try
        {
            foreach (TreeNode group in _navTree.Nodes)
            {
                foreach (TreeNode node in group.Nodes)
                {
                    if (ReferenceEquals(node.Tag, tabs.SelectedTab))
                    {
                        _navTree.SelectedNode = node;
                        node.EnsureVisible();
                        return;
                    }
                }
            }
        }
        finally
        {
            _navSync = false;
        }
    }

    private void LoadCommands()
    {
        if (_gridCommands is null)
        {
            return;
        }

        var items = new List<CommandItem>
        {
            new() { Nazwa = "SFC", Komenda = "sfc /scannow", Opis = "Skanuje i naprawia pliki systemu Windows.", Uwagi = "Uruchom jako administrator.", Przyklad = "Kolejność: DISM RestoreHealth -> SFC", WymagaAdmin = true },
            new() { Nazwa = "DISM CheckHealth", Komenda = "DISM /Online /Cleanup-Image /CheckHealth", Opis = "Szybkie sprawdzenie obrazu systemu.", Uwagi = "Uruchom jako administrator.", Przyklad = "DISM /Online /Cleanup-Image /ScanHealth", WymagaAdmin = true },
            new() { Nazwa = "DISM ScanHealth", Komenda = "DISM /Online /Cleanup-Image /ScanHealth", Opis = "Dokładniejsze skanowanie obrazu.", Uwagi = "Uruchom jako administrator.", Przyklad = "DISM /Online /Cleanup-Image /RestoreHealth", WymagaAdmin = true },
            new() { Nazwa = "DISM RestoreHealth", Komenda = "DISM /Online /Cleanup-Image /RestoreHealth", Opis = "Naprawa obrazu systemu.", Uwagi = "Uruchom jako administrator, potem SFC.", Przyklad = "DISM ... /RestoreHealth && sfc /scannow", WymagaAdmin = true },
            new() { Nazwa = "CHKDSK (sprawdź)", Komenda = "chkdsk C:", Opis = "Szybkie sprawdzenie dysku.", Uwagi = "", Przyklad = "chkdsk D:", WymagaAdmin = false },
            new() { Nazwa = "CHKDSK (napraw)", Komenda = "chkdsk C: /f", Opis = "Naprawa błędów systemu plików.", Uwagi = "Dla dysku systemowego może poprosić o restart.", Przyklad = "chkdsk C: /f", WymagaAdmin = true },
            new() { Nazwa = "CHKDSK (f/r)", Komenda = "chkdsk C: /f /r", Opis = "Naprawa + wyszukiwanie bad sectorów.", Uwagi = "Bardzo długo, zwykle wymaga restartu.", Przyklad = "chkdsk C: /f /r", WymagaAdmin = true },
            new() { Nazwa = "Test RAM", Komenda = "mdsched.exe", Opis = "Uruchamia diagnostykę pamięci.", Uwagi = "Potem restart.", Przyklad = "mdsched.exe", WymagaAdmin = false },
            new() { Nazwa = "Bootrec fixmbr", Komenda = "bootrec /fixmbr", Opis = "Naprawa MBR (Recovery).", Uwagi = "W CMD z instalatora/recovery.", Przyklad = "bootrec /rebuildbcd", WymagaAdmin = true },
            new() { Nazwa = "Bootrec fixboot", Komenda = "bootrec /fixboot", Opis = "Naprawa boot (Recovery).", Uwagi = "W CMD z instalatora/recovery.", Przyklad = "bootrec /scanos", WymagaAdmin = true },
            new() { Nazwa = "Bootrec scanos", Komenda = "bootrec /scanos", Opis = "Skan instalacji Windows.", Uwagi = "W CMD z instalatora/recovery.", Przyklad = "bootrec /fixmbr", WymagaAdmin = true },
            new() { Nazwa = "Bootrec rebuildbcd", Komenda = "bootrec /rebuildbcd", Opis = "Odbudowa BCD.", Uwagi = "W CMD z instalatora/recovery.", Przyklad = "bootrec /rebuildbcd", WymagaAdmin = true },
            new() { Nazwa = "Sterowniki", Komenda = "driverquery", Opis = "Lista sterowników.", Uwagi = "", Przyklad = "driverquery /v", WymagaAdmin = false },
            new() { Nazwa = "Systeminfo", Komenda = "systeminfo", Opis = "Informacje o systemie.", Uwagi = "", Przyklad = "systeminfo | findstr /B /C:\"OS\"", WymagaAdmin = false },
            new() { Nazwa = "IPCONFIG", Komenda = "ipconfig", Opis = "Podstawowe informacje o IP.", Uwagi = "", Przyklad = "ipconfig", WymagaAdmin = false },
            new() { Nazwa = "IPCONFIG /all", Komenda = "ipconfig /all", Opis = "Szczegółowe informacje o kartach sieciowych.", Uwagi = "", Przyklad = "ipconfig /all", WymagaAdmin = false },
            new() { Nazwa = "Flush DNS", Komenda = "ipconfig /flushdns", Opis = "Czyści cache DNS.", Uwagi = "", Przyklad = "ipconfig /flushdns", WymagaAdmin = false },
            new() { Nazwa = "Reset Winsock", Komenda = "netsh winsock reset", Opis = "Reset stosu sieciowego.", Uwagi = "Po tym restart PC.", Przyklad = "netsh winsock reset", WymagaAdmin = true },
            new() { Nazwa = "Reset IP", Komenda = "netsh int ip reset", Opis = "Reset IP stack.", Uwagi = "Po tym restart PC.", Przyklad = "netsh int ip reset", WymagaAdmin = true },
            new() { Nazwa = "Release IP", Komenda = "ipconfig /release", Opis = "Zwalnia adres IP.", Uwagi = "", Przyklad = "ipconfig /release", WymagaAdmin = false },
            new() { Nazwa = "Renew IP", Komenda = "ipconfig /renew", Opis = "Odnawia adres IP.", Uwagi = "", Przyklad = "ipconfig /renew", WymagaAdmin = false },
            new() { Nazwa = "Ping Google", Komenda = "ping 8.8.8.8 -t", Opis = "Test pingu (zatrzymanie CTRL+C).", Uwagi = "", Przyklad = "ping google.com -t", WymagaAdmin = false },
            new() { Nazwa = "Tracert", Komenda = "tracert google.com", Opis = "Trasa internetu.", Uwagi = "", Przyklad = "tracert 8.8.8.8", WymagaAdmin = false },
            new() { Nazwa = "Wyłącz hibernację", Komenda = "powercfg -h off", Opis = "Wyłącza hibernację (zwalnia miejsce).", Uwagi = "Uruchom jako administrator.", Przyklad = "powercfg -h off", WymagaAdmin = true },
            new() { Nazwa = "Cleanmgr", Komenda = "cleanmgr", Opis = "Uruchamia czyszczenie dysku.", Uwagi = "", Przyklad = "cleanmgr", WymagaAdmin = false },
            new() { Nazwa = "Event Viewer", Komenda = "eventvwr.msc", Opis = "Podgląd zdarzeń.", Uwagi = "", Przyklad = "eventvwr.msc", WymagaAdmin = false },
            new() { Nazwa = "Monitor niezawodności", Komenda = "perfmon /rel", Opis = "Historia crashy i błędów.", Uwagi = "", Przyklad = "perfmon /rel", WymagaAdmin = false }
        };

        _gridCommands.DataSource = items;
        try
        {
            if (_gridCommands.Columns.Contains("WymagaAdmin"))
            {
                _gridCommands.Columns["WymagaAdmin"].Visible = false;
            }
        }
        catch
        {
        }
        if (_gridCommands.Rows.Count > 0)
        {
            _gridCommands.CurrentCell = _gridCommands.Rows[0].Cells[0];
        }
        UpdateCommandDetailsBox();
    }

    private void EnsureCommandsContextMenu()
    {
        if (_gridCommands is null || _cmdContextMenu is not null)
        {
            return;
        }

        _cmdMenuRun = new ToolStripMenuItem("Uruchom", null, (_, _) => RunSelectedCommand(runAsAdmin: false));
        _cmdMenuRunAdmin = new ToolStripMenuItem("Uruchom jako administrator", null, (_, _) => RunSelectedCommand(runAsAdmin: true));
        _cmdMenuCopy = new ToolStripMenuItem("Kopiuj komendę", null, (_, _) => btnCopyCommand_Click(null, EventArgs.Empty));

        _cmdContextMenu = new ContextMenuStrip();
        _cmdContextMenu.Items.AddRange(new ToolStripItem[]
        {
            _cmdMenuRun,
            _cmdMenuRunAdmin,
            new ToolStripSeparator(),
            _cmdMenuCopy
        });
        _cmdContextMenu.Opening += cmdContextMenu_Opening;

        _gridCommands.ContextMenuStrip = _cmdContextMenu;
    }

    private void cmdContextMenu_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (_gridCommands is null || _gridCommands.CurrentRow is null)
        {
            _cmdMenuRun!.Enabled = false;
            _cmdMenuRunAdmin!.Enabled = false;
            _cmdMenuCopy!.Enabled = false;
            return;
        }

        var cmd = _gridCommands.CurrentRow.Cells["Komenda"]?.Value?.ToString();
        var ok = !string.IsNullOrWhiteSpace(cmd);
        _cmdMenuRun!.Enabled = ok;
        _cmdMenuRunAdmin!.Enabled = ok;
        _cmdMenuCopy!.Enabled = ok;
    }

    private void gridCommands_CellMouseDown(object? sender, DataGridViewCellMouseEventArgs e)
    {
        if (_gridCommands is null)
        {
            return;
        }

        if (e.Button == MouseButtons.Right && e.RowIndex >= 0)
        {
            _gridCommands.ClearSelection();
            _gridCommands.CurrentCell = _gridCommands.Rows[e.RowIndex].Cells[Math.Max(0, e.ColumnIndex)];
            _gridCommands.Rows[e.RowIndex].Selected = true;
        }
    }

    private void RunSelectedCommand(bool runAsAdmin)
    {
        if (_gridCommands is null || _gridCommands.CurrentRow is null)
        {
            return;
        }

        var cmd = _gridCommands.CurrentRow.Cells["Komenda"]?.Value?.ToString();
        if (string.IsNullOrWhiteSpace(cmd))
        {
            return;
        }

        StartCommand(cmd, runAsAdmin);
        AppendCommandActionLine($"Uruchomiono: {cmd}" + (runAsAdmin ? " (admin)" : ""));
        SetStatus("Uruchomiono komendę");
        UpdateCommandDetailsBox();
    }

    private void btnCopyCommand_Click(object? sender, EventArgs e)
    {
        try
        {
            if (_gridCommands is null || _gridCommands.CurrentRow is null)
            {
                return;
            }

            var cmd = _gridCommands.CurrentRow.Cells["Komenda"]?.Value?.ToString();
            if (string.IsNullOrWhiteSpace(cmd))
            {
                return;
            }

            Clipboard.SetText(cmd);
            SetStatus("Skopiowano komendę");
        }
        catch (Exception ex)
        {
            SetStatus($"Błąd: {ex.Message}");
        }
    }

    private void btnRunCommand_Click(object? sender, EventArgs e)
    {
        try
        {
            if (_gridCommands is null || _gridCommands.CurrentRow is null)
            {
                return;
            }

            var cmd = _gridCommands.CurrentRow.Cells["Komenda"]?.Value?.ToString();
            if (string.IsNullOrWhiteSpace(cmd))
            {
                return;
            }

            var requiresAdmin = false;
            try
            {
                var v = _gridCommands.CurrentRow.Cells["WymagaAdmin"]?.Value;
                requiresAdmin = v is bool b && b;
            }
            catch
            {
            }

            var runAsAdmin = false;
            if (requiresAdmin && !_isAdmin)
            {
                var ask = MessageBox.Show(this, "Ta komenda zwykle wymaga administratora. Uruchomić jako administrator?", "Komendy", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (ask != DialogResult.Yes)
                {
                    AppendCommandActionLine($"Anulowano: {cmd}");
                    SetStatus("Anulowano uruchomienie komendy");
                    return;
                }

                runAsAdmin = true;
            }

            StartCommand(cmd, runAsAdmin);
            AppendCommandActionLine($"Uruchomiono: {cmd}" + (runAsAdmin ? " (admin)" : ""));
            SetStatus("Uruchomiono komendę");
        }
        catch (Exception ex)
        {
            AppendCommandActionLine($"Błąd: {ex.Message}");
            SetStatus($"Błąd: {ex.Message}");
        }
        finally
        {
            UpdateCommandDetailsBox();
        }
    }

    private void gridCommands_SelectionChanged(object? sender, EventArgs e)
    {
        UpdateCommandDetailsBox();
    }

    private void UpdateCommandDetailsBox()
    {
        if (_txtCommandDetails is null || _gridCommands is null)
        {
            return;
        }

        string? nazwa = null;
        string? cmd = null;
        string? opis = null;
        string? uwagi = null;
        string? przyklad = null;

        try
        {
            if (_gridCommands.CurrentRow is not null)
            {
                nazwa = _gridCommands.CurrentRow.Cells["Nazwa"]?.Value?.ToString();
                cmd = _gridCommands.CurrentRow.Cells["Komenda"]?.Value?.ToString();
                opis = _gridCommands.CurrentRow.Cells["Opis"]?.Value?.ToString();
                uwagi = _gridCommands.CurrentRow.Cells["Uwagi"]?.Value?.ToString();
                przyklad = _gridCommands.CurrentRow.Cells["Przyklad"]?.Value?.ToString();
            }
        }
        catch
        {
        }

        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(nazwa)) parts.Add($"Nazwa: {nazwa}");
        if (!string.IsNullOrWhiteSpace(cmd)) parts.Add($"Komenda: {cmd}");
        if (!string.IsNullOrWhiteSpace(opis)) parts.Add($"Opis: {opis}");
        if (!string.IsNullOrWhiteSpace(uwagi)) parts.Add($"Uwagi: {uwagi}");
        if (!string.IsNullOrWhiteSpace(przyklad)) parts.Add($"Przykład: {przyklad}");

        var lastActions = _commandActionLines.Count == 0
            ? ""
            : "\r\n\r\nAkcje:\r\n" + string.Join("\r\n", _commandActionLines.TakeLast(20));

        _txtCommandDetails.Text = string.Join("\r\n", parts) + lastActions;
    }

    private void AppendCommandActionLine(string line)
    {
        _commandActionLines.Add($"[{DateTime.Now:HH:mm:ss}] {line}");
    }

    private static void StartCommand(string command, bool runAsAdmin)
    {
        if (command.EndsWith(".msc", StringComparison.OrdinalIgnoreCase) ||
            command.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
        {
            var psiApp = new System.Diagnostics.ProcessStartInfo(command)
            {
                UseShellExecute = true
            };
            if (runAsAdmin)
            {
                psiApp.Verb = "runas";
            }

            System.Diagnostics.Process.Start(psiApp);
            return;
        }

        var args = $"/k {command}";
        var psi = new System.Diagnostics.ProcessStartInfo("cmd.exe", args)
        {
            UseShellExecute = true
        };
        if (runAsAdmin)
        {
            psi.Verb = "runas";
        }

        System.Diagnostics.Process.Start(psi);
    }

    private void EnsureManufacturerContextMenus()
    {
        if (_mfgContextMenu is not null)
        {
            return;
        }

        _mfgMenuOpen = new ToolStripMenuItem("Strona producenta", null, (_, _) => OpenManufacturerFromMenu(openVendor: true));
        _mfgMenuSearch = new ToolStripMenuItem("Szukaj po modelu", null, (_, _) => OpenManufacturerFromMenu(openVendor: false));

        _mfgContextMenu = new ContextMenuStrip();
        _mfgContextMenu.Items.AddRange(new ToolStripItem[] { _mfgMenuOpen, _mfgMenuSearch });
        _mfgContextMenu.Opening += mfgContextMenu_Opening;

        gridCpu.ContextMenuStrip = _mfgContextMenu;
        gridGpus.ContextMenuStrip = _mfgContextMenu;
        gridRamModules.ContextMenuStrip = _mfgContextMenu;
        gridMotherboard.ContextMenuStrip = _mfgContextMenu;
        gridDiskDrives.ContextMenuStrip = _mfgContextMenu;
        gridBios.ContextMenuStrip = _mfgContextMenu;
    }

    private void mfgContextMenu_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (_mfgContextMenu?.SourceControl is not DataGridView grid)
        {
            _mfgMenuOpen!.Enabled = false;
            _mfgMenuSearch!.Enabled = false;
            return;
        }

        var text = GetManufacturerSourceText(grid);
        var hasText = !string.IsNullOrWhiteSpace(text);
        var url = ResolveVendorHomeUrl(text);

        _mfgMenuOpen!.Enabled = hasText;
        _mfgMenuSearch!.Enabled = hasText;
        _mfgMenuOpen.Text = url is null ? "Strona producenta (szukaj)" : "Strona producenta";
    }

    private void OpenManufacturerFromMenu(bool openVendor)
    {
        if (_mfgContextMenu?.SourceControl is not DataGridView grid)
        {
            return;
        }

        var text = GetManufacturerSourceText(grid);
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        if (openVendor)
        {
            var url = ResolveVendorHomeUrl(text) ?? BuildSearchUrl(text);
            OpenUrl(url);
            return;
        }

        OpenUrl(BuildSearchUrl(text));
    }

    private string? GetManufacturerSourceText(DataGridView grid)
    {
        if (ReferenceEquals(grid, gridCpu))
        {
            return _report?.Cpu.Name;
        }

        if (ReferenceEquals(grid, gridGpus))
        {
            return grid.CurrentRow?.Cells["Nazwa"]?.Value?.ToString();
        }

        if (ReferenceEquals(grid, gridRamModules))
        {
            return grid.CurrentRow?.Cells["Producent"]?.Value?.ToString();
        }

        if (ReferenceEquals(grid, gridMotherboard))
        {
            return _report?.Motherboard.Manufacturer ?? _report?.Motherboard.SystemManufacturer;
        }

        if (ReferenceEquals(grid, gridDiskDrives))
        {
            return grid.CurrentRow?.Cells["Model"]?.Value?.ToString();
        }

        if (ReferenceEquals(grid, gridBios))
        {
            return _report?.Bios.Manufacturer;
        }

        return null;
    }

    private static string BuildSearchUrl(string query)
    {
        var q = Uri.EscapeDataString($"{query} official site");
        return $"https://www.google.com/search?q={q}";
    }

    private static string? ResolveVendorHomeUrl(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        var s = input.Trim();
        var l = s.ToLowerInvariant();

        if (l.Contains("intel", StringComparison.OrdinalIgnoreCase)) return "https://www.intel.com/";
        if (l.Contains("nvidia", StringComparison.OrdinalIgnoreCase) || l.Contains("geforce", StringComparison.OrdinalIgnoreCase)) return "https://www.nvidia.com/";
        if (l.Contains("amd", StringComparison.OrdinalIgnoreCase) || l.Contains("ryzen", StringComparison.OrdinalIgnoreCase) || l.Contains("radeon", StringComparison.OrdinalIgnoreCase)) return "https://www.amd.com/";

        if (l.Contains("asus", StringComparison.OrdinalIgnoreCase) || l.Contains("asustek", StringComparison.OrdinalIgnoreCase)) return "https://www.asus.com/";
        if (l.Contains("gigabyte", StringComparison.OrdinalIgnoreCase)) return "https://www.gigabyte.com/";
        if (l.Contains("micro-star", StringComparison.OrdinalIgnoreCase) || l.Contains("msi", StringComparison.OrdinalIgnoreCase)) return "https://www.msi.com/";
        if (l.Contains("asrock", StringComparison.OrdinalIgnoreCase)) return "https://www.asrock.com/";

        if (l.Contains("dell", StringComparison.OrdinalIgnoreCase)) return "https://www.dell.com/";
        if (l.Contains("lenovo", StringComparison.OrdinalIgnoreCase)) return "https://www.lenovo.com/";
        if (l.Contains("hewlett-packard", StringComparison.OrdinalIgnoreCase) || l.Contains("hp", StringComparison.OrdinalIgnoreCase)) return "https://www.hp.com/";
        if (l.Contains("acer", StringComparison.OrdinalIgnoreCase)) return "https://www.acer.com/";
        if (l.Contains("microsoft", StringComparison.OrdinalIgnoreCase)) return "https://www.microsoft.com/";

        if (l.Contains("samsung", StringComparison.OrdinalIgnoreCase)) return "https://www.samsung.com/";
        if (l.Contains("seagate", StringComparison.OrdinalIgnoreCase)) return "https://www.seagate.com/";
        if (l.Contains("toshiba", StringComparison.OrdinalIgnoreCase)) return "https://www.toshiba.com/";
        if (l.Contains("kingston", StringComparison.OrdinalIgnoreCase)) return "https://www.kingston.com/";
        if (l.Contains("crucial", StringComparison.OrdinalIgnoreCase)) return "https://www.crucial.com/";
        if (l.Contains("sandisk", StringComparison.OrdinalIgnoreCase)) return "https://www.westerndigital.com/";
        if (l.Contains("western digital", StringComparison.OrdinalIgnoreCase) || l.Contains("wdc", StringComparison.OrdinalIgnoreCase) || l.Contains("wds", StringComparison.OrdinalIgnoreCase)) return "https://www.westerndigital.com/";

        if (l.Contains("corsair", StringComparison.OrdinalIgnoreCase)) return "https://www.corsair.com/";
        if (l.Contains("g.skill", StringComparison.OrdinalIgnoreCase) || l.Contains("gskill", StringComparison.OrdinalIgnoreCase)) return "https://www.gskill.com/";
        if (l.Contains("hynix", StringComparison.OrdinalIgnoreCase) || l.Contains("sk hynix", StringComparison.OrdinalIgnoreCase)) return "https://www.skhynix.com/";
        if (l.Contains("micron", StringComparison.OrdinalIgnoreCase)) return "https://www.micron.com/";
        if (l.Contains("adata", StringComparison.OrdinalIgnoreCase)) return "https://www.adata.com/";
        if (l.Contains("patriot", StringComparison.OrdinalIgnoreCase)) return "https://patriotmemory.com/";

        if (l.Contains("american megatrends", StringComparison.OrdinalIgnoreCase) || l.Contains("ami", StringComparison.OrdinalIgnoreCase)) return "https://www.ami.com/";
        if (l.Contains("insyde", StringComparison.OrdinalIgnoreCase)) return "https://www.insyde.com/";
        if (l.Contains("phoenix", StringComparison.OrdinalIgnoreCase)) return "https://www.phoenix.com/";

        return null;
    }

    private void btnEmptyRecycle_Click(object? sender, EventArgs e)
    {
        try
        {
            var hr = NativeMethods.SHEmptyRecycleBinW(IntPtr.Zero, null,
                NativeMethods.SHERB_NOCONFIRMATION | NativeMethods.SHERB_NOPROGRESSUI | NativeMethods.SHERB_NOSOUND);

            if (hr != 0)
            {
                _lblJunkStatus!.Text = $"Błąd kosza: 0x{hr:X8}";
            }
            else
            {
                _lblJunkStatus!.Text = "Kosz opróżniony";
                SetStatus("Wykonano: opróżnianie kosza");
            }
        }
        catch (Exception ex)
        {
            _lblJunkStatus!.Text = $"Błąd kosza: {ex.Message}";
            SetStatus($"Błąd: {ex.Message}");
        }
    }

    private void btnClearTemp_Click(object? sender, EventArgs e)
    {
        try
        {
            var deletedFiles = 0;
            var deletedDirs = 0;
            var errors = 0;

            TryDeleteDirectoryContents(Path.GetTempPath(), ref deletedFiles, ref deletedDirs, ref errors);

            if (_isAdmin)
            {
                TryDeleteDirectoryContents(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp"), ref deletedFiles, ref deletedDirs, ref errors);
            }

            _lblJunkStatus!.Text = $"Usunięto: pliki={deletedFiles}, foldery={deletedDirs}, błędy={errors}" + (_isAdmin ? "" : " (Windows\\Temp wymaga admina)");
            SetStatus("Wykonano: czyszczenie plików tymczasowych");
        }
        catch (Exception ex)
        {
            _lblJunkStatus!.Text = $"Błąd temp: {ex.Message}";
            SetStatus($"Błąd: {ex.Message}");
        }
    }

    private static void TryDeleteDirectoryContents(string directory, ref int deletedFiles, ref int deletedDirs, ref int errors)
    {
        try
        {
            if (!Directory.Exists(directory))
            {
                return;
            }

            foreach (var file in Directory.EnumerateFiles(directory))
            {
                try
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);
                    deletedFiles++;
                }
                catch
                {
                    errors++;
                }
            }

            foreach (var dir in Directory.EnumerateDirectories(directory))
            {
                try
                {
                    Directory.Delete(dir, true);
                    deletedDirs++;
                }
                catch
                {
                    errors++;
                }
            }
        }
        catch
        {
            errors++;
        }
    }

    private static class NativeMethods
    {
        public const uint SHERB_NOCONFIRMATION = 0x00000001;
        public const uint SHERB_NOPROGRESSUI = 0x00000002;
        public const uint SHERB_NOSOUND = 0x00000004;

        [System.Runtime.InteropServices.DllImport("shell32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        public static extern int SHEmptyRecycleBinW(IntPtr hwnd, string? pszRootPath, uint dwFlags);
    }

    private static void OpenUrl(string url)
    {
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true });
    }

    private static string GetAppVersion()
    {
        var asm = typeof(Form1).Assembly;
        var info = asm.GetCustomAttributes(typeof(System.Reflection.AssemblyInformationalVersionAttribute), false)
            .OfType<System.Reflection.AssemblyInformationalVersionAttribute>()
            .FirstOrDefault()
            ?.InformationalVersion;

        return string.IsNullOrWhiteSpace(info)
            ? (asm.GetName().Version?.ToString() ?? "0.0.0")
            : info;
    }

    private string FormatSmartMissing()
    {
        if (!_settings.CollectSmartDiskInfo)
        {
            return "";
        }

        return _isAdmin ? "Brak" : "Brak (uruchom jako admin)";
    }

    private string? FormatSmartNumber(uint? value)
    {
        if (value is null)
        {
            return FormatSmartMissing();
        }

        return value.Value.ToString();
    }

    private static bool IsAdministrator()
    {
        try
        {
            var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            var principal = new System.Security.Principal.WindowsPrincipal(identity);
            return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
        }
        catch
        {
            return false;
        }
    }


    private void SetStatus(string text)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => lblStatus.Text = text));
        }
        else
        {
            lblStatus.Text = text;
        }
    }

    private static void SetupKeyValueGrid(DataGridView grid)
    {
        SetupGrid(grid);
        grid.AutoGenerateColumns = false;
        grid.Columns.Clear();
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colKey",
            HeaderText = "Pole",
            DataPropertyName = "Key",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colValue",
            HeaderText = "Wartość",
            DataPropertyName = "Value",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        });
    }

    private static void SetupGrid(DataGridView grid)
    {
        grid.ReadOnly = true;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.AllowUserToResizeRows = false;
        grid.MultiSelect = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.RowHeadersVisible = false;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
        grid.BackgroundColor = SystemColors.Window;
    }

    private static void SetKeyValueRows(DataGridView grid, IEnumerable<(string key, string? value)> rows)
    {
        var data = rows
            .Select(r => new KeyValuePair<string, string>(r.key, r.value ?? ""))
            .ToList();

        grid.DataSource = data;
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
