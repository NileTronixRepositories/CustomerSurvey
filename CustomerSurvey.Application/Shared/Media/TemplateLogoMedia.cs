using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Shared.Validation;
using CustomerSurvey.Domain.Common;
using Microsoft.AspNetCore.Http;

namespace CustomerSurvey.Application.Shared.Media;

internal static class TemplateLogoMedia
{
    public static Error? Validate(IFormFile? logo)
    {
        var validationError = ImageAnswerFileValidator.Validate(logo);

        return validationError is null
            ? null
            : new Error(
                Code: "Templates.Logo.Invalid",
                Message: "Logo must be a valid JPG, JPEG, PNG, or WEBP image no larger than 5 MB.",
                Type: ErrorType.Validation);
    }

    public static string ToRelativePath(string fileName)
        => Path.Combine("Media", FileNames.TemplateLogos, fileName).Replace('\\', '/');

    public static string ToPhysicalPath(string relativePath)
        => Path.Combine("./wwwroot", relativePath.Replace('/', Path.DirectorySeparatorChar));
}
