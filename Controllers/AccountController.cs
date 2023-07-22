using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SourceforqualityAPI.Common;
using SourceforqualityAPI.Contracts;
//using SourceforqualityAPI.Entity;
using SourceforqualityAPI.Interfaces;
using SourceforqualityAPI.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace SourceforqualityAPI.Controllers
{
    
        [Route("api/[controller]")]
        [ApiController]
        public class AccountController : Controller
    {
            private IAccountService _oLogin;
        private readonly IFileUpload uploadService;

        public AccountController(IAccountService oLogin,IFileUpload uploadService)
            {
                _oLogin = oLogin;
            this.uploadService = uploadService;
        }
          
          [HttpPost("SignIn")]
            public async Task<ResponseModel<LoginResponseModel>> SingIn([FromBody] LoginModel oLogin)
            {
            var res = new ResponseModel<LoginResponseModel>();
            using (System.Data.IDbConnection con = new SqlConnection(Global.ConnectionString))
            {
                if (con.State == System.Data.ConnectionState.Closed)
                {
                    con.Open();
                }
                try
                {
                    res.Data = await _oLogin.AuthenticateUser(oLogin);
                    if (res.Data.Token == null)
                    {
                        res.Data = null;
                        res.StatusCode = 400;
                         var checkAccountStatus= con.QuerySingle<bool>("SELECT IsActive FROM Users WHERE Email = @Email", oLogin);
                        if(checkAccountStatus != true) {
                            res.Message = "Your Account has been suspended. Please contact the Admin for further details";
                        }
                        else
                        {
                            res.Message = "EmailId/Password  is not invalid";
                        }
                        
                        return res;
                    }
                    res.StatusCode = 200;
                    res.Message = "Login Successful";

                }
                catch (Exception ex)
                {
                    res.Data = null;
                    res.Message = ex.Message;
                    res.StatusCode = 400;
                }

                if (con.State == System.Data.ConnectionState.Open)
                {
                    con.Close();
                }
            }

           
            return res;



        }
        

        //}


        [HttpPost("SignUp")]
            public async Task<ResponseModel<string>> SignUp([FromBody] UserModel oUser)
            {
                var res = new ResponseModel<string>();
                try
                {
                    res.Data = await _oLogin.SaveUser(oUser);
                    res.StatusCode = 200;
                    res.Message = "Signup Successful";
                }
                catch (Exception ex)
                {
                    res.Data =null;
                    res.Message = ex.Message;
                    res.StatusCode = 400;
                }
                return res;
            }
        
        
        [HttpPost("ForgetPassword")]
        public async Task<ResponseModel<string>> ForgetPassword(ForgetpasswordInput data)
        {
            var res = new ResponseModel<string>();
            try
            {
                res.Data = await _oLogin.ForgotPassword(data.Email);
                if (res.Data != "Password reset link has been sent successfully. Please check your email.")
                {
                    res.StatusCode = 400;
                    res.Message = "Failed to send reset link";
                    return res;
                }
                res.StatusCode = 200;
                res.Message = "Reset Link Successfully send to your registered Email ";
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

    
        [HttpPost("SetPassword")]
        public async Task<ResponseModel<string>> SetPassword(ResetPasswordModel oLogin)
        {
            var res = new ResponseModel<string>();
            try
            {
                res.Data = await _oLogin.SetPassword(oLogin);
                if (res.Data != "Password updated successfully")
                {
                    res.StatusCode = 400;
                    res.Message = res.Data;
                    return res;
                }
                res.StatusCode = 200;
                res.Message = "Password Successfully Changed";

            }
            catch (Exception ex)
            {
                res.Data = null;
                res.Message = ex.Message;
                res.StatusCode = 400;
            }

            return res;

            //return await _oLogin.SetPassword(oLogin);
        }

        [Authorize(policy: "RequireAnyRole")]
        [HttpGet("GetUser/{id}")]
        public async Task<ResponseModel<UserModel>> GetUser(int id)
        {
            var res = new ResponseModel<UserModel>();
            try
            {
                res.Data =  _oLogin.GetUser(id);
                res.StatusCode = 200;
                res.Message = "User Details Fetched Successfully";
            }
            catch (Exception ex)
            {
                res.Data = null;
                res.Message = ex.Message;
                res.StatusCode = 400;
            }
            return res;


            //return Ok(_oLogin.GetUser(id));
        }

        [Authorize(policy: "RequireAnyRole")]
        [HttpPost("UpdateUser")]
        public async Task<ResponseModel<string>> UpdateUser(AccountSettingsDTO user)
        {
            var res = new ResponseModel<string>();
            try
            {
                res.Data = await _oLogin.UpdateUser(user);
                if (res.Data != "Success")
                {
                    res.StatusCode = 400;
                    res.Message = "Update Unsuccessful";
                    return res;
                }
                res.StatusCode = 200;
                res.Message = "User Updated Successfully";
            }
            catch (Exception ex)
            {
                res.Data = null;
                res.Message = ex.Message;
                res.StatusCode = 400;
            }
            return res;
        }
        [Authorize(policy: "RequireAnyRole")]
        [HttpGet("GetUserAccountInfo")]
        public async Task<ResponseModel<AccountSettingsDTO>> GetUserAccountInfo(int UserId)
        {
            var res = new ResponseModel<AccountSettingsDTO>();
            try
            {
                res.Data = await _oLogin.GetUserAccountInfo(UserId);
                res.StatusCode = 200;
                res.Message = "Account Info Retrieved Successfully";
            }
            catch (Exception ex)
            {
                res.Data = null;
                res.Message = ex.Message;
                res.StatusCode = 400;
            }
            return res;
        }


        [Authorize(policy: "RequireEndUserFoodSupplier")]
        [HttpPost]
        public async Task<IActionResult> UploadFileTest(IFormFile file)
        {
            var res = await uploadService.UploadFile(file);
            return Ok(res);
        }

        //public AccountController(UserManager<ApplicationUser> userManager)
        //{
        //    _userManager = userManager;
        //}

        //[HttpPost]
        //public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        Find the user by email
        //       var user = await _userManager.FindByEmailAsync(model.Email);
        //        if (user == null)
        //        {
        //            Don't reveal that the user does not exist
        //            return Ok();
        //        }

        //        Generate password reset token
        //        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        //        TODO: Send the password reset link to the user's email
        //         You can use libraries like SendGrid, MailKit, or System.Net.Mail to send emails
        //         For the purpose of this example, we'll just return the reset token
        //        return Ok(token);
        //    }

        //    return BadRequest(ModelState);
        //}




    }
}

