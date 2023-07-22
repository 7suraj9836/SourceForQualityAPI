using Dapper;
using SourceforqualityAPI.Common;
using SourceforqualityAPI.Contracts;
using SourceforqualityAPI.Interfaces;
using SourceforqualityAPI.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceforqualityAPI.Services
{
    public class SupplierSearchService : ISupplierSearchService
    {
        public async Task<List<FoodSupplierSearchOutput>> GetFoodSupplierByFilter(SupplierSearchFilter searchInputDTO)
        {
            /* List<FoodSupplierProfile>*/
            var foodSupplierProfile = new List<FoodSupplierSearchOutput>();
            var IntermediateFoodSupplier = new List<IntermediateFoodSupplierSearch>();

            using (System.Data.IDbConnection con = new SqlConnection(Global.ConnectionString))
            {
                if (con.State == System.Data.ConnectionState.Closed)
                {
                    con.Open();
                }

                IntermediateFoodSupplier = con.Query<IntermediateFoodSupplierSearch>($"exec Supplierprofile_SearchResults @SearchedBy,  @SearchValue,  @ProductCategoryId,@BusinessActivityCategoryId,@CertificationCategoryId,@CountryId,@PageNumber,@PageSize,@SortOrder ",searchInputDTO).ToList();

                foodSupplierProfile = IntermediateFoodSupplier.Select(x => new FoodSupplierSearchOutput()
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    Logo = x.Logo,
                    UserId= x.CreatedBy,   
                    Locations = new Location() { Latitude=x.Latitude,Longitude=x.Longitude},
                    TotalCount= x.TotalCount,
                    IsFavourite=x.IsFavourite,
                }).ToList();
                


                if (con.State == System.Data.ConnectionState.Open)
                {
                    con.Close();
                }
            }
            return foodSupplierProfile;
        }

        
    }
}
