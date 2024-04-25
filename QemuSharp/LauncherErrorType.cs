namespace QemuSharp;

public enum LauncherErrorType
{
    EmptyName,
    EmptyFirmwarePath,
    EmptyEfiNvRamPath,
    EmptyCustomFirmwarePath,
    EmptyCustomEfiNvRamPath,
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
    EmptyCustomDiskFormat,
    EmptyCustomDiskCacheMethod,
    EmptyCustomKeyboardModel,
    EmptyCustomMouseModel,
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
    InvalidCustomDiskFormat,
    InvalidCustomDiskCacheMethod,
    InvalidCustomKeyboardModel,
    InvalidCustomMouseModel,
    InvalidCustomQemuArgumentValue,
    InvalidCustomQemuArgumentParameterKey,
    InvalidCustomQemuArgumentParameterValue,
    CustomFirmwareDoesNotExist,
    CustomEfiNvRamDoesNotExist,
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

    InvalidAbsolutePointingOptionForMouse,

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