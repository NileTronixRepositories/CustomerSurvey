using BuildingBlock.Application.Email;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Shared.Dto
{
    public static class EmailTemplates
    {
        public static EmailMessage BuildEmailVerificationOtpMessage(
            string toEmail,
            string otpCode,
            int expiresInMinutes = 3,
            string? from = null)
        {
            var subject = "Verify your email - OTP";

            var html = $@"
<!doctype html>
<html lang=""en"">
<head>
  <meta charset=""utf-8"" />
  <meta name=""viewport"" content=""width=device-width, initial-scale=1"" />
  <title>{subject}</title>
</head>
<body style=""margin:0;padding:0;background:#f6f7fb;font-family:Arial,Helvetica,sans-serif;"">
  <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#f6f7fb;padding:24px 0;"">
    <tr>
      <td align=""center"">
        <table role=""presentation"" width=""600"" cellpadding=""0"" cellspacing=""0""
               style=""background:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 4px 18px rgba(0,0,0,.06);"">
          <tr>
            <td style=""padding:20px 24px;background:#111827;color:#fff;"">
              <div style=""font-size:18px;font-weight:700;"">Email Verification</div>
              <div style=""font-size:13px;opacity:.85;margin-top:4px;"">
                Use the code below to verify your email.
              </div>
            </td>
          </tr>

          <tr>
            <td style=""padding:24px;color:#111827;"">
              <div style=""font-size:14px;line-height:1.7;"">
                Your one-time verification code (OTP) is:
              </div>

              <div style=""margin:18px 0 14px 0;text-align:center;"">
                <span style=""
                  display:inline-block;
                  letter-spacing:6px;
                  font-size:28px;
                  font-weight:800;
                  padding:14px 18px;
                  border:1px solid #e5e7eb;
                  border-radius:10px;
                  background:#f9fafb;"">{otpCode}</span>
              </div>

              <div style=""font-size:13px;line-height:1.7;color:#374151;"">
                This code expires in <b>{expiresInMinutes} minutes</b>.
                If you didn’t request this, you can ignore this email.
              </div>

              <hr style=""border:none;border-top:1px solid #eee;margin:18px 0;"" />

              <div style=""font-size:12px;color:#6b7280;line-height:1.6;"">
                For your security, never share this code with anyone.
              </div>
            </td>
          </tr>

          <tr>
            <td style=""padding:14px 24px;background:#f9fafb;color:#6b7280;font-size:12px;"">
              © {DateTime.UtcNow:yyyy} NPARK
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";

            return new EmailMessage(
                To: toEmail,
                Subject: subject,
                HtmlBody: html,
                From: from,
                Attachments: null
            );
        }
    }
}