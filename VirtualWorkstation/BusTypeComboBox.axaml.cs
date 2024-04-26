using System;
using Avalonia.Controls;

namespace VirtualWorkstation;

public partial class BusTypeComboBox : UserControl
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

    public BusTypeComboBox() => InitializeComponent();
}