using System.Text.Json.Serialization;

namespace CustomerSurvey.Api.Contracts.Operators;

public sealed record SetOperatorFlagSubmissionRequest([property: JsonRequired] bool Enabled);
