using System.Buffers.Binary;
using System.IO.Compression;

namespace CustomerSurvey.infrastructure.Reports;

internal static class DonutChartPngRenderer
{
    private static readonly byte[] PngSignature =
    {
        137, 80, 78, 71, 13, 10, 26, 10
    };

    public static byte[] Render(
        IReadOnlyList<(decimal Value, string Color)> segments,
        int width,
        int height)
    {
        ArgumentNullException.ThrowIfNull(segments);

        if (width < 32 || height < 32)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Chart dimensions must be at least 32 pixels.");
        }

        var positiveSegments = segments
            .Where(x => x.Value > 0m)
            .Select(x => (Value: (double)x.Value, Color: ParseColor(x.Color)))
            .ToArray();
        var total = positiveSegments.Sum(x => x.Value);
        var pixels = new byte[height * (1 + (width * 4))];
        var centerX = width / 2d;
        var centerY = height / 2d;
        var outerRadius = Math.Min(width, height) * 0.42d;
        var innerRadius = outerRadius * 0.58d;
        var noDataColor = ParseColor("#CBD5E1");

        for (var y = 0; y < height; y++)
        {
            var rowOffset = y * (1 + (width * 4));
            pixels[rowOffset] = 0;

            for (var x = 0; x < width; x++)
            {
                var dx = (x + 0.5d) - centerX;
                var dy = (y + 0.5d) - centerY;
                var radius = Math.Sqrt((dx * dx) + (dy * dy));
                var color = (R: (byte)255, G: (byte)255, B: (byte)255);

                if (radius >= innerRadius && radius <= outerRadius)
                {
                    color = total <= 0d
                        ? noDataColor
                        : ResolveSegmentColor(positiveSegments, total, dx, dy);
                }

                var pixelOffset = rowOffset + 1 + (x * 4);
                pixels[pixelOffset] = color.R;
                pixels[pixelOffset + 1] = color.G;
                pixels[pixelOffset + 2] = color.B;
                pixels[pixelOffset + 3] = 255;
            }
        }

        using var output = new MemoryStream();
        output.Write(PngSignature);

        Span<byte> header = stackalloc byte[13];
        BinaryPrimitives.WriteInt32BigEndian(header[..4], width);
        BinaryPrimitives.WriteInt32BigEndian(header.Slice(4, 4), height);
        header[8] = 8;
        header[9] = 6;
        header[10] = 0;
        header[11] = 0;
        header[12] = 0;
        WriteChunk(output, "IHDR", header);

        using var compressed = new MemoryStream();
        using (var zlib = new ZLibStream(compressed, CompressionLevel.SmallestSize, leaveOpen: true))
        {
            zlib.Write(pixels);
        }

        WriteChunk(output, "IDAT", compressed.ToArray());
        WriteChunk(output, "IEND", ReadOnlySpan<byte>.Empty);
        return output.ToArray();
    }

    private static (byte R, byte G, byte B) ResolveSegmentColor(
        IReadOnlyList<(double Value, (byte R, byte G, byte B) Color)> segments,
        double total,
        double dx,
        double dy)
    {
        var angle = Math.Atan2(dy, dx) + (Math.PI / 2d);

        if (angle < 0d)
        {
            angle += Math.PI * 2d;
        }

        var position = angle / (Math.PI * 2d);
        var cumulative = 0d;

        foreach (var segment in segments)
        {
            cumulative += segment.Value / total;

            if (position <= cumulative)
            {
                return segment.Color;
            }
        }

        return segments[^1].Color;
    }

    private static (byte R, byte G, byte B) ParseColor(string color)
    {
        var value = color.Trim().TrimStart('#');

        if (value.Length != 6 || !int.TryParse(value, System.Globalization.NumberStyles.HexNumber, null, out var rgb))
        {
            throw new ArgumentException($"Invalid chart color '{color}'.", nameof(color));
        }

        return ((byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);
    }

    private static void WriteChunk(Stream stream, string type, ReadOnlySpan<byte> data)
    {
        Span<byte> length = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(length, data.Length);
        stream.Write(length);

        var typeBytes = System.Text.Encoding.ASCII.GetBytes(type);
        stream.Write(typeBytes);
        stream.Write(data);

        var crcInput = new byte[typeBytes.Length + data.Length];
        typeBytes.CopyTo(crcInput, 0);
        data.CopyTo(crcInput.AsSpan(typeBytes.Length));

        Span<byte> crc = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(crc, CalculateCrc32(crcInput));
        stream.Write(crc);
    }

    private static uint CalculateCrc32(ReadOnlySpan<byte> bytes)
    {
        var crc = 0xFFFFFFFFu;

        foreach (var value in bytes)
        {
            crc ^= value;

            for (var bit = 0; bit < 8; bit++)
            {
                crc = (crc & 1) == 1
                    ? (crc >> 1) ^ 0xEDB88320u
                    : crc >> 1;
            }
        }

        return ~crc;
    }
}
