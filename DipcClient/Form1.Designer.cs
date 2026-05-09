namespace DipcClient;

partial class Form1
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        layoutRoot = new TableLayoutPanel();
        layoutTop = new TableLayoutPanel();
        btnRefresh = new Button();
        btnExport = new Button();
        btnImport = new Button();
        lblStatus = new Label();
        tabs = new TabControl();
        tabSummary = new TabPage();
        gridSummary = new DataGridView();
        tabCpu = new TabPage();
        gridCpu = new DataGridView();
        tabGpu = new TabPage();
        gridGpus = new DataGridView();
        tabRam = new TabPage();
        splitRam = new SplitContainer();
        gridRam = new DataGridView();
        gridRamModules = new DataGridView();
        tabMotherboard = new TabPage();
        gridMotherboard = new DataGridView();
        tabDisks = new TabPage();
        splitDisks = new SplitContainer();
        gridDiskDrives = new DataGridView();
        gridLogicalDisks = new DataGridView();
        tabBios = new TabPage();
        gridBios = new DataGridView();
        tabSystem = new TabPage();
        gridSystem = new DataGridView();
        tabNetwork = new TabPage();
        gridNetwork = new DataGridView();
        tabDisplays = new TabPage();
        gridScreens = new DataGridView();
        tabEvents = new TabPage();
        layoutEvents = new TableLayoutPanel();
        lblEventFilter = new Label();
        txtEventFilter = new TextBox();
        cmbEventLevel = new ComboBox();
        cmbEventLog = new ComboBox();
        btnEventCopySelected = new Button();
        btnEventCopyAll = new Button();
        btnEventClearLog = new Button();
        gridEvents = new DataGridView();
        tabTemps = new TabPage();
        gridTemps = new DataGridView();
        tabOptions = new TabPage();
        layoutOptions = new TableLayoutPanel();
        linkAuthor = new LinkLabel();
        lblVersion = new Label();
        btnCheckUpdates = new Button();
        chkCollectEvents = new CheckBox();
        chkCollectTemps = new CheckBox();
        chkCollectSmart = new CheckBox();
        lblEventDays = new Label();
        nudEventDays = new NumericUpDown();
        lblMaxEvents = new Label();
        nudMaxEvents = new NumericUpDown();
        lblPowerPlan = new Label();
        cmbPowerPlan = new ComboBox();
        btnApplyPowerPlan = new Button();
        lblOptionsNote = new Label();
        statusStrip = new StatusStrip();
        statusAuthor = new ToolStripStatusLabel();
        statusVersion = new ToolStripStatusLabel();
        layoutRoot.SuspendLayout();
        layoutTop.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)gridSummary).BeginInit();
        tabs.SuspendLayout();
        tabSummary.SuspendLayout();
        tabCpu.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)gridCpu).BeginInit();
        tabGpu.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)gridGpus).BeginInit();
        tabRam.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splitRam).BeginInit();
        splitRam.Panel1.SuspendLayout();
        splitRam.Panel2.SuspendLayout();
        splitRam.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)gridRam).BeginInit();
        ((System.ComponentModel.ISupportInitialize)gridRamModules).BeginInit();
        tabMotherboard.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)gridMotherboard).BeginInit();
        tabDisks.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splitDisks).BeginInit();
        splitDisks.Panel1.SuspendLayout();
        splitDisks.Panel2.SuspendLayout();
        splitDisks.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)gridDiskDrives).BeginInit();
        ((System.ComponentModel.ISupportInitialize)gridLogicalDisks).BeginInit();
        tabBios.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)gridBios).BeginInit();
        tabSystem.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)gridSystem).BeginInit();
        tabNetwork.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)gridNetwork).BeginInit();
        tabDisplays.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)gridScreens).BeginInit();
        tabEvents.SuspendLayout();
        layoutEvents.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)gridEvents).BeginInit();
        tabTemps.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)gridTemps).BeginInit();
        tabOptions.SuspendLayout();
        layoutOptions.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)nudEventDays).BeginInit();
        ((System.ComponentModel.ISupportInitialize)nudMaxEvents).BeginInit();
        statusStrip.SuspendLayout();
        SuspendLayout();

        layoutRoot.ColumnCount = 1;
        layoutRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layoutRoot.Dock = DockStyle.Fill;
        layoutRoot.RowCount = 3;
        layoutRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 72F));
        layoutRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layoutRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));

        layoutTop.ColumnCount = 4;
        layoutTop.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160F));
        layoutTop.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180F));
        layoutTop.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180F));
        layoutTop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layoutTop.Dock = DockStyle.Fill;
        layoutTop.RowCount = 2;
        layoutTop.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
        layoutTop.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));

        btnRefresh.Dock = DockStyle.Fill;
        btnRefresh.Text = "Zbierz dane";
        btnRefresh.UseVisualStyleBackColor = true;
        btnRefresh.Click += btnRefresh_Click;

        btnExport.Dock = DockStyle.Fill;
        btnExport.Text = "Eksport do Excela";
        btnExport.UseVisualStyleBackColor = true;
        btnExport.Click += btnExport_Click;

        btnImport.Dock = DockStyle.Fill;
        btnImport.Text = "Wczytaj z Excela";
        btnImport.UseVisualStyleBackColor = true;
        btnImport.Click += btnImport_Click;

        lblStatus.Dock = DockStyle.Fill;
        lblStatus.Text = "Gotowe";
        lblStatus.TextAlign = ContentAlignment.MiddleLeft;

        layoutTop.Controls.Add(btnRefresh, 0, 0);
        layoutTop.Controls.Add(btnExport, 1, 0);
        layoutTop.Controls.Add(btnImport, 2, 0);
        layoutTop.Controls.Add(lblStatus, 0, 1);
        layoutTop.SetColumnSpan(lblStatus, 4);

        tabs.Dock = DockStyle.Fill;
        tabs.Controls.Add(tabSummary);
        tabs.Controls.Add(tabCpu);
        tabs.Controls.Add(tabGpu);
        tabs.Controls.Add(tabRam);
        tabs.Controls.Add(tabMotherboard);
        tabs.Controls.Add(tabDisks);
        tabs.Controls.Add(tabBios);
        tabs.Controls.Add(tabSystem);
        tabs.Controls.Add(tabNetwork);
        tabs.Controls.Add(tabDisplays);
        tabs.Controls.Add(tabEvents);
        tabs.Controls.Add(tabTemps);
        tabs.Controls.Add(tabOptions);

        tabSummary.Text = "Podsumowanie";
        tabSummary.Controls.Add(gridSummary);
        gridSummary.Dock = DockStyle.Fill;

        tabCpu.Text = "Procesor";
        tabCpu.Controls.Add(gridCpu);
        gridCpu.Dock = DockStyle.Fill;

        tabGpu.Text = "Karta graficzna";
        tabGpu.Controls.Add(gridGpus);
        gridGpus.Dock = DockStyle.Fill;

        tabRam.Text = "RAM";
        tabRam.Controls.Add(splitRam);
        splitRam.Dock = DockStyle.Fill;
        splitRam.Orientation = Orientation.Horizontal;
        splitRam.SplitterDistance = 120;
        splitRam.Panel1.Controls.Add(gridRam);
        splitRam.Panel2.Controls.Add(gridRamModules);
        gridRam.Dock = DockStyle.Fill;
        gridRamModules.Dock = DockStyle.Fill;

        tabMotherboard.Text = "Płyta / Model";
        tabMotherboard.Controls.Add(gridMotherboard);
        gridMotherboard.Dock = DockStyle.Fill;

        tabDisks.Text = "Dyski";
        tabDisks.Controls.Add(splitDisks);
        splitDisks.Dock = DockStyle.Fill;
        splitDisks.Orientation = Orientation.Horizontal;
        splitDisks.SplitterDistance = 220;
        splitDisks.Panel1.Controls.Add(gridDiskDrives);
        splitDisks.Panel2.Controls.Add(gridLogicalDisks);
        gridDiskDrives.Dock = DockStyle.Fill;
        gridLogicalDisks.Dock = DockStyle.Fill;

        tabBios.Text = "BIOS";
        tabBios.Controls.Add(gridBios);
        gridBios.Dock = DockStyle.Fill;

        tabSystem.Text = "System";
        tabSystem.Controls.Add(gridSystem);
        gridSystem.Dock = DockStyle.Fill;

        tabNetwork.Text = "Sieć";
        tabNetwork.Controls.Add(gridNetwork);
        gridNetwork.Dock = DockStyle.Fill;

        tabDisplays.Text = "Ekrany";
        tabDisplays.Controls.Add(gridScreens);
        gridScreens.Dock = DockStyle.Fill;

        tabEvents.Text = "Zdarzenia";
        tabEvents.Controls.Add(layoutEvents);

        layoutEvents.ColumnCount = 7;
        layoutEvents.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 48F));
        layoutEvents.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
        layoutEvents.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140F));
        layoutEvents.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140F));
        layoutEvents.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
        layoutEvents.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
        layoutEvents.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
        layoutEvents.Dock = DockStyle.Fill;
        layoutEvents.RowCount = 2;
        layoutEvents.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        layoutEvents.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        lblEventFilter.Dock = DockStyle.Fill;
        lblEventFilter.Text = "Filtr:";
        lblEventFilter.TextAlign = ContentAlignment.MiddleLeft;

        txtEventFilter.Dock = DockStyle.Fill;
        txtEventFilter.TextChanged += txtEventFilter_TextChanged;

        cmbEventLevel.Dock = DockStyle.Fill;
        cmbEventLevel.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbEventLevel.Items.AddRange(new object[] { "Krytyczne + Błędy", "Krytyczne", "Błędy", "Ostrzeżenia", "Wszystkie" });
        cmbEventLevel.SelectedIndexChanged += cmbEventLevel_SelectedIndexChanged;

        cmbEventLog.Dock = DockStyle.Fill;
        cmbEventLog.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbEventLog.Items.AddRange(new object[] { "Wszystkie logi", "System", "Application" });
        cmbEventLog.SelectedIndexChanged += cmbEventLog_SelectedIndexChanged;

        btnEventCopySelected.Dock = DockStyle.Fill;
        btnEventCopySelected.Text = "Kopiuj zaznaczone";
        btnEventCopySelected.UseVisualStyleBackColor = true;
        btnEventCopySelected.Click += btnEventCopySelected_Click;

        btnEventCopyAll.Dock = DockStyle.Fill;
        btnEventCopyAll.Text = "Kopiuj wszystko";
        btnEventCopyAll.UseVisualStyleBackColor = true;
        btnEventCopyAll.Click += btnEventCopyAll_Click;

        btnEventClearLog.Dock = DockStyle.Fill;
        btnEventClearLog.Text = "Wyczyść log";
        btnEventClearLog.UseVisualStyleBackColor = true;
        btnEventClearLog.Click += btnEventClearLog_Click;

        gridEvents.Dock = DockStyle.Fill;

        layoutEvents.Controls.Add(lblEventFilter, 0, 0);
        layoutEvents.Controls.Add(txtEventFilter, 1, 0);
        layoutEvents.Controls.Add(cmbEventLevel, 2, 0);
        layoutEvents.Controls.Add(cmbEventLog, 3, 0);
        layoutEvents.Controls.Add(btnEventCopySelected, 4, 0);
        layoutEvents.Controls.Add(btnEventCopyAll, 5, 0);
        layoutEvents.Controls.Add(btnEventClearLog, 6, 0);
        layoutEvents.Controls.Add(gridEvents, 0, 1);
        layoutEvents.SetColumnSpan(gridEvents, 7);

        tabTemps.Text = "Temperatury";
        tabTemps.Controls.Add(gridTemps);
        gridTemps.Dock = DockStyle.Fill;

        tabOptions.Text = "Opcje";
        tabOptions.Controls.Add(layoutOptions);

        layoutOptions.ColumnCount = 2;
        layoutOptions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        layoutOptions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        layoutOptions.Dock = DockStyle.Fill;
        layoutOptions.RowCount = 11;
        layoutOptions.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        layoutOptions.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        layoutOptions.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        layoutOptions.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        layoutOptions.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        layoutOptions.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        layoutOptions.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        layoutOptions.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        layoutOptions.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        layoutOptions.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));
        layoutOptions.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        lblVersion.Dock = DockStyle.Fill;
        lblVersion.Text = "Wersja:";
        lblVersion.TextAlign = ContentAlignment.MiddleLeft;

        linkAuthor.Dock = DockStyle.Fill;
        linkAuthor.Text = "Autor: dziondzio (dziondzio.xyz)";
        linkAuthor.TextAlign = ContentAlignment.MiddleLeft;
        linkAuthor.LinkClicked += linkAuthor_LinkClicked;

        btnCheckUpdates.Dock = DockStyle.Fill;
        btnCheckUpdates.Text = "Sprawdź aktualizacje";
        btnCheckUpdates.UseVisualStyleBackColor = true;
        btnCheckUpdates.Click += btnCheckUpdates_Click;

        chkCollectEvents.Dock = DockStyle.Fill;
        chkCollectEvents.Text = "Zbieraj zdarzenia (System/Application)";
        chkCollectEvents.CheckedChanged += chkCollectEvents_CheckedChanged;

        chkCollectTemps.Dock = DockStyle.Fill;
        chkCollectTemps.Text = "Zbieraj temperatury";
        chkCollectTemps.CheckedChanged += chkCollectTemps_CheckedChanged;

        chkCollectSmart.Dock = DockStyle.Fill;
        chkCollectSmart.Text = "Zbieraj SMART (godziny/cycles)";
        chkCollectSmart.CheckedChanged += chkCollectSmart_CheckedChanged;

        lblEventDays.Dock = DockStyle.Fill;
        lblEventDays.Text = "Zakres zdarzeń (dni):";
        lblEventDays.TextAlign = ContentAlignment.MiddleLeft;

        nudEventDays.Dock = DockStyle.Fill;
        nudEventDays.Minimum = 1;
        nudEventDays.Maximum = 60;
        nudEventDays.ValueChanged += nudEventDays_ValueChanged;

        lblMaxEvents.Dock = DockStyle.Fill;
        lblMaxEvents.Text = "Limit zdarzeń:";
        lblMaxEvents.TextAlign = ContentAlignment.MiddleLeft;

        nudMaxEvents.Dock = DockStyle.Fill;
        nudMaxEvents.Minimum = 50;
        nudMaxEvents.Maximum = 2000;
        nudMaxEvents.Increment = 50;
        nudMaxEvents.ValueChanged += nudMaxEvents_ValueChanged;

        lblPowerPlan.Dock = DockStyle.Fill;
        lblPowerPlan.Text = "Plan zasilania:";
        lblPowerPlan.TextAlign = ContentAlignment.MiddleLeft;

        cmbPowerPlan.Dock = DockStyle.Fill;
        cmbPowerPlan.DropDownStyle = ComboBoxStyle.DropDownList;

        btnApplyPowerPlan.Dock = DockStyle.Fill;
        btnApplyPowerPlan.Text = "Ustaw plan zasilania";
        btnApplyPowerPlan.UseVisualStyleBackColor = true;
        btnApplyPowerPlan.Click += btnApplyPowerPlan_Click;

        lblOptionsNote.Dock = DockStyle.Fill;
        lblOptionsNote.Text = "Beta testy.";
        lblOptionsNote.TextAlign = ContentAlignment.MiddleLeft;

        layoutOptions.Controls.Add(lblVersion, 0, 0);
        layoutOptions.Controls.Add(linkAuthor, 1, 0);
        layoutOptions.Controls.Add(btnCheckUpdates, 0, 1);
        layoutOptions.SetColumnSpan(btnCheckUpdates, 2);
        layoutOptions.Controls.Add(chkCollectEvents, 0, 2);
        layoutOptions.SetColumnSpan(chkCollectEvents, 2);
        layoutOptions.Controls.Add(chkCollectTemps, 0, 3);
        layoutOptions.SetColumnSpan(chkCollectTemps, 2);
        layoutOptions.Controls.Add(chkCollectSmart, 0, 4);
        layoutOptions.SetColumnSpan(chkCollectSmart, 2);
        layoutOptions.Controls.Add(lblEventDays, 0, 5);
        layoutOptions.Controls.Add(nudEventDays, 1, 5);
        layoutOptions.Controls.Add(lblMaxEvents, 0, 6);
        layoutOptions.Controls.Add(nudMaxEvents, 1, 6);
        layoutOptions.Controls.Add(lblPowerPlan, 0, 7);
        layoutOptions.Controls.Add(cmbPowerPlan, 1, 7);
        layoutOptions.Controls.Add(btnApplyPowerPlan, 0, 8);
        layoutOptions.SetColumnSpan(btnApplyPowerPlan, 2);
        layoutOptions.Controls.Add(lblOptionsNote, 0, 9);
        layoutOptions.SetColumnSpan(lblOptionsNote, 2);

        statusStrip.SizingGrip = false;
        statusStrip.Items.AddRange(new ToolStripItem[] { statusAuthor, statusVersion });

        statusAuthor.IsLink = true;
        statusAuthor.Text = "dziondzio.xyz";
        statusAuthor.Click += statusAuthor_Click;

        statusVersion.Text = "v";

        layoutRoot.Controls.Add(layoutTop, 0, 0);
        layoutRoot.Controls.Add(tabs, 0, 1);
        layoutRoot.Controls.Add(statusStrip, 0, 2);

        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1100, 700);
        Controls.Add(layoutRoot);
        MinimumSize = new Size(980, 620);
        Text = "DIPC - Info o komputerze";
        StartPosition = FormStartPosition.CenterScreen;
        Load += Form1_Load;

        layoutRoot.ResumeLayout(false);
        layoutTop.ResumeLayout(false);
        layoutTop.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)gridSummary).EndInit();
        tabs.ResumeLayout(false);
        tabSummary.ResumeLayout(false);
        tabCpu.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)gridCpu).EndInit();
        tabGpu.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)gridGpus).EndInit();
        tabRam.ResumeLayout(false);
        splitRam.Panel1.ResumeLayout(false);
        splitRam.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitRam).EndInit();
        splitRam.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)gridRam).EndInit();
        ((System.ComponentModel.ISupportInitialize)gridRamModules).EndInit();
        tabMotherboard.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)gridMotherboard).EndInit();
        tabDisks.ResumeLayout(false);
        splitDisks.Panel1.ResumeLayout(false);
        splitDisks.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitDisks).EndInit();
        splitDisks.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)gridDiskDrives).EndInit();
        ((System.ComponentModel.ISupportInitialize)gridLogicalDisks).EndInit();
        tabBios.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)gridBios).EndInit();
        tabSystem.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)gridSystem).EndInit();
        tabNetwork.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)gridNetwork).EndInit();
        tabDisplays.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)gridScreens).EndInit();
        tabEvents.ResumeLayout(false);
        layoutEvents.ResumeLayout(false);
        layoutEvents.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)gridEvents).EndInit();
        tabTemps.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)gridTemps).EndInit();
        tabOptions.ResumeLayout(false);
        layoutOptions.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)nudEventDays).EndInit();
        ((System.ComponentModel.ISupportInitialize)nudMaxEvents).EndInit();
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
    }

    #endregion

    private TableLayoutPanel layoutRoot;
    private TableLayoutPanel layoutTop;
    private Button btnRefresh;
    private Button btnExport;
    private Button btnImport;
    private Label lblStatus;
    private TabControl tabs;
    private TabPage tabSummary;
    private TabPage tabCpu;
    private TabPage tabGpu;
    private TabPage tabRam;
    private TabPage tabMotherboard;
    private TabPage tabDisks;
    private TabPage tabBios;
    private TabPage tabSystem;
    private TabPage tabNetwork;
    private TabPage tabDisplays;
    private TabPage tabEvents;
    private TabPage tabTemps;
    private TabPage tabOptions;
    private DataGridView gridSummary;
    private DataGridView gridCpu;
    private DataGridView gridGpus;
    private SplitContainer splitRam;
    private DataGridView gridRam;
    private DataGridView gridRamModules;
    private DataGridView gridMotherboard;
    private SplitContainer splitDisks;
    private DataGridView gridDiskDrives;
    private DataGridView gridLogicalDisks;
    private DataGridView gridBios;
    private DataGridView gridSystem;
    private DataGridView gridNetwork;
    private DataGridView gridScreens;
    private TableLayoutPanel layoutEvents;
    private Label lblEventFilter;
    private TextBox txtEventFilter;
    private ComboBox cmbEventLevel;
    private ComboBox cmbEventLog;
    private Button btnEventCopySelected;
    private Button btnEventCopyAll;
    private Button btnEventClearLog;
    private DataGridView gridEvents;
    private DataGridView gridTemps;
    private TableLayoutPanel layoutOptions;
    private LinkLabel linkAuthor;
    private Label lblVersion;
    private Button btnCheckUpdates;
    private CheckBox chkCollectEvents;
    private CheckBox chkCollectTemps;
    private CheckBox chkCollectSmart;
    private Label lblEventDays;
    private NumericUpDown nudEventDays;
    private Label lblMaxEvents;
    private NumericUpDown nudMaxEvents;
    private Label lblPowerPlan;
    private ComboBox cmbPowerPlan;
    private Button btnApplyPowerPlan;
    private Label lblOptionsNote;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel statusAuthor;
    private ToolStripStatusLabel statusVersion;
}
