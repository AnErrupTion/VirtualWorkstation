using System;
using Avalonia.Controls;
using QemuSharp.Structs.Enums;

namespace VirtualWorkstation;

public partial class DeviceBusControl : UserControl
{
    public int SelectedIndex
    {
        get => ComboBox.SelectedIndex;
        set => ComboBox.SelectedIndex = value;
    }

    public event EventHandler<SelectionChangedEventArgs>? SelectionChanged
    {
        add => ComboBox.SelectionChanged += value;
        remove => ComboBox.SelectionChanged -= value;
    }

    public string? Text
    {
        get => TextBox.Text;
        set => TextBox.Text = value;
    }

    public event EventHandler<TextChangedEventArgs>? TextChanged
    {
        add => TextBox.TextChanged += value;
        remove => TextBox.TextChanged -= value;
    }

    public DeviceBusControl() => InitializeComponent();

    private void ComboBox_OnSelectionChanged(object? _, SelectionChangedEventArgs e)
        => TextBox.IsEnabled = (BusType)SelectedIndex == BusType.Custom;
}