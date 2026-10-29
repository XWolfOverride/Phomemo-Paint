using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace PhomemoPaint.Editing;

/// <summary>
/// Patrón para medir la impresora con una regla: marco de 1 punto en el borde exacto del área
/// imprimible, marcas cada mm (largas cada 5 y 10, con número cada 10) en los cuatro lados y
/// una cruz en el centro. Ocupa el lienzo entero para poder medir también el largo impreso.
/// </summary>
internal static class CalibrationPattern
{
    public static void Draw(Bitmap layer, double dotsPerMm, string title)
    {
        int w = layer.Width, h = layer.Height;
        using var g = Graphics.FromImage(layer);
        g.Clear(Color.White);
        g.SmoothingMode = SmoothingMode.None;
        g.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
        using var brush = new SolidBrush(Color.Black);

        // Marco
        g.FillRectangle(brush, 0, 0, w, 1);
        g.FillRectangle(brush, 0, h - 1, w, 1);
        g.FillRectangle(brush, 0, 0, 1, h);
        g.FillRectangle(brush, w - 1, 0, 1, h);

        using var font = new Font(FontFamily.GenericSansSerif, (float)(2.2 * dotsPerMm), GraphicsUnit.Pixel);
        int Len(int mm) => (int)Math.Round((mm % 10 == 0 ? 4 : mm % 5 == 0 ? 2.5 : 1.2) * dotsPerMm);

        for (int mm = 1; mm * dotsPerMm < w - 1; mm++)
        {
            int x = (int)Math.Round(mm * dotsPerMm);
            g.FillRectangle(brush, x, 0, 1, Len(mm));
            g.FillRectangle(brush, x, h - Len(mm), 1, Len(mm));
            if (mm % 10 == 0)
                g.DrawString(mm.ToString(), font, brush, x + 2, Len(mm) - font.Height / 2f);
        }

        for (int mm = 1; mm * dotsPerMm < h - 1; mm++)
        {
            int y = (int)Math.Round(mm * dotsPerMm);
            g.FillRectangle(brush, 0, y, Len(mm), 1);
            g.FillRectangle(brush, w - Len(mm), y, Len(mm), 1);
            if (mm % 10 == 0)
                g.DrawString(mm.ToString(), font, brush, Len(mm) + 2, y - font.Height / 2f);
        }

        // Cruz central
        int cx = w / 2, cy = h / 2, arm = (int)(5 * dotsPerMm);
        g.FillRectangle(brush, cx - arm, cy, arm * 2 + 1, 1);
        g.FillRectangle(brush, cx, cy - arm, 1, arm * 2 + 1);

        var info = $"{title}\n{w} x {h} pt = {w / dotsPerMm:0.0} x {h / dotsPerMm:0.0} mm";
        var size = g.MeasureString(info, font);
        g.DrawString(info, font, brush, (w - size.Width) / 2, (float)(cy + arm + dotsPerMm * 2));
    }
}
