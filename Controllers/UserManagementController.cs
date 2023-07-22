using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SourceforqualityAPI.Interfaces;
using SourceforqualityAPI.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using iTextSharp.text.pdf;
using System.Net.Http.Headers;
using SourceforqualityAPI.Common;
using System.Data.SqlClient;
using Dapper;
using iTextSharp.text;
using System.Text;

namespace SourceforqualityAPI.Controllers
{
    public class UserManagementController : Controller
    {
        private readonly IUserManagementServices _userManagementServices;

        public UserManagementController(IUserManagementServices userManagementServices)
        {
            _userManagementServices = userManagementServices;
        }

        [Authorize(policy: "RequireAdminRole")]
        [HttpGet("GetUserDetails")]
        public async Task<ResponseModel<List<UserManagement>>> GetUserManagement(int pageNumber, int pageSize)
        {
            var res = new ResponseModel<List<UserManagement>>();
            try
            {
                res.Data =  _userManagementServices.GetUserManagement(pageNumber, pageSize);

                res.StatusCode = 200;
                res.Message = "User Data Fetched Successfully";
            }
            catch (Exception ex)
            {
                res.Data = null;
                res.Message = ex.Message;
                res.StatusCode = 400;
            }
            return res;
            //return Ok(_userManagementServices.GetUserManagement());
        }

        [Authorize(policy: "RequireAdminRole")]
        [HttpPost("DeleteUser/{id}")]
        public async Task<ResponseModel<string>> DeleteUser(int id)
        {
            var res = new ResponseModel<string>();
            try
            {
                res.Data = _userManagementServices.DeleteUser(id);
                res.StatusCode = 200;
                if(res.Data== "User does not exist")
                {
                    res.Message = "User does not exist";
                }
                else {
                    res.Message = "User Deleted Successfully";
                }
                
            }
            catch (Exception ex)
            {
                res.Data = null;
                res.Message = ex.Message;
                res.StatusCode = 400;
            }
            return res;

            //return Ok(_userManagementServices.DeleteUser(id));
        }

        [Authorize(policy: "RequireAdminRole")]
        [HttpPost("UserActiveStatus/{id}")]
        public async Task<ResponseModel<string>> UserAccountStatus(int id, string AccountStatus)
        {
            var res = new ResponseModel<string>();
            try
            {
                res.Data = _userManagementServices.UserAccountStatus(id,AccountStatus);
                res.StatusCode = 200;
                if (res.Data == "User status is InActive now")
                {
                    res.Message = "User status Is InActive Now";
                }
                else
                {
                    res.Message = "User status is Active";
                }

            }
            catch (Exception ex)
            {
                res.Data = null;
                res.Message = ex.Message;
                res.StatusCode = 400;
            }
            return res;

            //return Ok(_userManagementServices.DeleteUser(id));
        }

        [Authorize(policy: "RequireAdminRole")]
        [HttpPost("UserInActiveStatus/{id}")]
        public async Task<ResponseModel<string>> UserInActiveStatus(int id)
        {
            var res = new ResponseModel<string>();
            try
            {
                res.Data = _userManagementServices.UserInActiveStatus(id);
                res.StatusCode = 200;
                if (res.Data == "User status not changed")
                {
                    res.Message = "User status not changed";
                }
                else
                {
                    res.Message = "User status is inactive";
                }

            }
            catch (Exception ex)
            {
                res.Data = null;
                res.Message = ex.Message;
                res.StatusCode = 400;
            }
            return res;

            //return Ok(_userManagementServices.DeleteUser(id));
        }

