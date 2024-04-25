using QemuSharp.Structs.Enums;

namespace QemuSharp.Structs;

public class Disk
{
    public ulong Controller { get; set; }
    public DiskFormat Format { get; set; }
    public string CustomFormat { get; set; } = string.Empty;
    public DiskCacheMethod CacheMethod { get; set; }
    public string CustomCacheMethod { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public bool IsSsd { get; set; }
    public bool IsCdrom { get; set; }
    public bool IsRemovable { get; set; }

    public Disk() {}

    public Disk(Disk other)
    {
        Controller = other.Controller;
        Format = other.Format;
        CustomFormat = other.CustomFormat;
        CacheMethod = other.CacheMethod;
        CustomCacheMethod = other.CustomCacheMethod;
        Path = other.Path;
        IsSsd = other.IsSsd;
        IsCdrom = other.IsCdrom;
        IsRemovable = other.IsRemovable;
    }
}