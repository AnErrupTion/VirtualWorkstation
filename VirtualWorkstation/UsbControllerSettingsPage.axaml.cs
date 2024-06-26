using Avalonia.Controls;
using QemuSharp.Structs;
using QemuSharp.Structs.Enums;

namespace VirtualWorkstation;

public partial class UsbControllerSettingsPage : UserControl, ITabPage
{
    public bool Opened { get; set; }

    public int VmTabIndex { get; set; }

    public int Index { get; set; }

    private readonly VirtualMachine _vm;

    public UsbControllerSettingsPage(ref VirtualMachine vm, int index)
    {
        InitializeComponent();

        _vm = vm;

        Index = index;

        var usbController = vm.UsbControllers[index];
        Version.SelectedIndex = (int)usbController.Version;
        CustomVersion.Text = usbController.CustomVersion;
        CustomVersionBus.SelectedIndex = (int)usbController.CustomVersionBus.Type;
        CustomVersionBus.Text = usbController.CustomVersionBus.CustomType;
    }

    private void Version_OnSelectionChanged(object? _, SelectionChangedEventArgs e)
    {
        var version = (UsbVersion)Version.SelectedIndex;
        _vm.UsbControllers[Index].Version = version;

        var isVersionCustom = version == UsbVersion.Custom;
        CustomVersionGrid.IsVisible = isVersionCustom;
        CustomVersionBusGrid.IsVisible = isVersionCustom;
    }

    private void CustomVersion_OnTextChanged(object? _, TextChangedEventArgs e)
        => _vm.UsbControllers[Index].CustomVersion = CustomVersion.Text!;

    private void CustomVersionBus_OnSelectionChanged(object? _, SelectionChangedEventArgs e)
        => _vm.UsbControllers[Index].CustomVersionBus.Type = (BusType)CustomVersionBus.SelectedIndex;

    private void CustomVersionBus_OnTextChanged(object? _, TextChangedEventArgs e)
        => _vm.UsbControllers[Index].CustomVersionBus.CustomType = CustomVersionBus.Text!;
}