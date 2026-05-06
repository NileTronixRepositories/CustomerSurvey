using BuildingBlock.Application.Abstraction.QrCode;
using QRCoder;
using System.Text.Json;

namespace BuildingBlock.Infrastracture.Service
{
    public class QRCodeService : IQRCodeService
    {
        public byte[] GenerateQRCode<T>(T data)
        {
            var jsonData = JsonSerializer.Serialize(data);

            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(jsonData, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new QRCode(qrCodeData);

            using (var ms = new MemoryStream())
            {
                qrCode.GetGraphic(20).Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                return ms.ToArray();
            }
        }
    }
}