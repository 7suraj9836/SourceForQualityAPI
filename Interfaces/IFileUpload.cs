using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace SourceforqualityAPI.Interfaces
{
    public interface IFileUpload
    {
        Task<string> UploadFileToServer(IFormFile file);
        Task<int> UploadFile(IFormFile file);
        Task<string> GetFile(string file, string fileType);
        Task<string> DeleteFile(string FileStorageName);
    }
}
