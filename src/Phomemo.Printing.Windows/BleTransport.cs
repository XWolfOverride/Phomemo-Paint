using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Channels;
using Phomemo.Printing.Transport;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace Phomemo.Printing.Windows;

/// <summary>
/// Impresora por Bluetooth LE (GATT), sin emparejar. Servicio <c>FF00</c>: se escribe en
/// <c>FF02</c> y las respuestas llegan como notificaciones de <c>FF03</c>.
/// </summary>
/// <remarks>
/// Control de flujo por tokens (ver DOC/protocolos.md):
/// <list type="bullet">
/// <item><c>02 nn …</c>: tamaño de paquete (la T02 anuncia 182). Se manda en trozos de
/// <c>min(nn, MaxPduSize - 3)</c>.</item>
/// <item><c>01 nn</c>: la impresora concede nn tokens (llega uno al suscribirse y otro tras
/// cada paquete).</item>
/// <item><c>1A 36 nn</c>: paquetes por token (1 ⇒ 5; por defecto 1).</item>
/// </list>
/// Estos mensajes los consume el transporte; <see cref="ReadAsync"/> solo devuelve lo demás
/// (las respuestas <c>1A …</c>).
/// </remarks>
public sealed class BleTransport(ulong address) : IPrinterTransport
{
    public static readonly Guid ServiceUuid = BluetoothUuidHelper.FromShortId(0xFF00);
    public static readonly Guid WriteUuid = BluetoothUuidHelper.FromShortId(0xFF02);
    public static readonly Guid NotifyUuid = BluetoothUuidHelper.FromShortId(0xFF03);

    // Tamaño de paquete por defecto (hasta que la impresora anuncie otro).
    private const int DefaultPacketSize = 182;

    // Si la impresora no concede un token en este tiempo se deja de esperarlos y se pasa a
    // mandar con pausas fijas.
    private static readonly TimeSpan TokenTimeout = TimeSpan.FromMilliseconds(400);
    private static readonly TimeSpan FallbackDelay = TimeSpan.FromMilliseconds(20);

    private const int OpenAttempts = 3;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromMilliseconds(500);

    private readonly Channel<byte[]> _incoming = Channel.CreateUnbounded<byte[]>();
    private readonly SemaphoreSlim _tokens = new(0);
    private BluetoothLEDevice? _device;
    private GattDeviceService? _service;
    private GattCharacteristic? _write;
    private GattCharacteristic? _notify;
    private byte[] _pending = [];
    private int _maxPdu = DefaultPacketSize;
    private volatile int _packetSize = DefaultPacketSize;
    private volatile int _packetsPerToken = 1;
    private int _sentWithToken;
    private bool _flowControl = true;

    public BleTransport(string address) : this(BleAddress.Parse(address))
    {
    }

    public ulong Address { get; } = address;

    /// <summary>Traza opcional de la conexión (diagnóstico).</summary>
    public static Action<string>? Trace { get; set; }

    public bool IsOpen => _write is not null;

