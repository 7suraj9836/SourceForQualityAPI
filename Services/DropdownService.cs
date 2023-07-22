using Dapper;
using SourceforqualityAPI.Common;
using SourceforqualityAPI.Contracts;
using SourceforqualityAPI.Interfaces;
using SourceforqualityAPI.Model;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace SourceforqualityAPI.Services
{
    public class DropdownService : IDropdown
    {

        public async Task<List<DropdownResponse>> GetProductCategories()
        {
            var result = new List<DropdownResponse>();
        
            using (IDbConnection con = new SqlConnection(Global.ConnectionString))
            {
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }

             
                var query = $@" SELECT Id, Name  FROM ProductCatergory order by Name Asc ";

                result = con.Query<DropdownResponse>(query).ToList();

                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
            return result;
        }

        public async Task<List<DropdownResponse>> GetBusinessActivityCategories ()
        {
            var result = new List<DropdownResponse>();
   
            using (System.Data.IDbConnection con = new SqlConnection(Global.ConnectionString))
            {
                if (con.State == System.Data.ConnectionState.Closed)
                {
                    con.Open();
                }

                result = con.Query<DropdownResponse>($@"select Id,Name from [dbo].[BusinessActivityCategories ] order by Name Asc").ToList();

                if (con.State == System.Data.ConnectionState.Open)
                {
                    con.Close();
                }
            }
            return result;
        }

        public async Task<List<DropdownResponse>> GetCertificationCategories()
        {
            var result = new List<DropdownResponse>();
          
            using (System.Data.IDbConnection con = new SqlConnection(Global.ConnectionString))
            {
                if (con.State == System.Data.ConnectionState.Closed)
                {
                    con.Open();
                }

                result = con.Query<DropdownResponse>($@"select Id,Name from CertificationsCategories order by Name Asc ").ToList();

                if (con.State == System.Data.ConnectionState.Open)
                {
                    con.Close();
                }
            }
            return result;
        }

        public async Task<List<DropdownResponse>> GetCountries()
        {
            var result = new List<DropdownResponse>();

            using (System.Data.IDbConnection con = new SqlConnection(Global.ConnectionString))
            {
                if (con.State == System.Data.ConnectionState.Closed)
                {
                    con.Open();
                }

                result = con.Query<DropdownResponse>($@"select Id,Name from Countries order by Name Asc").ToList();

                if (con.State == System.Data.ConnectionState.Open)
                {
                    con.Close();
                }
            }
            return result;
        }

        public async Task<List<DropdownResponse>> GetFaqPages()
        {
            var result = new List<DropdownResponse>();

            using (System.Data.IDbConnection con = new SqlConnection(Global.ConnectionString))
            {
                if (con.State == System.Data.ConnectionState.Closed)
                {
                    con.Open();
                }

                result = con.Query<DropdownResponse>($@"select Id,Name from FaqPages order by Name Asc").ToList();

                if (con.State == System.Data.ConnectionState.Open)
                {
                    con.Close();
                }
            }
            return result;
        }
    }
}
