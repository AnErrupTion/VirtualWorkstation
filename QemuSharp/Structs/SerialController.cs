using QemuSharp.Structs.Enums;

namespace QemuSharp.Structs;

public class SerialController
{
    public SerialType Type { get; set; }
    public string CustomType { get; set; } = string.Empty;
    public DeviceBus CustomTypeBus { get; set; } = new();
    public ulong UsbController { get; set; }

    public SerialController() {}

    public SerialController(SerialController other)
    {
        Type = other.Type;
        CustomType = other.CustomType;
        CustomTypeBus = other.CustomTypeBus;
        UsbController = other.UsbController;
    }
}