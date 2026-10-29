using System.Drawing.Drawing2D;

namespace PhomemoPaint.Editing;

/// <summary>
/// Imagen pegada que flota sobre la capa de píxeles hasta que se fija. Guarda la imagen
/// original y la transformación, y se pasa a blanco y negro al renderizar, así que escalar o
/// girar varias veces no la degrada.
/// </summary>
internal sealed class ImageObject
{
    private Bitmap? _render;
    private Rectangle _renderBounds;

    public ImageObject(Bitmap source, PointF center, SizeF size)
    {
        Source = source;
        Center = center;
        Size = size;
    }

    /// <summary>Original en ARGB. Se comparte entre copias (no se modifica nunca).</summary>
    public Bitmap Source { get; }

    public PointF Center { get; set; }

    public SizeF Size { get; set; }

    /// <summary>Grados, sentido horario.</summary>
    public float Angle { get; set; }

    public MonoConversion Conversion { get; set; } = MonoConversion.Dither;

    public ImageObject Clone() => new(Source, Center, Size) { Angle = Angle, Conversion = Conversion };

    public void Invalidate()
    {
        _render?.Dispose();
        _render = null;
    }

    /// <summary>Esquina del rectángulo en coordenadas del documento (sx, sy ∈ {-1, 0, 1}).</summary>
    public PointF LocalToWorld(float lx, float ly)
    {
        double a = Angle * Math.PI / 180;
        float cos = (float)Math.Cos(a), sin = (float)Math.Sin(a);
        return new PointF(Center.X + lx * cos - ly * sin, Center.Y + lx * sin + ly * cos);
    }

    public PointF WorldToLocal(PointF p)
    {
        double a = -Angle * Math.PI / 180;
        float cos = (float)Math.Cos(a), sin = (float)Math.Sin(a);
        float dx = p.X - Center.X, dy = p.Y - Center.Y;
        return new PointF(dx * cos - dy * sin, dx * sin + dy * cos);
    }

    public bool Contains(PointF world)
    {
        var l = WorldToLocal(world);
        return Math.Abs(l.X) <= Size.Width / 2 && Math.Abs(l.Y) <= Size.Height / 2;
    }

    public PointF[] Corners() =>
    [
        LocalToWorld(-Size.Width / 2, -Size.Height / 2),
        LocalToWorld(Size.Width / 2, -Size.Height / 2),
        LocalToWorld(Size.Width / 2, Size.Height / 2),
        LocalToWorld(-Size.Width / 2, Size.Height / 2),
    ];

    /// <summary>Imagen en blanco y negro (transparente fuera) y dónde va en el documento.</summary>
    public (Bitmap Image, Rectangle Bounds) Render()
    {
        if (_render is not null) return (_render, _renderBounds);

        var corners = Corners();
        int x0 = (int)Math.Floor(corners.Min(p => p.X)), y0 = (int)Math.Floor(corners.Min(p => p.Y));
        int x1 = (int)Math.Ceiling(corners.Max(p => p.X)), y1 = (int)Math.Ceiling(corners.Max(p => p.Y));
        var bounds = Rectangle.FromLTRB(x0, y0, Math.Max(x0 + 1, x1), Math.Max(y0 + 1, y1));

        var bmp = new Bitmap(bounds.Width, bounds.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(bmp))
        {
            g.Clear(Color.Transparent);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TranslateTransform(Center.X - bounds.X, Center.Y - bounds.Y);
            g.RotateTransform(Angle);
            using var attrs = new System.Drawing.Imaging.ImageAttributes();
            attrs.SetWrapMode(WrapMode.TileFlipXY); // sin bordes semitransparentes al escalar
            g.DrawImage(Source,
                new Rectangle((int)Math.Round(-Size.Width / 2), (int)Math.Round(-Size.Height / 2),
                              Math.Max(1, (int)Math.Round(Size.Width)), Math.Max(1, (int)Math.Round(Size.Height))),
                0, 0, Source.Width, Source.Height, GraphicsUnit.Pixel, attrs);
        }
        Imaging.ToBlackAndWhite(bmp, Conversion);
        _render = bmp;
        _renderBounds = bounds;
        return (bmp, bounds);
    }
}
