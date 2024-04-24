using System.Diagnostics;

namespace QemuSharp;

public record ByteSize(ulong Value, ByteSuffix Suffix)
{
    public static ByteSize FromString(string str)
    {
        var end = 0;
        while (end < str.Length && char.IsDigit(str[end])) end++;

        if (end >= str.Length) throw new IndexOutOfRangeException(nameof(end));

        var quantityStr = str[..end];
        var suffixStart = end;

        while (end < str.Length && char.IsWhiteSpace(str[suffixStart])) suffixStart++;

        var suffixStr = str[suffixStart..];
        return new ByteSize(ulong.Parse(quantityStr), suffixStr switch
        {
            "B" => ByteSuffix.B,
            "K" or "KiB" => ByteSuffix.KiB,
            "M" or "MiB" => ByteSuffix.MiB,
            "G" or "GiB" => ByteSuffix.GiB,
            "T" or "TiB" => ByteSuffix.TiB,
            _ => throw new NotImplementedException()
        });
    }

    public override string ToString()
    {
        var suffix = Suffix switch
        {
            ByteSuffix.B => "B",
            ByteSuffix.KiB => "K",
            ByteSuffix.MiB => "M",
            ByteSuffix.GiB => "G",
            ByteSuffix.TiB => "T",
            _ => throw new UnreachableException()
        };

        return $"{Value}{suffix}";
    }
}