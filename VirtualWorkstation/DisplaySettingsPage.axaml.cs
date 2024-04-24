using System;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using QemuSharp.Structs;
using QemuSharp.Structs.Enums;

namespace VirtualWorkstation;

public partial class DisplaySettingsPage : UserControl, ITabPage
{
    public bool Opened { get; set; }

    public int VmTabIndex { get; set; }

    public int Index { get; set; }

    private readonly VirtualMachine _vm;

    public DisplaySettingsPage(ref VirtualMachine vm)
    {
        InitializeComponent();

        _vm = vm;
        
        CheckForUnsupportedOptions();

        Type.SelectedIndex = (int)vm.Display.Type;
    }

    private void Type_OnSelectionChanged(object? _, SelectionChangedEventArgs e)
    {
        _vm.Display.Type = (DisplayType)Type.SelectedIndex;
        CheckForUnsupportedOptions();
    }

    private void EnableUnsupportedOptions_OnIsCheckedChanged(object? _, RoutedEventArgs e)
        => CheckForUnsupportedOptions();

    private void CheckForUnsupportedOptions()
    {
        if (OperatingSystem.IsWindows() && _vm.Display.Type is DisplayType.Cocoa or DisplayType.DBus
            || OperatingSystem.IsMacOS() && _vm.Display.Type is DisplayType.Gtk or DisplayType.DBus
            || (OperatingSystem.IsLinux() || OperatingSystem.IsFreeBSD()) && _vm.Display.Type is DisplayType.Cocoa)
        {
            EnableUnsupportedOptions.IsEnabled = false;
            EnableUnsupportedOptions.IsChecked = true;
            return;
        }

        EnableUnsupportedOptions.IsEnabled = true;

        if (OperatingSystem.IsWindows())
        {
            for (var i = DisplayType.Cocoa; i <= DisplayType.DBus; i++) SetOption(i);
        }
        else if (OperatingSystem.IsMacOS())
        {
            SetOption(DisplayType.Gtk);
            SetOption(DisplayType.DBus);
        }
        else if (OperatingSystem.IsLinux() || OperatingSystem.IsFreeBSD())
        {
            SetOption(DisplayType.Cocoa);
        }
    }

    private void SetOption(DisplayType type)
    {
        if (Type.Items[(int)type] is not ComboBoxItem item) throw new UnreachableException();
        item.IsEnabled = EnableUnsupportedOptions.IsChecked!.Value;
    }
}