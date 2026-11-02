using System.Globalization;

namespace PhomemoPaint;

/// <summary>
/// Textos en español e inglés según la cultura de la interfaz. Se fuerza con la variable de
/// entorno PHOMEMOPAINT_LANG=es|en.
/// </summary>
internal static class S
{
    public static bool Spanish { get; } = DetectSpanish();

    private static bool DetectSpanish()
    {
        var forced = Environment.GetEnvironmentVariable("PHOMEMOPAINT_LANG");
        if (!string.IsNullOrWhiteSpace(forced))
            return forced.Trim().StartsWith("es", StringComparison.OrdinalIgnoreCase);
        return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "es";
    }

    private static string T(string es, string en) => Spanish ? es : en;

    // Menús
    public static string MenuFile => T("&Archivo", "&File");
    public static string MenuNew => T("&Nuevo (borrar todo)", "&New (clear all)");
    public static string MenuOpenCanvas => T("&Abrir PNG como lienzo...", "&Open PNG as canvas...");
    public static string MenuInsertImage => T("&Insertar imagen...", "&Insert image...");
    public static string MenuInsertText => T("Insertar &texto", "Insert &text");
    public static string MenuExportPng => T("&Exportar PNG...", "&Export PNG...");
    public static string MenuExit => T("&Salir", "E&xit");
    public static string MenuEdit => T("&Editar", "&Edit");
    public static string MenuUndo => T("&Deshacer", "&Undo");
    public static string MenuRedo => T("&Rehacer", "&Redo");
    public static string MenuPaste => T("&Pegar imagen", "&Paste image");
    public static string MenuCommit => T("&Fijar imagen", "&Fix image");
    public static string MenuCommitAll => T("Fijar &todas las imágenes", "Fix &all images");
    public static string MenuDelete => T("&Eliminar imagen", "&Delete image");
    public static string MenuView => T("&Ver", "&View");
    public static string MenuZoomIn => T("&Acercar", "Zoom &in");
    public static string MenuZoomOut => T("A&lejar", "Zoom &out");
    public static string MenuZoomFit => T("&Ajustar al ancho", "&Fit width");
    public static string MenuHelp => T("Ay&uda", "&Help");
    public static string MenuHelpContents => T("&Ayuda de PhomemoPaint", "PhomemoPaint &help");
    public static string MenuHelpTips => T("&Trucos y atajos", "&Tips and shortcuts");
    public static string MenuHelpProblems => T("&Si algo no funciona", "&Troubleshooting");
    public static string MenuAbout => T("Acerca &de PhomemoPaint", "&About PhomemoPaint");
    public static string HelpError(string msg) => T($"No se pudo abrir la ayuda:\n\n{msg}", $"Could not open the help:\n\n{msg}");
    public static string MenuPrinter => T("&Impresora", "&Printer");
    public static string MenuPrint => T("&Imprimir", "&Print");
    public static string MenuDetect => T("&Detectar modelo", "&Detect model");
    public static string MenuCalibration => T("&Patrón de calibración", "&Calibration pattern");
    public static string MenuDiagnostics => T("Diagnóstico de &señales...", "&Signal diagnostics...");

    // Barras
    public static string Port => T("Puerto:", "Port:");
    public static string Refresh => T("Actualizar puertos y buscar por Bluetooth", "Refresh ports and search Bluetooth");
    public static string Model => T("Modelo:", "Model:");
    public static string LabelWidth => T("Etiqueta (mm):", "Label (mm):");
    public static string CanvasHeight => T("Alto (puntos):", "Height (dots):");
    public static string Print => T("Imprimir", "Print");
    public static string Cancel => T("Cancelar", "Cancel");
    public static string Black => T("Negro", "Black");
    public static string White => T("Blanco", "White");
    public static string BrushSize => T("Tamaño del pincel", "Brush size");
    public static string Zoom => T("Zoom:", "Zoom:");
    public static string Conversion => T("B/N:", "B/W:");
    public static string PropertiesTitle => T("Propiedades", "Properties");
    public static string ToolBrush => T("Pincel (botón derecho: el otro color)", "Brush (right button: the other color)");
    public static string ToolText => T("Texto: haz clic en el lienzo para colocarlo", "Text: click on the canvas to place it");
    public static string ToolCursor => T("Cursor: selecciona y mueve objetos sin pintar", "Cursor: select and move objects without painting");
    public static string DrawingSelected => T("Dibujo a pincel: se puede mover, fijar o eliminar.", "Brush drawing: it can be moved, fixed or deleted.");
    public static string TextSample => T("Texto", "Text");
    public static string FontFamilyTip => T("Fuente", "Font");
    public static string FontSizeTip => T("Tamaño en puntos tipográficos", "Size in typographic points");
    public static string Bold => T("Negrita", "Bold");
    public static string Italic => T("Cursiva", "Italic");
    public static string Underline => T("Subrayado", "Underline");
    public static string AlignLeft => T("Alinear a la izquierda", "Align left");
    public static string AlignCenter => T("Centrar", "Center");
    public static string AlignRight => T("Alinear a la derecha", "Align right");
    public static string NoSelection => T("Nada seleccionado. Haz clic en una imagen o un texto para ver sus propiedades.", "Nothing selected. Click an image or a text to see its properties.");
    public static string ConversionLong => T("Paso a blanco y negro:", "Black and white:");
    public static string Dither => T("Tramado", "Dither");
    public static string Threshold => T("Umbral", "Threshold");
    public static string NoPorts => T("(sin puertos)", "(no ports)");
    public static string NotVerified => T("no verificado", "not verified");
    public static string Experimental => T("experimental", "experimental");
    public static string NeedsCompression => T("requiere compresión, no imprime", "needs compression, cannot print");
    public static string CompressionWarning(string model) =>
        T($"⚠ {model} necesita datos comprimidos (LZO), que aún no están implementados: no se puede imprimir.",
          $"⚠ {model} needs compressed data (LZO), not implemented yet: printing is disabled.");

