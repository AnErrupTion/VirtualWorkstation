using QemuSharp.Structs.Enums;

namespace QemuSharp.Structs;

public class Chipset
{
    public ChipsetModel Model { get; set; }
    public string CustomModel { get; set; } = string.Empty;
    public bool ForceUseNormalPci { get; set; }
    public Q35Options? Q35Options { get; set; }
    public I440FxOptions? I440FxOptions { get; set; }

    public Chipset() {}

    public Chipset(Chipset other)
    {
        Model = other.Model;
        CustomModel = other.CustomModel;
        ForceUseNormalPci = other.ForceUseNormalPci;
        if (other.Q35Options != null) Q35Options = new Q35Options(other.Q35Options);
        if (other.I440FxOptions != null) I440FxOptions = new I440FxOptions(other.I440FxOptions);
    }
}