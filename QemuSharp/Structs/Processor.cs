using QemuSharp.Structs.Enums;

namespace QemuSharp.Structs;

public class Processor
{
    public ProcessorModel Model { get; set; }
    public List<ProcessorFeature> AddFeatures { get; set; } = [];
    public List<ProcessorFeature> RemoveFeatures { get; set; } = [];
    public ulong Sockets { get; set; }
    public ulong Cores { get; set; }
    public ulong Threads { get; set; }

    public Processor() {}

    public Processor(Processor other)
    {
        Model = other.Model;
        AddFeatures = [..other.AddFeatures];
        RemoveFeatures = [..other.RemoveFeatures];
        Sockets = other.Sockets;
        Cores = other.Cores;
        Threads = other.Threads;
    }
}