using QemuSharp.Structs;
using QemuSharp.Structs.Enums;

namespace VirtualWorkstation.Profile;

public static partial class Profiles
{
    private static readonly VirtualMachine WindowsXpX86 = new()
    {
        Architecture = Architecture.I386,
        UseHardwareAcceleration = true,
        Firmware = new Firmware { Type = FirmwareType.X86LegacyBios },
        Chipset = new Chipset
        {
            Model = ChipsetModel.X86I440Fx,
            I440FxOptions = new I440FxOptions { EnablePs2Emulation = true, AcpiState = AcpiChipsetState.On, SouthbridgeType = I440FxSouthbridgeType.Piix4Isa }
        },
        Memory = new Memory { Size = 64, UseBallooning = false },
        Display = new Display { Type = DisplayType.Auto },
        AudioHostDevice = new AudioHostDevice { Type = AudioHostType.Auto },
        Processor = new Processor { Model = ProcessorModel.X86Host, Sockets = 1, Cores = 1, Threads = 1 },
        TrustedPlatformModule = new TrustedPlatformModule { DeviceType = TpmDeviceType.None },
        UsbControllers = 
        [
            new UsbController { Version = UsbVersion.Uhci }
        ],
        NetworkInterfaces =
        [
            new NetworkInterface { Type = NetworkType.Nat, Card = NetworkCard.Rtl8139 }
        ],
        GraphicControllers =
        [
            new GraphicsController { Card = GraphicsCard.Cirrus, HasVgaEmulation = true }
        ],
        AudioControllers =
        [
            new AudioController { Card = SoundCard.IntelAc97, HasOutput = true }
        ],
        DiskControllers =
        [
            new DiskController { Model = DiskBus.SouthbridgeIde }
        ],
        Disks =
        [
            new Disk { Controller = 0, CacheMethod = DiskCacheMethod.None, IsSsd = true },
            new Disk { Controller = 0, Format = DiskFormat.Raw, CacheMethod = DiskCacheMethod.None, IsCdrom = true }
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
}