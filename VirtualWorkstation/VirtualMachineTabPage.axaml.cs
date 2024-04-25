using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MsBox.Avalonia;
using QemuSharp;
using QemuSharp.Structs;
using Tomlyn;

namespace VirtualWorkstation;

public partial class VirtualMachineTabPage : UserControl, ITabPage
{
    private readonly MainWindow _parent;

    public bool Opened { get; set; }

    public int VmTabIndex { get; set; }

    public int Index { get; set; }

    public string VmPath { get; }

    public VirtualMachine Vm { get; private set; }

    public VirtualMachineTabPage(MainWindow parent, string path, ref VirtualMachine vm)
    {
        InitializeComponent();

        _parent = parent;
        VmPath = path;
        Vm = vm;

        RefreshArguments_OnClick(null, null!);
    }

    private async void Run_OnClick(object? _, RoutedEventArgs e)
    {
        var (errors, qemuPath, arguments) = Launcher.GetArguments(Vm,
            GlobalSettings.CustomQemuPath, false);

        if (errors.Count != 0)
        {
            var text = GetErrorsText(ref errors);
            var box = MessageBoxManager.GetMessageBoxStandard("Virtual Machine Errors", text);

            await box.ShowAsync();
            return;
        }

        Process.Start(new ProcessStartInfo(qemuPath!, arguments)
        {
            WorkingDirectory = Path.GetDirectoryName(VmPath)!
        });
    }

    private async void Settings_OnClick(object? _, RoutedEventArgs e)
    {
        // Create a full copy of the VM so that, in case we don't save our changes, they won't affect the main VM class
        var newVm = new VirtualMachine(Vm);

        var vmSettings = new VirtualMachineSettings(ref newVm);
        await vmSettings.ShowDialog(_parent);

        if (!vmSettings.HasSaved) return;
        if (_parent.VmTabs.Items[VmTabIndex] is not TabItem tabItem) throw new UnreachableException();
        if (_parent.VmList.Items[Index] is not ListBoxItem listItem) throw new UnreachableException();

        tabItem.Header = newVm.Name;
        listItem.Content = newVm.Name;

        var vmConfigFile = Toml.FromModel(newVm);
        await File.WriteAllTextAsync(VmPath, vmConfigFile);

        Vm = newVm;
    }

    private void RefreshArguments_OnClick(object? _, RoutedEventArgs e)
    {
        var (_, qemuPath, arguments) = Launcher.GetArguments(Vm, GlobalSettings.CustomQemuPath, true);
        if (!string.IsNullOrEmpty(qemuPath)) arguments.Insert(0, qemuPath);

        var sb = new StringBuilder();
        for (var i = 0; i < arguments.Count; i++)
        {
            var argument = arguments[i];
            sb.Append(argument);

            if (i == arguments.Count - 1) continue;
            sb.AppendLine(" \\");
            sb.Append('\t');
        }

        Arguments.Text = sb.ToString();
    }

