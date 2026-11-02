using Phomemo.Printing;
using Phomemo.Printing.Windows;
using PhomemoPaint.Editing;

namespace PhomemoPaint;

/// <summary>
/// Ventana principal. Los controles y su disposición están en MainForm.Designer.cs (editable con el
/// diseñador de Windows Forms); aquí, los textos en el idioma de la aplicación, los eventos y la
/// lógica.
/// </summary>
internal sealed partial class MainForm : Form
{
    private readonly AppSettings _settings = AppSettings.Load();
    private readonly IPortInfoProvider _ports = new WindowsPortInfoProvider();
    private readonly History _history = new();
    private readonly PaintDocument _doc;

    // Alto del lienzo: flota junto a la marca de fin del lienzo y lo coloca CanvasControl, por eso no
    // está en el diseñador.
    private readonly NumericUpDown _height = new() { Minimum = 8, Maximum = PaintDocument.MaxHeight, Width = 70, Increment = 8 };

    // Iconos del botón Imprimir (fuente Segoe MDL2 Assets): imprimir y, durante la impresión, cancelar.
    private const string PrintGlyph = "";
    private const string CancelGlyph = "";

    private string? _cutNotice;
    private PrinterModel _model;
    private readonly List<PortInfo> _blePorts = [];
    private bool _updating;
    private bool _scanning;
    private bool _textBoxPushed;
    private const int MinTextWidth = 64;
    private TextContent _lastText = new("", DefaultFont.FontFamily.Name, 14, FontStyle.Regular, TextAlignment.Left, true, 8);
    private CancellationTokenSource? _printCts;

    // Panel de la impresora: se consulta al seleccionarla y la batería cada 5 minutos. El cerrojo
    // evita que consulta, impresión y diagnóstico usen la conexión a la vez.
    private readonly SemaphoreSlim _printerLock = new(1, 1);
    private readonly System.Windows.Forms.Timer _batteryTimer = new() { Interval = 5 * 60 * 1000 };
    private PrinterInfo? _info;
    private DateTime? _infoUpdated;
    private string? _infoError;
    private string? _infoPortName;

