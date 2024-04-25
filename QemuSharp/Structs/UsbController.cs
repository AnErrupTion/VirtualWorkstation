using QemuSharp.Structs.Enums;

namespace QemuSharp.Structs;

public class UsbController
{
    public UsbVersion Version { get; set; }
    public string CustomVersion { get; set; } = string.Empty;

    public UsbController() {}

    public UsbController(UsbController other)
    {
        Version = other.Version;
        CustomVersion = other.CustomVersion;
    }
}