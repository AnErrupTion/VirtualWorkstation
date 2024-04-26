using QemuSharp.Structs.Enums;

namespace QemuSharp.Structs;

public class DiskController
{
    public DiskBus Model { get; set; }
    public string CustomModel { get; set; } = string.Empty;
    public DeviceBus CustomModelBus { get; set; } = new();
    public ulong UsbController { get; set; }

    public DiskController() {}

    public DiskController(DiskController other)
    {
        Model = other.Model;
        CustomModel = other.CustomModel;
        CustomModelBus = new DeviceBus(other.CustomModelBus);
        UsbController = other.UsbController;
    }
}