    // Estado
    public static string Ready => T("Listo", "Ready");
    public static string PrintExtent(int dots, double mm) =>
        dots == 0 ? T("Nada que imprimir", "Nothing to print") : T($"Se imprimen {dots} líneas ({mm:0.0} mm)", $"Prints {dots} lines ({mm:0.0} mm)");
    public static string CursorPos(int x, int y, double xmm, double ymm) => $"{x}, {y}  ({xmm:0.0}, {ymm:0.0} mm)";
    public static string Detected(string model, string how) => T($"Detectado {model} ({how})", $"Detected {model} ({how})");
    public static string DetectedByUsb(ushort vid, ushort pid) => $"USB {vid:X4}:{pid:X4}";
    public static string DetectedByBluetooth => "Bluetooth";
    public static string NotDetected => T("No se reconoce la impresora de este puerto: elige el modelo a mano.", "Printer on this port not recognised: choose the model manually.");
    public static string Printing => T("Imprimiendo...", "Printing...");
    public static string Printed(int lines, double mm, string response) =>
        T($"Impreso: {lines} líneas ({mm:0.0} mm). {response}", $"Printed: {lines} lines ({mm:0.0} mm). {response}");
    public static string Sent(int lines, double mm, string response) =>
        T($"Enviado: {lines} líneas ({mm:0.0} mm), sin confirmación de la impresora. {response}",
          $"Sent: {lines} lines ({mm:0.0} mm), not confirmed by the printer. {response}");
    public static string NoResponse => T("Sin respuesta.", "No response.");
    public static string PrintCancelled => T("Impresión cancelada", "Print cancelled");
    public static string Scanning => T("Buscando impresoras Bluetooth...", "Searching for Bluetooth printers...");
    public static string ScanDone(int count) => count == 0
        ? T("No se ha encontrado ninguna impresora Bluetooth (¿está encendida?).", "No Bluetooth printer found (is it on?).")
        : T($"Impresoras Bluetooth encontradas: {count}", $"Bluetooth printers found: {count}");
    public static string ScanFailed(string msg) => T($"No se pudo buscar por Bluetooth: {msg}", $"Bluetooth search failed: {msg}");
    public static string SwitchedToBluetooth(string model) =>
        T($"{model} encontrada por Bluetooth: se usa esa conexión (por USB no imprime).",
          $"{model} found over Bluetooth: using that connection (it does not print over USB).");
    public static string UsbHint(string model) =>
        T($" Si sale en blanco: la {model} solo imprime por Bluetooth.", $" If it comes out blank: the {model} only prints over Bluetooth.");

    // Panel de la impresora
    public static string InfoTitle => T("Impresora", "Printer");
    public static string InfoModel => T("Modelo", "Model");
    public static string InfoConnection => T("Conexión", "Connection");
    public static string InfoAddress => T("Dirección", "Address");
    public static string InfoSerial => T("N.º de serie", "Serial no.");
    public static string InfoFirmware => T("Firmware", "Firmware");
    public static string InfoBattery => T("Batería", "Battery");
    public static string InfoPaper => T("Papel", "Paper");
    public static string InfoCover => T("Tapa", "Cover");
    public static string InfoCoverClosed => T("cerrada", "closed");
    public static string InfoUpdated => T("Leído", "Read at");
    public static string InfoRefresh => T("Consultar", "Query");
    public static string InfoQuerying => T("consultando...", "querying...");
    public static string InfoNoAnswer => T("sin respuesta", "no answer");
    public static string InfoPaperOk => T("sí", "yes");
    public static string InfoSerialPort => T("Puerto serie", "Serial port");
    public static string DetectedBySerial => T("número de serie", "serial number");
    public static string WaitingForPrinter => T("Esperando a la impresora...", "Waiting for the printer...");

