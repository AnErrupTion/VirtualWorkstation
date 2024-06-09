namespace QemuSharp;

public record UsbDevice(ushort Bus, ushort Device, ushort VendorId, ushort ProductId, string Name);