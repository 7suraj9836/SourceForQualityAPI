using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SourceforqualityAPI.Model;
using System.Threading.Tasks;
using System;
using SourceforqualityAPI.Interfaces;
using SourceforqualityAPI.Services;
using System.Collections.Generic;
using SourceforqualityAPI.Contracts;

namespace SourceforqualityAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionManagementController : ControllerBase
    {

        private readonly ISubscriptionManagementServices _SubscriptionManagementServices;

        public SubscriptionManagementController(ISubscriptionManagementServices SubscriptionManagementServices)
        {
            _SubscriptionManagementServices = SubscriptionManagementServices;
        }



        [HttpGet("GetSupplierSubscriptionInfo")]
        public async Task<ResponseModel<List<SubscriptionManagement>>> GetSupplierSubscriptionInfo(int pageNumber, int pageSize)
        {
            var res = new ResponseModel<List<SubscriptionManagement>>();
            try
            {
                res.Data = _SubscriptionManagementServices.GetSupplierSubscriptionInfo(pageNumber, pageSize);
                res.StatusCode = 200;
                res.Message = "Subscription Data Fetched Successfully";
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




        [HttpPost("SaveSubscriptionInfo")]

        public async Task<ResponseModel<string>> SaveSubscriptionInfo(SubcriptionManagementSaveDTO faq)
        {
            var res = new ResponseModel<string>();
            try
            {
                res.Data = await _SubscriptionManagementServices.SaveSubscriptionInfo(faq);
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

        [HttpGet("GetSubscriptionInfoById")]
        public async Task<ResponseModel<SubscriptionManagement>> GetSubscriptionInfoById(int Id)
        {
            var res = new ResponseModel<SubscriptionManagement>();
            try
            {
                res.Data = _SubscriptionManagementServices.GetSubscriptionInfoById(Id);
                res.StatusCode = 200;
                res.Message = "Data Fetched Successfully";
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


        [HttpPost("UpdateSubscriptionInfo")]

        public async Task<ResponseModel<string>> UpdateSubscriptionInfo(SubcriptionManagementUpdateDTO faq)
        {
            var res = new ResponseModel<string>();
            try
            {
                res.Data = await _SubscriptionManagementServices.UpdateSubscriptionInfo(faq);
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

       
        [HttpPost("DeleteSubscriptionInfo/{id}")]
        public async Task<ResponseModel<string>> DeleteSubscriptionInfo(int id)
        {
            var res = new ResponseModel<string>();
            try
            {
                res.Data = _SubscriptionManagementServices.DeleteSubscriptionInfo(id);
                res.StatusCode = 200;
                if (res.Data == "Subscription does not exist")
                {
                    res.Message = "Subscription does not exist";
                }
                else
                {
                    res.Message = "Subscription Deleted Successfully";
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
