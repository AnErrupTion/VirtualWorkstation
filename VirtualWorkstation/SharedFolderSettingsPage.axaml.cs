using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using QemuSharp.Structs;

namespace VirtualWorkstation;

public partial class SharedFolderSettingsPage : UserControl, ITabPage
{
    public bool Opened { get; set; }

    public int VmTabIndex { get; set; }

    public int Index { get; set; }

    private readonly VirtualMachine _vm;

    public SharedFolderSettingsPage(ref VirtualMachine vm, int index)
    {
        InitializeComponent();

        _vm = vm;
        Index = index;

        Path.Text = vm.SharedFolders[index].Path;
    }

    private void Path_OnTextChanged(object? _, TextChangedEventArgs e)
        => _vm.SharedFolders[Index].Path = Path.Text!;

    private async void Browse_OnClick(object? _, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this) ?? throw new UnreachableException();
        var storageFolders = await topLevel.StorageProvider.OpenFolderPickerAsync(UiHelpers.DefaultFolderPickerOpenOptions);

        if (storageFolders.Count != 1) return;

        Path.Text = storageFolders[0].Path.LocalPath;
    }
}