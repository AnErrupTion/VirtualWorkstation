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
    }

    private void Model_OnSelectionChanged(object? _, SelectionChangedEventArgs e)
    {
        _vm.Chipset.Model = (ChipsetModel)Model.SelectedIndex;
        CheckForUnsupportedOptions();
    }

    private void EnableUnsupportedOptions_OnIsCheckedChanged(object? _, RoutedEventArgs e)
        => CheckForUnsupportedOptions();

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