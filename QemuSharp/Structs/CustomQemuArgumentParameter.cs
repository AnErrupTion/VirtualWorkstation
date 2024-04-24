namespace QemuSharp.Structs;

public class CustomQemuArgumentParameter
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;

    public CustomQemuArgumentParameter() {}

    public CustomQemuArgumentParameter(CustomQemuArgumentParameter other)
    {
        Key = other.Key;
        Value = other.Value;
    }
}