using QemuSharp.Structs.Enums;

namespace QemuSharp.Structs;

public class Mouse
{
    public MouseModel Model { get; set; }
    public ulong UsbController { get; set; }
    public bool UseAbsolutePointing { get; set; }

    public Mouse() {}

    public Mouse(Mouse other)
    {
        Model = other.Model;
        UsbController = other.UsbController;
        UseAbsolutePointing = other.UseAbsolutePointing;
    }
}