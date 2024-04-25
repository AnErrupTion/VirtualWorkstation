using QemuSharp.Structs.Enums;

namespace QemuSharp;

public record DiskInfo(DiskFormat Format, string? CustomFormat, ByteSize Size);