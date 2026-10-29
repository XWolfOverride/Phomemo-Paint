using System.Text.Json;
using System.Text.Json.Serialization;

namespace PhomemoPaint;

/// <summary>Preferencias en %APPDATA%\PhomemoPaint\settings.json.</summary>
internal sealed class AppSettings
{
    public string? LastPort { get; set; }

    public string? LastModel { get; set; }

    /// <summary>Último modelo elegido para cada puerto.</summary>
    public Dictionary<string, string> ModelByPort { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Impresoras Bluetooth LE vistas (dirección → nombre), para no tener que buscar al arrancar.</summary>
    public Dictionary<string, string> KnownBluetooth { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public int CanvasHeight { get; set; } = Editing.PaintDocument.DefaultHeight;

    public double LabelWidthMm { get; set; } = 40;

    public int BrushSize { get; set; } = 3;

    private static string FilePath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PhomemoPaint", "settings.json");

    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(FilePath) &&
                JsonSerializer.Deserialize(File.ReadAllText(FilePath), SettingsJson.Default.AppSettings) is { } s)
            {
                s.ModelByPort = new Dictionary<string, string>(s.ModelByPort, StringComparer.OrdinalIgnoreCase);
                s.KnownBluetooth = new Dictionary<string, string>(s.KnownBluetooth, StringComparer.OrdinalIgnoreCase);
                return s;
            }
        }
        catch (Exception e) when (e is IOException or JsonException or UnauthorizedAccessException)
        {
        }
        return new AppSettings();
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(this, SettingsJson.Default.AppSettings));
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
        }
    }
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(AppSettings))]
internal sealed partial class SettingsJson : JsonSerializerContext;
