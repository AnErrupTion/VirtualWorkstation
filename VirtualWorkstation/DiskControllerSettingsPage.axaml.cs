using System.Diagnostics;
using Avalonia.Controls;
using QemuSharp.Structs;
using QemuSharp.Structs.Enums;

namespace VirtualWorkstation;

public partial class DiskControllerSettingsPage : UserControl, ITabPage, IController
{
    public bool Opened { get; set; }

    public int VmTabIndex { get; set; }

    public int Index { get; set; }

    private readonly VirtualMachine _vm;

    public DiskControllerSettingsPage(ref VirtualMachine vm, int index)
    {
        InitializeComponent();

        _vm = vm;

        Index = index;

        if (_vm.UsbControllers.Count == 0)
        {
            if (Model.Items[(int)DiskBus.Usb] is not ComboBoxItem item) throw new UnreachableException();
            item.IsEnabled = false;
        }
        for (var i = 0; i < _vm.UsbControllers.Count; i++) UsbController.Items.Add(new ComboBoxItem { Content = i.ToString() });

        var diskController = vm.DiskControllers[index];
        Model.SelectedIndex = (int)diskController.Model;
        CustomModel.Text = diskController.CustomModel;
        CustomModelBus.SelectedIndex = (int)diskController.CustomModelBus.Type;
        CustomModelBus.Text = diskController.CustomModelBus.CustomType;
        UsbController.SelectedIndex = (int)diskController.UsbController;
    }

    private void Model_OnSelectionChanged(object? _, SelectionChangedEventArgs e)
    {
        var model = (DiskBus)Model.SelectedIndex;
        _vm.DiskControllers[Index].Model = model;

        var isModelCustom = model == DiskBus.Custom;
        CustomModelGrid.IsVisible = isModelCustom;
        CustomModelBusGrid.IsVisible = isModelCustom;

        UsbControllerGrid.IsVisible = model == DiskBus.Usb;
    }

    private void CustomModel_OnTextChanged(object? _, TextChangedEventArgs e)
        => _vm.DiskControllers[Index].CustomModel = CustomModel.Text!;

    private void CustomModelBus_OnSelectionChanged(object? _, SelectionChangedEventArgs e)
        => _vm.DiskControllers[Index].CustomModelBus.Type = (BusType)CustomModelBus.SelectedIndex;

    private void CustomModelBus_OnTextChanged(object? _, TextChangedEventArgs e)
        => _vm.DiskControllers[Index].CustomModelBus.CustomType = CustomModelBus.Text!;

    private void UsbController_OnSelectionChanged(object? _, SelectionChangedEventArgs e)
        => _vm.DiskControllers[Index].UsbController = (ulong)UsbController.SelectedIndex;

    public void RefreshUsbControllerList(int startIndex)
    {
        UiHelpers.RefreshControllerList(ref UsbController, startIndex);

        if (Model.Items[(int)DiskBus.Usb] is not ComboBoxItem item) throw new UnreachableException();
        item.IsEnabled = UsbController.Items.Count > 0;

        if (UsbController.SelectedIndex == -1) UsbController.SelectedIndex = 0;
    }

    public void TriggerSelectionChangedEvent() => UsbController_OnSelectionChanged(null, null!);
}