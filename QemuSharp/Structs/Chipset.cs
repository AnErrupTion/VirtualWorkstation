using QemuSharp.Structs.Enums;

namespace QemuSharp.Structs;

public class Chipset
{
    public ChipsetModel Model { get; set; }

    public Chipset() {}

    public Chipset(Chipset other)
    {
        Model = other.Model;
    }
}