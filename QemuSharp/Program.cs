namespace QemuSharp;

public record Program(string? Path, List<string> Arguments, bool WaitForExit);