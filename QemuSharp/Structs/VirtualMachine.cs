using QemuSharp.Structs.Enums;

namespace QemuSharp.Structs;

public class VirtualMachine
{
    public string Name { get; set; } = string.Empty;
    public Architecture Architecture { get; set; }
    public bool UseHardwareAcceleration { get; set; }
    public Firmware Firmware { get; set; }
    public Chipset Chipset { get; set; }
    public ulong Ram { get; set; }
    public Display Display { get; set; }
    public AudioHostDevice AudioHostDevice { get; set; }
    public Processor Processor { get; set; }

    public List<UsbController> UsbControllers { get; set; } = [];
    public List<NetworkInterface> NetworkInterfaces { get; set; } = [];
    public List<GraphicsController> GraphicControllers { get; set; } = [];
    public List<AudioController> AudioControllers { get; set; } = [];
    public List<DiskController> DiskControllers { get; set; } = [];
    public List<Disk> Disks { get; set; } = [];
    public List<Keyboard> Keyboards { get; set; } = [];
    public List<Mouse> Mice { get; set; } = [];
    public List<CustomQemuArgument> CustomQemuArguments { get; set; } = [];

    public VirtualMachine() {}

    public VirtualMachine(VirtualMachine other)
    {
        Name = other.Name;
        Architecture = other.Architecture;
        UseHardwareAcceleration = other.UseHardwareAcceleration;
        Firmware = new Firmware(other.Firmware);
        Chipset = new Chipset(other.Chipset);
        Ram = other.Ram;
        Display = new Display(other.Display);
        AudioHostDevice = new AudioHostDevice(other.AudioHostDevice);
        Processor = new Processor(other.Processor);

        foreach (var usbController in other.UsbControllers) UsbControllers.Add(new UsbController(usbController));
        foreach (var networkInterface in other.NetworkInterfaces) NetworkInterfaces.Add(new NetworkInterface(networkInterface));
        foreach (var graphicsController in other.GraphicControllers) GraphicControllers.Add(new GraphicsController(graphicsController));
        foreach (var audioController in other.AudioControllers) AudioControllers.Add(new AudioController(audioController));
        foreach (var diskController in other.DiskControllers) DiskControllers.Add(new DiskController(diskController));
        foreach (var disk in other.Disks) Disks.Add(new Disk(disk));
        foreach (var keyboard in other.Keyboards) Keyboards.Add(new Keyboard(keyboard));
        foreach (var mouse in other.Mice) Mice.Add(new Mouse(mouse));
        foreach (var customQemuArgument in other.CustomQemuArguments) CustomQemuArguments.Add(new CustomQemuArgument(customQemuArgument));
    }
}