    private string GetErrorsText(ref List<LauncherError> errors)
    {
        var text = new StringBuilder();

        text.AppendLine("The following errors have occured while trying to run the virtual machine:\n");
        foreach (var (errorType, index, secondIndex) in errors)
        {
            switch (errorType)
            {
                case LauncherErrorType.EmptyName:
                {
                    text.AppendLine(" * Empty name.");
                    break;
                }
                case LauncherErrorType.EmptyFirmwarePath:
                {
                    text.AppendLine(" * Empty auto-detected firmware path.");
                    break;
                }
                case LauncherErrorType.EmptyCustomFirmwarePath:
                {
                    text.AppendLine(" * Empty custom firmware path.");
                    break;
                }
                case LauncherErrorType.EmptyCustomChipsetModel:
                {
                    text.AppendLine(" * Empty custom chipset model.");
                    break;
                }
                case LauncherErrorType.EmptyCustomDisplayType:
                {
                    text.AppendLine(" * Empty custom display type.");
                    break;
                }
                case LauncherErrorType.EmptyCustomAudioHostDeviceType:
                {
                    text.AppendLine(" * Empty custom audio host device type.");
                    break;
                }
                case LauncherErrorType.EmptyCustomProcessorModel:
                {
                    text.AppendLine(" * Empty custom processor model.");
                    break;
                }
                case LauncherErrorType.EmptyCustomUsbControllerVersion:
                {
                    text.AppendLine($" * Empty custom USB controller {index} version.");
                    break;
                }
                case LauncherErrorType.EmptyCustomNetworkInterfaceType:
                {
                    text.AppendLine($" * Empty custom network interface {index} type.");
                    break;
                }
                case LauncherErrorType.EmptyCustomNetworkInterfaceCard:
                {
                    text.AppendLine($" * Empty custom network interface {index} card.");
                    break;
                }
                case LauncherErrorType.EmptyCustomGraphicsControllerCard:
                {
                    text.AppendLine($" * Empty custom graphics controller {index} card.");
                    break;
                }
                case LauncherErrorType.EmptyCustomAudioControllerCard:
                {
                    text.AppendLine($" * Empty custom audio controller {index} card.");
                    break;
                }
                case LauncherErrorType.EmptyCustomDiskControllerModel:
                {
                    text.AppendLine($" * Empty custom disk controller {index} model.");
                    break;
                }
                case LauncherErrorType.EmptyCustomDiskFormat:
                {
                    text.AppendLine($" * Empty custom disk {index} format.");
                    break;
                }
                case LauncherErrorType.EmptyCustomDiskCacheMethod:
                {
                    text.AppendLine($" * Empty custom disk {index} cache method.");
                    break;
                }
                case LauncherErrorType.EmptyDiskPath:
                {
                    text.AppendLine($" * Empty disk {index} path.");
                    break;
                }
                case LauncherErrorType.EmptyQemuPath:
                {
                    text.AppendLine(" * Empty QEMU path.");
                    break;
                }
                case LauncherErrorType.EmptyCustomQemuArgumentValue:
                {
                    text.AppendLine($" * Empty custom QEMU argument {index} value.");
                    break;
                }
                case LauncherErrorType.EmptyCustomQemuArgumentParameterKey:
                {
                    text.AppendLine($" * Empty custom QEMU argument {index} parameter {secondIndex} key.");
                    break;
                }
                case LauncherErrorType.EmptyCustomQemuArgumentParameterValue:
                {
                    text.AppendLine($" * Empty custom QEMU argument {index} parameter {secondIndex} value.");
                    break;
                }
                case LauncherErrorType.InvalidName:
                {
                    text.AppendLine(" * Name contains invalid characters.");
                    break;
                }
                case LauncherErrorType.InvalidCustomChipsetModel:
                {
                    text.AppendLine(" * Custom chipset model contains invalid characters.");
                    break;
                }
                case LauncherErrorType.InvalidCustomDisplayType:
                {
                    text.AppendLine(" * Custom display type contains invalid characters.");
                    break;
                }
                case LauncherErrorType.InvalidCustomAudioHostDeviceType:
                {
                    text.AppendLine(" * Custom audio host device type contains invalid characters.");
                    break;
                }
                case LauncherErrorType.InvalidCustomProcessorModel:
                {
                    text.AppendLine(" * Custom processor model contains invalid characters.");
                    break;
                }
                case LauncherErrorType.InvalidCustomUsbControllerVersion:
                {
                    text.AppendLine($" * Custom USB controller {index} version contains invalid characters.");
                    break;
                }
                case LauncherErrorType.InvalidCustomNetworkInterfaceType:
                {
                    text.AppendLine($" * Custom network interface {index} type contains invalid characters.");
                    break;
                }
                case LauncherErrorType.InvalidCustomNetworkInterfaceCard:
                {
                    text.AppendLine($" * Custom network interface {index} card contains invalid characters.");
                    break;
                }
                case LauncherErrorType.InvalidCustomGraphicsControllerCard:
                {
                    text.AppendLine($" * Custom graphics controller {index} card contains invalid characters.");
                    break;
                }
                case LauncherErrorType.InvalidCustomAudioControllerCard:
                {
                    text.AppendLine($" * Custom audio controller {index} card contains invalid characters.");
                    break;
                }
                case LauncherErrorType.InvalidCustomDiskControllerModel:
                {
                    text.AppendLine($" * Custom disk controller {index} model contains invalid characters.");
                    break;
                }
                case LauncherErrorType.InvalidCustomDiskFormat:
                {
                    text.AppendLine($" * Custom disk {index} format contains invalid characters.");
                    break;
                }
                case LauncherErrorType.InvalidCustomDiskCacheMethod:
                {
                    text.AppendLine($" * Custom disk {index} cache method contains invalid characters.");
                    break;
                }
                case LauncherErrorType.InvalidCustomQemuArgumentValue:
                {
                    text.AppendLine($" * Custom QEMU argument {index} value contains invalid characters.");
                    break;
                }
                case LauncherErrorType.InvalidCustomQemuArgumentParameterKey:
                {
                    text.AppendLine($" * Custom QEMU argument {index} parameter {secondIndex} key contains invalid characters.");
                    break;
                }
                case LauncherErrorType.InvalidCustomQemuArgumentParameterValue:
                {
                    text.AppendLine($" * Custom QEMU argument {index} parameter {secondIndex} value contains invalid characters.");
                    break;
                }
                case LauncherErrorType.CustomFirmwareDoesNotExist:
                {
                    text.AppendLine(" * Custom firmware file does not exist.");
                    break;
                }
                case LauncherErrorType.DiskDoesNotExist:
                {
                    text.AppendLine($" * Disk {index} file does not exist.");
                    break;
                }
                case LauncherErrorType.QemuSystemDoesNotExist:
                {
                    text.AppendLine(" * QEMU system executable file does not exist.");
                    break;
                }
                case LauncherErrorType.HardwareAccelerationUnavailable:
                {
                    text.AppendLine(" * Unavailable hardware acceleration on host OS.");
                    break;
                }
                case LauncherErrorType.AudioUnavailable:
                {
                    text.AppendLine(" * No audio host device available for host OS.");
                    break;
                }
                case LauncherErrorType.InvalidFirmwareTypeForArchitecture:
                {
                    text.AppendLine($" * Invalid firmware type \"{Vm.Firmware.Type}\" for architecture \"{Vm.Architecture}\".");
                    break;
                }
                case LauncherErrorType.InvalidChipsetModelForArchitecture:
                {
                    text.AppendLine($" * Invalid chipset model \"{Vm.Chipset.Model}\" for architecture \"{Vm.Architecture}\".");
                    break;
                }
                case LauncherErrorType.InvalidProcessorModelForArchitecture:
                {
                    text.AppendLine($" * Invalid processor model \"{Vm.Processor.Model}\" for architecture \"{Vm.Architecture}\".");
                    break;
                }
                case LauncherErrorType.InvalidAddedFeatureForArchitecture:
                {
                    text.AppendLine($" * Invalid added processor feature \"{Vm.Processor.AddFeatures[index]}\" for architecture \"{Vm.Architecture}\".");
                    break;
                }
                case LauncherErrorType.InvalidRemovedFeatureForArchitecture:
                {
                    text.AppendLine($" * Invalid removed processor feature \"{Vm.Processor.RemoveFeatures[index]}\" for architecture \"{Vm.Architecture}\".");
                    break;
                }
                case LauncherErrorType.InvalidForcePciOptionForChipsetModel:
                {
                    text.AppendLine($" * Invalid force use normal PCI option for chipset model \"{Vm.Chipset.Model}\".");
                    break;
                }
                case LauncherErrorType.UnsupportedDisplay:
                {
                    text.AppendLine($" * Unsupported display \"{Vm.Display.Type}\" for host OS.");
                    break;
                }
                case LauncherErrorType.UnsupportedAudioHostDevice:
                {
                    text.AppendLine($" * Unsupported audio host device \"{Vm.AudioHostDevice.Type}\" for host OS.");
                    break;
                }
                case LauncherErrorType.NoUsbControllersForNetworkCard:
                {
                    text.AppendLine($" * No USB controllers available for network interface \"{index}\".");
                    break;
                }
                case LauncherErrorType.NoUsbControllersForSoundCard:
                {
                    text.AppendLine($" * No USB controllers available for audio controller \"{index}\".");
                    break;
                }
                case LauncherErrorType.NoUsbControllersForDiskBus:
                {
                    text.AppendLine($" * No USB controllers available for disk controller \"{index}\".");
                    break;
                }
                case LauncherErrorType.NoUsbControllersForKeyboard:
                {
                    text.AppendLine($" * No USB controllers available for keyboard \"{index}\".");
                    break;
                }
                case LauncherErrorType.NoUsbControllersForMouse:
                {
                    text.AppendLine($" * No USB controllers available for mouse \"{index}\".");
                    break;
                }
                case LauncherErrorType.InvalidUsbControllerForNetworkCard:
                {
                    text.AppendLine($" * Invalid USB controller for network interface \"{index}\".");
                    break;
                }
                case LauncherErrorType.InvalidUsbControllerForSoundCard:
                {
                    text.AppendLine($" * Invalid USB controller for audio controller \"{index}\".");
                    break;
                }
                case LauncherErrorType.InvalidUsbControllerForDiskBus:
                {
                    text.AppendLine($" * Invalid USB controller for disk controller \"{index}\".");
                    break;
                }
                case LauncherErrorType.InvalidUsbControllerForKeyboard:
                {
                    text.AppendLine($" * Invalid USB controller for keyboard \"{index}\".");
                    break;
                }
                case LauncherErrorType.InvalidUsbControllerForMouse:
                {
                    text.AppendLine($" * Invalid USB controller for mouse \"{index}\".");
                    break;
                }
                case LauncherErrorType.InvalidVgaEmulationOptionForGraphicsCard:
                {
                    text.AppendLine($" * Invalid VGA emulation option for graphics controller \"{index}\".");
                    break;
                }
                case LauncherErrorType.InvalidGraphicsAccelerationOptionForGraphicsCard:
                {
                    text.AppendLine($" * Invalid graphics acceleration option for graphics controller \"{index}\".");
                    break;
                }
                case LauncherErrorType.InvalidInputOptionForSoundCard:
                {
                    text.AppendLine($" * Invalid input option for audio controller \"{index}\".");
                    break;
                }
                case LauncherErrorType.InvalidOutputOptionForSoundCard:
                {
                    text.AppendLine($" * Invalid output option for audio controller \"{index}\".");
                    break;
                }
                case LauncherErrorType.InvalidInputOutputOptionsForSoundCard:
                {
                    text.AppendLine($" * Invalid input/output option combination for audio controller \"{index}\".");
                    break;
                }
                case LauncherErrorType.UnusedDiskController:
                {
                    text.AppendLine($" * Unused disk controller \"{index}\".");
                    break;
                }
                case LauncherErrorType.InvalidDiskControllerForDisk:
                {
                    text.AppendLine($" * Invalid disk controller for disk \"{index}\".");
                    break;
                }
                case LauncherErrorType.InvalidCdromOptionForDiskBus:
                {
                    text.AppendLine($" * Invalid CD-ROM option for disk controller \"{index}\".");
                    break;
                }
                case LauncherErrorType.InvalidRemovableOptionForDiskBus:
                {
                    text.AppendLine($" * Invalid removable option for disk controller \"{index}\".");
                    break;
                }
                default: throw new UnreachableException();
            }
        }
        text.AppendLine("\nPlease correct all aforementioned components before proceeding again.");

        return text.ToString();
    }
}