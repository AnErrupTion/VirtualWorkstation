using System.Diagnostics;
using System.Text;
using QemuSharp.Structs;
using QemuSharp.Structs.Enums;

namespace QemuSharp;

public static class Launcher
{
    public static LauncherResult GetArguments(VirtualMachine vm, string customQemuPath, bool addQuotes)
    {
        var name = SanitizeName(vm.Name);
        var quotedName = addQuotes && name.Contains(' ') ? $"\"{name}\"" : name;
        var arguments = new List<string> { "-nodefaults" };
        var programs = new List<Program>();
        var errors = new List<LauncherError>();

        if (string.IsNullOrEmpty(name)) errors.Add(new LauncherError(LauncherErrorType.EmptyName));
        if (name.Length != vm.Name.Length) errors.Add(new LauncherError(LauncherErrorType.InvalidName));

        arguments.Add("-name");
        arguments.Add($"{quotedName},process={quotedName}");

        arguments.Add("-accel");
        if (vm.UseHardwareAcceleration)
        {
            if (OperatingSystem.IsWindows()) arguments.Add("whpx");
            else if (OperatingSystem.IsMacOS()) arguments.Add("hvf");
            else if (OperatingSystem.IsLinux()) arguments.Add("kvm");
            else errors.Add(new LauncherError(LauncherErrorType.HardwareAccelerationUnavailable));
        } else arguments.Add("tcg");

        string? nvRamPath = null;
        switch (vm.Firmware.Type)
        {
            case FirmwareType.Efi:
            {
                var path = PathLookup.LookupFiles(PathLookup.EfiPaths, PathLookup.EfiFiles);
                var quotedPath = addQuotes && path.Contains(' ') ? $"\"{path}\"" : path;

                if (string.IsNullOrEmpty(path)) errors.Add(new LauncherError(LauncherErrorType.EmptyFirmwarePath));

                nvRamPath = PathLookup.LookupFiles(PathLookup.EfiPaths, PathLookup.EfiNvramFiles);
                if (string.IsNullOrEmpty(nvRamPath)) errors.Add(new LauncherError(LauncherErrorType.EmptyEfiNvRamPath));

                arguments.Add("-drive");
                arguments.Add($"if=pflash,readonly=on,file={quotedPath}");

                arguments.Add("-drive");
                arguments.Add($"if=pflash,format=raw,file={Path.GetFileName(nvRamPath)}");
                break;
            }
            case FirmwareType.EfiSecureBoot:
            {
                var path = PathLookup.LookupFiles(PathLookup.EfiPaths, PathLookup.EfiSecureBootFiles);
                var quotedPath = addQuotes && path.Contains(' ') ? $"\"{path}\"" : path;

                if (string.IsNullOrEmpty(path)) errors.Add(new LauncherError(LauncherErrorType.EmptyCustomEfiSecureBootFirmwarePath));

                nvRamPath = PathLookup.LookupFiles(PathLookup.EfiPaths, PathLookup.EfiSecureBootNvramFiles);
                if (string.IsNullOrEmpty(nvRamPath)) errors.Add(new LauncherError(LauncherErrorType.EmptyCustomEfiSecureBootNvRamPath));

                arguments.Add("-drive");
                arguments.Add($"if=pflash,readonly=on,file={quotedPath}");

                arguments.Add("-drive");
                arguments.Add($"if=pflash,format=raw,file={Path.GetFileName(nvRamPath)}");
                break;
            }
            case FirmwareType.CustomPFlash:
            {
                var path = vm.Firmware.CustomPath;
                var quotedPath = addQuotes && path.Contains(' ') ? $"\"{path}\"" : path;

                if (string.IsNullOrEmpty(path))
                    errors.Add(new LauncherError(LauncherErrorType.EmptyCustomFirmwarePath));
                else if (!File.Exists(path))
                    errors.Add(new LauncherError(LauncherErrorType.CustomFirmwareDoesNotExist));

                nvRamPath = vm.Firmware.CustomNvRamPath;

                if (string.IsNullOrEmpty(nvRamPath))
                    errors.Add(new LauncherError(LauncherErrorType.EmptyCustomEfiNvRamPath));
                else if (!File.Exists(nvRamPath))
                    errors.Add(new LauncherError(LauncherErrorType.CustomEfiNvRamDoesNotExist));

                arguments.Add("-drive");
                arguments.Add($"if=pflash,readonly=on,file={quotedPath}");
                
                var quotedNvRamPath = addQuotes && nvRamPath.Contains(' ') ? $"\"{nvRamPath}\"" : nvRamPath;

                arguments.Add("-drive");
                arguments.Add($"if=pflash,format=raw,file={quotedNvRamPath}");
                break;
            }
            case FirmwareType.X86LegacyBios:
            {
                if (vm.Architecture is not Architecture.Amd64 and not Architecture.I386)
                    errors.Add(new LauncherError(LauncherErrorType.InvalidFirmwareTypeForArchitecture));

                var path = PathLookup.LookupFiles(PathLookup.BiosPaths, PathLookup.BiosFiles);
                var quotedPath = addQuotes && path.Contains(' ') ? $"\"{path}\"" : path;

                if (string.IsNullOrEmpty(path)) errors.Add(new LauncherError(LauncherErrorType.EmptyFirmwarePath));

                arguments.Add("-bios");
                arguments.Add(quotedPath);
                break;
            }
            case FirmwareType.X86CustomPc:
            {
                if (vm.Architecture is not Architecture.Amd64 and not Architecture.I386)
                    errors.Add(new LauncherError(LauncherErrorType.InvalidFirmwareTypeForArchitecture));

                var path = vm.Firmware.CustomPath;
                var quotedPath = addQuotes && path.Contains(' ') ? $"\"{path}\"" : path;

                if (string.IsNullOrEmpty(path))
                    errors.Add(new LauncherError(LauncherErrorType.EmptyCustomFirmwarePath));
                else if (!File.Exists(path))
                    errors.Add(new LauncherError(LauncherErrorType.CustomFirmwareDoesNotExist));

                arguments.Add("-bios");
                arguments.Add(quotedPath);
                break;
            }
            default: throw new UnreachableException();
        }

        string pciBusType;

        arguments.Add("-machine");
        switch (vm.Chipset.Model)
        {
            case ChipsetModel.X86Q35:
            {
                if (vm.Architecture is not Architecture.Amd64 and not Architecture.I386)
                    errors.Add(new LauncherError(LauncherErrorType.InvalidChipsetModelForArchitecture));

                if (vm.Chipset.ForceUseNormalPci)
                    errors.Add(new LauncherError(LauncherErrorType.InvalidForcePciOptionForChipsetModel));

                var chipset = new StringBuilder("q35");
                if (vm.Chipset.Q35Options != null)
                    chipset.Append($",i8042={(vm.Chipset.Q35Options.EnablePs2Emulation ? "on" : "off")}");

                pciBusType = "pcie";
                arguments.Add(chipset.ToString());
                break;
            }
            case ChipsetModel.X86I440Fx:
            {
                if (vm.Architecture is not Architecture.Amd64 and not Architecture.I386)
                    errors.Add(new LauncherError(LauncherErrorType.InvalidChipsetModelForArchitecture));

                if (!vm.Chipset.ForceUseNormalPci)
                    errors.Add(new LauncherError(LauncherErrorType.InvalidForcePciOptionForChipsetModel));

                var chipset = new StringBuilder("pc");
                if (vm.Chipset.I440FxOptions != null)
                    chipset.Append($",i8042={(vm.Chipset.I440FxOptions.EnablePs2Emulation ? "on" : "off")}");

                pciBusType = "pci";
                arguments.Add(chipset.ToString());
                break;
            }
            case ChipsetModel.Custom:
            {
                var model = SanitizeQemuArgumentStringWithOptions(vm.Chipset.CustomModel);

                if (string.IsNullOrEmpty(model))
                    errors.Add(new LauncherError(LauncherErrorType.EmptyCustomChipsetModel));

                if (model.Length != vm.Chipset.CustomModel.Length)
                    errors.Add(new LauncherError(LauncherErrorType.InvalidCustomChipsetModel));

                pciBusType = vm.Chipset.ForceUseNormalPci ? "pci" : "pcie";
                arguments.Add(model);
                break;
            }
            default: throw new UnreachableException();
        }

        arguments.Add("-m");
        arguments.Add($"{vm.Ram}M");

        // FIXME: Should we add the option to configure display graphics acceleration?
        // Generally, it shouldn't cause any problems (and should even be faster), but
        // apparently this could cause issues with some virtual graphics cards and their
        // drivers. (For example, absolute cursor position issues with the VirtIO GPU
        // driver on Windows)
        arguments.Add("-display");
        switch (vm.Display.Type)
        {
            case DisplayType.None:
            {
                arguments.Add("none");
                break;
            }
            case DisplayType.Auto:
            {
                if (OperatingSystem.IsMacOS()) goto case DisplayType.Cocoa;
                goto case DisplayType.Sdl;
            }
            case DisplayType.Sdl:
            {
                arguments.Add("sdl,gl=on");
                break;
            }
            case DisplayType.Gtk:
            {
                if (OperatingSystem.IsMacOS()) errors.Add(new LauncherError(LauncherErrorType.UnsupportedDisplay));

                arguments.Add("gtk,gl=on");
                break;
            }
            case DisplayType.Spice:
            {
                arguments.Add("spice-app,gl=on");
                break;
            }
            case DisplayType.Cocoa:
            {
                if (!OperatingSystem.IsMacOS()) errors.Add(new LauncherError(LauncherErrorType.UnsupportedDisplay));

                arguments.Add("cocoa");
                break;
            }
            case DisplayType.DBus:
            {
                if (!OperatingSystem.IsLinux() && !OperatingSystem.IsFreeBSD())
                    errors.Add(new LauncherError(LauncherErrorType.UnsupportedDisplay));

                arguments.Add("dbus,gl=on");
                break;
            }
            case DisplayType.Custom:
            {
                var type = SanitizeQemuArgumentStringWithOptions(vm.Display.CustomType);

                if (string.IsNullOrEmpty(type))
                    errors.Add(new LauncherError(LauncherErrorType.EmptyCustomDisplayType));

                if (type.Length != vm.Display.CustomType.Length)
                    errors.Add(new LauncherError(LauncherErrorType.InvalidCustomDisplayType));

                arguments.Add(type);
                break;
            }
            default: throw new UnreachableException();
        }

        arguments.Add("-audiodev");
        switch (vm.AudioHostDevice.Type)
        {
            case AudioHostType.None:
            {
                arguments.Add("none,id=audiodev");
                break;
            }
            case AudioHostType.Auto:
            {
                if (OperatingSystem.IsWindows()) goto case AudioHostType.DirectSound;
                if (OperatingSystem.IsMacOS()) goto case AudioHostType.CoreAudio;
                if (OperatingSystem.IsLinux()) goto case AudioHostType.PipeWire; // FIXME: Is it safe to assume PipeWire is the standard now?
                if (OperatingSystem.IsFreeBSD()) goto case AudioHostType.SndIo;

                errors.Add(new LauncherError(LauncherErrorType.AudioUnavailable));
                break;
            }
            case AudioHostType.Sdl:
            {
                arguments.Add("sdl,id=audiodev");
                break;
            }
            case AudioHostType.Alsa:
            {
                if (!OperatingSystem.IsLinux()) errors.Add(new LauncherError(LauncherErrorType.UnsupportedAudioHostDevice));
                
                arguments.Add("alsa,id=audiodev");
                break;
            }
            case AudioHostType.Oss:
            {
                if (!OperatingSystem.IsLinux() && !OperatingSystem.IsFreeBSD()) errors.Add(new LauncherError(LauncherErrorType.UnsupportedAudioHostDevice));

                arguments.Add("oss,id=audiodev");
                break;
            }
            case AudioHostType.PulseAudio:
            {
                if (!OperatingSystem.IsLinux() && !OperatingSystem.IsFreeBSD()) errors.Add(new LauncherError(LauncherErrorType.UnsupportedAudioHostDevice));

                arguments.Add("pa,id=audiodev");
                break;
            }
            case AudioHostType.PipeWire:
            {
                if (!OperatingSystem.IsLinux() && !OperatingSystem.IsFreeBSD()) errors.Add(new LauncherError(LauncherErrorType.UnsupportedAudioHostDevice));

                arguments.Add("pipewire,id=audiodev");
                break;
            }
            case AudioHostType.SndIo:
            {
                if (!OperatingSystem.IsFreeBSD() && !OperatingSystem.IsLinux()) errors.Add(new LauncherError(LauncherErrorType.UnsupportedAudioHostDevice));

                arguments.Add("sndio,id=audiodev");
                break;
            }
            case AudioHostType.CoreAudio:
            {
                if (!OperatingSystem.IsMacOS()) errors.Add(new LauncherError(LauncherErrorType.UnsupportedAudioHostDevice));

                arguments.Add("coreaudio,id=audiodev");
                break;
            }
            case AudioHostType.DirectSound:
            {
                if (!OperatingSystem.IsWindows()) errors.Add(new LauncherError(LauncherErrorType.UnsupportedAudioHostDevice));

                arguments.Add("dsound,id=audiodev");
                break;
            }
            case AudioHostType.Wav:
            {
                arguments.Add("wav,id=audiodev");
                break;
            }
            case AudioHostType.Custom:
            {
                var type = SanitizeQemuArgumentStringWithOptions(vm.AudioHostDevice.CustomType);

                if (string.IsNullOrEmpty(type))
                    errors.Add(new LauncherError(LauncherErrorType.EmptyCustomAudioHostDeviceType));

                if (type.Length != vm.AudioHostDevice.CustomType.Length)
                    errors.Add(new LauncherError(LauncherErrorType.InvalidCustomAudioHostDeviceType));

                arguments.Add($"{type},id=audiodev");
                break;
            }
            default: throw new UnreachableException();
        }

        arguments.Add("-cpu");
        var cpuModel = new StringBuilder();

        switch (vm.Processor.Model)
        {
            case ProcessorModel.X86Host:
            {
                if (vm.Architecture is not Architecture.Amd64 and not Architecture.I386)
                    errors.Add(new LauncherError(LauncherErrorType.InvalidProcessorModelForArchitecture));

                cpuModel.Append("host");
                break;
            }
            case ProcessorModel.X86Max:
            {
                if (vm.Architecture is not Architecture.Amd64 and not Architecture.I386)
                    errors.Add(new LauncherError(LauncherErrorType.InvalidProcessorModelForArchitecture));

                cpuModel.Append("max");
                break;
            }
            case ProcessorModel.Custom:
            {
                var model = SanitizeQemuArgumentStringWithOptions(vm.Processor.CustomModel);

                if (string.IsNullOrEmpty(model))
                    errors.Add(new LauncherError(LauncherErrorType.EmptyCustomProcessorModel));

                if (model.Length != vm.Processor.CustomModel.Length)
                    errors.Add(new LauncherError(LauncherErrorType.InvalidCustomProcessorModel));

                cpuModel.Append(model);
                break;
            }
            default: throw new UnreachableException();
        }

        for (var i = 0; i < vm.Processor.AddFeatures.Count; i++)
        {
            var feature = vm.Processor.AddFeatures[i];
            if (IsFeatureUnsupported(vm.Architecture, feature))
                errors.Add(new LauncherError(LauncherErrorType.InvalidAddedFeatureForArchitecture, i));

            cpuModel.Append(",+");
            cpuModel.Append(ProcessorFeatureToString(feature));
        }

        for (var i = 0; i < vm.Processor.RemoveFeatures.Count; i++)
        {
            var feature = vm.Processor.RemoveFeatures[i];
            if (IsFeatureUnsupported(vm.Architecture, feature))
                errors.Add(new LauncherError(LauncherErrorType.InvalidRemovedFeatureForArchitecture, i));

            cpuModel.Append(",-");
            cpuModel.Append(ProcessorFeatureToString(feature));
        }

        arguments.Add(cpuModel.ToString());

        arguments.Add("-smp");
        arguments.Add($"sockets={vm.Processor.Sockets},cores={vm.Processor.Cores},threads={vm.Processor.Threads}");

        if (vm.TrustedPlatformModule.DeviceType != TpmDeviceType.None)
        {
            arguments.Add("-tpmdev");
            switch (vm.TrustedPlatformModule.DeviceType)
            {
                case TpmDeviceType.Emulated:
                {
                    arguments.Add("emulator,chardev=chartpm,id=tpmdev");

                    var socketPath = Path.Combine(Path.GetTempPath(), $"swtpm-sock-{vm.Name}");
                    var quotedSocketPath = addQuotes && socketPath.Contains(' ') ? $"\"{socketPath}\"" : socketPath;

                    arguments.Add("-chardev");
                    arguments.Add($"socket,id=chartpm,path={quotedSocketPath}");
                    break;
                }
                case TpmDeviceType.Custom:
                {
                    var deviceType = SanitizeQemuArgumentStringWithOptions(vm.TrustedPlatformModule.CustomDeviceType);

                    if (string.IsNullOrEmpty(deviceType))
                        errors.Add(new LauncherError(LauncherErrorType.EmptyCustomTpmDeviceType));

                    if (deviceType.Length != vm.TrustedPlatformModule.CustomDeviceType.Length)
                        errors.Add(new LauncherError(LauncherErrorType.InvalidCustomTpmDeviceType));

                    arguments.Add($"{deviceType},id=tpmdev");
                    break;
                }
                default: throw new UnreachableException();
            }

            arguments.Add("-device");
            switch (vm.TrustedPlatformModule.Type)
            {
                case TpmType.Tis:
                {
                    arguments.Add("tpm-tis,bus=isa.0,tpmdev=tpmdev");
                    break;
                }
                case TpmType.Crb:
                {
                    arguments.Add("tpm-crb,tpmdev=tpmdev");
                    break;
                }
                case TpmType.Custom:
                {
                    var type = SanitizeQemuArgumentStringWithOptions(vm.TrustedPlatformModule.CustomType);

                    if (string.IsNullOrEmpty(type))
                        errors.Add(new LauncherError(LauncherErrorType.EmptyCustomTpmType));

                    if (type.Length != vm.TrustedPlatformModule.CustomType.Length)
                        errors.Add(new LauncherError(LauncherErrorType.InvalidCustomTpmType));

                    var busType = GetCustomBusType(vm.TrustedPlatformModule.CustomTypeBus, pciBusType);
                    arguments.Add($"{type}{busType},tpmdev=tpmdev");
                    break;
                }
                default: throw new UnreachableException();
            }
        }

        for (var i = 0; i < vm.UsbControllers.Count; i++)
        {
            var usbController = vm.UsbControllers[i];

            arguments.Add("-device");
            switch (usbController.Version)
            {
                case UsbVersion.Ohci:
                {
                    arguments.Add($"pci-ohci,bus={pciBusType}.0,id=usb{i}");
                    break;
                }
                case UsbVersion.Uhci:
                {
                    arguments.Add($"piix3-usb-uhci,bus={pciBusType}.0,id=usb{i}");
                    break;
                }
                case UsbVersion.Ehci:
                {
                    arguments.Add($"usb-ehci,bus={pciBusType}.0,id=usb{i}");
                    break;
                }
                case UsbVersion.Xhci:
                {
                    arguments.Add($"qemu-xhci,bus={pciBusType}.0,id=usb{i}");
                    break;
                }
                case UsbVersion.Custom:
                {
                    var model = SanitizeQemuArgumentStringWithOptions(usbController.CustomVersion);

                    if (string.IsNullOrEmpty(model))
                        errors.Add(new LauncherError(LauncherErrorType.EmptyCustomUsbControllerVersion, i));

                    if (model.Length != usbController.CustomVersion.Length)
                        errors.Add(new LauncherError(LauncherErrorType.InvalidCustomUsbControllerVersion, i));

                    var busType = GetCustomBusType(usbController.CustomVersionBus, pciBusType);
                    arguments.Add($"{model}{busType},id=usb{i}");
                    break;
                }
                default: throw new UnreachableException();
            }
        }

        for (var i = 0; i < vm.NetworkInterfaces.Count; i++)
        {
            var networkInterface = vm.NetworkInterfaces[i];

            arguments.Add("-netdev");
            switch (networkInterface.Type)
            {
                case NetworkType.Nat:
                {
                    arguments.Add($"user,id=network{i}");
                    break;
                }
                case NetworkType.Custom:
                {
                    var type = SanitizeQemuArgumentStringWithOptions(networkInterface.CustomType);

                    if (string.IsNullOrEmpty(type))
                        errors.Add(new LauncherError(LauncherErrorType.EmptyCustomNetworkInterfaceType, i));

                    if (type.Length != networkInterface.CustomType.Length)
                        errors.Add(new LauncherError(LauncherErrorType.InvalidCustomNetworkInterfaceType, i));

                    arguments.Add($"{type},id=network{i}");
                    break;
                }
                default: throw new UnreachableException();
            }

            arguments.Add("-device");
            switch (networkInterface.Card)
            {
                case NetworkCard.Rtl8139:
                {
                    arguments.Add($"rtl8139,bus={pciBusType}.0,netdev=network{i}");
                    break;
                }
                case NetworkCard.E1000:
                {
                    arguments.Add($"e1000,bus={pciBusType}.0,netdev=network{i}");
                    break;
                }
                case NetworkCard.E1000E:
                {
                    arguments.Add($"e1000e,bus={pciBusType}.0,netdev=network{i}");
                    break;
                }
                case NetworkCard.VMware:
                {
                    arguments.Add($"vmxnet3,bus={pciBusType}.0,netdev=network{i}");
                    break;
                }
                case NetworkCard.Usb:
                {
                    if (vm.UsbControllers.Count == 0)
                        errors.Add(new LauncherError(LauncherErrorType.NoUsbControllersForNetworkCard, i));

                    if (networkInterface.UsbController >= (ulong)vm.UsbControllers.Count)
                        errors.Add(new LauncherError(LauncherErrorType.InvalidUsbControllerForNetworkCard, i));

                    arguments.Add($"usb-net,bus=usb{networkInterface.UsbController}.0,netdev=network{i}");
                    break;
                }
                case NetworkCard.VirtIo:
                {
                    arguments.Add($"virtio-net-pci,bus={pciBusType}.0,netdev=network{i}");
                    break;
                }
                case NetworkCard.Custom:
                {
                    var card = SanitizeQemuArgumentStringWithOptions(networkInterface.CustomCard);

                    if (string.IsNullOrEmpty(card))
                        errors.Add(new LauncherError(LauncherErrorType.EmptyCustomNetworkInterfaceCard, i));

                    if (card.Length != networkInterface.CustomCard.Length)
                        errors.Add(new LauncherError(LauncherErrorType.InvalidCustomNetworkInterfaceCard, i));

                    var busType = GetCustomBusType(networkInterface.CustomCardBus, pciBusType);
                    arguments.Add($"{card}{busType},netdev=network{i}");
                    break;
                }
                default: throw new UnreachableException();
            }
        }

        for (var i = 0; i < vm.GraphicControllers.Count; i++)
        {
            var graphicsController = vm.GraphicControllers[i];

            arguments.Add("-device");
            switch (graphicsController.Card)
            {
                case GraphicsCard.Vga:
                {
                    if (!graphicsController.HasVgaEmulation)
                        errors.Add(new LauncherError(LauncherErrorType.InvalidVgaEmulationOptionForGraphicsCard, i));

                    if (graphicsController.HasGraphicsAcceleration)
                        errors.Add(new LauncherError(LauncherErrorType.InvalidGraphicsAccelerationOptionForGraphicsCard, i));

                    arguments.Add($"VGA,bus={pciBusType}.0");
                    break;
                }
                case GraphicsCard.Cirrus:
                {
                    if (!graphicsController.HasVgaEmulation)
                        errors.Add(new LauncherError(LauncherErrorType.InvalidVgaEmulationOptionForGraphicsCard, i));

                    if (graphicsController.HasGraphicsAcceleration)
                        errors.Add(new LauncherError(LauncherErrorType.InvalidGraphicsAccelerationOptionForGraphicsCard, i));

                    arguments.Add($"cirrus-vga,bus={pciBusType}.0");
                    break;
                }
                case GraphicsCard.Qxl:
                {
                    if (graphicsController.HasGraphicsAcceleration)
                        errors.Add(new LauncherError(LauncherErrorType.InvalidGraphicsAccelerationOptionForGraphicsCard, i));

                    var qxlDevice = graphicsController.HasVgaEmulation ? "qxl-vga" : "qxl";
                    arguments.Add($"{qxlDevice},bus={pciBusType}.0");
                    break;
                }
                case GraphicsCard.VMware:
                {
                    if (!graphicsController.HasVgaEmulation)
                        errors.Add(new LauncherError(LauncherErrorType.InvalidVgaEmulationOptionForGraphicsCard, i));

                    if (graphicsController.HasGraphicsAcceleration)
                        errors.Add(new LauncherError(LauncherErrorType.InvalidGraphicsAccelerationOptionForGraphicsCard, i));

                    arguments.Add($"vmware-svga,bus={pciBusType}.0");
                    break;
                }
                case GraphicsCard.VirtIo:
                {
                    var virtIoGpuDevice = graphicsController.HasVgaEmulation ? "virtio-vga" : "virtio-gpu";
                    if (graphicsController.HasGraphicsAcceleration) virtIoGpuDevice += "-gl";

                    arguments.Add($"{virtIoGpuDevice},bus={pciBusType}.0");
                    break;
                }
                case GraphicsCard.Custom:
                {
                    var card = SanitizeQemuArgumentStringWithOptions(graphicsController.CustomCard);

                    if (string.IsNullOrEmpty(card))
                        errors.Add(new LauncherError(LauncherErrorType.EmptyCustomGraphicsControllerCard, i));

                    if (card.Length != graphicsController.CustomCard.Length)
                        errors.Add(new LauncherError(LauncherErrorType.InvalidCustomGraphicsControllerCard, i));

                    if (graphicsController.HasVgaEmulation)
                        errors.Add(new LauncherError(LauncherErrorType.InvalidVgaEmulationOptionForGraphicsCard, i));

                    if (graphicsController.HasGraphicsAcceleration)
                        errors.Add(new LauncherError(LauncherErrorType.InvalidGraphicsAccelerationOptionForGraphicsCard, i));

                    var busType = GetCustomBusType(graphicsController.CustomCardBus, pciBusType);
                    arguments.Add($"{card}{busType}");
                    break;
                }
                default: throw new UnreachableException();
            }
        }

        for (var i = 0; i < vm.AudioControllers.Count; i++)
        {
            var audioController = vm.AudioControllers[i];

            arguments.Add("-device");
            switch (audioController.Card)
            {
                case SoundCard.SoundBlaster16:
                {
                    if (audioController.HasInput)
                        errors.Add(new LauncherError(LauncherErrorType.InvalidInputOptionForSoundCard, i));

                    if (!audioController.HasOutput)
                        errors.Add(new LauncherError(LauncherErrorType.InvalidOutputOptionForSoundCard, i));

                    arguments.Add("sb16,audiodev=audiodev");
                    break;
                }
                case SoundCard.IntelAc97:
                {
                    if (audioController.HasInput)
                        errors.Add(new LauncherError(LauncherErrorType.InvalidInputOptionForSoundCard, i));

                    if (!audioController.HasOutput)
                        errors.Add(new LauncherError(LauncherErrorType.InvalidOutputOptionForSoundCard, i));

                    arguments.Add($"AC97,bus={pciBusType}.0,audiodev=audiodev");
                    break;
                }
                case SoundCard.IntelHda6:
                {
                    arguments.Add($"intel-hda,bus={pciBusType}.0,id=hda{i}");

                    AddHdaDevices(ref arguments, ref audioController, i);
                    break;
                }
                case SoundCard.IntelHda9:
                {
                    arguments.Add($"ich9-intel-hda,bus={pciBusType}.0,id=hda{i}");

                    AddHdaDevices(ref arguments, ref audioController, i);
                    break;
                }
                case SoundCard.Usb:
                {
                    if (audioController.HasInput)
                        errors.Add(new LauncherError(LauncherErrorType.InvalidInputOptionForSoundCard, i));

                    if (!audioController.HasOutput)
                        errors.Add(new LauncherError(LauncherErrorType.InvalidOutputOptionForSoundCard, i));

                    if (vm.UsbControllers.Count == 0)
                        errors.Add(new LauncherError(LauncherErrorType.NoUsbControllersForSoundCard, i));

                    if (audioController.UsbController >= (ulong)vm.UsbControllers.Count)
                        errors.Add(new LauncherError(LauncherErrorType.InvalidUsbControllerForSoundCard, i));

                    arguments.Add($"usb-audio,bus=usb{audioController.UsbController}.0,audiodev=audiodev");
                    break;
                }
                case SoundCard.VirtIo:
                {
                    /* This is a QEMU limitation:
                     * > At the moment, no stream configuration is supported: the first one will always be a playback stream,
                     * > an optional second will always be a capture stream.
                     * https://qemu-project.gitlab.io/qemu/system/devices/virtio-snd.html */
                    if (audioController is { HasInput: true, HasOutput: false })
                        errors.Add(new LauncherError(LauncherErrorType.InvalidInputOutputOptionsForSoundCard, i));

                    var streams = audioController switch
                    {
                        { HasInput: true, HasOutput: true } => 2,
                        { HasInput: false, HasOutput: true } => 1,
                        _ => 0
                    };
                    arguments.Add($"virtio-sound-pci,bus={pciBusType}.0,audiodev=audiodev,streams={streams}");
                    break;
                }
                case SoundCard.Custom:
                {
                    var card = SanitizeQemuArgumentStringWithOptions(audioController.CustomCard);

                    if (string.IsNullOrEmpty(card))
                        errors.Add(new LauncherError(LauncherErrorType.EmptyCustomAudioControllerCard, i));

                    if (card.Length != audioController.CustomCard.Length)
                        errors.Add(new LauncherError(LauncherErrorType.InvalidCustomAudioControllerCard, i));

                    if (audioController.HasInput)
                        errors.Add(new LauncherError(LauncherErrorType.InvalidInputOptionForSoundCard, i));

                    if (audioController.HasOutput)
                        errors.Add(new LauncherError(LauncherErrorType.InvalidOutputOptionForSoundCard, i));

                    var busType = GetCustomBusType(audioController.CustomCardBus, pciBusType);
                    arguments.Add($"{card}{busType},audiodev=audiodev");
                    break;
                }
                default: throw new UnreachableException();
            }
        }

        var usedControllers = new Dictionary<int, (ulong Controller, bool Cdrom, bool Removable)>();

        for (var i = 0; i < vm.Disks.Count; i++)
        {
            var disk = vm.Disks[i];
            var format = disk.Format switch
            {
                DiskFormat.QCow2 => "qcow2",
                DiskFormat.Vdi => "vdi",
                DiskFormat.Vmdk => "vmdk",
                DiskFormat.VhdX => "vhdx",
                DiskFormat.Raw => "raw",
                DiskFormat.Custom => SanitizeQemuArgumentString(disk.CustomFormat),
                _ => throw new UnreachableException()
            };
            var cacheMethod = disk.CacheMethod switch
            {
                DiskCacheMethod.None => "none",
                DiskCacheMethod.WriteBack => "writeback",
                DiskCacheMethod.WriteThrough => "writethrough",
                DiskCacheMethod.DirectSync => "directsync",
                DiskCacheMethod.Unsafe => "unsafe",
                DiskCacheMethod.Custom => SanitizeQemuArgumentString(disk.CustomCacheMethod),
                _ => throw new UnreachableException()
            };
            var discard = disk.IsSsd ? "unmap" : "ignore";
            var path = disk.Path;
            
            if (string.IsNullOrEmpty(format))
                errors.Add(new LauncherError(LauncherErrorType.EmptyCustomDiskFormat, i));

            if (disk.Format == DiskFormat.Custom && format.Length != disk.CustomFormat.Length)
                errors.Add(new LauncherError(LauncherErrorType.InvalidCustomDiskFormat, i));

            if (string.IsNullOrEmpty(cacheMethod))
                errors.Add(new LauncherError(LauncherErrorType.EmptyCustomDiskCacheMethod, i));

            if (disk.CacheMethod == DiskCacheMethod.Custom && cacheMethod.Length != disk.CustomCacheMethod.Length)
                errors.Add(new LauncherError(LauncherErrorType.InvalidCustomDiskCacheMethod, i));

            var quotedPath = addQuotes && path.Contains(' ') ? $"\"{path}\"" : path;

            if (string.IsNullOrEmpty(path))
                errors.Add(new LauncherError(LauncherErrorType.EmptyDiskPath, i));
            else if (!File.Exists(path))
                errors.Add(new LauncherError(LauncherErrorType.DiskDoesNotExist, i));

            arguments.Add("-drive");
            arguments.Add($"if=none,format={format},cache={cacheMethod},discard={discard},file={quotedPath},id=drive{i}");

            usedControllers[i] = (disk.Controller, disk.IsCdrom, disk.IsRemovable);
        }

        for (var i = 0; i < vm.DiskControllers.Count; i++)
            if (!usedControllers.ContainsKey(i))
                errors.Add(new LauncherError(LauncherErrorType.UnusedDiskController, i));

        var usedSataControllers = new Dictionary<ulong, ulong>();
        var ideBus = 0UL;

        foreach (var (i, usedController) in usedControllers)
        {
            if (usedController.Controller >= (ulong)vm.DiskControllers.Count)
                errors.Add(new LauncherError(LauncherErrorType.InvalidDiskControllerForDisk, i));

            var diskController = vm.DiskControllers[(int)usedController.Controller];

            arguments.Add("-device");
            switch (diskController.Model)
            {
                case DiskBus.Floppy:
                {
                    if (usedController.Cdrom)
                        errors.Add(new LauncherError(LauncherErrorType.InvalidCdromOptionForDiskBus, i));

                    if (usedController.Removable)
                        errors.Add(new LauncherError(LauncherErrorType.InvalidRemovableOptionForDiskBus, i));

                    arguments.Add($"floppy,drive=drive{i}");
                    break;
                }
                case DiskBus.Ide:
                {
                    if (usedController.Removable)
                        errors.Add(new LauncherError(LauncherErrorType.InvalidRemovableOptionForDiskBus, i));

                    var ideDevice = usedController.Cdrom ? "ide-cd" : "ide-hd";
                    arguments.Add($"{ideDevice},bus=ide.{ideBus++},drive=drive{i}");
                    break;
                }
                case DiskBus.Sata:
                {
                    if (usedController.Removable)
                        errors.Add(new LauncherError(LauncherErrorType.InvalidRemovableOptionForDiskBus, i));

                    if (usedSataControllers.TryAdd(usedController.Controller, 0))
                    {
                        arguments.Add($"ahci,bus={pciBusType}.0,id=ahci{usedController.Controller}");
                        arguments.Add("-device");
                    }

                    var insertedDrives = usedSataControllers[usedController.Controller];
                    usedSataControllers[usedController.Controller] = insertedDrives + 1;

                    var ideDevice = usedController.Cdrom ? "ide-cd" : "ide-hd";
                    arguments.Add($"{ideDevice},bus=ahci{usedController.Controller}.{insertedDrives},drive=drive{i}");
                    break;
                }
                case DiskBus.Nvme:
                {
                    if (usedController.Cdrom)
                        errors.Add(new LauncherError(LauncherErrorType.InvalidCdromOptionForDiskBus, i));

                    if (usedController.Removable)
                        errors.Add(new LauncherError(LauncherErrorType.InvalidRemovableOptionForDiskBus, i));

                    arguments.Add($"nvme,bus={pciBusType}.0,drive=drive{i},serial=0B00B135");
                    break;
                }
                case DiskBus.Usb:
                {
                    if (usedController.Cdrom)
                        errors.Add(new LauncherError(LauncherErrorType.InvalidCdromOptionForDiskBus, i));

                    if (vm.UsbControllers.Count == 0)
                        errors.Add(new LauncherError(LauncherErrorType.NoUsbControllersForDiskBus, i));

                    if (diskController.UsbController >= (ulong)vm.UsbControllers.Count)
                        errors.Add(new LauncherError(LauncherErrorType.InvalidUsbControllerForDiskBus, i));

                    var removable = usedController.Removable ? "true" : "false";
                    arguments.Add($"usb-storage,bus=usb{diskController.UsbController}.0,drive=drive{i},removable={removable}");
                    break;
                }
                case DiskBus.VirtIo:
                {
                    if (usedController.Cdrom)
                        errors.Add(new LauncherError(LauncherErrorType.InvalidCdromOptionForDiskBus, i));

                    if (usedController.Removable)
                        errors.Add(new LauncherError(LauncherErrorType.InvalidRemovableOptionForDiskBus, i));

                    arguments.Add($"virtio-blk-pci,bus={pciBusType}.0,drive=drive{i}");
                    break;
                }
                case DiskBus.Custom:
                {
                    var model = SanitizeQemuArgumentStringWithOptions(diskController.CustomModel);

                    if (string.IsNullOrEmpty(model))
                        errors.Add(new LauncherError(LauncherErrorType.EmptyCustomDiskControllerModel, i));

                    if (model.Length != diskController.CustomModel.Length)
                        errors.Add(new LauncherError(LauncherErrorType.InvalidCustomDiskControllerModel, i));

                    var busType = GetCustomBusType(diskController.CustomModelBus, pciBusType);
                    arguments.Add($"{model}{busType},drive=drive{i}");
                    break;
                }
                default: throw new UnreachableException();
            }
        }

        for (var i = 0; i < vm.Keyboards.Count; i++)
        {
            var keyboard = vm.Keyboards[i];

            arguments.Add("-device");
            switch (keyboard.Model)
            {
                case KeyboardModel.Usb:
                {
                    if (vm.UsbControllers.Count == 0)
                        errors.Add(new LauncherError(LauncherErrorType.NoUsbControllersForKeyboard, i));

                    if (keyboard.UsbController >= (ulong)vm.UsbControllers.Count)
                        errors.Add(new LauncherError(LauncherErrorType.InvalidUsbControllerForKeyboard, i));

                    arguments.Add($"usb-kbd,bus=usb{keyboard.UsbController}.0");
                    break;
                }
                case KeyboardModel.VirtIo:
                {
                    arguments.Add($"virtio-keyboard-pci,bus={pciBusType}.0");
                    break;
                }
                case KeyboardModel.Custom:
                {
                    var model = SanitizeQemuArgumentStringWithOptions(keyboard.CustomModel);

                    if (string.IsNullOrEmpty(model))
                        errors.Add(new LauncherError(LauncherErrorType.EmptyCustomKeyboardModel, i));

                    if (model.Length != keyboard.CustomModel.Length)
                        errors.Add(new LauncherError(LauncherErrorType.InvalidCustomKeyboardModel, i));

                    var busType = GetCustomBusType(keyboard.CustomModelBus, pciBusType);
                    arguments.Add($"{model}{busType}");
                    break;
                }
                default: throw new UnreachableException();
            }
        }

        for (var i = 0; i < vm.Mice.Count; i++)
        {
            var mouse = vm.Mice[i];

            arguments.Add("-device");
            switch (mouse.Model)
            {
                case MouseModel.Usb:
                {
                    if (vm.UsbControllers.Count == 0)
                        errors.Add(new LauncherError(LauncherErrorType.NoUsbControllersForMouse, i));

                    if (mouse.UsbController >= (ulong)vm.UsbControllers.Count)
                        errors.Add(new LauncherError(LauncherErrorType.InvalidUsbControllerForMouse, i));

                    var usbMouseDevice = mouse.UseAbsolutePointing ? "usb-tablet" : "usb-mouse";
                    arguments.Add($"{usbMouseDevice},bus=usb{mouse.UsbController}.0");
                    break;
                }
                case MouseModel.VirtIo:
                {
                    var virtIoMouseDevice = mouse.UseAbsolutePointing ? "virtio-tablet-pci" : "virtio-mouse-pci";
                    arguments.Add($"{virtIoMouseDevice},bus={pciBusType}.0");
                    break;
                }
                case MouseModel.Custom:
                {
                    var model = SanitizeQemuArgumentStringWithOptions(mouse.CustomModel);

                    if (string.IsNullOrEmpty(model))
                        errors.Add(new LauncherError(LauncherErrorType.EmptyCustomMouseModel, i));

                    if (model.Length != mouse.CustomModel.Length)
                        errors.Add(new LauncherError(LauncherErrorType.InvalidCustomMouseModel, i));

                    if (mouse.UseAbsolutePointing)
                        errors.Add(new LauncherError(LauncherErrorType.InvalidAbsolutePointingOptionForMouse, i));

                    var busType = GetCustomBusType(mouse.CustomModelBus, pciBusType);
                    arguments.Add($"{model}{busType}");
                    break;
                }
                default: throw new UnreachableException();
            }
        }

        for (var i = 0; i < vm.CustomQemuArguments.Count; i++)
        {
            var customQemuArgument = vm.CustomQemuArguments[i];

            arguments.Add(customQemuArgument.Type switch
            {
                CustomQemuArgumentType.Device => "-device",
                CustomQemuArgumentType.TpmDevice => "-tpmdev",
                CustomQemuArgumentType.CharacterDevice => "-chardev",
                CustomQemuArgumentType.Drive => "-drive",
                CustomQemuArgumentType.Boot => "-boot",
                CustomQemuArgumentType.SmBios => "-smbios",
                _ => throw new UnreachableException()
            });

            var argumentValue = SanitizeQemuArgumentString(customQemuArgument.Value);
            var argument = new StringBuilder();

            if (customQemuArgument.Type < CustomQemuArgumentType.Drive)
            {
                if (string.IsNullOrEmpty(argumentValue))
                    errors.Add(new LauncherError(LauncherErrorType.EmptyCustomQemuArgumentValue, i));

                if (argumentValue.Length != customQemuArgument.Value.Length)
                    errors.Add(new LauncherError(LauncherErrorType.InvalidCustomQemuArgumentValue, i));

                argument.Append(argumentValue);
            }

            for (var j = 0; j < customQemuArgument.Parameters.Count; j++)
            {
                var parameter = customQemuArgument.Parameters[j];
                var key = SanitizeQemuArgumentString(parameter.Key);

                if (string.IsNullOrEmpty(key))
                    errors.Add(new LauncherError(LauncherErrorType.EmptyCustomQemuArgumentParameterKey, i, j));

                if (key.Length != parameter.Key.Length)
                    errors.Add(new LauncherError(LauncherErrorType.InvalidCustomQemuArgumentParameterKey, i, j));

                var value = SanitizeQemuParameterValueString(parameter.Value);

                if (string.IsNullOrEmpty(value))
                    errors.Add(new LauncherError(LauncherErrorType.EmptyCustomQemuArgumentParameterValue, i, j));

                if (value.Length != parameter.Value.Length)
                    errors.Add(new LauncherError(LauncherErrorType.InvalidCustomQemuArgumentParameterValue, i, j));

                if (argument.Length != 0) argument.Append(',');
                argument.Append(key);
                argument.Append('=');
                argument.Append(value);
            }

            arguments.Add(argument.ToString());
        }

        var qemuSystemPath = !string.IsNullOrEmpty(customQemuPath) ? customQemuPath
            : Path.GetDirectoryName(PathLookup.GetQemuImgPath());
        var quotedQemuSystemPath = qemuSystemPath;

        if (!string.IsNullOrEmpty(qemuSystemPath))
        {
            qemuSystemPath = Path.Combine(qemuSystemPath, vm.Architecture switch
            {
                Architecture.Amd64 => "qemu-system-x86_64",
                Architecture.I386 => "qemu-system-i386",
                _ => throw new UnreachableException()
            });
            if (OperatingSystem.IsWindows()) qemuSystemPath += ".exe";
            if (!File.Exists(qemuSystemPath)) errors.Add(new LauncherError(LauncherErrorType.QemuSystemDoesNotExist));

            quotedQemuSystemPath = addQuotes && qemuSystemPath.Contains(' ') ? $"\"{qemuSystemPath}\"" : qemuSystemPath;
        } else errors.Add(new LauncherError(LauncherErrorType.EmptyQemuPath));

        programs.Add(new Program(quotedQemuSystemPath, arguments, true));
        return new LauncherResult(errors, nvRamPath, programs);
    }

