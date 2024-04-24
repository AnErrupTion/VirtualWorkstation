using QemuSharp.Structs.Enums;

namespace QemuSharp.Structs;

public class Display
{
    public DisplayType Type { get; set; }

    public Display() {}

    public Display(Display other)
    {
        Type = other.Type;
    }
}