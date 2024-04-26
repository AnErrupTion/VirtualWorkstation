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
        CustomDeviceType.Text = vm.TrustedPlatformModule.CustomDeviceType;
        Type.SelectedIndex = (int)vm.TrustedPlatformModule.Type;
        CustomType.Text = vm.TrustedPlatformModule.CustomType;
        CustomTypeBus.SelectedIndex = (int)vm.TrustedPlatformModule.CustomTypeBus.Type;
        CustomTypeBus.Text = vm.TrustedPlatformModule.CustomTypeBus.CustomType;
    }

    private void DeviceType_OnSelectionChanged(object? _, SelectionChangedEventArgs e)
    {
        var deviceType = (TpmDeviceType)DeviceType.SelectedIndex;
        _vm.TrustedPlatformModule.DeviceType = deviceType;

        CustomDeviceTypeGrid.IsVisible = deviceType == TpmDeviceType.Custom;

        var isTpmPresent = deviceType != TpmDeviceType.None;
        TypeGrid.IsVisible = isTpmPresent;

        if (!isTpmPresent)
        {
            CustomTypeGrid.IsVisible = false;
            CustomTypeBusGrid.IsVisible = false;
            return;
        }

        if (Type.SelectedIndex != -1) Type_OnSelectionChanged(null, null!);
    }

    private void CustomDeviceType_OnTextChanged(object? _, TextChangedEventArgs e)
        => _vm.TrustedPlatformModule.CustomDeviceType = CustomDeviceType.Text!;

    private void Type_OnSelectionChanged(object? _, SelectionChangedEventArgs e)
    {
        var type = (TpmType)Type.SelectedIndex;
        _vm.TrustedPlatformModule.Type = type;

        var isTypeCustom = type == TpmType.Custom;
        CustomTypeGrid.IsVisible = isTypeCustom;
        CustomTypeBusGrid.IsVisible = isTypeCustom;
    }

    private void CustomType_OnTextChanged(object? _, TextChangedEventArgs e)
        => _vm.TrustedPlatformModule.CustomType = CustomType.Text!;

    private void CustomTypeBus_OnSelectionChanged(object? _, SelectionChangedEventArgs e)
        => _vm.TrustedPlatformModule.CustomTypeBus.Type = (BusType)CustomTypeBus.SelectedIndex;

    private void CustomTypeBus_OnTextChanged(object? _, TextChangedEventArgs e)
        => _vm.TrustedPlatformModule.CustomTypeBus.CustomType = CustomTypeBus.Text!;
}