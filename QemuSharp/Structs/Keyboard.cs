using QemuSharp.Structs.Enums;

namespace QemuSharp.Structs;

public class Keyboard
{
    public KeyboardModel Model { get; set; }
    public string CustomModel { get; set; } = string.Empty;
    public BusType CustomModelBus { get; set; }
    public ulong UsbController { get; set; }

    public Keyboard() {}

    public Keyboard(Keyboard other)
    {
        Model = other.Model;
        CustomModel = other.CustomModel;
        CustomModelBus = other.CustomModelBus;
        UsbController = other.UsbController;
    }
}