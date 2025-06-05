using QRCoder;

namespace QRFileManager.Services
{
    internal class QRCode
    {
        private QRCodeData qrCodeData;

        public QRCode(QRCodeData qrCodeData)
        {
            this.qrCodeData = qrCodeData;
        }
    }
}