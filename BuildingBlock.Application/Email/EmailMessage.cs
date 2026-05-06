namespace BuildingBlock.Application.Email
{
    public sealed record EmailMessage(
    string To,
    string Subject,
    string HtmlBody,
    string? From = null,
    IReadOnlyList<EmailAttachment>? Attachments = null
);

    public sealed record EmailAttachment(
        string FileName,
        byte[] Content,
        string ContentType
    );
}