namespace QemuSharp;

public enum DiskManagerError
{
    EmptyQemuPath,

    QemuImgDoesNotExist,

    ProcessNotStarted,
    ProcessHadErrors,

    UnexpectedProcessOutput
}