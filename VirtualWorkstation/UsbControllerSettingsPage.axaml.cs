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
    }

    private void Version_OnSelectionChanged(object? _, SelectionChangedEventArgs e)
    {
        var newVersion = (UsbVersion)Version.SelectedIndex;
        _vm.UsbControllers[Index].Version = newVersion;

        CustomVersion.IsEnabled = newVersion == UsbVersion.Custom;
    }

    private void CustomVersion_OnTextChanged(object? _, TextChangedEventArgs e)
        => _vm.UsbControllers[Index].CustomVersion = CustomVersion.Text!;
}