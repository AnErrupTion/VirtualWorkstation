using QemuSharp.Structs.Enums;

namespace QemuSharp.Structs;

public class UsbController
{
    public UsbVersion Version { get; set; }

    public UsbController() {}

    public UsbController(UsbController other)
    {
        Version = other.Version;
    }
}