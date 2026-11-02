using PhomemoPaint.Editing;
using Xunit;

namespace PhomemoPaint.Tests;

public class TextObjectTests
{
    private const float Wide = 2000;

    private static TextContent Sample(string text = "Hola", float size = 14,
        FontStyle style = FontStyle.Regular, TextAlignment alignment = TextAlignment.Left, bool black = true,
        string font = "Arial") =>
        new(text, font, size, style, alignment, black, 8);

    private static ImageObject Place(TextContent text, float width = Wide) => ImageObject.FromText(text, PointF.Empty, width);

    private static int CountOpaque(ImageObject obj)
    {
        var (image, _) = obj.Render();
        return Imaging.ReadPixels(image).Count(p => (p >> 24 & 0xFF) != 0);
    }

    /// <summary>Columna más a la izquierda y más a la derecha con tinta, relativas a la imagen.</summary>
    private static (int Left, int Right) InkColumns(ImageObject obj)
    {
        var (image, _) = obj.Render();
        var px = Imaging.ReadPixels(image);
        int left = int.MaxValue, right = -1;
        for (int i = 0; i < px.Length; i++)
        {
            if (px[i] == 0) continue;
            int x = i % image.Width;
            left = Math.Min(left, x);
            right = Math.Max(right, x);
        }
        return (left, right);
    }

    [Fact]
    public void HeightGrowsWithTheFont()
    {
        Assert.True(Sample(size: 24).Measure(Wide) > Sample(size: 12).Measure(Wide) * 1.8);
    }

    [Fact]
    public void FontSizeIsPhysical()
    {
        // 72 pt son 25,4 mm: a 8 puntos/mm, un em de 203 puntos.
        Assert.InRange(Sample(size: 72).Measure(Wide), 203, 260);
    }

    [Fact]
    public void EveryLineAddsHeight()
    {
        Assert.True(Sample("a\nb\nc").Measure(Wide) > Sample("a").Measure(Wide) * 2.5);
    }

    [Fact]
    public void WindowsLineBreaksCountOnce()
    {
        Assert.Equal(Sample("a\nb").Measure(Wide), Sample("a\r\nb").Measure(Wide));
    }

    [Fact]
    public void LongTextWrapsToTheBox()
    {
        var text = Sample("una frase bastante larga para que no quepa en una sola linea");
        Assert.True(text.Measure(200) > text.Measure(Wide) * 2);
    }

    [Fact]
    public void WrappedInkStaysInsideTheBox()
    {
        var obj = Place(Sample("una frase bastante larga para que no quepa en una sola linea"), 200);
        var (left, right) = InkColumns(obj);
        Assert.True(left >= 0);
        Assert.True(right - left <= 200);
    }

    [Fact]
    public void WordWiderThanTheBoxIsSplit()
    {
        var text = Sample("palabrademasiadolargaparaelcuadro");
        Assert.True(text.Measure(60) > text.Measure(Wide) * 2);
        var (left, right) = InkColumns(Place(text, 60));
        Assert.True(right - left <= 60);
    }

    [Fact]
    public void SpacesOnlyDoNotBreak()
    {
        Assert.True(Sample("   ").Measure(100) > 0);
        Assert.True(Sample("").Measure(100) > 0);
    }

    [Theory]
    [InlineData(TextAlignment.Left, 0, 60)]
    [InlineData(TextAlignment.Center, 170, 230)]
    [InlineData(TextAlignment.Right, 340, 400)]
    public void AlignmentUsesTheBox(TextAlignment alignment, int min, int max)
    {
        var (left, right) = InkColumns(Place(Sample("Ho", alignment: alignment), 400));
        Assert.InRange((left + right) / 2, min, max);
    }

    [Fact]
    public void UnknownFontFallsBack()
    {
        Assert.True(CountOpaque(Place(Sample(font: "No existe esta fuente"))) > 0);
    }

