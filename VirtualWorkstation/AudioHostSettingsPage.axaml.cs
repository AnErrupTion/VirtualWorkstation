using System;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using QemuSharp.Structs;
using QemuSharp.Structs.Enums;

namespace VirtualWorkstation;

public partial class AudioHostSettingsPage : UserControl, ITabPage
{
    public bool Opened { get; set; }

    public int VmTabIndex { get; set; }

    public int Index { get; set; }

    private readonly VirtualMachine _vm;

    public AudioHostSettingsPage(ref VirtualMachine vm)
    {
        InitializeComponent();

        _vm = vm;
        
        CheckForUnsupportedOptions();

        Type.SelectedIndex = (int)vm.AudioHostDevice.Type;
    }

    private void Type_OnSelectionChanged(object? _, SelectionChangedEventArgs e)
    {
        _vm.AudioHostDevice.Type = (AudioHostType)Type.SelectedIndex;
        CheckForUnsupportedOptions();
    }

    private void EnableUnsupportedOptions_OnIsCheckedChanged(object? _, RoutedEventArgs e)
        => CheckForUnsupportedOptions();

    private void CheckForUnsupportedOptions()
    {
        if (OperatingSystem.IsWindows() && _vm.AudioHostDevice.Type is >= AudioHostType.Alsa and <= AudioHostType.CoreAudio
            || OperatingSystem.IsMacOS() && _vm.AudioHostDevice.Type is >= AudioHostType.Alsa and <= AudioHostType.SndIo or AudioHostType.DirectSound
            || OperatingSystem.IsLinux() && _vm.AudioHostDevice.Type is AudioHostType.CoreAudio or AudioHostType.DirectSound
            || OperatingSystem.IsFreeBSD() && _vm.AudioHostDevice.Type is AudioHostType.Alsa or AudioHostType.CoreAudio or AudioHostType.DirectSound)
        {
            EnableUnsupportedOptions.IsEnabled = false;
            EnableUnsupportedOptions.IsChecked = true;
            return;
        }

        EnableUnsupportedOptions.IsEnabled = true;

        if (OperatingSystem.IsWindows())
        {
            for (var i = AudioHostType.Alsa; i <= AudioHostType.CoreAudio; i++) SetOption(i);
        }
        else if (OperatingSystem.IsMacOS())
        {
            for (var i = AudioHostType.Alsa; i <= AudioHostType.SndIo; i++) SetOption(i);
            SetOption(AudioHostType.DirectSound);
        }
        else if (OperatingSystem.IsLinux())
        {
            SetOption(AudioHostType.CoreAudio);
            SetOption(AudioHostType.DirectSound);
        }
        else if (OperatingSystem.IsFreeBSD())
        {
            SetOption(AudioHostType.Alsa);
            SetOption(AudioHostType.CoreAudio);
            SetOption(AudioHostType.DirectSound);
        }
    }

    private void SetOption(AudioHostType type)
    {
        if (Type.Items[(int)type] is not ComboBoxItem item) throw new UnreachableException();
        item.IsEnabled = EnableUnsupportedOptions.IsChecked!.Value;
    }
}