        [HttpGet]
        [Route("api/users/export")]
        public async Task<IActionResult> ExportUsers(int pageNumber, int pageSize, string format)
        {
            var Response = await GetUserManagement(pageNumber, pageSize);

            if (format == "pdf")
            {
                // Create a new PDF document
                var document = new iTextSharp.text.Document();
                var output = new MemoryStream();
                var writer = PdfWriter.GetInstance(document, output);
                document.Open();

                // Add user data to the PDF document
                foreach (var user in Response.Data)
                {
                    // Create a table with columns for the user data
                    var table = new PdfPTable(6);
                    table.WidthPercentage = 100;

                    // Add header row to the table
                    table.AddCell(new PdfPCell(new Phrase("ID")));
                    table.AddCell(new PdfPCell(new Phrase("First Name")));
                    table.AddCell(new PdfPCell(new Phrase("Last Name")));
                    table.AddCell(new PdfPCell(new Phrase("Email")));
                    table.AddCell(new PdfPCell(new Phrase("Mobile No.")));
                    table.AddCell(new PdfPCell(new Phrase("Active")));

                    // Add data row to the table
                    table.AddCell(new PdfPCell(new Phrase(user.Id.ToString())));
                    table.AddCell(new PdfPCell(new Phrase(user.FirstName)));
                    table.AddCell(new PdfPCell(new Phrase(user.LastName)));
                    table.AddCell(new PdfPCell(new Phrase(user.Email)));
                    table.AddCell(new PdfPCell(new Phrase(user.MobileNo)));
                    table.AddCell(new PdfPCell(new Phrase(user.IsActive.ToString())));

                    // Add the table to the PDF document
                    document.Add(table);
                }

                document.Close();

                // Set the HTTP response headers to indicate that the response is a PDF file
                return File(output.ToArray(), "application/pdf", "UserManagement.pdf");
            }
            else if (format == "csv")
            {
                // Create a new CSV file
                var csv = new StringBuilder();

                // Add header row to the CSV file
                csv.AppendLine("ID,First Name,Last Name,Email,Mobile No.,Active");

                var users = Response.Data;
                // Add user data to the CSV file
                foreach (var user in users)
                {
                    csv.AppendLine(string.Format("{0},{1},{2},{3},{4},{5}", user.Id, user.FirstName, user.LastName, user.Email, user.MobileNo, user.IsActive));
                }

                // Set the HTTP response headers to indicate that the response is a CSV file
                return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "UserManagement.csv");
            }

            return BadRequest("Unsupported format");
        }

        //[HttpGet]
        //[Route("api/users/export")]
        //public async Task<ResponseModel<FileContentResult>> ExportUsers(int pageNumber, int pageSize, string format)
        //{
        //    var ApiResponse = new ResponseModel<FileContentResult>();
        //    try
        //    {
        //        var Response = await GetUserManagement(pageNumber, pageSize);

        //        if (format == "pdf")
        //        {
        //            // Create a new PDF document
        //            var document = new iTextSharp.text.Document();
        //            var output = new MemoryStream();
        //            var writer = PdfWriter.GetInstance(document, output);
        //            document.Open();

        //            // Add user data to the PDF document
        //            foreach (var user in Response.Data)
        //            {
        //                // Create a table with columns for the user data
        //                var table = new PdfPTable(6);
        //                table.WidthPercentage = 100;

        //                // Add header row to the table
        //                table.AddCell(new PdfPCell(new Phrase("ID")));
        //                table.AddCell(new PdfPCell(new Phrase("First Name")));
        //                table.AddCell(new PdfPCell(new Phrase("Last Name")));
        //                table.AddCell(new PdfPCell(new Phrase("Email")));
        //                table.AddCell(new PdfPCell(new Phrase("Mobile No.")));
        //                table.AddCell(new PdfPCell(new Phrase("Active")));

        //                // Add data row to the table
        //                table.AddCell(new PdfPCell(new Phrase(user.Id.ToString())));
        //                table.AddCell(new PdfPCell(new Phrase(user.FirstName)));
        //                table.AddCell(new PdfPCell(new Phrase(user.LastName)));
        //                table.AddCell(new PdfPCell(new Phrase(user.Email)));
        //                table.AddCell(new PdfPCell(new Phrase(user.MobileNo)));
        //                table.AddCell(new PdfPCell(new Phrase(user.IsActive.ToString())));

