namespace Phomemo.Printing.Tests;

public class PrinterMessagesTests
{
    [Fact]
    public void ParsesResponsesSeenOnTheT02()
    {
        // Respuestas observadas en una T02, concatenadas.
        byte[] data =
        [
            0x1A, 0x04, 0x64,
            0x1A, 0x07, 0x01, 0x00, 0x00,
            0x1A, 0x08, (byte)'Q', (byte)'0', (byte)'0', (byte)'1',
            0x1A, 0x06, 0x89,
            0x1A, 0x09, 0x06,
            0x1A, 0x03, 0xA8,
            0x1A, 0x0F, 0x0C,
        ];

        var messages = PrinterMessages.Parse(data);

        Assert.Equal(
        [
            new PrinterMessage(PrinterMessageKind.Battery, 100),
            new PrinterMessage(PrinterMessageKind.Firmware, Text: "1.0.0"),
            new PrinterMessage(PrinterMessageKind.SerialNumber, Text: "Q001"),
            new PrinterMessage(PrinterMessageKind.PaperOk),
            new PrinterMessage(PrinterMessageKind.AutoOff, 30),
            new PrinterMessage(PrinterMessageKind.TemperatureOk),
            new PrinterMessage(PrinterMessageKind.Finished),
        ], messages);
        Assert.True(PrinterMessages.HasFinished(data));
    }

    [Theory]
    [InlineData(new byte[] { 0x1A, 0x06, 0x88 }, PrinterMessageKind.PaperOut)]
    [InlineData(new byte[] { 0x1A, 0x05, 0x99 }, PrinterMessageKind.CoverOpen)]
    [InlineData(new byte[] { 0x1A, 0x03, 0xA9 }, PrinterMessageKind.Overheated)]
    [InlineData(new byte[] { 0x1A, 0x22, 0x01 }, PrinterMessageKind.Unknown)]
    public void ParsesWarnings(byte[] data, PrinterMessageKind kind) =>
        Assert.Equal(kind, Assert.Single(PrinterMessages.Parse(data)).Kind);

    [Fact]
    public void LowBatteryCodesBecomePercentages() =>
        Assert.Equal(5, Assert.Single(PrinterMessages.Parse([0x1A, 0x04, 0xA2])).Value);

    [Fact]
    public void IgnoresGarbageAndTruncatedMessages()
    {
        Assert.Empty(PrinterMessages.Parse([0x00, 0x51, 0x1A]));
        Assert.False(PrinterMessages.HasFinished([0x1A, 0x0F]));
    }
}
