using Microsoft.AspNetCore.Authorization;
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
    public class UpdateRoleOnSubscriptionController : ControllerBase
    {
        private IUpdateRoleOnSubscriptionServices _oUser;
        public UpdateRoleOnSubscriptionController(IUpdateRoleOnSubscriptionServices oUser)
        {
            _oUser = oUser;
          
        }



        [Authorize(policy: "RequireAnyRole")]
        [HttpPost("UpdateRoleOnSubscription")]
        public async Task<ResponseModel<AccountSettingsDTO>> UpdateRoleOnSubscription(UpdateRoleOnSubscriptionDTO user)
        {
            var res = new ResponseModel<AccountSettingsDTO>();
            try
            {
                res.Data = await _oUser.UpdateRoleOnSubscription(user);
                if (res.Data == null)
                {
                    res.StatusCode = 400;
                    res.Message = "Update Unsuccessful";
                    return res;
                }
                res.StatusCode = 200;
                res.Message = "Role Updated Successfully";
            }
            catch (Exception ex)
            {
                res.Data = null;
                res.Message = ex.Message;
                res.StatusCode = 400;
            }
            return res;
        }
    }
}
