using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using QemuSharp;
using QemuSharp.Structs;
using QemuSharp.Structs.Enums;
using Tomlyn;
using VirtualWorkstation.Profile;

namespace VirtualWorkstation;

public partial class NewVirtualMachine : Window
{
    private readonly MainWindow _parent;
    private readonly IBrush? _labelBrush, _textBrush, _numericUpDownBrush;

    // I swear it's not null! I swear!!!!!
    private VirtualMachine _currentProfile = null!;

    public NewVirtualMachine(MainWindow parent)
    {
        InitializeComponent();

        _parent = parent;
        _labelBrush = NameLabel.Foreground;
        _textBrush = Name.Foreground;
        _numericUpDownBrush = Sockets.Foreground;

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

    private void Name_OnTextChanged(object? _, TextChangedEventArgs e)
    {
        NameLabel.Foreground = _labelBrush;
        Name.Foreground = _textBrush;
    }

    private async void FolderBrowse_OnClick(object? _, RoutedEventArgs e)
    {
        var storageFolders = await StorageProvider.OpenFolderPickerAsync(UiHelpers.DefaultFolderPickerOpenOptions);
        if (storageFolders.Count != 1) return;

        Folder.Text = storageFolders[0].Path.LocalPath;
    }

    private void BootImage_OnTextChanged(object? _, TextChangedEventArgs e)
    {
        BootImageLabel.Foreground = _labelBrush;
        BootImage.Foreground = _textBrush;
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

    private void Sockets_OnValueChanged(object? _, NumericUpDownValueChangedEventArgs e)
    {
        SocketsLabel.Foreground = _labelBrush;
        Sockets.Foreground = _numericUpDownBrush;
    }

    private void Cores_OnValueChanged(object? _, NumericUpDownValueChangedEventArgs e)
    {
        CoresLabel.Foreground = _labelBrush;
        Cores.Foreground = _numericUpDownBrush;
    }

    private void Threads_OnValueChanged(object? _, NumericUpDownValueChangedEventArgs e)
    {
        ThreadsLabel.Foreground = _labelBrush;
        Threads.Foreground = _numericUpDownBrush;
    }

    private void Ram_OnValueChanged(object? _, NumericUpDownValueChangedEventArgs e)
    {
        RamLabel.Foreground = _labelBrush;
        Ram.Foreground = _numericUpDownBrush;
    }

    private void CreateNewDisk_OnIsCheckedChanged(object? _, RoutedEventArgs e)
    {
        if (!CreateNewDisk.IsChecked!.Value) return;
        NewDiskPanel.IsEnabled = true;
        ExistingDiskPanel.IsEnabled = false;
    }

    private void NewDiskCustomPath_OnTextChanged(object? _, TextChangedEventArgs e)
    {
        NewDiskCustomPathLabel.Foreground = _labelBrush;
        NewDiskCustomPath.Foreground = _textBrush;
    }

    private async void NewDiskCustomPathBrowse_OnClick(object? _, RoutedEventArgs e)
    {
        var storageFiles = await StorageProvider.OpenFilePickerAsync(UiHelpers.DefaultFilePickerOpenOptions);
        if (storageFiles.Count != 1) return;

        NewDiskCustomPath.Text = storageFiles[0].Path.LocalPath;
    }

    private void NewDiskFormat_OnSelectionChanged(object? _, SelectionChangedEventArgs e)
    {
        var format = (DiskFormat)NewDiskFormat.SelectedIndex;
        NewDiskCustomFormat.IsEnabled = format == DiskFormat.Custom;
    }

    private void NewDiskCustomFormat_OnTextChanged(object? _, TextChangedEventArgs e)
    {
        NewDiskCustomFormatLabel.Foreground = _labelBrush;
        NewDiskCustomFormat.Foreground = _textBrush;
    }

    private void UseExistingDisk_OnIsCheckedChanged(object? _, RoutedEventArgs e)
    {
        if (!UseExistingDisk.IsChecked!.Value) return;
        NewDiskPanel.IsEnabled = false;
        ExistingDiskPanel.IsEnabled = true;
    }

    private void ExistingDiskPath_OnTextChanged(object? _, TextChangedEventArgs e)
    {
        ExistingDiskPathLabel.Foreground = _labelBrush;
        ExistingDiskPath.Foreground = _textBrush;

        ExistingDiskType.Content = "N/A";
        ExistingDiskSize.Content = "N/A";
    }

    private async void ExistingDiskPathBrowse_OnClick(object? _, RoutedEventArgs e)
    {
        var storageFiles = await StorageProvider.OpenFilePickerAsync(UiHelpers.DefaultFilePickerOpenOptions);
        if (storageFiles.Count != 1) return;

        ExistingDiskPath.Text = storageFiles[0].Path.LocalPath;

        var vmDiskFolder = Path.GetDirectoryName(ExistingDiskPath.Text);
        if (string.IsNullOrEmpty(vmDiskFolder))
        {
            ExistingDiskPathLabel.Foreground = Brushes.Red;
            ExistingDiskPath.Foreground = Brushes.Red;

            Status.Content = "Invalid disk path. (Does it point to a root directory?)";
            return;
        }

        var (error, diskInfo) = DiskManager.GetDiskInfo(GlobalSettings.CustomQemuPath, vmDiskFolder, ExistingDiskPath.Text!);
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

        if (NewDiskFormat.ComboBox.Items[(int)diskInfo!.Format] is not ComboBoxItem item) throw new UnreachableException();

        ExistingDiskType.Tag = diskInfo; // This allows us to retrieve it later
        ExistingDiskType.Content = item.Content;
        ExistingDiskSize.Content = diskInfo.Size.ToString();

        Status.Content = "N/A";
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
            NameLabel.Foreground = Brushes.Red;
            Name.Foreground = Brushes.Red;

            Status.Content = "Please enter a valid name.";
            return;
        }

        if (Name.Text.Any(c => !char.IsAsciiLetterOrDigit(c) && c is not ' ' and not '.'))
        {
            NameLabel.Foreground = Brushes.Red;
            Name.Foreground = Brushes.Red;

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
                BootImageLabel.Foreground = Brushes.Red;
                BootImage.Foreground = Brushes.Red;

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
            var vmDiskFormat = (DiskFormat)NewDiskFormat.SelectedIndex;
            var vmDiskFormatString = vmDiskFormat.ToExtensionString(NewDiskCustomFormat.Text!);

            if (string.IsNullOrEmpty(vmDiskFormatString))
            {
                NewDiskCustomFormatLabel.Foreground = Brushes.Red;

                Status.Content = "Custom disk format is empty.";
                return;
            }

            if (vmDiskFormatString.Any(c => !char.IsAsciiLetterOrDigit(c) && c is not '-' and not '_'))
            {
                NewDiskCustomFormatLabel.Foreground = Brushes.Red;
                NewDiskCustomFormat.Foreground = Brushes.Red;

                Status.Content = "Custom disk format contains invalid characters.";
                return;
            }

            var vmDiskPath = string.IsNullOrEmpty(NewDiskCustomPath.Text)
                ? Path.Combine(Folder.Text!, Name.Text!, Path.ChangeExtension(Name.Text!, vmDiskFormatString))
                : NewDiskCustomPath.Text!;

            var vmDiskFolder = Path.GetDirectoryName(vmDiskPath);
            if (string.IsNullOrEmpty(vmDiskFolder))
            {
                NewDiskCustomPathLabel.Foreground = Brushes.Red;
                NewDiskCustomPath.Foreground = Brushes.Red;

                Status.Content = "Invalid disk path. (Does it point to a root directory?)";
                return;
            }

            Directory.CreateDirectory(vmDiskFolder);

            _currentProfile.Disks[0].Format = vmDiskFormat;
            _currentProfile.Disks[0].CustomFormat = NewDiskCustomFormat.Text!;
            _currentProfile.Disks[0].Path = vmDiskPath;

            var error = DiskManager.CreateDisk(GlobalSettings.CustomQemuPath, vmDiskFolder,
                vmDiskFormat, NewDiskCustomFormat.Text!, vmDiskPath,
                new ByteSize((ulong)NewDiskSize.Value!.Value, ByteSuffix.GiB), PreAllocateNewDisk.IsChecked!.Value);

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
                    // String sanitization should've already been handled above
                    case DiskManagerError.EmptyCustomDiskFormat:
                    case DiskManagerError.InvalidCustomDiskFormat: throw new UnreachableException();
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
                    // We don't get the output in CreateDisk()
                    case DiskManagerError.UnexpectedProcessOutput: throw new UnreachableException();
                    default: throw new UnreachableException();
                }
            }
        }
        else if (UseExistingDisk.IsChecked!.Value)
        {
            var vmDiskPath = ExistingDiskPath.Text!;
            if (!File.Exists(vmDiskPath))
            {
                ExistingDiskPathLabel.Foreground = Brushes.Red;
                ExistingDiskPath.Foreground = Brushes.Red;

                Status.Content = "Disk file does not exist.";
                return;
            }

            _currentProfile.Disks[0].Path = vmDiskPath;

            var vmDiskFolder = Path.GetDirectoryName(vmDiskPath);
            // We should've already checked that in the TextChanged event of ExistingDiskPath
            if (string.IsNullOrEmpty(vmDiskFolder)) throw new UnreachableException();

            if (ExistingDiskType.Tag is not DiskInfo diskInfo) throw new UnreachableException();

            _currentProfile.Disks[0].Format = diskInfo.Format;
            if (!string.IsNullOrEmpty(diskInfo.CustomFormat)) _currentProfile.Disks[0].CustomFormat = diskInfo.CustomFormat;
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

            SocketsLabel.Foreground = Brushes.Red;
            Sockets.Foreground = Brushes.Red;
        } else Sockets.Value = _currentProfile.Processor.Sockets;

        if (Cores.Maximum < _currentProfile.Processor.Cores)
        {
            status += notEnoughResources ? ", cores" : " cores";
            notEnoughResources = true;

            CoresLabel.Foreground = Brushes.Red;
            Cores.Foreground = Brushes.Red;
        } else Cores.Value = _currentProfile.Processor.Cores;

        if (Threads.Maximum < _currentProfile.Processor.Threads)
        {
            status += notEnoughResources ? ", threads" : " threads";
            notEnoughResources = true;

            ThreadsLabel.Foreground = Brushes.Red;
            Threads.Foreground = Brushes.Red;
        } else Threads.Value = _currentProfile.Processor.Threads;

        if (Ram.Maximum < _currentProfile.Ram)
        {
            status += notEnoughResources ? ", RAM" : " RAM";
            notEnoughResources = true;

            RamLabel.Foreground = Brushes.Red;
            Ram.Foreground = Brushes.Red;
        } else Ram.Value = _currentProfile.Ram;

        if (notEnoughResources) Status.Content = $"{status} for this profile.";
    }
}