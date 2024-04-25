using QemuSharp.Structs.Enums;

namespace QemuSharp.Structs;

public class Display
{
    public DisplayType Type { get; set; }
    public string CustomType { get; set; } = string.Empty;

    public Display() {}

    public Display(Display other)
    {
        Type = other.Type;
        CustomType = other.CustomType;
    }
}