using System.Collections.Generic;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using QemuSharp.Structs;
using QemuSharp.Structs.Enums;

namespace VirtualWorkstation;

public static class UiHelpers
{
    public static readonly FilePickerOpenOptions DefaultFilePickerOpenOptions = new();
    public static readonly FolderPickerOpenOptions DefaultFolderPickerOpenOptions = new();

    // We need to initialize this field first, so it must be above the others
    private static readonly FilePickerFileType FilePickerTomlFileType = new("TOML File")
    {
        Patterns = new[] { "*.toml" },
        MimeTypes = new[] { "application/toml" }
    };

    public static readonly FilePickerOpenOptions FilePickerConfigOpenOptions = new()
    {
        Title = "Open Config File",
        FileTypeFilter = [FilePickerTomlFileType]
    };

    public static readonly FilePickerSaveOptions FilePickerConfigSaveOptions = new()
    {
        Title = "Save Config File",
        DefaultExtension = "toml",
        FileTypeChoices = [FilePickerTomlFileType]
    };

    public static string ToUiString(this UiComponent component) => component switch
    {
        UiComponent.System => "System",
        UiComponent.Firmware => "Firmware",
        UiComponent.Chipset => "Chipset",
        UiComponent.Display => "Display",
        UiComponent.AudioHost => "Audio Host",
        UiComponent.Processor => "Processor",
        UiComponent.TrustedPlatformModule => "Trusted Platform Module",
        UiComponent.UsbController => "USB Controller",
        UiComponent.NetworkInterface => "Network Interface",
        UiComponent.GraphicsController => "Graphics Controller",
        UiComponent.SerialController => "Serial Controller",
        UiComponent.AudioController => "Audio Controller",
        UiComponent.DiskController => "Disk Controller",
        UiComponent.Disk => "Disk",
        UiComponent.Keyboard => "Keyboard",
        UiComponent.Mouse => "Mouse",
        UiComponent.CustomQemuArgument => "Custom QEMU Argument",
        _ => throw new UnreachableException()
    };

    public static string ToExtensionString(this DiskFormat format, string customFormat) => format switch
    {
        DiskFormat.QCow2 => "qcow2",
        DiskFormat.Vdi => "vdi",
        DiskFormat.Vmdk => "vmdk",
        DiskFormat.VhdX => "vhdx",
        DiskFormat.Raw => "img",
        DiskFormat.Custom => customFormat,
        _ => throw new UnreachableException()
    };

    public static void RefreshControllerList(ref ComboBox comboBox, int startIndex)
    {
        if (startIndex == -1)
        {
            // The controller was added
            comboBox.Items.Add(new ComboBoxItem { Content = comboBox.Items.Count.ToString() });
            return;
        }

        // The controller was removed
        comboBox.Items.RemoveAt(startIndex);

        for (var i = startIndex; i < comboBox.Items.Count; i++)
        {
            if (comboBox.Items[i] is not ComboBoxItem item) throw new UnreachableException();
            item.Content = i.ToString();
        }
    }

    public static void RemoveCurrentListItem<TSettingsPage>(ref ListBox componentList) where TSettingsPage : ITabPage
    {
        var listIndex = componentList.SelectedIndex + 1;
        while (listIndex < componentList.Items.Count)
        {
            if (componentList.Items[listIndex++] is not ListBoxItem childItem) throw new UnreachableException();
            if (childItem.Tag is not TSettingsPage page) break;

            var content = childItem.Content?.ToString() ?? throw new UnreachableException();
            childItem.Content = $"{content.Remove(content.LastIndexOf(' ') + 1)}{--page.Index}";
        }
    }

