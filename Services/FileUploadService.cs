using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SourceforqualityAPI.Common;
using SourceforqualityAPI.Interfaces;
using SourceforqualityAPI.Model;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace SourceforqualityAPI.Services
{
    public class FileUploadService : IFileUpload
    {
        private readonly ILogger logger;

        public FileUploadService(ILogger logger)
        {
            this.logger = logger;
        }

        public async Task<string> GetFile(string file,string fileType)
        {
            string data = "";
            if (string.IsNullOrEmpty(file))
                return "";
            try
            {
                //data: image / png; base64
                var prefix = "data:...Type...;base64,";
                prefix=prefix.Replace("...Type...",fileType);
                //string uploads = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Uploads");
                string uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads");
                string filePath = Path.Combine(uploads, file);
                Byte[] bytes = File.ReadAllBytes(filePath);
                data = prefix + Convert.ToBase64String(bytes);
            }
            catch (Exception ex)
            {
                data = "";
            }            
            return data;
        }

        public async Task<string> DeleteFile(string FileStorageName)
        {
            var data = "";
            if (string.IsNullOrEmpty(FileStorageName))
                return "";
            try
            {
                using (System.Data.IDbConnection con = new SqlConnection(Global.ConnectionString))
                {
                    if (con.State == System.Data.ConnectionState.Closed)
                    {
                        con.Open();
                    }
                    con.Execute(@"delete from FileUploadTable where FileStorageName=@FileStorageName", new { FileStorageName });
                    if (con.State == System.Data.ConnectionState.Closed)
                    {
                        con.Close();
                    }
                }
                //string uploads = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Uploads");
                try
                {
                    string uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads");
                    string filePath = Path.Combine(uploads, FileStorageName);
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError($"DeleteFiles - problem occured in delete file from disk - {ex.Message}");
                }     
            }
            catch (Exception ex)
            {
                logger.LogError($"DeleteFiles - problem occured in delete file from db - {ex.Message}");
                data = ex.Message;
            }
            return data;
        }

        public async Task<int> UploadFile(IFormFile file)
        {
            var FileName = file.FileName;
            var Type = file.ContentType;
            var fileStorageName = "";
            var FileID = 0;
            try
            {
                var data = await UploadFileToServer(file);
                //if (data.ToLower().Trim() == "failed")
                //{
                //    throw new Exception("File upload failed");
                //}
                fileStorageName = data;
                // insert data into FileUploadTable
                //select SCOPE_IDENTITY();
                using (System.Data.IDbConnection con = new SqlConnection(Global.ConnectionString))
                {
                    if (con.State == System.Data.ConnectionState.Closed)
                    {
                        con.Open();
                    }
                    FileUploadTable fileRecord=new FileUploadTable() { 
                        Id=0,
                        FileName=FileName, 
                        Type=Type,
                        FileStorageName=fileStorageName
                    };
                    var id = con.QuerySingle<int>(@"
                    insert into FileUploadTable(FileName,FileStorageName,Type) values (@FileName,@FileStorageName,@Type)
                    ;select SCOPE_IDENTITY();", fileRecord);
                    FileID=id;
                    if (con.State == System.Data.ConnectionState.Closed)
                    {
                        con.Close();
                    }
                }
                }
            catch (Exception ex)
            {
                throw ex;
            }
            return FileID;
        }

        public async Task<string> UploadFileToServer(IFormFile file)
        {
            string res = "";
        //    string uploads = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Uploads");
            string uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads");

            string fileName =Guid.NewGuid().ToString()+file.FileName;
            string filePath = Path.Combine(uploads, fileName);          
            
            try
            {
                using (Stream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
                res = fileName;
            }
            catch (Exception ex)
            {
                res = "Failed";
            }
            return res;
        }
    }
}
