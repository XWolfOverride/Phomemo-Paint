namespace Phomemo.Printing.Tests;

public class MonoBitmapTests
{
    [Fact]
    public void PacksMsbFirstWithOneAsBlack()
    {
        var bmp = new MonoBitmap(10, 1);
        bmp[0, 0] = true;
        bmp[9, 0] = true;

        Assert.Equal(2, bmp.BytesPerRow);
        Assert.Equal(new byte[] { 0x80, 0x40 }, bmp.Data);
        Assert.True(bmp[9, 0]);
        Assert.False(bmp[8, 0]);
    }

    [Fact]
    public void FromPixelsMatchesIndexer()
    {
        bool[] px = [true, false, false, true, false, true];
        var bmp = MonoBitmap.FromPixels(3, 2, px);

        Assert.True(bmp[0, 0]);
        Assert.True(bmp[0, 1]);
        Assert.True(bmp[2, 1]);
        Assert.False(bmp[1, 1]);
    }

    [Fact]
    public void LastInkRowIgnoresTrailingBlankRows()
    {
        var bmp = new MonoBitmap(16, 10);
        Assert.Equal(-1, bmp.LastInkRow());

        bmp[3, 2] = true;
        bmp[15, 6] = true;
        Assert.Equal(6, bmp.LastInkRow());
    }

    [Fact]
    public void WithWidthCropsRightWithoutScaling()
    {
        var bmp = new MonoBitmap(20, 2);
        bmp[0, 0] = true;
        bmp[11, 1] = true;
        bmp[19, 1] = true;

        var narrow = bmp.WithWidth(12);

        Assert.Equal(12, narrow.Width);
        Assert.True(narrow[0, 0]);
        Assert.True(narrow[11, 1]);
        Assert.Equal(0, narrow.Data[3] & 0x0F); // relleno de la fila 1 en blanco
    }

    [Fact]
    public void WithWidthPadsRightWithWhite()
    {
        var bmp = new MonoBitmap(8, 1);
        bmp[7, 0] = true;

        var wide = bmp.WithWidth(384);

        Assert.Equal(48, wide.BytesPerRow);
        Assert.True(wide[7, 0]);
        Assert.Equal(0, wide.Data.Skip(1).Count(b => b != 0));
    }

    [Fact]
    public void PackedConstructorClearsPaddingBits()
    {
        var bmp = new MonoBitmap(4, 1, [0xFF]);
        Assert.Equal(0xF0, bmp.Data[0]);
    }

    [Fact]
    public void CropRowsCopiesTheRange()
    {
        var bmp = new MonoBitmap(8, 4);
        bmp[1, 2] = true;

        var part = bmp.CropRows(2, 2);

        Assert.Equal(2, part.Height);
        Assert.True(part[1, 0]);
    }
}
