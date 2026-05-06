namespace BuildingBlock.Application.Abstraction.QrCode
{
    public interface IQRCodeService
    {
        byte[] GenerateQRCode<T>(T data);
    }
}