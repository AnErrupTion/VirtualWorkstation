namespace QemuSharp;

public enum LauncherErrorType
{
    EmptyName,
    EmptyFirmwarePath,
    EmptyCustomFirmwarePath,
    EmptyCustomChipsetModel,
    EmptyDiskPath,
    EmptyQemuPath,
    EmptyCustomQemuArgumentValue,
    EmptyCustomQemuArgumentParameterKey,
    EmptyCustomQemuArgumentParameterValue,

    InvalidName,
    InvalidCustomChipsetModel,
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