        //                // Add the table to the PDF document
        //                document.Add(table);
        //            }

        //            document.Close();

        //            // Set the HTTP response headers to indicate that the response is a PDF file
        //            var response = new HttpResponseMessage(HttpStatusCode.OK);
        //            response.Content = new ByteArrayContent(output.ToArray());
        //            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
        //            response.Content.Headers.ContentDisposition.FileName = "users.pdf";
        //            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        //            var responseFiledata = File(output.ToArray(), "application/pdf", "UserManagement.pdf");
        //            //var pdfBase64 = Convert.ToBase64String(output.ToArray());
        //            ApiResponse.Data = responseFiledata;
        //            ApiResponse.StatusCode = 200;
        //            ApiResponse.Message = "PDF Generated Successfully";
        //            return ApiResponse;

        //            //return File(output.ToArray(), "application/pdf", "UserManagement.pdf");
        //        }
        //        else if (format == "csv")
        //        {
        //            // Create a new CSV file
        //            var csv = new StringBuilder();

        //            // Add header row to the CSV file
        //            csv.AppendLine("ID,First Name,Last Name,Email,Mobile No.,Active");

        //            var users = Response.Data;
        //            // Add user data to the CSV file
        //            foreach (var user in users)
        //            {
        //                csv.AppendLine(string.Format("{0},{1},{2},{3},{4},{5}", user.Id, user.FirstName, user.LastName, user.Email, user.MobileNo, user.IsActive));
        //            }

        //            // Set the HTTP response headers to indicate that the response is a CSV file
        //            var response = new HttpResponseMessage(HttpStatusCode.OK);
        //            response.Content = new StringContent(csv.ToString());
        //            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
        //            response.Content.Headers.ContentDisposition.FileName = "users.csv";
        //            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
        //            var responseFiledata = File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "UserManagement.csv");
        //            ApiResponse.Data = responseFiledata;
        //            ApiResponse.StatusCode = 200;
        //            ApiResponse.Message = "CSV Generated Successfully";
        //            return ApiResponse;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ApiResponse.Data = null;
        //        ApiResponse.Message = ex.Message;
        //        ApiResponse.StatusCode = 400;
        //        return ApiResponse;
        //    }
        //    ApiResponse.Data = null;
        //    ApiResponse.Message = "Unsupported format";
        //    ApiResponse.StatusCode = 400;
        //    return ApiResponse;

        //}

        //public HttpResponseMessage ExportUsers(int pageNumber, int pageSize)
        //{
        //    try
        //    {
        //        var users = GetUserManagement(pageNumber, pageSize);

        //        // Create a new PDF document
        //        var document = new System.Reflection.Metadata.Document();
        //        var output = new MemoryStream();
        //        var writer = PdfWriter.GetInstance(document, output);
        //        document.Open();

        //        // Add user data to the PDF document
        //        foreach (var user in users)
        //        {
        //            // Create a table with columns for the user data
        //            var table = new PdfPTable(6);
        //            table.WidthPercentage = 100;

        //            // Add header row to the table
        //            table.AddCell(new PdfPCell(new Phrase("ID")));
        //            table.AddCell(new PdfPCell(new Phrase("First Name")));
        //            table.AddCell(new PdfPCell(new Phrase("Last Name")));
        //            table.AddCell(new PdfPCell(new Phrase("Email")));
        //            table.AddCell(new PdfPCell(new Phrase("Mobile No.")));
        //            table.AddCell(new PdfPCell(new Phrase("Active")));

        //            // Add data row to the table
        //            table.AddCell(new PdfPCell(new Phrase(user.Id.ToString())));
        //            table.AddCell(new PdfPCell(new Phrase(user.FirstName)));
        //            table.AddCell(new PdfPCell(new Phrase(user.LastName)));
        //            table.AddCell(new PdfPCell(new Phrase(user.Email)));
        //            table.AddCell(new PdfPCell(new Phrase(user.MobileNo)));
        //            table.AddCell(new PdfPCell(new Phrase(user.IsActive.ToString())));

