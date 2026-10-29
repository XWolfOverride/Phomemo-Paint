using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using Phomemo.Printing;
using Phomemo.Printing.Transport;

namespace PhomemoPaint;

/// <summary>
/// Puertos COM con su VID/PID USB, leídos del registro
/// (HKLM\SYSTEM\CurrentControlSet\Enum\USB\VID_xxxx&amp;PID_xxxx\…\Device Parameters\PortName).
/// Es específico de Windows, por eso vive en la aplicación y no en Phomemo.Printing.
/// </summary>
internal sealed partial class WindowsPortInfoProvider : IPortInfoProvider
{
    public IReadOnlyList<PortInfo> GetPorts()
    {
        var present = SerialPortTransport.ListPorts();
        var known = new Dictionary<string, PortInfo>(StringComparer.OrdinalIgnoreCase);
        try
        {
            ScanUsb(known);
        }
        catch (Exception e) when (e is System.Security.SecurityException or UnauthorizedAccessException or IOException)
        {
        }
        return present.Select(p => known.TryGetValue(p, out var info) ? info : new PortInfo(p)).ToArray();
    }

    private static void ScanUsb(Dictionary<string, PortInfo> result)
    {
        using var usb = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Enum\USB");
        if (usb is null) return;
        foreach (var deviceName in usb.GetSubKeyNames())
        {
            var m = VidPid().Match(deviceName);
            if (!m.Success) continue;
            ushort vid = ushort.Parse(m.Groups[1].Value, NumberStyles.HexNumber);
            ushort pid = ushort.Parse(m.Groups[2].Value, NumberStyles.HexNumber);
            using var device = usb.OpenSubKey(deviceName);
            if (device is null) continue;
            foreach (var instanceName in device.GetSubKeyNames())
            {
                using var instance = device.OpenSubKey(instanceName);
                using var parameters = instance?.OpenSubKey("Device Parameters");
                if (parameters?.GetValue("PortName") is not string port) continue;
                var friendly = instance!.GetValue("FriendlyName") as string;
                // "Dispositivo serie USB (COM3)" -> "Dispositivo serie USB"
                var description = friendly is null ? null : PortSuffix().Replace(friendly, "").Trim();
                result[port] = new PortInfo(port, description, vid, pid);
            }
        }
    }

    [GeneratedRegex(@"VID_([0-9A-Fa-f]{4})&PID_([0-9A-Fa-f]{4})")]
    private static partial Regex VidPid();

    [GeneratedRegex(@"\s*\(COM\d+\)\s*$")]
    private static partial Regex PortSuffix();
}
