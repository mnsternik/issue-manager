using IssueManager.Exceptions;
using IssueManager.Models;

namespace IssueManager.Services.Files
{
    public class FileService : IFileService
    {
        private readonly string[] _allowedExtensions = { ".jpg", ".png", ".pdf", ".docx", ".doc", ".txt" };
        private const int _maxFileSize = 2 * 1024 * 1024; // 2MB

        public async Task<List<Attachment>> ProcessFilesAsync(IEnumerable<IFormFile> files)
        {
            var attachments = new List<Attachment>();
            
            foreach (var file in files)
            {
                if (file.Length > 0 && file.Length < _maxFileSize)
                {
                    var fileExtenstion = Path.GetExtension(file.FileName).ToLower();

                    if (!_allowedExtensions.Contains(fileExtenstion))
                    {
                        throw new InvalidFileTypeException("Invalid file type."); 
                    }

                    using (var memoryStream = new MemoryStream())
                    {
                        await file.CopyToAsync(memoryStream);
                        attachments.Add(new Attachment
                        {
                            FileName = file.FileName,
                            ContentType = file.ContentType,
                            FileData = memoryStream.ToArray()
                        });
                    }
                }
            }

            return attachments;
        }
    }
}
