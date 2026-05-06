using BuildingBlock.Application.Abstraction.Media;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BuildingBlock.Infrastracture.Service
{
    public class MediaService : IMediaService
    {
        private readonly ILogger<MediaService> _logger;
        private readonly string _fileSavePath = "./wwwroot/Media";

        public MediaService(ILogger<MediaService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger is not configured.");
        }

        public async Task<string> SaveAsync(IFormFile mediaFile, string folderName)
        {
            if (mediaFile == null)
                throw new ArgumentNullException(nameof(mediaFile), "Media file cannot be null.");

            _logger.LogInformation("Saving media file: {FileName} to folder: {FolderName}", mediaFile.FileName, folderName);

            var filePath = GetUniqueFilePath(folderName, Path.GetExtension(mediaFile.FileName));
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

            try
            {
                await using var stream = new FileStream(filePath, FileMode.Create);
                await mediaFile.CopyToAsync(stream);
                _logger.LogInformation("File saved successfully: {FilePath}", filePath);

                return Path.GetFileName(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving file: {FileName}", mediaFile.FileName);
                throw;
            }
        }

        public async Task<Stream> GetStream(IFormFile formFile)
        {
            if (formFile == null)
                throw new ArgumentNullException(nameof(formFile), "Form file cannot be null.");

            _logger.LogInformation("Converting form file to stream: {FileName}", formFile.FileName);

            var memoryStream = new MemoryStream();
            await formFile.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            return memoryStream;
        }

        public async Task<string> SaveVideoAsync(IFormFile videoFile, string folderName)
        {
            if (videoFile == null)
                throw new ArgumentNullException(nameof(videoFile), "Video file cannot be null.");

            _logger.LogInformation("Saving video file: {FileName} to folder: {FolderName}", videoFile.FileName, folderName);

            var filePath = GetUniqueFilePath(folderName, Path.GetExtension(videoFile.FileName));
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

            try
            {
                await using var stream = new FileStream(filePath, FileMode.Create);
                await videoFile.CopyToAsync(stream);
                _logger.LogInformation("Video saved successfully: {FilePath}", filePath);
                return Path.GetFileName(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving video file: {FileName}", videoFile.FileName);
                throw;
            }
        }

        private string GetUniqueFilePath(string folderName, string extension)
        {
            var fileName = $"{Guid.NewGuid()}{extension}";
            return Path.Combine(_fileSavePath, folderName, fileName).Replace("\\", "/");
        }

        public async Task<List<string>> SaveAsync(List<IFormFile> formFiles, string folderName)
        {
            var uploadDirectory = Path.Combine(_fileSavePath, folderName);

            ArgumentException.ThrowIfNullOrEmpty(uploadDirectory, "UploadDirectory is not configured.");

            var filePaths = new List<string>();

            foreach (var file in formFiles)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var filePath = Path.Combine(uploadDirectory, fileName);

                if (!Directory.Exists(uploadDirectory))
                {
                    Directory.CreateDirectory(uploadDirectory);
                }

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                filePath = filePath.Replace("\\", "/");

                filePaths.Add(filePath);
            }
            return filePaths;
        }

        public void Remove(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public void RemoveRange(IEnumerable<string> filePaths)
        {
            foreach (var filePath in filePaths)
            {
                Remove(filePath);
            }
        }
    }
}