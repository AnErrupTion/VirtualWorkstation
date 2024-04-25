using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using QemuSharp;
using QemuSharp.Structs;
using QemuSharp.Structs.Enums;
using Tomlyn;

namespace VirtualWorkstation;

public partial class NewVirtualMachine : Window
{
    private readonly MainWindow _parent;

    // I swear it's not null! I swear!!!!!
    private VirtualMachine _currentProfile = null!;

    public NewVirtualMachine(MainWindow parent)
    {
        InitializeComponent();

        _parent = parent;

        // FIXME: Determine correct amount of sockets, cores and threads
        Sockets.Maximum = Environment.ProcessorCount;
        Cores.Maximum = Environment.ProcessorCount;
        Threads.Maximum = Environment.ProcessorCount;

        var memoryInfo = GC.GetGCMemoryInfo();
        var ramMiB = memoryInfo.TotalAvailableMemoryBytes / 1024 / 1024;
        Ram.Maximum = ramMiB;

        Profile.SelectedIndex = 0;
        Folder.Text = GlobalSettings.VmFolder;
    }

    private async void FolderBrowse_OnClick(object? _, RoutedEventArgs e)
    {
        var storageFolders = await StorageProvider.OpenFolderPickerAsync(UiHelpers.DefaultFolderPickerOpenOptions);
        if (storageFolders.Count != 1) return;

        Folder.Text = storageFolders[0].Path.LocalPath;
    }

    private async void BootImageBrowse_OnClick(object? _, RoutedEventArgs e)
    {
        var storageFiles = await StorageProvider.OpenFilePickerAsync(UiHelpers.DefaultFilePickerOpenOptions);
        if (storageFiles.Count != 1) return;

        BootImage.Text = storageFiles[0].Path.LocalPath;
    }

    private void Profile_OnSelectionChanged(object? _, SelectionChangedEventArgs e)
    {
        var profile = Profiles.ToProfile((UiProfile)Profile.SelectedIndex);

        // Create a new instance to not modify the original profile
        _currentProfile = new VirtualMachine(profile.vm);

        NewDiskSize.Value = profile.diskSize;

        CheckResources();
    }

    private void CreateNewDisk_OnIsCheckedChanged(object? _, RoutedEventArgs e)
    {
        if (!CreateNewDisk.IsChecked!.Value) return;
        NewDiskPanel.IsEnabled = true;
        ExistingDiskPanel.IsEnabled = false;
    }

    private async void NewDiskCustomPathBrowse_OnClick(object? _, RoutedEventArgs e)
    {
        var storageFiles = await StorageProvider.OpenFilePickerAsync(UiHelpers.DefaultFilePickerOpenOptions);
        if (storageFiles.Count != 1) return;

        NewDiskCustomPath.Text = storageFiles[0].Path.LocalPath;
    }

    private void UseExistingDisk_OnIsCheckedChanged(object? _, RoutedEventArgs e)
    {
        if (!UseExistingDisk.IsChecked!.Value) return;
        NewDiskPanel.IsEnabled = false;
        ExistingDiskPanel.IsEnabled = true;
    }

    private void ExistingDiskPath_OnTextChanged(object? _, TextChangedEventArgs e)
    {
        var vmDiskPath = ExistingDiskPath.Text!;
        if (!File.Exists(vmDiskPath))
        {
            ExistingDiskType.Content = "N/A";
            ExistingDiskSize.Content = "N/A";

            Status.Content = "Invalid disk path. (Does it exist?)";
            return;
        }

        var vmDiskFolder = Path.GetDirectoryName(vmDiskPath);
        if (string.IsNullOrEmpty(vmDiskFolder))
        {
            Status.Content = "Invalid disk path. (Does it point to a root directory?)";
            return;
        }

        var (error, diskInfo) = DiskManager.GetDiskInfo(GlobalSettings.CustomQemuPath, vmDiskFolder, vmDiskPath);
        if (error != null)
        {
            switch (error)
            {
                case DiskManagerError.EmptyQemuPath:
                {
                    Status.Content = "Error getting existing disk information: empty QEMU path.";
                    return;
                }
                case DiskManagerError.QemuImgDoesNotExist:
                {
                    Status.Content = "Error getting existing disk information: QEMU image executable file does not exist.";
                    return;
                }
                case DiskManagerError.ProcessNotStarted:
                {
                    Status.Content = "Error getting existing disk information: process failed to start.";
                    return;
                }
                case DiskManagerError.ProcessHadErrors:
                {
                    Status.Content = "Error getting existing disk information: process had errors.";
                    return;
                }
                case DiskManagerError.UnexpectedProcessOutput:
                {
                    Status.Content = "Error getting existing disk information: found unexpected output in process.";
                    return;
                }
                default: throw new UnreachableException();
            }
        }

        if (NewDiskType.ComboBox.Items[(int)diskInfo!.Format] is not ComboBoxItem item) throw new UnreachableException();

        ExistingDiskType.Tag = diskInfo.Format; // This allows us to retrieve it later
        ExistingDiskType.Content = item.Content;
        ExistingDiskSize.Content = diskInfo.Size.ToString();

        Status.Content = "N/A";
    }

    private async void ExistingDiskPathBrowse_OnClick(object? _, RoutedEventArgs e)
    {
        var storageFiles = await StorageProvider.OpenFilePickerAsync(UiHelpers.DefaultFilePickerOpenOptions);
        if (storageFiles.Count != 1) return;

        ExistingDiskPath.Text = storageFiles[0].Path.LocalPath;
    }

    private void NoDisk_OnIsCheckedChanged(object? _, RoutedEventArgs e)
    {
        if (!NoDisk.IsChecked!.Value) return;
        NewDiskPanel.IsEnabled = false;
        ExistingDiskPanel.IsEnabled = false;
    }

