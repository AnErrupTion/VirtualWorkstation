namespace QemuSharp.Structs;

public class UsbHostDevice
{
    public ushort VendorId { get; set; }
    public ushort ProductId { get; set; }
    public ulong UsbController { get; set; }

    public UsbHostDevice() {}

    public UsbHostDevice(UsbHostDevice other)
    {
        VendorId = other.VendorId;
        ProductId = other.ProductId;
        UsbController = other.UsbController;
    }
}