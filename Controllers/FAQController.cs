using iTextSharp.text;
using Microsoft.AspNetCore.Mvc;
using SourceforqualityAPI.Interfaces;
using SourceforqualityAPI.Model;
using SourceforqualityAPI.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SourceforqualityAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FAQController : Controller
    {
        private readonly IFAQManagementServices _FaqManagementServices;

        public FAQController(IFAQManagementServices FaqManagementServices)
        {
            _FaqManagementServices = FaqManagementServices;
        }




         [HttpGet("GetFAQ")]
         public async Task<ResponseModel<List<FAQManagement>>> GetFAQ(int pageNumber, int pageSize)
        {
            var res = new ResponseModel<List<FAQManagement>>();
            try
            {
                res.Data = _FaqManagementServices.GetFAQ(pageNumber, pageSize);
                res.StatusCode = 200;
                res.Message = "FAQ Data Fetched Successfully";
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


       
        [HttpPost("SaveFAQ")]

        public async Task<ResponseModel<string>> SaveFAQ(FAQManagement faq)
        {
            var res = new ResponseModel<string>();
            try
            {
                res.Data =await _FaqManagementServices.SaveFAQ(faq);
                res.StatusCode = 200;
                res.Message = "Data Saved Successfully";

            }
            catch (Exception ex)
            {
                res.Data = null;
                res.StatusCode = 400;
                res.Message = ex.Message;
            }
            return res;
        }

        [HttpGet("GetFAQById")]
        public async Task<ResponseModel<FAQManagement>> GetFAQById(int Id)
        {
            var res = new ResponseModel<FAQManagement>();
            try
            {
                res.Data = _FaqManagementServices.GetFAQById(Id);
                res.StatusCode = 200;
                res.Message = "FAQ Data Fetched Successfully";
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




        [HttpPost("UpdateFAQ")]

        public async Task<ResponseModel<string>> UpdateFAQ(FAQManagement faq)
        {
            var res = new ResponseModel<string>();
            try
            {
                res.Data = await _FaqManagementServices.UpdateFAQ(faq);
                res.StatusCode = 200;
                res.Message = "Data Updated Successfully";

            }
            catch (Exception ex)
            {
                res.Data = null;
                res.StatusCode = 400;
                res.Message = ex.Message;
            }
            return res;
        }

        [HttpPost("DeleteFAQ/{id}")]
        public async Task<ResponseModel<string>> DeleteFAQ(int id)
        {
            var res = new ResponseModel<string>();
            try
            {
                res.Data = _FaqManagementServices.DeleteFAQ(id);
                res.StatusCode = 200;
                if (res.Data == "FAQ does not exist")
                {
                    res.Message = "FAQ does not exist";
                }
                else
                {
                    res.Message = "FAQ Deleted Successfully";
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

    }
}
