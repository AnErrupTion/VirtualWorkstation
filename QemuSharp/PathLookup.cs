namespace QemuSharp;

// TODO: Windows and macOS
public static class PathLookup
{
    public static readonly string[] BiosPaths =
    [
        "/usr/share/qemu",
        "/usr/share/seabios"
    ];

    public static readonly string[] BiosFiles =
    [
        "bios.bin"
    ];

    public static readonly string[] EfiPaths =
    [
        "/usr/share/qemu",
        "/usr/share/OVMF",
        "/usr/share/OVMF/x64",
        "/usr/share/ovmf"
    ];

    public static readonly string[] EfiFiles =
    [
        "edk2-x86_64-code.fd",
        "OVMF_CODE.fd",
        "OVMF.fd"
    ];
    
    public static readonly string[] EfiSecureBootFiles =
    [
        "edk2-x86_64-code.secboot.fd",
        "OVMF_CODE.secboot.fd",
        "OVMF.secboot.fd"
    ];

    public static readonly string[] EfiNvramFiles =
    [
        "edk2-i386-vars.fd",
        "OVMF_VARS.fd"
    ];

    public static readonly string[] EfiSecureBootNvramFiles =
    [
        "edk2-i386-vars.secboot.fd",
        "OVMF_VARS.secboot.fd"
    ];

    public static readonly string[] QemuPaths =
    [
        "/usr/bin"
    ];

    public const string QemuImgFile = "qemu-img";
    
    public static string GetQemuImgPath() => LookupFile(QemuPaths, QemuImgFile);
    
    public static string LookupFile(string[] paths, string file)
    {
        foreach (var path in paths)
            foreach (var fullFileName in Directory.EnumerateFiles(path))
                if (Path.GetFileName(fullFileName) == file) return fullFileName;

        return string.Empty;
    }

    public static string LookupFiles(string[] paths, string[] files)
    {
        foreach (var path in paths)
            foreach (var fullFileName in Directory.EnumerateFiles(path))
                if (files.Any(file => Path.GetFileName(fullFileName) == file)) return fullFileName;

        return string.Empty;
    }
}