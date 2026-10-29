using Phomemo.Printing.Transport;

namespace Phomemo.Printing.Windows;

/// <summary>Crea el transporte adecuado para un <see cref="PortInfo"/>.</summary>
public static class Transports
{
    public static IPrinterTransport Create(PortInfo port) => port.Kind switch
    {
        PortKind.BluetoothLE => new BleTransport(port.Address ?? throw new ArgumentException("Falta la dirección Bluetooth.", nameof(port))),
        _ => new SerialPortTransport(port.Name),
    };
}
