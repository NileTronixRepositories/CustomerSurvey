using System.Buffers.Binary;
using System.IO.Compression;

namespace CustomerSurvey.infrastructure.Reports;

internal static class HorizontalBarChartPngRenderer
{
    private static readonly byte[] PngSignature = { 137, 80, 78, 71, 13, 10, 26, 10 };

    public static byte[] Render(IReadOnlyList<decimal> percentages, int width, int height)
    {
        ArgumentNullException.ThrowIfNull(percentages);
        if (width < 64 || height < 64)
        {
            throw new ArgumentOutOfRangeException(nameof(width));
        }

        var pixels = new byte[height * (1 + width * 4)];
        var values = percentages.Take(8).ToArray();
        var rowHeight = values.Length == 0 ? height : Math.Max(12, (height - 16) / values.Length);

        for (var y = 0; y < height; y++)
        {
            var rowOffset = y * (1 + width * 4);
            pixels[rowOffset] = 0;

            for (var x = 0; x < width; x++)
            {
                var color = (R: (byte)255, G: (byte)255, B: (byte)255);
                if (values.Length > 0)
                {
                    var index = Math.Clamp((y - 8) / rowHeight, 0, values.Length - 1);
                    var top = 8 + index * rowHeight + 2;
                    var bottom = top + Math.Max(6, rowHeight - 5);
                    if (y >= top && y < bottom && x >= 8 && x < width - 8)
                    {
                        color = ((R: (byte)226, G: (byte)232, B: (byte)240));
                        var value = Math.Clamp(values[index], 0m, 100m);
                        var barEnd = 8 + (int)Math.Round((width - 16) * (double)(value / 100m));
                        if (x < barEnd)
                        {
                            color = value >= 80m
                                ? ((byte)21, (byte)128, (byte)61)
                                : value >= 60m
                                    ? ((byte)37, (byte)99, (byte)235)
                                    : value >= 40m
                                        ? ((byte)234, (byte)179, (byte)8)
                                        : ((byte)220, (byte)38, (byte)38);
                        }
                    }
                }

                var offset = rowOffset + 1 + x * 4;
                pixels[offset] = color.R;
                pixels[offset + 1] = color.G;
                pixels[offset + 2] = color.B;
                pixels[offset + 3] = 255;
            }
        }

        using var output = new MemoryStream();
        output.Write(PngSignature);
        Span<byte> header = stackalloc byte[13];
        BinaryPrimitives.WriteInt32BigEndian(header[..4], width);
        BinaryPrimitives.WriteInt32BigEndian(header.Slice(4, 4), height);
        header[8] = 8;
        header[9] = 6;
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

    private static void WriteChunk(Stream output, string type, ReadOnlySpan<byte> data)
    {
        Span<byte> length = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(length, data.Length);
        output.Write(length);
        var typeBytes = System.Text.Encoding.ASCII.GetBytes(type);
        output.Write(typeBytes);
        output.Write(data);
        var crcInput = new byte[typeBytes.Length + data.Length];
        typeBytes.CopyTo(crcInput, 0);
        data.CopyTo(crcInput.AsSpan(typeBytes.Length));
        Span<byte> crc = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(crc, CalculateCrc32(crcInput));
        output.Write(crc);
    }

    private static uint CalculateCrc32(ReadOnlySpan<byte> bytes)
    {
        var crc = 0xFFFFFFFFu;
        foreach (var value in bytes)
        {
            crc ^= value;
            for (var bit = 0; bit < 8; bit++)
            {
                crc = (crc & 1) == 1 ? (crc >> 1) ^ 0xEDB88320u : crc >> 1;
            }
        }

        return ~crc;
    }
}
