using System.Collections.Generic;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using QemuSharp.Structs;

namespace VirtualWorkstation;

public partial class CustomQemuArgumentParameterControl : UserControl
{
    private readonly List<CustomQemuArgument> _arguments;
    private readonly Controls _parameterControls;
    private readonly int _argumentIndex;
    private int _index;

    public CustomQemuArgumentParameterControl(List<CustomQemuArgument> arguments, ref int argumentIndex, Controls parameterControls, int index)
    {
        InitializeComponent();

        _arguments = arguments;
        _parameterControls = parameterControls;
        _argumentIndex = argumentIndex;
        _index = index;

        var parameter = arguments[argumentIndex].Parameters[_index];
        Key.Text = parameter.Key;
        Value.Text = parameter.Value;
    }

    private void Key_OnTextChanged(object? _, TextChangedEventArgs e)
        => _arguments[_argumentIndex].Parameters[_index].Key = Key.Text!;

    private void Value_OnTextChanged(object? _, TextChangedEventArgs e)
        => _arguments[_argumentIndex].Parameters[_index].Value = Value.Text!;

    private void Remove_OnClick(object? _, RoutedEventArgs e)
    {
        var parameterControlIndex = _index + 1; // Our first child is the "Add" button so we must skip it

        _arguments[_argumentIndex].Parameters.RemoveAt(_index);
        _parameterControls.RemoveAt(parameterControlIndex);

        for (var i = parameterControlIndex; i < _parameterControls.Count; i++)
        {
            if (_parameterControls[i] is not CustomQemuArgumentParameterControl parameterControl) throw new UnreachableException();
            parameterControl._index--;
        }
    }
}