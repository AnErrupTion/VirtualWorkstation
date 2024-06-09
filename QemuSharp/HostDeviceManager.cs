using System.Diagnostics;
using System.Globalization;

namespace QemuSharp;

public static class HostDeviceManager
{
    private static readonly char[] Separator = [' '];

    public static (HostDeviceManagerError?, List<UsbDevice>?) GetUsbDevices()
    {
        if (!OperatingSystem.IsLinux()) return (HostDeviceManagerError.UnsupportedHostOperatingSystem, null);

        var devices = new List<UsbDevice>();
        var lsusbProcess = Process.Start(new ProcessStartInfo
        {
            FileName = "lsusb",
            RedirectStandardOutput = true
        });

        if (lsusbProcess == null) return (HostDeviceManagerError.ProcessNotStarted, null);

        lsusbProcess.WaitForExit();

        while (!lsusbProcess.StandardOutput.EndOfStream)
        {
            var lsusbDevice = lsusbProcess.StandardOutput.ReadLine();
            if (string.IsNullOrEmpty(lsusbDevice)) continue;

            // 7 substrings maximum because we don't want to split the name
            var array = lsusbDevice.Split(Separator, 7);
            var bus = ushort.Parse(array[1]);
            var device = ushort.Parse(array[3][..^1]);
            var vendorProductCombo = array[5].Split(':');
            var vendorId = ushort.Parse(vendorProductCombo[0], NumberStyles.HexNumber);
            var productId = ushort.Parse(vendorProductCombo[1], NumberStyles.HexNumber);
            var name = array[6];

            devices.Add(new UsbDevice(bus, device, vendorId, productId, name));
        }

        return (null, devices);
    }
}