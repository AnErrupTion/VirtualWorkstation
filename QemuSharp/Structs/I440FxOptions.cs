using QemuSharp.Structs.Enums;

namespace QemuSharp.Structs;

public class I440FxOptions
{
    public bool EnablePs2Emulation { get; set; }
    public bool EnableAcpi { get; set; }
    public I440FxSouthbridgeType SouthbridgeType { get; set; }

    public I440FxOptions() {}

    public I440FxOptions(I440FxOptions other)
    {
        EnablePs2Emulation = other.EnablePs2Emulation;
        EnableAcpi = other.EnableAcpi;
        SouthbridgeType = other.SouthbridgeType;
    }
}