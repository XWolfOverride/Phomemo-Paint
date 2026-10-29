using Phomemo.Printing;
using Phomemo.Printing.Windows;
using PhomemoPaint.Editing;

namespace PhomemoPaint;

internal sealed class MainForm : Form
{
    private readonly AppSettings _settings = AppSettings.Load();
    private readonly IPortInfoProvider _ports = new WindowsPortInfoProvider();
    private readonly History _history = new();
    private readonly CanvasControl _canvas = new() { Dock = DockStyle.Fill };
    private readonly PaintDocument _doc;

    private readonly ToolStripComboBox _portBox = new() { DropDownStyle = ComboBoxStyle.DropDownList, AutoSize = false, Width = 200 };
    private readonly ToolStripComboBox _modelBox = new() { DropDownStyle = ComboBoxStyle.DropDownList, AutoSize = false, Width = 200 };
    private readonly NumericUpDown _labelWidth = new() { Minimum = 10, Maximum = 120, DecimalPlaces = 0, Width = 60 };
    private readonly ToolStripLabel _labelWidthLabel = new(S.LabelWidth);
    private readonly ToolStripControlHost _labelWidthHost;
    private readonly NumericUpDown _height = new() { Minimum = 8, Maximum = PaintDocument.MaxHeight, Width = 70, Increment = 8 };
    private readonly ToolStripButton _printButton = new(S.Print) { DisplayStyle = ToolStripItemDisplayStyle.Text, Font = new Font(SystemFonts.MenuFont!, FontStyle.Bold) };
    private readonly ToolStripButton _blackButton = new(S.Black) { CheckOnClick = false, Checked = true };
    private readonly ToolStripButton _whiteButton = new(S.White) { CheckOnClick = false };
    private readonly NumericUpDown _brush = new() { Minimum = 1, Maximum = 64, Width = 50 };
    private readonly ToolStripComboBox _zoomBox = new() { DropDownStyle = ComboBoxStyle.DropDownList, AutoSize = false, Width = 55 };
    private readonly ToolStripComboBox _conversionBox = new() { DropDownStyle = ComboBoxStyle.DropDownList, AutoSize = false, Width = 90 };
    private readonly ToolStripStatusLabel _cursorLabel = new() { AutoSize = false, Width = 170, TextAlign = ContentAlignment.MiddleLeft };
    private readonly ToolStripStatusLabel _extentLabel = new() { AutoSize = false, Width = 230, TextAlign = ContentAlignment.MiddleLeft };
    private readonly ToolStripStatusLabel _statusLabel = new() { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
    private readonly ToolStripProgressBar _progress = new() { Visible = false, Width = 150 };

    private ToolStripMenuItem _undoItem = null!, _redoItem = null!, _commitItem = null!, _deleteItem = null!;
    private PrinterModel _model;
    private readonly List<PortInfo> _blePorts = [];
    private bool _updating;
    private bool _scanning;
    private CancellationTokenSource? _printCts;

    public MainForm()
    {
        Text = S.AppTitle;
        Size = new Size(940, 900);
        StartPosition = FormStartPosition.CenterScreen;
        KeyPreview = true;
        AllowDrop = true;

        _model = PrinterCatalog.Find(_settings.LastModel) ?? PrinterCatalog.Default;
        _doc = new PaintDocument(CanvasWidth(_model), Math.Clamp(_settings.CanvasHeight, 8, PaintDocument.MaxHeight));
        _canvas.Document = _doc;
        _canvas.DotsPerMm = _model.DotsPerMm;
        _canvas.BrushSize = Math.Clamp(_settings.BrushSize, 1, 64);

        _labelWidthHost = new ToolStripControlHost(_labelWidth);
        BuildUi();
        WireEvents();

        _updating = true;
        _labelWidth.Value = (decimal)Math.Clamp(_settings.LabelWidthMm, 10, 120);
        _height.Value = _doc.Height;
        _brush.Value = _canvas.BrushSize;
        _updating = false;

        RefreshModels();
        RefreshPorts(scan: false);
        _doc.NotifyChanged();
        UpdateState();
    }

    // ------------------------------------------------------------------ construcción

    private void BuildUi()
    {
        var menu = new MenuStrip();
        var file = new ToolStripMenuItem(S.MenuFile);
        file.DropDownItems.AddRange(
        [
            Item(S.MenuNew, Keys.Control | Keys.N, (_, _) => ClearAll()),
            Item(S.MenuOpenCanvas, Keys.Control | Keys.O, (_, _) => OpenAsCanvas()),
            Item(S.MenuInsertImage, Keys.Control | Keys.I, (_, _) => InsertImageFromFile()),
            Item(S.MenuExportPng, Keys.Control | Keys.S, (_, _) => ExportPng()),
            new ToolStripSeparator(),
            Item(S.MenuExit, Keys.None, (_, _) => Close()),
        ]);
        var edit = new ToolStripMenuItem(S.MenuEdit);
        _undoItem = Item(S.MenuUndo, Keys.Control | Keys.Z, (_, _) => Undo());
        _redoItem = Item(S.MenuRedo, Keys.Control | Keys.Y, (_, _) => Redo());
        _commitItem = Item(S.MenuCommit, Keys.None, (_, _) => CommitSelected());
        _commitItem.ShortcutKeyDisplayString = "Intro";
        _deleteItem = Item(S.MenuDelete, Keys.None, (_, _) => DeleteSelected());
        _deleteItem.ShortcutKeyDisplayString = S.Spanish ? "Supr" : "Del";
        edit.DropDownItems.AddRange(
        [
            _undoItem, _redoItem, new ToolStripSeparator(),
            Item(S.MenuPaste, Keys.Control | Keys.V, (_, _) => PasteImage()),
            _commitItem,
            Item(S.MenuCommitAll, Keys.None, (_, _) => CommitAll()),
            _deleteItem,
        ]);
        var view = new ToolStripMenuItem(S.MenuView);
        view.DropDownItems.AddRange(
        [
            Item(S.MenuZoomIn, Keys.Control | Keys.Oemplus, (_, _) => _canvas.Zoom++),
            Item(S.MenuZoomOut, Keys.Control | Keys.OemMinus, (_, _) => _canvas.Zoom--),
            Item(S.MenuZoomFit, Keys.Control | Keys.D0, (_, _) => _canvas.Zoom = _canvas.FitZoom(_canvas.ClientSize.Width)),
        ]);
        var printer = new ToolStripMenuItem(S.MenuPrinter);
        printer.DropDownItems.AddRange(
        [
            Item(S.MenuPrint, Keys.Control | Keys.P, (_, _) => PrintOrCancel()),
            Item(S.MenuDetect, Keys.None, (_, _) => DetectModel(interactive: true)),
            new ToolStripSeparator(),
            Item(S.MenuCalibration, Keys.None, (_, _) => DrawCalibration()),
            Item(S.MenuDiagnostics, Keys.None, (_, _) => ShowDiagnostics()),
        ]);
        menu.Items.AddRange([file, edit, view, printer]);

        var refresh = new ToolStripButton("⟳") { ToolTipText = S.Refresh };
        refresh.Click += (_, _) => RefreshPorts(scan: true);
        var printerBar = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, Padding = new Padding(4, 2, 4, 2) };
        printerBar.Items.AddRange(
        [
            new ToolStripLabel(S.Port), _portBox, refresh, new ToolStripSeparator(),
            new ToolStripLabel(S.Model), _modelBox, _labelWidthLabel, _labelWidthHost, new ToolStripSeparator(),
            new ToolStripLabel(S.CanvasHeight), new ToolStripControlHost(_height), new ToolStripSeparator(),
            _printButton,
        ]);

        for (int z = 1; z <= 8; z++) _zoomBox.Items.Add($"×{z}");
        _conversionBox.Items.AddRange([S.Dither, S.Threshold]);
        var toolsBar = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, Padding = new Padding(4, 2, 4, 2) };
        toolsBar.Items.AddRange(
        [
            _blackButton, _whiteButton, new ToolStripLabel(S.BrushSize), new ToolStripControlHost(_brush),
            new ToolStripSeparator(),
            new ToolStripLabel(S.Zoom), _zoomBox,
            new ToolStripSeparator(),
            Button(S.MenuInsertImage.Replace("&", "").TrimEnd('.'), (_, _) => InsertImageFromFile()),
            Button(S.MenuCommit.Replace("&", ""), (_, _) => CommitSelected()),
            Button(S.MenuDelete.Replace("&", ""), (_, _) => DeleteSelected()),
            new ToolStripLabel(S.Conversion), _conversionBox,
        ]);

