using System.Drawing.Drawing2D;

namespace PhomemoPaint.Editing;

/// <summary>
/// Imagen, texto o grupo de trazos que flota sobre la capa de píxeles hasta que se fija. Guarda el
/// original y la transformación, y se pasa a blanco y negro al renderizar, así que escalar o girar
/// varias veces no lo degrada.
/// </summary>
internal sealed class ImageObject
{
    /// <summary>Margen del dibujo de los trazos, para no rehacerlo cada vez que crecen un poco.</summary>
    private const int InkMargin = 32;

    private Bitmap? _render;
    private Rectangle _renderBounds;
    private RectangleF _inkBounds; // en las coordenadas de los puntos de los trazos, con el grosor

    public ImageObject(Bitmap source, PointF center, SizeF size)
    {
        Source = source;
        Center = center;
        Size = size;
    }

    private ImageObject(TextContent text, PointF center, SizeF size)
    {
        Text = text;
        Center = center;
        Size = size;
        Conversion = MonoConversion.Threshold; // el texto es solo blanco o negro: sin tramar
    }

    private ImageObject(List<BrushStroke> ink, RectangleF inkBounds, PointF center, SizeF size)
    {
        Ink = ink;
        _inkBounds = inkBounds;
        Center = center;
        Size = size;
    }

    /// <summary>
    /// Texto nuevo con la esquina superior izquierda en <paramref name="topLeft"/> y un cuadro de
    /// ese ancho; el alto lo da el texto.
    /// </summary>
    public static ImageObject FromText(TextContent text, PointF topLeft, float width)
    {
        var size = new SizeF(width, text.Measure(width));
        return new ImageObject(text, new PointF(topLeft.X + size.Width / 2, topLeft.Y + size.Height / 2), size);
    }

    /// <summary>Grupo de trazos, vacío hasta el primer <see cref="BeginStroke"/>.</summary>
    public static ImageObject NewInk() => new([], RectangleF.Empty, PointF.Empty, SizeF.Empty);

    /// <summary>Original en ARGB, o null si no es una imagen. Se comparte entre copias (no se modifica nunca).</summary>
    public Bitmap? Source { get; }

    /// <summary>Contenido si es un texto; el ancho del cuadro es el de <see cref="Size"/>.</summary>
    public TextContent? Text { get; private set; }

    /// <summary>Alto pedido con los tiradores: el cuadro nunca es más bajo que el texto.</summary>
    public float TextBoxHeight { get; private set; }

    /// <summary>Trazos si es un dibujo a pincel. Este objeto solo se mueve: sin tiradores.</summary>
    public List<BrushStroke>? Ink { get; }

    public PointF Center { get; set; }

    public SizeF Size { get; set; }

    /// <summary>Grados, sentido horario.</summary>
    public float Angle { get; set; }

    public MonoConversion Conversion { get; set; } = MonoConversion.Dither;

    /// <summary>Lo que mide el trazo del pincel respecto a su punto, sin el margen de seguridad.</summary>
    private PointF InkOffset => Ink is null || Ink.Count == 0
        ? PointF.Empty
        : new PointF(Center.X - (_inkBounds.X + _inkBounds.Width / 2), Center.Y - (_inkBounds.Y + _inkBounds.Height / 2));

    public ImageObject Clone()
    {
        ImageObject copy;
        if (Ink is not null)
            copy = new ImageObject(Ink.Select(s => s.Clone()).ToList(), _inkBounds, Center, Size);
        else if (Text is not null)
            copy = new ImageObject(Text, Center, Size) { TextBoxHeight = TextBoxHeight };
        else
            copy = new ImageObject(Source!, Center, Size);
        copy.Angle = Angle;
        copy.Conversion = Conversion;
        return copy;
    }

    /// <summary>Cambia el texto dejando quieto el borde superior del cuadro.</summary>
    public void SetText(TextContent text)
    {
        var top = LocalToWorld(0, -Size.Height / 2);
        Text = text;
        Size = new SizeF(Size.Width, Math.Max(TextBoxHeight, text.Measure(Size.Width)));
        var moved = LocalToWorld(0, -Size.Height / 2);
        Center = new PointF(Center.X + top.X - moved.X, Center.Y + top.Y - moved.Y);
        Invalidate();
    }

