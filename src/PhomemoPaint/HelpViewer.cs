using System.ComponentModel;
using System.Diagnostics;

namespace PhomemoPaint;

/// <summary>
/// Ayuda: páginas HTML incrustadas en el exe (carpeta Help del proyecto) que se copian a la
/// carpeta temporal y se abren en el navegador. Al estar en %TEMP%, el liberador de espacio de
/// Windows las limpia; por eso se vuelven a copiar cada vez que se abre la ayuda.
/// </summary>
internal static class HelpViewer
{
    private const string ResourcePrefix = "help/";

    /// <summary>Abre la ayuda, opcionalmente en una sección (id del HTML).</summary>
    public static void Show(IWin32Window? owner, string? section = null)
    {
        try
        {
            string dir = Extract();
            string page = S.Spanish ? "ayuda.es.html" : "help.en.html";
            string target = string.IsNullOrEmpty(section) ? page : $"{page}#{section}";
            // Al abrir un fichero local, Windows suele perder el "#sección": se entra por una página
            // que redirige.
            string redirect = Path.Combine(dir, "abrir.html");
            File.WriteAllText(redirect,
                $"<!doctype html><meta charset=\"utf-8\"><meta http-equiv=\"refresh\" content=\"0;url={target}\">" +
                $"<script>location.replace('{target}');</script>");
            Process.Start(new ProcessStartInfo(redirect) { UseShellExecute = true });
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or Win32Exception)
        {
            MessageBox.Show(owner, S.HelpError(e.Message), S.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    /// <summary>Copia los recursos de la ayuda a %TEMP%\PhomemoPaint\ayuda-&lt;versión&gt;.</summary>
    private static string Extract()
    {
        var assembly = typeof(HelpViewer).Assembly;
        string dir = Path.Combine(Path.GetTempPath(), "PhomemoPaint", "ayuda-" + assembly.GetName().Version);
        foreach (var name in assembly.GetManifestResourceNames())
        {
            // Las páginas usan también el icono de la aplicación.
            if (!name.StartsWith(ResourcePrefix, StringComparison.Ordinal) && name != "Phome.ico") continue;
            string relative = name.StartsWith(ResourcePrefix, StringComparison.Ordinal) ? name[ResourcePrefix.Length..] : name;
            string path = Path.Combine(dir, relative.Replace('\\', '/'));
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            using var source = assembly.GetManifestResourceStream(name)!;
            using var file = File.Create(path);
            source.CopyTo(file);
        }
        return dir;
    }
}
