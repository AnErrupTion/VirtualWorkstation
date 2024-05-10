using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MsBox.Avalonia;
using QemuSharp.Structs;
using QemuSharp.Structs.Enums;

namespace VirtualWorkstation;

public partial class VirtualMachineSettings : Window
{
    public bool HasSaved { get; private set; }

    // We want to store every component from "UsbController" to "CustomQemuArgument" included, so we subtract
    // "CustomQemuArgument" to "TrustedPlatformModule" since our enum values start at 0 (and "TrustedPlatformModule" is
    // not a controller).
    public List<int> ComponentIndices { get; } = new(UiComponent.CustomQemuArgument - UiComponent.TrustedPlatformModule);

    private VirtualMachine _vm;

    public VirtualMachineSettings(ref VirtualMachine vm)
    {
        InitializeComponent();

        _vm = vm;

        Title += vm.Name;

        ComponentList.Items.Add(new ListBoxItem { Content = UiComponent.System.ToUiString(), Tag = new SystemSettingsPage(ref vm) });
        ComponentList.Items.Add(new ListBoxItem { Content = UiComponent.Memory.ToUiString(), Tag = new MemorySettingsPage(ref vm) });
        ComponentList.Items.Add(new ListBoxItem { Content = UiComponent.Firmware.ToUiString(), Tag = new FirmwareSettingsPage(ref vm) });
        ComponentList.Items.Add(new ListBoxItem { Content = UiComponent.Chipset.ToUiString(), Tag = new ChipsetSettingsPage(ref vm) });
        ComponentList.Items.Add(new ListBoxItem { Content = UiComponent.Display.ToUiString(), Tag = new DisplaySettingsPage(ref vm) });
        ComponentList.Items.Add(new ListBoxItem { Content = UiComponent.AudioHost.ToUiString(), Tag = new AudioHostSettingsPage(ref vm) });
        ComponentList.Items.Add(new ListBoxItem { Content = UiComponent.Processor.ToUiString(), Tag = new ProcessorSettingsPage(ref vm) });
        ComponentList.Items.Add(new ListBoxItem { Content = UiComponent.TrustedPlatformModule.ToUiString(), Tag = new TrustedPlatformModuleSettingsPage(ref vm) });

        for (var i = 0; i < vm.UsbControllers.Count; i++)
            ComponentList.Items.Add(new ListBoxItem { Content = $"{UiComponent.UsbController.ToUiString()} {i}", Tag = new UsbControllerSettingsPage(ref vm, i) });
        ComponentIndices.Add(ComponentList.Items.Count);

        for (var i = 0; i < vm.NetworkInterfaces.Count; i++)
            ComponentList.Items.Add(new ListBoxItem { Content = $"{UiComponent.NetworkInterface.ToUiString()} {i}", Tag = new NetworkInterfaceSettingsPage(ref vm, i) });
        ComponentIndices.Add(ComponentList.Items.Count);

        for (var i = 0; i < vm.GraphicControllers.Count; i++)
            ComponentList.Items.Add(new ListBoxItem { Content = $"{UiComponent.GraphicsController.ToUiString()} {i}", Tag = new GraphicsControllerSettingsPage(ref vm, i) });
        ComponentIndices.Add(ComponentList.Items.Count);

        for (var i = 0; i < vm.SerialControllers.Count; i++)
            ComponentList.Items.Add(new ListBoxItem { Content = $"{UiComponent.SerialController.ToUiString()} {i}", Tag = new SerialControllerSettingsPage(ref vm, i) });
        ComponentIndices.Add(ComponentList.Items.Count);

        for (var i = 0; i < vm.AudioControllers.Count; i++)
            ComponentList.Items.Add(new ListBoxItem { Content = $"{UiComponent.AudioController.ToUiString()} {i}", Tag = new AudioControllerSettingsPage(ref vm, i) });
        ComponentIndices.Add(ComponentList.Items.Count);

        for (var i = 0; i < vm.DiskControllers.Count; i++)
            ComponentList.Items.Add(new ListBoxItem { Content = $"{UiComponent.DiskController.ToUiString()} {i}", Tag = new DiskControllerSettingsPage(ref vm, i) });
        ComponentIndices.Add(ComponentList.Items.Count);

        for (var i = 0; i < vm.Disks.Count; i++)
            ComponentList.Items.Add(new ListBoxItem { Content = $"{UiComponent.Disk.ToUiString()} {i}", Tag = new DiskSettingsPage(ref vm, i) });
        ComponentIndices.Add(ComponentList.Items.Count);

        for (var i = 0; i < vm.Keyboards.Count; i++)
            ComponentList.Items.Add(new ListBoxItem { Content = $"{UiComponent.Keyboard.ToUiString()} {i}", Tag = new KeyboardSettingsPage(ref vm, i) });
        ComponentIndices.Add(ComponentList.Items.Count);

        for (var i = 0; i < vm.Mice.Count; i++)
            ComponentList.Items.Add(new ListBoxItem { Content = $"{UiComponent.Mouse.ToUiString()} {i}", Tag = new MouseSettingsPage(ref vm, i) });
        ComponentIndices.Add(ComponentList.Items.Count);

        for (var i = 0; i < vm.CustomQemuArguments.Count; i++)
            ComponentList.Items.Add(new ListBoxItem { Content = $"{UiComponent.CustomQemuArgument.ToUiString()} {i}", Tag = new CustomQemuArgumentSettingsPage(ref vm, i) });
        ComponentIndices.Add(ComponentList.Items.Count);

        ComponentList.SelectedIndex = 0;
    }

