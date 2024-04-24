using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using QemuSharp.Structs;
using QemuSharp.Structs.Enums;

namespace VirtualWorkstation;

public partial class SystemSettingsPage : UserControl, ITabPage
{
    public bool Opened { get; set; }

    public int VmTabIndex { get; set; }

    public int Index { get; set; }

    private readonly VirtualMachine _vm;

    public SystemSettingsPage(ref VirtualMachine vm)
    {
        InitializeComponent();

        _vm = vm;

        var memoryInfo = GC.GetGCMemoryInfo();
        var ramMiB = memoryInfo.TotalAvailableMemoryBytes / 1024 / 1024;
        Ram.Maximum = ramMiB;

        Name.Text = vm.Name;
        Architecture.SelectedIndex = (int)vm.Architecture;
        UseHardwareAcceleration.IsChecked = vm.UseHardwareAcceleration;
        Ram.Value = vm.Ram;
    }

    private void Name_OnTextChanged(object? _, TextChangedEventArgs e)
        => _vm.Name = Name.Text!;

    private void Architecture_OnSelectionChanged(object? _, SelectionChangedEventArgs e)
        => _vm.Architecture = (Architecture)Architecture.SelectedIndex;
    
    private void UseHardwareAcceleration_OnIsCheckedChanged(object? _, RoutedEventArgs e) 
        => _vm.UseHardwareAcceleration = UseHardwareAcceleration.IsChecked!.Value;

    private void Ram_OnValueChanged(object? _, NumericUpDownValueChangedEventArgs e)
        => _vm.Ram = (ulong)e.NewValue!.Value;
}