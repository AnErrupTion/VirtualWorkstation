namespace QemuSharp.Structs;

public class Memory
{
    public ulong Size { get; set; }
    public bool UseBallooning { get; set; }
    public bool MemorySharing { get; set; }

    public Memory() {}

    public Memory(Memory other)
    {
        Size = other.Size;
        UseBallooning = other.UseBallooning;
        MemorySharing = other.MemorySharing;
    }
}