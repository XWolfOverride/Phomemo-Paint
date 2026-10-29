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
    public static string BrushSize => T("Pincel:", "Brush:");
    public static string Zoom => T("Zoom:", "Zoom:");
    public static string Conversion => T("B/N:", "B/W:");
    public static string Dither => T("Tramado", "Dither");
    public static string Threshold => T("Umbral", "Threshold");
    public static string NoPorts => T("(sin puertos)", "(no ports)");
    public static string NotVerified => T("no verificado", "not verified");
    public static string Experimental => T("experimental", "experimental");

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

    // Mensajes de la impresora
    public static string MsgFinished => T("terminado", "finished");
    public static string MsgBattery(int pct) => T($"batería {pct} %", $"battery {pct}%");
    public static string MsgPaperOut => T("SIN PAPEL", "OUT OF PAPER");
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
    public static string WidthChange(string model, int newDots, double newMm, int oldDots, bool narrower) =>
        T($"{model} imprime {newDots} puntos de ancho ({newMm:0.#} mm); el lienzo tiene {oldDots}.\n\n" +
          (narrower ? "Se recortará la parte derecha de lo dibujado (no se escala)." : "Se añadirá blanco a la derecha (no se escala).") +
          "\n\n¿Cambiar el ancho?",
          $"{model} prints {newDots} dots wide ({newMm:0.#} mm); the canvas has {oldDots}.\n\n" +
          (narrower ? "The right part of the drawing will be cut off (no scaling)." : "White will be added on the right (no scaling).") +
          "\n\nChange the width?");
    public static string HeightCut(int rows) =>
        T($"Hay dibujo por debajo de la fila {rows}. Se perderá al reducir el alto.\n\n¿Continuar?",
          $"There is drawing below row {rows}. It will be lost when reducing the height.\n\nContinue?");
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