    public async Task OpenAsync(CancellationToken ct = default)
    {
        if (IsOpen) return;
        try
        {
            _device = await BluetoothLEDevice.FromBluetoothAddressAsync(Address).AsTask(ct).ConfigureAwait(false)
                      ?? throw new IOException($"No se encuentra el dispositivo Bluetooth {BleAddress.Format(Address)}.");
            // El GattDeviceService hay que liberarlo siempre: si queda abierto, la siguiente
            // conexión recibe características incompletas o no recibe notificaciones.
            GattCharacteristic? write = null;
            GattCommunicationStatus status = GattCommunicationStatus.Unreachable;
            for (int attempt = 1; ; attempt++)
            {
                var services = await _device.GetGattServicesForUuidAsync(ServiceUuid, BluetoothCacheMode.Uncached).AsTask(ct).ConfigureAwait(false);
                status = services.Status;
                foreach (var extra in services.Services.Skip(1)) extra.Dispose();
                _service = services.Services.FirstOrDefault();
                if (_service is not null)
                {
                    var chars = await _service.GetCharacteristicsAsync(BluetoothCacheMode.Uncached).AsTask(ct).ConfigureAwait(false);
                    write = chars.Characteristics.FirstOrDefault(c => c.Uuid == WriteUuid);
                    _notify = chars.Characteristics.FirstOrDefault(c => c.Uuid == NotifyUuid);
                    Trace?.Invoke($"intento {attempt}: estado {_device.ConnectionStatus}, características {chars.Status} ({chars.Characteristics.Count})");
                    if (write is not null && _notify is not null && await SubscribeAsync(_notify, ct).ConfigureAwait(false))
                        break;
                }
                if (attempt == OpenAttempts)
                {
                    if (write is null)
                        throw new IOException($"El dispositivo {BleAddress.Format(Address)} no responde o no tiene el servicio FF00/FF02 ({status}).");
                    // Se puede escribir pero no llegarán respuestas: sin control de flujo.
                    _notify = null;
                    _flowControl = false;
                    break;
                }
                _notify = null;
                _service?.Dispose();
                _service = null;
                await Task.Delay(RetryDelay, ct).ConfigureAwait(false);
            }
            using (var session = await GattSession.FromDeviceIdAsync(_device.BluetoothDeviceId).AsTask(ct).ConfigureAwait(false))
                _maxPdu = Math.Max(20, session.MaxPduSize - 3);
            _write = write;
        }
        catch
        {
            await DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }

    private async Task<bool> SubscribeAsync(GattCharacteristic notify, CancellationToken ct)
    {
        notify.ValueChanged -= OnNotify;
        notify.ValueChanged += OnNotify;
        var result = await notify.WriteClientCharacteristicConfigurationDescriptorWithResultAsync(
            GattClientCharacteristicConfigurationDescriptorValue.Notify).AsTask(ct).ConfigureAwait(false);
        if (result.Status == GattCommunicationStatus.Success) return true;
        notify.ValueChanged -= OnNotify;
        return false;
    }

    public async Task WriteAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default)
    {
        var write = _write ?? throw new InvalidOperationException("La conexión no está abierta.");
        int off = 0;
        while (off < data.Length)
        {
            int chunk = Math.Min(_packetSize, _maxPdu);
            var piece = data.Slice(off, Math.Min(chunk, data.Length - off)).ToArray();
            if (_flowControl)
            {
                // Cada token permite mandar _packetsPerToken paquetes.
                if (_sentWithToken == 0 && !await _tokens.WaitAsync(TokenTimeout, ct).ConfigureAwait(false))
                {
                    Trace?.Invoke($"sin token en {TokenTimeout.TotalMilliseconds} ms: pausas fijas");
                    _flowControl = false;
                }
                _sentWithToken = (_sentWithToken + 1) % Math.Max(1, _packetsPerToken);
            }
            else
            {
                await Task.Delay(FallbackDelay, ct).ConfigureAwait(false);
            }
            var result = await write.WriteValueWithResultAsync(piece.AsBuffer(), GattWriteOption.WriteWithoutResponse)
                .AsTask(ct).ConfigureAwait(false);
            if (result.Status != GattCommunicationStatus.Success)
                throw new IOException($"Error de escritura Bluetooth: {result.Status}.");
            off += piece.Length;
        }
    }

    public async Task<int> ReadAsync(Memory<byte> buffer, TimeSpan timeout, CancellationToken ct = default)
    {
        if (_pending.Length == 0)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(timeout);
            try
            {
                _pending = await _incoming.Reader.ReadAsync(cts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                return 0;
            }
        }
        int n = Math.Min(buffer.Length, _pending.Length);
        _pending.AsSpan(0, n).CopyTo(buffer.Span);
        _pending = _pending[n..];
        return n;
    }

    private void OnNotify(GattCharacteristic sender, GattValueChangedEventArgs args)
    {
        var data = args.CharacteristicValue.ToArray();
        Trace?.Invoke("<- " + Convert.ToHexString(data));
        switch (data)
        {
            case [0x02, var size, _, ..]:
                if (size > 0) _packetSize = size;
                return;
            case [PrinterMessages.Prefix, 0x36, var perToken, ..]:
                _packetsPerToken = perToken == 1 ? 5 : perToken;
                return;
            case [0x01, var granted, ..]:
                if (granted > 0) _tokens.Release(granted);
                return;
        }
        _incoming.Writer.TryWrite(data);
    }

    public ValueTask DisposeAsync()
    {
        if (_notify is not null) _notify.ValueChanged -= OnNotify;
        _notify = null;
        _write = null;
        _service?.Dispose();
        _service = null;
        _device?.Dispose();
        _device = null;
        return ValueTask.CompletedTask;
    }

    public override string ToString() => "Bluetooth " + BleAddress.Format(Address);
}

/// <summary>Conversión entre la dirección numérica y "AA:BB:CC:DD:EE:FF".</summary>
public static class BleAddress
{
    public static string Format(ulong address) =>
        string.Join(":", Enumerable.Range(0, 6).Select(i => ((byte)(address >> (8 * (5 - i)))).ToString("X2")));

    public static ulong Parse(string text)
    {
        var hex = text.Replace(":", "").Replace("-", "");
        if (hex.Length != 12 || !ulong.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out var value))
            throw new FormatException($"Dirección Bluetooth no válida: {text}");
        return value;
    }
}
