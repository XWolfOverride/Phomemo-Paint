namespace Phomemo.Printing.Tests;

public class CatalogTests
{
    [Fact]
    public void T02Is384DotsAt203Dpi()
    {
        var t02 = PrinterCatalog.Get("t02");
        Assert.Equal(384, t02.HeadWidthDots);
        Assert.Equal(203, t02.Dpi);
        Assert.Equal(48, t02.HeadWidthMm, 0);
        Assert.Equal(ProtocolFamily.M02, t02.Family);
    }

    [Fact]
    public void IdsAreUnique()
    {
        Assert.Equal(PrinterCatalog.All.Count, PrinterCatalog.All.Select(m => m.Id.ToUpperInvariant()).Distinct().Count());
    }

    [Fact]
    public void UsbIdsPointToExistingModels()
    {
        foreach (var id in PrinterCatalog.UsbIds.Values)
            Assert.NotNull(PrinterCatalog.Find(id));
    }

    [Fact]
    public void DetectsT02ByUsbId()
    {
        var result = ModelDetector.Detect(new PortInfo("COM3", UsbVendorId: 0x4C4A, UsbProductId: 0x4155));
        Assert.Equal("T02", result.Model?.Id);
        Assert.Equal(DetectionSource.UsbId, result.Source);
    }

    [Theory]
    [InlineData("Mr.in_M02", "M02")]
    [InlineData("T02", "T02")]
    [InlineData("M832-1234", "M832")]
    public void DetectsByBluetoothName(string name, string expected)
    {
        var result = ModelDetector.Detect(new PortInfo("COM5", BluetoothName: name));
        Assert.Equal(expected, result.Model?.Id);
    }

    [Fact]
    public void UnknownPortIsNotDetected()
    {
        Assert.Null(ModelDetector.Detect(new PortInfo("COM1", UsbVendorId: 1, UsbProductId: 2)).Model);
    }

    [Fact]
    public void MediaWidthIsClampedToTheHead()
    {
        var m110 = PrinterCatalog.Get("M110");
        Assert.Equal(320, m110.DotsForMediaWidth(40));
        Assert.Equal(384, m110.DotsForMediaWidth(80));
    }
}
