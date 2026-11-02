using System.ComponentModel;
using Phomemo.Printing;

namespace PhomemoPaint;

/// <summary>
/// Datos de la impresora seleccionada (conexión, n.º de serie, batería…) y el botón Consultar. El
/// título y los selectores de puerto y modelo están en la ventana principal. Editable con el
/// diseñador (PrinterInfoPanel.Designer.cs).
/// </summary>
internal sealed partial class PrinterInfoPanel : UserControl
{
    public PrinterInfoPanel()
    {
        InitializeComponent();
        ApplyTexts();
    }

    /// <summary>El usuario pide volver a consultar a la impresora.</summary>
    public event EventHandler? RefreshRequested;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool Busy
    {
        set
        {
            _refresh.Enabled = !value;
            if (value) _updated.Text = S.InfoQuerying;
        }
    }

    /// <summary>Textos en el idioma de la aplicación (el diseñador guarda los españoles).</summary>
    private void ApplyTexts()
    {
        _connectionCaption.Text = S.InfoConnection;
        _addressCaption.Text = S.InfoAddress;
        _serialCaption.Text = S.InfoSerial;
        _firmwareCaption.Text = S.InfoFirmware;
        _batteryCaption.Text = S.InfoBattery;
        _paperCaption.Text = S.InfoPaper;
        _coverCaption.Text = S.InfoCover;
        _updatedCaption.Text = S.InfoUpdated;
        _tips.SetToolTip(_refresh, S.InfoRefresh);
    }

    public void ShowInfo(PortInfo? port, PrinterInfo? info, DateTime? updated, string? error)
    {
        _connection.Text = port is null ? "—" : port.Kind == PortKind.BluetoothLE ? "Bluetooth LE" : S.InfoSerialPort;
        _address.Text = port switch
        {
            null => "—",
            { Kind: PortKind.BluetoothLE } => port.Address ?? "—",
            { UsbVendorId: { } vid, UsbProductId: { } pid } => $"{port.Name} (USB {vid:X4}:{pid:X4})",
            _ => port.Name,
        };
        _serial.Text = info?.SerialNumber ?? "—";
        _firmware.Text = info?.Firmware ?? "—";
        _battery.Text = info?.BatteryPercent is { } b ? $"{b} %" : "—";
        _battery.ForeColor = info?.BatteryPercent is <= 15 ? Color.Firebrick : ForeColor;
        _paper.Text = info?.PaperOut switch { true => S.MsgPaperOut, false => S.InfoPaperOk, null => "—" };
        _paper.ForeColor = info?.PaperOut == true ? Color.Firebrick : ForeColor;
        _cover.Text = info?.CoverOpen switch { true => S.MsgCoverOpen, false => S.InfoCoverClosed, null => "—" };
        _cover.ForeColor = info?.CoverOpen == true ? Color.Firebrick : ForeColor;
        _updated.Text = error ?? (updated is { } t ? t.ToString("HH:mm") : "—");
    }

    private void Refresh_Click(object? sender, EventArgs e) => RefreshRequested?.Invoke(this, EventArgs.Empty);
}
