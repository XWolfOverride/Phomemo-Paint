namespace PhomemoPaint
{
    partial class MainForm
    {
        /// <summary>Variable del diseñador necesaria.</summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>Limpiar los recursos que se estén usando.</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar el contenido de este
        /// método con el editor de código.
        /// </summary>
        /// <remarks>
        /// Los textos que se ven aquí son los españoles; en ejecución los vuelve a poner
        /// <c>ApplyTexts()</c> en el idioma de la aplicación. Los eventos se conectan en
        /// <c>WireEvents()</c>.
        /// </remarks>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            _tips = new ToolTip(components);
            _brushButton = new IconButton();
            _textButton = new IconButton();
            _cursorButton = new IconButton();
            _insertButton = new IconButton();
            _pasteButton = new IconButton();
            _commitButton = new IconButton();
            _deleteButton = new IconButton();
            _printButton = new IconButton();
            _refreshButton = new IconButton();
            _zoomOutButton = new IconButton();
            _zoomInButton = new IconButton();
            _zoomFitButton = new IconButton();
            _blackButton = new RadioButton();
            _whiteButton = new RadioButton();
            _brush = new NumericUpDown();
            _menu = new MenuStrip();
            _fileMenu = new ToolStripMenuItem();
            _newItem = new ToolStripMenuItem();
            _openCanvasItem = new ToolStripMenuItem();
            _insertItem = new ToolStripMenuItem();
            _insertTextItem = new ToolStripMenuItem();
            _exportItem = new ToolStripMenuItem();
            _fileSeparator = new ToolStripSeparator();
            _exitItem = new ToolStripMenuItem();
            _editMenu = new ToolStripMenuItem();
            _undoItem = new ToolStripMenuItem();
            _redoItem = new ToolStripMenuItem();
            _editSeparator = new ToolStripSeparator();
            _pasteItem = new ToolStripMenuItem();
            _commitItem = new ToolStripMenuItem();
            _commitAllItem = new ToolStripMenuItem();
            _deleteItem = new ToolStripMenuItem();
            _viewMenu = new ToolStripMenuItem();
            _zoomInItem = new ToolStripMenuItem();
            _zoomOutItem = new ToolStripMenuItem();
            _zoomFitItem = new ToolStripMenuItem();
            _printerMenu = new ToolStripMenuItem();
            _printItem = new ToolStripMenuItem();
            _detectItem = new ToolStripMenuItem();
            _printerSeparator = new ToolStripSeparator();
            _calibrationItem = new ToolStripMenuItem();
            _diagnosticsItem = new ToolStripMenuItem();
            _helpMenu = new ToolStripMenuItem();
            _helpItem = new ToolStripMenuItem();
            _tipsItem = new ToolStripMenuItem();
            _problemsItem = new ToolStripMenuItem();
            _helpSeparator = new ToolStripSeparator();
            _aboutItem = new ToolStripMenuItem();
            _status = new StatusStrip();
            _cursorLabel = new ToolStripStatusLabel();
            _extentLabel = new ToolStripStatusLabel();
            _warningLabel = new ToolStripStatusLabel();
            _statusLabel = new ToolStripStatusLabel();
            _progress = new ToolStripProgressBar();
            _canvas = new PhomemoPaint.Editing.CanvasControl();
            _sidePanel = new Panel();
            _zoomSection = new Panel();
            _zoomBox = new ComboBox();
            _printerSection = new Panel();
            _printerContent = new FlowLayoutPanel();
            _portRow = new FlowLayoutPanel();
            _portBox = new ComboBox();
            _modelBox = new ComboBox();
            _compressionWarning = new Label();
            _labelWidthRow = new FlowLayoutPanel();
            _labelWidthLabel = new Label();
            _labelWidth = new NumericUpDown();
            _infoPanel = new PrinterInfoPanel();
            _printerTitle = new Label();
            _propertiesSection = new Panel();
            _propertiesContent = new FlowLayoutPanel();
            _noSelection = new Label();
            _selectionProps = new FlowLayoutPanel();
            _textProps = new FlowLayoutPanel();
            _textBox = new TextBox();
            _fontBox = new ComboBox();
            _fontRow = new FlowLayoutPanel();
            _fontSizeBox = new NumericUpDown();
            _boldButton = new IconButton();
            _italicButton = new IconButton();
            _underlineButton = new IconButton();
            _alignLeftButton = new IconButton();
            _alignCenterButton = new IconButton();
            _alignRightButton = new IconButton();
            _conversionLabel = new Label();
            _conversionBox = new ComboBox();
            _propertiesTitle = new Label();
            _toolsSection = new Panel();
            _toolsRow2 = new FlowLayoutPanel();
            _toolsContent = new FlowLayoutPanel();
            _toolsRow1 = new FlowLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)_brush).BeginInit();
            _menu.SuspendLayout();
            _status.SuspendLayout();
            _sidePanel.SuspendLayout();
            _zoomSection.SuspendLayout();
            _printerSection.SuspendLayout();
            _printerContent.SuspendLayout();
            _portRow.SuspendLayout();
            _labelWidthRow.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_labelWidth).BeginInit();
            _propertiesSection.SuspendLayout();
            _propertiesContent.SuspendLayout();
            _selectionProps.SuspendLayout();
            _textProps.SuspendLayout();
            _fontRow.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_fontSizeBox).BeginInit();
            _toolsSection.SuspendLayout();
            _toolsContent.SuspendLayout();
            SuspendLayout();
            // 
            // _brushButton
            // 
            _brushButton.BackColor = Color.LightSkyBlue;
            _brushButton.FlatAppearance.BorderColor = SystemColors.ControlDark;
            _brushButton.FlatStyle = FlatStyle.Flat;
            _brushButton.Location = new Point(0, 0);
            _brushButton.Margin = new Padding(0, 0, 4, 4);
            _brushButton.Name = "_brushButton";
            _brushButton.Size = new Size(36, 36);
            _brushButton.TabIndex = 0;
            _brushButton.Text = "";
            _tips.SetToolTip(_brushButton, "Pincel (botón derecho: el otro color)");
            _brushButton.UseVisualStyleBackColor = false;
            //
            // _textButton
            //
            _textButton.FlatAppearance.BorderColor = SystemColors.ControlDark;
            _textButton.FlatStyle = FlatStyle.Flat;
            _textButton.Location = new Point(174, 0);
            _textButton.Margin = new Padding(0, 0, 4, 4);
            _textButton.Name = "_textButton";
            _textButton.Size = new Size(36, 36);
            _textButton.TabIndex = 10;
            _textButton.Text = "";
            _tips.SetToolTip(_textButton, "Texto: haz clic en el lienzo para colocarlo (Ctrl+T)");
            _textButton.UseVisualStyleBackColor = false;
            // 
            // _cursorButton
            // 
            _cursorButton.FlatAppearance.BorderColor = SystemColors.ControlDark;
            _cursorButton.FlatStyle = FlatStyle.Flat;
            _cursorButton.Location = new Point(119, 43);
            _cursorButton.Margin = new Padding(0, 0, 4, 4);
            _cursorButton.Name = "_cursorButton";
            _cursorButton.Size = new Size(32, 32);
            _cursorButton.TabIndex = 11;
            _cursorButton.Text = "";
            _tips.SetToolTip(_cursorButton, "Cursor: selecciona y mueve objetos sin pintar (V)");
            _cursorButton.UseVisualStyleBackColor = false;
            // 
            // _insertButton
            // 
            _insertButton.FlatAppearance.BorderColor = SystemColors.ControlDark;
            _insertButton.FlatStyle = FlatStyle.Flat;
            _insertButton.Location = new Point(94, 0);
            _insertButton.Margin = new Padding(0, 0, 4, 4);
            _insertButton.Name = "_insertButton";
            _insertButton.Size = new Size(36, 36);
            _insertButton.TabIndex = 1;
            _insertButton.Text = "";
            _tips.SetToolTip(_insertButton, "Insertar imagen (Ctrl+I)");
            _insertButton.UseVisualStyleBackColor = true;
            // 
            // _pasteButton
            // 
            _pasteButton.FlatAppearance.BorderColor = SystemColors.ControlDark;
            _pasteButton.FlatStyle = FlatStyle.Flat;
            _pasteButton.Location = new Point(134, 0);
            _pasteButton.Margin = new Padding(0, 0, 4, 4);
            _pasteButton.Name = "_pasteButton";
            _pasteButton.Size = new Size(36, 36);
            _pasteButton.TabIndex = 2;
            _pasteButton.Text = "";
            _tips.SetToolTip(_pasteButton, "Pegar imagen (Ctrl+V)");
            _pasteButton.UseVisualStyleBackColor = true;
            // 
            // _commitButton
            // 
            _commitButton.FlatAppearance.BorderColor = SystemColors.ControlDark;
            _commitButton.FlatStyle = FlatStyle.Flat;
            _commitButton.Location = new Point(155, 43);
            _commitButton.Margin = new Padding(0, 0, 4, 4);
            _commitButton.Name = "_commitButton";
            _commitButton.Size = new Size(32, 32);
            _commitButton.TabIndex = 3;
            _commitButton.Text = "";
            _tips.SetToolTip(_commitButton, "Fijar imagen (Intro)");
            _commitButton.UseVisualStyleBackColor = true;
            // 
            // _deleteButton
            // 
            _deleteButton.FlatAppearance.BorderColor = SystemColors.ControlDark;
            _deleteButton.FlatStyle = FlatStyle.Flat;
            _deleteButton.Location = new Point(191, 43);
            _deleteButton.Margin = new Padding(0, 0, 4, 4);
            _deleteButton.Name = "_deleteButton";
            _deleteButton.Size = new Size(32, 32);
            _deleteButton.TabIndex = 4;
            _deleteButton.Text = "";
            _tips.SetToolTip(_deleteButton, "Eliminar imagen (Supr)");
            _deleteButton.UseVisualStyleBackColor = true;
            // 
            // _printButton
            // 
            _printButton.BackColor = Color.FromArgb(214, 236, 214);
            _printButton.FlatAppearance.BorderColor = SystemColors.ControlDark;
            _printButton.FlatStyle = FlatStyle.Flat;
            _printButton.IconSize = 16F;
            _printButton.Location = new Point(229, 0);
            _printButton.Margin = new Padding(6, 0, 0, 4);
            _printButton.Name = "_printButton";
            _printButton.Size = new Size(42, 76);
            _printButton.TabIndex = 5;
            _printButton.Text = "";
            _tips.SetToolTip(_printButton, "Imprimir (Ctrl+P)");
            _printButton.UseVisualStyleBackColor = false;
            // 
            // _refreshButton
            // 
            _refreshButton.FlatAppearance.BorderColor = SystemColors.ControlDark;
            _refreshButton.FlatStyle = FlatStyle.Flat;
            _refreshButton.Location = new Point(214, 0);
            _refreshButton.Margin = new Padding(0, 0, 0, 4);
            _refreshButton.Name = "_refreshButton";
            _refreshButton.Size = new Size(36, 36);
            _refreshButton.TabIndex = 1;
            _refreshButton.Text = "";
            _tips.SetToolTip(_refreshButton, "Actualizar puertos y buscar por Bluetooth");
            _refreshButton.UseVisualStyleBackColor = true;
            // 
            // _zoomOutButton
            // 
            _zoomOutButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _zoomOutButton.FlatAppearance.BorderColor = SystemColors.ControlDark;
            _zoomOutButton.FlatStyle = FlatStyle.Flat;
            _zoomOutButton.Location = new Point(87, 5);
            _zoomOutButton.Margin = new Padding(0, 0, 4, 4);
            _zoomOutButton.Name = "_zoomOutButton";
            _zoomOutButton.Size = new Size(36, 29);
            _zoomOutButton.TabIndex = 0;
            _zoomOutButton.Text = "";
            _tips.SetToolTip(_zoomOutButton, "Alejar (Ctrl+−)");
            _zoomOutButton.UseVisualStyleBackColor = true;
            // 
            // _zoomInButton
            // 
            _zoomInButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _zoomInButton.FlatAppearance.BorderColor = SystemColors.ControlDark;
            _zoomInButton.FlatStyle = FlatStyle.Flat;
            _zoomInButton.Location = new Point(191, 5);
            _zoomInButton.Margin = new Padding(0, 0, 4, 4);
            _zoomInButton.Name = "_zoomInButton";
            _zoomInButton.Size = new Size(36, 29);
            _zoomInButton.TabIndex = 2;
            _zoomInButton.Text = "";
            _tips.SetToolTip(_zoomInButton, "Acercar (Ctrl++)");
            _zoomInButton.UseVisualStyleBackColor = true;
            // 
            // _zoomFitButton
            // 
            _zoomFitButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _zoomFitButton.FlatAppearance.BorderColor = SystemColors.ControlDark;
            _zoomFitButton.FlatStyle = FlatStyle.Flat;
            _zoomFitButton.Location = new Point(231, 5);
            _zoomFitButton.Margin = new Padding(0, 0, 4, 4);
            _zoomFitButton.Name = "_zoomFitButton";
            _zoomFitButton.Size = new Size(36, 29);
            _zoomFitButton.TabIndex = 3;
            _zoomFitButton.Text = "";
            _tips.SetToolTip(_zoomFitButton, "Ajustar al ancho (Ctrl+0)");
            _zoomFitButton.UseVisualStyleBackColor = true;
            // 
            // _blackButton
            // 
            _blackButton.Appearance = Appearance.Button;
            _blackButton.BackColor = Color.Black;
            _blackButton.Checked = true;
            _blackButton.FlatAppearance.CheckedBackColor = Color.Black;
            _blackButton.FlatAppearance.MouseDownBackColor = Color.Black;
            _blackButton.FlatAppearance.MouseOverBackColor = Color.Black;
            _blackButton.FlatStyle = FlatStyle.Flat;
            _blackButton.Location = new Point(0, 43);
            _blackButton.Margin = new Padding(0, 0, 4, 4);
            _blackButton.Name = "_blackButton";
            _blackButton.Size = new Size(32, 32);
            _blackButton.TabIndex = 6;
            _blackButton.TabStop = true;
            _tips.SetToolTip(_blackButton, "Negro");
            _blackButton.UseVisualStyleBackColor = false;
            // 
            // _whiteButton
            // 
            _whiteButton.Appearance = Appearance.Button;
            _whiteButton.BackColor = Color.White;
            _whiteButton.FlatAppearance.CheckedBackColor = Color.White;
            _whiteButton.FlatAppearance.MouseDownBackColor = Color.White;
            _whiteButton.FlatAppearance.MouseOverBackColor = Color.White;
            _whiteButton.FlatStyle = FlatStyle.Flat;
            _whiteButton.Location = new Point(36, 43);
            _whiteButton.Margin = new Padding(0, 0, 4, 4);
            _whiteButton.Name = "_whiteButton";
            _whiteButton.Size = new Size(32, 32);
            _whiteButton.TabIndex = 7;
            _tips.SetToolTip(_whiteButton, "Blanco");
            _whiteButton.UseVisualStyleBackColor = false;
            // 
            // _brush
            // 
            _brush.Location = new Point(38, 9);
            _brush.Margin = new Padding(0, 7, 0, 0);
            _brush.Maximum = new decimal(new int[] { 64, 0, 0, 0 });
            _brush.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            _brush.Name = "_brush";
            _brush.Size = new Size(46, 27);
            _brush.TabIndex = 9;
            _tips.SetToolTip(_brush, "Tamaño del pincel");
            _brush.Value = new decimal(new int[] { 3, 0, 0, 0 });
            // 
            // _menu
            // 
            _menu.ImageScalingSize = new Size(20, 20);
            _menu.Items.AddRange(new ToolStripItem[] { _fileMenu, _editMenu, _viewMenu, _printerMenu, _helpMenu });
            _menu.Location = new Point(0, 0);
            _menu.Name = "_menu";
            _menu.Size = new Size(1202, 28);
            _menu.TabIndex = 0;
            // 
            // _fileMenu
            // 
            _fileMenu.DropDownItems.AddRange(new ToolStripItem[] { _newItem, _openCanvasItem, _insertItem, _insertTextItem, _exportItem, _fileSeparator, _exitItem });
            _fileMenu.Name = "_fileMenu";
            _fileMenu.Size = new Size(73, 24);
            _fileMenu.Text = "&Archivo";
            // 
            // _newItem
            // 
            _newItem.Name = "_newItem";
            _newItem.ShortcutKeys = Keys.Control | Keys.N;
            _newItem.Size = new Size(306, 26);
            _newItem.Text = "&Nuevo";
            // 
            // _openCanvasItem
            // 
            _openCanvasItem.Name = "_openCanvasItem";
            _openCanvasItem.ShortcutKeys = Keys.Control | Keys.O;
            _openCanvasItem.Size = new Size(306, 26);
            _openCanvasItem.Text = "&Abrir PNG como lienzo...";
            // 
            // _insertItem
            // 
            _insertItem.Name = "_insertItem";
            _insertItem.ShortcutKeys = Keys.Control | Keys.I;
            _insertItem.Size = new Size(306, 26);
            _insertItem.Text = "&Insertar imagen...";
            // 
            // _insertTextItem
            // 
            _insertTextItem.Name = "_insertTextItem";
            _insertTextItem.ShortcutKeys = Keys.Control | Keys.T;
            _insertTextItem.Size = new Size(306, 26);
            _insertTextItem.Text = "Insertar &texto";
            // 
            // _exportItem
            // 
            _exportItem.Name = "_exportItem";
            _exportItem.ShortcutKeys = Keys.Control | Keys.S;
            _exportItem.Size = new Size(306, 26);
            _exportItem.Text = "&Exportar PNG...";
            // 
            // _fileSeparator
            // 
            _fileSeparator.Name = "_fileSeparator";
            _fileSeparator.Size = new Size(303, 6);
            // 
            // _exitItem
            // 
            _exitItem.Name = "_exitItem";
            _exitItem.Size = new Size(306, 26);
            _exitItem.Text = "&Salir";
            // 
            // _editMenu
            // 
            _editMenu.DropDownItems.AddRange(new ToolStripItem[] { _undoItem, _redoItem, _editSeparator, _pasteItem, _commitItem, _commitAllItem, _deleteItem });
            _editMenu.Name = "_editMenu";
            _editMenu.Size = new Size(62, 24);
            _editMenu.Text = "&Editar";
            // 
            // _undoItem
            // 
            _undoItem.Name = "_undoItem";
            _undoItem.ShortcutKeys = Keys.Control | Keys.Z;
            _undoItem.Size = new Size(239, 26);
            _undoItem.Text = "&Deshacer";
            // 
            // _redoItem
            // 
            _redoItem.Name = "_redoItem";
            _redoItem.ShortcutKeys = Keys.Control | Keys.Y;
            _redoItem.Size = new Size(239, 26);
            _redoItem.Text = "&Rehacer";
            // 
            // _editSeparator
            // 
            _editSeparator.Name = "_editSeparator";
            _editSeparator.Size = new Size(236, 6);
            // 
            // _pasteItem
            // 
            _pasteItem.Name = "_pasteItem";
            _pasteItem.ShortcutKeys = Keys.Control | Keys.V;
            _pasteItem.Size = new Size(239, 26);
            _pasteItem.Text = "&Pegar imagen";
            // 
            // _commitItem
            // 
            _commitItem.Name = "_commitItem";
            _commitItem.ShortcutKeyDisplayString = "Intro";
            _commitItem.Size = new Size(239, 26);
            _commitItem.Text = "&Fijar imagen";
            // 
            // _commitAllItem
            // 
            _commitAllItem.Name = "_commitAllItem";
            _commitAllItem.Size = new Size(239, 26);
            _commitAllItem.Text = "Fijar &todas";
            // 
            // _deleteItem
            // 
            _deleteItem.Name = "_deleteItem";
            _deleteItem.ShortcutKeyDisplayString = "Supr";
            _deleteItem.Size = new Size(239, 26);
            _deleteItem.Text = "&Eliminar imagen";
            // 
            // _viewMenu
            // 
            _viewMenu.DropDownItems.AddRange(new ToolStripItem[] { _zoomInItem, _zoomOutItem, _zoomFitItem });
            _viewMenu.Name = "_viewMenu";
            _viewMenu.Size = new Size(44, 24);
            _viewMenu.Text = "&Ver";
            // 
            // _zoomInItem
            // 
            _zoomInItem.Name = "_zoomInItem";
            _zoomInItem.ShortcutKeys = Keys.Control | Keys.Oemplus;
            _zoomInItem.Size = new Size(248, 26);
            _zoomInItem.Text = "&Acercar";
            // 
            // _zoomOutItem
            // 
            _zoomOutItem.Name = "_zoomOutItem";
            _zoomOutItem.ShortcutKeys = Keys.Control | Keys.OemMinus;
            _zoomOutItem.Size = new Size(248, 26);
            _zoomOutItem.Text = "A&lejar";
            // 
            // _zoomFitItem
            // 
            _zoomFitItem.Name = "_zoomFitItem";
            _zoomFitItem.ShortcutKeys = Keys.Control | Keys.D0;
            _zoomFitItem.Size = new Size(248, 26);
            _zoomFitItem.Text = "&Ajustar al ancho";
            // 
            // _printerMenu
            // 
            _printerMenu.DropDownItems.AddRange(new ToolStripItem[] { _printItem, _detectItem, _printerSeparator, _calibrationItem, _diagnosticsItem });
            _printerMenu.Name = "_printerMenu";
            _printerMenu.Size = new Size(90, 24);
            _printerMenu.Text = "&Impresora";
            // 
            // _printItem
            // 
            _printItem.Name = "_printItem";
            _printItem.ShortcutKeys = Keys.Control | Keys.P;
            _printItem.Size = new Size(254, 26);
            _printItem.Text = "&Imprimir";
            // 
            // _detectItem
            // 
            _detectItem.Name = "_detectItem";
            _detectItem.Size = new Size(254, 26);
            _detectItem.Text = "&Detectar modelo";
            // 
            // _printerSeparator
            // 
            _printerSeparator.Name = "_printerSeparator";
            _printerSeparator.Size = new Size(251, 6);
            // 
            // _calibrationItem
            // 
            _calibrationItem.Name = "_calibrationItem";
            _calibrationItem.Size = new Size(254, 26);
            _calibrationItem.Text = "Patrón de &calibración";
            // 
            // _diagnosticsItem
            // 
            _diagnosticsItem.Name = "_diagnosticsItem";
            _diagnosticsItem.Size = new Size(254, 26);
            _diagnosticsItem.Text = "Diagnóstico de &señales...";
            // 
            // _helpMenu
            // 
            _helpMenu.DropDownItems.AddRange(new ToolStripItem[] { _helpItem, _tipsItem, _problemsItem, _helpSeparator, _aboutItem });
            _helpMenu.Name = "_helpMenu";
            _helpMenu.Size = new Size(65, 24);
            _helpMenu.Text = "Ay&uda";
            // 
            // _helpItem
            // 
            _helpItem.Name = "_helpItem";
            _helpItem.ShortcutKeyDisplayString = "F1";
            _helpItem.Size = new Size(283, 26);
            _helpItem.Text = "&Ayuda de PhomemoPaint";
            // 
            // _tipsItem
            // 
            _tipsItem.Name = "_tipsItem";
            _tipsItem.Size = new Size(283, 26);
            _tipsItem.Text = "&Trucos y atajos";
            // 
            // _problemsItem
            // 
            _problemsItem.Name = "_problemsItem";
            _problemsItem.Size = new Size(283, 26);
            _problemsItem.Text = "&Si algo no funciona";
            // 
            // _helpSeparator
            // 
            _helpSeparator.Name = "_helpSeparator";
            _helpSeparator.Size = new Size(280, 6);
            // 
            // _aboutItem
            // 
            _aboutItem.Name = "_aboutItem";
            _aboutItem.Size = new Size(283, 26);
            _aboutItem.Text = "Acerca &de PhomemoPaint";
            // 
            // _status
            // 
            _status.ImageScalingSize = new Size(20, 20);
            _status.Items.AddRange(new ToolStripItem[] { _cursorLabel, _extentLabel, _warningLabel, _statusLabel, _progress });
            _status.Location = new Point(0, 909);
            _status.Name = "_status";
            _status.Size = new Size(1202, 24);
            _status.TabIndex = 3;
            // 
            // _cursorLabel
            // 
            _cursorLabel.AutoSize = false;
            _cursorLabel.Name = "_cursorLabel";
            _cursorLabel.Size = new Size(170, 18);
            _cursorLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _extentLabel
            // 
            _extentLabel.AutoSize = false;
            _extentLabel.Name = "_extentLabel";
            _extentLabel.Size = new Size(230, 18);
            _extentLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _warningLabel
            // 
            _warningLabel.BackColor = Color.Khaki;
            _warningLabel.ForeColor = Color.Black;
            _warningLabel.Margin = new Padding(4, 3, 4, 2);
            _warningLabel.Name = "_warningLabel";
            _warningLabel.Size = new Size(0, 19);
            _warningLabel.Visible = false;
            // 
            // _statusLabel
            // 
            _statusLabel.Name = "_statusLabel";
            _statusLabel.Size = new Size(787, 18);
            _statusLabel.Spring = true;
            _statusLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _progress
            // 
            _progress.Name = "_progress";
            _progress.Size = new Size(150, 16);
            _progress.Visible = false;
            // 
            // _canvas
            // 
            _canvas.AutoScroll = true;
            _canvas.BackColor = SystemColors.AppWorkspace;
            _canvas.Dock = DockStyle.Fill;
            _canvas.Location = new Point(0, 28);
            _canvas.Name = "_canvas";
            _canvas.Size = new Size(922, 881);
            _canvas.TabIndex = 1;
            _canvas.Tag = "help:lienzo";
            // 
            // _sidePanel
            // 
            _sidePanel.Controls.Add(_zoomSection);
            _sidePanel.Controls.Add(_printerSection);
            _sidePanel.Controls.Add(_propertiesSection);
            _sidePanel.Controls.Add(_toolsSection);
            _sidePanel.Dock = DockStyle.Right;
            _sidePanel.Location = new Point(922, 28);
            _sidePanel.Name = "_sidePanel";
            _sidePanel.Padding = new Padding(4);
            _sidePanel.Size = new Size(280, 881);
            _sidePanel.TabIndex = 2;
            // 
            // _zoomSection
            // 
            _zoomSection.Controls.Add(_zoomOutButton);
            _zoomSection.Controls.Add(_zoomBox);
            _zoomSection.Controls.Add(_zoomFitButton);
            _zoomSection.Controls.Add(_zoomInButton);
            _zoomSection.Dock = DockStyle.Bottom;
            _zoomSection.Location = new Point(4, 838);
            _zoomSection.Name = "_zoomSection";
            _zoomSection.Padding = new Padding(8, 6, 8, 8);
            _zoomSection.Size = new Size(272, 39);
            _zoomSection.TabIndex = 3;
            _zoomSection.Tag = "help:zoom";
            // 
            // _zoomBox
            // 
            _zoomBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _zoomBox.DropDownStyle = ComboBoxStyle.DropDownList;
            _zoomBox.Items.AddRange(new object[] { "×1", "×2", "×3", "×4", "×5", "×6", "×7", "×8" });
            _zoomBox.Location = new Point(127, 6);
            _zoomBox.Margin = new Padding(0, 7, 4, 0);
            _zoomBox.Name = "_zoomBox";
            _zoomBox.Size = new Size(60, 28);
            _zoomBox.TabIndex = 1;
            // 
            // _printerSection
            // 
            _printerSection.AutoSize = true;
            _printerSection.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            _printerSection.BorderStyle = BorderStyle.FixedSingle;
            _printerSection.Controls.Add(_printerContent);
            _printerSection.Controls.Add(_printerTitle);
            _printerSection.Dock = DockStyle.Top;
            _printerSection.Location = new Point(4, 218);
            _printerSection.Name = "_printerSection";
            _printerSection.Padding = new Padding(8, 6, 8, 8);
            _printerSection.Size = new Size(272, 399);
            _printerSection.TabIndex = 2;
            _printerSection.Tag = "help:impresora";
            // 
            // _printerContent
            // 
            _printerContent.AutoSize = true;
            _printerContent.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            _printerContent.Controls.Add(_portRow);
            _printerContent.Controls.Add(_modelBox);
            _printerContent.Controls.Add(_compressionWarning);
            _printerContent.Controls.Add(_labelWidthRow);
            _printerContent.Controls.Add(_infoPanel);
            _printerContent.Dock = DockStyle.Top;
            _printerContent.FlowDirection = FlowDirection.TopDown;
            _printerContent.Location = new Point(8, 30);
            _printerContent.Name = "_printerContent";
            _printerContent.Size = new Size(254, 359);
            _printerContent.TabIndex = 0;
            _printerContent.WrapContents = false;
            // 
            // _portRow
            // 
            _portRow.AutoSize = true;
            _portRow.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            _portRow.Controls.Add(_portBox);
            _portRow.Controls.Add(_refreshButton);
            _portRow.Location = new Point(0, 0);
            _portRow.Margin = new Padding(0);
            _portRow.Name = "_portRow";
            _portRow.Size = new Size(250, 40);
            _portRow.TabIndex = 0;
            // 
            // _portBox
            // 
            _portBox.DropDownStyle = ComboBoxStyle.DropDownList;
            _portBox.DropDownWidth = 330;
            _portBox.Location = new Point(0, 6);
            _portBox.Margin = new Padding(0, 6, 4, 0);
            _portBox.Name = "_portBox";
            _portBox.Size = new Size(210, 28);
            _portBox.TabIndex = 0;
            // 
            // _modelBox
            // 
            _modelBox.DropDownStyle = ComboBoxStyle.DropDownList;
            _modelBox.DropDownWidth = 330;
            _modelBox.Location = new Point(0, 40);
            _modelBox.Margin = new Padding(0, 0, 0, 4);
            _modelBox.Name = "_modelBox";
            _modelBox.Size = new Size(250, 28);
            _modelBox.TabIndex = 1;
            // 
            // _compressionWarning
            // 
            _compressionWarning.AutoSize = true;
            _compressionWarning.ForeColor = Color.Firebrick;
            _compressionWarning.Location = new Point(0, 72);
            _compressionWarning.Margin = new Padding(0, 0, 0, 4);
            _compressionWarning.MaximumSize = new Size(250, 0);
            _compressionWarning.Name = "_compressionWarning";
            _compressionWarning.Size = new Size(0, 17);
            _compressionWarning.TabIndex = 2;
            _compressionWarning.Visible = false;
            // 
            // _labelWidthRow
            // 
            _labelWidthRow.AutoSize = true;
            _labelWidthRow.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            _labelWidthRow.Controls.Add(_labelWidthLabel);
            _labelWidthRow.Controls.Add(_labelWidth);
            _labelWidthRow.Location = new Point(0, 93);
            _labelWidthRow.Margin = new Padding(0);
            _labelWidthRow.Name = "_labelWidthRow";
            _labelWidthRow.Size = new Size(177, 33);
            _labelWidthRow.TabIndex = 3;
            // 
            // _labelWidthLabel
            // 
            _labelWidthLabel.AutoSize = true;
            _labelWidthLabel.Location = new Point(0, 6);
            _labelWidthLabel.Margin = new Padding(0, 6, 4, 0);
            _labelWidthLabel.Name = "_labelWidthLabel";
            _labelWidthLabel.Size = new Size(107, 20);
            _labelWidthLabel.TabIndex = 0;
            _labelWidthLabel.Text = "Etiqueta (mm):";
            // 
            // _labelWidth
            // 
            _labelWidth.Location = new Point(114, 3);
            _labelWidth.Maximum = new decimal(new int[] { 120, 0, 0, 0 });
            _labelWidth.Minimum = new decimal(new int[] { 10, 0, 0, 0 });
            _labelWidth.Name = "_labelWidth";
            _labelWidth.Size = new Size(60, 27);
            _labelWidth.TabIndex = 1;
            _labelWidth.Value = new decimal(new int[] { 40, 0, 0, 0 });
            // 
            // _infoPanel
            // 
            _infoPanel.AutoSize = true;
            _infoPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            _infoPanel.Location = new Point(0, 132);
            _infoPanel.Margin = new Padding(0, 6, 0, 0);
            _infoPanel.Name = "_infoPanel";
            _infoPanel.Size = new Size(115, 227);
            _infoPanel.TabIndex = 4;
            // 
            // _printerTitle
            // 
            _printerTitle.AutoSize = true;
            _printerTitle.Dock = DockStyle.Top;
            _printerTitle.Location = new Point(8, 6);
            _printerTitle.Name = "_printerTitle";
            _printerTitle.Padding = new Padding(0, 0, 0, 4);
            _printerTitle.Size = new Size(76, 24);
            _printerTitle.TabIndex = 1;
            _printerTitle.Text = "Impresora";
            // 
            // _propertiesSection
            // 
            _propertiesSection.AutoSize = true;
            _propertiesSection.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            _propertiesSection.BorderStyle = BorderStyle.FixedSingle;
            _propertiesSection.Controls.Add(_propertiesContent);
            _propertiesSection.Controls.Add(_propertiesTitle);
            _propertiesSection.Dock = DockStyle.Top;
            _propertiesSection.Location = new Point(4, 81);
            _propertiesSection.Name = "_propertiesSection";
            _propertiesSection.Padding = new Padding(8, 6, 8, 8);
            _propertiesSection.Size = new Size(272, 137);
            _propertiesSection.TabIndex = 1;
            _propertiesSection.Tag = "help:propiedades";
            // 
            // _propertiesContent
            // 
            _propertiesContent.AutoSize = true;
            _propertiesContent.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            _propertiesContent.Controls.Add(_noSelection);
            _propertiesContent.Controls.Add(_selectionProps);
            _propertiesContent.Controls.Add(_textProps);
            _propertiesContent.Dock = DockStyle.Top;
            _propertiesContent.FlowDirection = FlowDirection.TopDown;
            _propertiesContent.Location = new Point(8, 30);
            _propertiesContent.Name = "_propertiesContent";
            _propertiesContent.Size = new Size(254, 97);
            _propertiesContent.TabIndex = 0;
            _propertiesContent.WrapContents = false;
            // 
            // _noSelection
            // 
            _noSelection.AutoSize = true;
            _noSelection.ForeColor = SystemColors.GrayText;
            _noSelection.Location = new Point(3, 0);
            _noSelection.MaximumSize = new Size(245, 0);
            _noSelection.Name = "_noSelection";
            _noSelection.Size = new Size(243, 40);
            _noSelection.TabIndex = 0;
            _noSelection.Text = "Nada seleccionado. Haz clic en una imagen o un texto para ver sus propiedades.";
            // 
            // _selectionProps
            // 
            _selectionProps.AutoSize = true;
            _selectionProps.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            _selectionProps.Controls.Add(_conversionLabel);
            _selectionProps.Controls.Add(_conversionBox);
            _selectionProps.FlowDirection = FlowDirection.TopDown;
            _selectionProps.Location = new Point(0, 40);
            _selectionProps.Margin = new Padding(0);
            _selectionProps.Name = "_selectionProps";
            _selectionProps.Size = new Size(161, 57);
            _selectionProps.TabIndex = 1;
            _selectionProps.WrapContents = false;
            //
            // _textProps
            //
            _textProps.AutoSize = true;
            _textProps.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            _textProps.Controls.Add(_textBox);
            _textProps.Controls.Add(_fontBox);
            _textProps.Controls.Add(_fontRow);
            _textProps.FlowDirection = FlowDirection.TopDown;
            _textProps.Location = new Point(0, 97);
            _textProps.Margin = new Padding(0);
            _textProps.Name = "_textProps";
            _textProps.Size = new Size(250, 138);
            _textProps.TabIndex = 2;
            _textProps.Tag = "help:texto";
            _textProps.Visible = false;
            _textProps.WrapContents = false;
            //
            // _textBox
            //
            _textBox.AcceptsReturn = true;
            _textBox.Location = new Point(0, 0);
            _textBox.Margin = new Padding(0, 0, 0, 4);
            _textBox.Multiline = true;
            _textBox.Name = "_textBox";
            _textBox.ScrollBars = ScrollBars.Vertical;
            _textBox.Size = new Size(250, 70);
            _textBox.TabIndex = 0;
            //
            // _fontBox
            //
            _fontBox.DropDownStyle = ComboBoxStyle.DropDownList;
            _fontBox.DropDownWidth = 250;
            _fontBox.Location = new Point(0, 74);
            _fontBox.Margin = new Padding(0, 0, 0, 4);
            _fontBox.Name = "_fontBox";
            _fontBox.Size = new Size(250, 28);
            _fontBox.TabIndex = 1;
            _tips.SetToolTip(_fontBox, "Fuente");
            //
            // _fontRow
            //
            _fontRow.AutoSize = true;
            _fontRow.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            _fontRow.Controls.Add(_fontSizeBox);
            _fontRow.Controls.Add(_boldButton);
            _fontRow.Controls.Add(_italicButton);
            _fontRow.Controls.Add(_underlineButton);
            _fontRow.Controls.Add(_alignLeftButton);
            _fontRow.Controls.Add(_alignCenterButton);
            _fontRow.Controls.Add(_alignRightButton);
            _fontRow.Location = new Point(0, 106);
            _fontRow.Margin = new Padding(0);
            _fontRow.Name = "_fontRow";
            _fontRow.Size = new Size(250, 28);
            _fontRow.TabIndex = 2;
            _fontRow.WrapContents = false;
            //
            // _fontSizeBox
            //
            _fontSizeBox.Location = new Point(0, 0);
            _fontSizeBox.Margin = new Padding(0, 0, 2, 0);
            _fontSizeBox.Maximum = new decimal(new int[] { 300, 0, 0, 0 });
            _fontSizeBox.Minimum = new decimal(new int[] { 4, 0, 0, 0 });
            _fontSizeBox.Name = "_fontSizeBox";
            _fontSizeBox.Size = new Size(60, 27);
            _fontSizeBox.TabIndex = 0;
            _tips.SetToolTip(_fontSizeBox, "Tamaño en puntos tipográficos");
            _fontSizeBox.Value = new decimal(new int[] { 14, 0, 0, 0 });
            // 
            // _boldButton
            // 
            _boldButton.FlatAppearance.BorderColor = SystemColors.ControlDark;
            _boldButton.FlatStyle = FlatStyle.Flat;
            _boldButton.IconSize = 11F;
            _boldButton.Location = new Point(62, 0);
            _boldButton.Margin = new Padding(0, 0, 2, 0);
            _boldButton.Name = "_boldButton";
            _boldButton.Size = new Size(28, 28);
            _boldButton.TabIndex = 1;
            _boldButton.Text = "";
            _tips.SetToolTip(_boldButton, "Negrita");
            _boldButton.UseVisualStyleBackColor = false;
            // 
            // _italicButton
            // 
            _italicButton.FlatAppearance.BorderColor = SystemColors.ControlDark;
            _italicButton.FlatStyle = FlatStyle.Flat;
            _italicButton.IconSize = 11F;
            _italicButton.Location = new Point(92, 0);
            _italicButton.Margin = new Padding(0, 0, 2, 0);
            _italicButton.Name = "_italicButton";
            _italicButton.Size = new Size(28, 28);
            _italicButton.TabIndex = 2;
            _italicButton.Text = "";
            _tips.SetToolTip(_italicButton, "Cursiva");
            _italicButton.UseVisualStyleBackColor = false;
            // 
            // _underlineButton
            // 
            _underlineButton.FlatAppearance.BorderColor = SystemColors.ControlDark;
            _underlineButton.FlatStyle = FlatStyle.Flat;
            _underlineButton.IconSize = 11F;
            _underlineButton.Location = new Point(122, 0);
            _underlineButton.Margin = new Padding(0, 0, 2, 0);
            _underlineButton.Name = "_underlineButton";
            _underlineButton.Size = new Size(28, 28);
            _underlineButton.TabIndex = 3;
            _underlineButton.Text = "";
            _tips.SetToolTip(_underlineButton, "Subrayado");
            _underlineButton.UseVisualStyleBackColor = false;
            // 
            // _alignLeftButton
            // 
            _alignLeftButton.FlatAppearance.BorderColor = SystemColors.ControlDark;
            _alignLeftButton.FlatStyle = FlatStyle.Flat;
            _alignLeftButton.IconSize = 11F;
            _alignLeftButton.Location = new Point(158, 0);
            _alignLeftButton.Margin = new Padding(6, 0, 2, 0);
            _alignLeftButton.Name = "_alignLeftButton";
            _alignLeftButton.Size = new Size(28, 28);
            _alignLeftButton.TabIndex = 4;
            _alignLeftButton.Text = "";
            _tips.SetToolTip(_alignLeftButton, "Alinear a la izquierda");
            _alignLeftButton.UseVisualStyleBackColor = false;
            // 
            // _alignCenterButton
            // 
            _alignCenterButton.FlatAppearance.BorderColor = SystemColors.ControlDark;
            _alignCenterButton.FlatStyle = FlatStyle.Flat;
            _alignCenterButton.IconSize = 11F;
            _alignCenterButton.Location = new Point(188, 0);
            _alignCenterButton.Margin = new Padding(0, 0, 2, 0);
            _alignCenterButton.Name = "_alignCenterButton";
            _alignCenterButton.Size = new Size(28, 28);
            _alignCenterButton.TabIndex = 5;
            _alignCenterButton.Text = "";
            _tips.SetToolTip(_alignCenterButton, "Centrar");
            _alignCenterButton.UseVisualStyleBackColor = false;
            // 
            // _alignRightButton
            // 
            _alignRightButton.FlatAppearance.BorderColor = SystemColors.ControlDark;
            _alignRightButton.FlatStyle = FlatStyle.Flat;
            _alignRightButton.IconSize = 11F;
            _alignRightButton.Location = new Point(218, 0);
            _alignRightButton.Margin = new Padding(0, 0, 2, 0);
            _alignRightButton.Name = "_alignRightButton";
            _alignRightButton.Size = new Size(28, 28);
            _alignRightButton.TabIndex = 6;
            _alignRightButton.Text = "";
            _tips.SetToolTip(_alignRightButton, "Alinear a la derecha");
            _alignRightButton.UseVisualStyleBackColor = false;
            // 
            // _conversionLabel
            // 
            _conversionLabel.AutoSize = true;
            _conversionLabel.Location = new Point(0, 6);
            _conversionLabel.Margin = new Padding(0, 6, 4, 0);
            _conversionLabel.Name = "_conversionLabel";
            _conversionLabel.Size = new Size(157, 20);
            _conversionLabel.TabIndex = 0;
            _conversionLabel.Text = "Paso a blanco y negro:";
            // 
            // _conversionBox
            // 
            _conversionBox.DropDownStyle = ComboBoxStyle.DropDownList;
            _conversionBox.Items.AddRange(new object[] { "Tramado", "Umbral" });
            _conversionBox.Location = new Point(0, 29);
            _conversionBox.Margin = new Padding(0, 3, 0, 0);
            _conversionBox.Name = "_conversionBox";
            _conversionBox.Size = new Size(110, 28);
            _conversionBox.TabIndex = 1;
            // 
            // _propertiesTitle
            // 
            _propertiesTitle.AutoSize = true;
            _propertiesTitle.Dock = DockStyle.Top;
            _propertiesTitle.Location = new Point(8, 6);
            _propertiesTitle.Name = "_propertiesTitle";
            _propertiesTitle.Padding = new Padding(0, 0, 0, 4);
            _propertiesTitle.Size = new Size(92, 24);
            _propertiesTitle.TabIndex = 1;
            _propertiesTitle.Text = "Propiedades";
            // 
            // _toolsSection
            // 
            _toolsSection.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            _toolsSection.Controls.Add(_blackButton);
            _toolsSection.Controls.Add(_whiteButton);
            _toolsSection.Controls.Add(_brush);
            _toolsSection.Controls.Add(_brushButton);
            _toolsSection.Controls.Add(_toolsRow2);
            _toolsSection.Controls.Add(_toolsContent);
            _toolsSection.Controls.Add(_textButton);
            _toolsSection.Controls.Add(_cursorButton);
            _toolsSection.Controls.Add(_insertButton);
            _toolsSection.Controls.Add(_printButton);
            _toolsSection.Controls.Add(_pasteButton);
            _toolsSection.Controls.Add(_deleteButton);
            _toolsSection.Controls.Add(_commitButton);
            _toolsSection.Dock = DockStyle.Top;
            _toolsSection.Location = new Point(4, 4);
            _toolsSection.Name = "_toolsSection";
            _toolsSection.Padding = new Padding(8, 6, 8, 8);
            _toolsSection.Size = new Size(272, 77);
            _toolsSection.TabIndex = 0;
            _toolsSection.Tag = "help:herramientas";
            // 
            // _toolsRow2
            // 
            _toolsRow2.AutoSize = true;
            _toolsRow2.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            _toolsRow2.Location = new Point(11, 47);
            _toolsRow2.Margin = new Padding(0);
            _toolsRow2.Name = "_toolsRow2";
            _toolsRow2.Size = new Size(0, 0);
            _toolsRow2.TabIndex = 1;
            // 
            // _toolsContent
            // 
            _toolsContent.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _toolsContent.AutoSize = true;
            _toolsContent.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            _toolsContent.Controls.Add(_toolsRow1);
            _toolsContent.FlowDirection = FlowDirection.TopDown;
            _toolsContent.Location = new Point(0, 0);
            _toolsContent.Name = "_toolsContent";
            _toolsContent.Size = new Size(0, 0);
            _toolsContent.TabIndex = 0;
            _toolsContent.WrapContents = false;
            // 
            // _toolsRow1
            // 
            _toolsRow1.AutoSize = true;
            _toolsRow1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            _toolsRow1.Location = new Point(0, 0);
            _toolsRow1.Margin = new Padding(0);
            _toolsRow1.Name = "_toolsRow1";
            _toolsRow1.Size = new Size(0, 0);
            _toolsRow1.TabIndex = 0;
            // 
            // MainForm
            // 
            AllowDrop = true;
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(1192, 893);
            Controls.Add(_canvas);
            Controls.Add(_sidePanel);
            Controls.Add(_menu);
            Controls.Add(_status);
            KeyPreview = true;
            MainMenuStrip = _menu;
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "PhomemoPaint";
            ((System.ComponentModel.ISupportInitialize)_brush).EndInit();
            _menu.ResumeLayout(false);
            _menu.PerformLayout();
            _status.ResumeLayout(false);
            _status.PerformLayout();
            _sidePanel.ResumeLayout(false);
            _sidePanel.PerformLayout();
            _zoomSection.ResumeLayout(false);
            _printerSection.ResumeLayout(false);
            _printerSection.PerformLayout();
            _printerContent.ResumeLayout(false);
            _printerContent.PerformLayout();
            _portRow.ResumeLayout(false);
            _labelWidthRow.ResumeLayout(false);
            _labelWidthRow.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)_labelWidth).EndInit();
            _propertiesSection.ResumeLayout(false);
            _propertiesSection.PerformLayout();
            _propertiesContent.ResumeLayout(false);
            _propertiesContent.PerformLayout();
            _selectionProps.ResumeLayout(false);
            _selectionProps.PerformLayout();
            _textProps.ResumeLayout(false);
            _textProps.PerformLayout();
            _fontRow.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)_fontSizeBox).EndInit();
            _toolsSection.ResumeLayout(false);
            _toolsSection.PerformLayout();
            _toolsContent.ResumeLayout(false);
            _toolsContent.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolTip _tips;
        private System.Windows.Forms.MenuStrip _menu;
        private System.Windows.Forms.ToolStripMenuItem _fileMenu;
        private System.Windows.Forms.ToolStripMenuItem _newItem;
        private System.Windows.Forms.ToolStripMenuItem _openCanvasItem;
        private System.Windows.Forms.ToolStripMenuItem _insertItem;
        private System.Windows.Forms.ToolStripMenuItem _insertTextItem;
        private System.Windows.Forms.ToolStripMenuItem _exportItem;
        private System.Windows.Forms.ToolStripSeparator _fileSeparator;
        private System.Windows.Forms.ToolStripMenuItem _exitItem;
        private System.Windows.Forms.ToolStripMenuItem _editMenu;
        private System.Windows.Forms.ToolStripMenuItem _undoItem;
        private System.Windows.Forms.ToolStripMenuItem _redoItem;
        private System.Windows.Forms.ToolStripSeparator _editSeparator;
        private System.Windows.Forms.ToolStripMenuItem _pasteItem;
        private System.Windows.Forms.ToolStripMenuItem _commitItem;
        private System.Windows.Forms.ToolStripMenuItem _commitAllItem;
        private System.Windows.Forms.ToolStripMenuItem _deleteItem;
        private System.Windows.Forms.ToolStripMenuItem _viewMenu;
        private System.Windows.Forms.ToolStripMenuItem _zoomInItem;
        private System.Windows.Forms.ToolStripMenuItem _zoomOutItem;
        private System.Windows.Forms.ToolStripMenuItem _zoomFitItem;
        private System.Windows.Forms.ToolStripMenuItem _printerMenu;
        private System.Windows.Forms.ToolStripMenuItem _printItem;
        private System.Windows.Forms.ToolStripMenuItem _detectItem;
        private System.Windows.Forms.ToolStripSeparator _printerSeparator;
        private System.Windows.Forms.ToolStripMenuItem _calibrationItem;
        private System.Windows.Forms.ToolStripMenuItem _diagnosticsItem;
        private System.Windows.Forms.ToolStripMenuItem _helpMenu;
        private System.Windows.Forms.ToolStripMenuItem _helpItem;
        private System.Windows.Forms.ToolStripMenuItem _tipsItem;
        private System.Windows.Forms.ToolStripMenuItem _problemsItem;
        private System.Windows.Forms.ToolStripSeparator _helpSeparator;
        private System.Windows.Forms.ToolStripMenuItem _aboutItem;
        private System.Windows.Forms.StatusStrip _status;
        private System.Windows.Forms.ToolStripStatusLabel _cursorLabel;
        private System.Windows.Forms.ToolStripStatusLabel _extentLabel;
        private System.Windows.Forms.ToolStripStatusLabel _warningLabel;
        private System.Windows.Forms.ToolStripStatusLabel _statusLabel;
        private System.Windows.Forms.ToolStripProgressBar _progress;
        private PhomemoPaint.Editing.CanvasControl _canvas;
        private System.Windows.Forms.Panel _sidePanel;
        private System.Windows.Forms.Panel _toolsSection;
        private System.Windows.Forms.FlowLayoutPanel _toolsContent;
        private System.Windows.Forms.FlowLayoutPanel _toolsRow1;
        private IconButton _brushButton;
        private IconButton _textButton;
        private IconButton _cursorButton;
        private IconButton _insertButton;
        private IconButton _pasteButton;
        private IconButton _commitButton;
        private IconButton _deleteButton;
        private IconButton _printButton;
        private System.Windows.Forms.FlowLayoutPanel _toolsRow2;
        private System.Windows.Forms.Panel _propertiesSection;
        private System.Windows.Forms.FlowLayoutPanel _propertiesContent;
        private System.Windows.Forms.Label _noSelection;
        private System.Windows.Forms.FlowLayoutPanel _selectionProps;
        private System.Windows.Forms.FlowLayoutPanel _textProps;
        private System.Windows.Forms.TextBox _textBox;
        private System.Windows.Forms.ComboBox _fontBox;
        private System.Windows.Forms.FlowLayoutPanel _fontRow;
        private System.Windows.Forms.NumericUpDown _fontSizeBox;
        private IconButton _boldButton;
        private IconButton _italicButton;
        private IconButton _underlineButton;
        private IconButton _alignLeftButton;
        private IconButton _alignCenterButton;
        private IconButton _alignRightButton;
        private System.Windows.Forms.Label _conversionLabel;
        private System.Windows.Forms.ComboBox _conversionBox;
        private System.Windows.Forms.Label _propertiesTitle;
        private System.Windows.Forms.Panel _printerSection;
        private System.Windows.Forms.FlowLayoutPanel _printerContent;
        private System.Windows.Forms.FlowLayoutPanel _portRow;
        private System.Windows.Forms.ComboBox _portBox;
        private IconButton _refreshButton;
        private System.Windows.Forms.ComboBox _modelBox;
        private System.Windows.Forms.Label _compressionWarning;
        private System.Windows.Forms.FlowLayoutPanel _labelWidthRow;
        private System.Windows.Forms.Label _labelWidthLabel;
        private System.Windows.Forms.NumericUpDown _labelWidth;
        private PhomemoPaint.PrinterInfoPanel _infoPanel;
        private System.Windows.Forms.Label _printerTitle;
        private System.Windows.Forms.Panel _zoomSection;
        private IconButton _zoomOutButton;
        private System.Windows.Forms.ComboBox _zoomBox;
        private IconButton _zoomInButton;
        private IconButton _zoomFitButton;
        private RadioButton _blackButton;
        private RadioButton _whiteButton;
        private NumericUpDown _brush;
    }
}
