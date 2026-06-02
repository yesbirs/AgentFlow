using AgentFlow.Api.Services;
using AgentFlow.API.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgentFlow.Api.Controllers;

[ApiController]
[Route("v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class FilesController : ControllerBase
{
    private readonly IFileService _fileService;
    private readonly ILogger<FilesController> _logger;

    public FilesController(IFileService fileService, ILogger<FilesController> logger)
    {
        _fileService = fileService;
        _logger = logger;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var attachment = await _fileService.UploadFileAsync(file, userId, cancellationToken);

        return Ok(new
        {
            attachment.Id,
            attachment.Name,
            attachment.ContentType,
            attachment.SizeBytes,
            attachment.CreatedAt
        });
    }

    [HttpGet("download/{fileId}")]
    public async Task<IActionResult> Download(Guid fileId, CancellationToken cancellationToken)
    {
        try
        {
            var (stream, fileName, contentType) = await _fileService.DownloadFileAsync(fileId, cancellationToken);
            return File(stream, contentType, fileName);
        }
        catch (FileNotFoundException)
        {
            return NotFound(new { Message = $"File {fileId} not found." });
        }
    }
}