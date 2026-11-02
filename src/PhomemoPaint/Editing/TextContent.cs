using System.Drawing.Drawing2D;

namespace PhomemoPaint.Editing;

public enum TextAlignment
{
    Left,
    Center,
    Right,
}

/// <summary>Contenido y formato de un texto flotante. El tamaño va en puntos tipográficos.</summary>
/// <param name="DotsPerMm">Resolución de la impresora: 12 pt miden lo mismo en el papel en cualquier modelo.</param>
internal sealed record TextContent(
    string Text,
    string FontFamily,
    float FontSize,
    FontStyle Style,
    TextAlignment Alignment,
    bool Black,
    double DotsPerMm)
{
    public const float MinFontSize = 4;
    public const float MaxFontSize = 300;

    /// <summary>Alto del em en puntos de impresora.</summary>
    private float EmDots => (float)(FontSize / 72 * 25.4 * DotsPerMm);

    /// <summary>
    /// Trazado del texto ajustado a un cuadro de <paramref name="boxWidth"/> de ancho, con la esquina
    /// superior izquierda en (0, 0), y el alto que ocupa. Las líneas se parten por las palabras y las
    /// alineaciones se refieren al cuadro. Al ser vectorial sale nítido con cualquier tamaño y giro.
    /// </summary>
    public (GraphicsPath Path, float Height) BuildPath(float boxWidth)
    {
        using var family = ResolveFamily();
        var style = AvailableStyle(family, Style);
        float em = EmDots;
        float unit = em / family.GetEmHeight(style);
        float lineHeight = family.GetLineSpacing(style) * unit;
        float baseline = family.GetCellAscent(style) * unit;
        float rule = Math.Max(1, em / 16);

        var lines = WrapLines(family, style, em, boxWidth);
        var result = new GraphicsPath(FillMode.Winding);
        for (int i = 0; i < lines.Count; i++)
        {
            if (lines[i].Length == 0) continue;
            using var path = new GraphicsPath();
            path.AddString(lines[i], family, (int)style, em, PointF.Empty, StringFormat.GenericTypographic);
            if (path.PointCount == 0) continue; // solo espacios
            var bounds = path.GetBounds();
            float left = Alignment switch
            {
                TextAlignment.Center => (boxWidth - bounds.Width) / 2,
                TextAlignment.Right => boxWidth - bounds.Width,
                _ => 0,
            };
            float top = i * lineHeight;
            using var move = new Matrix();
            move.Translate(left - bounds.X, top);
            path.Transform(move);
            result.AddPath(path, connect: false);
            // GraphicsPath no subraya: la raya se añade aparte.
            if (Style.HasFlag(FontStyle.Underline))
                result.AddRectangle(new RectangleF(left, top + baseline + rule, bounds.Width, rule));
        }
        return (result, (float)Math.Ceiling(Math.Max(1, lines.Count * lineHeight)));
    }

    /// <summary>Alto del texto ajustado a ese ancho.</summary>
    public float Measure(float boxWidth)
    {
        var (path, height) = BuildPath(boxWidth);
        path.Dispose();
        return height;
    }

    private List<string> WrapLines(FontFamily family, FontStyle style, float em, float boxWidth)
    {
        var lines = new List<string>();
        foreach (var paragraph in Text.Replace("\r\n", "\n").Split('\n'))
        {
            string line = "";
            foreach (var word in SplitWords(paragraph))
            {
                string rest = word;
                if (Fits(line + rest))
                {
                    line += rest;
                    continue;
                }
                if (line.Length > 0)
                {
                    lines.Add(line.TrimEnd());
                    line = "";
                }
                // Una palabra más ancha que el cuadro se parte por las letras.
                while (!Fits(rest) && rest.Length > 1)
                {
                    int take = rest.Length - 1;
                    while (take > 1 && !Fits(rest[..take])) take--;
                    lines.Add(rest[..take]);
                    rest = rest[take..];
                }
                line = rest;
            }
            lines.Add(line.TrimEnd());
        }
        return lines;

        bool Fits(string candidate)
        {
            using var path = new GraphicsPath();
            path.AddString(candidate.TrimEnd(), family, (int)style, em, PointF.Empty, StringFormat.GenericTypographic);
            return path.PointCount == 0 || path.GetBounds().Width <= boxWidth;
        }
    }

    /// <summary>Palabras con los espacios que las siguen.</summary>
    private static IEnumerable<string> SplitWords(string paragraph)
    {
        int start = 0;
        for (int i = 1; i <= paragraph.Length; i++)
        {
            bool wordEnds = i == paragraph.Length || (paragraph[i - 1] == ' ' && paragraph[i] != ' ');
            if (!wordEnds) continue;
            yield return paragraph[start..i];
            start = i;
        }
    }

    private FontFamily ResolveFamily()
    {
        try
        {
            return new FontFamily(FontFamily);
        }
        catch (ArgumentException)
        {
            return new FontFamily(System.Drawing.FontFamily.GenericSansSerif.Name);
        }
    }

    /// <summary>Algunas fuentes no tienen todos los estilos y GDI+ falla si se piden.</summary>
    private static FontStyle AvailableStyle(FontFamily family, FontStyle wanted)
    {
        wanted &= ~FontStyle.Underline;
        FontStyle[] options = [wanted, FontStyle.Regular, FontStyle.Bold, FontStyle.Italic, FontStyle.Bold | FontStyle.Italic];
        foreach (var style in options)
            if (family.IsStyleAvailable(style)) return style;
        return FontStyle.Regular;
    }
}
