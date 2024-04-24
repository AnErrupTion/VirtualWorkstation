using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using QemuSharp.Structs;
using QemuSharp.Structs.Enums;

namespace VirtualWorkstation;

public partial class MouseSettingsPage : UserControl, ITabPage, IController
{
    public bool Opened { get; set; }

    public int VmTabIndex { get; set; }

    public int Index { get; set; }

    private readonly VirtualMachine _vm;

    public MouseSettingsPage(ref VirtualMachine vm, int index)
    {
        InitializeComponent();

        _vm = vm;

        Index = index;

        if (_vm.UsbControllers.Count == 0)
        {
            if (Model.Items[(int)MouseModel.Usb] is not ComboBoxItem item) throw new UnreachableException();
            item.IsEnabled = false;

            UsbController.IsEnabled = false;
        }
        for (var i = 0; i < _vm.UsbControllers.Count; i++) UsbController.Items.Add(new ComboBoxItem { Content = i.ToString() });

        var mouse = vm.Mice[index];
        Model.SelectedIndex = (int)mouse.Model;
        UsbController.SelectedIndex = (int)mouse.UsbController;
        UseAbsolutePointing.IsChecked = mouse.UseAbsolutePointing;
    }

    private void Model_OnSelectionChanged(object? _, SelectionChangedEventArgs e)
    {
        var model = (MouseModel)Model.SelectedIndex;

        UsbController.IsEnabled = model == MouseModel.Usb;
        _vm.Mice[Index].Model = model;
    }

    private void UsbController_OnSelectionChanged(object? _, SelectionChangedEventArgs e)
        => _vm.Mice[Index].UsbController = (ulong)UsbController.SelectedIndex;

    private void UseAbsolutePointing_OnIsCheckedChanged(object? _, RoutedEventArgs e)
        => _vm.Mice[Index].UseAbsolutePointing = UseAbsolutePointing.IsChecked!.Value;

    public void RefreshUsbControllerList(int startIndex) => UiHelpers.RefreshControllerList(ref UsbController, startIndex);

    public void TriggerSelectionChangedEvent() => UsbController_OnSelectionChanged(null, null!);
}