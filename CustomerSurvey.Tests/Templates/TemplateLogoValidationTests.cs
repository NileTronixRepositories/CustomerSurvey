using CustomerSurvey.Application.Shared.Media;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace CustomerSurvey.Tests.Templates;

public sealed class TemplateLogoValidationTests
{
    [Fact]
    public void Validate_AcceptsSupportedImageWithMatchingSignature()
    {
        var logo = CreateFile(
            "logo.png",
            "image/png",
            [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]);

        var error = TemplateLogoMedia.Validate(logo);

        Assert.Null(error);
    }

    [Fact]
    public void Validate_RejectsUnsupportedExtension()
    {
        var logo = CreateFile(
            "logo.gif",
            "image/png",
            [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]);

        var error = TemplateLogoMedia.Validate(logo);

        Assert.NotNull(error);
        Assert.Equal("Templates.Logo.Invalid", error.Code);
    }

    [Fact]
    public void Validate_RejectsUnsupportedContentType()
    {
        var logo = CreateFile(
            "logo.png",
            "application/octet-stream",
            [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]);

        var error = TemplateLogoMedia.Validate(logo);

        Assert.NotNull(error);
        Assert.Equal("Templates.Logo.Invalid", error.Code);
    }

    [Fact]
    public void Validate_RejectsMismatchedFileSignature()
    {
        var logo = CreateFile("logo.png", "image/png", [0x01, 0x02, 0x03, 0x04]);

        var error = TemplateLogoMedia.Validate(logo);

        Assert.NotNull(error);
        Assert.Equal("Templates.Logo.Invalid", error.Code);
    }

    [Fact]
    public void Validate_RejectsFileLargerThanFiveMegabytes()
    {
        var content = new byte[(5 * 1024 * 1024) + 1];
        content[0] = 0x89;
        content[1] = 0x50;
        content[2] = 0x4E;
        content[3] = 0x47;
        content[4] = 0x0D;
        content[5] = 0x0A;
        content[6] = 0x1A;
        content[7] = 0x0A;
        var logo = CreateFile("logo.png", "image/png", content);

        var error = TemplateLogoMedia.Validate(logo);

        Assert.NotNull(error);
        Assert.Equal("Templates.Logo.Invalid", error.Code);
    }

    private static FormFile CreateFile(string fileName, string contentType, byte[] content)
    {
        var stream = new MemoryStream(content);

        return new FormFile(stream, 0, stream.Length, "Logo", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }
}
