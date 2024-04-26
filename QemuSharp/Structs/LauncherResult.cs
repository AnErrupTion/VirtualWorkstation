namespace QemuSharp.Structs;

public record LauncherResult(List<LauncherError> Errors, string? QuotedQemuSystemPath, string? NvRamPath,
    List<string> Arguments);