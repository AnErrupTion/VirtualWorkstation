using QemuSharp.Structs.Enums;

namespace QemuSharp.Structs;

public class GraphicsController
{
    public GraphicsCard Card { get; set; }
    public bool HasVgaEmulation { get; set; }
    public bool HasGraphicsAcceleration { get; set; }

    public GraphicsController() {}

    public GraphicsController(GraphicsController other)
    {
        Card = other.Card;
        HasVgaEmulation = other.HasVgaEmulation;
        HasGraphicsAcceleration = other.HasGraphicsAcceleration;
    }
}