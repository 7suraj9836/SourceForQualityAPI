//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Configuration;
//using Microsoft.IdentityModel.Tokens;
//using SourceforqualityAPI.Common;
//using SourceforqualityAPI.Contracts;
//using SourceforqualityAPI.Entity;
//using System;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Text;
//using System.Threading.Tasks;

//namespace SourceforqualityAPI.Controllers
//{
//    [Route("api/v1")]
//    [ApiController]
//    public class LoginController : ControllerBase
//    {
//        private readonly IUserLoginRepo _userService;
//        private readonly IConfiguration _configuration;
//        private readonly IPasswordHasher<UserLogin> _passwordHasher;
//        public LoginController(IUserLoginRepo userService, IConfiguration configuration, IPasswordHasher<UserLogin> passwordHasher )
//        {
//            _userService = userService;
//            _configuration = configuration;
//            _passwordHasher = passwordHasher;
//        }

//        [HttpPost("Login")]
//        //public async Task<IActionResult> Login(UserLogin userLogin)
//        //{
//        //    if (userLogin == null)
//        //        return BadRequest(
//        //            new ResponseGlobal()
//        //            {
//        //                ResponseCode = ((int)System.Net.HttpStatusCode.BadRequest),
//        //                Message = Common.CommonVars.MessageResults.InvalidLogin.ToString()
//        //            });
//        //    var getUser = await _userService.UserLogin(userLogin.Email);
//        //    if (getUser == null)
//        //        return BadRequest(
//        //            new ResponseGlobal()
//        //            {
//        //                ResponseCode = ((int)System.Net.HttpStatusCode.BadRequest),
//        //                Message = Common.CommonVars.MessageResults.InvalidLogin.ToString()
//        //            });
//        //    var result = _passwordHasher.VerifyHashedPassword(getUser, getUser.Password, userLogin.Password);
//        //    if (result != PasswordVerificationResult.Success)
//        //    {
//        //        //return BadRequest("Invalid email or password.");
//        //        return BadRequest(
//        //           new ResponseGlobal()
//        //           {
//        //               ResponseCode = ((int)System.Net.HttpStatusCode.BadRequest),
//        //               Message = Common.CommonVars.MessageResults.InvalidLogin.ToString()
//        //           });
//        //    }
//        //    UserProfile user = new UserProfile();
//        //    user.Email = getUser.Email;
//        //    user.FullName = getUser.FullName;
//        //    user.RoleId = getUser.RoleId;
//        //    user.Id = getUser.Id;
            


//        //    string token = GenerateToken(getUser.Email);
//        //    return Ok(
//        //        new ResponseGlobal()
//        //        {
//        //            ResponseCode = ((int)System.Net.HttpStatusCode.OK),
//        //            Message = Common.CommonVars.MessageResults.SuccessGet.ToString(),
//        //            Data = token
//        //        });
//        //}


//        //public async Task<LoginResponseModel> Login(LoginModel oLogin)
//        //{
//        //    var oResponseModel = new LoginResponseModel();
//        //    try
//        //    {
//        //        using (System.Data.IDbConnection con = new SqlConnection(Global.ConnectionString))
//        //        {
//        //            if (con.State == System.Data.ConnectionState.Closed)
//        //            {
//        //                con.Open();
//        //            }

//        //            bool userExists = con.ExecuteScalar<bool>($"select count(1) from Tbl_Master_User where EmailId=@EmailID and convert(varchar(50), DECRYPTBYPASSPHRASE('8', [Password]))=@Password", oLogin);

//        //            if (userExists)
//        //            {
//        //                var userStatus = con.QuerySingle<bool>($"select IsActive from Tbl_Master_User where EmailID=@EmailID and convert(varchar(50), DECRYPTBYPASSPHRASE('8', [Password]))=@Password", oLogin);

//        //                if (!userStatus)
//        //                {
//        //                    oResponseModel.Message = "User account is Inactive. Please contact to Admin Support !";
//        //                }

//        //                oResponseModel = con.QuerySingle<LoginResponseModel>($"select UserID, UserName from Tbl_Master_User where EMailId = @EmailID", oLogin);

//        //                var userRole = "Admin";// await con.QuerySingleAsync<string>($"select b.LevelName from Tbl_Master_User a left join Tbl_Master_UserLevel b on a.UserLevelID = b.LevelID where a.EmailID=@EmailID", oLogin);

//        //                var userID = await con.QuerySingleAsync<int>($"select UserId from Tbl_Master_User where EmailId = @EmailID", oLogin);

//        //                oResponseModel.Token = CreateJwtToken(userRole, userID);
//        //            }
//        //            else
//        //            {
//        //                oResponseModel.Message = "EmailID / Password is incorrect.";
//        //            }
//        //            con.Close();
//        //        }
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        oResponseModel.Message = ex.Message;
//        //    }

//        //    return oResponseModel;
//        //}

//        [HttpPost("SignUp")]
//        public async Task<IActionResult> SignUp(UserProfile user)
//        {
//            var existingUser = await _userService.GetByEmail(user.Email);

//            if (existingUser != null)
//            {
//                return BadRequest("A user with this email already exists.");
//            }
//            var userId = await _userService.UserSignUp(user);

//            return Ok(userId);
//        }
//        private string GenerateToken(string email)
//        {
//            string secretKey = _configuration.GetSection("AppSettings:JWT:Key").Value;
//            string issuer = _configuration.GetSection("AppSettings:JWT:Issuer").Value;
//            string audience = _configuration.GetSection("AppSettings:JWT:Audience").Value;
//            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
//            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

//            var claims = new[]
//            {
//                new Claim(ClaimTypes.Email, email)
//            };

//            var token = new JwtSecurityToken(
//            issuer: issuer,
//            audience: audience,
//            claims: claims,
//            expires: DateTime.UtcNow.AddDays(7),
//            signingCredentials: credentials);

//            var encodedToken = new JwtSecurityTokenHandler().WriteToken(token);
//            return encodedToken;
//        }

//        [HttpPost("ForgetPassword")]
//        public async Task<IActionResult> ForgetPassword(string email)
//        {
//            var existingUser = await _userService.GetByEmail(email);

//            if (existingUser == null)
//            {
//                return BadRequest("A user with this email is not exists.");
//            }
//            _userService.ForgetPasswordByEmail(email);

//            return Ok("Reset password link send successfully.");
//        }

//        [HttpPost("ResetPassword")]
//        public async Task<IActionResult> ResetPassword(UserLogin user)
//        {
//            var existingUser = await _userService.GetByEmail(user.Email);

//            if (existingUser == null)
//            {
//                return BadRequest("A user with this email is not exists.");
//            }

//            string token = "1cac943c-efb5-422c-b63b-f1704bbefad1";// HttpContext.Request.QueryString.Value;
//            _userService.ResetPasswordByEmail(user, token);

//            return Ok("Password reset successfully.");
//        }
//    }
//}
