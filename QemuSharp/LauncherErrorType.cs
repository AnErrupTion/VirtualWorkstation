namespace QemuSharp;

public enum LauncherErrorType
{
    EmptyName,
    EmptyFirmwarePath,
    EmptyCustomFirmwarePath,
    EmptyCustomChipsetModel,
    EmptyCustomDisplayType,
    EmptyCustomAudioHostDeviceType,
    EmptyCustomProcessorModel,
    EmptyCustomUsbControllerVersion,
    EmptyCustomNetworkInterfaceType,
    EmptyCustomNetworkInterfaceCard,
    EmptyCustomGraphicsControllerCard,
    EmptyCustomAudioControllerCard,
    EmptyCustomDiskControllerModel,
    EmptyDiskPath,
    EmptyQemuPath,
    EmptyCustomQemuArgumentValue,
    EmptyCustomQemuArgumentParameterKey,
    EmptyCustomQemuArgumentParameterValue,

    InvalidName,
    InvalidCustomChipsetModel,
    InvalidCustomDisplayType,
    InvalidCustomAudioHostDeviceType,
    InvalidCustomProcessorModel,
    InvalidCustomUsbControllerVersion,
    InvalidCustomNetworkInterfaceType,
    InvalidCustomNetworkInterfaceCard,
    InvalidCustomGraphicsControllerCard,
    InvalidCustomAudioControllerCard,
    InvalidCustomDiskControllerModel,
    InvalidCustomQemuArgumentValue,
    InvalidCustomQemuArgumentParameterKey,
    InvalidCustomQemuArgumentParameterValue,
    CustomFirmwareDoesNotExist,
    DiskDoesNotExist,
    QemuSystemDoesNotExist,

    HardwareAccelerationUnavailable,
    AudioUnavailable,

    InvalidFirmwareTypeForArchitecture,
    InvalidChipsetModelForArchitecture,
    InvalidProcessorModelForArchitecture,
    InvalidAddedFeatureForArchitecture,
    InvalidRemovedFeatureForArchitecture,

    InvalidForcePciOptionForChipsetModel,

    UnsupportedDisplay,
    UnsupportedAudioHostDevice,

    NoUsbControllersForNetworkCard,
    NoUsbControllersForSoundCard,
    NoUsbControllersForDiskBus,
    NoUsbControllersForKeyboard,
    NoUsbControllersForMouse,

    InvalidUsbControllerForNetworkCard,
    InvalidUsbControllerForSoundCard,
    InvalidUsbControllerForDiskBus,
    InvalidUsbControllerForKeyboard,
    InvalidUsbControllerForMouse,

    InvalidVgaEmulationOptionForGraphicsCard,
    InvalidGraphicsAccelerationOptionForGraphicsCard,

    InvalidInputOptionForSoundCard,
    InvalidOutputOptionForSoundCard,
    InvalidInputOutputOptionsForSoundCard,

    UnusedDiskController,
    InvalidDiskControllerForDisk,

    InvalidCdromOptionForDiskBus,
    InvalidRemovableOptionForDiskBus
}