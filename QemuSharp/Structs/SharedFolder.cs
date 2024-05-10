namespace QemuSharp.Structs;

public class SharedFolder
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;

    public SharedFolder() {}

    public SharedFolder(SharedFolder other)
    {
        Name = other.Name;
        Path = other.Path;
    }
}