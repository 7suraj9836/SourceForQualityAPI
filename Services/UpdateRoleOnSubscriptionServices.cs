using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using Org.BouncyCastle.Bcpg;
using SourceforqualityAPI.Common;
using SourceforqualityAPI.Contracts;
using SourceforqualityAPI.Interfaces;
using SourceforqualityAPI.Model;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace SourceforqualityAPI.Services
{
    public class UpdateRoleOnSubscriptionServices: IUpdateRoleOnSubscriptionServices
    {
        public async Task<AccountSettingsDTO> UpdateRoleOnSubscription(UpdateRoleOnSubscriptionDTO user)
        {
            string CheckSubscription = "";
            
            AccountSettingsDTO result = new AccountSettingsDTO();

            using (System.Data.IDbConnection con = new SqlConnection(Global.ConnectionString))
            {
                if (con.State == System.Data.ConnectionState.Closed)
                {
                    con.Open();

                }
                var UserID = user.UserId;
                if (user.SubscriptionPlanId == 1) {
                 
                    var rowsAffected = con.Execute(@"update  Users set IsSubscribed=@IsSubscribed,RoleId=2,SubscriptionPlanId=@SubscriptionPlanId where Id=@UserId", new { UserId = user.UserId, IsSubscribed = true, SubscriptionPlanId = user.SubscriptionPlanId });
                   
                    result = con.QuerySingle<AccountSettingsDTO>($@"SELECT sm.Id AS SubscriptionPlanId, u.Id AS UserId,u.IsSubscribed ,u.FirstName, u.LastName, u.Logo, u.Email, u.MobileNo, u.RoleId FROM Users AS u INNER JOIN [dbo].[SubscriptionManagement] AS sm ON u.SubscriptionPlanId = sm.Id WHERE u.Id = {UserID} ");
                        if (rowsAffected > 0)
                    {
                        CheckSubscription = "Success";
                    }

                }
                else if(user.SubscriptionPlanId ==2||user.SubscriptionPlanId==3)
                {
                 
                    var rowsAffected = con.Execute(@"update  Users set IsSubscribed=@IsSubscribed,RoleId=3,SubscriptionPlanId=@SubscriptionPlanId where Id=@UserId ", new { UserId =user.UserId, IsSubscribed = true , SubscriptionPlanId =user.SubscriptionPlanId});
                    result = con.QuerySingle<AccountSettingsDTO>($@"SELECT sm.Id AS SubscriptionPlanId, u.Id AS UserId, u.FirstName, u.IsSubscribed,u.LastName, u.Logo, u.Email, u.MobileNo, u.RoleId FROM Users AS u INNER JOIN [dbo].[SubscriptionManagement] AS sm ON u.SubscriptionPlanId = sm.Id WHERE u.Id = {UserID}");
                    if (rowsAffected > 0)
                    {
                        CheckSubscription = "Success";
                    }
                }
                else
                {
                    CheckSubscription = "Failed";
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
