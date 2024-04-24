using QemuSharp.Structs.Enums;

namespace QemuSharp.Structs;

public class Disk
{
    public ulong Controller { get; set; }
    public DiskFormat Format { get; set; }
    public DiskCacheMethod CacheMethod { get; set; }
    public string Path { get; set; } = string.Empty;
    public bool IsSsd { get; set; }
    public bool IsCdrom { get; set; }
    public bool IsRemovable { get; set; }

    public Disk() {}

    public Disk(Disk other)
    {
        Controller = other.Controller;
        Format = other.Format;
        CacheMethod = other.CacheMethod;
        Path = other.Path;
        IsSsd = other.IsSsd;
        IsCdrom = other.IsCdrom;
        IsRemovable = other.IsRemovable;
    }
}