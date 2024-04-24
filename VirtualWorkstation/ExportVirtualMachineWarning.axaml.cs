using System.Collections.Generic;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using QemuSharp.Structs;
using QemuSharp.Structs.Enums;

namespace VirtualWorkstation;

public partial class ExportVirtualMachineWarning : Window
{
    public bool ContinueExport { get; private set; }

    public List<ExportWarningComponent> DiscardedComponents { get; } = [];

    public ExportVirtualMachineWarning() => InitializeComponent();

    public bool CheckForSystemComponents(VirtualMachine vm)
    {
        Title += vm.Name;

        var hasSystemComponents = false;

        if (!string.IsNullOrEmpty(vm.Firmware.CustomPath))
        {
            ComponentTree.Items.Add(new TreeViewItem
            {
                Header = new CheckBox { Content = "Firmware > Custom path" },
                Items = { new TreeViewItem { Header = "The custom firmware path may be dependent on the host OS and/or its location. If unchecked, it will be emptied." } },
                Tag = new ExportWarningComponent(ExportWarningComponentType.FirmwareCustomPath, -1)
            });

            hasSystemComponents = true;
        }

        if (vm.Display.Type != DisplayType.Auto)
        {
            ComponentTree.Items.Add(new TreeViewItem
            {
                Header = new CheckBox { Content = "Display > Type", IsChecked = true },
                Items = { new TreeViewItem { Header = "The display type is dependent on the host OS (it isn't Auto). If unchecked, it will be reverted to Auto." } },
                Tag = new ExportWarningComponent(ExportWarningComponentType.DisplayType, -1)
            });

            hasSystemComponents = true;
        }

        if (vm.AudioHostDevice.Type != AudioHostType.Auto)
        {
            ComponentTree.Items.Add(new TreeViewItem
            {
                Header = new CheckBox { Content = "Audio Host > Type", IsChecked = true },
                Items = { new TreeViewItem { Header = "The audio host type is dependent on the host OS (it isn't Auto). If unchecked, it will be reverted to Auto." } },
                Tag = new ExportWarningComponent(ExportWarningComponentType.AudioHostType, -1)
            });

            hasSystemComponents = true;
        }

        for (var i = 0; i < vm.Disks.Count; i++)
        {
            var disk = vm.Disks[i];
            if (string.IsNullOrEmpty(disk.Path)) continue;

            ComponentTree.Items.Add(new TreeViewItem
            {
                Header = new CheckBox { Content = $"Disk {i} > Path", IsChecked = true },
                Items = { new TreeViewItem { Header = "The disk path may be dependent on the host OS and/or its location. If unchecked, it will be emptied." } },
                Tag = new ExportWarningComponent(ExportWarningComponentType.DiskPath, i)
            });

            hasSystemComponents = true;
        }

        return hasSystemComponents;
    }

    private void Continue_OnClick(object? _, RoutedEventArgs e)
    {
        foreach (var obj in ComponentTree.Items)
        {
            if (obj is not TreeViewItem item) throw new UnreachableException();
            if (item.Header is not CheckBox checkBox) throw new UnreachableException();
            if (item.Tag is not ExportWarningComponent component) throw new UnreachableException();

            if (!checkBox.IsChecked!.Value) DiscardedComponents.Add(component);
        }

        ContinueExport = true;
        Close();
    }
}