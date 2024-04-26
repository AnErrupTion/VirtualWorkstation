using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using QemuSharp.Structs;
using QemuSharp.Structs.Enums;

namespace VirtualWorkstation;

public partial class GraphicsControllerSettingsPage : UserControl, ITabPage
{
    public bool Opened { get; set; }

    public int VmTabIndex { get; set; }

    public int Index { get; set; }

    private readonly VirtualMachine _vm;

    public GraphicsControllerSettingsPage(ref VirtualMachine vm, int index)
    {
        InitializeComponent();

        _vm = vm;

        Index = index;

        var graphicsController = vm.GraphicControllers[index];
        Card.SelectedIndex = (int)graphicsController.Card;
        CustomCard.Text = graphicsController.CustomCard;
        CustomCardBus.SelectedIndex = (int)graphicsController.CustomCardBus;
        HasVgaEmulation.IsChecked = graphicsController.HasVgaEmulation;
        HasGraphicsAcceleration.IsChecked = graphicsController.HasGraphicsAcceleration;
    }

    private void Card_OnSelectionChanged(object? _, SelectionChangedEventArgs e)
    {
        var card = (GraphicsCard)Card.SelectedIndex;
        _vm.GraphicControllers[Index].Card = card;

        var isCardCustom = card == GraphicsCard.Custom;
        CustomCard.IsEnabled = isCardCustom;
        CustomCardBus.IsEnabled = isCardCustom;

        switch (card)
        {
            case GraphicsCard.Vga:
            case GraphicsCard.Cirrus:
            case GraphicsCard.VMware:
            {
                HasVgaEmulation.IsEnabled = false;
                HasVgaEmulation.IsChecked = true;
                HasGraphicsAcceleration.IsEnabled = false;
                HasGraphicsAcceleration.IsChecked = false;
                break;
            }
            case GraphicsCard.Qxl:
            {
                HasVgaEmulation.IsEnabled = true;
                HasGraphicsAcceleration.IsEnabled = false;
                HasGraphicsAcceleration.IsChecked = false;
                break;
            }
            case GraphicsCard.VirtIo:
            {
                HasVgaEmulation.IsEnabled = true;
                HasGraphicsAcceleration.IsEnabled = true;
                break;
            }
            case GraphicsCard.Custom:
            {
                HasVgaEmulation.IsEnabled = false;
                HasVgaEmulation.IsChecked = false;
                HasGraphicsAcceleration.IsEnabled = false;
                HasGraphicsAcceleration.IsChecked = false;
                break;
            }
            default: throw new UnreachableException();
        }
    }

    private void CustomCard_OnTextChanged(object? _, TextChangedEventArgs e)
        => _vm.GraphicControllers[Index].CustomCard = CustomCard.Text!;

    private void CustomCardBus_OnSelectionChanged(object? _, SelectionChangedEventArgs e)
        => _vm.GraphicControllers[Index].CustomCardBus = (BusType)CustomCardBus.SelectedIndex;

    private void HasVgaEmulation_OnIsCheckedChanged(object? _, RoutedEventArgs e)
        => _vm.GraphicControllers[Index].HasVgaEmulation = HasVgaEmulation.IsChecked!.Value;

    private void HasGraphicsAcceleration_OnIsCheckedChanged(object? _, RoutedEventArgs e)
        => _vm.GraphicControllers[Index].HasGraphicsAcceleration = HasGraphicsAcceleration.IsChecked!.Value;
}