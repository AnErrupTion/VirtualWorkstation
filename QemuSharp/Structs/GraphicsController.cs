using QemuSharp.Structs.Enums;

namespace QemuSharp.Structs;

public class GraphicsController
{
    public GraphicsCard Card { get; set; }
    public string CustomCard { get; set; } = string.Empty;
    public DeviceBus CustomCardBus { get; set; } = new();
    public bool HasVgaEmulation { get; set; }
    public bool HasGraphicsAcceleration { get; set; }

    public GraphicsController() {}

    public GraphicsController(GraphicsController other)
    {
        Card = other.Card;
        CustomCard = other.CustomCard;
        CustomCardBus = new DeviceBus(other.CustomCardBus);
        HasVgaEmulation = other.HasVgaEmulation;
        HasGraphicsAcceleration = other.HasGraphicsAcceleration;
    }
}