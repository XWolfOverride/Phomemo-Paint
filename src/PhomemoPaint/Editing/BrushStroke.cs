using System.Drawing.Drawing2D;

namespace PhomemoPaint.Editing;

/// <summary>Un trazo del pincel: la línea que une los puntos, de un grosor y un color.</summary>
internal sealed class BrushStroke(int size, bool black)
{
    public int Size { get; } = size;

    public bool Black { get; } = black;

    public List<PointF> Points { get; } = [];

    public BrushStroke Clone()
    {
        var copy = new BrushStroke(Size, Black);
        copy.Points.AddRange(Points);
        return copy;
    }

    public void Paint(Graphics g)
    {
        if (Points.Count == 1)
            BrushPainter.Segment(g, Points[0], Points[0], Size, Black);
        for (int i = 1; i < Points.Count; i++)
            BrushPainter.Segment(g, Points[i - 1], Points[i], Size, Black);
    }
}

internal static class BrushPainter
{
    /// <summary>Tramo de pincel redondo entre dos puntos (centros de píxel), sin suavizado.</summary>
    public static void Segment(Graphics g, PointF from, PointF to, int size, bool black)
    {
        g.SmoothingMode = SmoothingMode.None;
        g.PixelOffsetMode = PixelOffsetMode.Half;
        var color = black ? Color.Black : Color.White;
        if (size <= 2)
        {
            // GDI+ no pinta elipses de 1-2 px de forma fiable: cuadrados a lo largo del trazo.
            using var brush = new SolidBrush(color);
            int steps = (int)Math.Ceiling(Math.Max(Math.Abs(to.X - from.X), Math.Abs(to.Y - from.Y)));
            for (int i = 0; i <= steps; i++)
            {
                float t = steps == 0 ? 0 : (float)i / steps;
                int x = (int)Math.Floor(from.X + (to.X - from.X) * t - (size - 1) / 2f);
                int y = (int)Math.Floor(from.Y + (to.Y - from.Y) * t - (size - 1) / 2f);
                g.FillRectangle(brush, x, y, size, size);
            }
            return;
        }
        using (var b = new SolidBrush(color))
        {
            g.FillEllipse(b, from.X - size / 2f, from.Y - size / 2f, size, size);
            g.FillEllipse(b, to.X - size / 2f, to.Y - size / 2f, size, size);
        }
        if (from != to)
        {
            using var pen = new Pen(color, size) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round };
            g.DrawLine(pen, from, to);
        }
    }
}
