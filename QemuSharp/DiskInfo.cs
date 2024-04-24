using QemuSharp.Structs.Enums;

namespace QemuSharp;

public record DiskInfo(DiskFormat Format, ByteSize Size);