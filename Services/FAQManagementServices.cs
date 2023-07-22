using Dapper;
using iTextSharp.text;
using SourceforqualityAPI.Common;
using SourceforqualityAPI.Interfaces;
using SourceforqualityAPI.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace SourceforqualityAPI.Services
{
    public class FAQManagementServices:IFAQManagementServices
    {


        public  List<FAQManagement> GetFAQ(int pageSize, int pageNumber)
        {
            var res = new List<FAQManagement>();
            int offset = (pageNumber - 1) * pageSize;
            using (System.Data.IDbConnection con = new SqlConnection(Global.ConnectionString))
            {
                if (con.State == System.Data.ConnectionState.Closed)
                {
                    con.Open();
                }

                res = con.Query<FAQManagement>("select Id,Page,Question,Answer from FaqManagement").ToList();

                if (con.State == System.Data.ConnectionState.Open)
                {
                    con.Close();
                }

            }
            return res;

        }
        
        
        public async Task<string> SaveFAQ(FAQManagement faq)
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

                    var rowsAffected = con.Execute("insert into FaqManagement(Page,Question,Answer) values(@Page,@Question,@Answer)", faq);

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


        public FAQManagement GetFAQById(int id)
        {
            var faqManagement = new FAQManagement();
            using (System.Data.IDbConnection con = new SqlConnection(Global.ConnectionString))
            {
                if (con.State == System.Data.ConnectionState.Closed)
                {
                    con.Open();
                }
                faqManagement = con.QuerySingle<FAQManagement>("Select Id, Page, Question, Answer from FaqManagement where Id =@Id", new { Id = id });
                if (con.State == System.Data.ConnectionState.Open)
                {
                    con.Close();
                }
            }
            return faqManagement;
        }


        public async Task<string> UpdateFAQ(FAQManagement faqManagement)
        {
            String result = "";
            using (System.Data.IDbConnection con = new SqlConnection(Global.ConnectionString))
            {
                if (con.State == System.Data.ConnectionState.Closed)
                {
                    con.Open();
                }
                var rowsAffected = con.Execute("Update  FaqManagement  set Page=@Page, Question=@Question, Answer=@Answer WHERE Id = @Id", faqManagement);
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


        public string DeleteFAQ(int id)
        {
            try
            {
               

                using (System.Data.IDbConnection con = new SqlConnection(Global.ConnectionString))
                {
                    if (con.State == System.Data.ConnectionState.Closed)
                    {
                        con.Open();
                    }
                    var rowsAffected = con.Execute("Delete From FaqManagement  where Id=@Id", new { Id = id });
                    if (rowsAffected == 0)
                    {
                        return "Question does not exist";

                    }
                }
                return "success";

            }
            catch (Exception ex)
            { return ex.Message; }
        }


    }
}
