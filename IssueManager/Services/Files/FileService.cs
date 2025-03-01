using IssueManager.Data;
using IssueManager.Exceptions;
using IssueManager.Models;

namespace IssueManager.Services.Files
{
    public class FileService : IFileService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FileService> _logger; 

        private readonly string[] _allowedExtensions = { ".jpg", ".png", ".pdf", ".docx", ".doc", ".txt" };
        private const int _maxFileSize = 2 * 1024 * 1024; // 2MB

        public FileService(ApplicationDbContext context, ILogger<FileService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Attachment>> ProcessFilesAsync(IEnumerable<IFormFile> files)
        {
            _logger.LogInformation("Processing {FileCount} files", files.Count());

            var attachments = new List<Attachment>();
            var processedFiles = 0;
            var skippedFiles = 0;

            try
            {
                foreach (var file in files)
                {
                    _logger.LogDebug("Processing file: {FileName} ({FileSize} bytes)", file.FileName, file.Length);

                    if (file.Length == 0)
                    {
                        _logger.LogWarning("Skipped empty file: {FileName}", file.FileName);
                        skippedFiles++;
                        continue;
                    }

                    if (file.Length >= _maxFileSize)
                    {
                        _logger.LogWarning("File {FileName} exceeds size limit ({FileSize} bytes)", file.FileName, file.Length);
                        skippedFiles++;
                        continue;
                    }

                    var fileExtension = Path.GetExtension(file.FileName).ToLower();
                    if (!_allowedExtensions.Contains(fileExtension))
                    {
                        _logger.LogError("Invalid file type {FileType} for {FileName}", fileExtension, file.FileName);
                        throw new FileProcessingException("Invalid file type.");
                    }

                    try
                    {
                        using var memoryStream = new MemoryStream();
                        await file.CopyToAsync(memoryStream);

                        attachments.Add(new Attachment
                        {
                            FileName = file.FileName,
                            ContentType = file.ContentType,
                            FileData = memoryStream.ToArray()
                        });

                        _logger.LogDebug("Successfully processed {FileName}", file.FileName);
                        processedFiles++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing file {FileName}", file.FileName);
                        throw new FileProcessingException("File processing failed", ex);
                    }
                }

                _logger.LogInformation("File processing completed. Success: {ProcessedFiles}, Skipped: {SkippedFiles}", processedFiles, skippedFiles);

                return attachments;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "File processing failed");
                throw;
            }
        }

        public async Task<Attachment?> GetAttachmentAsync(int? id)
        {
            _logger.LogInformation("Retrieving attachment {AttachmentId}", id);

            if (id == null)
            {
                _logger.LogWarning("Null attachment ID requested");
                return null;
            }

            try
            {
                var attachment = await _context.Attachments.FindAsync(id);
                if (attachment == null)
                {
                    _logger.LogWarning("Attachment {AttachmentId} not found", id);
                }
                else
                {
                    _logger.LogDebug("Found attachment {AttachmentId}: {FileName}", id, attachment.FileName);
                }
                return attachment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving attachment {AttachmentId}", id);
                throw new FileProcessingException("Attachment retrieval failed", ex);
            }
        }
    }
}
