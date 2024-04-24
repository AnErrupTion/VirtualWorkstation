using System.Diagnostics;
using Avalonia.Controls;
using QemuSharp.Structs;
using QemuSharp.Structs.Enums;

namespace VirtualWorkstation;

public partial class NetworkInterfaceSettingsPage : UserControl, ITabPage, IController
{
    public bool Opened { get; set; }

    public int VmTabIndex { get; set; }

    public int Index { get; set; }

    private readonly VirtualMachine _vm;

    public NetworkInterfaceSettingsPage(ref VirtualMachine vm, int index)
    {
        InitializeComponent();

        _vm = vm;

        Index = index;

        if (_vm.UsbControllers.Count == 0)
        {
            if (Card.Items[(int)NetworkCard.Usb] is not ComboBoxItem item) throw new UnreachableException();
            item.IsEnabled = false;

            UsbController.IsEnabled = false;
        }
        for (var i = 0; i < _vm.UsbControllers.Count; i++) UsbController.Items.Add(new ComboBoxItem { Content = i.ToString() });

        var networkInterface = vm.NetworkInterfaces[index];
        Type.SelectedIndex = (int)networkInterface.Type;
        Card.SelectedIndex = (int)networkInterface.Card;
        UsbController.SelectedIndex = (int)networkInterface.UsbController;
    }

    private void Type_OnSelectionChanged(object? _, SelectionChangedEventArgs e)
        => _vm.NetworkInterfaces[Index].Type = (NetworkType)Type.SelectedIndex;

    private void Card_OnSelectionChanged(object? _, SelectionChangedEventArgs e)
    {
        var card = (NetworkCard)Card.SelectedIndex;

        UsbController.IsEnabled = card == NetworkCard.Usb;
        _vm.NetworkInterfaces[Index].Card = card;
    }

    private void UsbController_OnSelectionChanged(object? _, SelectionChangedEventArgs e)
        => _vm.NetworkInterfaces[Index].UsbController = (ulong)UsbController.SelectedIndex;

    public void RefreshUsbControllerList(int startIndex) => UiHelpers.RefreshControllerList(ref UsbController, startIndex);

    public void TriggerSelectionChangedEvent() => UsbController_OnSelectionChanged(null, null!);
}