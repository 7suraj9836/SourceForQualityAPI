using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SourceforqualityAPI.Model
{
    public class SignUpModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string MobileNo { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        //public Int32 RoleId { get; set; } = 2;

        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public DateTime UpdatedOn { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;

    }

    public class LoginModel
    {
        //[Required(ErrorMessage = "Email Address is required")]
        //[EmailAddress(ErrorMessage = "Email Address is invalid")]
        public string? Email { get; set; }
        public string? Password { get; set; }

        [JsonIgnore]
        public string? DPassword { get; set; }
        public string ?GoogleToken { get; set; }
        public string Id { get;  set; }
        public string Provider { get; set; }

        //public FacebookResponse Response { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        
        public string Logo { get; set; }

        public Int32 RoleId { get; set; }


        //public string ProviderName { get; set; }
        //public string SocialId { get; set; }

    }

    //public class FacebookResponse
    //{
    //    public string FirstName { get; set; }
    //    public string LastName { get; set; }
    //    public string? Email { get; set; }
    //    public string? Name { get; set; }
        
    //}


    public class LoginResponseModel
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public bool IsActive { get; set; }
        public string Email { get; set; }
        public string? MobileNo { get; set; }
        [JsonIgnore]
        public string Password { get; set; }
        public string Logo { get; set; }
        public Int16 RoleId { get; set; } = 2;
        public string RoleName { get; set; }
        public bool IsSubscribed { get; set; }
        public string Message { get; set; }

        public ApiAuthorizedResponseModel Token { get; set; }
    }
   
    public enum SubscriptionType
{
    FREE,
    MONTHLY,
    YEARLY
}

    
     

    public class ApiAuthorizedResponseModel
    {
        public string AccessToken { get; set; }
        public string TokenType { get; set; }
        public long ExpiresIn { get; set; }
    }

    public class UserModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
      
        public string ?MobileNo { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public Int32 RoleId { get; set; } = 2;
        public DateTime CreatedOn { get; set; }=DateTime.Now;
        public DateTime UpdatedOn { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } =true;

        //Added in the latest code
        //public IFormFile? ProfileImage { get; set; }
        public string? EPassword { get; set; }
      //  public int ProfileImageId { get; set; }

      //  public bool IsSubscribed { get; set; }
    }


    public class facebookSignUpModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Logo { get; set; }

        public Int32 RoleId { get; set; }

        public string ProviderName { get; set; }
        public string SocialId { get; set; }

    }


    public class UserUpdateModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MobileNo { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public Int32 RoleId { get; set; } = 2;
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public DateTime UpdatedOn { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = false;
        //Added in the latest code
        public IFormFile? ProfileImage { get; set; }
        public int ProfileImageId { get; set; }
        public bool IsSubscribed { get; set; }
    }

    public class ResetPasswordModel
    {
        public int Id { get; set; }
        public string Password { get; set; }
        public string PasswordResetKey { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "The Email field is required.")]
        [EmailAddress(ErrorMessage = "The Email field is not a valid email address.")]
        public string Email { get; set; }
    }
}
