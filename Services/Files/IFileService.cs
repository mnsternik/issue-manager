using IssueManager.Models;

namespace IssueManager.Services.Files
{
    public interface IFileService
    {
        public Task<List<Attachment>> ProcessFilesAsync(IEnumerable<IFormFile> files);
    }
}
