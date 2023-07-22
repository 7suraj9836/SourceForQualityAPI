
using Microsoft.AspNetCore.Builder;
using SourceforqualityAPI.Contracts;
using SourceforqualityAPI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SourceforqualityAPI.Interfaces
{
   public interface IAccountService
    {
        Task<LoginResponseModel> AuthenticateUser(LoginModel oLogin);
        Task<string> ForgotPassword(string EmailID);
        Task<string> SetPassword(ResetPasswordModel oLogin);
        Task<string> SaveUser(UserModel oUser);
        UserModel GetUser(int id);
        Task<string> UpdateUser(AccountSettingsDTO user);
        
       
        Task<AccountSettingsDTO> GetUserAccountInfo(int UserID);
        //Task<bool> CheckIfEmailExists(object Email);
    }
}
