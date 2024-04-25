using QemuSharp.Structs.Enums;

namespace QemuSharp.Structs;

public class AudioHostDevice
{
    public AudioHostType Type { get; set; }

    public string CustomType { get; set; } = string.Empty;

    public AudioHostDevice() {}

    public AudioHostDevice(AudioHostDevice other)
    {
        Type = other.Type;
        CustomType = other.CustomType;
    }
}