using System.Diagnostics;
using QemuSharp.Structs.Enums;

namespace QemuSharp;

public static class DiskManager
{
    public static DiskManagerError? CreateDisk(string customQemuPath, string workingDirectory, DiskFormat diskFormat,
        string path, ByteSize size, bool preAllocate)
    {
        var qemuImgPath = !string.IsNullOrEmpty(customQemuPath)
            ? Path.Combine(customQemuPath, PathLookup.QemuImgFile)
            : PathLookup.GetQemuImgPath();

        if (string.IsNullOrEmpty(qemuImgPath)) return DiskManagerError.EmptyQemuPath;
        if (!File.Exists(qemuImgPath)) return DiskManagerError.QemuImgDoesNotExist;

        var format = diskFormat switch
        {
            DiskFormat.QCow2 => "qcow2",
            DiskFormat.Vdi => "vdi",
            DiskFormat.Vmdk => "vmdk",
            DiskFormat.VhdX => "vhdx",
            DiskFormat.Raw => "raw",
            _ => throw new UnreachableException()
        };

        var process = Process.Start(new ProcessStartInfo(qemuImgPath,
        [
            "create", "-o", $"preallocation={(preAllocate ? "full" : "off")}", "-q", "-f", format, path, size.ToString()
        ])
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        });
        if (process == null) return DiskManagerError.ProcessNotStarted;

        return process.StandardError.ReadToEnd().Length != 0 ? DiskManagerError.ProcessHadErrors : null;
    }

    public static (DiskManagerError?, DiskInfo?) GetDiskInfo(string customQemuPath, string workingDirectory, string path)
    {
        var qemuImgPath = !string.IsNullOrEmpty(customQemuPath)
            ? Path.Combine(customQemuPath, PathLookup.QemuImgFile)
            : PathLookup.GetQemuImgPath();

        if (string.IsNullOrEmpty(qemuImgPath)) return (DiskManagerError.EmptyQemuPath, null);
        if (!File.Exists(qemuImgPath)) return (DiskManagerError.QemuImgDoesNotExist, null);

        var process = Process.Start(new ProcessStartInfo(qemuImgPath,
        [
            "info", path
        ])
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        });
        if (process == null) return (DiskManagerError.ProcessNotStarted, null);
        if (process.StandardError.ReadToEnd().Length != 0) return (DiskManagerError.ProcessHadErrors, null);

        DiskFormat? diskFormat = null;
        ByteSize? byteSize = null;

        var output = process.StandardOutput.ReadToEnd().Split('\n');
        foreach (var line in output)
        {
            if (line.StartsWith("file format: "))
            {
                var format = line[13..].ToLowerInvariant();
                diskFormat = format switch
                {
                    "qcow2" => DiskFormat.QCow2,
                    "vdi" => DiskFormat.Vdi,
                    "vmdk" => DiskFormat.Vmdk,
                    "vhdx" => DiskFormat.VhdX,
                    "raw" => DiskFormat.Raw,
                    _ => throw new NotImplementedException()
                };
            }
            else if (line.StartsWith("virtual size: "))
            {
                var size = line.Substring(14, line.IndexOf('(') - 15);
                byteSize = ByteSize.FromString(size);
            }
        }

        // Pattern matching allows us to do a strict equality check with null, without any operator overloading stuff
        if (diskFormat is null || byteSize is null) return (DiskManagerError.UnexpectedProcessOutput, null);

        return (null, new DiskInfo(diskFormat.Value, byteSize));
    }
}