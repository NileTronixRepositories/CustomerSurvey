using BuildingBlock.Application.Email;
using BuildingBlock.Application.Option;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace BuildingBlock.Infrastracture.Service
{
    internal sealed class MailKitEmailSender : IEmailSender
    {
        private readonly SmtpOptions _options;
        private readonly ILogger<MailKitEmailSender> _logger;

        public MailKitEmailSender(IOptions<SmtpOptions> options, ILogger<MailKitEmailSender> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task SendAsync(EmailMessage message, CancellationToken ct = default)
        {
            var mime = new MimeMessage();

            var fromEmail = message.From ?? _options.FromEmail;
            mime.From.Add(new MailboxAddress(_options.FromName, fromEmail));
            mime.To.Add(MailboxAddress.Parse(message.To));
            mime.Subject = message.Subject;

            var builder = new BodyBuilder { HtmlBody = message.HtmlBody };

            if (message.Attachments is not null)
            {
                foreach (var a in message.Attachments)
                    builder.Attachments.Add(a.FileName, a.Content, ContentType.Parse(a.ContentType));
            }

            mime.Body = builder.ToMessageBody();

            try
            {
                using var client = new SmtpClient();

                // مهم في بعض السيرفرات
                client.CheckCertificateRevocation = true;

                var secureSocket =
                    _options.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;

                await client.ConnectAsync(_options.Host, _options.Port, secureSocket, ct);
                await client.AuthenticateAsync(_options.UserName, _options.Password, ct);
                await client.SendAsync(mime, ct);
                await client.DisconnectAsync(true, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To} subject {Subject}", message.To, message.Subject);
                throw; // أو اعمل Result pattern لو تحب
            }
        }
    }
}