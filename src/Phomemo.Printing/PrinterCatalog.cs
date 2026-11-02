namespace Phomemo.Printing;

/// <summary>Modelos conocidos e identificadores USB. Ver la tabla de DOC/protocolos.md.</summary>
public static class PrinterCatalog
{
    public static IReadOnlyList<PrinterModel> All { get; } =
    [
        // Densidad 3: la que mejor tramado da en las pruebas (2 y 4 también correctas).
        new("T02", "Phomemo T02", ProtocolFamily.M02, 203, 384, false, 0, 4, Verified: true, Experimental: false, DefaultDarkness: 3),
        new("M02", "Phomemo M02", ProtocolFamily.M02, 203, 384, false, 0, 2, Verified: true, Experimental: false),
        new("M02Pro", "Phomemo M02 Pro", ProtocolFamily.M02, 300, 576, false, 0, 2, Verified: false, Experimental: false),
        new("M02S", "Phomemo M02S", ProtocolFamily.M02, 300, 576, false, 0, 2, Verified: false, Experimental: false),
        // Variantes detectadas del grupo M02 (mismo protocolo; datos sin probar).
        .. M02Variants(203, 384, "M02C", "M02A", "M02D", "M02E", "M02X", "M02L", "M02X/L", "MR2", "KP-Q1", "Q02", "Y02C"),
        .. M02Variants(300, 576, "M02PRE", "M02S-H", "M02H", "Y02S"),
        // Grupo M03 (detectado): protocolo M02 sin probar.
        .. Group(ProtocolFamily.M02, 203, 576, 2, false, "M03", "M03A", "TP31"),
        .. Group(ProtocolFamily.M02, 300, 864, 2, false, "M03S", "M03AS"),
        // Etiquetas estrechas P3100/P1000 (detectado).
        .. Group(ProtocolFamily.M02, 203, 82, 8, false, "P3100", "P3100D", "P3100J", "P3100DJ", "P1000", "P1000C"),
        new("M110", "Phomemo M110", ProtocolFamily.M110, 203, 384, true, 40, 0, Verified: false, Experimental: false),
        new("M120", "Phomemo M120", ProtocolFamily.M110, 203, 384, true, 40, 0, Verified: false, Experimental: false),
        new("M220", "Phomemo M220", ProtocolFamily.M110, 203, 576, true, 40, 0, Verified: false, Experimental: false),
        new("M421", "Phomemo M421", ProtocolFamily.M110, 203, 812, true, 101.6, 0, Verified: false, Experimental: false),
        new("D30", "Phomemo D30", ProtocolFamily.D30, 203, 96, false, 0, 2, Verified: false, Experimental: true),
        new("M04S", "Phomemo M04S (50 mm)", ProtocolFamily.M04, 300, 590, false, 0, 6, Verified: true, Experimental: true),
        new("M04S-110", "Phomemo M04S (110 mm)", ProtocolFamily.M04, 300, 1232, false, 0, 6, Verified: true, Experimental: true),
        new("M832", "Phomemo M832 (110 mm)", ProtocolFamily.M832, 300, 1299, false, 0, 1, Verified: true, Experimental: true),
        // M04 detectadas.
        .. Group(ProtocolFamily.M04, 300, 1228, 6, false, "M04AS"),
        .. Group(ProtocolFamily.M04, 203, 832, 6, false, "M04AS-H"),
        // A4 (216 mm) detectadas sin compresión: protocolo M832 (un bloque, datos sin comprimir).
        .. Group(ProtocolFamily.M832, 203, 1728, 1, false,
            "M08F", "M08D", "M08F-WS", "A88U", "A28U", "A884", "TP81", "TP82", "TP83"),
        // A4 detectadas que trabajan con compresión LZO: en el catálogo pero no se pueden imprimir.
        .. Group(ProtocolFamily.M832, 203, 1728, 1, true,
            "A281M", "A282M", "A285M", "M832T", "PR20", "PR20Lite", "S821", "S821K", "S823"),
        .. Group(ProtocolFamily.M832, 300, 2551, 1, true,
            "G100", "G100Pro", "H831", "H832", "H833", "M831", "M833", "M834", "M835",
            "P831", "P832", "Q300", "Q301", "Q302", "R831", "T831", "M08FS"),
    ];

