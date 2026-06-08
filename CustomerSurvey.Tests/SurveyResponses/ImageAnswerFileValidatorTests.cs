using CustomerSurvey.Application.Shared.Validation;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace CustomerSurvey.Tests.SurveyResponses;

public sealed class ImageAnswerFileValidatorTests
{
    [Fact]
    public void Validate_ReturnsNull_ForValidJpeg()
    {
        var file = CreateFormFile(
            fileName: "answer.jpg",
            contentType: "image/jpeg",
            bytes: [0xFF, 0xD8, 0xFF, 0xE0, 0x00]);

        var error = ImageAnswerFileValidator.Validate(file);

        Assert.Null(error);
    }

    [Fact]
    public void Validate_ReturnsRequired_WhenFileIsMissing()
    {
        var error = ImageAnswerFileValidator.Validate(null);

        Assert.NotNull(error);
        Assert.Equal("SubmitResponse.ImageFile.Required", error.Code);
    }

    [Fact]
    public void Validate_ReturnsInvalidType_WhenContentTypeIsNotImage()
    {
        var file = CreateFormFile(
            fileName: "answer.jpg",
            contentType: "application/octet-stream",
            bytes: [0xFF, 0xD8, 0xFF, 0xE0, 0x00]);

        var error = ImageAnswerFileValidator.Validate(file);

        Assert.NotNull(error);
        Assert.Equal("SubmitResponse.ImageFile.InvalidType", error.Code);
    }

    [Fact]
    public void Validate_ReturnsInvalidExtension_WhenExtensionIsUnsupported()
    {
        var file = CreateFormFile(
            fileName: "answer.gif",
            contentType: "image/jpeg",
            bytes: [0xFF, 0xD8, 0xFF, 0xE0, 0x00]);

        var error = ImageAnswerFileValidator.Validate(file);

        Assert.NotNull(error);
        Assert.Equal("SubmitResponse.ImageFile.InvalidExtension", error.Code);
    }

    [Fact]
    public void Validate_ReturnsInvalidType_WhenSignatureDoesNotMatchExtension()
    {
        var file = CreateFormFile(
            fileName: "answer.png",
            contentType: "image/png",
            bytes: [0xFF, 0xD8, 0xFF, 0xE0, 0x00]);

        var error = ImageAnswerFileValidator.Validate(file);

        Assert.NotNull(error);
        Assert.Equal("SubmitResponse.ImageFile.InvalidType", error.Code);
    }

    private static IFormFile CreateFormFile(
        string fileName,
        string contentType,
        byte[] bytes)
    {
        var stream = new MemoryStream(bytes);

        return new FormFile(
            stream,
            baseStreamOffset: 0,
            length: bytes.Length,
            name: "imageFile",
            fileName: fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }
}
