using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace PhomemoPaint.Editing;

/// <summary>
/// Lienzo con scroll propio, zoom entero, reglas en mm fijas en los bordes, pincel y tiradores
/// para mover, escalar y girar las imágenes flotantes.
/// </summary>
internal sealed class CanvasControl : ScrollableControl
{
    private const int RulerSize = 26;
    private const int HandleRadius = 5;
    private const int RotateHandleDistance = 26;

    private PaintDocument? _document;
    private int _zoom = 2;
    private double _dotsPerMm = 8;

    private DragKind _drag;
    private PointF _lastDoc;
    private PointF _dragStart;
    private ImageObject? _dragObject;
    private ImageObject? _dragOriginal;
    private int _hx, _hy;
    private bool _strokeBlack;
    private Point? _mouse;

    private enum DragKind { None, Stroke, Move, Resize, Rotate }

    public CanvasControl()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
        AutoScroll = true;
        BackColor = SystemColors.AppWorkspace;
        TabStop = true;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public PaintDocument? Document
    {
        get => _document;
        set
        {
            _document = value;
            UpdateScrollSize();
            Invalidate();
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int Zoom
    {
        get => _zoom;
        set
        {
            value = Math.Clamp(value, 1, 8);
            if (value == _zoom) return;
            _zoom = value;
            UpdateScrollSize();
            Invalidate();
            ZoomChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>Para la regla.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public double DotsPerMm
    {
        get => _dotsPerMm;
        set { _dotsPerMm = value; Invalidate(); }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int BrushSize { get; set; } = 3;

    /// <summary>Color del botón izquierdo; el derecho pinta el contrario.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool BrushBlack { get; set; } = true;

    /// <summary>Antes de modificar el documento (para guardar la instantánea de deshacer).</summary>
    public event EventHandler? BeforeEdit;

    /// <summary>Al terminar una modificación.</summary>
    public event EventHandler? Edited;

    public event EventHandler? SelectionChanged;

    public event EventHandler? ZoomChanged;

    /// <summary>Posición del ratón en puntos del documento (null si fuera).</summary>
    public event EventHandler<Point?>? CursorMoved;

    /// <summary>Primera fila del documento visible.</summary>
    public int VisibleTopRow => Math.Max(0, -AutoScrollPosition.Y / _zoom);

    public int VisibleRows => Math.Max(1, (ClientSize.Height - RulerSize) / _zoom);

    /// <summary>Mayor zoom con el que el ancho entero cabe en <paramref name="clientWidth"/>.</summary>
    public int FitZoom(int clientWidth)
    {
        if (_document is null) return 1;
        int avail = clientWidth - RulerSize - SystemInformation.VerticalScrollBarWidth - 4;
        return Math.Clamp(avail / Math.Max(1, _document.Width), 1, 8);
    }

    public void UpdateScrollSize()
    {
        if (_document is null) return;
        AutoScrollMinSize = new Size(RulerSize + _document.Width * _zoom + 1, RulerSize + _document.Height * _zoom + 1);
        Invalidate();
    }

    // ---------------------------------------------------------------- coordenadas

    private int OriginX => RulerSize + AutoScrollPosition.X;

    private int OriginY => RulerSize + AutoScrollPosition.Y;

    private PointF ToDoc(Point p) => new((p.X - OriginX) / (float)_zoom, (p.Y - OriginY) / (float)_zoom);

    private PointF ToScreen(PointF d) => new(OriginX + d.X * _zoom, OriginY + d.Y * _zoom);

    private static PointF PixelCenter(PointF d) => new((float)Math.Floor(d.X) + 0.5f, (float)Math.Floor(d.Y) + 0.5f);

    // ---------------------------------------------------------------- tiradores

    private PointF RotateHandle(ImageObject o) =>
        o.LocalToWorld(0, -o.Size.Height / 2 - RotateHandleDistance / (float)_zoom);

    private IEnumerable<(int Hx, int Hy, PointF Doc)> ResizeHandles(ImageObject o)
    {
        float w = o.Size.Width / 2, h = o.Size.Height / 2;
        for (int hy = -1; hy <= 1; hy++)
            for (int hx = -1; hx <= 1; hx++)
                if (hx != 0 || hy != 0)
                    yield return (hx, hy, o.LocalToWorld(hx * w, hy * h));
    }

    private static bool Near(PointF a, PointF b) =>
        Math.Abs(a.X - b.X) <= HandleRadius + 2 && Math.Abs(a.Y - b.Y) <= HandleRadius + 2;

    private (DragKind Kind, int Hx, int Hy) HitHandle(Point screen)
    {
        if (_document?.Selected is not { } o) return (DragKind.None, 0, 0);
        if (Near(ToScreen(RotateHandle(o)), screen)) return (DragKind.Rotate, 0, 0);
        foreach (var (hx, hy, d) in ResizeHandles(o))
            if (Near(ToScreen(d), screen)) return (DragKind.Resize, hx, hy);
        return (DragKind.None, 0, 0);
    }

    // ---------------------------------------------------------------- ratón

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        Focus();
        if (_document is null || _drag != DragKind.None) return;
        if (e.Button != MouseButtons.Left && e.Button != MouseButtons.Right) return;
        if (e.X < RulerSize || e.Y < RulerSize) return;

        var doc = ToDoc(e.Location);
        _dragStart = doc;
        _lastDoc = doc;

        if (e.Button == MouseButtons.Left)
        {
            var (kind, hx, hy) = HitHandle(e.Location);
            var target = _document.Selected;
            if (kind == DragKind.None && _document.HitTest(doc) is { } hit)
            {
                kind = DragKind.Move;
                target = hit;
                if (_document.Selected != hit)
                {
                    _document.Selected = hit;
                    SelectionChanged?.Invoke(this, EventArgs.Empty);
                }
            }
            if (kind != DragKind.None && target is not null)
            {
                BeforeEdit?.Invoke(this, EventArgs.Empty);
                _dragObject = _document.Selected;
                _dragOriginal = _dragObject?.Clone();
                _drag = kind;
                _hx = hx;
                _hy = hy;
                Capture = true;
                return;
            }
        }

        if (_document.Selected is not null)
        {
            _document.Selected = null;
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        BeforeEdit?.Invoke(this, EventArgs.Empty);
        _drag = DragKind.Stroke;
        _strokeBlack = e.Button == MouseButtons.Left ? BrushBlack : !BrushBlack;
        var p = PixelCenter(doc);
        _document.Stroke(p, p, BrushSize, _strokeBlack);
        _lastDoc = p;
        Capture = true;
        Invalidate();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        _mouse = e.Location;
        if (_document is null) return;
        var doc = ToDoc(e.Location);
        bool inside = doc.X >= 0 && doc.Y >= 0 && doc.X < _document.Width && doc.Y < _document.Height;
        CursorMoved?.Invoke(this, inside ? new Point((int)doc.X, (int)doc.Y) : null);

        switch (_drag)
        {
            case DragKind.Stroke:
                var p = PixelCenter(doc);
                _document.Stroke(_lastDoc, p, BrushSize, _strokeBlack);
                _lastDoc = p;
                break;
            case DragKind.Move when _dragObject is not null && _dragOriginal is not null:
                _dragObject.Center = new PointF(
                    _dragOriginal.Center.X + (float)Math.Round(doc.X - _dragStart.X),
                    _dragOriginal.Center.Y + (float)Math.Round(doc.Y - _dragStart.Y));
                _dragObject.Invalidate();
                break;
            case DragKind.Resize when _dragObject is not null && _dragOriginal is not null:
                ResizeObject(doc, (ModifierKeys & Keys.Shift) != 0);
                break;
            case DragKind.Rotate when _dragObject is not null && _dragOriginal is not null:
                RotateObject(doc, (ModifierKeys & Keys.Shift) != 0);
                break;
            default:
                UpdateCursor(e.Location, doc);
                break;
        }
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (_drag == DragKind.None) return;
        _drag = DragKind.None;
        _dragObject = null;
        _dragOriginal = null;
        Capture = false;
        Edited?.Invoke(this, EventArgs.Empty);
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _mouse = null;
        CursorMoved?.Invoke(this, null);
        Invalidate();
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        if ((ModifierKeys & Keys.Control) != 0)
        {
            Zoom += e.Delta > 0 ? 1 : -1;
            return;
        }
        base.OnMouseWheel(e);
        Invalidate();
    }

    protected override void OnScroll(ScrollEventArgs se)
    {
        base.OnScroll(se);
        Invalidate();
    }

    private void UpdateCursor(Point screen, PointF doc)
    {
        var (kind, hx, hy) = HitHandle(screen);
        if (kind == DragKind.Rotate) Cursor = Cursors.Hand;
        else if (kind == DragKind.Resize) Cursor = ResizeCursor(hx, hy);
        else if (_document?.HitTest(doc) is not null) Cursor = Cursors.SizeAll;
        else Cursor = Cursors.Cross;
    }

    private Cursor ResizeCursor(int hx, int hy)
    {
        // Dirección del tirador girada con el objeto, redondeada a múltiplos de 45°.
        double angle = Math.Atan2(hy, hx) * 180 / Math.PI + (_document?.Selected?.Angle ?? 0);
        int oct = (int)Math.Round(((angle % 180) + 180) % 180 / 45) % 4;
        return oct switch
        {
            0 => Cursors.SizeWE,
            1 => Cursors.SizeNWSE,
            2 => Cursors.SizeNS,
            _ => Cursors.SizeNESW,
        };
    }

    private void ResizeObject(PointF mouse, bool free)
    {
        var o = _dragObject!;
        var orig = _dragOriginal!;
        float w0 = orig.Size.Width, h0 = orig.Size.Height;
        var anchor = orig.LocalToWorld(-_hx * w0 / 2, -_hy * h0 / 2);

        // Vector ancla→ratón en los ejes del objeto.
        double a = -orig.Angle * Math.PI / 180;
        float dx = mouse.X - anchor.X, dy = mouse.Y - anchor.Y;
        float lx = (float)(dx * Math.Cos(a) - dy * Math.Sin(a));
        float ly = (float)(dx * Math.Sin(a) + dy * Math.Cos(a));

        float w = w0, h = h0;
        if (_hx != 0 && _hy != 0 && !free)
        {
            float s = Math.Max(lx * _hx / w0, ly * _hy / h0);
            s = Math.Max(s, 1f / Math.Min(w0, h0));
            w = w0 * s;
            h = h0 * s;
        }
        else
        {
            if (_hx != 0) w = Math.Max(1, lx * _hx);
            if (_hy != 0) h = Math.Max(1, ly * _hy);
        }

        o.Size = new SizeF((float)Math.Round(w), (float)Math.Round(h));
        double b = orig.Angle * Math.PI / 180;
        float cx = _hx * o.Size.Width / 2, cy = _hy * o.Size.Height / 2;
        o.Center = new PointF(
            anchor.X + (float)(cx * Math.Cos(b) - cy * Math.Sin(b)),
            anchor.Y + (float)(cx * Math.Sin(b) + cy * Math.Cos(b)));
        o.Invalidate();
    }

    private void RotateObject(PointF mouse, bool snap)
    {
        var o = _dragObject!;
        var c = _dragOriginal!.Center;
        double start = Math.Atan2(_dragStart.Y - c.Y, _dragStart.X - c.X);
        double now = Math.Atan2(mouse.Y - c.Y, mouse.X - c.X);
        double angle = _dragOriginal.Angle + (now - start) * 180 / Math.PI;
        angle = ((angle % 360) + 360) % 360;
        if (snap) angle = Math.Round(angle / 15) * 15 % 360;
        o.Angle = (float)angle;
        o.Invalidate();
    }

    // ---------------------------------------------------------------- dibujo

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(BackColor);
        if (_document is null) return;

        int ox = OriginX, oy = OriginY;
        var page = new Rectangle(ox, oy, _document.Width * _zoom, _document.Height * _zoom);

        // Solo las filas visibles: el lienzo puede tener 20000.
        int firstRow = Math.Max(0, (RulerSize - oy) / _zoom);
        int lastRow = Math.Min(_document.Height, (ClientSize.Height - oy) / _zoom + 1);
        g.InterpolationMode = InterpolationMode.NearestNeighbor;
        g.PixelOffsetMode = PixelOffsetMode.Half;
        if (lastRow > firstRow)
        {
            var src = new Rectangle(0, firstRow, _document.Width, lastRow - firstRow);
            var dst = new Rectangle(ox, oy + firstRow * _zoom, src.Width * _zoom, src.Height * _zoom);
            g.DrawImage(_document.Layer, dst, src, GraphicsUnit.Pixel);
        }

        var state = g.Save();
        g.SetClip(page);
        foreach (var obj in _document.Objects)
        {
            var (img, b) = obj.Render();
            g.DrawImage(img, new Rectangle(ox + b.X * _zoom, oy + b.Y * _zoom, b.Width * _zoom, b.Height * _zoom),
                new Rectangle(0, 0, b.Width, b.Height), GraphicsUnit.Pixel);
        }
        g.Restore(state);

        g.PixelOffsetMode = PixelOffsetMode.Default;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        // Lo que sobresale de los objetos (no se imprime), en gris.
        foreach (var obj in _document.Objects)
        {
            var pts = obj.Corners().Select(ToScreen).ToArray();
            using var pen = new Pen(Color.FromArgb(120, Color.SteelBlue)) { DashStyle = DashStyle.Dot };
            g.DrawPolygon(pen, pts);
        }

        g.SmoothingMode = SmoothingMode.None;
        using (var border = new Pen(Color.DimGray))
            g.DrawRectangle(border, page.X - 1, page.Y - 1, page.Width + 1, page.Height + 1);

        // Fin de lo que se imprimiría.
        if (_document.PrintHeight > 0)
        {
            int y = oy + _document.PrintHeight * _zoom;
            using var pen = new Pen(Color.OrangeRed) { DashStyle = DashStyle.Dash };
            g.DrawLine(pen, ox - 6, y, page.Right + 6, y);
        }

        if (_document.Selected is { } sel)
            PaintSelection(g, sel);

        if (_drag is DragKind.None or DragKind.Stroke && _mouse is { } m && m.X > RulerSize && m.Y > RulerSize &&
            (_document.Selected is null || HitHandle(m).Kind == DragKind.None) && _document.HitTest(ToDoc(m)) is null)
        {
            float r = BrushSize * _zoom / 2f;
            var c = ToScreen(PixelCenter(ToDoc(m)));
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using var pen = new Pen(Color.FromArgb(160, Color.DeepSkyBlue));
            g.DrawEllipse(pen, c.X - r, c.Y - r, r * 2, r * 2);
        }

        PaintRulers(g);
    }

    private void PaintSelection(Graphics g, ImageObject o)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var pts = o.Corners().Select(ToScreen).ToArray();
        using var pen = new Pen(Color.DodgerBlue, 1.5f);
        g.DrawPolygon(pen, pts);

        var top = ToScreen(o.LocalToWorld(0, -o.Size.Height / 2));
        var rot = ToScreen(RotateHandle(o));
        g.DrawLine(pen, top, rot);
        g.FillEllipse(Brushes.White, rot.X - HandleRadius, rot.Y - HandleRadius, HandleRadius * 2, HandleRadius * 2);
        g.DrawEllipse(pen, rot.X - HandleRadius, rot.Y - HandleRadius, HandleRadius * 2, HandleRadius * 2);

        g.SmoothingMode = SmoothingMode.None;
        foreach (var (_, _, d) in ResizeHandles(o))
        {
            var s = ToScreen(d);
            var r = new RectangleF(s.X - HandleRadius + 1, s.Y - HandleRadius + 1, HandleRadius * 2 - 2, HandleRadius * 2 - 2);
            g.FillRectangle(Brushes.White, r);
            g.DrawRectangle(Pens.DodgerBlue, r.X, r.Y, r.Width, r.Height);
        }
    }

    private void PaintRulers(Graphics g)
    {
        if (_document is null) return;
        g.SmoothingMode = SmoothingMode.None;
        var back = SystemColors.Control;
        using var backBrush = new SolidBrush(back);
        using var tick = new Pen(SystemColors.ControlText);
        using var font = new Font(Font.FontFamily, 7f);
        using var textBrush = new SolidBrush(SystemColors.ControlText);

        g.FillRectangle(backBrush, 0, 0, ClientSize.Width, RulerSize);
        g.FillRectangle(backBrush, 0, 0, RulerSize, ClientSize.Height);

        double pxPerMm = _dotsPerMm * _zoom;
        int step = pxPerMm >= 3 ? 1 : pxPerMm >= 1 ? 5 : 10;
        int labelEvery = pxPerMm * 10 >= 22 ? 10 : 50;

        double widthMm = _document.Width / _dotsPerMm;
        for (int mm = 0; mm <= widthMm; mm += step)
        {
            int x = OriginX + (int)Math.Round(mm * pxPerMm);
            if (x < RulerSize || x > ClientSize.Width) continue;
            int len = mm % 10 == 0 ? 12 : mm % 5 == 0 ? 8 : 4;
            g.DrawLine(tick, x, RulerSize - len, x, RulerSize - 1);
            if (mm % labelEvery == 0)
                g.DrawString(mm.ToString(), font, textBrush, x + 1, 1);
        }

        double heightMm = _document.Height / _dotsPerMm;
        int firstMm = Math.Max(0, (int)((RulerSize - OriginY) / pxPerMm) - 1);
        for (int mm = firstMm - firstMm % step; mm <= heightMm; mm += step)
        {
            int y = OriginY + (int)Math.Round(mm * pxPerMm);
            if (y < RulerSize) continue;
            if (y > ClientSize.Height) break;
            int len = mm % 10 == 0 ? 12 : mm % 5 == 0 ? 8 : 4;
            g.DrawLine(tick, RulerSize - len, y, RulerSize - 1, y);
            if (mm % labelEvery == 0)
                g.DrawString(mm.ToString(), font, textBrush, 1, y + 1);
        }

        // Posición del ratón en las reglas.
        if (_mouse is { } m)
        {
            using var mark = new Pen(Color.OrangeRed);
            if (m.X >= RulerSize) g.DrawLine(mark, m.X, 0, m.X, RulerSize - 1);
            if (m.Y >= RulerSize) g.DrawLine(mark, 0, m.Y, RulerSize - 1, m.Y);
        }

        g.FillRectangle(backBrush, 0, 0, RulerSize, RulerSize);
        g.DrawString("mm", font, textBrush, 3, 8);
        g.DrawLine(tick, 0, RulerSize - 1, ClientSize.Width, RulerSize - 1);
        g.DrawLine(tick, RulerSize - 1, 0, RulerSize - 1, ClientSize.Height);
    }
}
