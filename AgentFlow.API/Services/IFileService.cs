using AgentFlow.API.Models;

namespace AgentFlow.API.Services
{
    public interface IFileService
    {
        Task<FileAttachment> UploadFileAsync(IFormFile file, string userId, CancellationToken cancellationToken);

        Task<(Stream fileStream, string fileName, string contentType)> DownloadFileAsync(Guid fileId, CancellationToken cancellationToken = default);
    }
}