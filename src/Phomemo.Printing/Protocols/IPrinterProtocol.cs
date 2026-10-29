namespace Phomemo.Printing.Protocols;

/// <summary>Convierte una imagen en los bytes que hay que mandar a la impresora.</summary>
public interface IPrinterProtocol
{
    ProtocolFamily Family { get; }

    /// <summary>Pausas y tamaños de envío que necesita esta familia.</summary>
    TransportHints Hints { get; }

    /// <summary>
    /// Trozos a escribir en orden. El ancho de la imagen se ajusta al del modelo sin escalar
    /// (recorte o relleno blanco a la derecha).
    /// </summary>
    IReadOnlyList<ReadOnlyMemory<byte>> BuildJob(MonoBitmap image, PrinterModel model, PrintOptions options);
}

/// <summary>Cómo enviar los datos.</summary>
/// <param name="ChunkSize">Bytes por escritura.</param>
/// <param name="ChunkDelay">Pausa entre escrituras.</param>
/// <param name="DrainIdle">Tras enviar, se leen respuestas hasta este tiempo sin recibir nada…</param>
/// <param name="DrainMin">…pero sin cerrar antes de este tiempo…</param>
/// <param name="DrainMax">…ni después de este.</param>
/// <param name="DrainPerKb">Tiempo mínimo extra por KB enviado (impresoras lentas que no responden mientras imprimen).</param>
public sealed record TransportHints(
    int ChunkSize,
    TimeSpan ChunkDelay,
    TimeSpan DrainIdle,
    TimeSpan DrainMin,
    TimeSpan DrainMax,
    TimeSpan DrainPerKb)
{
    // La impresora calla mientras imprime y al acabar manda 1A 0F 0C (fin de la espera). Ese aviso
    // a veces llega tarde o no llega (también lo notan en phomemo-tools, que espera 8 s): sin él se
    // espera al menos 4 s o ~0,5 s por KB (a la T02 el aviso le llega ~2,5 s después de enviar 2 KB).
    public static TransportHints Default { get; } = new(
        4096, TimeSpan.Zero,
        TimeSpan.FromSeconds(1.5), TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(0.5));

    /// <summary>Tiempo mínimo que hay que mantener el puerto abierto tras enviar <paramref name="bytes"/>.</summary>
    public TimeSpan MinHold(long bytes) =>
        TimeSpan.FromTicks(Math.Max(DrainMin.Ticks, DrainPerKb.Ticks * bytes / 1000));
}

public static class ProtocolFactory
{
    public static IPrinterProtocol For(ProtocolFamily family) => family switch
    {
        ProtocolFamily.M02 => new M02Protocol(),
        ProtocolFamily.M110 => new M110Protocol(),
        ProtocolFamily.D30 => new D30Protocol(),
        ProtocolFamily.M04 => new M04Protocol(),
        ProtocolFamily.M832 => new M832Protocol(),
        _ => throw new ArgumentOutOfRangeException(nameof(family)),
    };

    public static IPrinterProtocol For(PrinterModel model) => For(model.Family);
}