    // Mensajes de la impresora
    public static string MsgFinished => T("terminado", "finished");
    public static string MsgBattery(int pct) => T($"batería {pct} %", $"battery {pct}%");
    public static string MsgPaperOut => T("SIN PAPEL", "OUT OF PAPER");
    public static string NoPaper => T("La impresora no tiene papel. Pon un rollo y vuelve a imprimir.", "The printer is out of paper. Load a roll and print again.");
    public static string CoverIsOpen => T("La tapa de la impresora está abierta. Ciérrala y vuelve a imprimir.", "The printer cover is open. Close it and print again.");
    public static string MsgCoverOpen => T("TAPA ABIERTA", "COVER OPEN");
    public static string MsgOverheated => T("CABEZAL CALIENTE", "HEAD OVERHEATED");
    public static string MsgFirmware(string v) => T($"firmware {v}", $"firmware {v}");
    public static string MsgSerial(string sn) => T($"n.º de serie {sn}", $"serial {sn}");
    public static string MsgAutoOff(int min) => T($"apagado automático {min} min", $"auto-off {min} min");

    // Mensajes
    public static string AppTitle => "PhomemoPaint";
    public static string NothingToPrint => T("El lienzo está en blanco: no hay nada que imprimir.", "The canvas is blank: there is nothing to print.");
    public static string ChoosePort => T("Elige el puerto de la impresora.", "Choose the printer port.");
    public static string PrintError(string msg) => T($"No se pudo imprimir:\n\n{msg}", $"Could not print:\n\n{msg}");
    public static string PortBusy(string port) => T($"El puerto {port} está ocupado por otro programa o no existe.", $"Port {port} is in use by another program or does not exist.");
    public static string WidthCutNotice(string model, int dots, double mm) =>
        T($"⚠ {model}: el lienzo pasa a {dots} puntos ({mm:0.#} mm) y se ha recortado la derecha. Ctrl+Z deshace.",
          $"⚠ {model}: the canvas is now {dots} dots ({mm:0.#} mm) and the right side was cut off. Ctrl+Z undoes it.");
    public static string HeightCutNotice(int rows, double mm) =>
        T($"⚠ Se ha recortado el dibujo por debajo de {rows} puntos ({mm:0.#} mm). Ctrl+Z deshace.",
          $"⚠ The drawing below {rows} dots ({mm:0.#} mm) was cut off. Ctrl+Z undoes it.");
    public static string ObjectsOutside =>
        T("⚠ Un objeto sobresale del papel: esa parte no se imprimirá.", "⚠ An object sticks out of the paper: that part will not be printed.");
    public static string ConfirmClear => T("¿Borrar todo el lienzo?", "Clear the whole canvas?");
    public static string ConfirmCalibration => T("El patrón de calibración sustituye a lo dibujado. ¿Continuar?", "The calibration pattern replaces the drawing. Continue?");
    public static string ImageFilter => T("Imágenes|*.png;*.jpg;*.jpeg;*.bmp;*.gif|Todos los ficheros|*.*", "Images|*.png;*.jpg;*.jpeg;*.bmp;*.gif|All files|*.*");
    public static string PngFilter => T("PNG|*.png", "PNG|*.png");
    public static string NoImageInClipboard => T("El portapapeles no contiene ninguna imagen.", "The clipboard contains no image.");
    public static string CannotOpen(string msg) => T($"No se pudo abrir la imagen:\n\n{msg}", $"Could not open the image:\n\n{msg}");
    public static string CanvasTooWide(int imgW, int canvasW) =>
        T($"La imagen tiene {imgW} puntos de ancho y el lienzo {canvasW}: se recortará la derecha.",
          $"The image is {imgW} dots wide and the canvas {canvasW}: the right side will be cut off.");

    // Calibración
    public static string CalibrationTitle(string model, int dots, int dpi) => $"{model}  {dots} pt  {dpi} dpi";

    // Diagnóstico
    public static string DiagTitle => T("Diagnóstico de señales", "Signal diagnostics");
    public static string DiagHelp => T(
        "Envía cada línea (bytes en hexadecimal) y muestra la respuesta. Úsalo solo con consultas: un comando " +
        "desconocido puede cambiar la configuración de la impresora. 1F 11 14 (actualizar firmware) está bloqueado.",
        "Sends each line (hex bytes) and shows the response. Use it only with queries: an unknown command may " +
        "change the printer settings. 1F 11 14 (firmware update) is blocked.");
    public static string DiagBlocked(string line) => T(
        $"Bloqueado: {line} inicia una actualización de firmware.", $"Blocked: {line} starts a firmware update.");
    public static string DiagSend => T("Enviar", "Send");
    public static string DiagCopy => T("Copiar resultado", "Copy result");
    public static string DiagBadHex(string line) => T($"Línea no válida: {line}", $"Invalid line: {line}");
    public static string DiagNoAnswer => T("(sin respuesta)", "(no answer)");
}
