using Dapper;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SourceforqualityAPI.Common;
using SourceforqualityAPI.Contracts;
using SourceforqualityAPI.Interfaces;
using SourceforqualityAPI.Model;
using SourceforqualityAPI.Services;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Headers;

namespace SourceforqualityAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupplierProfileController : ControllerBase
    {

        private readonly ISupplierProfileServices _supplierProfileServices;

        public SupplierProfileController(ISupplierProfileServices supplierProfileServices)
        {
            _supplierProfileServices = supplierProfileServices;
        }


        //public IActionResult GetFoodSupplier()
        //{
        //    return Ok(_supplierProfileServices.GetFoodSupplier());

        //}
        [Authorize(policy: "RequireFoodSupplierAdmin")]
        [HttpGet("GetSupplierProfile")]
        public async Task<ResponseModel<List<FoodSupplierGetDto>>> GetFoodSupplier(int pageNumber, int pageSize)
        {
            var res = new ResponseModel<List<FoodSupplierGetDto>>();
            try
            {
                res.Data = _supplierProfileServices.GetFoodSupplier(pageNumber, pageSize);

                res.StatusCode = 200;
                res.Message = "Data Fetched Successfully ";
            }
            catch (Exception ex)
            {
                res.Data = null;
                res.Message = ex.Message;
                res.StatusCode = 400;
            }
            return res;

            
        }
       // [Authorize(policy: "RequireEndUserRole")]
        [HttpPost("SaveSupplierProfile")]
        public async Task<ResponseModel<string>> SaveSupplierProfile(FoodSupplierProfile foodSupplier)
        {
            var res = new ResponseModel<string>();
            try
            {
                res.Data = await _supplierProfileServices.SaveSupplierProfile(foodSupplier);
                if (res.Data != "Success")
                {
                    res.StatusCode = 400;
                    res.Message = "Unable To save supplier Data";
                    return res;
                }
                res.StatusCode = 200;
                res.Message = "Supplier Saved Successfully";
            }
            catch (Exception ex)
            {
                res.Data = null;
                res.Message = ex.Message;
                res.StatusCode = 400;
            }
            return res;
            //return await _oLogin.ForgotPassword(emailId);
        }

        // Controller to Save Temp Supplier data before Confirmation

        //[Authorize(policy: "RequireFoodSupplierRole")]
        //[HttpPost("TempSaveSupplierProfile")]
        //public async Task<ResponseModel<string>> TempSaveSupplierProfile(FoodSupplierProfile foodSupplier)
        //{
        //    var res = new ResponseModel<string>();
        //    try
        //    {
        //        res.Data = await _supplierProfileServices.TempSaveSupplierProfile(foodSupplier);
        //        if (res.Data != "Success")
        //        {
        //            res.StatusCode = 400;
        //            res.Message = "Unable To save supplier Data for Approval";
        //            return res;
        //        }
        //        res.StatusCode = 200;
        //        res.Message = "Data Saved Successfully";
        //    }
        //    catch (Exception ex)
        //    {
        //        res.Data = null;
        //        res.Message = ex.Message;
        //        res.StatusCode = 400;
        //    }
        //    return res;
        //    //return await _oLogin.ForgotPassword(emailId);
        //}



        //[Authorize(policy: "RequireEndUserRole")]
        [HttpPost("UpdateUserFavouriteSupplier")]
        public async Task<ResponseModel<string>> UpdateUserFavouriteSupplier(UserFavouriteSupplierDataModel inputModel)
        {
            var res = new ResponseModel<string>();
            try
            {
                res.Data = await _supplierProfileServices.UpdateUserFavouriteSupplier(inputModel);
                if (res.Data != "Success")
                {
                    res.StatusCode = 400;
                    res.Message = "Unable to save favourite supplier data ";
                    return res;
                }
                res.StatusCode = 200;
                res.Message = "Favourite Supplier Saved Successfully";
            }
            catch (Exception ex)
            {
                res.Data = null;
                res.Message = ex.Message;
                res.StatusCode = 400;
            }
            return res;
            //return await _oLogin.ForgotPassword(emailId);
        }



        //public IActionResult SaveSupplierProfile([FromForm] FoodSupplierProfile foodSupplier)
        //{
        //    return Ok(_supplierProfileServices.SaveSupplierProfile(foodSupplier));
        //}

        [HttpGet("GetFoodSupplierById/{id}")]
         [Authorize]
        //[Authorize(policy: "RequireFoodSupplierAdmin")]
        public async Task<ResponseModel<FoodSupplierOutputDto>> GetFoodSupplierById(int id)
        {
            var userId=Convert.ToInt32(User.Claims.Where(x => x.Type == "UserID").FirstOrDefault().Value);
            var res = new ResponseModel<FoodSupplierOutputDto>();
            try
            {
                res.Data = _supplierProfileServices.GetFoodSupplierById( id, userId);
                res.StatusCode = 200;
                res.Message = "Data Fetched Successfully ";
            }
            catch (Exception ex)
            {
                res.Data = null;
                res.Message = ex.Message;
                res.StatusCode = 400;
            }
            return res;
        }

        //Controller to Get the data of the Supplier from Temporary table
        [HttpGet("TempGetFoodSupplierById/{id}")]
        public async Task<ResponseModel<FoodSupplierOutputDto>> TempGetFoodSupplierById(int id)
        {
            var res = new ResponseModel<FoodSupplierOutputDto>();
            try
            {
                res.Data = _supplierProfileServices.TempGetFoodSupplierById(id);
                res.StatusCode = 200;
                res.Message = "Data Fetched Successfully ";
            }
            catch (Exception ex)
            {
                res.Data = null;
                res.Message = ex.Message;
                res.StatusCode = 400;
            }
            return res;
        }



        [Authorize(policy: "RequireFoodSupplierAdmin")]
        [HttpPost("UpdateFoodSupplier")]
        public async Task<ResponseModel<string>> UpdateFoodSupplier(FoodSupplierUpdateInputDTO foodSupplier)
        {
            var res = new ResponseModel<string>();
            try
            {
                res.Data = await _supplierProfileServices.TempUpdateFoodSupplier(foodSupplier);
                if (res.Data != "Success")
                {
                    res.StatusCode = 400;
                    res.Message = "Unable To Update Supplier Data";
                    return res;
                }
                res.StatusCode = 200;
                res.Message = "Supplier Updated Successfully";


                //res.Data = await _supplierProfileServices.UpdateFoodSupplier(foodSupplier);
                //res.StatusCode = 200;
                //res.Message = "Supplier Updated Successfully";
            }
            catch (Exception ex)
            {
                res.Data = null;
                res.Message = ex.Message;
                res.StatusCode = 400;
            }
            return res;
            //return await _oLogin.ForgotPassword(emailId);
        }


        //[HttpPost("TempUpdateFoodSupplier")]
        //public async Task<ResponseModel<string>> TempUpdateFoodSupplier(FoodSupplierUpdateInputDTO foodSupplier)
        //{
        //    var res = new ResponseModel<string>();
        //    try
        //    {
        //        res.Data = await _supplierProfileServices.TempUpdateFoodSupplier(foodSupplier);
        //        if (res.Data != "Success")
        //        {
        //            res.StatusCode = 400;
        //            res.Message = "Unable To Update Supplier Data";
        //            return res;
        //        }
        //        res.StatusCode = 200;
        //        res.Message = "Supplier Updated Successfully";


        //        //res.Data = await _supplierProfileServices.UpdateFoodSupplier(foodSupplier);
        //        //res.StatusCode = 200;
        //        //res.Message = "Supplier Updated Successfully";
        //    }
        //    catch (Exception ex)
        //    {
        //        res.Data = null;
        //        res.Message = ex.Message;
        //        res.StatusCode = 400;
        //    }
        //    return res;
        //    //return await _oLogin.ForgotPassword(emailId);
        //}


        [Authorize(policy: "RequireAdminRole")]
        [HttpPost("DeleteFoodSupplier/{id}")]
        public async Task<ResponseModel<string>> DeleteFoodSupplier(int id)
        {
            var res = new ResponseModel<string>();
            try
            {
                res.Data =   _supplierProfileServices.DeleteFoodSupplier(id);
                res.StatusCode = 200;
                if (res.Data == "Supplier does not exist")
                {
                    res.Message = "Supplier does not exist";
                }
                else {
                    res.Message = "Supplier Deleted Successfully";
                }

            }
            catch (Exception ex)
            {
                res.Data = null;
                res.Message = ex.Message;
                res.StatusCode = 400;
            }
            return res;
        }

        //{
        //            return Ok(_supplierProfileServices.DeleteFoodSupplier(id));
        //        }

        [Authorize(policy: "RequireAdminRole")]
        [HttpPost("SubscriptionStatusActive/{id}")]
        public async Task<ResponseModel<string>> SubscriptionStatusActive(int id)
        {
            var res = new ResponseModel<string>();
            try
            {
                res.Data = _supplierProfileServices.SubscriptionStatusActive(id);
                res.StatusCode = 200;
                if (res.Data == "Supplier Subscription Status is not Active")
                {
                    res.Message = "Supplier Subscription Status is not Active";
                }
                else
                {
                    res.Message = "Supplier Status is now set Active";
                }

            }
            catch (Exception ex)
            {
                res.Data = null;
                res.Message = ex.Message;
                res.StatusCode = 400;
            }
            return res;
        }

        [Authorize(policy: "RequireAdminRole")]
        [HttpPost("SubscriptionStatusInActive/{id}")]
        public async Task<ResponseModel<string>> SubscriptionStatusInActive(int id)
        {
            var res = new ResponseModel<string>();
            try
            {
                res.Data = _supplierProfileServices.SubscriptionStatusInActive(id);
                res.StatusCode = 200;
                if (res.Data == "Supplier Subscription Status is Active")
                {
                    res.Message = "Supplier Subscription Status is Active";
                }
                else
                {
                    res.Message = "Supplier Status is now set InActive";
                }

            }
            catch (Exception ex)
            {
                res.Data = null;
                res.Message = ex.Message;
                res.StatusCode = 400;
            }
            return res;
        }


        [HttpGet]
        [Route("api/Suppliers/export")]
        //public async Task<ResponseModel<FileContentResult>> ExportSupplier(int pageNumber, int pageSize, string format)
        public async Task<IActionResult> ExportSupplier(int pageNumber, int pageSize, string format)
        {
            var ApiResponse = new ResponseModel<FileContentResult>();
            try
            {

                var Response = await GetFoodSupplier(pageNumber, pageSize);

                if (format == "pdf")
                {
                    // Create a new PDF document
                    var document = new iTextSharp.text.Document();
                    var output = new MemoryStream();
                    var writer = PdfWriter.GetInstance(document, output);
                    document.Open();

                    // Create a table with columns for the user data
                    var table = new PdfPTable(10);
                    table.WidthPercentage = 100;
                  
                    // Add header row to the table
                    table.AddCell(new PdfPCell(new Phrase("ID")));
                    table.AddCell(new PdfPCell(new Phrase("Company Name")));
                    table.AddCell(new PdfPCell(new Phrase("Location")));
                    table.AddCell(new PdfPCell(new Phrase("Email Address")));
                    table.AddCell(new PdfPCell(new Phrase("Mobile Number")));
                    table.AddCell(new PdfPCell(new Phrase("Awards Title")));
                    table.AddCell(new PdfPCell(new Phrase("Qualifications")));
                    table.AddCell(new PdfPCell(new Phrase("Services")));
                    table.AddCell(new PdfPCell(new Phrase("Subscription Status")));
                    table.AddCell(new PdfPCell(new Phrase("Account Status")));

                    // Add user data to the PDF document
                    foreach (var user in Response.Data)
                    {
                        // Add data row to the table
                        table.AddCell(new PdfPCell(new Phrase(user.Id.ToString())));
                        table.AddCell(new PdfPCell(new Phrase(user.Name)));
                        table.AddCell(new PdfPCell(new Phrase(user.CountryId)));
                        table.AddCell(new PdfPCell(new Phrase(user.Email)));
                        table.AddCell(new PdfPCell(new Phrase(user.PhoneNumber)));
                        table.AddCell(new PdfPCell(new Phrase(user.AwardTitle)));
                        table.AddCell(new PdfPCell(new Phrase(user.CertificationsCategoryId.ToString())));
                        table.AddCell(new PdfPCell(new Phrase(user.BusinessActivityCategoryId.ToString())));
                        table.AddCell(new PdfPCell(new Phrase(user.SubscriptionStatus.ToString())));
                        table.AddCell(new PdfPCell(new Phrase(user.AccountStatus.ToString())));


                        // Add the table to the PDF document
                        document.Add(table);
                    }

                    document.Close();

                    // Set the HTTP response headers to indicate that the response is a PDF file
                    //var response = new HttpResponseMessage(HttpStatusCode.OK);
                    //response.Content = new ByteArrayContent(output.ToArray());
                    //response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                    //response.Content.Headers.ContentDisposition.FileName = "users.pdf";
                    //response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

                    //var responseFiledata = File(output.ToArray(), "application/pdf", "FoodSupplier.pdf");
                    ////var pdfBase64 = Convert.ToBase64String(output.ToArray());
                    //ApiResponse.Data = responseFiledata;
                    //ApiResponse.StatusCode = 200;
                    //ApiResponse.Message = "PDF Generated Successfully";
                    //return ApiResponse;
                    return File(output.ToArray(), "application/pdf", "FoodSupplier.pdf");
                }
                else if (format == "csv")
                {
                    // Create a new CSV file
                    var csv = new StringBuilder();

                    // Add header row to the CSV file
                    csv.AppendLine("ID,Company Name,Location,Email Address,Mobile Number,Awards Title,Qualifications,Services,Subscription Status,Account Status");

                    var users = Response.Data;
                    // Add user data to the CSV file
                    foreach (var user in users)
                    {
                        csv.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}", user.Id, user.Name, user.CountryId, user.Email, user.PhoneNumber, user.AwardTitle, user.CertificationsCategoryId, user.BusinessActivityCategoryId, user.SubscriptionStatus, user.AccountStatus));
                    }

                    // Set the HTTP response headers to indicate that the response is a CSV file
                    //var response = new HttpResponseMessage(HttpStatusCode.OK);
                    //response.Content = new StringContent(csv.ToString());
                    //response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                    //response.Content.Headers.ContentDisposition.FileName = "users.csv";
                    //response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/csv");

                    //var responseFiledata = File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "UserManagement.csv");
                    //ApiResponse.Data = responseFiledata;
                    //ApiResponse.StatusCode = 200;
                    //ApiResponse.Message = "CSV Generated Successfully";
                    //return ApiResponse;
                    return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "FoodSupplier.csv");
                }
            }
            catch (Exception ex)
            {
                //ApiResponse.Data = null;
                //ApiResponse.Message = ex.Message;
                //ApiResponse.StatusCode = 400;
                //return ApiResponse;
                return BadRequest("Unsupported format");
            }
            //ApiResponse.Data = null;
            //ApiResponse.Message = "Unsupported format";
            //ApiResponse.StatusCode = 400;
            //return ApiResponse;
            return BadRequest("Unsupported format");
        }

            
        }
    }

