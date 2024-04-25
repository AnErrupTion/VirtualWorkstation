using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using QemuSharp.Structs;
using QemuSharp.Structs.Enums;

namespace VirtualWorkstation;

public partial class FirmwareSettingsPage : UserControl, ITabPage
{
    public bool Opened { get; set; }

    public int VmTabIndex { get; set; }

    public int Index { get; set; }

    private readonly VirtualMachine _vm;

    public FirmwareSettingsPage(ref VirtualMachine vm)
    {
        InitializeComponent();

        _vm = vm;

        CheckForUnsupportedOptions();

        Type.SelectedIndex = (int)vm.Firmware.Type;
        CustomPath.Text = vm.Firmware.CustomPath;
    }

    private void Type_OnSelectionChanged(object? _, SelectionChangedEventArgs e)
    {
        _vm.Firmware.Type = (FirmwareType)Type.SelectedIndex;
        CheckForUnsupportedOptions();

        var isCustomType = _vm.Firmware.Type is FirmwareType.CustomPFlash or FirmwareType.X86CustomPc;
        CustomPath.IsEnabled = isCustomType;
        Browse.IsEnabled = isCustomType;
    }

    private void EnableUnsupportedOptions_OnIsCheckedChanged(object? _, RoutedEventArgs e)
        => CheckForUnsupportedOptions();

    private void CustomPath_OnTextChanged(object? _, TextChangedEventArgs e)
        => _vm.Firmware.CustomPath = CustomPath.Text!;

    private async void Browse_OnClick(object? _, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this) ?? throw new UnreachableException();
        var storageFiles = await topLevel.StorageProvider.OpenFilePickerAsync(UiHelpers.DefaultFilePickerOpenOptions);

        if (storageFiles.Count != 1) return;

        CustomPath.Text = storageFiles[0].Path.LocalPath;
    }

    private void CheckForUnsupportedOptions()
    {
        if (_vm.Architecture is Architecture.Amd64 or Architecture.I386)
        {
            EnableUnsupportedOptions.IsEnabled = false;
            EnableUnsupportedOptions.IsChecked = false;
            return;
        }

        if (_vm.Firmware.Type is >= FirmwareType.X86LegacyBios and <= FirmwareType.X86CustomPc)
        {
            EnableUnsupportedOptions.IsEnabled = false;
            EnableUnsupportedOptions.IsChecked = true;
            return;
        }

        EnableUnsupportedOptions.IsEnabled = true;

        for (var i = FirmwareType.X86LegacyBios; i <= FirmwareType.X86CustomPc; i++)
        {
            if (Type.Items[(int)i] is not ComboBoxItem item) throw new UnreachableException();
            item.IsEnabled = EnableUnsupportedOptions.IsChecked!.Value;
        }
    }
}