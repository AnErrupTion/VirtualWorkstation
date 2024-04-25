using QemuSharp.Structs.Enums;

namespace QemuSharp.Structs;

public class Chipset
{
    public ChipsetModel Model { get; set; }
    public string CustomModel { get; set; } = string.Empty;
    public bool ForceUseNormalPci { get; set; }

    public Chipset() {}

    public Chipset(Chipset other)
    {
        Model = other.Model;
        CustomModel = other.CustomModel;
        ForceUseNormalPci = other.ForceUseNormalPci;
    }
}