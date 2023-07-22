using Dapper;
using SourceforqualityAPI.Common;
using SourceforqualityAPI.Interfaces;
using SourceforqualityAPI.Model;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System;
using System.Linq;
using SourceforqualityAPI.Contracts;

namespace SourceforqualityAPI.Services
{
    public class SubscriptionManagementServices: ISubscriptionManagementServices
    {
        
        public List<SubscriptionManagement> GetSupplierSubscriptionInfo(int pageSize, int pageNumber)
        {
            var res = new List<SubscriptionManagement>();
            int offset = (pageNumber - 1) * pageSize;
            using (System.Data.IDbConnection con = new SqlConnection(Global.ConnectionString))
            {
                if (con.State == System.Data.ConnectionState.Closed)
                {
                    con.Open();
                }

                res = con.Query<SubscriptionManagement>("select Id,Name,Price,Validity,NumberOfPurchasers,Enable,Description,PopularRating,Benefits as BenefitsString from SubscriptionManagement").ToList().Select(x=>new SubscriptionManagement() { 
                    Id=x.Id,
                    Name=x.Name,
                    Price=x.Price,
                    Validity=x.Validity,
                    NumberOfPurchasers=x.NumberOfPurchasers,
                    Enable=x.Enable,
                    Description=x.Description,
                    PopularRating=x.PopularRating,
                    BenefitsString=x.BenefitsString,
                    Benefits=x.BenefitsString.Split(",")
                }).ToList();

                if (con.State == System.Data.ConnectionState.Open)
                {
                    con.Close();
                }

            }
            return res;

        }


        

        public async Task<string> SaveSubscriptionInfo(SubcriptionManagementSaveDTO faq)
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

                    var parameters = new
                    {
                        Name = faq.Name,
                        Validity = faq.Validity,
                        Price = faq.Price,
                        Benefits = string.Join(", ", faq.Benefits),
                        Enable = faq.Enable
                    };

                    var rowsAffected = await con.ExecuteAsync("INSERT INTO SubscriptionManagement (Name, Validity, Price, Benefits, Enable) VALUES (@Name, @Validity, @Price, @Benefits, @Enable)", parameters);

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
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }

            return result;
        }







        public SubscriptionManagement GetSubscriptionInfoById(int id)
        {
            var subscriptionManagement = new SubscriptionManagement();
            using (System.Data.IDbConnection con = new SqlConnection(Global.ConnectionString))
            {
                if (con.State == System.Data.ConnectionState.Closed)
                {
                    con.Open();
                }

                subscriptionManagement = con.Query<SubscriptionManagement>("select Id,Name,Price,Validity,NumberOfPurchasers,Description,PopularRating,Benefits as BenefitsString from SubscriptionManagement where Id = @Id", new { Id = id }).SingleOrDefault();

                if (subscriptionManagement != null)
                {
                    // Split the BenefitsString into an array of benefits
                    subscriptionManagement.Benefits = subscriptionManagement.BenefitsString?.Split(",");
                }

                if (con.State == System.Data.ConnectionState.Open)
                {
                    con.Close();
                }
            }
            return subscriptionManagement;
        }


       
        public async Task<string> UpdateSubscriptionInfo(SubcriptionManagementUpdateDTO updatedSubscription)
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

                    var parameters = new
                    {
                        Id = updatedSubscription.Id,
                        Name = updatedSubscription.Name,
                        Validity = updatedSubscription.Validity,
                        Price = updatedSubscription.Price,
                        Benefits = string.Join(", ", updatedSubscription.Benefits),
                        Enable = updatedSubscription.Enable
                    };

                    var rowsAffected = await con.ExecuteAsync(@"UPDATE SubscriptionManagement
                                                        SET Name = @Name,
                                                            Validity = @Validity,
                                                            Price = @Price,
                                                            Benefits = @Benefits,
                                                            Enable = @Enable
                                                        WHERE Id = @Id", parameters);

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
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }

            return result;
        }



        public string DeleteSubscriptionInfo(int id)
        {
            try
            {


                using (System.Data.IDbConnection con = new SqlConnection(Global.ConnectionString))
                {
                    if (con.State == System.Data.ConnectionState.Closed)
                    {
                        con.Open();
                    }
                    var rowsAffected = con.Execute("Delete From SubscriptionManagement  where Id=@Id", new { Id = id });
                    if (rowsAffected == 0)
                    {
                        return "Subscription does not exist";

                    }
                }
                return "success";

            }
            catch (Exception ex)
            { return ex.Message; }
        }

    }
}
