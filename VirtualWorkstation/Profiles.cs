using System.Diagnostics;
using QemuSharp.Structs;
using QemuSharp.Structs.Enums;

namespace VirtualWorkstation;

/// <summary>
/// Used by <see cref="NewVirtualMachine"/>. The main disk is always going to be disk 0 and use disk controller 0, while
/// the boot image is always going to be disk 1 and use disk controller 1. Values that will be overriden (like RAM or
/// processor topology) define recommended values for the profile.
/// </summary>
public static class Profiles
{
    public static readonly VirtualMachine Windows10 = new()
    {
        Architecture = Architecture.Amd64,
        UseHardwareAcceleration = true,
        Firmware = new Firmware { Type = FirmwareType.Efi },
        Chipset = new Chipset { Model = ChipsetModel.X86Q35, Q35Options = new Q35Options { EnablePs2Emulation = false, AcpiState = AcpiChipsetState.On } },
        Ram = 2048,
        Display = new Display { Type = DisplayType.Auto },
        AudioHostDevice = new AudioHostDevice { Type = AudioHostType.Auto },
        Processor = new Processor { Model = ProcessorModel.X86Host, Sockets = 1, Cores = 2, Threads = 1 },
        TrustedPlatformModule = new TrustedPlatformModule { DeviceType = TpmDeviceType.None },
        UsbControllers =
        [
            new UsbController { Version = UsbVersion.Xhci }
        ],
        NetworkInterfaces =
        [
            new NetworkInterface { Type = NetworkType.Nat, Card = NetworkCard.E1000E }
        ],
        GraphicControllers =
        [
            new GraphicsController { Card = GraphicsCard.Qxl, HasVgaEmulation = true }
        ],
        AudioControllers =
        [
            new AudioController { Card = SoundCard.IntelHda9, HasOutput = true }
        ],
        DiskControllers =
        [
            new DiskController { Model = DiskBus.Nvme },
            new DiskController { Model = DiskBus.Ich9Ahci }
        ],
        Disks =
        [
            new Disk { Controller = 0, CacheMethod = DiskCacheMethod.None, IsSsd = true },
            new Disk { Controller = 1, Format = DiskFormat.Raw, CacheMethod = DiskCacheMethod.None, IsCdrom = true }
        ],
        Keyboards =
        [
            new Keyboard { Model = KeyboardModel.Usb, UsbController = 0 }
        ],
        Mice =
        [
            new Mouse { Model = MouseModel.Usb, UsbController = 0, UseAbsolutePointing = true }
        ]
    };

    public static readonly VirtualMachine LinuxVirtIo = new()
    {
        Architecture = Architecture.Amd64,
        UseHardwareAcceleration = true,
        Firmware = new Firmware { Type = FirmwareType.Efi },
        Chipset = new Chipset { Model = ChipsetModel.X86Q35, Q35Options = new Q35Options { EnablePs2Emulation = false, AcpiState = AcpiChipsetState.On } },
        Ram = 1024,
        Display = new Display { Type = DisplayType.Auto },
        AudioHostDevice = new AudioHostDevice { Type = AudioHostType.Auto },
        Processor = new Processor { Model = ProcessorModel.X86Host, Sockets = 1, Cores = 2, Threads = 1 },
        TrustedPlatformModule = new TrustedPlatformModule { DeviceType = TpmDeviceType.None },
        NetworkInterfaces =
        [
            new NetworkInterface { Type = NetworkType.Nat, Card = NetworkCard.VirtIo }
        ],
        GraphicControllers =
        [
            new GraphicsController { Card = GraphicsCard.VirtIo, HasVgaEmulation = true, HasGraphicsAcceleration = true }
        ],
        AudioControllers =
        [
            new AudioController { Card = SoundCard.VirtIo, HasOutput = true }
        ],
        DiskControllers =
        [
            new DiskController { Model = DiskBus.VirtIoBlock },
            new DiskController { Model = DiskBus.Ich9Ahci }
        ],
        Disks =
        [
            new Disk { Controller = 0, CacheMethod = DiskCacheMethod.None, IsSsd = true },
            new Disk { Controller = 1, Format = DiskFormat.Raw, CacheMethod = DiskCacheMethod.None, IsCdrom = true }
        ],
        Keyboards =
        [
            new Keyboard { Model = KeyboardModel.VirtIo }
        ],
        Mice =
        [
            new Mouse { Model = MouseModel.VirtIo, UseAbsolutePointing = true }
        ]
    };

    public static (VirtualMachine vm, ulong diskSize) ToProfile(UiProfile profile) => profile switch
    {
        UiProfile.Windows10 => (Windows10, 32),
        UiProfile.LinuxVirtIo => (LinuxVirtIo, 16),
        _ => throw new UnreachableException()
    };
}