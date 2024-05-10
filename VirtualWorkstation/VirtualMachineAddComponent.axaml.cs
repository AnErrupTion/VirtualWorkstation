using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using QemuSharp.Structs;

namespace VirtualWorkstation;

public partial class VirtualMachineAddComponent : Window
{
    private VirtualMachine _vm;
    private readonly VirtualMachineSettings _parent;

    public VirtualMachineAddComponent(ref VirtualMachine vm, VirtualMachineSettings parent)
    {
        InitializeComponent();

        Title += vm.Name;

        _vm = vm;
        _parent = parent;
    }

    private void Add_OnClick(object? _, RoutedEventArgs e)
    {
        var listIndex = _parent.ComponentIndices[ComponentList.SelectedIndex];

        // Adding "UsbController" allows us to offset the selected index to start from "UsbController" (we can't add
        // System, Firmware, etc...)
        switch (ComponentList.SelectedIndex + UiComponent.UsbController)
        {
            case UiComponent.UsbController:
            {
                var index = _vm.UsbControllers.Count;

                _vm.UsbControllers.Add(new UsbController());
                _parent.ComponentList.Items.Insert(listIndex, new ListBoxItem
                {
                    Content = $"{UiComponent.UsbController.ToUiString()} {index}",
                    Tag = new UsbControllerSettingsPage(ref _vm, index)
                });

                for (var i = ComponentList.SelectedIndex; i < _parent.ComponentIndices.Count; i++)
                    _parent.ComponentIndices[i]++;

                UiHelpers.RefreshUsbControllerLists(ref _parent.ComponentList, _parent.ComponentIndices, ref _vm, -1);
                break;
            }
            case UiComponent.NetworkInterface:
            {
                var index = _vm.NetworkInterfaces.Count;

                _vm.NetworkInterfaces.Add(new NetworkInterface());
                _parent.ComponentList.Items.Insert(listIndex, new ListBoxItem
                {
                    Content = $"{UiComponent.NetworkInterface.ToUiString()} {index}",
                    Tag = new NetworkInterfaceSettingsPage(ref _vm, index)
                });

                for (var i = ComponentList.SelectedIndex; i < _parent.ComponentIndices.Count; i++)
                    _parent.ComponentIndices[i]++;
                break;
            }
            case UiComponent.GraphicsController:
            {
                var index = _vm.GraphicControllers.Count;

                _vm.GraphicControllers.Add(new GraphicsController());
                _parent.ComponentList.Items.Insert(listIndex, new ListBoxItem
                {
                    Content = $"{UiComponent.GraphicsController.ToUiString()} {index}",
                    Tag = new GraphicsControllerSettingsPage(ref _vm, index)
                });

                for (var i = ComponentList.SelectedIndex; i < _parent.ComponentIndices.Count; i++)
                    _parent.ComponentIndices[i]++;
                break;
            }
            case UiComponent.SerialController:
            {
                var index = _vm.SerialControllers.Count;

                _vm.SerialControllers.Add(new SerialController());
                _parent.ComponentList.Items.Insert(listIndex, new ListBoxItem
                {
                    Content = $"{UiComponent.SerialController.ToUiString()} {index}",
                    Tag = new SerialControllerSettingsPage(ref _vm, index)
                });

                for (var i = ComponentList.SelectedIndex; i < _parent.ComponentIndices.Count; i++)
                    _parent.ComponentIndices[i]++;
                break;
            }
            case UiComponent.AudioController:
            {
                var index = _vm.AudioControllers.Count;

                _vm.AudioControllers.Add(new AudioController());
                _parent.ComponentList.Items.Insert(listIndex, new ListBoxItem
                {
                    Content = $"{UiComponent.AudioController.ToUiString()} {index}",
                    Tag = new AudioControllerSettingsPage(ref _vm, index)
                });

                for (var i = ComponentList.SelectedIndex; i < _parent.ComponentIndices.Count; i++)
                    _parent.ComponentIndices[i]++;
                break;
            }
            case UiComponent.DiskController:
            {
                var index = _vm.DiskControllers.Count;

                _vm.DiskControllers.Add(new DiskController());
                _parent.ComponentList.Items.Insert(listIndex, new ListBoxItem
                {
                    Content = $"{UiComponent.DiskController.ToUiString()} {index}",
                    Tag = new DiskControllerSettingsPage(ref _vm, index)
                });

                for (var i = ComponentList.SelectedIndex; i < _parent.ComponentIndices.Count; i++)
                    _parent.ComponentIndices[i]++;

                UiHelpers.RefreshDiskControllerLists(ref _parent.ComponentList, _parent.ComponentIndices, ref _vm, -1);
                break;
            }
            case UiComponent.Disk:
            {
                var index = _vm.Disks.Count;

                _vm.Disks.Add(new Disk());
                _parent.ComponentList.Items.Insert(listIndex, new ListBoxItem
                {
                    Content = $"{UiComponent.Disk.ToUiString()} {index}",
                    Tag = new DiskSettingsPage(ref _vm, index)
                });

                for (var i = ComponentList.SelectedIndex; i < _parent.ComponentIndices.Count; i++)
                    _parent.ComponentIndices[i]++;
                break;
            }
            case UiComponent.Keyboard:
            {
                var index = _vm.Keyboards.Count;

                _vm.Keyboards.Add(new Keyboard());
                _parent.ComponentList.Items.Insert(listIndex, new ListBoxItem
                {
                    Content = $"{UiComponent.Keyboard.ToUiString()} {index}",
                    Tag = new KeyboardSettingsPage(ref _vm, index)
                });

                for (var i = ComponentList.SelectedIndex; i < _parent.ComponentIndices.Count; i++)
                    _parent.ComponentIndices[i]++;
                break;
            }
            case UiComponent.Mouse:
            {
                var index = _vm.Mice.Count;

                _vm.Mice.Add(new Mouse());
                _parent.ComponentList.Items.Insert(listIndex, new ListBoxItem
                {
                    Content = $"{UiComponent.Mouse.ToUiString()} {index}",
                    Tag = new MouseSettingsPage(ref _vm, index)
                });

                for (var i = ComponentList.SelectedIndex; i < _parent.ComponentIndices.Count; i++)
                    _parent.ComponentIndices[i]++;
                break;
            }
            case UiComponent.SharedFolder:
            {
                var index = _vm.SharedFolders.Count;

                _vm.SharedFolders.Add(new SharedFolder());
                _parent.ComponentList.Items.Insert(listIndex, new ListBoxItem
                {
                    Content = $"{UiComponent.SharedFolder.ToUiString()} {index}",
                    Tag = new SharedFolderSettingsPage(ref _vm, index)
                });

                for (var i = ComponentList.SelectedIndex; i < _parent.ComponentIndices.Count; i++)
                    _parent.ComponentIndices[i]++;
                break;
            }
            case UiComponent.CustomQemuArgument:
            {
                var index = _vm.CustomQemuArguments.Count;

                _vm.CustomQemuArguments.Add(new CustomQemuArgument());
                _parent.ComponentList.Items.Insert(listIndex, new ListBoxItem
                {
                    Content = $"{UiComponent.CustomQemuArgument.ToUiString()} {index}",
                    Tag = new CustomQemuArgumentSettingsPage(ref _vm, index)
                });

                for (var i = ComponentList.SelectedIndex; i < _parent.ComponentIndices.Count; i++)
                    _parent.ComponentIndices[i]++;
                break;
            }
            default: throw new UnreachableException();
        }

        _parent.ComponentList.SelectedIndex = listIndex;

        Close();
    }
}