using QemuSharp.Structs.Enums;

namespace QemuSharp.Structs;

public class AudioHostDevice
{
    public AudioHostType Type { get; set; }

    public AudioHostDevice() {}

    public AudioHostDevice(AudioHostDevice other)
    {
        Type = other.Type;
    }
}