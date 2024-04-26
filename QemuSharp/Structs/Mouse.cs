using QemuSharp.Structs.Enums;

namespace QemuSharp.Structs;

public class Mouse
{
    public MouseModel Model { get; set; }
    public string CustomModel { get; set; } = string.Empty;
    public BusType CustomModelBus { get; set; }
    public ulong UsbController { get; set; }
    public bool UseAbsolutePointing { get; set; }

    public Mouse() {}

    public Mouse(Mouse other)
    {
        Model = other.Model;
        CustomModel = other.CustomModel;
        CustomModelBus = other.CustomModelBus;
        UsbController = other.UsbController;
        UseAbsolutePointing = other.UseAbsolutePointing;
    }
}