        //            // Add the table to the PDF document
        //            document.Add(table);
        //        }

        //        document.Close();

        //        // Set the HTTP response headers to indicate that the response is a PDF file
        //        var response = new HttpResponseMessage(HttpStatusCode.OK);
        //        response.Content = new ByteArrayContent(output.ToArray());
        //        response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
        //        response.Content.Headers.ContentDisposition.FileName = "users.pdf";
        //        response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

        //        return response;
        //    }
        //    catch (Exception ex)
        //    {
        //        // Handle and log the exception
        //        return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
        //    }
        //}

        //[HttpGet]
        //[Route("api/users/export")]
        //public async Task<IActionResult> ExportUsers(int pageNumber, int pageSize, string format)
        //{
        //    var Response = await GetUserManagement(pageNumber, pageSize);

        //    if (format == "pdf")
        //    {
        //        // Create a new PDF document
        //        var document = new iTextSharp.text.Document();
        //        var output = new MemoryStream();
        //        var writer = PdfWriter.GetInstance(document, output);
        //        document.Open();

        //        // Add user data to the PDF document
        //        foreach (var user in Response.Data)
        //        {
        //            // Create a table with columns for the user data
        //            var table = new PdfPTable(6);
        //            table.WidthPercentage = 100;

        //            // Add header row to the table
        //            table.AddCell(new PdfPCell(new Phrase("ID")));
        //            table.AddCell(new PdfPCell(new Phrase("First Name")));
        //            table.AddCell(new PdfPCell(new Phrase("Last Name")));
        //            table.AddCell(new PdfPCell(new Phrase("Email")));
        //            table.AddCell(new PdfPCell(new Phrase("Mobile No.")));
        //            table.AddCell(new PdfPCell(new Phrase("Active")));

        //            // Add data row to the table
        //            table.AddCell(new PdfPCell(new Phrase(user.Id.ToString())));
        //            table.AddCell(new PdfPCell(new Phrase(user.FirstName)));
        //            table.AddCell(new PdfPCell(new Phrase(user.LastName)));
        //            table.AddCell(new PdfPCell(new Phrase(user.Email)));
        //            table.AddCell(new PdfPCell(new Phrase(user.MobileNo)));
        //            table.AddCell(new PdfPCell(new Phrase(user.IsActive.ToString())));

        //            // Add the table to the PDF document
        //            document.Add(table);
        //        }

        //        document.Close();

        //        // Set the HTTP response headers to indicate that the response is a PDF file
        //        return File(output.ToArray(), "application/pdf", "UserManagement.pdf");
        //    }
        //    else if (format == "csv")
        //    {
        //        // Create a new CSV file
        //        var csv = new StringBuilder();

        //        // Add header row to the CSV file
        //        csv.AppendLine("ID,First Name,Last Name,Email,Mobile No.,Active");

        //        var users = Response.Data;
        //        // Add user data to the CSV file
        //        foreach (var user in users)
        //        {
        //            csv.AppendLine(string.Format("{0},{1},{2},{3},{4},{5}", user.Id, user.FirstName, user.LastName, user.Email, user.MobileNo, user.IsActive));
        //        }

        //        // Set the HTTP response headers to indicate that the response is a CSV file
        //        return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "UserManagement.csv");
        //    }

        //    return BadRequest("Unsupported format");
        //}


        //[HttpGet]
        //[Route("api/users/export")]
        //public async Task<ActionResult<ResponseModel<DownloadLinkModel>>> ExportUsers(int pageNumber, int pageSize, string format)
        //{
        //    var response = new ResponseModel<DownloadLinkModel>();

        //    var Response = await GetUserManagement(pageNumber, pageSize);

        //    if (format == "pdf")
        //    {
        //        // Create a new PDF document
        //        var document = new iTextSharp.text.Document();
        //        var output = new MemoryStream();
        //        var writer = PdfWriter.GetInstance(document, output);
        //        document.Open();

