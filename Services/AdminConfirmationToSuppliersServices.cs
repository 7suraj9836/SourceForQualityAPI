using SourceforqualityAPI.Common;
using System.Drawing;
using System.Net.Mail;
using System.Net;
using System;
using Dapper;
using System.Data.SqlClient;
using System.Threading.Tasks;
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



namespace SourceforqualityAPI.Services
{
    public class AdminConfirmationToSuppliersServices: IAdminConfirmationToSuppliersServices
    {
        private readonly ISupplierProfileServices _supplierProfileServices;

        public AdminConfirmationToSuppliersServices(ISupplierProfileServices supplierProfileServices)
        {
            _supplierProfileServices = supplierProfileServices;
        }

        public enum ApprovalStatus
        {
           Pending,
           Approved,
           Rejected
        }
        
        
        public async Task<string> AdminConfirmationToSuppliers (AdminConfirmationToSupplier status)
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

                    bool userExists = await con.ExecuteScalarAsync<bool>($"select count(1) from Users where Email = @value", new { value = status.Email });

                    if (userExists)
                    {
                       
                        var userData = con.QuerySingle($"select Id,FirstName from Users where Email = @value", new { value = status.Email });
                         var countSupplier = con.QuerySingle<int>($"select count(*) from SupplierProfiles where CreatedBy = @createdBy", new { createdBy = userData.Id });
                        if(countSupplier <= 0) {
                            throw new Exception("This user is not a supplier");
                        }

                        

                        if (userData.Id > 0 && (status.ApprovalStatus==1))
                        {

                            var supplierCount = con.QuerySingle<int>("select count(*) from Temp1_SupplierProfiles where CreatedBy=@CreatedBy;", new { CreatedBy = userData.Id });
                            
                            if (supplierCount > 0) {
                                try
                                {
                                    await _supplierProfileServices.UpdateApprovedSupplier(userData.Id);
                                }
                                catch (Exception ex)
                                {
                                    throw ex;
                                }
                            }
                            
                            // get the data from temporary table and insert in main table


                            /*string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Common/ReadUrl.txt");
                            string text = File.ReadAllText(path);*/
                            string text = "http://sourceforqualitydev.iworklab.com/";
                            MailMessage message = new MailMessage();
                            SmtpClient smtp = new SmtpClient();
                            message.From = new MailAddress(Global.SmtpFromEmail);
                            message.To.Add(status.Email);
                            message.Subject = "SourceForQuality -Admin Confirmation To Supplier";
                            message.IsBodyHtml = true;
                            message.Body = $"Hi {userData.FirstName} , your registration for Supplier has been Approved ";

                           
                            smtp.Port = Global.SmtpPort;
                            smtp.Host = Global.SmtpHost;
                            smtp.EnableSsl = true;
                            //smtp.UseDefaultCredentials = true;
                            smtp.Credentials = new NetworkCredential(Global.SmtpCredId, Global.SmtpCredPassword);
                            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                            smtp.Send(message);

                            int rowsaffected = con.Execute(@"update SupplierProfiles set ApprovalStatus=1 where CreatedBy=@Id", new { Id = userData.Id });
                            
                            //Should I Update the RoleId to 3 overhere?

                        }
                        else if (userData.Id > 0 && (status.ApprovalStatus == 2))
                        {
                            string text = "http://sourceforqualitydev.iworklab.com/";
                            MailMessage message = new MailMessage();
                            SmtpClient smtp = new SmtpClient();
                            message.From = new MailAddress(Global.SmtpFromEmail);
                            message.To.Add(status.Email);
                            message.Subject = "SourceForQuality -Admin Confirmation To Supplier";
                            message.IsBodyHtml = true;
                            message.Body = $"Hi {userData.FirstName} , your registration for Supplier has been Rejected ";


                            smtp.Port = Global.SmtpPort;
                            smtp.Host = Global.SmtpHost;
                            smtp.EnableSsl = true;
                            //smtp.UseDefaultCredentials = true;
                            smtp.Credentials = new NetworkCredential(Global.SmtpCredId, Global.SmtpCredPassword);
                            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                            smtp.Send(message);
                            int rowsaffected=  con.Execute(@"update SupplierProfiles set ApprovalStatus=2 where CreatedBy=@Id", new { Id = userData.Id });
                            //Should I Update the RoleId to 2 overhere?
                        }
                        else
                        {
                            int rowsaffected = con.Execute(@"update SupplierProfiles set ApprovalStatus=0 where CreatedBy=@Id", new { Id = userData.Id });
                        }
                        result = "Mail has been sent successfully. Please check your email.";

                       
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
    }
}

