using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using QemuSharp.Structs;
using QemuSharp.Structs.Enums;

namespace VirtualWorkstation;

public partial class DiskSettingsPage : UserControl, ITabPage
{
    public bool Opened { get; set; }

    public int VmTabIndex { get; set; }

    public int Index { get; set; }

    private readonly VirtualMachine _vm;

    public DiskSettingsPage(ref VirtualMachine vm, int index)
    {
        InitializeComponent();

        _vm = vm;

        Index = index;

        for (var i = 0; i < vm.DiskControllers.Count; i++) Controller.Items.Add(new ComboBoxItem { Content = i.ToString() });

        var disk = vm.Disks[index];
        Controller.SelectedIndex = (int)disk.Controller;
        Format.SelectedIndex = (int)disk.Format;
        CacheMethod.SelectedIndex = (int)disk.CacheMethod;
        Path.Text = disk.Path;
        IsSsd.IsChecked = disk.IsSsd;
        IsCdrom.IsChecked = disk.IsCdrom;
        IsRemovable.IsChecked = disk.IsRemovable;
    }

    private void Controller_OnSelectionChanged(object? _, SelectionChangedEventArgs e)
    {
        _vm.Disks[Index].Controller = (ulong)Controller.SelectedIndex;

        var diskController = _vm.DiskControllers[Controller.SelectedIndex];
        switch (diskController.Model)
        {
            case DiskBus.Ide:
            case DiskBus.Sata:
            {
                IsCdrom.IsEnabled = true;
                IsRemovable.IsEnabled = false;
                IsRemovable.IsChecked = false;
                break;
            }
            case DiskBus.Nvme:
            case DiskBus.VirtIo:
            case DiskBus.Custom:
            {
                IsCdrom.IsEnabled = false;
                IsCdrom.IsChecked = false;
                IsRemovable.IsEnabled = false;
                IsRemovable.IsChecked = false;
                break;
            }
            case DiskBus.Usb:
            {
                IsCdrom.IsEnabled = false;
                IsRemovable.IsEnabled = true;
                break;
            }
            default: throw new UnreachableException();
        }
    }

    private void Format_OnSelectionChanged(object? _, SelectionChangedEventArgs e)
        => _vm.Disks[Index].Format = (DiskFormat)Format.SelectedIndex;

    private void CacheMethod_OnSelectionChanged(object? _, SelectionChangedEventArgs e)
        => _vm.Disks[Index].CacheMethod = (DiskCacheMethod)CacheMethod.SelectedIndex;

    private void Path_OnTextChanged(object? _, TextChangedEventArgs e)
        => _vm.Disks[Index].Path = Path.Text!;

    private async void Browse_OnClick(object? _, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this) ?? throw new UnreachableException();
        var storageFiles = await topLevel.StorageProvider.OpenFilePickerAsync(UiHelpers.DefaultFilePickerOpenOptions);

        if (storageFiles.Count != 1) return;

        Path.Text = storageFiles[0].Path.LocalPath;
    }

    private void IsSsd_OnIsCheckedChanged(object? _, RoutedEventArgs e)
        => _vm.Disks[Index].IsSsd = IsSsd.IsChecked!.Value;

    private void IsCdrom_OnIsCheckedChanged(object? _, RoutedEventArgs e)
        => _vm.Disks[Index].IsCdrom = IsCdrom.IsChecked!.Value;

    private void IsRemovable_OnIsCheckedChanged(object? _, RoutedEventArgs e)
        => _vm.Disks[Index].IsRemovable = IsRemovable.IsChecked!.Value;

    public void RefreshDiskControllerList(int startIndex) => UiHelpers.RefreshControllerList(ref Controller, startIndex);

    public void TriggerSelectionChangedEvent() => Controller_OnSelectionChanged(null, null!);
}