using QemuSharp.Structs.Enums;

namespace QemuSharp.Structs;

public class Firmware
{
    public FirmwareType Type { get; set; }
    public string CustomPath { get; set; } = string.Empty;
    public string CustomNvRamPath { get; set; } = string.Empty;

    public Firmware() {}

    public Firmware(Firmware other)
    {
        Type = other.Type;
        CustomPath = other.CustomPath;
        CustomNvRamPath = other.CustomNvRamPath;
    }
}