using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CustomerSurvey.Api.Swagger;

public sealed class AcceptLanguageHeaderOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= new List<OpenApiParameter>();

        var alreadyExists = operation.Parameters.Any(x =>
            string.Equals(x.Name, "Accept-Language", StringComparison.OrdinalIgnoreCase));

        if (alreadyExists)
        {
            return;
        }

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "Accept-Language",
            In = ParameterLocation.Header,
            Required = false,
            Description = "Response language. Use ar for Arabic, en for English.",
            Schema = new OpenApiSchema
            {
                Type = "string",
                Default = new OpenApiString("en"),
                Enum = new List<IOpenApiAny>
                {
                    new OpenApiString("ar"),
                    new OpenApiString("en")
                }
            }
        });
    }
}