using Dapper;
using Optivem.Framework.Core.Application;
using SendGrid;
using SourceforqualityAPI.Common;
using SourceforqualityAPI.Interfaces;
using SourceforqualityAPI.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SourceforqualityAPI.Services
{
    public class ContactServices:IContactService
    {



        

        public List<Contacts> GetContacts(int pageNumber, int pageSize)
        {
            var Ins = new List<Contacts>();
            int offset = (pageNumber - 1) * pageSize; // Calculate the offset based on the page number and page size
            using (System.Data.IDbConnection con = new SqlConnection(Global.ConnectionString))
            {
                if (con.State == System.Data.ConnectionState.Closed)
                {
                    con.Open();
                }
              
                // Construct the SQL query based on the sort column and direction
                string sqlQuery = $"SELECT Id, Name, Subject, Email, Mobile, Message FROM contacts ORDER BY Name  OFFSET {offset} ROWS FETCH NEXT {pageSize} ROWS ONLY";
                Ins = con.Query<Contacts>(sqlQuery).ToList();
                if (con.State == System.Data.ConnectionState.Open)
                {
                    con.Close();
                }
            }
            return Ins;
        }




        public string SaveContact(Contacts contact)
        {
            string result = "";
            using (System.Data.IDbConnection con = new SqlConnection(Global.ConnectionString))
            {
                if (con.State == System.Data.ConnectionState.Closed)
                {
                    con.Open();

                }

                var sql = con.Execute("insert into contacts (  Name, Subject, Email, Mobile, Message) values(@Name,@Subject,@Email,@Mobile,@Message)", contact);


                if (sql > 0)
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
