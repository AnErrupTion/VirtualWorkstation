using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using QemuSharp.Structs;
using QemuSharp.Structs.Enums;

namespace VirtualWorkstation;

public partial class AudioControllerSettingsPage : UserControl, ITabPage, IController
{
    public bool Opened { get; set; }

    public int VmTabIndex { get; set; }

    public int Index { get; set; }

    private readonly VirtualMachine _vm;

    public AudioControllerSettingsPage(ref VirtualMachine vm, int index)
    {
        InitializeComponent();

        _vm = vm;

        Index = index;

        if (_vm.UsbControllers.Count == 0)
        {
            if (Card.Items[(int)SoundCard.Usb] is not ComboBoxItem item) throw new UnreachableException();
            item.IsEnabled = false;

            UsbController.IsEnabled = false;
        }
        for (var i = 0; i < _vm.UsbControllers.Count; i++) UsbController.Items.Add(new ComboBoxItem { Content = i.ToString() });

        var audioController = vm.AudioControllers[index];
        Card.SelectedIndex = (int)audioController.Card;
        CustomCard.Text = audioController.CustomCard;
        CustomCardBus.SelectedIndex = (int)audioController.CustomCardBus;
        HasInput.IsChecked = audioController.HasInput;
        HasOutput.IsChecked = audioController.HasOutput;
        UsbController.SelectedIndex = (int)audioController.UsbController;
    }

    private void Card_OnSelectionChanged(object? _, SelectionChangedEventArgs e)
    {
        var card = (SoundCard)Card.SelectedIndex;
        _vm.AudioControllers[Index].Card = card;

        var isCardCustom = card == SoundCard.Custom;
        CustomCardGrid.IsVisible = isCardCustom;
        CustomCardBusGrid.IsVisible = isCardCustom;

        UsbControllerGrid.IsVisible = card == SoundCard.Usb;

        switch (card)
        {
            case SoundCard.SoundBlaster16:
            case SoundCard.IntelAc97:
            case SoundCard.Usb:
            {
                HasInput.IsEnabled = false;
                HasInput.IsChecked = false;
                HasOutput.IsEnabled = false;
                HasOutput.IsChecked = true;
                break;
            }
            case SoundCard.IntelHda6:
            case SoundCard.IntelHda9:
            {
                HasInput.IsEnabled = true;
                HasOutput.IsEnabled = true;
                break;
            }
            case SoundCard.VirtIo:
            {
                HasInput.IsEnabled = true;
                HasOutput.IsEnabled = false;
                HasOutput.IsChecked = true;
                break;
            }
            case SoundCard.Custom:
            {
                HasInput.IsEnabled = false;
                HasInput.IsChecked = false;
                HasOutput.IsEnabled = false;
                HasOutput.IsChecked = false;
                break;
            }
            default: throw new UnreachableException();
        }
    }

    private void CustomCard_OnTextChanged(object? _, TextChangedEventArgs e)
        => _vm.AudioControllers[Index].CustomCard = CustomCard.Text!;

    private void CustomCardBus_OnSelectionChanged(object? _, SelectionChangedEventArgs e)
        => _vm.AudioControllers[Index].CustomCardBus = (BusType)CustomCardBus.SelectedIndex;

    private void UsbController_OnSelectionChanged(object? _, SelectionChangedEventArgs e)
        => _vm.AudioControllers[Index].UsbController = (ulong)UsbController.SelectedIndex;

    private void HasInput_OnIsCheckedChanged(object? _, RoutedEventArgs e)
        => _vm.AudioControllers[Index].HasInput = HasInput.IsChecked!.Value;

    private void HasOutput_OnIsCheckedChanged(object? _, RoutedEventArgs e)
        => _vm.AudioControllers[Index].HasOutput = HasOutput.IsChecked!.Value;

    public void RefreshUsbControllerList(int startIndex) => UiHelpers.RefreshControllerList(ref UsbController, startIndex);

    public void TriggerSelectionChangedEvent() => UsbController_OnSelectionChanged(null, null!);
}