using QemuSharp.Structs.Enums;

namespace QemuSharp.Structs;

public class DiskController
{
    public DiskBus Model { get; set; }
    public string CustomModel { get; set; } = string.Empty;
    public ulong UsbController { get; set; }

    public DiskController() {}

    public DiskController(DiskController other)
    {
        Model = other.Model;
        CustomModel = other.CustomModel;
        UsbController = other.UsbController;
    }
}