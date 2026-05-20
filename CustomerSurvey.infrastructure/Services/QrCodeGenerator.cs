using CustomerSurvey.Application.Abstraction.Services;
using QRCoder;

namespace CustomerSurvey.infrastructure.Services
{
    internal sealed class QrCodeGenerator : IQrCodeGenerator
    {
        public string GenerateBase64Png(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            using var qrGenerator = new QRCodeGenerator();

            using var qrCodeData = qrGenerator.CreateQrCode(
                plainText: text.Trim(),
                eccLevel: QRCodeGenerator.ECCLevel.Q);

            var qrCode = new PngByteQRCode(qrCodeData);

            var qrCodeBytes = qrCode.GetGraphic(pixelsPerModule: 20);

            return $"data:image/png;base64,{Convert.ToBase64String(qrCodeBytes)}";
        }
    }
}