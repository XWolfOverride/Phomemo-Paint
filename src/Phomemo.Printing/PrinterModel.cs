namespace Phomemo.Printing;

/// <summary>Protocolo que habla cada grupo de modelos. Ver DOC/protocolos.md.</summary>
public enum ProtocolFamily
{
    M02,
    M110,
    D30,
    M04,
    M832,
}

/// <summary>Tipo de papel (solo lo usan las familias que lo admiten).</summary>
public enum MediaType : byte
{
    LabelWithGaps = 0x0A,
    Continuous = 0x0B,
    LabelWithMarks = 0x26,
}

/// <summary>Descripción de un modelo de impresora.</summary>
/// <param name="Id">Identificador corto y estable ("T02").</param>
/// <param name="DisplayName">Nombre para mostrar.</param>
/// <param name="Family">Protocolo.</param>
/// <param name="Dpi">Resolución (igual en horizontal y vertical).</param>
/// <param name="HeadWidthDots">Ancho del cabezal en puntos: el máximo imprimible.</param>
/// <param name="WidthDependsOnMedia">El ancho útil depende de la etiqueta cargada (familia M110).</param>
/// <param name="DefaultMediaWidthMm">Ancho de etiqueta por defecto si <paramref name="WidthDependsOnMedia"/>.</param>
/// <param name="FeedLines">Avance tras imprimir para poder cortar.</param>
/// <param name="Verified">Datos comprobados con hardware real o con fuentes coincidentes.</param>
/// <param name="Experimental">Protocolo implementado sin poder probarlo.</param>
/// <param name="RequiresCompression">
/// Se ha detectado que el modelo trabaja con los datos comprimidos (LZO, <c>1F 11 35 01</c>), que
/// esta biblioteca no implementa: no se puede imprimir en él.
/// </param>
/// <param name="DefaultDarkness">Densidad si el trabajo no indica otra; null = la de la familia.</param>
public sealed record PrinterModel(
    string Id,
    string DisplayName,
    ProtocolFamily Family,
    int Dpi,
    int HeadWidthDots,
    bool WidthDependsOnMedia,
    double DefaultMediaWidthMm,
    int FeedLines,
    bool Verified,
    bool Experimental,
    bool RequiresCompression = false,
    int? DefaultDarkness = null)
{
    /// <summary>Se puede generar un trabajo para este modelo.</summary>
    public bool CanPrint => !RequiresCompression;

    public double DotsPerMm => Dpi / 25.4;

    public double HeadWidthMm => HeadWidthDots / DotsPerMm;

    /// <summary>Puntos que caben en una etiqueta de <paramref name="mm"/> milímetros, con tope en el cabezal.</summary>
    public int DotsForMediaWidth(double mm) =>
        Math.Clamp((int)Math.Round(mm * DotsPerMm), 8, HeadWidthDots);

    public override string ToString() => DisplayName;
}
