using System.Text;

namespace Phomemo.Printing;

public enum PrinterMessageKind
{
    /// <summary><c>1A 0F 0C</c>: la impresora ha terminado el trabajo.</summary>
    Finished,
    /// <summary><c>1A 04 nn</c>: batería en % (A1/A2/A3 = baja: 10, 5 y 3 %).</summary>
    Battery,
    /// <summary><c>1A 06 88</c>.</summary>
    PaperOut,
    /// <summary><c>1A 06 89</c>.</summary>
    PaperOk,
    /// <summary><c>1A 05 99</c>.</summary>
    CoverOpen,
    /// <summary><c>1A 05 98</c>.</summary>
    CoverClosed,
    /// <summary><c>1A 03 A9</c>.</summary>
    Overheated,
    /// <summary><c>1A 03 A8</c>.</summary>
    TemperatureOk,
    /// <summary><c>1A 07 a b c</c>: versión del firmware.</summary>
    Firmware,
    /// <summary><c>1A 08 …</c>: número de serie en ASCII.</summary>
    SerialNumber,
    /// <summary><c>1A 09 n</c>: apagado automático, n × 5 minutos.</summary>
    AutoOff,
    Unknown,
}

/// <summary>Un mensaje de la impresora. <see cref="Value"/> y <see cref="Text"/> según el tipo.</summary>
public sealed record PrinterMessage(PrinterMessageKind Kind, int Value = 0, string? Text = null);

/// <summary>Datos de la impresora reunidos de sus respuestas; null = no lo ha dicho.</summary>
public sealed record PrinterInfo(
    string? SerialNumber,
    string? Firmware,
    int? BatteryPercent,
    bool? PaperOut,
    bool? CoverOpen,
    bool? Overheated,
    byte[] Response)
{
    /// <summary>Modelo según el prefijo del número de serie.</summary>
    public PrinterModel? ModelFromSerial => PrinterCatalog.FindBySerialNumber(SerialNumber);

    public static PrinterInfo From(byte[] response)
    {
        string? serial = null, firmware = null;
        int? battery = null;
        bool? paperOut = null, coverOpen = null, hot = null;
        foreach (var m in PrinterMessages.Parse(response))
        {
            switch (m.Kind)
            {
                case PrinterMessageKind.SerialNumber when !string.IsNullOrWhiteSpace(m.Text): serial = m.Text.Trim(); break;
                case PrinterMessageKind.Firmware: firmware = m.Text; break;
                case PrinterMessageKind.Battery: battery = m.Value; break;
                case PrinterMessageKind.PaperOut: paperOut = true; break;
                case PrinterMessageKind.PaperOk: paperOut = false; break;
                case PrinterMessageKind.CoverOpen: coverOpen = true; break;
                case PrinterMessageKind.CoverClosed: coverOpen = false; break;
                case PrinterMessageKind.Overheated: hot = true; break;
                case PrinterMessageKind.TemperatureOk: hot = false; break;
            }
        }
        return new PrinterInfo(serial, firmware, battery, paperOut, coverOpen, hot, response);
    }

    /// <summary>Combina con una lectura más reciente: lo nuevo manda, lo que falte se conserva.</summary>
    public PrinterInfo Merge(PrinterInfo newer) => new(
        newer.SerialNumber ?? SerialNumber, newer.Firmware ?? Firmware, newer.BatteryPercent ?? BatteryPercent,
        newer.PaperOut ?? PaperOut, newer.CoverOpen ?? CoverOpen, newer.Overheated ?? Overheated, newer.Response);
}

/// <summary>
/// Interpreta las respuestas <c>1A tt …</c> de la familia M02 (y otras del mismo fabricante).
/// Significados en DOC/protocolos.md.
/// </summary>
public static class PrinterMessages
{
    public const byte Prefix = 0x1A;

    public static IReadOnlyList<PrinterMessage> Parse(ReadOnlySpan<byte> data)
    {
        var result = new List<PrinterMessage>();
        int i = 0;
        while (i < data.Length)
        {
            if (data[i] != Prefix) { i++; continue; }
            if (i + 1 >= data.Length) break;
            byte type = data[i + 1];
            int argStart = i + 2;
            int next = data[argStart..].IndexOf(Prefix) is var n and >= 0 ? argStart + n : data.Length;
            var args = data[argStart..next];
            byte a = args.Length > 0 ? args[0] : (byte)0;
            switch (type)
            {
                case 0x03 when args.Length > 0:
                    result.Add(new(a == 0xA9 ? PrinterMessageKind.Overheated : PrinterMessageKind.TemperatureOk));
                    break;
                case 0x04 when args.Length > 0:
                    result.Add(new(PrinterMessageKind.Battery, a switch { 0xA1 => 10, 0xA2 => 5, 0xA3 => 3, _ => Math.Min((int)a, 100) }));
                    break;
                case 0x05 when args.Length > 0:
                    result.Add(new(a == 0x99 ? PrinterMessageKind.CoverOpen : PrinterMessageKind.CoverClosed));
                    break;
                case 0x06 when args.Length > 0:
                    result.Add(new(a == 0x88 ? PrinterMessageKind.PaperOut : PrinterMessageKind.PaperOk));
                    break;
                case 0x07 when args.Length > 0:
                    var version = new StringBuilder();
                    foreach (var b in args) version.Append(version.Length == 0 ? "" : ".").Append(b);
                    result.Add(new(PrinterMessageKind.Firmware, Text: version.ToString()));
                    break;
                case 0x08:
                    result.Add(new(PrinterMessageKind.SerialNumber, Text: Encoding.ASCII.GetString(args)));
                    break;
                case 0x09 when args.Length > 0:
                    result.Add(new(PrinterMessageKind.AutoOff, a * 5));
                    break;
                case 0x0F when a == 0x0C:
                    result.Add(new(PrinterMessageKind.Finished));
                    break;
                default:
                    result.Add(new(PrinterMessageKind.Unknown, type));
                    break;
            }
            i = next;
        }
        return result;
    }

    /// <summary>¿Contiene <c>1A 0F 0C</c> (trabajo terminado)?</summary>
    public static bool HasFinished(ReadOnlySpan<byte> data) => data.IndexOf((ReadOnlySpan<byte>)[Prefix, 0x0F, 0x0C]) >= 0;
}