    /// <summary>Cambia el cuadro del texto; quien llama coloca el objeto.</summary>
    public void ResizeTextBox(float width, float height)
    {
        TextBoxHeight = height;
        Size = new SizeF(width, Math.Max(height, Text!.Measure(width)));
        Invalidate();
    }

    /// <summary>Empieza un trazo en un punto del documento.</summary>
    public void BeginStroke(PointF point, int size, bool black)
    {
        Ink!.Add(new BrushStroke(size, black));
        AddInkPoint(point);
    }

    /// <summary>Alarga el último trazo. Pinta solo el tramo nuevo: el dibujo puede ser grande.</summary>
    public void AddInkPoint(PointF point)
    {
        var stroke = Ink![^1];
        var offset = InkOffset;
        var stored = new PointF(point.X - offset.X, point.Y - offset.Y);
        var from = stroke.Points.Count > 0 ? stroke.Points[^1] : stored;
        stroke.Points.Add(stored);

        float pad = stroke.Size / 2f + 1;
        var dot = RectangleF.FromLTRB(
            (float)Math.Floor(stored.X - pad), (float)Math.Floor(stored.Y - pad),
            (float)Math.Ceiling(stored.X + pad), (float)Math.Ceiling(stored.Y + pad));
        _inkBounds = _inkBounds.IsEmpty ? dot : RectangleF.Union(_inkBounds, dot);
        Size = _inkBounds.Size;
        Center = new PointF(_inkBounds.X + _inkBounds.Width / 2 + offset.X, _inkBounds.Y + _inkBounds.Height / 2 + offset.Y);

        var area = new RectangleF(dot.X + offset.X, dot.Y + offset.Y, dot.Width, dot.Height);
        if (_render is null || !RectangleF.Union(_renderBounds, area).Equals((RectangleF)_renderBounds))
        {
            Invalidate();
            return;
        }
        using var g = Graphics.FromImage(_render);
        g.TranslateTransform(offset.X - _renderBounds.X, offset.Y - _renderBounds.Y);
        BrushPainter.Segment(g, from, stored, stroke.Size, stroke.Black);
    }

    /// <summary>Zona que ocupan los trazos en el documento, sin contar el grosor del borde.</summary>
    public RectangleF InkExtent()
    {
        var offset = InkOffset;
        var points = Ink!.SelectMany(s => s.Points).ToList();
        if (points.Count == 0) return RectangleF.Empty;
        return RectangleF.FromLTRB(points.Min(p => p.X) + offset.X, points.Min(p => p.Y) + offset.Y,
                                   points.Max(p => p.X) + offset.X, points.Max(p => p.Y) + offset.Y);
    }

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
        if (Ink is not null) bounds.Inflate(InkMargin, InkMargin);

        var bmp = new Bitmap(bounds.Width, bounds.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(bmp))
        {
            g.Clear(Color.Transparent);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            if (Ink is not null)
            {
                var offset = InkOffset;
                g.TranslateTransform(offset.X - bounds.X, offset.Y - bounds.Y);
                foreach (var stroke in Ink)
                    stroke.Paint(g);
            }
            else
            {
                g.TranslateTransform(Center.X - bounds.X, Center.Y - bounds.Y);
                g.RotateTransform(Angle);
                if (Text is { } text)
                {
                    var (path, _) = text.BuildPath(Size.Width);
                    using (path)
                    using (var brush = new SolidBrush(text.Black ? Color.Black : Color.White))
                    {
                        g.SmoothingMode = SmoothingMode.AntiAlias;
                        g.TranslateTransform(-Size.Width / 2, -Size.Height / 2);
                        g.FillPath(brush, path);
                    }
                }
                else
                {
                    using var attrs = new System.Drawing.Imaging.ImageAttributes();
                    attrs.SetWrapMode(WrapMode.TileFlipXY); // sin bordes semitransparentes al escalar
                    g.DrawImage(Source!,
                        new Rectangle((int)Math.Round(-Size.Width / 2), (int)Math.Round(-Size.Height / 2),
                                      Math.Max(1, (int)Math.Round(Size.Width)), Math.Max(1, (int)Math.Round(Size.Height))),
                        0, 0, Source!.Width, Source.Height, GraphicsUnit.Pixel, attrs);
                }
            }
        }
        if (Ink is null)
            Imaging.ToBlackAndWhite(bmp, Conversion);
        _render = bmp;
        _renderBounds = bounds;
        return (bmp, bounds);
    }
}
