namespace LiveLock.Core.Formatting;

public static class ColorParser
{
    /// <summary>Parses #RRGGBB or #AARRGGBB.</summary>
    public static bool TryParse(string? hex, out (byte A, byte R, byte G, byte B) color)
    {
        color = (255, 255, 255, 255);
        if (string.IsNullOrWhiteSpace(hex) || hex[0] != '#') return false;
        var h = hex[1..];
        if (h.Length != 6 && h.Length != 8) return false;
        if (!h.All(Uri.IsHexDigit)) return false;

        byte Part(int index) => Convert.ToByte(h.Substring(index, 2), 16);

        if (h.Length == 6) color = ((byte)255, Part(0), Part(2), Part(4));
        else color = (Part(0), Part(2), Part(4), Part(6));
        return true;
    }

    public static string ToHex(byte r, byte g, byte b) => $"#{r:X2}{g:X2}{b:X2}";
}
