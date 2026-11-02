using PhomemoPaint.Editing;
using Xunit;

namespace PhomemoPaint.Tests;

public class InkObjectTests
{
    private static PaintDocument Doc() => new(384, 200);

    private static ImageObject Line(PaintDocument doc, float x0, float y, float x1, int size = 3, bool black = true)
    {
        var ink = doc.BeginStroke(new PointF(x0, y), size, black);
        for (float x = x0 + 1; x <= x1; x++)
            ink.AddInkPoint(new PointF(x, y));
        return ink;
    }

    private static bool InkAt(PaintDocument doc, int x, int y) => doc.ToMono()[x, y];

    [Fact]
    public void StrokesOfAGroupShareOneObject()
    {
        using var doc = Doc();
        var first = Line(doc, 10.5f, 10.5f, 50.5f);
        var second = Line(doc, 10.5f, 40.5f, 50.5f);
        Assert.Same(first, second);
        Assert.Single(doc.Objects);
        Assert.Equal(2, first.Ink!.Count);
    }

    [Fact]
    public void ClosingTheGroupStartsAnother()
    {
        using var doc = Doc();
        Line(doc, 10.5f, 10.5f, 50.5f);
        doc.ActiveInk = null;
        Line(doc, 10.5f, 40.5f, 50.5f);
        Assert.Equal(2, doc.Objects.Count);
    }

    [Fact]
    public void AGroupThatLeftTheDocumentIsNotActive()
    {
        using var doc = Doc();
        var ink = Line(doc, 10.5f, 10.5f, 50.5f);
        doc.Commit(ink);
        Assert.Null(doc.ActiveInk);
    }

    [Fact]
    public void BoxWrapsAllTheStrokes()
    {
        using var doc = Doc();
        var ink = Line(doc, 10.5f, 10.5f, 50.5f);
        Line(doc, 100.5f, 150.5f, 120.5f);
        var box = ink.Corners();
        Assert.True(box[0].X <= 9 && box[0].Y <= 9);
        Assert.True(box[2].X >= 122 && box[2].Y >= 152);
    }

    [Fact]
    public void StrokeIsPaintedAndPrinted()
    {
        using var doc = Doc();
        Line(doc, 10.5f, 10.5f, 50.5f);
        doc.NotifyChanged();
        Assert.True(InkAt(doc, 30, 10));
        Assert.True(doc.PrintHeight > 10);
    }

    [Fact]
    public void MovingKeepsTheDrawing()
    {
        using var doc = Doc();
        var ink = Line(doc, 10.5f, 10.5f, 50.5f);
        ink.Center = new PointF(ink.Center.X + 100, ink.Center.Y + 60);
        ink.Invalidate();
        Assert.False(InkAt(doc, 30, 10));
        Assert.True(InkAt(doc, 130, 70));
    }

    [Fact]
    public void GroupGrowsAfterBeingMoved()
    {
        using var doc = Doc();
        var ink = Line(doc, 10.5f, 10.5f, 50.5f);
        ink.Center = new PointF(ink.Center.X + 100, ink.Center.Y + 60);
        ink.Invalidate();
        Line(doc, 10.5f, 150.5f, 50.5f);
        Assert.True(InkAt(doc, 130, 70));
        Assert.True(InkAt(doc, 30, 150));
    }

    [Fact]
    public void GrowingPastTheCachedDrawingKeepsEverything()
    {
        using var doc = Doc();
        var ink = Line(doc, 10.5f, 10.5f, 20.5f);
        ink.Render();
        Line(doc, 10.5f, 190.5f, 380.5f);
        Assert.True(InkAt(doc, 15, 10));
        Assert.True(InkAt(doc, 370, 190));
    }

    [Fact]
    public void BackgroundOfTheObjectIsTransparent()
    {
        using var doc = Doc();
        using (var g = Graphics.FromImage(doc.Layer))
            g.FillRectangle(Brushes.Black, 0, 0, 384, 200);
        Line(doc, 10.5f, 10.5f, 50.5f, black: false);
        Assert.False(InkAt(doc, 30, 10));
        Assert.True(InkAt(doc, 30, 30));
    }

    [Fact]
    public void DoesNotDeform()
    {
        using var doc = Doc();
        var ink = Line(doc, 10.5f, 10.5f, 50.5f);
        Assert.Equal(0, ink.Angle);
        Assert.NotNull(ink.Ink);
    }

    [Fact]
    public void UndoRemovesOnlyTheLastStroke()
    {
        using var doc = Doc();
        var history = new History();
        Line(doc, 10.5f, 10.5f, 50.5f);
        history.Push(doc);
        Line(doc, 10.5f, 40.5f, 50.5f);
        history.Undo(doc);
        Assert.Single(doc.Objects);
        Assert.Single(doc.ActiveInk!.Ink!);
        Assert.False(InkAt(doc, 30, 40));
    }

    [Fact]
    public void AfterUndoTheGroupContinues()
    {
        using var doc = Doc();
        var history = new History();
        Line(doc, 10.5f, 10.5f, 50.5f);
        history.Push(doc);
        Line(doc, 10.5f, 40.5f, 50.5f);
        history.Undo(doc);
        Line(doc, 10.5f, 70.5f, 50.5f);
        Assert.Single(doc.Objects);
    }

    [Fact]
    public void CloneDoesNotShareStrokes()
    {
        using var doc = Doc();
        var ink = Line(doc, 10.5f, 10.5f, 50.5f);
        var copy = ink.Clone();
        ink.AddInkPoint(new PointF(80.5f, 80.5f));
        Assert.NotEqual(ink.Ink![0].Points.Count, copy.Ink![0].Points.Count);
    }

    [Fact]
    public void ExtentIgnoresTheBrushWidth()
    {
        using var doc = Doc();
        var ink = Line(doc, 10.5f, 10.5f, 50.5f, size: 21);
        Assert.Equal(10.5f, ink.InkExtent().Left, 2);
        Assert.Equal(50.5f, ink.InkExtent().Right, 2);
    }
}
