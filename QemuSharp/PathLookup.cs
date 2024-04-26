namespace QemuSharp;

public static class PathLookup
{
    private static readonly string QemuWindowsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "qemu");
    private static readonly string QemuWindowsFirmwarePath = Path.Combine(QemuWindowsPath, "share");

    public static readonly string[] BiosPaths =
    [
        "/usr/share/qemu",
        "/usr/share/seabios",
        "/opt/homebrew/share/qemu",
        QemuWindowsFirmwarePath
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
        "/usr/share/ovmf",
        "/opt/homebrew/share/qemu",
        QemuWindowsFirmwarePath
    ];

    public static readonly string[] EfiFiles =
    [
        "edk2-x86_64-code.fd",
        "OVMF_CODE.fd",
        "OVMF.fd"
    ];
    
    public static readonly string[] EfiSecureBootFiles =
    [
        "edk2-x86_64-secure-code.fd",
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
        "edk2-i386-vars.fd", // This one doesn't have a special name
        "OVMF_VARS.secboot.fd"
    ];

    public static readonly string[] ExecutablePaths =
    [
        "/usr/bin",
        "/opt/homebrew/bin",
        QemuWindowsPath
    ];

    public static readonly string QemuImgFile = OperatingSystem.IsWindows() ? "qemu-img.exe" : "qemu-img";
    public const string SwtpmFile = "swtpm";

    public static string GetQemuImgPath() => LookupFile(ExecutablePaths, QemuImgFile);
    
    public static string LookupFile(string[] paths, string file)
    {
        foreach (var path in paths)
        {
            if (!Directory.Exists(path)) continue;

            foreach (var fullFileName in Directory.EnumerateFiles(path))
                if (Path.GetFileName(fullFileName) == file) return fullFileName;
        }

        return string.Empty;
    }

    public static string LookupFiles(string[] paths, string[] files)
    {
        foreach (var path in paths)
        {
            if (!Directory.Exists(path)) continue;

            foreach (var fullFileName in Directory.EnumerateFiles(path))
                if (files.Any(file => Path.GetFileName(fullFileName) == file)) return fullFileName;
        }

        return string.Empty;
    }
}