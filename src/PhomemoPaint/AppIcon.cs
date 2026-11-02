namespace PhomemoPaint;

/// <summary>Icono de la aplicación (Phome.ico, recurso incrustado) para las ventanas.</summary>
internal static class AppIcon
{
    private static Icon? _icon;

    public static Icon? Icon => _icon ??= Load();

    private static Icon? Load()
    {
        using var stream = typeof(AppIcon).Assembly.GetManifestResourceStream("Phome.ico");
        return stream is null ? null : new Icon(stream);
    }
}
