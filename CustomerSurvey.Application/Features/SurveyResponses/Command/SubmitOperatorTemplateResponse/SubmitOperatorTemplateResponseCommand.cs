using BuildingBlock.Application.Abstraction;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.SurveyResponses.Command.SubmitOperatorTemplateResponse
{
    public sealed record SubmitOperatorTemplateResponseCommand
       : ICommand<SubmitOperatorTemplateResponseResponse>
    {
        public Guid TemplateId { get; init; }

        public IReadOnlyCollection<SubmitOperatorTemplateCustomInputCommandItem> CustomInputs { get; init; }
            = Array.Empty<SubmitOperatorTemplateCustomInputCommandItem>();

        public IReadOnlyCollection<SubmitOperatorTemplateAnswerCommandItem> Answers { get; init; }
            = Array.Empty<SubmitOperatorTemplateAnswerCommandItem>();
    }

    public sealed record SubmitOperatorTemplateCustomInputCommandItem
    {
        public Guid CustomInputId { get; init; }

        public string? Value { get; init; }
    }

    public sealed record SubmitOperatorTemplateAnswerCommandItem
    {
        public Guid QuestionId { get; init; }

        public Guid? SelectedQuestionOptionId { get; init; }

        public int? StarRatingValue { get; init; }

        public int? SmileValue { get; init; }

        public string? TextAnswer { get; init; }

        public IFormFile? VoiceFile { get; init; }
    }
}