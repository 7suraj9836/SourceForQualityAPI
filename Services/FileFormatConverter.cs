using Microsoft.AspNetCore.Http;
using SourceforqualityAPI.Interfaces;
using System.Collections.Generic;
using System.IO;
using System;

namespace SourceforqualityAPI.Services
{
    public class FileFormatConverter : IFileFormatConverter
    {       

        public string ToBase64(IFormFile File)
        {
            string base64 = "";
            using (var ms = new MemoryStream())
            {
                File.CopyTo(ms);
                var fileBytes = ms.ToArray();
                base64 = Convert.ToBase64String(fileBytes);
            }
            return base64;
        }

        public IFormFile Base64ToIFormFile(string Base64FileString,string FileName)
        {
            FormFile formFile = null; 
            byte[] bytes = Convert.FromBase64String(Base64FileString.Substring(Base64FileString.IndexOf(",")+1));
            MemoryStream stream = new MemoryStream(bytes);

            formFile = new FormFile(stream, 0, bytes.Length, FileName, FileName)
            {
                Headers = new HeaderDictionary()
            };
            var colonPlace= Base64FileString.IndexOf(':');
            var semiColon= Base64FileString.IndexOf(';');
            formFile.ContentType = Base64FileString.Substring(colonPlace+1,semiColon-colonPlace-1);
            return formFile;
        }

       
    }
}
