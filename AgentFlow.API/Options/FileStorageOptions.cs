namespace AgentFlow.Api.Options;

public class FileStorageOptions
{
    // Where to store files on disk
    public string UploadPath { get; set; } = "uploads";

    // Max file size in bytes (10 MB default)
    public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024;

    // Allowed extensions (empty = allow all)
    public List<string> AllowedExtensions { get; set; } = new();
}