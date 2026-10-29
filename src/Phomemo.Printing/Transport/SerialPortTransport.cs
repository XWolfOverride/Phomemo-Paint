using System.IO.Ports;

namespace Phomemo.Printing.Transport;

/// <summary>
/// Puerto serie: COMx en Windows, /dev/ttyACM* o /dev/rfcomm* en Linux, /dev/cu.* en macOS.
/// USB CDC-ACM y Bluetooth SPP se ven igual.
/// </summary>
public sealed class SerialPortTransport(string portName, int baudRate = SerialPortTransport.DefaultBaudRate) : IPrinterTransport
{
    /// <summary>Velocidad habitual en estas impresoras (phomemo-protocol). En CDC-ACM no tiene efecto real.</summary>
    public const int DefaultBaudRate = 128000;

    private SerialPort? _port;

    public string PortName { get; } = portName;

    public bool IsOpen => _port?.IsOpen == true;

    public static IReadOnlyList<string> ListPorts() =>
        SerialPort.GetPortNames().Distinct().Order(StringComparer.OrdinalIgnoreCase).ToArray();

    public Task OpenAsync(CancellationToken ct = default)
    {
        if (IsOpen) return Task.CompletedTask;
        var port = new SerialPort(PortName, baudRate, Parity.None, 8, StopBits.One)
        {
            Handshake = Handshake.None,
            DtrEnable = true,
            RtsEnable = true,
            WriteTimeout = 10_000,
        };
        try
        {
            port.Open();
        }
        catch
        {
            port.Dispose();
            throw;
        }
        _port = port;
        return Task.CompletedTask;
    }

    public async Task WriteAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default)
    {
        var port = _port ?? throw new InvalidOperationException("El puerto no está abierto.");
        await port.BaseStream.WriteAsync(data, ct).ConfigureAwait(false);
        await port.BaseStream.FlushAsync(ct).ConfigureAwait(false);
    }

    // SerialStream no respeta bien la cancelación de ReadAsync en todas las plataformas: se usa la
    // lectura síncrona con ReadTimeout en un hilo del pool.
    public Task<int> ReadAsync(Memory<byte> buffer, TimeSpan timeout, CancellationToken ct = default)
    {
        var port = _port ?? throw new InvalidOperationException("El puerto no está abierto.");
        return Task.Run(() =>
        {
            ct.ThrowIfCancellationRequested();
            port.ReadTimeout = Math.Max(1, (int)timeout.TotalMilliseconds);
            var tmp = new byte[buffer.Length];
            try
            {
                int n = port.Read(tmp, 0, tmp.Length);
                tmp.AsSpan(0, n).CopyTo(buffer.Span);
                return n;
            }
            catch (TimeoutException)
            {
                return 0;
            }
        }, ct);
    }

    public ValueTask DisposeAsync()
    {
        var port = _port;
        _port = null;
        if (port is not null)
        {
            try { port.Close(); } catch (IOException) { }
            port.Dispose();
        }
        return ValueTask.CompletedTask;
    }

    public override string ToString() => PortName;
}