    private void Create_OnClick(object? _, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(Name.Text))
        {
            Status.Content = "Please enter a valid name.";
            return;
        }

        if (Name.Text.Any(c => !char.IsAsciiLetterOrDigit(c) && c is not ' ' and not '.'))
        {
            Status.Content = "Name contains invalid characters.";
            return;
        }

        _currentProfile.Name = Name.Text;
        _currentProfile.Processor.Sockets = (ulong)Sockets.Value!.Value;
        _currentProfile.Processor.Cores = (ulong)Cores.Value!.Value;
        _currentProfile.Processor.Threads = (ulong)Threads.Value!.Value;
        _currentProfile.Ram = (ulong)Ram.Value!.Value;

        if (!string.IsNullOrEmpty(BootImage.Text))
        {
            if (!File.Exists(BootImage.Text))
            {
                Status.Content = "Boot image file doesn't exist.";
                return;
            }

            _currentProfile.Disks[1].Path = BootImage.Text;
        }
        else
        {
            _currentProfile.Disks.RemoveAt(1);
            _currentProfile.DiskControllers.RemoveAt(1);
        }

        if (CreateNewDisk.IsChecked!.Value)
        {
            var vmDiskFormat = (DiskFormat)NewDiskType.SelectedIndex;
            var vmDiskPath = string.IsNullOrEmpty(NewDiskCustomPath.Text)
                ? Path.Combine(Folder.Text!, Name.Text!, Path.ChangeExtension(Name.Text!, vmDiskFormat.ToExtensionString()))
                : NewDiskCustomPath.Text!;

            var vmDiskFolder = Path.GetDirectoryName(vmDiskPath);
            if (string.IsNullOrEmpty(vmDiskFolder))
            {
                Status.Content = "Invalid disk path. (Does it point to a root directory?)";
                return;
            }

            Directory.CreateDirectory(vmDiskFolder);

            _currentProfile.Disks[0].Format = vmDiskFormat;
            _currentProfile.Disks[0].Path = vmDiskPath;

            var error = DiskManager.CreateDisk(GlobalSettings.CustomQemuPath, vmDiskFolder,
                vmDiskFormat, vmDiskPath, new ByteSize((ulong)NewDiskSize.Value!.Value, ByteSuffix.GiB),
                PreAllocateNewDisk.IsChecked!.Value);

            if (error != null)
            {
                switch (error)
                {
                    case DiskManagerError.EmptyQemuPath:
                    {
                        Status.Content = "Error creating disk: empty QEMU path.";
                        return;
                    }
                    case DiskManagerError.QemuImgDoesNotExist:
                    {
                        Status.Content = "Error creating disk: QEMU image executable file does not exist.";
                        return;
                    }
                    case DiskManagerError.ProcessNotStarted:
                    {
                        Status.Content = "Error creating disk: process failed to start.";
                        return;
                    }
                    case DiskManagerError.ProcessHadErrors:
                    {
                        Status.Content = "Error creating disk: process had errors.";
                        return;
                    }
                    case DiskManagerError.UnexpectedProcessOutput:
                    {
                        // We don't get the output in CreateDisk()
                        throw new UnreachableException();
                    }
                    default: throw new UnreachableException();
                }
            }
        }
        else if (UseExistingDisk.IsChecked!.Value)
        {
            var vmDiskPath = ExistingDiskPath.Text!;
            if (!File.Exists(vmDiskPath))
            {
                Status.Content = "Disk file does not exist.";
                return;
            }

            _currentProfile.Disks[0].Path = vmDiskPath;

            var vmDiskFolder = Path.GetDirectoryName(vmDiskPath);
            // We should've already checked that in the TextChanged event of ExistingDiskPath
            if (string.IsNullOrEmpty(vmDiskFolder)) throw new UnreachableException();

            if (ExistingDiskType.Tag is not DiskFormat diskFormat) throw new UnreachableException();
            _currentProfile.Disks[0].Format = diskFormat;
        }

        var vmConfigFile = Path.ChangeExtension(Name.Text!, "toml");
        var vmConfigPath = Path.Combine(Folder.Text!, Name.Text!, vmConfigFile);
        var vmConfigToml = Toml.FromModel(_currentProfile);

        Directory.CreateDirectory(Path.GetDirectoryName(vmConfigPath)!);
        File.WriteAllText(vmConfigPath, vmConfigToml);

        _parent.VmList.Items.Add(new ListBoxItem
        {
            Content = _currentProfile.Name,
            Tag = new VirtualMachineTabPage(_parent, vmConfigPath, ref _currentProfile)
            {
                Index = _parent.VmList.Items.Count
            }
        });

        Close();
    }

    private void CheckResources()
    {
        var status = "You don't have enough";
        var notEnoughResources = false;

        if (Sockets.Maximum < _currentProfile.Processor.Sockets)
        {
            status += " sockets";
            notEnoughResources = true;
        } else Sockets.Value = _currentProfile.Processor.Sockets;

        if (Cores.Maximum < _currentProfile.Processor.Cores)
        {
            status += notEnoughResources ? ", cores" : " cores";
            notEnoughResources = true;
        } else Cores.Value = _currentProfile.Processor.Cores;

        if (Threads.Maximum < _currentProfile.Processor.Threads)
        {
            status += notEnoughResources ? ", threads" : " threads";
            notEnoughResources = true;
        } else Threads.Value = _currentProfile.Processor.Threads;

        if (Ram.Maximum < _currentProfile.Ram)
        {
            status += notEnoughResources ? ", RAM" : " RAM";
            notEnoughResources = true;
        } else Ram.Value = _currentProfile.Ram;

        if (notEnoughResources) Status.Content = $"{status} for this profile.";
    }
}