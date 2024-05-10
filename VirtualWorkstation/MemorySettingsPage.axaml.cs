using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using QemuSharp.Structs;

namespace VirtualWorkstation;

public partial class MemorySettingsPage : UserControl, ITabPage
{
    public bool Opened { get; set; }

    public int VmTabIndex { get; set; }

    public int Index { get; set; }

    private readonly VirtualMachine _vm;

    public MemorySettingsPage(ref VirtualMachine vm)
    {
        InitializeComponent();

        _vm = vm;

        var memoryInfo = GC.GetGCMemoryInfo();
        var ramMiB = memoryInfo.TotalAvailableMemoryBytes / 1024 / 1024;
        Ram.Maximum = ramMiB;

        Ram.Value = vm.Memory.Size;
        UseMemoryBallooning.IsChecked = vm.Memory.UseBallooning;
        MemorySharing.IsChecked = vm.Memory.MemorySharing;
    }

    private void Ram_OnValueChanged(object? _, NumericUpDownValueChangedEventArgs e)
        => _vm.Memory.Size = (ulong)e.NewValue!.Value;

    private void UseMemoryBallooning_OnIsCheckedChanged(object? _, RoutedEventArgs e)
        => _vm.Memory.UseBallooning = UseMemoryBallooning.IsChecked!.Value;

    private void MemorySharing_OnIsCheckedChanged(object? _, RoutedEventArgs e)
        => _vm.Memory.MemorySharing = MemorySharing.IsChecked!.Value;
}