using QemuSharp.Structs.Enums;

namespace QemuSharp.Structs;

public class AudioController
{
    public SoundCard Card { get; set; }
    public string CustomCard { get; set; } = string.Empty;
    public DeviceBus CustomCardBus { get; set; } = new();
    public ulong UsbController { get; set; }
    public bool HasInput { get; set; }
    public bool HasOutput { get; set; }

    public AudioController() {}

    public AudioController(AudioController other)
    {
        Card = other.Card;
        CustomCard = other.CustomCard;
        CustomCardBus = new DeviceBus(other.CustomCardBus);
        UsbController = other.UsbController;
        HasInput = other.HasInput;
        HasOutput = other.HasOutput;
    }
}