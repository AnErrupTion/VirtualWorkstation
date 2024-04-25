namespace QemuSharp;

public enum DiskManagerError
{
    EmptyQemuPath,

    QemuImgDoesNotExist,

    EmptyCustomDiskFormat,
    InvalidCustomDiskFormat,

    ProcessNotStarted,
    ProcessHadErrors,

    UnexpectedProcessOutput
}