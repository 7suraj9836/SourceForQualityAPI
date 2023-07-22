using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SourceforqualityAPI.Contracts;
using SourceforqualityAPI.Model;
using System.Threading.Tasks;
using System;
using SourceforqualityAPI.Interfaces;

namespace SourceforqualityAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminConfirmationToSupplierController : ControllerBase
    {

        private IAdminConfirmationToSuppliersServices _adminConfirmation;
        public AdminConfirmationToSupplierController(IAdminConfirmationToSuppliersServices adminConfirmation)
        {
            _adminConfirmation = adminConfirmation;
        }

        


        [HttpPost("AdminConfirmationToSuppliers")]
        public async Task<ResponseModel<string>> AdminConfirmationToSuppliers(AdminConfirmationToSupplier data)
        {
            var res = new ResponseModel<string>();
            try
            {
                res.Data = await _adminConfirmation.AdminConfirmationToSuppliers(data);
                if (res.Data != "Mail has been sent successfully. Please check your email.")
                {
                    res.StatusCode = 400;
                    res.Message = "Failed to send Confirmation link";
                    return res;
                }
                res.StatusCode = 200;
                res.Message = "Mail has been sent successfully. Please check your email. ";
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

    }
}
