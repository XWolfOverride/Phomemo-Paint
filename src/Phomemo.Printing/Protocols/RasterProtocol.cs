namespace Phomemo.Printing.Protocols;

/// <summary>Base común: todas las familias mandan la imagen con ESC/POS <c>GS v 0</c>.</summary>
public abstract class RasterProtocol : IPrinterProtocol
{
    protected const byte ESC = 0x1B;
    protected const byte GS = 0x1D;
    protected const byte US = 0x1F;

    public abstract ProtocolFamily Family { get; }

    public virtual TransportHints Hints => TransportHints.Default;

    /// <summary>Máximo de líneas por bloque <c>GS v 0</c>.</summary>
    protected virtual int MaxLinesPerBlock => ushort.MaxValue;

    public IReadOnlyList<ReadOnlyMemory<byte>> BuildJob(MonoBitmap image, PrinterModel model, PrintOptions options)
    {
        ArgumentNullException.ThrowIfNull(image);
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(options);
        if (model.Family != Family)
            throw new ArgumentException($"{model.Id} usa el protocolo {model.Family}, no {Family}.", nameof(model));
        if (model.RequiresCompression)
            throw new NotSupportedException($"{model.Id} necesita datos comprimidos (LZO), que no están implementados.");

        image = image.WithWidth(TargetWidth(image, model));
        var chunks = new List<ReadOnlyMemory<byte>> { Header(model, options) };
        for (int line = 0; line < image.Height; line += MaxLinesPerBlock)
        {
            int lines = Math.Min(MaxLinesPerBlock, image.Height - line);
            chunks.Add(BlockMarker(image.BytesPerRow, lines));
            chunks.Add(image.Data.AsMemory(line * image.BytesPerRow, lines * image.BytesPerRow));
        }
        chunks.Add(Footer(image, model, options));
        return chunks;
    }

    /// <summary>Ancho en puntos que se envía.</summary>
    protected virtual int TargetWidth(MonoBitmap image, PrinterModel model) =>
        model.WidthDependsOnMedia ? Math.Min(image.Width, model.HeadWidthDots) : model.HeadWidthDots;

    protected abstract byte[] Header(PrinterModel model, PrintOptions options);

    protected abstract byte[] Footer(MonoBitmap image, PrinterModel model, PrintOptions options);

    /// <summary><c>GS v 0 m xL xH yL yH</c>, modo normal.</summary>
    public static byte[] BlockMarker(int bytesPerRow, int lines) =>
    [
        GS, 0x76, 0x30, 0x00,
        (byte)bytesPerRow, (byte)(bytesPerRow >> 8),
        (byte)lines, (byte)(lines >> 8),
    ];

    /// <summary><c>ESC d n</c>: imprimir y avanzar n líneas.</summary>
    protected static byte[] Feed(int lines) => [ESC, 0x64, (byte)Math.Clamp(lines, 0, 255)];

    protected static int FeedLines(PrinterModel model, PrintOptions options) => options.FeedLines ?? model.FeedLines;
}

/// <summary>M02, M02 Pro, M02S y T02: ESC/POS plano, bloques de 255 líneas como máximo.</summary>
public sealed class M02Protocol : RasterProtocol
{
    public override ProtocolFamily Family => ProtocolFamily.M02;

    protected override int MaxLinesPerBlock => 255;

    // Cabecera ampliada: densidad, coeficiente de calor, tipo de papel y sin compresión.
    protected override byte[] Header(PrinterModel model, PrintOptions options) =>
    [
        ESC, 0x40,                                                          // ESC @: inicializar
        US, 0x11, 0x02, (byte)Math.Clamp(options.Darkness ?? model.DefaultDarkness ?? 4, 1, 15), // densidad (4 salvo modelo)
        US, 0x11, 0x37, (byte)Math.Clamp(options.HeatCoefficient ?? 100, 1, 255), // coeficiente de calor
        US, 0x11, options.Media == MediaType.LabelWithMarks ? (byte)0x26 : (byte)0x0B, // papel: rollo o marcas
        US, 0x11, 0x35, 0x00,                                               // datos sin comprimir
    ];

