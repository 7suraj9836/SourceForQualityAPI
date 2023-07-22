using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SourceforqualityAPI.Common;
using SourceforqualityAPI.Interfaces;
using SourceforqualityAPI.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt; 
using System.Net.Mail;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using SourceforqualityAPI.Contracts; 
using Google.Apis.Auth;
using BCryptNet = BCrypt.Net.BCrypt;

//,IFileUpload fileUploadService

namespace SourceforqualityAPI.Services
{
    public class AccountService : IAccountService
    {
        LoginModel _oLogin = new LoginModel();
        private IConfiguration _configuration;
        //private readonly IFileUpload _fileUploadService;
        UserModel _oUser = new UserModel();
       // var _lUsers = new List<UserModel>();

        public AccountService(IConfiguration configuration)
        {
            _configuration = configuration;
            //_fileUploadService = fileUploadService;
        }
        public ApiAuthorizedResponseModel CreateJwtToken(string userRole, int userID)
        {
            ApiAuthorizedResponseModel resp = new ApiAuthorizedResponseModel();

            var TokenHandler = new JwtSecurityTokenHandler();
            var TokenKey = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SecretKey"]);
            var TokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                        new Claim("UserID", userID.ToString()),
                        new Claim(ClaimTypes.Role, userRole),
                           new Claim("aud", _configuration["JwtSettings:ValidAudience"]),
                           new Claim("iss", _configuration["JwtSettings:ValidIssuer"])
                }),
                Expires = DateTime.UtcNow.AddHours(10),
                SigningCredentials =
                    new SigningCredentials(
                        new SymmetricSecurityKey(TokenKey),
                        SecurityAlgorithms.HmacSha256Signature
                        )
            };

            var Token = TokenHandler.CreateToken(TokenDescriptor);
            var jwtToken = TokenHandler.WriteToken(Token);

            resp.ExpiresIn = DateTime.UtcNow.AddHours(10).ToFileTimeUtc();
            resp.AccessToken = jwtToken;
            resp.TokenType = "bearer";
            return resp;
        }

        public async Task<string> ForgotPassword(string Email)
        {
            string result = "";
            try
            {
                using (System.Data.IDbConnection con = new SqlConnection(Global.ConnectionString))
                {
                    if (con.State == System.Data.ConnectionState.Closed)
                    {
                        con.Open();
                    }

                    bool userExists = await con.ExecuteScalarAsync<bool>($"select count(1) from Users where Email = @value", new { value = Email });

                    if (userExists)
                    {
                        /* var userLevel = con.QuerySingle<int>($"select UserLevelID from Tbl_Master_User where EmailId = @value", new { value = EmailID });*/
                        var userData = con.QuerySingle($"select Id,FirstName from Users where Email = @value", new { value = Email });
                     //   var FirstName = con.QuerySingle<int>($"select FirstName from Users where Email = @value", new { value = Email });
                        

                        //Generate Password Reset Key
                        var PasswordResetKey = Guid.NewGuid().ToString();
                        con.Execute($"update Users set PasswordResetKey=@PasswordResetKey, PasswordResetKeyCreatedOn = GETUTCDATE() where Id=@Id", new { Id = userData.Id, PasswordResetKey = PasswordResetKey });

                        if (userData.Id > 0)
                        {
                            /*string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Common/ReadUrl.txt");
                            string text = File.ReadAllText(path);*/
                            string text = "http://sourceforqualitydev.iworklab.com/";
                            MailMessage message = new MailMessage();
                            SmtpClient smtp = new SmtpClient();
                            message.From = new MailAddress(Global.SmtpFromEmail);
                            message.To.Add(Email);
                            message.Subject = "SourceForQuality - Reset Password";
                            message.IsBodyHtml = true;
                            message.Body = "Hi" + userData.FirstName + ", Please click on the following link to reset your password for " + Email + ": <a href='" + text + "/reset-password/" + userData.Id + $"/{PasswordResetKey}' style='color:blue'>Link to reset password</a>"
                                            + @"<br/><br/><p style='color:red'> This link will expire in 24 hours. </p>";
                            smtp.Port = Global.SmtpPort;
                            smtp.Host = Global.SmtpHost;
                            smtp.EnableSsl = true;
                            //smtp.UseDefaultCredentials = true;
                            smtp.Credentials = new NetworkCredential(Global.SmtpCredId, Global.SmtpCredPassword);
                            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                            smtp.Send(message);
                        }

                        result = "Password reset link has been sent successfully. Please check your email.";
                    }
                    else
                    {
                        result = "User does not exist";
                    }

                    if (con != null && con.State == System.Data.ConnectionState.Open)
                    {
                        con.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }

            return result;
        }

        //public async Task<string> SetPassword(ResetPasswordModel oLogin)
        //{
        //    string result = "";
        //    try
        //    {
        //        using (System.Data.IDbConnection con = new SqlConnection(Global.ConnectionString))
        //        {
        //            if (con.State == System.Data.ConnectionState.Closed)
        //            {
        //                con.Open();
        //            }

        //            var passwordResetKey = await con.QuerySingleAsync<string>("select ISNULL(PasswordResetKey,'') from Users where Id = @Id", new { Id = oLogin.Id });

        //            var passwordResetKeyCreatedOn = con.QuerySingle<string>("select ISNULL(PasswordResetKeyCreatedOn,'') from Users where Id = @Id", new { Id = oLogin.Id });
        //            // check if this date is too old

        //            var oldPassword = con.QuerySingle<string>("select convert(varchar(max),DECRYPTBYPASSPHRASE('8',Password)) from Users where Id = @Id", new { Id = oLogin.Id });

        //            var hashedPasswordFromDB = con.QuerySingle<string>("SELECT Password FROM Users WHERE Id = @Id", new { Id = oLogin.Id });

        //            if (BCrypt.Net.BCrypt.Verify(oLogin.Password, hashedPasswordFromDB))
        //            {
        //                // Password is correct, proceed with the password reset
        //                // ...
        //            }


        //            if ((oLogin.PasswordResetKey == passwordResetKey || oLogin.PasswordResetKey == oldPassword) && oLogin.PasswordResetKey != "" && oLogin.PasswordResetKey != null)
        //            {
        //                var oPassword = con.Execute("update Users set Password=EncryptByPassPhrase('8',cast(@Password as varchar)),PasswordResetKey=null where Id=@Id", new { Password = oLogin.Password, Id = oLogin.Id });

        //                if (oPassword <= 0)
        //                {
        //                    result = "Something went Wrong";
        //                }
        //                else
        //                {
        //                    result = "Password updated successfully";
        //                }
        //            }
        //            else
        //            {
        //                result = "Link is expired now, please request again";
        //            }

        //            if (con != null && con.State == System.Data.ConnectionState.Open)
        //            {
        //                con.Close();
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        result = ex.Message;
        //    }
        //    return result;
        //}

        public async Task<string> SetPassword(ResetPasswordModel oLogin)
        {
            string result = "";
            try
            {
                using (System.Data.IDbConnection con = new SqlConnection(Global.ConnectionString))
                {
                    if (con.State == System.Data.ConnectionState.Closed)
                    {
                        con.Open();
                    }

                    var passwordResetKey = await con.QuerySingleAsync<string>("select ISNULL(PasswordResetKey,'') from Users where Id = @Id", new { Id = oLogin.Id });

                    var passwordResetKeyCreatedOn = con.QuerySingle<string>("select ISNULL(PasswordResetKeyCreatedOn,'') from Users where Id = @Id", new { Id = oLogin.Id });


                    var hashedPasswordFromDB = con.QuerySingle<string>("SELECT Password FROM Users WHERE Id = @Id", new { Id = oLogin.Id });

                    if ((BCryptNet.Verify(oLogin.PasswordResetKey, hashedPasswordFromDB)|| oLogin.PasswordResetKey == passwordResetKey) &&  oLogin.PasswordResetKey != "" && oLogin.PasswordResetKey != null)
                    {
                        var hashedNewPassword = BCryptNet.HashPassword(oLogin.Password);

                        var oPassword = con.Execute("UPDATE Users SET Password=@Password, PasswordResetKey=NULL WHERE Id=@Id", new { Password = hashedNewPassword, Id = oLogin.Id });

                        if (oPassword <= 0)

                    {
                            result = "Something went wrong";
                        }
                        else
                        {
                            result = "Password updated successfully";
                        }
                    }
                    else
                    {
                        result = "Link is expired now, please request again";
                    }

                    if (con != null && con.State == System.Data.ConnectionState.Open)
                    {
                        con.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }


        public async Task<string> SaveUser(UserModel oUser)
        {
            string checkMessage = "";
            int operationType = Convert.ToInt32(oUser.Id == 0 ? 1 : 2);
            checkMessage = await InsertUser(oUser, operationType);
            return checkMessage;
        }

        private async Task<string> InsertUser(UserModel oUser, int operationType)
        {
            string sqlResult = "";

            try
            {
                using (System.Data.IDbConnection con = new SqlConnection(Global.ConnectionString))
                {
                    if (con.State == System.Data.ConnectionState.Closed)
                    {
                        con.Open();
                    }

                    if (operationType == 1) // New User Creation
                    {
                        bool emailIdExists = await con.ExecuteScalarAsync<bool>($"select count(1) from Users where Email = @value", new { value = oUser.Email });

                        if (emailIdExists == false)
                        {
                            oUser.EPassword =  BCryptNet.HashPassword(oUser.Password);
                            var query = @"insert into Users(FirstName, LastName, MobileNo, Email, Password, RoleId,IsActive,CreatedOn,UpdatedOn) values (@FirstName, @LastName, @MobileNo, @Email,@EPassword,@RoleId,@IsActive,@CreatedOn,@UpdatedOn)";

                            if (oUser.RoleId == 0)
                            {
                                oUser.RoleId = 2;
                            }
                            var oUsers = con.Execute(query, oUser);

                            if (oUsers <= 0)
                            {
                                sqlResult = "Something went wrong";

                            }
                            else
                            {
                                sqlResult = "User created successfully";
                                //sqlResult = oUser.RoleId.ToString();
                            }
                        }
                        else
                        {
                            sqlResult = "EmailID already exists";
                            //return  BadRequest("Incorrect Email or Password");
                        }
                    }
                    else
                    {
                        // Update Existing User

                        bool userExists = await con.ExecuteScalarAsync<bool>($"select count(1) from Users where Id = @value", new { value = oUser.Id });

                        if (userExists)
                        {
                            var query = "";

                            var oUsers = con.Execute(query, oUser);

                            if (oUsers <= 0)
                            {
                                sqlResult = "Something went wrong";
                            }
                            else
                            {
                                sqlResult = "User updated successfully";
                                sqlResult = oUser.RoleId.ToString();
                            }

                        }
                        else
                        {
                            sqlResult = "User does not exist";
                        }
                    }

                    if (con != null && con.State == System.Data.ConnectionState.Open)
                    {
                        con.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                sqlResult = ex.Message;
            }
            return sqlResult;
        }


        public async Task<LoginResponseModel> AuthenticateUser(LoginModel oLogin)
        {
            var oResponseModel = new LoginResponseModel();
            try
            {
                using (System.Data.IDbConnection con = new SqlConnection(Global.ConnectionString))
                {
                    if (con.State == System.Data.ConnectionState.Closed)
                    {
                        con.Open();
                    }

                    bool userExists = false;
                    string userRole = "";
                    int userID = 0;

               


                    if (!string.IsNullOrEmpty(oLogin.Email) && !string.IsNullOrEmpty(oLogin.Password))
                    {
                        // Authenticate using email/password
                        oResponseModel = con.QuerySingle<LoginResponseModel>("SELECT Id, FirstName + ' ' + LastName AS FullName, RoleId, Email, Password, IsSubscribed, MobileNo,IsActive, Logo FROM Users WHERE Email = @Email", oLogin);
                        bool isPasswordMatched = BCryptNet.Verify(oLogin.Password, oResponseModel.Password);

                        userExists = con.ExecuteScalar<bool>("SELECT COUNT(1) FROM Users WHERE Email = @Email", oLogin);
                        // userExists = con.ExecuteScalar<bool>("SELECT COUNT(1) FROM Users WHERE Email = @Email AND CONVERT(varchar(50), DECRYPTBYPASSPHRASE('8', [Password])) = @Password", oLogin);

                        if (userExists==true && isPasswordMatched==true && oResponseModel.IsActive==true)
                        {
                         


                            if (isPasswordMatched)
                            {
                            var roleId = 2;
                            if (oResponseModel.RoleId == 0)
                            {
                                oResponseModel.RoleId = (short)roleId;
                            }
                            if (oResponseModel.RoleId == 1)
                            {
                                userRole = "Admin";
                            }
                            else if (oResponseModel.RoleId == 2)
                            {
                                userRole = "EndUser";
                            }
                            else if (oResponseModel.RoleId == 3)
                            {
                                userRole = "FoodSupplier";
                            }

                            userID = await con.QuerySingleAsync<int>("SELECT Id FROM Users WHERE Email = @Email", oResponseModel = con.QuerySingle<LoginResponseModel>("SELECT Id, FirstName + ' ' + LastName AS FullName,MobileNo, RoleId, Email, Password, IsSubscribed, Logo FROM Users WHERE Email = @Email", oLogin));

                            oResponseModel.Token = CreateJwtToken(userRole, userID);
                        }
                            else
                            {
                              
                                   
                                
                                 oResponseModel.Message = "Email ID/Password combination not valid"; 
                                
                            }
                        }
                        else
                        {
                            if(oResponseModel.IsActive== false)
                            {
                                oResponseModel.Message = "Your Account is suspended. Please contact Admin for more details";
                            }
                            else
                            {
                                oResponseModel.Message = "Email ID/Password combination not valid";
                            }
                            
                        }
                    }

                    else if (!string.IsNullOrEmpty(oLogin.GoogleToken))
                    {
                        // Authenticate using Google
                        GoogleJsonWebSignature.Payload payload;
                        try
                        {
                            // Validate and decode the Google token
                            payload = await GoogleJsonWebSignature.ValidateAsync(oLogin.GoogleToken);
                        }
                        catch (Exception ex)
                        {
                            oResponseModel.Message = "Google authentication failed";
                            // Handle the exception or log the error
                            return oResponseModel;
                        }

                        // Check if the email from the Google token exists in the Users table
                        userExists = con.ExecuteScalar<bool>("SELECT COUNT(1) FROM Users WHERE Email = @Email", new { Email = payload.Email });
                        if (userExists)
                        {
                            // Retrieve the user's information from the Users table
                            oResponseModel = con.QuerySingle<LoginResponseModel>("SELECT Id, FirstName + ' ' + LastName AS FullName, RoleId, Email, Password, IsSubscribed, Logo FROM Users WHERE Email = @Email", new { Email = payload.Email });

                            var roleId = 2;
                            if (oResponseModel.RoleId == 0)
                            {
                                oResponseModel.RoleId = (short)roleId;
                            }

                            if (oResponseModel.RoleId == 1)
                            {
                                userRole = "Admin";
                            }
                            else if (oResponseModel.RoleId == 2)
                            {
                                userRole = "EndUser";
                            }
                            else if (oResponseModel.RoleId == 3)
                            {
                                userRole = "FoodSupplier";
                            }

                            userID = oResponseModel.Id;

                            oResponseModel.Token = CreateJwtToken(userRole, userID);
                        }
                        else
                        {
                            oResponseModel.Message = "Google authentication failed: Email not found";
                        }
                    }
                    // ...

                    else if (!string.IsNullOrEmpty(oLogin.Id))
                    {

                        bool userExist = false;
                        string messages = "";
                        string usersRole = "";
                        int usersID = 0;

                        userExist = con.ExecuteScalar<bool>("SELECT COUNT(1) FROM Users WHERE SocialId = @SocialId and ProviderName=@ProviderName", new { SocialId = oLogin.Id, ProviderName = oLogin.Provider });
                        if (userExist)
                        {
                            oResponseModel = con.QuerySingle<LoginResponseModel>("SELECT Id, FirstName + ' ' + LastName AS FullName, RoleId, Email, Password, IsSubscribed, Logo FROM Users WHERE Email = @Email", oLogin);
                            var roleId = 2;
                            if (oResponseModel.RoleId == 0)
                            {
                                oResponseModel.RoleId = (short)roleId;
                            }
                            if (oResponseModel.RoleId == 1)
                            {
                                usersRole = "Admin";
                            }
                            else if (oResponseModel.RoleId == 2)
                            {
                                usersRole = "EndUser";
                            }
                            else if (oResponseModel.RoleId == 3)
                            {
                                usersRole = "FoodSupplier";
                            }
                            oResponseModel.Token = CreateJwtToken(usersRole, usersID);
                        }
                        else if (!userExist)
                        {
                            string message = "";
                            string userRoles = "";

                            //bool emailIdExists = await con.ExecuteScalarAsync<bool>($"select count(1) from Users where Email = @value", new { value = oLogin.Email });

                            //if (!emailIdExists)
                            //{
                                var query = @"insert into Users(FirstName, LastName, SocialId, Email, ProviderName,RoleId ) values (@FirstName, @LastName,@Id, @Email,@Provider,@RoleId)";

                                if (oLogin.RoleId == 0)
                                {
                                    oLogin.RoleId = 2;
                                }


                                var oUsers = con.Execute(query, oLogin);


                                userRoles = "EndUser";



                                if (oUsers <= 0)
                                {
                                    message = "Something went wrong";

                                }
                                else
                                {
                                    message = "User created successfully";
                                    message = oLogin.RoleId.ToString();
                                }

                                oResponseModel = con.QuerySingle<LoginResponseModel>("SELECT Id, FirstName + ' ' + LastName AS FullName, RoleId, Email, Password, IsSubscribed, Logo FROM Users WHERE SocialId = @Id  and ProviderName=@Provider", oLogin);

                                oResponseModel.Token = CreateJwtToken(userRoles, usersID);

                            //}

                            //else
                            //{
                            //    message = "EmailID already exists";
                            //}
                        }

                        // ...

                        else
                        {
                            oResponseModel.Message = "Invalid login request";
                        }

                        oResponseModel.RoleName = userRole;
                        con.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                oResponseModel.Message = ex.Message;
            }

            return oResponseModel;
        }


        public UserModel GetUser(int id)
        {
            var user = new UserModel();
            using (System.Data.IDbConnection con = new SqlConnection(Global.ConnectionString))
            {
                if (con.State == System.Data.ConnectionState.Closed)
                {
                    con.Open();
                }
                user = con.QuerySingle<UserModel>("select Id,FirstName,LastName,MobileNo,Email from Users where Id =@Id", new { Id = id });
                if (con.State == System.Data.ConnectionState.Open)
                {
                    con.Close();
                }
            }
            return user;
        }

        public async Task<AccountSettingsDTO> GetUserAccountInfo(int UserID) 
        {

            AccountSettingsDTO result=new AccountSettingsDTO();
            using (System.Data.IDbConnection con = new SqlConnection(Global.ConnectionString))
            {
                if (con.State == System.Data.ConnectionState.Closed)
                {
                    con.Open();
                }

                result = con.QuerySingle<AccountSettingsDTO>($@"select Id UserId,FirstName Firstname,LastName Lastname,Logo,Email,MobileNo ,RoleId from Users where Id={UserID}");
                
                if (con.State == System.Data.ConnectionState.Open)
                {
                    con.Close();
                }
            }
            return result;

        }

        public async Task<string> UpdateUser(AccountSettingsDTO user)
        {
            string result = "";
            int ImageId = 0;
            using (System.Data.IDbConnection con = new SqlConnection(Global.ConnectionString))
            {
                if (con.State == System.Data.ConnectionState.Closed)
                {
                    con.Open();
                }
               
                var rowsAffected = con.Execute($"Update  Users set  FirstName=@Firstname, LastName=@Lastname,MobileNo=@MobileNo, Email=@Email,Logo=@Logo WHERE Id = @UserId", user);
                if (rowsAffected > 0)
                {
                    result = "Success";
                }
                else
                {
                    result = "Failed";
                }
                if (con.State == System.Data.ConnectionState.Open)
                {
                    con.Close();
                }
            }
            return result;
        }
    }
}