    private static IEnumerable<PrinterModel> M02Variants(int dpi, int width, params string[] ids) =>
        ids.Select(id => new PrinterModel(id, "Phomemo " + id, ProtocolFamily.M02, dpi, width, false, 0, 2,
            Verified: false, Experimental: false));

    /// <summary>Modelos detectados sin probar: no verificados y experimentales.</summary>
    private static IEnumerable<PrinterModel> Group(ProtocolFamily family, int dpi, int width, int feed,
        bool requiresCompression, params string[] ids) =>
        ids.Select(id => new PrinterModel(id, "Phomemo " + id, family, dpi, width, false, 0, feed,
            Verified: false, Experimental: true, RequiresCompression: requiresCompression));

    /// <summary>
    /// Identificadores USB (VID, PID) vistos en impresoras reales. El mismo modelo puede usar
    /// chips distintos según el lote, así que es solo una pista.
    /// </summary>
    public static IReadOnlyDictionary<(ushort Vid, ushort Pid), string> UsbIds { get; } =
        new Dictionary<(ushort, ushort), string>
        {
            [(0x4C4A, 0x4155)] = "T02",   // T02 probada ("Dispositivo serie USB", chip JieLi)
            [(0x0493, 0xB002)] = "M02",   // phomemo-tools, backend CUPS
            [(0x0493, 0x8760)] = "M110",  // phomemo-tools, backend CUPS
        };

    /// <summary>
    /// Prefijo del número de serie (4 primeros caracteres, consulta <c>1F 11 09</c>) → modelo.
    /// Detectados; solo uno de los de la T02 se ha comprobado con una impresora.
    /// </summary>
    public static IReadOnlyDictionary<string, string> SerialPrefixes { get; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["M001"] = "M02", ["Q096"] = "M02",
            ["Q003"] = "M02S", ["Q021"] = "M02S", ["Q071"] = "M02S", ["Q075"] = "M02S",
            ["Q005"] = "M02Pro", ["Q070"] = "M02Pro",
            ["Q022"] = "M02E",
            ["Q123"] = "M02X",
            ["Q127"] = "M02D", ["Q167"] = "M02D",
            ["Q020"] = "T02", ["Q095"] = "T02", ["Q114"] = "T02",
            ["Q008"] = "M03",
            ["Q016"] = "M04S",
            ["Q032"] = "P3100", ["Q078"] = "P3100",
            ["Q051"] = "P3100D",
        };

    public static PrinterModel Default => Get("T02");

    /// <summary>Modelo a partir del número de serie (por su prefijo), si se conoce.</summary>
    public static PrinterModel? FindBySerialNumber(string? serial) =>
        serial is { Length: >= 4 } && SerialPrefixes.TryGetValue(serial[..4], out var id) ? Find(id) : null;

    public static PrinterModel Get(string id) =>
        Find(id) ?? throw new KeyNotFoundException($"Modelo desconocido: {id}");

    public static PrinterModel? Find(string? id) =>
        All.FirstOrDefault(m => string.Equals(m.Id, id, StringComparison.OrdinalIgnoreCase));

    public static PrinterModel? FindByUsbId(ushort vid, ushort pid) =>
        UsbIds.TryGetValue((vid, pid), out var id) ? Find(id) : null;

    /// <summary>
    /// Modelo a partir del nombre Bluetooth: "Mr.in_M02", "T02", "M832…" (backend CUPS).
    /// </summary>
    public static PrinterModel? FindByBluetoothName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        name = name.Trim();
        if (name.StartsWith("Mr.in_", StringComparison.OrdinalIgnoreCase))
            name = name[6..].Replace(" ", "");
        if (Find(name) is { } exact)
            return exact;
        if (name.StartsWith("M832", StringComparison.OrdinalIgnoreCase))
            return Find("M832");
        return null;
    }
}
