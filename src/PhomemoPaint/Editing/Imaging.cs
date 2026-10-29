using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Phomemo.Printing;

namespace PhomemoPaint.Editing;

public enum MonoConversion
{
    Dither,
    Threshold,
}

/// <summary>Conversiones entre <see cref="Bitmap"/> y blanco y negro.</summary>
internal static class Imaging
{
    public const int White = unchecked((int)0xFFFFFFFF);
    public const int Black = unchecked((int)0xFF000000);

    public static Bitmap NewLayer(int width, int height)
    {
        var bmp = new Bitmap(Math.Max(1, width), Math.Max(1, height), PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.White);
        return bmp;
    }

    public static int[] ReadPixels(Bitmap bmp)
    {
        var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        try
        {
            var px = new int[bmp.Width * bmp.Height];
            for (int y = 0; y < bmp.Height; y++)
                Marshal.Copy(data.Scan0 + y * data.Stride, px, y * bmp.Width, bmp.Width);
            return px;
        }
        finally
        {
            bmp.UnlockBits(data);
        }
    }

    public static Bitmap FromPixels(int width, int height, int[] px)
    {
        var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        var data = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
        try
        {
            for (int y = 0; y < height; y++)
                Marshal.Copy(px, y * width, data.Scan0 + y * data.Stride, width);
        }
        finally
        {
            bmp.UnlockBits(data);
        }
        return bmp;
    }

    /// <summary>Luminancia 0-255 compuesta sobre blanco.</summary>
    public static int Gray(int argb)
    {
        int a = (argb >> 24) & 0xFF, r = (argb >> 16) & 0xFF, g = (argb >> 8) & 0xFF, b = argb & 0xFF;
        int lum = (r * 299 + g * 587 + b * 114) / 1000;
        return 255 - (255 - lum) * a / 255;
    }

    /// <summary>Capa (solo blanco y negro) a formato de impresora.</summary>
    public static MonoBitmap ToMono(Bitmap bmp)
    {
        var px = ReadPixels(bmp);
        var mono = new MonoBitmap(bmp.Width, bmp.Height);
        for (int y = 0; y < bmp.Height; y++)
            for (int x = 0; x < bmp.Width; x++)
                if (Gray(px[y * bmp.Width + x]) < 128)
                    mono[x, y] = true;
        return mono;
    }

    public static Bitmap FromMono(MonoBitmap mono)
    {
        var px = new int[mono.Width * mono.Height];
        for (int y = 0; y < mono.Height; y++)
            for (int x = 0; x < mono.Width; x++)
                px[y * mono.Width + x] = mono[x, y] ? Black : White;
        return FromPixels(Math.Max(1, mono.Width), Math.Max(1, mono.Height), px);
    }

    /// <summary>
    /// Pasa a blanco y negro opaco los píxeles visibles (alfa ≥ 128) y deja transparentes los
    /// demás. El tramado es Floyd-Steinberg y solo reparte error entre píxeles visibles.
    /// </summary>
    public static void ToBlackAndWhite(Bitmap bmp, MonoConversion mode, int threshold = 128)
    {
        int w = bmp.Width, h = bmp.Height;
        var px = ReadPixels(bmp);
        var gray = new float[w * h];
        for (int i = 0; i < px.Length; i++)
            gray[i] = Gray(px[i]);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int i = y * w + x;
                if (((px[i] >> 24) & 0xFF) < 128)
                {
                    px[i] = 0;
                    continue;
                }
                bool black = gray[i] < threshold;
                px[i] = black ? Black : White;
                if (mode != MonoConversion.Dither) continue;
                float err = gray[i] - (black ? 0 : 255);
                Spread(x + 1, y, 7f / 16);
                Spread(x - 1, y + 1, 3f / 16);
                Spread(x, y + 1, 5f / 16);
                Spread(x + 1, y + 1, 1f / 16);

                void Spread(int sx, int sy, float f)
                {
                    if (sx >= 0 && sx < w && sy < h) gray[sy * w + sx] += err * f;
                }
            }
        }

        var data = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
        try
        {
            for (int y = 0; y < h; y++)
                Marshal.Copy(px, y * w, data.Scan0 + y * data.Stride, w);
        }
        finally
        {
            bmp.UnlockBits(data);
        }
    }

    /// <summary>Copia en 32 bpp ARGB, independiente del fichero o del portapapeles.</summary>
    public static Bitmap CopyArgb(Image image)
    {
        var bmp = new Bitmap(image.Width, image.Height, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(bmp);
        g.CompositingMode = CompositingMode.SourceCopy;
        g.DrawImage(image, 0, 0, image.Width, image.Height);
        return bmp;
    }

    /// <summary>Abre una imagen sin dejar el fichero bloqueado.</summary>
    public static Bitmap Load(string path)
    {
        using var stream = new MemoryStream(File.ReadAllBytes(path));
        using var img = Image.FromStream(stream);
        return CopyArgb(img);
    }

    /// <summary>Guarda en PNG de 1 bit por píxel.</summary>
    public static void SaveMonoPng(MonoBitmap mono, string path)
    {
        using var bmp = new Bitmap(Math.Max(1, mono.Width), Math.Max(1, mono.Height), PixelFormat.Format1bppIndexed);
        var pal = bmp.Palette;
        pal.Entries[0] = Color.White;
        pal.Entries[1] = Color.Black;
        bmp.Palette = pal;
        var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, PixelFormat.Format1bppIndexed);
        try
        {
            // Mismo empaquetado que la impresora (MSB primero), con índice 1 = negro.
            for (int y = 0; y < mono.Height; y++)
                Marshal.Copy(mono.Data, y * mono.BytesPerRow, data.Scan0 + y * data.Stride, mono.BytesPerRow);
        }
        finally
        {
            bmp.UnlockBits(data);
        }
        bmp.Save(path, ImageFormat.Png);
    }
}
