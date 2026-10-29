using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;

namespace Phomemo.Printing.Windows;

/// <summary>
/// Busca impresoras Bluetooth LE por sus anuncios: nombre reconocible por el catálogo
/// ("T02", "Mr.in_M02"…) o servicio anunciado <c>AF30</c>/<c>FF00</c> (la T02 anuncia AF30).
/// No hace falta emparejar.
/// </summary>
public static class BleScanner
{
    private static readonly Guid[] PrinterServices =
    [
        BluetoothUuidHelper.FromShortId(0xAF30),
        BluetoothUuidHelper.FromShortId(0xFF00),
    ];

    public static async Task<IReadOnlyList<PortInfo>> ScanAsync(TimeSpan duration, CancellationToken ct = default)
    {
        var names = new Dictionary<ulong, string?>();
        var candidates = new HashSet<ulong>();
        var watcher = new BluetoothLEAdvertisementWatcher { ScanningMode = BluetoothLEScanningMode.Active };
        watcher.Received += (_, e) =>
        {
            lock (names)
            {
                var name = e.Advertisement.LocalName;
                if (!string.IsNullOrWhiteSpace(name) || !names.ContainsKey(e.BluetoothAddress))
                    names[e.BluetoothAddress] = string.IsNullOrWhiteSpace(name) ? names.GetValueOrDefault(e.BluetoothAddress) : name;
                if (e.Advertisement.ServiceUuids.Any(PrinterServices.Contains) ||
                    PrinterCatalog.FindByBluetoothName(name) is not null)
                    candidates.Add(e.BluetoothAddress);
            }
        };
        watcher.Start();
        try
        {
            await Task.Delay(duration, ct).ConfigureAwait(false);
        }
        finally
        {
            watcher.Stop();
        }
        lock (names)
        {
            return candidates
                .Select(a => PortInfo.BluetoothLE(BleAddress.Format(a), names.GetValueOrDefault(a)))
                .OrderBy(p => p.BluetoothName, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
    }
}
