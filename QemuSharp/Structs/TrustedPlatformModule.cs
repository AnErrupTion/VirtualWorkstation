using QemuSharp.Structs.Enums;

namespace QemuSharp.Structs;

public class TrustedPlatformModule
{
    public TpmDeviceType DeviceType { get; set; }
    public string CustomDeviceType { get; set; } = string.Empty;
    public TpmType Type { get; set; }
    public string CustomType { get; set; } = string.Empty;
    public DeviceBus CustomTypeBus { get; set; } = new();

    public TrustedPlatformModule() {}

    public TrustedPlatformModule(TrustedPlatformModule other)
    {
        DeviceType = other.DeviceType;
        CustomDeviceType = other.CustomDeviceType;
        Type = other.Type;
        CustomType = other.CustomType;
        CustomTypeBus = new DeviceBus(other.CustomTypeBus);
    }
}