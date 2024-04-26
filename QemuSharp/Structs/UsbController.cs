using QemuSharp.Structs.Enums;

namespace QemuSharp.Structs;

public class UsbController
{
    public UsbVersion Version { get; set; }
    public string CustomVersion { get; set; } = string.Empty;
    public DeviceBus CustomVersionBus { get; set; } = new();

    public UsbController() {}

    public UsbController(UsbController other)
    {
        Version = other.Version;
        CustomVersion = other.CustomVersion;
        CustomVersionBus = new DeviceBus(other.CustomVersionBus);
    }
}