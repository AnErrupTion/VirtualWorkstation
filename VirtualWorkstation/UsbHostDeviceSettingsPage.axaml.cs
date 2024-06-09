using System.Diagnostics;
using Avalonia.Controls;
using QemuSharp;
using QemuSharp.Structs;

namespace VirtualWorkstation;

public partial class UsbHostDeviceSettingsPage : UserControl, ITabPage, IController
{
    public bool Opened { get; set; }

    public int VmTabIndex { get; set; }

    public int Index { get; set; }

    private readonly VirtualMachine _vm;

    public UsbHostDeviceSettingsPage(ref VirtualMachine vm, int index)
    {
        InitializeComponent();

        _vm = vm;

        Index = index;

        for (var i = 0; i < _vm.UsbControllers.Count; i++) UsbController.Items.Add(new ComboBoxItem { Content = i.ToString() });

        var usbHostDevice = vm.UsbHostDevices[index];
        var (_, usbDevices) = HostDeviceManager.GetUsbDevices();
        var selectedUsbDeviceIndex = -1;

        for (var i = 0; i < usbDevices!.Count; i++)
        {
            var usbDevice = usbDevices[i];
            if (usbDevice.VendorId == usbHostDevice.VendorId && usbDevice.ProductId == usbHostDevice.ProductId)
                selectedUsbDeviceIndex = i;

            UsbDevice.Items.Add(new ListBoxItem
            {
                Content = $"{usbDevice.VendorId:X4}:{usbDevice.ProductId:X4} {usbDevice.Name}",
                Tag = usbDevice
            });
        }

        UsbDevice.SelectedIndex = selectedUsbDeviceIndex;
        UsbController.SelectedIndex = (int)usbHostDevice.UsbController;
    }

    private void UsbDevice_OnSelectionChanged(object? _, SelectionChangedEventArgs e)
    {
        if (UsbDevice.SelectedItem is not ListBoxItem item) throw new UnreachableException();
        if (item.Tag is not UsbDevice usbDevice) throw new UnreachableException();

        _vm.UsbHostDevices[Index].VendorId = usbDevice.VendorId;
        _vm.UsbHostDevices[Index].ProductId = usbDevice.ProductId;
    }

    private void UsbController_OnSelectionChanged(object? _, SelectionChangedEventArgs e)
        => _vm.UsbHostDevices[Index].UsbController = (ulong)UsbController.SelectedIndex;

    public void RefreshUsbControllerList(int startIndex)
    {
        UiHelpers.RefreshControllerList(ref UsbController, startIndex);

        if (UsbController.SelectedIndex == -1) UsbController.SelectedIndex = 0;
    }

    public void TriggerSelectionChangedEvent() => UsbController_OnSelectionChanged(null, null!);
}