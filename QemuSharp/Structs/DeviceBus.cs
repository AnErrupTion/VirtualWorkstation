using QemuSharp.Structs.Enums;

namespace QemuSharp.Structs;

public class DeviceBus
{
    public BusType Type { get; set; }
    public string CustomType { get; set; } = string.Empty;

    public DeviceBus() {}

    public DeviceBus(DeviceBus other)
    {
        Type = other.Type;
        CustomType = other.CustomType;
    }
}