        var status = new StatusStrip();
        status.Items.AddRange([_cursorLabel, _extentLabel, _statusLabel, _progress]);

        Controls.Add(_canvas);
        Controls.Add(toolsBar);
        Controls.Add(printerBar);
        Controls.Add(menu);
        Controls.Add(status);
        MainMenuStrip = menu;
    }

    private static ToolStripMenuItem Item(string text, Keys keys, EventHandler click)
    {
        var item = new ToolStripMenuItem(text, null, click);
        if (keys != Keys.None) item.ShortcutKeys = keys;
        return item;
    }

    private static ToolStripButton Button(string text, EventHandler click) =>
        new(text, null, click) { DisplayStyle = ToolStripItemDisplayStyle.Text };

    private void WireEvents()
    {
        _canvas.BeforeEdit += (_, _) => _history.Push(_doc);
        _canvas.Edited += (_, _) => DocumentEdited();
        _canvas.SelectionChanged += (_, _) => UpdateState();
        _canvas.ZoomChanged += (_, _) => { _updating = true; _zoomBox.SelectedIndex = _canvas.Zoom - 1; _updating = false; };
        _canvas.CursorMoved += (_, p) => _cursorLabel.Text = p is { } c
            ? S.CursorPos(c.X, c.Y, c.X / _model.DotsPerMm, c.Y / _model.DotsPerMm) : "";
        _doc.Changed += (_, _) => UpdateExtent();

        _portBox.SelectedIndexChanged += (_, _) => { if (!_updating) PortChanged(); };
        _modelBox.SelectedIndexChanged += (_, _) => { if (!_updating) ModelChanged(); };
        _labelWidth.ValueChanged += (_, _) => { if (!_updating) LabelWidthChanged(); };
        _height.ValueChanged += (_, _) => { if (!_updating) HeightChanged(); };
        _brush.ValueChanged += (_, _) => _canvas.BrushSize = (int)_brush.Value;
        _zoomBox.SelectedIndexChanged += (_, _) => { if (!_updating && _zoomBox.SelectedIndex >= 0) _canvas.Zoom = _zoomBox.SelectedIndex + 1; };
        _conversionBox.SelectedIndexChanged += (_, _) => { if (!_updating) ConversionChanged(); };
        _blackButton.Click += (_, _) => SetBrushColor(true);
        _whiteButton.Click += (_, _) => SetBrushColor(false);
        _printButton.Click += (_, _) => PrintOrCancel();

        Shown += (_, _) =>
        {
            _canvas.Zoom = _canvas.FitZoom(_canvas.ClientSize.Width);
            _zoomBox.SelectedIndex = _canvas.Zoom - 1;
            _canvas.Focus();
            _ = ScanBluetoothAsync();
        };
        FormClosing += (_, _) => SaveSettings();
        DragEnter += (_, e) => e.Effect = e.Data?.GetDataPresent(DataFormats.FileDrop) == true ? DragDropEffects.Copy : DragDropEffects.None;
        DragDrop += (_, e) =>
        {
            if (e.Data?.GetData(DataFormats.FileDrop) is string[] { Length: > 0 } files)
                InsertImage(files[0]);
        };
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        // Intro/Supr/Esc solo actúan sobre el lienzo, para no robárselas a los campos numéricos.
        if (ActiveControl == _canvas)
        {
            switch (keyData)
            {
                case Keys.Enter: CommitSelected(); return true;
                case Keys.Delete: DeleteSelected(); return true;
                case Keys.Escape: Deselect(); return true;
            }
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }

    // ------------------------------------------------------------------ impresora

    private int CanvasWidth(PrinterModel model) =>
        model.WidthDependsOnMedia ? model.DotsForMediaWidth(_settings.LabelWidthMm) : model.HeadWidthDots;

    private static string ModelText(PrinterModel m)
    {
        var tags = new List<string>();
        if (!m.Verified) tags.Add(S.NotVerified);
        if (m.Experimental) tags.Add(S.Experimental);
        return tags.Count == 0 ? m.DisplayName : $"{m.DisplayName} ({string.Join(", ", tags)})";
    }

    private void RefreshModels()
    {
        _updating = true;
        _modelBox.Items.Clear();
        foreach (var m in PrinterCatalog.All)
            _modelBox.Items.Add(new ModelItem(m));
        SelectModelInBox(_model);
        _updating = false;
        UpdateLabelWidthVisibility();
    }

    private void SelectModelInBox(PrinterModel model)
    {
        for (int i = 0; i < _modelBox.Items.Count; i++)
            if (((ModelItem)_modelBox.Items[i]!).Model == model)
                _modelBox.SelectedIndex = i;
    }

    /// <summary>Recarga la lista (COM + Bluetooth conocidos) y, con <paramref name="scan"/>, busca por Bluetooth.</summary>
    private void RefreshPorts(bool scan)
    {
        if (IsDisposed) return;
        var current = (_portBox.SelectedItem as PortInfo)?.Name ?? _settings.LastPort;
        var ports = AllPorts();
        _updating = true;
        _portBox.Items.Clear();
        foreach (var p in ports) _portBox.Items.Add(p);
        _updating = false;

        if (ports.Count == 0)
        {
            _portBox.Text = "";
            _statusLabel.Text = S.NoPorts;
        }
        else
        {
            // Preferencia: el puerto anterior; si no está, una impresora Bluetooth reconocible;
            // si no, el primer puerto con una impresora reconocible.
            var pick = ports.FirstOrDefault(p => string.Equals(p.Name, current, StringComparison.OrdinalIgnoreCase))
                       ?? ports.FirstOrDefault(p => p.Kind == PortKind.BluetoothLE && ModelDetector.Detect(p).Model is not null)
                       ?? ModelDetector.FindFirst(ports)?.Port
                       ?? ports[0];
            _portBox.SelectedItem = pick; // dispara PortChanged
        }
        if (scan) _ = ScanBluetoothAsync();
    }

    private List<PortInfo> AllPorts()
    {
        var list = _ports.GetPorts().ToList();
        var ble = new Dictionary<string, PortInfo>(StringComparer.OrdinalIgnoreCase);
        foreach (var (address, name) in _settings.KnownBluetooth)
            ble[address] = PortInfo.BluetoothLE(address, string.IsNullOrEmpty(name) ? null : name);
        foreach (var p in _blePorts)
            ble[p.Address!] = p;
        list.AddRange(ble.Values.OrderBy(p => p.BluetoothName, StringComparer.OrdinalIgnoreCase));
        return list;
    }

    private async Task ScanBluetoothAsync()
    {
        if (_scanning) return;
        _scanning = true;
        _statusLabel.Text = S.Scanning;
        try
        {
            var found = await BleScanner.ScanAsync(TimeSpan.FromSeconds(5));
            if (IsDisposed) return;
            _blePorts.Clear();
            _blePorts.AddRange(found);
            foreach (var p in found)
                _settings.KnownBluetooth[p.Address!] = p.BluetoothName ?? "";
            RefreshPorts(scan: false);
            _statusLabel.Text = S.ScanDone(found.Count);

            // La misma impresora por USB y por Bluetooth: se prefiere Bluetooth (la T02 no
            // imprime por USB).
            if (_portBox.SelectedItem is PortInfo { Kind: PortKind.Serial } selected &&
                ModelDetector.Detect(selected).Model is { } model &&
                _portBox.Items.OfType<PortInfo>().FirstOrDefault(p => p.Kind == PortKind.BluetoothLE &&
                    found.Any(f => f.Address == p.Address) && ModelDetector.Detect(p).Model == model) is { } blePort)
            {
                _portBox.SelectedItem = blePort;
                _statusLabel.Text = S.SwitchedToBluetooth(model.DisplayName);
            }
        }
        catch (Exception e) when (e is not OutOfMemoryException)
        {
            if (!IsDisposed) _statusLabel.Text = S.ScanFailed(e.Message);
        }
        finally
        {
            _scanning = false;
        }
    }

    private void PortChanged()
    {
        if (_portBox.SelectedItem is not PortInfo port) return;
        _settings.LastPort = port.Name;
        DetectModel(interactive: false);
    }

    /// <summary>
    /// Modelo del puerto: por VID/PID o nombre Bluetooth; si no, el que se usó la última vez en
    /// ese puerto.
    /// </summary>
    private void DetectModel(bool interactive)
    {
        if (_portBox.SelectedItem is not PortInfo port)
        {
            if (interactive) MessageBox.Show(this, S.ChoosePort, S.AppTitle);
            return;
        }
        var result = ModelDetector.Detect(port);
        PrinterModel? model = result.Model;
        if (model is not null)
        {
            _statusLabel.Text = S.Detected(model.DisplayName, result.Source == DetectionSource.UsbId
                ? S.DetectedByUsb(port.UsbVendorId!.Value, port.UsbProductId!.Value) : S.DetectedByBluetooth);
        }
        else
        {
            if (_settings.ModelByPort.TryGetValue(port.Name, out var id)) model = PrinterCatalog.Find(id);
            _statusLabel.Text = S.NotDetected;
            if (interactive) MessageBox.Show(this, S.NotDetected, S.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        if (model is not null && model != _model)
            ApplyModel(model);
    }

    private void ModelChanged()
    {
        if (_modelBox.SelectedItem is not ModelItem item || item.Model == _model) return;
        if (!ApplyModel(item.Model))
        {
            _updating = true;
            SelectModelInBox(_model);
            _updating = false;
        }
    }

    /// <summary>Cambia de modelo avisando si el ancho cambia y hay algo dibujado.</summary>
    private bool ApplyModel(PrinterModel model)
    {
        int newWidth = model.WidthDependsOnMedia ? model.DotsForMediaWidth((double)_labelWidth.Value) : model.HeadWidthDots;
        if (!ConfirmWidth(model, newWidth)) return false;
        _model = model;
        _canvas.DotsPerMm = model.DotsPerMm;
        _settings.LastModel = model.Id;
        if (_portBox.SelectedItem is PortInfo port) _settings.ModelByPort[port.Name] = model.Id;
        _updating = true;
        SelectModelInBox(model);
        _updating = false;
        UpdateLabelWidthVisibility();
        ResizeCanvas(newWidth, _doc.Height);
        return true;
    }

    private bool ConfirmWidth(PrinterModel model, int newWidth)
    {
        if (newWidth == _doc.Width || !_doc.HasInk()) return true;
        var answer = MessageBox.Show(this,
            S.WidthChange(model.DisplayName, newWidth, newWidth / model.DotsPerMm, _doc.Width, newWidth < _doc.Width),
            S.AppTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        return answer == DialogResult.Yes;
    }

    private void LabelWidthChanged()
    {
        if (!_model.WidthDependsOnMedia) return;
        int newWidth = _model.DotsForMediaWidth((double)_labelWidth.Value);
        if (!ConfirmWidth(_model, newWidth))
        {
            _updating = true;
            _labelWidth.Value = (decimal)Math.Round(_doc.Width / _model.DotsPerMm);
            _updating = false;
            return;
        }
        _settings.LabelWidthMm = (double)_labelWidth.Value;
        ResizeCanvas(newWidth, _doc.Height);
    }

    private void UpdateLabelWidthVisibility()
    {
        _labelWidthLabel.Visible = _labelWidthHost.Visible = _model.WidthDependsOnMedia;
        if (_model.WidthDependsOnMedia)
            _labelWidth.Maximum = (decimal)Math.Floor(_model.HeadWidthMm);
    }

    private void HeightChanged()
    {
        int h = (int)_height.Value;
        if (h < _doc.Height)
        {
            var mono = _doc.ToMono();
            bool cut = false;
            for (int y = h; y < mono.Height && !cut; y++)
                cut = mono.Row(y).IndexOfAnyExcept((byte)0) >= 0;
            if (cut && MessageBox.Show(this, S.HeightCut(h), S.AppTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                _updating = true;
                _height.Value = _doc.Height;
                _updating = false;
                return;
            }
        }
        _settings.CanvasHeight = h;
        ResizeCanvas(_doc.Width, h);
    }

    private void ResizeCanvas(int width, int height)
    {
        if (width == _doc.Width && height == _doc.Height) return;
        _history.Push(_doc);
        _doc.Resize(width, height);
        _canvas.UpdateScrollSize();
        DocumentEdited();
    }

    // ------------------------------------------------------------------ imprimir

    private async void PrintOrCancel()
    {
        if (_printCts is not null)
        {
            _printCts.Cancel();
            return;
        }
        if (_portBox.SelectedItem is not PortInfo port)
        {
            MessageBox.Show(this, S.ChoosePort, S.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        var image = _doc.ToPrintImage();
        if (image is null)
        {
            MessageBox.Show(this, S.NothingToPrint, S.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        _printCts = new CancellationTokenSource();
        _printButton.Text = S.Cancel;
        _progress.Value = 0;
        _progress.Visible = true;
        _statusLabel.Text = S.Printing;
        var progress = new Progress<double>(v => _progress.Value = (int)Math.Round(v * 100));
        try
        {
            await using var transport = Transports.Create(port);
            var printer = new PhomemoPrinter(transport, _model);
            var result = await printer.PrintAsync(image, PrintOptions.Default, progress, _printCts.Token);
            double mm = image.Height / _model.DotsPerMm;
            var summary = DiagnosticsForm.Describe(result.Response);
            _statusLabel.Text = result.Finished
                ? S.Printed(image.Height, mm, summary)
                : S.Sent(image.Height, mm, summary) + (port.Kind == PortKind.Serial ? S.UsbHint(_model.DisplayName) : "");
        }
        catch (OperationCanceledException)
        {
            _statusLabel.Text = S.PrintCancelled;
        }
        catch (UnauthorizedAccessException)
        {
            _statusLabel.Text = S.PortBusy(port.Name);
            MessageBox.Show(this, S.PrintError(S.PortBusy(port.Name)), S.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        // Bluetooth (WinRT) puede lanzar COMException y otras; cualquier fallo se muestra.
        catch (Exception e) when (e is not OutOfMemoryException)
        {
            _statusLabel.Text = e.Message;
            MessageBox.Show(this, S.PrintError(e.Message), S.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _printCts.Dispose();
            _printCts = null;
            _printButton.Text = S.Print;
            _progress.Visible = false;
        }
    }

    private void ShowDiagnostics()
    {
        if (_portBox.SelectedItem is not PortInfo port)
        {
            MessageBox.Show(this, S.ChoosePort, S.AppTitle);
            return;
        }
        using var form = new DiagnosticsForm(port, _model);
        form.ShowDialog(this);
    }

    private void DrawCalibration()
    {
        if (_doc.HasInk() && MessageBox.Show(this, S.ConfirmCalibration, S.AppTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;
        _history.Push(_doc);
        _doc.Objects.Clear();
        _doc.Selected = null;
        CalibrationPattern.Draw(_doc.Layer, _model.DotsPerMm, S.CalibrationTitle(_model.DisplayName, _doc.Width, _model.Dpi));
        DocumentEdited();
    }

    // ------------------------------------------------------------------ edición

    private void SetBrushColor(bool black)
    {
        _canvas.BrushBlack = black;
        _blackButton.Checked = black;
        _whiteButton.Checked = !black;
    }

    private void DocumentEdited()
    {
        _doc.NotifyChanged();
        UpdateState();
        _canvas.Invalidate();
    }

    private void UpdateExtent() =>
        _extentLabel.Text = S.PrintExtent(_doc.PrintHeight, _doc.PrintHeight / _model.DotsPerMm);

    private void UpdateState()
    {
        _undoItem.Enabled = _history.CanUndo;
        _redoItem.Enabled = _history.CanRedo;
        bool sel = _doc.Selected is not null;
        _commitItem.Enabled = _deleteItem.Enabled = _conversionBox.Enabled = sel;
        _updating = true;
        _conversionBox.SelectedIndex = sel ? (int)_doc.Selected!.Conversion : -1;
        _updating = false;
        UpdateExtent();
    }

    private void Undo()
    {
        if (!_history.Undo(_doc)) return;
        SyncAfterRestore();
    }

    private void Redo()
    {
        if (!_history.Redo(_doc)) return;
        SyncAfterRestore();
    }

    /// <summary>Tras deshacer, el tamaño del lienzo puede ser otro.</summary>
    private void SyncAfterRestore()
    {
        _updating = true;
        _height.Value = Math.Clamp(_doc.Height, (int)_height.Minimum, (int)_height.Maximum);
        _updating = false;
        _canvas.UpdateScrollSize();
        DocumentEdited();
    }

    private void ClearAll()
    {
        if (_doc.HasInk() && MessageBox.Show(this, S.ConfirmClear, S.AppTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;
        _history.Push(_doc);
        _doc.Clear();
        DocumentEdited();
    }

    private void CommitSelected()
    {
        if (_doc.Selected is not { } obj) return;
        _history.Push(_doc);
        _doc.Commit(obj);
        DocumentEdited();
    }

    private void CommitAll()
    {
        if (_doc.Objects.Count == 0) return;
        _history.Push(_doc);
        _doc.CommitAll();
        DocumentEdited();
    }

    private void DeleteSelected()
    {
        if (_doc.Selected is not { } obj) return;
        _history.Push(_doc);
        _doc.Objects.Remove(obj);
        _doc.Selected = null;
        DocumentEdited();
    }

    private void Deselect()
    {
        if (_doc.Selected is null) return;
        _doc.Selected = null;
        UpdateState();
        _canvas.Invalidate();
    }

    private void ConversionChanged()
    {
        if (_doc.Selected is not { } obj || _conversionBox.SelectedIndex < 0) return;
        _history.Push(_doc);
        obj.Conversion = (MonoConversion)_conversionBox.SelectedIndex;
        obj.Invalidate();
        DocumentEdited();
    }

    private void PasteImage()
    {
        Image? img = null;
        try
        {
            if (Clipboard.ContainsImage()) img = Clipboard.GetImage();
            else if (Clipboard.ContainsFileDropList() && Clipboard.GetFileDropList() is { Count: > 0 } files)
            {
                InsertImage(files[0]!);
                return;
            }
        }
        catch (System.Runtime.InteropServices.ExternalException)
        {
        }
        if (img is null)
        {
            MessageBox.Show(this, S.NoImageInClipboard, S.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        using (img)
            AddObject(Imaging.CopyArgb(img));
    }

    private void InsertImageFromFile()
    {
        using var dlg = new OpenFileDialog { Filter = S.ImageFilter };
        if (dlg.ShowDialog(this) == DialogResult.OK)
            InsertImage(dlg.FileName);
    }

    private void InsertImage(string path)
    {
        try
        {
            AddObject(Imaging.Load(path));
        }
        catch (Exception e) when (e is IOException or ArgumentException or OutOfMemoryException or UnauthorizedAccessException)
        {
            MessageBox.Show(this, S.CannotOpen(e.Message), S.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>Nueva imagen flotante arriba del área visible, reducida si no cabe a lo ancho.</summary>
    private void AddObject(Bitmap source)
    {
        float scale = Math.Min(1f, _doc.Width / (float)source.Width);
        var size = new SizeF((float)Math.Max(1, Math.Round(source.Width * scale)), (float)Math.Max(1, Math.Round(source.Height * scale)));
        float top = _canvas.VisibleTopRow;
        var center = new PointF((float)Math.Round(_doc.Width / 2f), (float)Math.Round(top + size.Height / 2));
        _history.Push(_doc);
        var obj = new ImageObject(source, center, size);
        _doc.Objects.Add(obj);
        _doc.Selected = obj;
        DocumentEdited();
        _canvas.Focus();
    }

    private void OpenAsCanvas()
    {
        using var dlg = new OpenFileDialog { Filter = S.ImageFilter };
        if (dlg.ShowDialog(this) != DialogResult.OK) return;
        Bitmap src;
        try
        {
            src = Imaging.Load(dlg.FileName);
        }
        catch (Exception e) when (e is IOException or ArgumentException or OutOfMemoryException or UnauthorizedAccessException)
        {
            MessageBox.Show(this, S.CannotOpen(e.Message), S.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        using (src)
        {
            if (src.Width > _doc.Width)
                MessageBox.Show(this, S.CanvasTooWide(src.Width, _doc.Width), S.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
            _history.Push(_doc);
            // Sin escalar: mismo ancho de lienzo, el alto pasa a ser el de la imagen.
            int height = Math.Clamp(src.Height, 8, PaintDocument.MaxHeight);
            Imaging.ToBlackAndWhite(src, MonoConversion.Threshold);
            var layer = Imaging.NewLayer(_doc.Width, height);
            using (var g = Graphics.FromImage(layer))
                g.DrawImageUnscaled(src, 0, 0);
            _doc.Replace(layer);
        }
        SyncAfterRestore();
    }

    private void ExportPng()
    {
        using var dlg = new SaveFileDialog { Filter = S.PngFilter, FileName = "phomemo.png" };
        if (dlg.ShowDialog(this) != DialogResult.OK) return;
        var image = _doc.ToPrintImage() ?? new MonoBitmap(_doc.Width, 1);
        Imaging.SaveMonoPng(image, dlg.FileName);
    }

    private void SaveSettings()
    {
        _settings.BrushSize = _canvas.BrushSize;
        _settings.CanvasHeight = _doc.Height;
        _settings.LastModel = _model.Id;
        _settings.Save();
    }

    private sealed record ModelItem(PrinterModel Model)
    {
        public override string ToString() => ModelText(Model);
    }
}
