using Phomemo.Printing.Transport;

namespace Phomemo.Printing;

public enum PortKind
{
    /// <summary>Puerto serie: USB CDC-ACM o Bluetooth clásico (SPP).</summary>
    Serial,
    /// <summary>Bluetooth LE (GATT). <see cref="PortInfo.Address"/> es la MAC.</summary>
    BluetoothLE,
}

/// <summary>Un puerto o dispositivo y lo que el sistema sepa de él.</summary>
/// <param name="Name">Identificador para abrirlo y recordarlo (COM3, /dev/ttyACM0, BLE:AA:BB:…).</param>
/// <param name="Description">Texto descriptivo del sistema, si lo hay.</param>
/// <param name="UsbVendorId">VID si es un dispositivo USB.</param>
/// <param name="UsbProductId">PID si es un dispositivo USB.</param>
/// <param name="BluetoothName">Nombre del dispositivo si es Bluetooth.</param>
/// <param name="Kind">Tipo de conexión.</param>
/// <param name="Address">Dirección Bluetooth (AA:BB:CC:DD:EE:FF) en <see cref="PortKind.BluetoothLE"/>.</param>
public sealed record PortInfo(
    string Name,
    string? Description = null,
    ushort? UsbVendorId = null,
    ushort? UsbProductId = null,
    string? BluetoothName = null,
    PortKind Kind = PortKind.Serial,
    string? Address = null)
{
    public const string BlePrefix = "BLE:";

    /// <summary>Dispositivo Bluetooth LE con su nombre anunciado.</summary>
    public static PortInfo BluetoothLE(string address, string? name) =>
        new(BlePrefix + address, "Bluetooth", BluetoothName: name, Kind: PortKind.BluetoothLE, Address: address);

    public override string ToString() => Kind == PortKind.BluetoothLE
        ? $"{BluetoothName ?? "?"} — Bluetooth {Address}"
        : string.IsNullOrEmpty(Description) ? Name : $"{Name} — {Description}";
}

/// <summary>
/// Enumera puertos. Saber el VID/PID o el nombre Bluetooth de un puerto necesita APIs de cada
/// sistema, así que las implementaciones específicas viven fuera de esta biblioteca.
/// </summary>
public interface IPortInfoProvider
{
    IReadOnlyList<PortInfo> GetPorts();
}

/// <summary>Solo nombres de puerto, sin datos USB. Funciona en cualquier sistema.</summary>
public sealed class BasicPortInfoProvider : IPortInfoProvider
{
    public IReadOnlyList<PortInfo> GetPorts() =>
        SerialPortTransport.ListPorts().Select(n => new PortInfo(n)).ToArray();
}

public enum DetectionSource
{
    None,
    UsbId,
    BluetoothName,
}

/// <summary>Modelo sugerido para un puerto y de dónde sale la sugerencia.</summary>
public sealed record DetectionResult(PrinterModel? Model, DetectionSource Source)
{
    public static DetectionResult NotFound { get; } = new(null, DetectionSource.None);
}

/// <summary>Detección de mejor esfuerzo: siempre debe poder corregirla el usuario.</summary>
public static class ModelDetector
{
    public static DetectionResult Detect(PortInfo port)
    {
        ArgumentNullException.ThrowIfNull(port);
        if (port.UsbVendorId is { } vid && port.UsbProductId is { } pid &&
            PrinterCatalog.FindByUsbId(vid, pid) is { } byUsb)
            return new DetectionResult(byUsb, DetectionSource.UsbId);
        if (PrinterCatalog.FindByBluetoothName(port.BluetoothName) is { } byName)
            return new DetectionResult(byName, DetectionSource.BluetoothName);
        return DetectionResult.NotFound;
    }

    /// <summary>Primer puerto con una impresora reconocible.</summary>
    public static (PortInfo Port, DetectionResult Result)? FindFirst(IEnumerable<PortInfo> ports) =>
        ports.Select(p => (Port: p, Result: Detect(p)))
             .Where(t => t.Result.Model is not null)
             .Select(t => ((PortInfo, DetectionResult)?)t)
             .FirstOrDefault();
}
