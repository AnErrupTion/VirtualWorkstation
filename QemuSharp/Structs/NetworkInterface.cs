using QemuSharp.Structs.Enums;

namespace QemuSharp.Structs;

public class NetworkInterface
{
    public NetworkType Type { get; set; }
    public string CustomType { get; set; } = string.Empty;
    public NetworkCard Card { get; set; }
    public string CustomCard { get; set; } = string.Empty;
    public BusType CustomCardBus { get; set; }
    public ulong UsbController { get; set; }

    public NetworkInterface() {}

    public NetworkInterface(NetworkInterface other)
    {
        Type = other.Type;
        CustomType = other.CustomType;
        Card = other.Card;
        CustomCard = other.CustomCard;
        CustomCardBus = other.CustomCardBus;
        UsbController = other.UsbController;
    }
}