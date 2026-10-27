using LiveLock.Core.Formatting;
using LiveLock.Core.Models;
using Xunit;

namespace LiveLock.Core.Tests;

public class ColorParserTests
{
    [Fact]
    public void Parses_Rgb_AsOpaque()
    {
        Assert.True(ColorParser.TryParse("#39F5FF", out var c));
        Assert.Equal(((byte)255, (byte)0x39, (byte)0xF5, (byte)0xFF), c);
    }

    [Fact]
    public void Parses_Argb()
    {
        Assert.True(ColorParser.TryParse("#80102030", out var c));
        Assert.Equal(((byte)0x80, (byte)0x10, (byte)0x20, (byte)0x30), c);
    }

    [Theory]
    [InlineData(null)] [InlineData("")] [InlineData("FFFFFF")] [InlineData("#FFF")] [InlineData("#GGGGGG")] [InlineData("#12345")]
    public void Rejects_Invalid(string? input) => Assert.False(ColorParser.TryParse(input, out _));

    [Fact] public void ToHex_RoundTrips() => Assert.Equal("#0A0B0C", ColorParser.ToHex(10, 11, 12));

    [Fact]
    public void AppSettings_Validate_RepairsNullSections_AndBadLibraryEntries()
    {
        var s = new AppSettings { Clock = null!, Library = new List<WallpaperItem> { new() { Path = "" }, new() { Path = "a.mp4" } } };
        s.Validate();
        Assert.NotNull(s.Clock);
        Assert.Single(s.Library);
    }
}