    // Solo el avance. La impresora responde 1A 0F 0C al terminar.
    protected override byte[] Footer(MonoBitmap image, PrinterModel model, PrintOptions options) =>
        Feed(FeedLines(model, options));
}

/// <summary>M110, M120, M220, M421: velocidad, densidad y tipo de papel; un solo bloque.</summary>
public sealed class M110Protocol : RasterProtocol
{
    public override ProtocolFamily Family => ProtocolFamily.M110;

    protected override byte[] Header(PrinterModel model, PrintOptions options) =>
    [
        ESC, 0x4E, 0x0D, (byte)Math.Clamp(options.Speed ?? 5, 1, 5),
        ESC, 0x4E, 0x04, (byte)Math.Clamp(options.Darkness ?? 10, 1, 15),
        US, 0x11, (byte)options.Media,
    ];

    protected override byte[] Footer(MonoBitmap image, PrinterModel model, PrintOptions options) =>
    [
        US, 0xF0, 0x05, 0x00,
        US, 0xF0, 0x03, 0x00,
    ];
}

/// <summary>
/// D30 (experimental). El filtro CUPS gira la página 90° porque la etiqueta se diseña apaisada;
/// aquí el lienzo ya tiene el ancho del cabezal, así que no se gira.
/// </summary>
public sealed class D30Protocol : RasterProtocol
{
    public override ProtocolFamily Family => ProtocolFamily.D30;

    protected override byte[] Header(PrinterModel model, PrintOptions options) =>
    [
        US, 0x11, 0x24, 0x00,   // sin documentar, necesario según phomemo-tools
        ESC, 0x40,
    ];

    // El avance se hace mandando líneas en blanco, sin cabecera de bloque (como el filtro CUPS).
    protected override byte[] Footer(MonoBitmap image, PrinterModel model, PrintOptions options) =>
        new byte[image.BytesPerRow * FeedLines(model, options)];
}

/// <summary>M04S/M04AS (experimental): un único bloque y envío lento.</summary>
public sealed class M04Protocol : RasterProtocol
{
    public override ProtocolFamily Family => ProtocolFamily.M04;

    // Buffer pequeño y sin control de flujo: trozos de 256 bytes cada 20 ms, y mantener la
    // conexión abierta mientras imprime (~1 KB/s) sin responder.
    public override TransportHints Hints { get; } = new(
        256, TimeSpan.FromMilliseconds(20),
        TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(8), TimeSpan.FromMinutes(10), TimeSpan.FromSeconds(1));

    protected override byte[] Header(PrinterModel model, PrintOptions options) =>
    [
        US, 0x11, 0x02, (byte)Math.Clamp(options.Darkness ?? 0x0B, 1, 15),
        US, 0x11, 0x37, 0xB7,   // calor/velocidad
        US, 0x11, 0x0B,         // papel continuo
        US, 0x11, 0x35, 0x00,   // sin compresión
    ];

    protected override byte[] Footer(MonoBitmap image, PrinterModel model, PrintOptions options) =>
        Feed(FeedLines(model, options));
}

/// <summary>M832 (experimental): papel continuo, un único bloque.</summary>
public sealed class M832Protocol : RasterProtocol
{
    public override ProtocolFamily Family => ProtocolFamily.M832;

    protected override byte[] Header(PrinterModel model, PrintOptions options) =>
    [
        US, 0x11, (byte)MediaType.Continuous,
        US, 0x11, 0x35, 0x00,
    ];

    protected override byte[] Footer(MonoBitmap image, PrinterModel model, PrintOptions options) =>
    [
        .. Feed(FeedLines(model, options)),
        .. Feed(2),
        US, 0x11, 0x11,         // consulta de estado
    ];
}