    [Theory]
    [InlineData(TextAlignment.Left)]
    [InlineData(TextAlignment.Center)]
    [InlineData(TextAlignment.Right)]
    public void RendersOnlyPureBlackAndTransparent(TextAlignment alignment)
    {
        var (image, _) = Place(Sample("Hola\nmundo largo", alignment: alignment), 300).Render();
        Assert.All(Imaging.ReadPixels(image), p => Assert.True(p == 0 || p == Imaging.Black));
    }

    [Fact]
    public void WhiteTextIsWhite()
    {
        var (image, _) = Place(Sample(black: false), 300).Render();
        Assert.Contains(Imaging.White, Imaging.ReadPixels(image));
        Assert.DoesNotContain(Imaging.Black, Imaging.ReadPixels(image));
    }

    [Fact]
    public void UnderlineAddsInk()
    {
        Assert.True(CountOpaque(Place(Sample(style: FontStyle.Underline), 300)) > CountOpaque(Place(Sample(), 300)));
    }

    [Fact]
    public void BoldAddsInk()
    {
        Assert.True(CountOpaque(Place(Sample(style: FontStyle.Bold), 300)) > CountOpaque(Place(Sample(), 300)));
    }

    [Fact]
    public void WritingKeepsTheBoxWidthAndTop()
    {
        var obj = ImageObject.FromText(Sample("a"), new PointF(50, 60), 200);
        obj.SetText(Sample("una frase bastante larga para que no quepa en una sola linea"));
        Assert.Equal(200, obj.Size.Width);
        Assert.True(obj.Size.Height > 60);
        Assert.Equal(50, obj.Corners()[0].X, 2);
        Assert.Equal(60, obj.Corners()[0].Y, 2);
    }

    [Fact]
    public void BoxIsNeverShorterThanTheText()
    {
        var obj = Place(Sample("una\ndos\ntres"), 200);
        float textHeight = obj.Size.Height;
        obj.ResizeTextBox(200, 5);
        Assert.Equal(textHeight, obj.Size.Height);
    }

    [Fact]
    public void BoxCanBeTallerThanTheText()
    {
        var obj = Place(Sample(), 200);
        obj.ResizeTextBox(120, 300);
        Assert.Equal(new SizeF(120, 300), obj.Size);
    }

    [Fact]
    public void BoxKeepsItsHeightWhenTheTextShrinks()
    {
        var obj = Place(Sample("una\ndos\ntres"), 200);
        obj.ResizeTextBox(200, 400);
        obj.SetText(Sample("a"));
        Assert.Equal(400, obj.Size.Height);
    }

    [Fact]
    public void NarrowerBoxWrapsMore()
    {
        var obj = Place(Sample("una frase bastante larga"), 500);
        float oneLine = obj.Size.Height;
        obj.ResizeTextBox(120, 0);
        Assert.True(obj.Size.Height > oneLine);
    }

    [Fact]
    public void CloneKeepsTheText()
    {
        var obj = Place(Sample("copia"), 200);
        obj.ResizeTextBox(200, 300);
        obj.Angle = 30;
        var copy = obj.Clone();
        Assert.Equal(obj.Text, copy.Text);
        Assert.Equal(30, copy.Angle);
        Assert.Equal(MonoConversion.Threshold, copy.Conversion);
        Assert.Equal(300, copy.TextBoxHeight);
    }

    [Fact]
    public void UndoRestoresTheText()
    {
        using var doc = new PaintDocument(384, 200);
        var obj = Place(Sample("antes"), 200);
        doc.Objects.Add(obj);
        doc.Selected = obj;
        var history = new History();
        history.Push(doc);
        obj.SetText(Sample("después"));
        history.Undo(doc);
        Assert.Equal("antes", doc.Selected!.Text!.Text);
    }

    [Fact]
    public void CommitPrintsTheText()
    {
        using var doc = new PaintDocument(384, 200);
        var obj = ImageObject.FromText(Sample(), new PointF(10, 10), 300);
        doc.Objects.Add(obj);
        doc.Commit(obj);
        doc.NotifyChanged();
        Assert.Empty(doc.Objects);
        Assert.True(doc.PrintHeight > 10);
    }
}
