using Dapper;
using SourceforqualityAPI.Common;
using SourceforqualityAPI.Contracts;
using SourceforqualityAPI.Model;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace SourceforqualityAPI.Services
{
    public class SupplierFavouriteSearchService
    {
        public async Task<List<FoodSupplierSearchOutput>> GetFoodSupplierByFilter(SupplierSearchFilter searchInputDTO)
        {
            /* List<FoodSupplierProfile>*/
            var foodSupplierProfile = new List<FoodSupplierSearchOutput>();

            using (System.Data.IDbConnection con = new SqlConnection(Global.ConnectionString))
            {
                if (con.State == System.Data.ConnectionState.Closed)
                {
                    con.Open();
                }

                foodSupplierProfile = con.Query<FoodSupplierSearchOutput>($"exec Supplierprofile_SearchResult @SearchedBy,  @SearchValue,  @ProductCategoryId,@BusinessActivityCategoryId,@CertificationCategoryId,@CountryId,@PageNumber,@PageSize,@SortOrder ", searchInputDTO).ToList();

                if (con.State == System.Data.ConnectionState.Open)
                {
                    con.Close();
                }
            }
            return foodSupplierProfile;
        }
    }
}
