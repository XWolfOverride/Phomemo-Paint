using System.Globalization;
using System.Text;
using Phomemo.Printing;
using Phomemo.Printing.Windows;

namespace PhomemoPaint;

/// <summary>
/// Manda comandos sueltos al puerto y muestra las respuestas, para averiguar qué contesta cada
/// modelo (p. ej. si alguna consulta identifica el modelo).
/// </summary>
internal sealed class DiagnosticsForm : Form
{
    // Consultas conocidas. Lo que va tras '#' es comentario.
    private const string DefaultCommands =
        "1F 11 08   # bateria\r\n1F 11 07   # firmware\r\n1F 11 09   # numero de serie\r\n" +
        "1F 11 0E   # apagado automatico\r\n1F 11 11   # papel\r\n" +
        "1F 11 38   # chip (1A 16/17: 3 o 7 = JieLi)\r\n1F 11 63   # capacidades (1A 3B: grises, doble dpi)";

    // 1F 11 14 + tamaño inicia la actualización de firmware: nunca desde aquí.
    private static readonly byte[] FirmwareUpdate = [0x1F, 0x11, 0x14];

    private readonly PortInfo _port;
    private readonly PrinterModel _model;
    private readonly TextBox _commands = new() { Multiline = true, Dock = DockStyle.Fill, Font = new Font(FontFamily.GenericMonospace, 10), Text = DefaultCommands, ScrollBars = ScrollBars.Vertical };
    private readonly TextBox _output = new() { Multiline = true, Dock = DockStyle.Fill, ReadOnly = true, Font = new Font(FontFamily.GenericMonospace, 10), ScrollBars = ScrollBars.Both, WordWrap = false };
    private readonly Button _send = new() { Text = S.DiagSend, AutoSize = true };

    public DiagnosticsForm(PortInfo port, PrinterModel model)
    {
        _port = port;
        _model = model;
        Text = $"{S.DiagTitle} — {port}";
        Size = new Size(640, 520);
        StartPosition = FormStartPosition.CenterParent;
        MinimizeBox = false;

        var help = new Label { Text = S.DiagHelp, Dock = DockStyle.Top, AutoSize = false, Height = 48, Padding = new Padding(6) };
        var copy = new Button { Text = S.DiagCopy, AutoSize = true };
        copy.Click += (_, _) => { if (_output.TextLength > 0) Clipboard.SetText(_output.Text); };
        _send.Click += async (_, _) => await SendAsync();

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(4) };
        buttons.Controls.AddRange([copy, _send]);

        var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 120 };
        split.Panel1.Controls.Add(_commands);
        split.Panel2.Controls.Add(_output);

        Controls.Add(split);
        Controls.Add(buttons);
        Controls.Add(help);
    }

    private async Task SendAsync()
    {
        var commands = new List<(string Line, byte[] Bytes)>();
        foreach (var raw in _commands.Lines)
        {
            int hash = raw.IndexOf('#');
            var line = (hash >= 0 ? raw[..hash] : raw).Trim();
            if (line.Length == 0) continue;
            if (!TryParseHex(line, out var bytes))
            {
                MessageBox.Show(this, S.DiagBadHex(line), S.DiagTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (bytes.AsSpan().IndexOf(FirmwareUpdate) >= 0)
            {
                MessageBox.Show(this, S.DiagBlocked(line), S.DiagTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            commands.Add((line, bytes));
        }

        _send.Enabled = false;
        var sb = new StringBuilder(_output.Text);
        try
        {
            await using var transport = Transports.Create(_port);
            var printer = new PhomemoPrinter(transport, _model);
            foreach (var (line, bytes) in commands)
            {
                var answer = await printer.QueryAsync(bytes);
                sb.AppendLine($"> {Hex(bytes)}");
                sb.AppendLine(answer.Length == 0 ? $"< {S.DiagNoAnswer}" : $"< {Hex(answer)}   {Ascii(answer)}   {Describe(answer)}");
                _output.Text = sb.ToString();
                _output.SelectionStart = _output.TextLength;
                _output.ScrollToCaret();
            }
            sb.AppendLine();
            _output.Text = sb.ToString();
        }
        catch (Exception e) when (e is not OutOfMemoryException)
        {
            MessageBox.Show(this, e.Message, S.DiagTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _send.Enabled = true;
        }
    }

    internal static bool TryParseHex(string line, out byte[] bytes)
    {
        var parts = line.Replace(",", " ").Split(' ', StringSplitOptions.RemoveEmptyEntries);
        bytes = new byte[parts.Length];
        for (int i = 0; i < parts.Length; i++)
        {
            var p = parts[i].StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? parts[i][2..] : parts[i];
            if (!byte.TryParse(p, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out bytes[i]))
                return false;
        }
        return bytes.Length > 0;
    }

    internal static string Hex(byte[] bytes) => string.Join(' ', bytes.Select(b => b.ToString("X2")));

    /// <summary>Respuestas <c>1A …</c> en texto ("terminado, batería 80 %."); hex si no se reconocen.</summary>
    internal static string Describe(byte[] response)
    {
        if (response.Length == 0) return S.NoResponse;
        var parts = PrinterMessages.Parse(response).Select(m => m.Kind switch
        {
            PrinterMessageKind.Finished => S.MsgFinished,
            PrinterMessageKind.Battery => S.MsgBattery(m.Value),
            PrinterMessageKind.PaperOut => S.MsgPaperOut,
            PrinterMessageKind.CoverOpen => S.MsgCoverOpen,
            PrinterMessageKind.Overheated => S.MsgOverheated,
            PrinterMessageKind.Firmware => S.MsgFirmware(m.Text!),
            PrinterMessageKind.SerialNumber => S.MsgSerial(m.Text!),
            PrinterMessageKind.AutoOff => S.MsgAutoOff(m.Value),
            _ => null,
        }).OfType<string>().Distinct().ToList();
        return parts.Count == 0 ? Hex(response) : string.Join(", ", parts) + ".";
    }

    private static string Ascii(byte[] bytes) =>
        new(bytes.Select(b => b is >= 0x20 and < 0x7F ? (char)b : '.').ToArray());
}
