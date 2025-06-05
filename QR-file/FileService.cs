using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using QRCoder;
using QRFileManager.Data;
using QRFileManager.Models;
using System.Drawing;
using System.Drawing.Imaging;

namespace QRFileManager.Services
{
    public class FileService
    {
        private readonly FileDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileService> _logger;
        private readonly string _uploadPath;
        private readonly string _qrCodePath;

        public FileService(FileDbContext context, IWebHostEnvironment environment, ILogger<FileService> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;

            _uploadPath = Path.Combine(_environment.WebRootPath, "uploads");
            _qrCodePath = Path.Combine(_environment.WebRootPath, "qr-codes");

            // Створюємо директорії якщо не існують
            Directory.CreateDirectory(_uploadPath);
            Directory.CreateDirectory(_qrCodePath);
        }

        public async Task<List<FileModel>> GetAllFilesAsync()
        {
            return await _context.Files
                .Where(f => f.IsActive)
                .OrderByDescending(f => f.UploadDate)
                .ToListAsync();
        }

        public async Task<FileModel?> GetFileByIdAsync(string id)
        {
            return await _context.Files.FirstOrDefaultAsync(f => f.Id == id && f.IsActive);
        }

        public async Task<FileModel> UploadFileAsync(IFormFile file, string uploaderIP)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Файл не може бути пустим");

            var fileId = Guid.NewGuid().ToString();
            var extension = Path.GetExtension(file.FileName);
            var storedName = $"{fileId}{extension}";
            var filePath = Path.Combine(_uploadPath, storedName);

            // Зберігаємо файл на диск
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Створюємо запис в БД
            var fileModel = new FileModel
            {
                Id = fileId,
                OriginalName = file.FileName,
                StoredName = storedName,
                MimeType = file.ContentType,
                FileSize = file.Length,
                UploaderIP = uploaderIP,
                UploadDate = DateTime.UtcNow // Використовуємо UTC час
            };

            _context.Files.Add(fileModel);
            await _context.SaveChangesAsync();

            // Генеруємо QR-код
            await GenerateQRCodeAsync(fileModel);

            return fileModel;
        }

        public async Task<bool> DeleteFileAsync(string id)
        {
            var file = await GetFileByIdAsync(id);
            if (file == null) return false;

            try
            {
                // Видаляємо файл з диску
                var filePath = Path.Combine(_uploadPath, file.StoredName);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                // Видаляємо QR-код
                if (!string.IsNullOrEmpty(file.QRCodePath))
                {
                    var qrPath = Path.Combine(_environment.WebRootPath, file.QRCodePath.TrimStart('/'));
                    if (File.Exists(qrPath))
                    {
                        File.Delete(qrPath);
                    }
                }

                // Позначаємо як неактивний замість повного видалення
                file.IsActive = false;
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка при видаленні файлу {FileId}", id);
                return false;
            }
        }

        public async Task<(byte[] fileBytes, string contentType, string fileName)?> DownloadFileAsync(string id)
        {
            var file = await GetFileByIdAsync(id);
            if (file == null) return null;

            var filePath = Path.Combine(_uploadPath, file.StoredName);
            if (!File.Exists(filePath)) return null;

            try
            {
                // Збільшуємо лічильник скачувань
                file.DownloadCount++;
                await _context.SaveChangesAsync();

                var fileBytes = await File.ReadAllBytesAsync(filePath);
                return (fileBytes, file.MimeType, file.OriginalName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка при завантаженні файлу {FileId}", id);
                return null;
            }
        }

        private async Task GenerateQRCodeAsync(FileModel file)
        {
            try
            {
                // Генеруємо повний URL для QR-коду
                var downloadUrl = $"https://yourdomain.com/api/files/download/{file.Id}";
                // Або використовуйте відносний шлях: var downloadUrl = $"/api/files/download/{file.Id}";

                var qrGenerator = new QRCodeGenerator();
                var qrCodeData = qrGenerator.CreateQrCode(downloadUrl, QRCodeGenerator.ECCLevel.Q);
                var qrCode = new QRCode(qrCodeData);

                // Створюємо bitmap без using для qrCode та qrCodeData
                using var qrCodeImage = qrCode.GetGraphic(20); 

                var qrFileName = $"{file.Id}.png";
                var qrFilePath = Path.Combine(_qrCodePath, qrFileName);

                qrCodeImage.Save(qrFilePath, ImageFormat.Png);

                file.QRCodePath = $"/qr-codes/{qrFileName}";
                await _context.SaveChangesAsync();

                // Очищаємо ресурси
                qrCode.Dispose(); 
                qrCodeData.Dispose();
                qrGenerator.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка генерації QR-коду для файлу {FileId}", file.Id);
            }
        }

        // Додатковий метод для отримання статистики файлу
        public async Task<FileModel?> GetFileStatsAsync(string id)
        {
            return await _context.Files
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == id && f.IsActive);
        }

        // Метод для очищення старих файлів
        public async Task<int> CleanupOldFilesAsync(int daysOld = 30)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
            var oldFiles = await _context.Files
                .Where(f => f.UploadDate < cutoffDate && f.IsActive)
                .ToListAsync();

            var deletedCount = 0;
            foreach (var file in oldFiles)
            {
                if (await DeleteFileAsync(file.Id))
                {
                    deletedCount++;
                }
            }

            return deletedCount;
        }
    }
}