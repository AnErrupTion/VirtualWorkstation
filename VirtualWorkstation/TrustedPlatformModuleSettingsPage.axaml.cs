using Avalonia.Controls;
using QemuSharp.Structs;
using QemuSharp.Structs.Enums;

namespace VirtualWorkstation;

public partial class TrustedPlatformModuleSettingsPage : UserControl, ITabPage
{
    public bool Opened { get; set; }

    public int VmTabIndex { get; set; }

    public int Index { get; set; }

    private readonly VirtualMachine _vm;

    public TrustedPlatformModuleSettingsPage(ref VirtualMachine vm)
    {
        InitializeComponent();

        _vm = vm;

        DeviceType.SelectedIndex = (int)vm.TrustedPlatformModule.DeviceType;
        Type.SelectedIndex = (int)vm.TrustedPlatformModule.Type;
    }

    private void DeviceType_OnSelectionChanged(object? _, SelectionChangedEventArgs e)
    {
        var deviceType = (TpmDeviceType)DeviceType.SelectedIndex;
        _vm.TrustedPlatformModule.DeviceType = deviceType;

        Type.IsEnabled = deviceType != TpmDeviceType.None;
    }

    private void Type_OnSelectionChanged(object? _, SelectionChangedEventArgs e)
        => _vm.TrustedPlatformModule.Type = (TpmType)Type.SelectedIndex;
}