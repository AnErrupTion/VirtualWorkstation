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
        }
        for (var i = 0; i < _vm.UsbControllers.Count; i++) UsbController.Items.Add(new ComboBoxItem { Content = i.ToString() });

        var networkInterface = vm.NetworkInterfaces[index];
        Type.SelectedIndex = (int)networkInterface.Type;
        CustomType.Text = networkInterface.CustomType;
        Card.SelectedIndex = (int)networkInterface.Card;
        CustomCard.Text = networkInterface.CustomCard;
        CustomCardBus.SelectedIndex = (int)networkInterface.CustomCardBus.Type;
        CustomCardBus.Text = networkInterface.CustomCardBus.CustomType;
        UsbController.SelectedIndex = (int)networkInterface.UsbController;
    }

    private void Type_OnSelectionChanged(object? _, SelectionChangedEventArgs e)
    {
        var type = (NetworkType)Type.SelectedIndex;
        _vm.NetworkInterfaces[Index].Type = type;

        CustomTypeGrid.IsVisible = type == NetworkType.Custom;
    }

    private void CustomType_OnTextChanged(object? _, TextChangedEventArgs e)
        => _vm.NetworkInterfaces[Index].CustomType = CustomType.Text!;

    private void Card_OnSelectionChanged(object? _, SelectionChangedEventArgs e)
    {
        var card = (NetworkCard)Card.SelectedIndex;

        _vm.NetworkInterfaces[Index].Card = card;

        var isCardCustom = card == NetworkCard.Custom;
        CustomCardGrid.IsVisible = isCardCustom;
        CustomCardBusGrid.IsVisible = isCardCustom;

        UsbControllerGrid.IsVisible = card == NetworkCard.Usb;
    }

    private void CustomCard_OnTextChanged(object? _, TextChangedEventArgs e)
        => _vm.NetworkInterfaces[Index].CustomCard = CustomCard.Text!;

    private void CustomCardBus_OnSelectionChanged(object? _, SelectionChangedEventArgs e)
        => _vm.NetworkInterfaces[Index].CustomCardBus.Type = (BusType)CustomCardBus.SelectedIndex;

    private void CustomCardBus_OnTextChanged(object? _, TextChangedEventArgs e)
        => _vm.NetworkInterfaces[Index].CustomCardBus.CustomType = CustomCardBus.Text!;

    private void UsbController_OnSelectionChanged(object? _, SelectionChangedEventArgs e)
        => _vm.NetworkInterfaces[Index].UsbController = (ulong)UsbController.SelectedIndex;

    public void RefreshUsbControllerList(int startIndex)
    {
        UiHelpers.RefreshControllerList(ref UsbController, startIndex);

        if (Card.Items[(int)NetworkCard.Usb] is not ComboBoxItem item) throw new UnreachableException();
        item.IsEnabled = UsbController.Items.Count > 0;

        if (UsbController.SelectedIndex == -1) UsbController.SelectedIndex = 0;
    }

    public void TriggerSelectionChangedEvent() => UsbController_OnSelectionChanged(null, null!);
}