using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using QemuSharp.Structs;
using QemuSharp.Structs.Enums;

namespace VirtualWorkstation;

public partial class SerialControllerSettingsPage : UserControl, ITabPage, IController
{
    public bool Opened { get; set; }

    public int VmTabIndex { get; set; }

    public int Index { get; set; }

    private readonly VirtualMachine _vm;

    public SerialControllerSettingsPage(ref VirtualMachine vm, int index)
    {
        InitializeComponent();

        _vm = vm;

        Index = index;

        if (_vm.UsbControllers.Count == 0)
        {
            if (Type.Items[(int)SerialType.Usb] is not ComboBoxItem item) throw new UnreachableException();
            item.IsEnabled = false;
        }
        for (var i = 0; i < _vm.UsbControllers.Count; i++) UsbController.Items.Add(new ComboBoxItem { Content = i.ToString() });

        var type = vm.SerialControllers[index];
        Type.SelectedIndex = (int)type.Type;
        CustomType.Text = type.CustomType;
        CustomTypeBus.SelectedIndex = (int)type.CustomTypeBus.Type;
        CustomTypeBus.Text = type.CustomTypeBus.CustomType;
        UsbController.SelectedIndex = (int)type.UsbController;
    }

    private void Type_OnSelectionChanged(object? _, SelectionChangedEventArgs e)
    {
        var type = (SerialType)Type.SelectedIndex;
        _vm.SerialControllers[Index].Type = type;

        var isCustomModel = type == SerialType.Custom;
        CustomTypeGrid.IsVisible = isCustomModel;
        CustomTypeBusGrid.IsVisible = isCustomModel;

        UsbControllerGrid.IsVisible = type == SerialType.Usb;
    }

    private void CustomType_OnTextChanged(object? _, TextChangedEventArgs e)
        => _vm.SerialControllers[Index].CustomType = CustomType.Text!;

    private void CustomTypeBus_OnSelectionChanged(object? _, SelectionChangedEventArgs e)
        => _vm.SerialControllers[Index].CustomTypeBus.Type = (BusType)CustomTypeBus.SelectedIndex;

    private void CustomTypeBus_OnTextChanged(object? _, TextChangedEventArgs e)
        => _vm.SerialControllers[Index].CustomTypeBus.CustomType = CustomTypeBus.Text!;

    private void UsbController_OnSelectionChanged(object? _, SelectionChangedEventArgs e)
        => _vm.SerialControllers[Index].UsbController = (ulong)UsbController.SelectedIndex;

    public void RefreshUsbControllerList(int startIndex)
    {
        UiHelpers.RefreshControllerList(ref UsbController, startIndex);

        if (Type.Items[(int)SerialType.Usb] is not ComboBoxItem item) throw new UnreachableException();
        item.IsEnabled = UsbController.Items.Count > 0;

        if (UsbController.SelectedIndex == -1) UsbController.SelectedIndex = 0;
    }

    public void TriggerSelectionChangedEvent() => UsbController_OnSelectionChanged(null, null!);
}