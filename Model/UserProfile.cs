//using System;
//using System.ComponentModel.DataAnnotations;

//namespace SourceforqualityAPI.Model
//{


//    public class LoginModel
//    {
//        [Required(ErrorMessage = "Email Address is required")]
//        [EmailAddress(ErrorMessage = "Email Address is invalid")]
//        public string EmailId { get; set; }
//        public string Password { get; set; }

//    }



//    public class LoginResponseModel
//    {
//        public int Id { get; set; }
//        public string FullName { get; set; }
//        public string Email { get; set; }
//        public ApiAuthorizedResponseModel Token { get; set; }
//        public string ProfileImage { get; set; }
//        public Int16 RoleId { get; set; }
//        public string Password { get; set; }
//    }

//    public class ApiAuthorizedResponseModel
//    {
//        public string AccessToken { get; set; }
//        public string TokenType { get; set; }
//        public long ExpiresIn { get; set; }
//    }

//    public class UserProfile
//    {
//        public int Id { get; set; }
//        public string FirstName { get; set; }
//        public string LastName { get; set; }
//        public string FullName
//        {
//            get
//            {
//                return string.Format("{0} {1} {2}", FirstName, LastName);
//            }
//        }//FirstName + " " + LastName
//        public Int64 MobileNo { get; set; }
//        public string Email { get; set; }
//        public string Password { get; set; }
//        public Int16 RoleId { get; set; }
//    }



//}
