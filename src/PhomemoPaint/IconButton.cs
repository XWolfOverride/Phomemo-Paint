using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace PhomemoPaint;

/// <summary>
/// Botón con un icono de Segoe MDL2 Assets. La fuente se calcula aquí y no pasa por el diseñador,
/// que encoge las fuentes explícitas al guardar en una pantalla escalada. El tamaño se edita con
/// <see cref="IconSize"/> y el icono es la propiedad Text.
/// </summary>
[DesignerCategory("Code")]
internal sealed class IconButton : Button
{
    private const float DefaultIconSize = 13F;
    private float _iconSize = DefaultIconSize;

    public IconButton() => base.Font = new Font("Segoe MDL2 Assets", _iconSize);

    /// <summary>Tamaño del icono en puntos.</summary>
    [DefaultValue(DefaultIconSize)]
    public float IconSize
    {
        get => _iconSize;
        set
        {
            _iconSize = value;
            base.Font = new Font("Segoe MDL2 Assets", value);
        }
    }

    [AllowNull]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public override Font Font
    {
        get => base.Font;
        set => base.Font = value;
    }
}
