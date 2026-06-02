using AgentFlow.Api.Options;
using AgentFlow.API.Data;
using AgentFlow.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AgentFlow.API.Services
{
    public class LocalFileService : IFileService
    {
        private readonly AgentFlowDBContext _dbContext;
        private readonly FileStorageOptions _options;
        private readonly ILogger<LocalFileService> _logger;

        public LocalFileService(AgentFlowDBContext dbContext, IOptions<FileStorageOptions> options, ILogger<LocalFileService> logger)
        {
            _dbContext = dbContext;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<(Stream fileStream, string fileName, string contentType)> DownloadFileAsync(
        Guid fileId,
        CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Downloading file: {FileId}", fileId);

            var attachment = await _dbContext.FileAttachments
                .FirstOrDefaultAsync(f => f.Id == fileId, cancellationToken);

            if (attachment == null)
            {
                throw new FileNotFoundException($"File {fileId} not found.");
            }

            if (!System.IO.File.Exists(attachment.StoredPath))
            {
                throw new FileNotFoundException($"File {fileId} exists in database but not on disk.");
            }

            var stream = new FileStream(attachment.StoredPath, FileMode.Open, FileAccess.Read);
            return (stream, attachment.Name, attachment.ContentType);
        }

        public async Task<FileAttachment> UploadFileAsync(IFormFile file, string userId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("User {UserId} uploading file: {FileName}, Size: {Size} bytes",
                userId, file.FileName, file.Length);

            // Validate file size
            if (file.Length > _options.MaxFileSizeBytes)
            {
                throw new InvalidOperationException($"File exceeds maximum size of {_options.MaxFileSizeBytes} bytes.");
            }

            // Validate extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (_options.AllowedExtensions.Any() && !_options.AllowedExtensions.Contains(extension))
            {
                throw new InvalidOperationException($"File type {extension} is not allowed.");
            }

            // Generate unique storage path
            var fileId = Guid.NewGuid();
            var storageFolder = Path.Combine(_options.UploadPath, userId.ToString());
            Directory.CreateDirectory(storageFolder);

            var storedFileName = $"{fileId}{extension}";
            var storedPath = Path.Combine(storageFolder, storedFileName);

            // Save file to disk
            using (var stream = new FileStream(storedPath, FileMode.Create))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            // Save metadata to database
            var attachment = new FileAttachment
            {
                Id = fileId,
                Name = file.FileName,
                ContentType = file.ContentType,
                SizeBytes = file.Length,
                StoredPath = storedPath,
                UploadedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.FileAttachments.Add(attachment);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("File uploaded successfully: {FileId}", fileId);
            return attachment;
        }
    }
}