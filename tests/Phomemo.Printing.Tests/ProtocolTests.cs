using Phomemo.Printing.Protocols;

namespace Phomemo.Printing.Tests;

public class ProtocolTests
{
    private static byte[] Concat(IReadOnlyList<ReadOnlyMemory<byte>> chunks) =>
        chunks.SelectMany(c => c.ToArray()).ToArray();

    [Fact]
    public void T02JobHasHeaderBlocksAndFooter()
    {
        var model = PrinterCatalog.Get("T02");
        var image = new MonoBitmap(384, 10);
        image[0, 0] = true;

        var bytes = Concat(ProtocolFactory.For(model).BuildJob(image, model, PrintOptions.Default));

        // Secuencia probada en una T02 por Bluetooth.
        byte[] header = [0x1B, 0x40, 0x1B, 0x61, 0x00, 0x1F, 0x11, 0x37, 0x64, 0x1F, 0x11, 0x02, 0x04];
        byte[] marker = [0x1D, 0x76, 0x30, 0x00, 48, 0, 10, 0];
        byte[] footer = [0x1B, 0x64, 4];
        Assert.Equal(header, bytes[..header.Length]);
        Assert.Equal(marker, bytes[header.Length..(header.Length + 8)]);
        Assert.Equal(0x80, bytes[header.Length + 8]);
        Assert.Equal(header.Length + marker.Length + 480 + footer.Length, bytes.Length);
        Assert.Equal(footer, bytes[^footer.Length..]);
    }

    [Fact]
    public void M02SplitsTallImagesIn255LineBlocks()
    {
        var model = PrinterCatalog.Get("T02");
        var image = new MonoBitmap(384, 600);

        var chunks = ProtocolFactory.For(model).BuildJob(image, model, PrintOptions.Default);
        var markers = chunks.Where(c => c.Length == 8 && c.Span[0] == 0x1D).Select(c => c.Span[6] | c.Span[7] << 8).ToArray();

        Assert.Equal(new[] { 255, 255, 90 }, markers);
    }

    [Fact]
    public void FixedWidthModelsPadNarrowImagesToTheHead()
    {
        var model = PrinterCatalog.Get("T02");
        var image = new MonoBitmap(100, 1);

        var chunks = ProtocolFactory.For(model).BuildJob(image, model, PrintOptions.Default);

        Assert.Equal(48, chunks[1].Span[4]);
        Assert.Equal(48, chunks[2].Length);
    }

    [Fact]
    public void M110SendsOneBlockWithTheImageWidth()
    {
        var model = PrinterCatalog.Get("M110");
        var image = new MonoBitmap(320, 600);
        var options = new PrintOptions { Speed = 3, Darkness = 8, Media = MediaType.Continuous };

        var chunks = ProtocolFactory.For(model).BuildJob(image, model, options);

        Assert.Equal(new byte[] { 0x1B, 0x4E, 0x0D, 3, 0x1B, 0x4E, 0x04, 8, 0x1F, 0x11, 0x0B }, chunks[0].ToArray());
        Assert.Equal(new byte[] { 0x1D, 0x76, 0x30, 0x00, 40, 0, 0x58, 0x02 }, chunks[1].ToArray());
        Assert.Equal(40 * 600, chunks[2].Length);
        Assert.Equal(4, chunks.Count);
    }

    [Fact]
    public void M110ClampsWidthToTheHead()
    {
        var model = PrinterCatalog.Get("M110");
        var chunks = ProtocolFactory.For(model).BuildJob(new MonoBitmap(500, 1), model, PrintOptions.Default);
        Assert.Equal(48, chunks[1].Span[4]);
    }

    [Fact]
    public void FeedLinesOptionOverridesModel()
    {
        var model = PrinterCatalog.Get("T02");
        var chunks = ProtocolFactory.For(model).BuildJob(new MonoBitmap(384, 1), model, new PrintOptions { FeedLines = 9 });
        Assert.Equal(new byte[] { 0x1B, 0x64, 9 }, chunks[^1].Span[..3].ToArray());
    }

    [Fact]
    public void RejectsModelOfAnotherFamily()
    {
        Assert.Throws<ArgumentException>(() =>
            new M02Protocol().BuildJob(new MonoBitmap(8, 1), PrinterCatalog.Get("M110"), PrintOptions.Default));
    }

    [Theory]
    [MemberData(nameof(AllModels))]
    public void EveryModelBuildsAJob(string id)
    {
        var model = PrinterCatalog.Get(id);
        var image = new MonoBitmap(model.HeadWidthDots, 300);
        image[0, 299] = true;

        var chunks = ProtocolFactory.For(model).BuildJob(image, model, PrintOptions.Default);

        Assert.True(chunks.Sum(c => c.Length) > image.Data.Length);
    }

    public static TheoryData<string> AllModels()
    {
        var data = new TheoryData<string>();
        foreach (var m in PrinterCatalog.All) data.Add(m.Id);
        return data;
    }
}
