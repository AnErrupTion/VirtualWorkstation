using QemuSharp.Structs.Enums;

namespace QemuSharp.Structs;

public class TrustedPlatformModule
{
    public TpmDeviceType DeviceType { get; set; }
    public TpmType Type { get; set; }

    public TrustedPlatformModule() {}

    public TrustedPlatformModule(TrustedPlatformModule other)
    {
        DeviceType = other.DeviceType;
        Type = other.Type;
    }
}