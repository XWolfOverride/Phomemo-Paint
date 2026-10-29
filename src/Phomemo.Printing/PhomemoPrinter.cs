using System.Diagnostics;
using Phomemo.Printing.Protocols;
using Phomemo.Printing.Transport;

namespace Phomemo.Printing;

/// <summary>Resultado de un envío.</summary>
/// <param name="BytesSent">Bytes escritos.</param>
/// <param name="Response">Lo que respondió la impresora mientras se esperaba.</param>
public sealed record PrintResult(long BytesSent, byte[] Response)
{
    public IReadOnlyList<PrinterMessage> Messages => PrinterMessages.Parse(Response);

    /// <summary>
    /// La impresora confirmó el final del trabajo (<c>1A 0F 0C</c> recibido después de enviarlo
    /// entero; uno anterior puede ser de otro trabajo).
    /// </summary>
    public bool Finished { get; init; }
}

/// <summary>
/// Envía trabajos a una impresora: codifica con el protocolo del modelo, escribe respetando
/// las pausas de la familia y espera la respuesta antes de dejar cerrar el puerto
/// (cerrar en cuanto se termina de escribir puede cortar la impresión).
/// </summary>
public sealed class PhomemoPrinter
{
    private readonly IPrinterTransport _transport;

    public PhomemoPrinter(IPrinterTransport transport, PrinterModel model, TransportHints? hints = null)
    {
        _transport = transport ?? throw new ArgumentNullException(nameof(transport));
        Model = model ?? throw new ArgumentNullException(nameof(model));
        Protocol = ProtocolFactory.For(model);
        Hints = hints ?? Protocol.Hints;
    }

    public PrinterModel Model { get; }

    public IPrinterProtocol Protocol { get; }

    public TransportHints Hints { get; }

    /// <summary>Imprime la imagen tal cual (el recorte de filas en blanco es cosa de quien llama).</summary>
    /// <param name="progress">De 0 a 1.</param>
    public async Task<PrintResult> PrintAsync(MonoBitmap image, PrintOptions? options = null,
        IProgress<double>? progress = null, CancellationToken ct = default)
    {
        var chunks = Protocol.BuildJob(image, Model, options ?? PrintOptions.Default);
        long total = chunks.Sum(c => (long)c.Length);

        await EnsureOpenAsync(ct).ConfigureAwait(false);
        long sent = 0;
        progress?.Report(0);
        foreach (var chunk in chunks)
        {
            for (int off = 0; off < chunk.Length; off += Hints.ChunkSize)
            {
                var piece = chunk.Slice(off, Math.Min(Hints.ChunkSize, chunk.Length - off));
                await _transport.WriteAsync(piece, ct).ConfigureAwait(false);
                sent += piece.Length;
                progress?.Report(0.9 * sent / total);
                if (Hints.ChunkDelay > TimeSpan.Zero)
                    await Task.Delay(Hints.ChunkDelay, ct).ConfigureAwait(false);
            }
        }

        // Lo que ya esté recibido al acabar de enviar no es de este trabajo: la impresora no puede
        // haberlo terminado antes de recibirlo entero, y la T02 manda a veces el 1A 0F 0C del
        // trabajo anterior con retraso. Se guarda en la respuesta pero no cuenta como final.
        var early = await ReadPendingAsync(ct).ConfigureAwait(false);

        // Se espera a "trabajo terminado"; las impresoras que no lo mandan, hasta el silencio.
        var response = await DrainAsync(Hints.DrainIdle, Hints.MinHold(total), Hints.DrainMax, ct,
            stopWhenFinished: true).ConfigureAwait(false);
        progress?.Report(1);
        return new PrintResult(total, [.. early, .. response]) { Finished = PrinterMessages.HasFinished(response) };
    }

    /// <summary>
    /// Manda un comando suelto (p. ej. una consulta de estado <c>1F 11 xx</c>) y devuelve lo que
    /// responda la impresora. Para experimentar: no hay consultas documentadas.
    /// </summary>
    public async Task<byte[]> QueryAsync(ReadOnlyMemory<byte> command, TimeSpan? wait = null, CancellationToken ct = default)
    {
        var idle = wait ?? TimeSpan.FromMilliseconds(600);
        await EnsureOpenAsync(ct).ConfigureAwait(false);
        await _transport.WriteAsync(command, ct).ConfigureAwait(false);
        return await DrainAsync(idle, TimeSpan.Zero, idle * 4, ct).ConfigureAwait(false);
    }

    /// <summary>Lo que ya esté recibido, sin esperar.</summary>
    private async Task<byte[]> ReadPendingAsync(CancellationToken ct)
    {
        var received = new MemoryStream();
        var buffer = new byte[256];
        int n;
        while ((n = await _transport.ReadAsync(buffer, TimeSpan.FromMilliseconds(1), ct).ConfigureAwait(false)) > 0)
            received.Write(buffer, 0, n);
        return received.ToArray();
    }

    private async Task EnsureOpenAsync(CancellationToken ct)
    {
        if (!_transport.IsOpen)
            await _transport.OpenAsync(ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Lee respuestas hasta que pasan <paramref name="idle"/> sin datos, sin terminar antes de
    /// <paramref name="min"/> ni después de <paramref name="max"/>. Con
    /// <paramref name="stopWhenFinished"/> termina en cuanto llega <c>1A 0F 0C</c>.
    /// </summary>
    private async Task<byte[]> DrainAsync(TimeSpan idle, TimeSpan min, TimeSpan max, CancellationToken ct,
        bool stopWhenFinished = false)
    {
        var received = new MemoryStream();
        var buffer = new byte[256];
        var sw = Stopwatch.StartNew();
        var lastData = TimeSpan.Zero;
        var slice = TimeSpan.FromMilliseconds(200);
        while (sw.Elapsed < max)
        {
            var left = max - sw.Elapsed;
            int n = await _transport.ReadAsync(buffer, left < slice ? left : slice, ct).ConfigureAwait(false);
            if (n > 0)
            {
                received.Write(buffer, 0, n);
                lastData = sw.Elapsed;
                if (stopWhenFinished && PrinterMessages.HasFinished(received.GetBuffer().AsSpan(0, (int)received.Length)))
                    break;
            }
            else if (sw.Elapsed >= min && sw.Elapsed - lastData >= idle)
            {
                break;
            }
        }
        return received.ToArray();
    }
}
