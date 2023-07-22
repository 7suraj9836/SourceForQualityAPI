using Dapper;
using SourceforqualityAPI.Common;
using SourceforqualityAPI.Contracts;
using SourceforqualityAPI.Interfaces;
using SourceforqualityAPI.Model;
using System;
using System.Data.SqlClient;
using BCryptNet = BCrypt.Net.BCrypt;

namespace SourceforqualityAPI.Services
{
    public class ChangePasswordServices : IChangePasswordServices
    {
     public string ChangePassword(ChangePasswordDTO changePassword)
    {
        string result = "";
        using (System.Data.IDbConnection con = new SqlConnection(Global.ConnectionString))
        {
            if (con.State == System.Data.ConnectionState.Closed)
            {
                con.Open();
            }

            var Email = changePassword.Email;

            var ExistingPassword = con.QuerySingle<string>(@"select [Password] from Users where Email=@Email", changePassword);

            if (BCryptNet.Verify(changePassword.OldPassword, ExistingPassword) && changePassword.NewPassword == changePassword.ConfirmPassword)
            {
                var newPasswordHash = BCryptNet.HashPassword(changePassword.NewPassword);

                var rowsAffected = con.Execute(@"update Users set [Password]=@Password where Email=@Email", new { Password = newPasswordHash, Email = changePassword.Email });

                if (rowsAffected > 0)
                {
                    result = "Success";
                }
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
