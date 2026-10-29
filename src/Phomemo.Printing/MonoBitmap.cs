namespace Phomemo.Printing;

/// <summary>
/// Imagen de 1 bit por punto en el formato que esperan las impresoras: filas de
/// <see cref="BytesPerRow"/> bytes, bit más significativo primero, 1 = negro.
/// Es lo único que la interfaz de usuario entrega a la capa de impresión.
/// </summary>
public sealed class MonoBitmap
{
    public int Width { get; }
    public int Height { get; }
    public int BytesPerRow { get; }
    public byte[] Data { get; }

    public MonoBitmap(int width, int height)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(width);
        ArgumentOutOfRangeException.ThrowIfNegative(height);
        Width = width;
        Height = height;
        BytesPerRow = (width + 7) / 8;
        Data = new byte[BytesPerRow * height];
    }

    /// <summary>Crea la imagen a partir de datos ya empaquetados (se copian).</summary>
    public MonoBitmap(int width, int height, ReadOnlySpan<byte> packed) : this(width, height)
    {
        if (packed.Length != Data.Length)
            throw new ArgumentException($"Se esperaban {Data.Length} bytes y hay {packed.Length}.", nameof(packed));
        packed.CopyTo(Data);
        ClearPadding();
    }

    /// <summary>Crea la imagen a partir de un píxel por elemento (true = negro), fila a fila.</summary>
    public static MonoBitmap FromPixels(int width, int height, ReadOnlySpan<bool> black)
    {
        if (black.Length != width * height)
            throw new ArgumentException($"Se esperaban {width * height} píxeles y hay {black.Length}.", nameof(black));
        var bmp = new MonoBitmap(width, height);
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                if (black[y * width + x])
                    bmp[x, y] = true;
        return bmp;
    }

    public bool this[int x, int y]
    {
        get
        {
            CheckBounds(x, y);
            return (Data[y * BytesPerRow + (x >> 3)] & (0x80 >> (x & 7))) != 0;
        }
        set
        {
            CheckBounds(x, y);
            int i = y * BytesPerRow + (x >> 3);
            byte mask = (byte)(0x80 >> (x & 7));
            Data[i] = value ? (byte)(Data[i] | mask) : (byte)(Data[i] & ~mask);
        }
    }

    public ReadOnlySpan<byte> Row(int y) => Data.AsSpan(y * BytesPerRow, BytesPerRow);

    /// <summary>Última fila con algún punto negro, o -1 si la imagen está en blanco.</summary>
    public int LastInkRow()
    {
        for (int y = Height - 1; y >= 0; y--)
            if (Row(y).IndexOfAnyExcept((byte)0) >= 0)
                return y;
        return -1;
    }

    /// <summary>Copia de las filas <paramref name="first"/> .. first+count-1.</summary>
    public MonoBitmap CropRows(int first, int count)
    {
        if (first < 0 || count < 0 || first + count > Height)
            throw new ArgumentOutOfRangeException(nameof(count));
        return new MonoBitmap(Width, count, Data.AsSpan(first * BytesPerRow, count * BytesPerRow));
    }

    /// <summary>
    /// Copia con otro ancho, sin escalar: si es más estrecha se pierde la parte derecha,
    /// si es más ancha se rellena con blanco a la derecha.
    /// </summary>
    public MonoBitmap WithWidth(int width)
    {
        if (width == Width) return new MonoBitmap(Width, Height, Data);
        var r = new MonoBitmap(width, Height);
        int copy = Math.Min(BytesPerRow, r.BytesPerRow);
        for (int y = 0; y < Height; y++)
            Row(y)[..copy].CopyTo(r.Data.AsSpan(y * r.BytesPerRow, copy));
        r.ClearPadding();
        return r;
    }

    // Los bits sobrantes del último byte de cada fila (cuando el ancho no es múltiplo de 8)
    // deben ser blanco, o la impresora los quemaría.
    private void ClearPadding()
    {
        int extra = BytesPerRow * 8 - Width;
        if (extra == 0 || BytesPerRow == 0) return;
        byte keep = (byte)(0xFF << extra);
        for (int y = 0; y < Height; y++)
            Data[y * BytesPerRow + BytesPerRow - 1] &= keep;
    }

    private void CheckBounds(int x, int y)
    {
        if ((uint)x >= (uint)Width || (uint)y >= (uint)Height)
            throw new ArgumentOutOfRangeException(nameof(x), $"({x},{y}) fuera de {Width}x{Height}");
    }
}
