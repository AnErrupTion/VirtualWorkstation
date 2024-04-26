using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using QemuSharp.Structs;
using QemuSharp.Structs.Enums;

namespace VirtualWorkstation;

public partial class ChipsetSettingsPage : UserControl, ITabPage
{
    public bool Opened { get; set; }

    public int VmTabIndex { get; set; }

    public int Index { get; set; }

    private readonly VirtualMachine _vm;

    public ChipsetSettingsPage(ref VirtualMachine vm)
    {
        InitializeComponent();

        _vm = vm;

        CheckForUnsupportedOptions();

        Model.SelectedIndex = (int)vm.Chipset.Model;
        CustomModel.Text = vm.Chipset.CustomModel;
        ForceUseNormalPci.IsChecked = vm.Chipset.ForceUseNormalPci;

        if (vm.Chipset.Q35Options != null)
        {
            Q35EnablePs2Emulation.IsChecked = vm.Chipset.Q35Options.EnablePs2Emulation;
            Q35EnableAcpi.IsChecked = vm.Chipset.Q35Options.EnableAcpi;
        }

        if (vm.Chipset.I440FxOptions != null)
        {
            I440FxEnablePs2Emulation.IsChecked = vm.Chipset.I440FxOptions.EnablePs2Emulation;
            I440FxEnableAcpi.IsChecked = vm.Chipset.I440FxOptions.EnableAcpi;
        }
    }

    private void Model_OnSelectionChanged(object? _, SelectionChangedEventArgs e)
    {
        _vm.Chipset.Model = (ChipsetModel)Model.SelectedIndex;
        CheckForUnsupportedOptions();

        var isCustomModel = _vm.Chipset.Model == ChipsetModel.Custom;
        CustomModelGrid.IsVisible = isCustomModel;
        ForceUseNormalPci.IsEnabled = isCustomModel;
        if (!isCustomModel) ForceUseNormalPci.IsChecked = _vm.Chipset.Model == ChipsetModel.X86I440Fx;

        Q35Options.IsVisible = _vm.Chipset.Model == ChipsetModel.X86Q35;
        I440FxOptions.IsVisible = _vm.Chipset.Model == ChipsetModel.X86I440Fx;
    }

    private void EnableUnsupportedOptions_OnIsCheckedChanged(object? _, RoutedEventArgs e)
        => CheckForUnsupportedOptions();

    private void CustomModel_OnTextChanged(object? _, TextChangedEventArgs e)
        => _vm.Chipset.CustomModel = CustomModel.Text!;

    private void ForceUseNormalPci_OnIsCheckedChanged(object? _, RoutedEventArgs e)
        => _vm.Chipset.ForceUseNormalPci = ForceUseNormalPci.IsChecked!.Value;

    private void Q35EnablePs2Emulation_OnIsCheckedChanged(object? _, RoutedEventArgs e)
    {
        _vm.Chipset.Q35Options ??= new Q35Options();
        _vm.Chipset.Q35Options.EnablePs2Emulation = Q35EnablePs2Emulation.IsChecked!.Value;
    }

    private void I440FxEnablePs2Emulation_OnIsCheckedChanged(object? _, RoutedEventArgs e)
    {
        _vm.Chipset.I440FxOptions ??= new I440FxOptions();
        _vm.Chipset.I440FxOptions.EnablePs2Emulation = I440FxEnablePs2Emulation.IsChecked!.Value;
    }

    private void Q35EnableAcpi_OnIsCheckedChanged(object? _, RoutedEventArgs e)
    {
        _vm.Chipset.Q35Options ??= new Q35Options();
        _vm.Chipset.Q35Options.EnableAcpi = Q35EnableAcpi.IsChecked!.Value;
    }

    private void I440FxEnableAcpi_OnIsCheckedChanged(object? _, RoutedEventArgs e)
    {
        _vm.Chipset.I440FxOptions ??= new I440FxOptions();
        _vm.Chipset.I440FxOptions.EnableAcpi = I440FxEnableAcpi.IsChecked!.Value;
    }

    private void CheckForUnsupportedOptions()
    {
        if (_vm.Architecture is Architecture.Amd64 or Architecture.I386)
        {
            EnableUnsupportedOptions.IsEnabled = false;
            EnableUnsupportedOptions.IsChecked = false;
            return;
        }

        if (_vm.Chipset.Model is >= ChipsetModel.X86Q35 and <= ChipsetModel.X86I440Fx)
        {
            EnableUnsupportedOptions.IsEnabled = false;
            EnableUnsupportedOptions.IsChecked = true;
            return;
        }

        EnableUnsupportedOptions.IsEnabled = true;

        for (var i = ChipsetModel.X86Q35; i <= ChipsetModel.X86I440Fx; i++)
        {
            if (Model.Items[(int)i] is not ComboBoxItem item) throw new UnreachableException();
            item.IsEnabled = EnableUnsupportedOptions.IsChecked!.Value;
        }
    }
}