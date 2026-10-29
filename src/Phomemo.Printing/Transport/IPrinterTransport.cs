namespace Phomemo.Printing.Transport;

/// <summary>Canal de bytes con la impresora (puerto serie, Bluetooth SPP, fichero…).</summary>
public interface IPrinterTransport : IAsyncDisposable
{
    bool IsOpen { get; }

    Task OpenAsync(CancellationToken ct = default);

    Task WriteAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default);

    /// <summary>Lee lo que haya; devuelve 0 si en <paramref name="timeout"/> no llega nada.</summary>
    Task<int> ReadAsync(Memory<byte> buffer, TimeSpan timeout, CancellationToken ct = default);
}

/// <summary>Transporte sobre un <see cref="Stream"/> cualquiera (pruebas, volcado a fichero).</summary>
public sealed class StreamTransport(Stream output, Stream? input = null, bool leaveOpen = false) : IPrinterTransport
{
    public bool IsOpen { get; private set; }

    public Task OpenAsync(CancellationToken ct = default)
    {
        IsOpen = true;
        return Task.CompletedTask;
    }

    public async Task WriteAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default) =>
        await output.WriteAsync(data, ct).ConfigureAwait(false);

    public async Task<int> ReadAsync(Memory<byte> buffer, TimeSpan timeout, CancellationToken ct = default)
    {
        if (input is not null)
        {
            int n = await input.ReadAsync(buffer, ct).ConfigureAwait(false);
            if (n > 0) return n;
        }
        // Sin datos: se comporta como un puerto que no responde en ese tiempo.
        await Task.Delay(timeout, ct).ConfigureAwait(false);
        return 0;
    }

    public async ValueTask DisposeAsync()
    {
        IsOpen = false;
        if (leaveOpen) return;
        await output.DisposeAsync().ConfigureAwait(false);
        if (input is not null && !ReferenceEquals(input, output))
            await input.DisposeAsync().ConfigureAwait(false);
    }
}
