using Microsoft.AspNetCore.Http;

namespace BuildingBlock.Application.Abstraction.Media
{
    public interface IMediaService
    {
        Task<string> SaveAsync(IFormFile mediaFile, string folderName);

        Task<List<string>> SaveAsync(List<IFormFile> formFiles, string folderName);

        Task<Stream> GetStream(IFormFile formFile);

        void Remove(string filePath);

        void RemoveRange(IEnumerable<string> filePaths);

        Task<string> SaveVideoAsync(IFormFile videoFile, string folderName);
    }
}