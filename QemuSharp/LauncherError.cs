namespace QemuSharp;

public record LauncherError(LauncherErrorType Type, int Index = -1, int SecondIndex = -1);