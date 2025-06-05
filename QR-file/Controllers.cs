using Microsoft.AspNetCore.Mvc;
using QRFileManager.Models;
using QRFileManager.Services;

namespace QRFileManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilesController : ControllerBase
    {
        private readonly FileService _fileService;
        private readonly ILogger<FilesController> _logger;

        public FilesController(FileService fileService, ILogger<FilesController> logger)
        {
            _fileService = fileService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<FileModel>>> GetFiles()
        {
            try
            {
                var files = await _fileService.GetAllFilesAsync();
                return Ok(files);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка отримання списку файлів");
                return StatusCode(500, new { error = "Помилка сервера" });
            }
        }

        [HttpPost("upload")]
        public async Task<ActionResult> UploadFile(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { error = "Файл не завантажено" });
                }

                var uploaderIP = GetClientIP();
                var fileModel = await _fileService.UploadFileAsync(file, uploaderIP);

                return Ok(new { success = true, file = fileModel });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка завантаження файлу");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("download/{id}")]
        public async Task<ActionResult> DownloadFile(string id)
        {
            try
            {
                var result = await _fileService.DownloadFileAsync(id);
                if (result == null)
                {
                    return NotFound(new { error = "Файл не знайдено" });
                }

                var (fileBytes, contentType, fileName) = result.Value;
                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка скачування файлу {FileId}", id);
                return StatusCode(500, new { error = "Помилка сервера" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteFile(string id)
        {
            try
            {
                var success = await _fileService.DeleteFileAsync(id);
                if (!success)
                {
                    return NotFound(new { error = "Файл не знайдено" });
                }

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка видалення файлу {FileId}", id);
                return StatusCode(500, new { error = "Помилка сервера" });
            }
        }

        private string GetClientIP()
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            if (string.IsNullOrEmpty(ipAddress) || ipAddress == "::1")
            {
                ipAddress = "127.0.0.1";
            }
            return ipAddress;
        }
    }
}