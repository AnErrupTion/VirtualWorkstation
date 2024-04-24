using QemuSharp.Structs.Enums;

namespace QemuSharp.Structs;

public class CustomQemuArgument
{
    public CustomQemuArgumentType Type { get; set; }
    public string Value { get; set; } = string.Empty;
    public List<CustomQemuArgumentParameter> Parameters { get; set; } = [];

    public CustomQemuArgument() {}

    public CustomQemuArgument(CustomQemuArgument other)
    {
        Type = other.Type;
        Value = other.Value;

        foreach (var parameter in other.Parameters) Parameters.Add(new CustomQemuArgumentParameter(parameter));
    }
}