using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using QemuSharp.Structs;
using QemuSharp.Structs.Enums;
using Tomlyn;

namespace VirtualWorkstation;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        GlobalSettings.Load();

        foreach (var vmFolder in Directory.EnumerateDirectories(GlobalSettings.VmFolder))
        {
            // VM_FOLDER/MyVM => MyVM
            var lastFolderName = Path.GetFileName(vmFolder) ?? throw new UnreachableException();
            // MyVM => MyVM.toml
            var vmConfigFileName = Path.ChangeExtension(lastFolderName, "toml");
            // MyVM.toml => VM_FOLDER/MyVM/MyVM.toml
            var vmConfigPath = Path.Combine(vmFolder, vmConfigFileName);
            if (!File.Exists(vmConfigPath)) continue;

            var vmConfigModel = Toml.ToModel<VirtualMachine>(File.ReadAllText(vmConfigPath));
            VmList.Items.Add(new ListBoxItem
            {
                Content = vmConfigModel.Name,
                Tag = new VirtualMachineTabPage(this, vmConfigPath, ref vmConfigModel)
                {
                    Index = VmList.Items.Count
                }
            });
        }

        VmList.SelectedIndex = 0;
    }

    private async void NewVm_OnClick(object? _, RoutedEventArgs e)
    {
        var newVm = new NewVirtualMachine(this);
        await newVm.ShowDialog(this);
    }

    private async void DeleteVm_OnClick(object? _, RoutedEventArgs e)
    {
        if (VmList.SelectedItem is not ListBoxItem item) throw new UnreachableException();
        if (item.Tag is not VirtualMachineTabPage vmTabPage) throw new UnreachableException();

        var box = MessageBoxManager.GetMessageBoxStandard("Delete Virtual Machine",
            $"Are you sure you want to delete the \"{vmTabPage.Vm.Name}\" virtual machine?\n" +
            "Note that this will delete everything inside the config directory, but not outside.", ButtonEnum.YesNo);
        var result = await box.ShowAsync();

        if (result != ButtonResult.Yes) return;

        for (var i = VmTabs.SelectedIndex + 1; i < VmTabs.Items.Count; i++)
        {
            if (VmTabs.Items[i] is not TabItem tabItem) throw new UnreachableException();
            if (tabItem.Content is not VirtualMachineTabPage tabPage) throw new UnreachableException();

            tabPage.VmTabIndex--;
        }

        VmTabs.Items.RemoveAt(VmTabs.SelectedIndex);
        VmList.Items.RemoveAt(VmList.SelectedIndex);
        VmList.SelectedIndex = Math.Clamp(--VmList.SelectedIndex, 0, VmList.Items.Count);

        Directory.Delete(Path.GetDirectoryName(vmTabPage.VmPath)!, true);
    }

    private async void ImportFrom_OnClick(object? _, RoutedEventArgs e)
    {
        var storageFiles = await StorageProvider.OpenFilePickerAsync(UiHelpers.FilePickerConfigOpenOptions);
        if (storageFiles.Count != 1) return;

        var importPath = storageFiles[0].Path.LocalPath;
        // IMPORT_PATH/MyVM.toml => MyVM
        var vmConfigName = Path.GetFileNameWithoutExtension(importPath);
        // MyVM => VM_FOLDER/MyVM
        var vmConfigDirectory = Path.Combine(GlobalSettings.VmFolder, vmConfigName);
        Directory.CreateDirectory(vmConfigDirectory);

        // IMPORT_PATH/MyVM.toml => MyVM.toml
        var vmConfigFileName = Path.GetFileName(importPath);
        // MyVM.toml => VM_FOLDER/MyVM/MyVM.toml
        var vmConfigPath = Path.Combine(vmConfigDirectory, vmConfigFileName);
        var vmConfigToml = await File.ReadAllTextAsync(importPath);
        var vmConfigModel = Toml.ToModel<VirtualMachine>(vmConfigToml);

        File.Copy(importPath, vmConfigPath);

        VmList.Items.Add(new ListBoxItem
        {
            Content = vmConfigModel.Name,
            Tag = new VirtualMachineTabPage(this, vmConfigPath, ref vmConfigModel)
            {
                Index = VmList.Items.Count
            }
        });
    }

    private async void ExportTo_OnClick(object? _, RoutedEventArgs e)
    {
        if (VmList.SelectedItem is not ListBoxItem item) throw new UnreachableException();
        if (item.Tag is not VirtualMachineTabPage vmTabPage) throw new UnreachableException();

        var exportVmWarning = new ExportVirtualMachineWarning();
        var vm = vmTabPage.Vm;

        if (exportVmWarning.CheckForSystemComponents(vmTabPage.Vm))
        {
            await exportVmWarning.ShowDialog(this);

            if (!exportVmWarning.ContinueExport) return;
            if (exportVmWarning.DiscardedComponents.Count != 0) ModifyVm(ref vm, exportVmWarning.DiscardedComponents);
        }

        var storageFile = await StorageProvider.SaveFilePickerAsync(UiHelpers.FilePickerConfigSaveOptions);
        if (storageFile == null) return;

        var exportPath = storageFile.Path.LocalPath;
        var vmConfigToml = Toml.FromModel(vm);

        await File.WriteAllTextAsync(exportPath, vmConfigToml);
    }

    private void Exit_OnClick(object? _, RoutedEventArgs e) => Close();

    private async void Settings_OnClick(object? _, RoutedEventArgs e)
    {
        var settings = new Settings();
        await settings.ShowDialog(this);
    }

    private async void About_OnClick(object? _, RoutedEventArgs e)
    {
        var box = MessageBoxManager.GetMessageBoxStandard("About Virtual Workstation",
            "Virtual Workstation is a free and open-source replacement for the proprietary VMware Workstation.\n" +
            "It is a GUI frontend to QEMU and is designed for maximum flexibility and guest performance, by\n" +
            "allowing every single option to be modified.\n\n" +
            "Virtual Workstation is developed by Sartox Software under the GPLv2 license. For more information,\n" +
            "check the GitHub repository: https://github.com/SartoxSoftware/VirtualWorkstation");
        await box.ShowAsync();
    }

    private void VmList_OnSelectionChanged(object? _, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count != 1) return;
        if (e.AddedItems[0] is not ListBoxItem item) throw new UnreachableException();
        if (item.Tag is not VirtualMachineTabPage vmTabPage) throw new UnreachableException();

        if (vmTabPage.Opened)
        {
            VmTabs.SelectedIndex = vmTabPage.VmTabIndex;
            return;
        }

        vmTabPage.VmTabIndex = VmTabs.Items.Count;
        vmTabPage.Opened = true;

        VmTabs.Items.Add(new TabItem { Header = item.Content, Content = item.Tag });
        VmTabs.SelectedIndex = vmTabPage.VmTabIndex;
    }

    private static void ModifyVm(ref VirtualMachine vm, List<ExportWarningComponent> discardedComponents)
    {
        // We need to create a copy of the VM in this case, so as to not modify the original
        vm = new VirtualMachine(vm);

        foreach (var component in discardedComponents)
        {
            switch (component.Type)
            {
                case ExportWarningComponentType.FirmwareCustomPath:
                {
                    vm.Firmware.CustomPath = string.Empty;
                    break;
                }
                case ExportWarningComponentType.DisplayType:
                {
                    vm.Display.Type = DisplayType.Auto;
                    break;
                }
                case ExportWarningComponentType.AudioHostType:
                {
                    vm.AudioHostDevice.Type = AudioHostType.Auto;
                    break;
                }
                case ExportWarningComponentType.DiskPath:
                {
                    vm.Disks[component.Index].Path = string.Empty;
                    break;
                }
                default: throw new UnreachableException();
            }
        }
    }
}