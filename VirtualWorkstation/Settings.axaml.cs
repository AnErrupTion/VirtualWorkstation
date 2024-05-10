using Avalonia.Controls;
using Avalonia.Interactivity;

namespace VirtualWorkstation;

public partial class Settings : Window
{
    public Settings()
    {
        InitializeComponent();

        VmFolder.Text = GlobalSettings.VmFolder;
        CustomQemuPath.Text = GlobalSettings.CustomQemuPath;
        CustomSwtpmPath.Text = GlobalSettings.CustomSwtpmPath;
        CustomVirtioFsDPath.Text = GlobalSettings.CustomVirtioFsDPath;
    }

    private async void BrowseVmFolder_OnClick(object? _, RoutedEventArgs e)
    {
        var storageFolders = await StorageProvider.OpenFolderPickerAsync(UiHelpers.DefaultFolderPickerOpenOptions);
        if (storageFolders.Count != 1) return;

        VmFolder.Text = storageFolders[0].Path.LocalPath;
    }

    private async void BrowseQemu_OnClick(object? _, RoutedEventArgs e)
    {
        var storageFolders = await StorageProvider.OpenFolderPickerAsync(UiHelpers.DefaultFolderPickerOpenOptions);
        if (storageFolders.Count != 1) return;

        CustomQemuPath.Text = storageFolders[0].Path.LocalPath;
    }

    private async void BrowseSwtpm_OnClick(object? _, RoutedEventArgs e)
    {
        var storageFolders = await StorageProvider.OpenFolderPickerAsync(UiHelpers.DefaultFolderPickerOpenOptions);
        if (storageFolders.Count != 1) return;

        CustomSwtpmPath.Text = storageFolders[0].Path.LocalPath;
    }

    private async void BrowseVirtioFsD_OnClick(object? _, RoutedEventArgs e)
    {
        var storageFolders = await StorageProvider.OpenFolderPickerAsync(UiHelpers.DefaultFolderPickerOpenOptions);
        if (storageFolders.Count != 1) return;

        CustomVirtioFsDPath.Text = storageFolders[0].Path.LocalPath;
    }

    private void Save_OnClick(object? _, RoutedEventArgs e)
    {
        GlobalSettings.VmFolder = VmFolder.Text!;
        GlobalSettings.CustomQemuPath = CustomQemuPath.Text!;
        GlobalSettings.CustomSwtpmPath = CustomSwtpmPath.Text!;
        GlobalSettings.CustomVirtioFsDPath = CustomVirtioFsDPath.Text!;
        GlobalSettings.Save();

        Close();
    }
}