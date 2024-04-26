namespace QemuSharp.Structs;

public class Q35Options
{
    public bool EnablePs2Emulation { get; set; }

    public Q35Options() {}

    public Q35Options(Q35Options other)
    {
        EnablePs2Emulation = other.EnablePs2Emulation;
    }
}