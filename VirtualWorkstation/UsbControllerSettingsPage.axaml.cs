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

        Version.SelectedIndex = (int)vm.UsbControllers[index].Version;
    }

    private void Version_OnSelectionChanged(object? _, SelectionChangedEventArgs e)
        => _vm.UsbControllers[Index].Version = (UsbVersion)Version.SelectedIndex;
}