    private static bool IsFeatureUnsupported(Architecture architecture, ProcessorFeature feature)
        => feature is >= ProcessorFeature.X86Kvm and <= ProcessorFeature.X86HyperVPassThrough
           && architecture is not Architecture.Amd64 and not Architecture.I386;

    private static string ProcessorFeatureToString(ProcessorFeature feature) => feature switch
    {
        ProcessorFeature.X86Kvm => "kvm",
        ProcessorFeature.X86HyperVPassThrough => "hv-passthrough",
        _ => throw new UnreachableException()
    };

    private static void AddHdaDevices(ref List<string> arguments, ref AudioController audioController, int i)
    {
        arguments.Add("-device");
        switch (audioController)
        {
            case { HasInput: true, HasOutput: true }:
            {
                arguments.Add($"hda-duplex,bus=hda{i}.0,audiodev=audiodev");
                break;
            }
            case { HasInput: false, HasOutput: true }:
            {
                arguments.Add($"hda-output,bus=hda{i}.0,audiodev=audiodev");
                break;
            }
            default:
            {
                arguments.Add($"hda-input,bus=hda{i}.0,audiodev=audiodev");
                break;
            }
        }
    }

    private static string GetCustomBusType(DeviceBus deviceBus, string pciBusType) => deviceBus.Type switch
    {
        BusType.Default => string.Empty,
        BusType.Isa => ",bus=isa.0",
        BusType.Pci => $",bus={pciBusType}.0",
        BusType.Usb => ",bus=usb.0",
        BusType.Custom => $",bus={deviceBus.CustomType}",
        _ => throw new UnreachableException()
    };

    private static string SanitizeName(string name)
    {
        var newName = new StringBuilder();

        foreach (var c in name)
            if (char.IsAsciiLetterOrDigit(c) || c is ' ' or '.')
                newName.Append(c);

        return newName.ToString();
    }

    private static string SanitizeQemuArgumentString(string str)
    {
        var newStr = new StringBuilder();

        foreach (var c in str)
            if (char.IsAsciiLetterOrDigit(c) || c is '-' or '_' or '.')
                newStr.Append(c);

        return newStr.ToString();
    }

    private static string SanitizeQemuParameterValueString(string str)
    {
        var newStr = new StringBuilder();

        foreach (var c in str)
            if (char.IsAsciiLetterOrDigit(c) || c is '-' or '_' or '/' or '\\' or ':' or '.' or ' ' or '(' or ')')
                newStr.Append(c);

        return newStr.ToString();
    }

    private static string SanitizeQemuArgumentStringWithOptions(string str)
    {
        var newStr = new StringBuilder();

        foreach (var c in str)
            if (char.IsAsciiLetterOrDigit(c) || c is '-' or '_' or '.' or ',')
                newStr.Append(c);

        return newStr.ToString();
    }
}