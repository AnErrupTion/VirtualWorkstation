using QemuSharp.Structs.Enums;

namespace QemuSharp.Structs;

public class DiskController
{
    public DiskBus Model { get; set; }
    public ulong UsbController { get; set; }

    public DiskController() {}

    public DiskController(DiskController other)
    {
        Model = other.Model;
        UsbController = other.UsbController;
    }
}