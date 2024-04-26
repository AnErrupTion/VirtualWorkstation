namespace QemuSharp;

public record LauncherResult(List<LauncherError> Errors, string? NvRamPath, List<Program> Programs);