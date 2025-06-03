
using System.ComponentModel.DataAnnotations;

namespace QRFileManager.Models
{
    public class FileModel
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string OriginalName { get; set; } = string.Empty;

        [Required]
        public string StoredName { get; set; } = string.Empty;

        [Required]
        public string MimeType { get; set; } = string.Empty;

        public long FileSize { get; set; }

        public DateTime UploadDate { get; set; } = DateTime.Now;

        public int DownloadCount { get; set; } = 0;

        public string? QRCodePath { get; set; }

        public string UploaderIP { get; set; } = string.Empty;

        // Майбутні налаштування доступу
        public bool IsActive { get; set; } = true;
        public string? PasswordHash { get; set; }
        public bool LocalNetworkOnly { get; set; } = false;
        public int? DownloadLimit { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}