    private void ComponentList_OnSelectionChanged(object? _, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count != 1) throw new UnreachableException();
        if (e.AddedItems[0] is not ListBoxItem item) throw new UnreachableException();
        if (item.Tag is not ITabPage settingsPage) throw new UnreachableException();

        Remove.IsEnabled = ComponentList.SelectedIndex > (int)UiComponent.TrustedPlatformModule;

        if (settingsPage.Opened)
        {
            ComponentTabs.SelectedIndex = settingsPage.VmTabIndex;
            return;
        }

        settingsPage.VmTabIndex = ComponentTabs.Items.Count;
        settingsPage.Opened = true;

        ComponentTabs.Items.Add(new TabItem { Header = item.Content, Content = item.Tag });
        ComponentTabs.SelectedIndex = settingsPage.VmTabIndex;
    }

    private async void Add_OnClick(object? _, RoutedEventArgs e)
    {
        var addComponent = new VirtualMachineAddComponent(ref _vm, this);
        await addComponent.ShowDialog(this);
    }

    private async void Remove_OnClick(object? _, RoutedEventArgs e)
    {
        if (ComponentList.SelectedItem is not ListBoxItem item) throw new UnreachableException();

        switch (item.Tag)
        {
            case UsbControllerSettingsPage usbControllerSettingsPage:
            {
                var usbController = (ulong)usbControllerSettingsPage.Index;
                if (!await CheckUsbControllerUsage(usbController)) return;

                var index = usbControllerSettingsPage.Index;
                UiHelpers.RemoveCurrentListItem<UsbControllerSettingsPage>(ref ComponentList);

                UiHelpers.RefreshUsbControllerLists(ref ComponentList, ComponentIndices, ref _vm, index);
                _vm.UsbControllers.RemoveAt(index);
                UiHelpers.TriggerUsbControllerEvents(ref ComponentList, ComponentIndices, ref _vm);

                for (var i = UiComponent.UsbController - UiComponent.UsbController; i < ComponentIndices.Count; i++)
                    ComponentIndices[i]--;
                break;
            }
            case NetworkInterfaceSettingsPage networkInterfaceSettingsPage:
            {
                var index = networkInterfaceSettingsPage.Index;
                UiHelpers.RemoveCurrentListItem<NetworkInterfaceSettingsPage>(ref ComponentList);

                _vm.NetworkInterfaces.RemoveAt(index);

                for (var i = UiComponent.NetworkInterface - UiComponent.UsbController; i < ComponentIndices.Count; i++)
                    ComponentIndices[i]--;
                break;
            }
            case GraphicsControllerSettingsPage graphicsControllerSettingsPage:
            {
                var index = graphicsControllerSettingsPage.Index;
                UiHelpers.RemoveCurrentListItem<GraphicsControllerSettingsPage>(ref ComponentList);

                _vm.GraphicControllers.RemoveAt(index);

                for (var i = UiComponent.GraphicsController - UiComponent.UsbController; i < ComponentIndices.Count; i++)
                    ComponentIndices[i]--;
                break;
            }
            case SerialControllerSettingsPage serialControllerSettingsPage:
            {
                var index = serialControllerSettingsPage.Index;
                UiHelpers.RemoveCurrentListItem<SerialControllerSettingsPage>(ref ComponentList);

                _vm.SerialControllers.RemoveAt(index);

                for (var i = UiComponent.SerialController - UiComponent.UsbController; i < ComponentIndices.Count; i++)
                    ComponentIndices[i]--;
                break;
            }
            case AudioControllerSettingsPage audioControllerSettingsPage:
            {
                var index = audioControllerSettingsPage.Index;
                UiHelpers.RemoveCurrentListItem<AudioControllerSettingsPage>(ref ComponentList);

                _vm.AudioControllers.RemoveAt(index);

                for (var i = UiComponent.AudioController - UiComponent.UsbController; i < ComponentIndices.Count; i++)
                    ComponentIndices[i]--;
                break;
            }
            case DiskControllerSettingsPage diskControllerSettingsPage:
            {
                var controller = (ulong)diskControllerSettingsPage.Index;
                if (!await CheckDiskControllerUsage(controller)) return;

                var index = diskControllerSettingsPage.Index;
                UiHelpers.RemoveCurrentListItem<DiskControllerSettingsPage>(ref ComponentList);

                UiHelpers.RefreshDiskControllerLists(ref ComponentList, ComponentIndices, ref _vm, index);
                _vm.DiskControllers.RemoveAt(index);
                UiHelpers.TriggerDiskControllerEvents(ref ComponentList, ComponentIndices, ref _vm);

                for (var i = UiComponent.DiskController - UiComponent.UsbController; i < ComponentIndices.Count; i++)
                    ComponentIndices[i]--;
                break;
            }
            case DiskSettingsPage diskSettingsPage:
            {
                var index = diskSettingsPage.Index;
                UiHelpers.RemoveCurrentListItem<DiskSettingsPage>(ref ComponentList);

                _vm.Disks.RemoveAt(index);

                for (var i = UiComponent.Disk - UiComponent.UsbController; i < ComponentIndices.Count; i++)
                    ComponentIndices[i]--;
                break;
            }
            case KeyboardSettingsPage keyboardSettingsPage:
            {
                var index = keyboardSettingsPage.Index;
                UiHelpers.RemoveCurrentListItem<KeyboardSettingsPage>(ref ComponentList);

                _vm.Keyboards.RemoveAt(index);

                for (var i = UiComponent.Keyboard - UiComponent.UsbController; i < ComponentIndices.Count; i++)
                    ComponentIndices[i]--;
                break;
            }
            case MouseSettingsPage mouseSettingsPage:
            {
                var index = mouseSettingsPage.Index;
                UiHelpers.RemoveCurrentListItem<MouseSettingsPage>(ref ComponentList);

                _vm.Mice.RemoveAt(index);

                for (var i = UiComponent.Mouse - UiComponent.UsbController; i < ComponentIndices.Count; i++)
                    ComponentIndices[i]--;
                break;
            }
            case CustomQemuArgumentSettingsPage customQemuArgumentSettingsPage:
            {
                var index = customQemuArgumentSettingsPage.Index;
                UiHelpers.RemoveCurrentListItem<CustomQemuArgumentSettingsPage>(ref ComponentList);

                _vm.CustomQemuArguments.RemoveAt(index);

                for (var i = UiComponent.CustomQemuArgument - UiComponent.UsbController; i < ComponentIndices.Count; i++)
                    ComponentIndices[i]--;
                break;
            }
            default: throw new UnreachableException();
        }

        for (var i = ComponentTabs.SelectedIndex + 1; i < ComponentTabs.Items.Count; i++)
        {
            if (ComponentTabs.Items[i] is not TabItem tabItem) throw new UnreachableException();
            if (tabItem.Content is not ITabPage settingsPage) throw new UnreachableException();

            settingsPage.VmTabIndex--;
        }

        ComponentTabs.Items.RemoveAt(ComponentTabs.SelectedIndex);
        // We can subtract here directly because we know we won't overflow
        ComponentList.Items.RemoveAt(ComponentList.SelectedIndex--);
    }

    private void Save_OnClick(object? _, RoutedEventArgs e)
    {
        HasSaved = true;
        Close();
    }

    private async Task<bool> CheckUsbControllerUsage(ulong usbController)
    {
        var text = new StringBuilder();
        var usedCount = 0;

        for (var i = 0; i < _vm.NetworkInterfaces.Count; i++)
        {
            var networkInterface = _vm.NetworkInterfaces[i];
            if (networkInterface.Card != NetworkCard.Usb || networkInterface.UsbController != usbController) continue;

            text.AppendLine($" * Network Interface {i}");
            usedCount++;
        }

        for (var i = 0; i < _vm.SerialControllers.Count; i++)
        {
            var serialController = _vm.SerialControllers[i];
            if (serialController.Type != SerialType.Usb || serialController.UsbController != usbController) continue;

            text.AppendLine($" * Serial Controller {i}");
            usedCount++;
        }

        for (var i = 0; i < _vm.AudioControllers.Count; i++)
        {
            var audioController = _vm.AudioControllers[i];
            if (audioController.Card != SoundCard.Usb || audioController.UsbController != usbController) continue;

            text.AppendLine($" * Audio Controller {i}");
            usedCount++;
        }

        for (var i = 0; i < _vm.DiskControllers.Count; i++)
        {
            var diskController = _vm.DiskControllers[i];
            if (diskController.Model != DiskBus.Usb || diskController.UsbController != usbController) continue;

            text.AppendLine($" * Disk Controller {i}");
            usedCount++;
        }

        for (var i = 0; i < _vm.Keyboards.Count; i++)
        {
            var keyboard = _vm.Keyboards[i];
            if (keyboard.Model != KeyboardModel.Usb || keyboard.UsbController != usbController) continue;

            text.AppendLine($" * Keyboard {i}");
            usedCount++;
        }

        for (var i = 0; i < _vm.Mice.Count; i++)
        {
            var mouse = _vm.Mice[i];
            if (mouse.Model != MouseModel.Usb || mouse.UsbController != usbController) continue;

            text.AppendLine($" * Mouse {i}");
            usedCount++;
        }

        if (usedCount == 0) return true;

        text.Insert(0, "The USB controller you're trying to delete is used by the following components:\n");
        text.AppendLine("Please modify all aforementioned components before proceeding.");
        var box = MessageBoxManager.GetMessageBoxStandard("Used USB Controller Warning", text.ToString());
        await box.ShowAsync();

        return false;
    }

    private async Task<bool> CheckDiskControllerUsage(ulong controller)
    {
        var text = new StringBuilder();
        var usedCount = 0;

        for (var i = 0; i < _vm.Disks.Count; i++)
        {
            if (_vm.Disks[i].Controller != controller) continue;
            text.AppendLine($" * Disk {i}");
            usedCount++;
        }

        if (usedCount == 0) return true;

        text.Insert(0, "The disk controller you're trying to delete is used by the following disks:\n");
        text.AppendLine("Please modify all aforementioned disks before proceeding.");
        var box = MessageBoxManager.GetMessageBoxStandard("Used Disk Controller Warning", text.ToString());
        await box.ShowAsync();

        return false;
    }
}