using QemuSharp.Structs.Enums;

namespace QemuSharp.Structs;

public class Keyboard
{
    public KeyboardModel Model { get; set; }
    public ulong UsbController { get; set; }

    public Keyboard() {}

    public Keyboard(Keyboard other)
    {
        Model = other.Model;
        UsbController = other.UsbController;
    }
}