using QemuSharp.Structs.Enums;

namespace QemuSharp.Structs;

public class Keyboard
{
    public KeyboardModel Model { get; set; }
    public string CustomModel { get; set; } = string.Empty;
    public ulong UsbController { get; set; }

    public Keyboard() {}

    public Keyboard(Keyboard other)
    {
        Model = other.Model;
        CustomModel = other.CustomModel;
        UsbController = other.UsbController;
    }
}