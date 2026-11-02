namespace Phomemo.Printing;

/// <summary>Opciones de un trabajo. Lo que no se indica toma el valor del modelo.</summary>
public sealed record PrintOptions
{
    /// <summary>Densidad: 1-15 en la familia M110; <c>1F 11 02 nn</c> en la M02 (4 por defecto).</summary>
    public int? Darkness { get; init; }

    /// <summary>Coeficiente de calor de la familia M02 (<c>1F 11 37 nn</c>): 100 por defecto, 150 más negro.</summary>
    public int? HeatCoefficient { get; init; }

    /// <summary>Velocidad 1-5 (familia M110).</summary>
    public int? Speed { get; init; }

    public MediaType Media { get; init; } = MediaType.LabelWithGaps;

    /// <summary>Líneas de avance tras imprimir. null = las del modelo.</summary>
    public int? FeedLines { get; init; }

    public static PrintOptions Default { get; } = new();
}
