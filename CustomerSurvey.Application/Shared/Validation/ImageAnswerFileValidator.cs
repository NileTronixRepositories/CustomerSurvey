using BuildingBlock.Domain.Results;
using CustomerSurvey.Domain.Resources;
using Microsoft.AspNetCore.Http;

namespace CustomerSurvey.Application.Shared.Validation;

internal static class ImageAnswerFileValidator
{
    public const long MaxImageFileSizeInBytes = 5 * 1024 * 1024;

    private static readonly string[] AllowedContentTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp"
    ];

    private static readonly string[] AllowedExtensions =
    [
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    ];

    public static Error? Validate(IFormFile? imageFile)
    {
        if (imageFile is null)
        {
            return new Error(
                Code: "SubmitResponse.ImageFile.Required",
                Message: ErrorMessage.SubmitResponse_ImageFile_Required,
                Type: ErrorType.Validation);
        }

        if (imageFile.Length == 0)
        {
            return new Error(
                Code: "SubmitResponse.ImageFile.Empty",
                Message: ErrorMessage.SubmitResponse_ImageFile_Empty,
                Type: ErrorType.Validation);
        }

        if (imageFile.Length > MaxImageFileSizeInBytes)
        {
            return new Error(
                Code: "SubmitResponse.ImageFile.MaxSize",
                Message: ErrorMessage.SubmitResponse_ImageFile_MaxSize,
                Type: ErrorType.Validation);
        }

        var contentTypeAllowed = !string.IsNullOrWhiteSpace(imageFile.ContentType) &&
            imageFile.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase) &&
            AllowedContentTypes.Contains(imageFile.ContentType, StringComparer.OrdinalIgnoreCase);

        if (!contentTypeAllowed)
        {
            return new Error(
                Code: "SubmitResponse.ImageFile.InvalidType",
                Message: ErrorMessage.SubmitResponse_ImageFile_InvalidType,
                Type: ErrorType.Validation);
        }

        var extension = Path.GetExtension(imageFile.FileName);

        var extensionAllowed = AllowedExtensions.Contains(
            extension,
            StringComparer.OrdinalIgnoreCase);

        if (!extensionAllowed)
        {
            return new Error(
                Code: "SubmitResponse.ImageFile.InvalidExtension",
                Message: ErrorMessage.SubmitResponse_ImageFile_InvalidExtension,
                Type: ErrorType.Validation);
        }

        if (!HasExpectedSignature(imageFile, extension))
        {
            return new Error(
                Code: "SubmitResponse.ImageFile.InvalidType",
                Message: ErrorMessage.SubmitResponse_ImageFile_InvalidType,
                Type: ErrorType.Validation);
        }

        return null;
    }

    private static bool HasExpectedSignature(IFormFile imageFile, string extension)
    {
        Span<byte> header = stackalloc byte[12];

        using var stream = imageFile.OpenReadStream();
        var bytesRead = stream.Read(header);

        return extension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => IsJpeg(header, bytesRead),
            ".png" => IsPng(header, bytesRead),
            ".webp" => IsWebp(header, bytesRead),
            _ => false
        };
    }

    private static bool IsJpeg(ReadOnlySpan<byte> header, int bytesRead)
    {
        return bytesRead >= 3 &&
               header[0] == 0xFF &&
               header[1] == 0xD8 &&
               header[2] == 0xFF;
    }

    private static bool IsPng(ReadOnlySpan<byte> header, int bytesRead)
    {
        return bytesRead >= 8 &&
               header[0] == 0x89 &&
               header[1] == 0x50 &&
               header[2] == 0x4E &&
               header[3] == 0x47 &&
               header[4] == 0x0D &&
               header[5] == 0x0A &&
               header[6] == 0x1A &&
               header[7] == 0x0A;
    }

    private static bool IsWebp(ReadOnlySpan<byte> header, int bytesRead)
    {
        return bytesRead >= 12 &&
               header[0] == 0x52 &&
               header[1] == 0x49 &&
               header[2] == 0x46 &&
               header[3] == 0x46 &&
               header[8] == 0x57 &&
               header[9] == 0x45 &&
               header[10] == 0x42 &&
               header[11] == 0x50;
    }
}