        //        // Add user data to the PDF document
        //        foreach (var user in Response.Data)
        //        {
        //            // Create a table with columns for the user data
        //            var table = new PdfPTable(6);
        //            table.WidthPercentage = 100;

        //            // Add header row to the table
        //            table.AddCell(new PdfPCell(new Phrase("ID")));
        //            table.AddCell(new PdfPCell(new Phrase("First Name")));
        //            table.AddCell(new PdfPCell(new Phrase("Last Name")));
        //            table.AddCell(new PdfPCell(new Phrase("Email")));
        //            table.AddCell(new PdfPCell(new Phrase("Mobile No.")));
        //            table.AddCell(new PdfPCell(new Phrase("Active")));

        //            // Add data row to the table
        //            table.AddCell(new PdfPCell(new Phrase(user.Id.ToString())));
        //            table.AddCell(new PdfPCell(new Phrase(user.FirstName)));
        //            table.AddCell(new PdfPCell(new Phrase(user.LastName)));
        //            table.AddCell(new PdfPCell(new Phrase(user.Email)));
        //            table.AddCell(new PdfPCell(new Phrase(user.MobileNo)));
        //            table.AddCell(new PdfPCell(new Phrase(user.IsActive.ToString())));

        //            // Add the table to the PDF document
        //            document.Add(table);
        //        }

        //        document.Close();

        //        // Save the PDF file to a temporary location
        //        var tempFileName = Path.GetTempFileName();
        //        var pdfFilePath = Path.ChangeExtension(tempFileName, "pdf");
        //        System.IO.File.WriteAllBytes(pdfFilePath, output.ToArray());

        //        // Generate the download link
        //        var downloadLink = Url.Action("DownloadFile", "Users", new { fileName = Path.GetFileName(pdfFilePath) }, Request.Scheme);

        //        // Set the response data
        //        response.StatusCode = 200;
        //        response.Data = new DownloadLinkModel { Pdf = downloadLink };
        //        response.Message = "Download can be downloaded";

        //        return Ok(response);
        //    }
        //    else if (format == "csv")
        //    {
        //        // Create a new CSV file
        //        var csv = new StringBuilder();

        //        // Add header row to the CSV file
        //        csv.AppendLine("ID,First Name,Last Name,Email,Mobile No.,Active");

        //        var users = Response.Data;
        //        // Add user data to the CSV file
        //        foreach (var user in users)
        //        {
        //            csv.AppendLine(string.Format("{0},{1},{2},{3},{4},{5}", user.Id, user.FirstName, user.LastName, user.Email, user.MobileNo, user.IsActive));
        //        }

        //        // Save the CSV file to a temporary location
        //        var tempFileName = Path.GetTempFileName();
        //        var csvFilePath = Path.ChangeExtension(tempFileName, "csv");
        //        System.IO.File.WriteAllText(csvFilePath, csv.ToString(), Encoding.UTF8);

        //        // Generate the download link
        //        var downloadLink = Url.Action("DownloadFile", "Users", new { fileName = Path.GetFileName(csvFilePath) }, Request.Scheme);

        //        // Set the response data
        //        response.StatusCode = 200;
        //        response.Data = new DownloadLinkModel { Csv = downloadLink };
        //        response.Message = "Download can be downloaded";

        //        return Ok(response);
        //    }

        //    response.StatusCode = 400;
        //    response.Message = "Unsupported format";

        //    return BadRequest(response);
        //}

        //[HttpGet]
        //[Route("api/users/download")]
        //public IActionResult DownloadFile(string fileName)
        //{
        //    var filePath = Path.Combine(Path.GetTempPath(), fileName);

        //    if (!System.IO.File.Exists(filePath))
        //    {
        //        return NotFound();
        //    }

        //    var fileBytes = System.IO.File.ReadAllBytes(filePath);

        //    return File(fileBytes, "application/octet-stream", fileName);
        //}





    }
}
