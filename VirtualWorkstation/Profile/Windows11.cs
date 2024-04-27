using QemuSharp.Structs;
using QemuSharp.Structs.Enums;

namespace VirtualWorkstation.Profile;

public static partial class Profiles
{
    private static readonly VirtualMachine Windows11 = new()
    {
        Architecture = Architecture.Amd64,
        UseHardwareAcceleration = true,
        Firmware = new Firmware { Type = FirmwareType.EfiSecureBoot },
        Chipset = new Chipset { Model = ChipsetModel.X86Q35, Q35Options = new Q35Options { EnablePs2Emulation = false, AcpiState = AcpiChipsetState.On } },
        Ram = 4096,
        Display = new Display { Type = DisplayType.Auto },
        AudioHostDevice = new AudioHostDevice { Type = AudioHostType.Auto },
        Processor = new Processor { Model = ProcessorModel.X86Host, Sockets = 1, Cores = 2, Threads = 1 },
        TrustedPlatformModule = new TrustedPlatformModule { DeviceType = TpmDeviceType.Emulated, Type = TpmType.Crb, Version = TpmVersion.V20 },
        NetworkInterfaces =
        [
            new NetworkInterface { Type = NetworkType.Nat, Card = NetworkCard.VirtIo }
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
            new Keyboard { Model = KeyboardModel.VirtIo, UsbController = 0 }
        ],
        Mice =
        [
            new Mouse { Model = MouseModel.VirtIo, UsbController = 0, UseAbsolutePointing = true }
        ]
    };
}