using Phomemo.Printing.Protocols;
using Phomemo.Printing.Transport;

namespace Phomemo.Printing.Tests;

public class PrinterTests
{
    private static readonly TransportHints NoWait =
        new(4096, TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero);

    [Fact]
    public async Task PrintWritesTheWholeJobAndReportsProgress()
    {
        var model = PrinterCatalog.Get("T02");
        var output = new MemoryStream();
        await using var transport = new StreamTransport(output, leaveOpen: true);
        var printer = new PhomemoPrinter(transport, model, NoWait);
        var image = new MonoBitmap(384, 300);
        var reports = new List<double>();

        var result = await printer.PrintAsync(image, progress: new SyncProgress(reports));

        var expected = ProtocolFactory.For(model).BuildJob(image, model, PrintOptions.Default).Sum(c => c.Length);
        Assert.Equal(expected, output.Length);
        Assert.Equal(expected, result.BytesSent);
        Assert.Equal(1.0, reports[^1]);
    }

    [Fact]
    public async Task SmallChunkHintsSplitWrites()
    {
        var output = new CountingStream();
        await using var transport = new StreamTransport(output);
        var hints = NoWait with { ChunkSize = 16 };
        var printer = new PhomemoPrinter(transport, PrinterCatalog.Get("T02"), hints);

        await printer.PrintAsync(new MonoBitmap(384, 1));

        Assert.All(output.Writes, w => Assert.True(w <= 16));
    }

    [Fact]
    public async Task PrintCollectsPrinterResponse()
    {
        var input = new MemoryStream([0x1A, 0x04, 0x5A]);
        await using var transport = new StreamTransport(new MemoryStream(), input);
        var hints = NoWait with { DrainIdle = TimeSpan.FromMilliseconds(50), DrainMax = TimeSpan.FromSeconds(2) };
        var printer = new PhomemoPrinter(transport, PrinterCatalog.Get("T02"), hints);

        var result = await printer.PrintAsync(new MonoBitmap(384, 1));

        Assert.Equal(new byte[] { 0x1A, 0x04, 0x5A }, result.Response);
    }

    [Fact]
    public async Task PrintStopsWaitingWhenThePrinterReportsFinished()
    {
        var input = new LateStream([0x1A, 0x0F, 0x0C]);
        await using var transport = new StreamTransport(new MemoryStream(), input);
        // Sin el aviso de fin esperaría al menos 30 s.
        var hints = NoWait with { DrainIdle = TimeSpan.FromSeconds(30), DrainMin = TimeSpan.FromSeconds(30), DrainMax = TimeSpan.FromSeconds(60) };
        var printer = new PhomemoPrinter(transport, PrinterCatalog.Get("T02"), hints);
        var sw = System.Diagnostics.Stopwatch.StartNew();

        var result = await printer.PrintAsync(new MonoBitmap(384, 1));

        Assert.True(result.Finished);
        Assert.True(sw.Elapsed < TimeSpan.FromSeconds(10));
    }

    [Fact]
    public async Task FinishedReceivedBeforeTheJobEndsBelongsToAnotherJob()
    {
        // 1A 0F 0C ya recibido al acabar de enviar: es el aviso retrasado del trabajo anterior.
        var input = new MemoryStream([0x1A, 0x0F, 0x0C]);
        await using var transport = new StreamTransport(new MemoryStream(), input);
        var hints = NoWait with { DrainIdle = TimeSpan.FromMilliseconds(50), DrainMax = TimeSpan.FromSeconds(2) };
        var printer = new PhomemoPrinter(transport, PrinterCatalog.Get("T02"), hints);

        var result = await printer.PrintAsync(new MonoBitmap(384, 1));

        Assert.False(result.Finished);
        Assert.Equal(new byte[] { 0x1A, 0x0F, 0x0C }, result.Response);
    }

    [Fact]
    public async Task ReadyQueryAsksForPaperAndCover()
    {
        // Respuesta observada con la tapa abierta: «sin papel» y «tapa abierta».
        var output = new MemoryStream();
        await using var transport = new StreamTransport(output, new LateStream([0x1A, 0x06, 0x88, 0x1A, 0x05, 0x99]));
        var printer = new PhomemoPrinter(transport, PrinterCatalog.Get("T02"));

        var info = await printer.QueryReadyAsync();

        Assert.Equal(new byte[] { 0x1F, 0x11, 0x11, 0x1F, 0x11, 0x12 }, output.ToArray());
        Assert.True(info.PaperOut);
        Assert.True(info.CoverOpen);
    }

    [Fact]
    public async Task QueryReturnsResponse()
    {
        var output = new MemoryStream();
        var input = new MemoryStream([0x1A, 0x07, 0x01, 0x00, 0x00]);
        await using var transport = new StreamTransport(output, input);
        var printer = new PhomemoPrinter(transport, PrinterCatalog.Get("T02"));

        var response = await printer.QueryAsync(new byte[] { 0x1F, 0x11, 0x07 }, TimeSpan.FromMilliseconds(50));

        Assert.Equal(new byte[] { 0x1F, 0x11, 0x07 }, output.ToArray());
        Assert.Equal(5, response.Length);
    }

    [Fact]
    public void HintsKeepPortOpenProportionalToData()
    {
        var hints = new M04Protocol().Hints;
        Assert.Equal(TimeSpan.FromSeconds(8), hints.MinHold(100));
        Assert.Equal(TimeSpan.FromSeconds(20), hints.MinHold(20_000));
    }

    private sealed class SyncProgress(List<double> values) : IProgress<double>
    {
        public void Report(double value) => values.Add(value);
    }

    /// <summary>Devuelve los datos a partir de la segunda lectura (llegan después de enviar).</summary>
    private sealed class LateStream(byte[] data) : MemoryStream(data)
    {
        private bool _first = true;

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (!_first) return base.ReadAsync(buffer, cancellationToken);
            _first = false;
            return ValueTask.FromResult(0);
        }
    }

    private sealed class CountingStream : MemoryStream
    {
        public List<int> Writes { get; } = [];

        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            Writes.Add(buffer.Length);
            return base.WriteAsync(buffer, cancellationToken);
        }
    }
}
