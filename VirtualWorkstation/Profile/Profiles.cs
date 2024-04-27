using System.Diagnostics;
using QemuSharp.Structs;

namespace VirtualWorkstation.Profile;

/// <summary>
/// Used by <see cref="NewVirtualMachine"/>. The main disk is always going to be disk 0 and use disk controller 0, while
/// the boot image is always going to be disk 1 and use disk controller 1. Values that will be overriden (like RAM or
/// processor topology) define recommended values for the profile.
/// Note: Some profiles may only have 1 disk controller. In this case, it's the same for both the disk and the boot
/// image.
/// </summary>
public static partial class Profiles
{
    public static (VirtualMachine vm, ulong diskSize) ToProfile(UiProfile profile) => profile switch
    {
        UiProfile.Windows11X64 => (Windows11X64, 64),
        UiProfile.Windows10X64 => (Windows10X64, 32),
        UiProfile.Windows10X86 => (Windows10X86, 32),
        UiProfile.Windows8X64 => (Windows8X64, 20),
        UiProfile.Windows8X86 => (Windows8X86, 16),
        UiProfile.Windows7X64 => (Windows7X64, 20),
        UiProfile.Windows7X86 => (Windows7X86, 16),
        UiProfile.WindowsVistaX64 => (WindowsVistaX64, 20),
        UiProfile.WindowsVistaX86 => (WindowsVistaX86, 20),
        UiProfile.WindowsXpX64 => (WindowsXpX64, 4),
        UiProfile.WindowsXpX86 => (WindowsXpX86, 4),
        UiProfile.Windows2000X86 => (Windows2000X86, 1),
        UiProfile.LinuxX64 => (LinuxX64, 16),
        _ => throw new UnreachableException()
    };
}