using Microsoft.AspNetCore.Http;

namespace SourceforqualityAPI.Interfaces
{
    public interface IFileFormatConverter
    {
        IFormFile Base64ToIFormFile(string Base64FileString, string FileName);
        string ToBase64(IFormFile File);


    }
}
