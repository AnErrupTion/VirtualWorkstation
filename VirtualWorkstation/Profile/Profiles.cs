using System.Diagnostics;
using QemuSharp.Structs;

namespace VirtualWorkstation.Profile;

/// <summary>
/// Used by <see cref="NewVirtualMachine"/>. The main disk is always going to be disk 0 and use disk controller 0, while
/// the boot image is always going to be disk 1 and use disk controller 1. Values that will be overriden (like RAM or
/// processor topology) define recommended values for the profile.
/// </summary>
public static partial class Profiles
{
    public static (VirtualMachine vm, ulong diskSize) ToProfile(UiProfile profile) => profile switch
    {
        UiProfile.Windows10 => (Windows10, 32),
        UiProfile.Linux => (Linux, 16),
        _ => throw new UnreachableException()
    };
}