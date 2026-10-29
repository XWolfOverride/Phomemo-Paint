namespace Phomemo.Printing;

/// <summary>Modelos conocidos e identificadores USB. Ver la tabla de DOC/protocolos.md.</summary>
public static class PrinterCatalog
{
    public static IReadOnlyList<PrinterModel> All { get; } =
    [
        new("T02", "Phomemo T02", ProtocolFamily.M02, 203, 384, false, 0, 4, Verified: true, Experimental: false),
        new("M02", "Phomemo M02", ProtocolFamily.M02, 203, 384, false, 0, 2, Verified: true, Experimental: false),
        new("M02Pro", "Phomemo M02 Pro", ProtocolFamily.M02, 300, 576, false, 0, 2, Verified: false, Experimental: false),
        new("M02S", "Phomemo M02S", ProtocolFamily.M02, 300, 576, false, 0, 2, Verified: false, Experimental: false),
        new("M110", "Phomemo M110", ProtocolFamily.M110, 203, 384, true, 40, 0, Verified: false, Experimental: false),
        new("M120", "Phomemo M120", ProtocolFamily.M110, 203, 384, true, 40, 0, Verified: false, Experimental: false),
        new("M220", "Phomemo M220", ProtocolFamily.M110, 203, 576, true, 40, 0, Verified: false, Experimental: false),
        new("M421", "Phomemo M421", ProtocolFamily.M110, 203, 812, true, 101.6, 0, Verified: false, Experimental: false),
        new("D30", "Phomemo D30", ProtocolFamily.D30, 203, 96, false, 0, 2, Verified: false, Experimental: true),
        new("M04S", "Phomemo M04S (50 mm)", ProtocolFamily.M04, 300, 590, false, 0, 6, Verified: true, Experimental: true),
        new("M04S-110", "Phomemo M04S (110 mm)", ProtocolFamily.M04, 300, 1232, false, 0, 6, Verified: true, Experimental: true),
        new("M832", "Phomemo M832 (110 mm)", ProtocolFamily.M832, 300, 1299, false, 0, 1, Verified: true, Experimental: true),
    ];

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

    public static PrinterModel Default => Get("T02");

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
        if (name.StartsWith("M832", StringComparison.OrdinalIgnoreCase))
            return Find("M832");
        return Find(name);
    }
}