    public MainForm()
    {
        InitializeComponent();
        Icon = AppIcon.Icon;
        ApplyTexts();
        StyleSwatch(_blackButton);
        StyleSwatch(_whiteButton);
        // Fuentes en código: el diseñador las encoge al guardar en una pantalla escalada.
        _printerTitle.Font = _propertiesTitle.Font = new Font(Font, FontStyle.Bold);
        _brush.Font = new Font(Font.FontFamily, 8F);
        foreach (var family in FontFamily.Families)
            _fontBox.Items.Add(family.Name);

        _model = PrinterCatalog.Find(_settings.LastModel) ?? PrinterCatalog.Default;
        _doc = new PaintDocument(CanvasWidth(_model), Math.Clamp(_settings.CanvasHeight, 8, PaintDocument.MaxHeight));
        _canvas.Document = _doc;
        _canvas.DotsPerMm = _model.DotsPerMm;
        _canvas.BrushSize = Math.Clamp(_settings.BrushSize, 1, 64);
        _canvas.AttachHeightBox(_height);
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

    private static string PrintTip => S.Print + " (Ctrl+P)";

    /// <summary>
    /// Textos en el idioma de la aplicación. El diseñador guarda los españoles para que la ventana se
    /// vea completa al editarla; aquí se sustituyen todos.
    /// </summary>
    private void ApplyTexts()
    {
        static string Plain(string menuText) => menuText.Replace("&", "").TrimEnd('.');
        string enter = S.Spanish ? "Intro" : "Enter";
        string del = S.Spanish ? "Supr" : "Del";

        Text = S.AppTitle;
        _fileMenu.Text = S.MenuFile;
        _newItem.Text = S.MenuNew;
        _openCanvasItem.Text = S.MenuOpenCanvas;
        _insertItem.Text = S.MenuInsertImage;
        _insertTextItem.Text = S.MenuInsertText;
        _exportItem.Text = S.MenuExportPng;
        _exitItem.Text = S.MenuExit;
        _editMenu.Text = S.MenuEdit;
        _undoItem.Text = S.MenuUndo;
        _redoItem.Text = S.MenuRedo;
        _pasteItem.Text = S.MenuPaste;
        _commitItem.Text = S.MenuCommit;
        _commitItem.ShortcutKeyDisplayString = enter;
        _commitAllItem.Text = S.MenuCommitAll;
        _deleteItem.Text = S.MenuDelete;
        _deleteItem.ShortcutKeyDisplayString = del;
        _viewMenu.Text = S.MenuView;
        _zoomInItem.Text = S.MenuZoomIn;
        _zoomOutItem.Text = S.MenuZoomOut;
        _zoomFitItem.Text = S.MenuZoomFit;
        _printerMenu.Text = S.MenuPrinter;
        _printItem.Text = S.MenuPrint;
        _detectItem.Text = S.MenuDetect;
        _calibrationItem.Text = S.MenuCalibration;
        _diagnosticsItem.Text = S.MenuDiagnostics;
        _helpMenu.Text = S.MenuHelp;
        _helpItem.Text = S.MenuHelpContents;
        _tipsItem.Text = S.MenuHelpTips;
        _problemsItem.Text = S.MenuHelpProblems;
        _aboutItem.Text = S.MenuAbout;
        _propertiesTitle.Text = S.PropertiesTitle;
        _printerTitle.Text = S.InfoTitle;
        _conversionLabel.Text = S.ConversionLong;
        _labelWidthLabel.Text = S.LabelWidth;
        _noSelection.Text = S.NoSelection;
        _conversionBox.Items[0] = S.Dither;
        _conversionBox.Items[1] = S.Threshold;

        _tips.SetToolTip(_brushButton, S.ToolBrush);
        _tips.SetToolTip(_textButton, S.ToolText + " (Ctrl+T)");
        _tips.SetToolTip(_cursorButton, S.ToolCursor + " (V)");
        _tips.SetToolTip(_insertButton, Plain(S.MenuInsertImage) + " (Ctrl+I)");
        _tips.SetToolTip(_pasteButton, Plain(S.MenuPaste) + " (Ctrl+V)");
        _tips.SetToolTip(_commitButton, $"{Plain(S.MenuCommit)} ({enter})");
        _tips.SetToolTip(_deleteButton, $"{Plain(S.MenuDelete)} ({del})");
        _tips.SetToolTip(_printButton, PrintTip);
        _tips.SetToolTip(_brush, S.BrushSize);
        _tips.SetToolTip(_blackButton, S.Black);
        _tips.SetToolTip(_whiteButton, S.White);
        _tips.SetToolTip(_refreshButton, S.Refresh);
        _tips.SetToolTip(_zoomOutButton, Plain(S.MenuZoomOut) + " (Ctrl+−)");
        _tips.SetToolTip(_zoomInButton, Plain(S.MenuZoomIn) + " (Ctrl++)");
        _tips.SetToolTip(_zoomFitButton, Plain(S.MenuZoomFit) + " (Ctrl+0)");
        _tips.SetToolTip(_fontBox, S.FontFamilyTip);
        _tips.SetToolTip(_fontSizeBox, S.FontSizeTip);
        _tips.SetToolTip(_boldButton, S.Bold);
        _tips.SetToolTip(_italicButton, S.Italic);
        _tips.SetToolTip(_underlineButton, S.Underline);
        _tips.SetToolTip(_alignLeftButton, S.AlignLeft);
        _tips.SetToolTip(_alignCenterButton, S.AlignCenter);
        _tips.SetToolTip(_alignRightButton, S.AlignRight);
    }

    /// <summary>Muestra de color (negro / blanco): borde azul grueso cuando está elegida.</summary>
    private static void StyleSwatch(RadioButton swatch)
    {
        void Border() =>
            (swatch.FlatAppearance.BorderSize, swatch.FlatAppearance.BorderColor) =
                swatch.Checked ? (3, Color.DodgerBlue) : (1, SystemColors.ControlDark);
        swatch.CheckedChanged += (_, _) => Border();
        Border();
    }

    private void WireEvents()
    {
        // Menús y botones.
        _newItem.Click += (_, _) => ClearAll();
        _openCanvasItem.Click += (_, _) => OpenAsCanvas();
        _insertItem.Click += (_, _) => InsertImageFromFile();
        _insertButton.Click += (_, _) => InsertImageFromFile();
        _exportItem.Click += (_, _) => ExportPng();
        _exitItem.Click += (_, _) => Close();
        _undoItem.Click += (_, _) => Undo();
        _redoItem.Click += (_, _) => Redo();
        _pasteItem.Click += (_, _) => PasteImage();
        _pasteButton.Click += (_, _) => PasteImage();
        _commitItem.Click += (_, _) => CommitSelected();
        _commitButton.Click += (_, _) => CommitSelected();
        _commitAllItem.Click += (_, _) => CommitAll();
        _deleteItem.Click += (_, _) => DeleteSelected();
        _deleteButton.Click += (_, _) => DeleteSelected();
        _zoomInItem.Click += (_, _) => _canvas.Zoom++;
        _zoomInButton.Click += (_, _) => _canvas.Zoom++;
        _zoomOutItem.Click += (_, _) => _canvas.Zoom--;
        _zoomOutButton.Click += (_, _) => _canvas.Zoom--;
        _zoomFitItem.Click += (_, _) => _canvas.Zoom = _canvas.FitZoom(_canvas.ClientSize.Width);
        _zoomFitButton.Click += (_, _) => _canvas.Zoom = _canvas.FitZoom(_canvas.ClientSize.Width);
        _printItem.Click += (_, _) => PrintOrCancel();
        _printButton.Click += (_, _) => PrintOrCancel();
        _detectItem.Click += (_, _) => DetectModel(interactive: true);
        _calibrationItem.Click += (_, _) => DrawCalibration();
        _diagnosticsItem.Click += (_, _) => ShowDiagnostics();
        _helpItem.Click += (_, _) => HelpViewer.Show(this);
        _tipsItem.Click += (_, _) => HelpViewer.Show(this, "trucos");
        _problemsItem.Click += (_, _) => HelpViewer.Show(this, "problemas");
        _aboutItem.Click += (_, _) => HelpViewer.Show(this, "acerca");
        _brushButton.Click += (_, _) => SetTool(CanvasTool.Brush);
        _textButton.Click += (_, _) => SetTool(CanvasTool.Text);
        _cursorButton.Click += (_, _) => SetTool(CanvasTool.Cursor);
        _insertTextItem.Click += (_, _) => SetTool(CanvasTool.Text);
        _refreshButton.Click += (_, _) => RefreshPorts(scan: true);

        // Lienzo y documento.
        _canvas.BeforeEdit += (_, _) => { _cutNotice = null; _history.Push(_doc); };
        _canvas.Edited += (_, _) => DocumentEdited();
        _canvas.SelectionChanged += (_, _) => { DropEmptyTexts(); UpdateState(); };
        _canvas.TextRequested += (_, p) => AddText(p);
        _canvas.ZoomChanged += (_, _) => { _updating = true; _zoomBox.SelectedIndex = _canvas.Zoom - 1; _updating = false; };
        // Arrastrar la marca de fin del lienzo equivale a escribir el alto: mismos avisos y deshacer.
        _canvas.HeightDragged += (_, h) => _height.Value = Math.Clamp(h, (int)_height.Minimum, (int)_height.Maximum);
        _canvas.CursorMoved += (_, p) => _cursorLabel.Text = p is { } c
            ? S.CursorPos(c.X, c.Y, c.X / _model.DotsPerMm, c.Y / _model.DotsPerMm) : "";
        _doc.Changed += (_, _) => UpdateExtent();

        // Controles.
        _portBox.SelectedIndexChanged += (_, _) => { if (!_updating) PortChanged(); };
        _modelBox.SelectedIndexChanged += (_, _) => { if (!_updating) ModelChanged(); };
        _labelWidth.ValueChanged += (_, _) => { if (!_updating) LabelWidthChanged(); };
        _height.ValueChanged += (_, _) => { if (!_updating) HeightChanged(); };
        _brush.ValueChanged += (_, _) => _canvas.BrushSize = (int)_brush.Value;
        _zoomBox.SelectedIndexChanged += (_, _) => { if (!_updating && _zoomBox.SelectedIndex >= 0) _canvas.Zoom = _zoomBox.SelectedIndex + 1; };
        _conversionBox.SelectedIndexChanged += (_, _) => { if (!_updating) ConversionChanged(); };
        _textBox.TextChanged += (_, _) => { if (!_updating) TextTyped(); };
        _textBox.Enter += (_, _) => _textBoxPushed = false;
        _fontBox.SelectedIndexChanged += (_, _) => { if (!_updating && _fontBox.SelectedItem is string f) ChangeText(t => t with { FontFamily = f }); };
        _fontSizeBox.ValueChanged += (_, _) => { if (!_updating) ChangeText(t => t with { FontSize = (float)_fontSizeBox.Value }); };
        _boldButton.Click += (_, _) => ToggleStyle(FontStyle.Bold);
        _italicButton.Click += (_, _) => ToggleStyle(FontStyle.Italic);
        _underlineButton.Click += (_, _) => ToggleStyle(FontStyle.Underline);
        _alignLeftButton.Click += (_, _) => ChangeText(t => t with { Alignment = TextAlignment.Left });
        _alignCenterButton.Click += (_, _) => ChangeText(t => t with { Alignment = TextAlignment.Center });
        _alignRightButton.Click += (_, _) => ChangeText(t => t with { Alignment = TextAlignment.Right });
        _blackButton.Click += (_, _) => SetBrushColor(true);
        _whiteButton.Click += (_, _) => SetBrushColor(false);

        // Ventana.
        Shown += (_, _) =>
        {
            _canvas.Zoom = _canvas.FitZoom(_canvas.ClientSize.Width);
            _zoomBox.SelectedIndex = _canvas.Zoom - 1;
            _canvas.Focus();
            _ = ScanBluetoothAsync();
        };
        FormClosing += (_, _) => { _batteryTimer.Stop(); SaveSettings(); };
        _infoPanel.RefreshRequested += (_, _) => _ = RefreshInfoAsync(batteryOnly: false);
        _batteryTimer.Tick += (_, _) => _ = RefreshInfoAsync(batteryOnly: true);
        _batteryTimer.Start();
        DragEnter += (_, e) => e.Effect = e.Data?.GetDataPresent(DataFormats.FileDrop) == true ? DragDropEffects.Copy : DragDropEffects.None;
        DragDrop += (_, e) =>
        {
            if (e.Data?.GetData(DataFormats.FileDrop) is string[] { Length: > 0 } files)
                InsertImage(files[0]);
        };
    }

    /// <summary>Sección de la ayuda de la zona bajo el ratón: el primer "help:…" en el Tag, de dentro afuera.</summary>
    private string? HelpSectionAt(Point screen)
    {
        Control? c = this;
        for (var child = c.GetChildAtPoint(c.PointToClient(screen), GetChildAtPointSkip.Invisible); child is not null;
             child = c.GetChildAtPoint(c.PointToClient(screen), GetChildAtPointSkip.Invisible))
            c = child;
        for (; c is not null; c = c.Parent)
            if (c.Tag is string tag && tag.StartsWith("help:", StringComparison.Ordinal))
                return tag[5..];
        return null;
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.F1)
        {
            HelpViewer.Show(this, HelpSectionAt(Cursor.Position));
            return true;
        }
        // En el cuadro de texto, pegar y deshacer son los suyos, no los del menú.
        if (ActiveControl == _textBox)
        {
            if (keyData == Keys.Escape)
            {
                _canvas.Focus();
                return true;
            }
            if (keyData is (Keys.Control | Keys.V) or (Keys.Control | Keys.Z) or (Keys.Control | Keys.Y))
                return false;
        }
        // Intro/Supr/Esc solo actúan sobre el lienzo, para no robárselas a los campos numéricos.
        if (ActiveControl == _canvas)
        {
            switch (keyData)
            {
                case Keys.Enter: CommitSelected(); return true;
                case Keys.Delete: DeleteSelected(); return true;
                case Keys.Escape: Deselect(); return true;
                case Keys.V: SetTool(CanvasTool.Cursor); return true;
                case Keys.B: SetTool(CanvasTool.Brush); return true;
                case Keys.T: SetTool(CanvasTool.Text); return true;
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
        if (m.RequiresCompression) tags.Add(S.NeedsCompression);
        else if (!m.Verified) tags.Add(S.NotVerified);
        if (m.Experimental && !m.RequiresCompression) tags.Add(S.Experimental);
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
            // Una impresora conectada no se anuncia: si la seleccionada es Bluetooth y responde, no
            // se dice que no se ha encontrado nada.
            if (found.Count > 0 || _portBox.SelectedItem is not PortInfo { Kind: PortKind.BluetoothLE } || _info is null)
                _statusLabel.Text = S.ScanDone(found.Count);
            else
                _statusLabel.Text = S.Ready;

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
        // Al recargar la lista se vuelve a seleccionar el mismo puerto: no hay que consultar otra vez.
        if (port.Name == _infoPortName) return;
        _infoPortName = port.Name;
        _settings.LastPort = port.Name;
        DetectModel(interactive: false);
        _info = null;
        _infoUpdated = null;
        _infoError = null;
        UpdateInfoPanel();
        _ = RefreshInfoAsync(batteryOnly: false);
    }

    private void UpdateInfoPanel() =>
        _infoPanel.ShowInfo(_portBox.SelectedItem as PortInfo, _info, _infoUpdated, _infoError);

    /// <summary>
    /// Consulta número de serie, firmware, batería y papel (o solo la batería). Solo a la impresora
    /// seleccionada: al buscar puertos o dispositivos Bluetooth no se manda nada. Si el número de
    /// serie indica otro modelo, se cambia a ese.
    /// </summary>
    private async Task RefreshInfoAsync(bool batteryOnly)
    {
        if (IsDisposed || _portBox.SelectedItem is not PortInfo port) return;
        // La lectura periódica no espera: si la impresora está ocupada se salta.
        if (!await _printerLock.WaitAsync(batteryOnly ? 0 : 10_000)) return;
        _infoPanel.Busy = true;
        try
        {
            await using var transport = Transports.Create(port);
            var info = await new PhomemoPrinter(transport, _model).QueryInfoAsync(batteryOnly);
            if (IsDisposed || !Equals(_portBox.SelectedItem, port)) return;
            if (info.Response.Length == 0)
            {
                _infoError = S.InfoNoAnswer;
                return;
            }
            _info = _info is null ? info : _info.Merge(info);
            _infoUpdated = DateTime.Now;
            _infoError = null;
            if (!batteryOnly && info.ModelFromSerial is { } model && model != _model && ApplyModel(model))
                _statusLabel.Text = S.Detected(model.DisplayName, S.DetectedBySerial);
        }
        catch (Exception) when (!IsDisposed)
        {
            _infoError = S.InfoNoAnswer;
        }
        finally
        {
            _printerLock.Release();
            if (!IsDisposed)
            {
                _infoPanel.Busy = false;
                UpdateInfoPanel();
            }
        }
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

    /// <summary>
    /// El ancho cambia sin preguntar (se puede deshacer); si se recorta dibujo se avisa en la barra
    /// de estado.
    /// </summary>
    private bool ConfirmWidth(PrinterModel model, int newWidth)
    {
        if (newWidth < _doc.Width && _doc.HasInk())
            _cutNotice = S.WidthCutNotice(model.DisplayName, newWidth, newWidth / model.DotsPerMm);
        return true;
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
        _labelWidthRow.Visible = _model.WidthDependsOnMedia;
        if (_model.WidthDependsOnMedia)
            _labelWidth.Maximum = (decimal)Math.Floor(_model.HeadWidthMm);
        UpdatePrintAvailability();
    }

    /// <summary>Los modelos que necesitan compresión se pueden elegir (para dibujar) pero no imprimir.</summary>
    private void UpdatePrintAvailability()
    {
        bool printing = _printCts is not null;
        _printButton.Enabled = _printItem.Enabled = printing || _model.CanPrint;
        _compressionWarning.Visible = !_model.CanPrint;
        _compressionWarning.Text = _model.CanPrint ? "" : S.CompressionWarning(_model.DisplayName);
        if (!printing)
            _tips.SetToolTip(_printButton, _model.CanPrint ? PrintTip : S.CompressionWarning(_model.DisplayName));
        if (!_model.CanPrint)
            _statusLabel.Text = S.CompressionWarning(_model.DisplayName);
        else if (_statusLabel.Text?.StartsWith('⚠') == true)
            _statusLabel.Text = S.Ready;
        UpdateInfoPanel();
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
            // Sin preguntar (se puede deshacer): aviso en la barra de estado.
            if (cut)
                _cutNotice = S.HeightCutNotice(h, h / _model.DotsPerMm);
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
        if (!_model.CanPrint)
        {
            MessageBox.Show(this, S.CompressionWarning(_model.DisplayName), S.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
        _printButton.Text = CancelGlyph;
        _tips.SetToolTip(_printButton, S.Cancel);
        _progress.Value = 0;
        _progress.Visible = true;
        _statusLabel.Text = S.Printing;
        var progress = new Progress<double>(v => _progress.Value = (int)Math.Round(v * 100));
        bool locked = false;
        try
        {
            // Si se está consultando la impresora, se espera a que acabe.
            if (!_printerLock.Wait(0))
            {
                _statusLabel.Text = S.WaitingForPrinter;
                await _printerLock.WaitAsync(_printCts.Token);
                _statusLabel.Text = S.Printing;
            }
            locked = true;
            await using var transport = Transports.Create(port);
            var printer = new PhomemoPrinter(transport, _model);

            // Sin papel o con la tapa abierta la impresora acepta el trabajo y no avisa: se
            // comprueba antes en la misma conexión.
            var ready = await printer.QueryReadyAsync(_printCts.Token);
            if (ready.Response.Length > 0)
            {
                _info = _info is null ? ready : _info.Merge(ready);
                UpdateInfoPanel();
            }
            if (ready.PaperOut == true || ready.CoverOpen == true)
            {
                // Con la tapa abierta el sensor también dice «sin papel»: la tapa va primero.
                var problem = ready.CoverOpen == true ? S.CoverIsOpen : S.NoPaper;
                _statusLabel.Text = problem;
                MessageBox.Show(this, problem, S.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = await printer.PrintAsync(image, PrintOptions.Default, progress, _printCts.Token);
            if (result.Response.Length > 0)
            {
                // Lo que diga la impresora al imprimir (papel, batería…) también va al panel.
                var info = PrinterInfo.From(result.Response);
                _info = _info is null ? info : _info.Merge(info);
                UpdateInfoPanel();
            }
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
            if (locked) _printerLock.Release();
            _printCts.Dispose();
            _printCts = null;
            _printButton.Text = PrintGlyph;
            UpdatePrintAvailability();
            _progress.Visible = false;
        }
    }

    private async void ShowDiagnostics()
    {
        if (_portBox.SelectedItem is not PortInfo port)
        {
            MessageBox.Show(this, S.ChoosePort, S.AppTitle);
            return;
        }
        // El diagnóstico abre su propia conexión: mientras, nada más usa la impresora.
        await _printerLock.WaitAsync();
        try
        {
            using var form = new DiagnosticsForm(port, _model);
            form.ShowDialog(this);
        }
        finally
        {
            _printerLock.Release();
        }
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
        if (_doc.Selected?.Text is { } text && text.Black != black)
            ChangeText(t => t with { Black = black });
    }

    private void DocumentEdited()
    {
        _doc.NotifyChanged();
        UpdateState();
        _canvas.Invalidate();
    }

    private void UpdateExtent()
    {
        _extentLabel.Text = S.PrintExtent(_doc.PrintHeight, _doc.PrintHeight / _model.DotsPerMm);
        UpdateWarning();
    }

    /// <summary>
    /// Aviso amarillo de la barra de estado: lo último que se ha recortado (hasta la siguiente
    /// edición) o, si no, que alguna imagen sobresale del papel.
    /// </summary>
    private void UpdateWarning()
    {
        string? text = _cutNotice ?? (_doc.Objects.Any(ObjectOutsidePage) ? S.ObjectsOutside : null);
        _warningLabel.Text = text ?? "";
        _warningLabel.Visible = text is not null;
    }

    private bool ObjectOutsidePage(ImageObject o) =>
        o.Ink is null
            ? o.Corners().Any(OutsidePage)
            : Corners(o.InkExtent()).Any(OutsidePage); // sin el grosor: pintar pegado al borde no es salirse

    private bool OutsidePage(PointF p) => p.X < -0.5f || p.Y < -0.5f || p.X > _doc.Width + 0.5f || p.Y > _doc.Height + 0.5f;

    private static PointF[] Corners(RectangleF r) => [new(r.Left, r.Top), new(r.Right, r.Bottom)];

    private void UpdateState()
    {
        _undoItem.Enabled = _history.CanUndo;
        _redoItem.Enabled = _history.CanRedo;
        bool sel = _doc.Selected is not null;
        bool target = sel || _doc.ActiveInk is not null;
        _commitItem.Enabled = _deleteItem.Enabled = _commitButton.Enabled = _deleteButton.Enabled = target;
        var text = _doc.Selected?.Text;
        bool ink = _doc.Selected?.Ink is not null;
        _selectionProps.Visible = sel && text is null && !ink;
        _textProps.Visible = text is not null;
        _noSelection.Visible = !sel || ink;
        _noSelection.Text = ink ? S.DrawingSelected : S.NoSelection;
        _updating = true;
        _conversionBox.SelectedIndex = sel ? (int)_doc.Selected!.Conversion : -1;
        if (text is not null) ShowText(text);
        _updating = false;
        UpdateExtent();
    }

    private void Undo()
    {
        _cutNotice = null;
        if (!_history.Undo(_doc)) return;
        SyncAfterRestore();
    }

    private void Redo()
    {
        _cutNotice = null;
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
        if ((_doc.Selected ?? _doc.ActiveInk) is not { } obj) return;
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
        if ((_doc.Selected ?? _doc.ActiveInk) is not { } obj) return;
        _history.Push(_doc);
        _doc.Objects.Remove(obj);
        _doc.Selected = null;
        DocumentEdited();
    }

    private void Deselect()
    {
        _doc.ActiveInk = null;
        if (_doc.Selected is null) return;
        _doc.Selected = null;
        DropEmptyTexts();
        UpdateState();
        _canvas.Invalidate();
    }

    private void ConversionChanged()
    {
        if (_doc.Selected is not { Text: null, Ink: null } obj || _conversionBox.SelectedIndex < 0) return;
        _history.Push(_doc);
        obj.Conversion = (MonoConversion)_conversionBox.SelectedIndex;
        obj.Invalidate();
        DocumentEdited();
    }

    private void PasteImage()
    {
        Bitmap? img = null;
        try
        {
            if (Clipboard.ContainsFileDropList() && Clipboard.GetFileDropList() is { Count: > 0 } files)
            {
                InsertImage(files[0]!);
                return;
            }
            img = Imaging.FromClipboard();
        }
        catch (System.Runtime.InteropServices.ExternalException)
        {
        }
        if (img is null)
        {
            MessageBox.Show(this, S.NoImageInClipboard, S.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        AddObject(img);
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
        SetTool(CanvasTool.Cursor); // lo normal tras pegar es colocarla
        _doc.Selected = obj;
        DocumentEdited();
        _canvas.Focus();
    }

    // ------------------------------------------------------------------ texto

    /// <summary>Cambiar de herramienta cierra el grupo de trazos; con el cursor queda seleccionado.</summary>
    private void SetTool(CanvasTool tool)
    {
        if (tool != _canvas.Tool && _doc.ActiveInk is { } ink)
        {
            _doc.ActiveInk = null;
            if (tool == CanvasTool.Cursor) _doc.Selected = ink;
        }
        else if (tool == CanvasTool.Brush && _doc.Selected is not null)
        {
            _doc.Selected = null;
            DropEmptyTexts();
        }
        _canvas.Tool = tool;
        _cursorButton.BackColor = tool == CanvasTool.Cursor ? Color.LightSkyBlue : SystemColors.Control;
        _brushButton.BackColor = tool == CanvasTool.Brush ? Color.LightSkyBlue : SystemColors.Control;
        _textButton.BackColor = tool == CanvasTool.Text ? Color.LightSkyBlue : SystemColors.Control;
        UpdateState();
        _canvas.Invalidate();
        _canvas.Focus();
    }

    /// <summary>Texto nuevo en <paramref name="topLeft"/>, listo para escribir encima del de ejemplo.</summary>
    private void AddText(PointF topLeft)
    {
        _history.Push(_doc);
        var text = _lastText with { Text = S.TextSample, Black = _canvas.BrushBlack, DotsPerMm = _model.DotsPerMm };
        // El cuadro llega hasta el borde del papel: el texto se parte ahí, no fuera.
        float x = (float)Math.Round(topLeft.X);
        var obj = ImageObject.FromText(text, new PointF(x, (float)Math.Round(topLeft.Y)), Math.Max(MinTextWidth, _doc.Width - x));
        _doc.Objects.Add(obj);
        _doc.Selected = obj;
        DocumentEdited();
        _textBox.Focus();
        _textBox.SelectAll();
    }

    /// <summary>Copia el texto seleccionado a los controles.</summary>
    private void ShowText(TextContent text)
    {
        // Sin reasignar si no cambia, para no mover el cursor mientras se escribe.
        if (_textBox.Text != text.Text) _textBox.Text = text.Text;
        _fontBox.SelectedItem = text.FontFamily;
        _fontSizeBox.Value = Math.Clamp((decimal)text.FontSize, _fontSizeBox.Minimum, _fontSizeBox.Maximum);
        Mark(_boldButton, text.Style.HasFlag(FontStyle.Bold));
        Mark(_italicButton, text.Style.HasFlag(FontStyle.Italic));
        Mark(_underlineButton, text.Style.HasFlag(FontStyle.Underline));
        Mark(_alignLeftButton, text.Alignment == TextAlignment.Left);
        Mark(_alignCenterButton, text.Alignment == TextAlignment.Center);
        Mark(_alignRightButton, text.Alignment == TextAlignment.Right);

        static void Mark(Button button, bool on) => button.BackColor = on ? Color.LightSkyBlue : SystemColors.Control;
    }

    /// <summary>Escribir seguido es un solo paso de deshacer.</summary>
    private void TextTyped()
    {
        if (_doc.Selected?.Text is not { } text) return;
        if (!_textBoxPushed)
        {
            _history.Push(_doc);
            _textBoxPushed = true;
        }
        _doc.Selected.SetText(text with { Text = _textBox.Text });
        DocumentEdited();
    }

    private void ChangeText(Func<TextContent, TextContent> change)
    {
        if (_doc.Selected?.Text is not { } text) return;
        _history.Push(_doc);
        _doc.Selected.SetText(change(text));
        _lastText = _doc.Selected.Text!;
        DocumentEdited();
    }

    private void ToggleStyle(FontStyle style) => ChangeText(t => t with { Style = t.Style ^ style });

    /// <summary>Un texto que se ha dejado vacío no se ve ni se puede seleccionar: fuera.</summary>
    private void DropEmptyTexts() =>
        _doc.Objects.RemoveAll(o => o != _doc.Selected && o.Text is { } t && string.IsNullOrWhiteSpace(t.Text));

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
