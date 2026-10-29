namespace Phomemo.Printing;

/// <summary>Opciones de un trabajo. Lo que no se indica toma el valor del modelo.</summary>
public sealed record PrintOptions
{
    /// <summary>Densidad 1-15 (familia M110).</summary>
    public int? Darkness { get; init; }

    /// <summary>Velocidad 1-5 (familia M110).</summary>
    public int? Speed { get; init; }

    public MediaType Media { get; init; } = MediaType.LabelWithGaps;

    /// <summary>Líneas de avance tras imprimir. null = las del modelo.</summary>
    public int? FeedLines { get; init; }

    public static PrintOptions Default { get; } = new();
}
