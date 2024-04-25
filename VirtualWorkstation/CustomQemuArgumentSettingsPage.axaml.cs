using Avalonia.Controls;
using Avalonia.Interactivity;
using QemuSharp.Structs;
using QemuSharp.Structs.Enums;

namespace VirtualWorkstation;

public partial class CustomQemuArgumentSettingsPage : UserControl, ITabPage
{
    public bool Opened { get; set; }

    public int VmTabIndex { get; set; }

    public int Index
    {
        get => _index;
        set => _index = value;
    }

    private readonly VirtualMachine _vm;
    private int _index;

    public CustomQemuArgumentSettingsPage(ref VirtualMachine vm, int index)
    {
        InitializeComponent();

        _vm = vm;
        _index = index;

        var customQemuArgument = vm.CustomQemuArguments[index];
        Type.SelectedIndex = (int)customQemuArgument.Type;
        Value.Text = customQemuArgument.Value;

        for (var i = 0; i < customQemuArgument.Parameters.Count; i++) Parameters.Children.Add(
            new CustomQemuArgumentParameterControl(_vm.CustomQemuArguments, ref _index,
                Parameters.Children, i));
    }

    private void Type_OnSelectionChanged(object? _, SelectionChangedEventArgs e)
    {
        var type = (CustomQemuArgumentType)Type.SelectedIndex;
        _vm.CustomQemuArguments[Index].Type = type;

        Value.IsEnabled = type < CustomQemuArgumentType.Drive;
    }

    private void Value_OnTextChanged(object? _, TextChangedEventArgs e)
        => _vm.CustomQemuArguments[Index].Value = Value.Text!;

    private void AddParameter_OnClick(object? _, RoutedEventArgs e)
    {
        var parameters = _vm.CustomQemuArguments[Index].Parameters;
        parameters.Add(new CustomQemuArgumentParameter());

        Parameters.Children.Add(new CustomQemuArgumentParameterControl(_vm.CustomQemuArguments,
            ref _index, Parameters.Children, parameters.Count - 1));
    }
}