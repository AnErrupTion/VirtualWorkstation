using QemuSharp.Structs.Enums;

namespace QemuSharp.Structs;

public class NetworkInterface
{
    public NetworkType Type { get; set; }
    public NetworkCard Card { get; set; }
    public ulong UsbController { get; set; }

    public NetworkInterface() {}

    public NetworkInterface(NetworkInterface other)
    {
        Type = other.Type;
        Card = other.Card;
        UsbController = other.UsbController;
    }
}