    public static void RefreshUsbControllerLists(ref ListBox componentList, List<int> componentIndices, ref VirtualMachine vm, int startIndex)
    {
        RefreshUsbControllerList<NetworkInterfaceSettingsPage>(ref componentList,
            componentIndices[UiComponent.NetworkInterface - UiComponent.UsbController] - vm.NetworkInterfaces.Count, startIndex);
        RefreshUsbControllerList<SerialControllerSettingsPage>(ref componentList,
            componentIndices[UiComponent.SerialController - UiComponent.UsbController] - vm.SerialControllers.Count, startIndex);
        RefreshUsbControllerList<AudioControllerSettingsPage>(ref componentList,
            componentIndices[UiComponent.AudioController - UiComponent.UsbController] - vm.AudioControllers.Count, startIndex);
        RefreshUsbControllerList<DiskControllerSettingsPage>(ref componentList,
            componentIndices[UiComponent.DiskController - UiComponent.UsbController] - vm.DiskControllers.Count, startIndex);
        RefreshUsbControllerList<KeyboardSettingsPage>(ref componentList,
            componentIndices[UiComponent.Keyboard - UiComponent.UsbController] - vm.Keyboards.Count, startIndex);
        RefreshUsbControllerList<MouseSettingsPage>(ref componentList,
            componentIndices[UiComponent.Mouse - UiComponent.UsbController] - vm.Mice.Count, startIndex);
    }

    public static void TriggerUsbControllerEvents(ref ListBox componentList, List<int> componentIndices, ref VirtualMachine vm)
    {
        TriggerUsbControllerEvent<NetworkInterfaceSettingsPage>(ref componentList,
            componentIndices[UiComponent.NetworkInterface - UiComponent.UsbController] - vm.NetworkInterfaces.Count);
        TriggerUsbControllerEvent<SerialControllerSettingsPage>(ref componentList,
            componentIndices[UiComponent.SerialController - UiComponent.UsbController] - vm.SerialControllers.Count);
        TriggerUsbControllerEvent<AudioControllerSettingsPage>(ref componentList,
            componentIndices[UiComponent.AudioController - UiComponent.UsbController] - vm.AudioControllers.Count);
        TriggerUsbControllerEvent<DiskControllerSettingsPage>(ref componentList,
            componentIndices[UiComponent.DiskController - UiComponent.UsbController] - vm.DiskControllers.Count);
        TriggerUsbControllerEvent<KeyboardSettingsPage>(ref componentList,
            componentIndices[UiComponent.Keyboard - UiComponent.UsbController] - vm.Keyboards.Count);
        TriggerUsbControllerEvent<MouseSettingsPage>(ref componentList,
            componentIndices[UiComponent.Mouse - UiComponent.UsbController] - vm.Mice.Count);
    }

    public static void RefreshDiskControllerLists(ref ListBox componentList, List<int> componentIndices, ref VirtualMachine vm, int startIndex)
    {
        var startListIndex = componentIndices[UiComponent.Disk - UiComponent.UsbController] - vm.Disks.Count;
        while (startListIndex < componentList.Items.Count)
        {
            if (componentList.Items[startListIndex++] is not ListBoxItem childItem) throw new UnreachableException();
            if (childItem.Tag is not DiskSettingsPage page) break;

            page.RefreshDiskControllerList(startIndex);
        }
    }

    public static void TriggerDiskControllerEvents(ref ListBox componentList, List<int> componentIndices, ref VirtualMachine vm)
    {
        var startListIndex = componentIndices[UiComponent.Disk - UiComponent.UsbController] - vm.Disks.Count;
        while (startListIndex < componentList.Items.Count)
        {
            if (componentList.Items[startListIndex++] is not ListBoxItem childItem) throw new UnreachableException();
            if (childItem.Tag is not DiskSettingsPage page) break;

            page.TriggerSelectionChangedEvent();
        }
    }

    private static void RefreshUsbControllerList<TSettingsPage>(ref ListBox componentList, int startListIndex, int startIndex)
        where TSettingsPage : IController
    {
        while (startListIndex < componentList.Items.Count)
        {
            if (componentList.Items[startListIndex++] is not ListBoxItem childItem) throw new UnreachableException();
            if (childItem.Tag is not TSettingsPage page) break;

            page.RefreshUsbControllerList(startIndex);
        }
    }

    private static void TriggerUsbControllerEvent<TSettingsPage>(ref ListBox componentList, int startListIndex)
        where TSettingsPage : IController
    {
        while (startListIndex < componentList.Items.Count)
        {
            if (componentList.Items[startListIndex++] is not ListBoxItem childItem) throw new UnreachableException();
            if (childItem.Tag is not TSettingsPage page) break;

            page.TriggerSelectionChangedEvent();
        }
    }
}