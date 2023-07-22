using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SourceforqualityAPI.Interfaces;
using SourceforqualityAPI.Model;
using System.Threading.Tasks;
using System;
using SourceforqualityAPI.Contracts;
using Microsoft.AspNetCore.Authorization;
using SourceforqualityAPI.Common;
using System.Data.SqlClient;
using Dapper;
using BCryptNet = BCrypt.Net.BCrypt;

namespace SourceforqualityAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChangePasswordController : ControllerBase
    {
        private readonly IChangePasswordServices _changePasswordServices;

        public ChangePasswordController(IChangePasswordServices changePasswordServices)
        {
            _changePasswordServices = changePasswordServices;
        }

        //[Authorize(policy:"RequireAnyRole")]
        [HttpPost("ChangePassword")]
        public async Task<ResponseModel<string>> ChangePassword(ChangePasswordDTO changePassword)
        {
            var res = new ResponseModel<string>();
            using (System.Data.IDbConnection con = new SqlConnection(Global.ConnectionString))
            {
                if (con.State == System.Data.ConnectionState.Closed)
                {
                    con.Open();
                }

                try
                {
                    res.Data = _changePasswordServices.ChangePassword(changePassword);
                    if (res.Data == "Failed")
                    {
                        var ExistingPassword = con.QuerySingle<string>(@"select [Password] from Users where Email=@Email", changePassword);
                        res.StatusCode = 400;
                        if (!(BCryptNet.Verify(changePassword.OldPassword, ExistingPassword)))
                        {
                            res.Message = "Old Password is incorrect";
                        }
                        else { res.Message = "Password Not Changed"; }
                            
                        return res;
                    }
                    res.StatusCode = 200;
                    res.Message = "Password Changed Successfully";
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

            //return Ok(_contactServices.SaveContact(contact));
        }
    }
}
