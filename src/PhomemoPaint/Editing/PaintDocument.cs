using System.Drawing.Drawing2D;
using Phomemo.Printing;

namespace PhomemoPaint.Editing;

/// <summary>
/// Lo que se está dibujando: una capa de píxeles en blanco y negro con el ancho del cabezal y
/// el alto que elige el usuario, más las imágenes flotantes que aún no se han fijado.
/// </summary>
internal sealed class PaintDocument : IDisposable
{
    public const int DefaultHeight = 400;
    public const int MaxHeight = 20000;

    public PaintDocument(int width, int height)
    {
        Layer = Imaging.NewLayer(width, height);
    }

    public Bitmap Layer { get; private set; }

    public int Width => Layer.Width;

    public int Height => Layer.Height;

    /// <summary>De abajo arriba en orden de dibujo; el último es el de encima.</summary>
    public List<ImageObject> Objects { get; } = [];

    public ImageObject? Selected { get; set; }

    /// <summary>Filas que se imprimirían (hasta la última con tinta). Ver <see cref="UpdateInkExtent"/>.</summary>
    public int PrintHeight { get; private set; }

    public event EventHandler? Changed;

    public void NotifyChanged()
    {
        UpdateInkExtent();
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void UpdateInkExtent()
    {
        using var flat = Flatten();
        PrintHeight = Imaging.ToMono(flat).LastInkRow() + 1;
    }

    public bool HasInk() => PrintHeight > 0 || Objects.Count > 0;

    /// <summary>
    /// Cambia el tamaño sin escalar: lo que sobra a la derecha o abajo se pierde y lo nuevo es
    /// blanco. Las imágenes flotantes no se mueven.
    /// </summary>
    public void Resize(int width, int height)
    {
        if (width == Width && height == Height) return;
        var layer = Imaging.NewLayer(width, height);
        using (var g = Graphics.FromImage(layer))
            g.DrawImageUnscaled(Layer, 0, 0);
        Layer.Dispose();
        Layer = layer;
    }

    /// <summary>Capa con las imágenes flotantes encima.</summary>
    public Bitmap Flatten()
    {
        var bmp = (Bitmap)Layer.Clone();
        using var g = Graphics.FromImage(bmp);
        foreach (var obj in Objects)
            DrawObject(g, obj);
        return bmp;
    }

    /// <summary>Lo que se manda a imprimir: todo el ancho, desde arriba hasta la última fila con tinta.</summary>
    public MonoBitmap? ToPrintImage()
    {
        using var flat = Flatten();
        var mono = Imaging.ToMono(flat);
        int last = mono.LastInkRow();
        return last < 0 ? null : mono.CropRows(0, last + 1);
    }

    public MonoBitmap ToMono()
    {
        using var flat = Flatten();
        return Imaging.ToMono(flat);
    }

    public void Clear()
    {
        using (var g = Graphics.FromImage(Layer))
            g.Clear(Color.White);
        Objects.Clear();
        Selected = null;
    }

    /// <summary>Funde la imagen flotante con la capa de píxeles.</summary>
    public void Commit(ImageObject obj)
    {
        using (var g = Graphics.FromImage(Layer))
            DrawObject(g, obj);
        Objects.Remove(obj);
        if (Selected == obj) Selected = null;
    }

    public void CommitAll()
    {
        foreach (var obj in Objects.ToArray())
            Commit(obj);
    }

    public void Replace(Bitmap layer)
    {
        Layer.Dispose();
        Layer = layer;
        Objects.Clear();
        Selected = null;
    }

    public ImageObject? HitTest(PointF p) => Objects.LastOrDefault(o => o.Contains(p));

    /// <summary>Trazo de pincel redondo entre dos puntos (centros de píxel), sin suavizado.</summary>
    public void Stroke(PointF from, PointF to, int size, bool black)
    {
        using var g = Graphics.FromImage(Layer);
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

    private static void DrawObject(Graphics g, ImageObject obj)
    {
        var (img, bounds) = obj.Render();
        g.CompositingMode = CompositingMode.SourceOver;
        g.DrawImageUnscaled(img, bounds.Location);
    }

    public Snapshot TakeSnapshot() =>
        new(Imaging.ToMono(Layer), Objects.Select(o => o.Clone()).ToList(), Selected is null ? -1 : Objects.IndexOf(Selected));

    public void Restore(Snapshot s)
    {
        Layer.Dispose();
        Layer = Imaging.FromMono(s.Layer);
        Objects.Clear();
        Objects.AddRange(s.Objects.Select(o => o.Clone()));
        Selected = s.SelectedIndex >= 0 && s.SelectedIndex < Objects.Count ? Objects[s.SelectedIndex] : null;
    }

    public void Dispose()
    {
        Layer.Dispose();
        foreach (var o in Objects) o.Invalidate();
    }

    /// <summary>Estado para deshacer. La capa se guarda a 1 bit para no gastar memoria.</summary>
    public sealed record Snapshot(MonoBitmap Layer, List<ImageObject> Objects, int SelectedIndex);
}

/// <summary>Deshacer / rehacer con instantáneas.</summary>
internal sealed class History(int capacity = 50)
{
    private readonly LinkedList<PaintDocument.Snapshot> _undo = new();
    private readonly Stack<PaintDocument.Snapshot> _redo = new();

    public bool CanUndo => _undo.Count > 0;

    public bool CanRedo => _redo.Count > 0;

    /// <summary>Guardar el estado justo antes de un cambio.</summary>
    public void Push(PaintDocument doc)
    {
        _undo.AddLast(doc.TakeSnapshot());
        if (_undo.Count > capacity) _undo.RemoveFirst();
        _redo.Clear();
    }

    public bool Undo(PaintDocument doc)
    {
        if (_undo.Last is not { } last) return false;
        _redo.Push(doc.TakeSnapshot());
        _undo.RemoveLast();
        doc.Restore(last.Value);
        return true;
    }

    public bool Redo(PaintDocument doc)
    {
        if (!_redo.TryPop(out var s)) return false;
        _undo.AddLast(doc.TakeSnapshot());
        doc.Restore(s);
        return true;
    }

    public void Clear()
    {
        _undo.Clear();
        _redo.Clear();
    }
}
