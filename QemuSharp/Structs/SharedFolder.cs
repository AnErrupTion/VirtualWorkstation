namespace QemuSharp.Structs;

public class SharedFolder
{
    public string Path { get; set; } = string.Empty;

    public SharedFolder() {}

    public SharedFolder(SharedFolder other)
    {
        Path = other.Path;
    }
}