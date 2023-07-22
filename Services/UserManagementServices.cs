using Dapper;
using SourceforqualityAPI.Common;
using SourceforqualityAPI.Interfaces;
using SourceforqualityAPI.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace SourceforqualityAPI.Services
{
    public class UserManagementServices: IUserManagementServices
    {
        //public List<UserModel> GetUserManagement()
        //{
        //    var Ins = new List<UserModel>();
        //    using (System.Data.IDbConnection con = new SqlConnection(Global.ConnectionString))
        //    {
        //        if (con.State == System.Data.ConnectionState.Closed)
        //        {
        //            con.Open();
        //        }
        //        Ins = con.Query<UserModel>("SELECT Id, [FirstName], [LastName], [Email], [MobileNo], [CreatedOn], [UpdatedOn] ,IsActive from [dbo].[Users]").ToList();
        //        if (con.State == System.Data.ConnectionState.Open)
        //        {
        //            con.Close();
        //        }
        //    }
        //    return Ins;

        //}

        //public List<UserModel> GetUserManagement(int pageNumber, int pageSize)
        //{
        //    var users = new List<UserModel>();
        //    using (System.Data.IDbConnection con = new SqlConnection(Global.ConnectionString))
        //    {
        //        if (con.State == System.Data.ConnectionState.Closed)
        //        {
        //            con.Open();
        //        }

        //        // Calculate the offset based on the page number and page size
        //        int offset = (pageNumber - 1) * pageSize;

        //        // Fetch only the required number of records using the OFFSET-FETCH clause
        //        string query = $"SELECT Id, [FirstName], [LastName], [Email], [MobileNo], [CreatedOn], [UpdatedOn], IsActive FROM [dbo].[Users] ORDER BY Id OFFSET {offset} ROWS FETCH NEXT {pageSize} ROWS ONLY";
        //        users = con.Query<UserModel>(query).ToList();

        //        if (con.State == System.Data.ConnectionState.Open)
        //        {
        //            con.Close();
        //        }
        //    }
        //    return users;
        //}

        public List<UserManagement> GetUserManagement(int pageNumber, int pageSize/*, string type*//*, string sortField, string sortDirection*/)
        {
            var Ins = new List<UserManagement>();
            using (System.Data.IDbConnection con = new SqlConnection(Global.ConnectionString))
            {
                if (con.State == System.Data.ConnectionState.Closed)
                {
                    con.Open();
                }
                var offset = (pageNumber - 1) * pageSize;
                var query = $"SELECT Id, [FirstName], [LastName], [Email], [MobileNo], [CreatedOn], [UpdatedOn], IsActive FROM [dbo].[Users] ORDER BY [FirstName] ASC OFFSET {offset} ROWS FETCH NEXT {pageSize} ROWS ONLY";
                Ins = con.Query<UserManagement>(query).ToList();
                if (con.State == System.Data.ConnectionState.Open)
                {
                    con.Close();
                }
            }
            return Ins;
        }




        public string DeleteUser(int id)
        {
            try
            {

                using (System.Data.IDbConnection con = new SqlConnection(Global.ConnectionString))
                {
                    if (con.State == System.Data.ConnectionState.Closed)
                    {
                        con.Open();
                    }
                    var rowsAffected = con.Execute("Delete From [dbo].[Users]  where Id=@Id", new { Id = id });
                    if (rowsAffected ==0)
                    {
                        return "User does not exist";
                        
                    }
                }
                return "success";

            }
            catch (Exception ex)
            { return ex.Message; }
        }

       public string UserAccountStatus(int id, string AccountStatus)
        {
            try
            {

                using (System.Data.IDbConnection con = new SqlConnection(Global.ConnectionString))
                {
                    if (con.State == System.Data.ConnectionState.Closed)
                    {
                        con.Open();
                    }
                    if (AccountStatus == "Active") {
                        var rowsAffected = con.Execute("UPDATE [dbo].[Users] SET IsActive=1 where Id=@id", new { Id = id });
                        if (rowsAffected > 0)
                        {
                            return "User status Is Active Now";

                        }
                    }
                    else if (AccountStatus == "InActive")
                    {
                        var rowsAffected = con.Execute("UPDATE [dbo].[Users] SET IsActive=0 where Id=@id", new { Id = id });
                        if (rowsAffected > 0)
                        {
                            return "User status is InActive now";

                        }
                    }
                    
                }
                return "success";

            }
            catch (Exception ex)
            { return ex.Message; }
        }

        public string UserInActiveStatus(int id)
        {
            try
            {

                using (System.Data.IDbConnection con = new SqlConnection(Global.ConnectionString))
                {
                    if (con.State == System.Data.ConnectionState.Closed)
                    {
                        con.Open();
                    }

                    var rowsAffected = con.Execute("UPDATE [dbo].[Users] SET IsActive=0 where Id=@id", new { Id = id });
                    if (rowsAffected == 0)
                    {
                        return "User status not changed";

                    }
                }
                return "success";

            }
            catch (Exception ex)
            { return ex.Message; }
        }
   


    }
}
