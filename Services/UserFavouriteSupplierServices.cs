using Dapper;
using SourceforqualityAPI.Common;
using SourceforqualityAPI.Model;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System;
using SourceforqualityAPI.Interfaces;
using System.Collections.Generic;
using System.Linq;
using SourceforqualityAPI.Contracts;

namespace SourceforqualityAPI.Services
{
    public class UserFavouriteSupplierServices: IUserFavouriteSupplierServices
    {
       
        public List<FavouriteSuppliersList> GetAllFavouriteSuppliers(SortFavSupplierByCategory favSupplier)
        {
            var res = new List<FavouriteSuppliersList>();
            //int offset = (pageNumber - 1) * pageSize;
            using (System.Data.IDbConnection con = new SqlConnection(Global.ConnectionString))
            {
                if (con.State == System.Data.ConnectionState.Closed)
                {
                    con.Open();
                }

                res = con.Query<FavouriteSuppliersList>($"exec User_Favsupplier_Search @userid, @sortBy ", favSupplier).ToList();

                if (con.State == System.Data.ConnectionState.Open)
                {
                    con.Close();
                }

            }
            return res; 

        }

      //  new { UserId = userId}
    //SELECT sp.Id, sp.Name Name, sp.Description Description, sp.Logo Logo FROM[dbo].[SupplierProfiles] sp INNER JOIN UserFavouriteSuppliers ufs ON sp.[Id] = ufs.[SupplierId]  WHERE ufs.UserId= @UserId

    //public List<SortFavSupplierByCategoryOutputModel> GetAllFavouriteSuppliersBySortCategory(SortFavSupplierByCategory favSupplier )
    //    {
    //        var res = new List<SortFavSupplierByCategoryOutputModel>();
           
    //        using (System.Data.IDbConnection con = new SqlConnection(Global.ConnectionString))
    //        {
    //            if (con.State == System.Data.ConnectionState.Closed)
    //            {
    //                con.Open();
    //            }

    //            //res = con.Query<FavouriteSuppliersList>(" SELECT sp.Id,sp.Name Name,sp.Description Description,sp.Logo Logo FROM [dbo].[SupplierProfiles] sp INNER JOIN UserFavouriteSuppliers ufs ON sp.[Id] = ufs.[SupplierId]  WHERE ufs.UserId=@UserId", new { UserId = userId }).ToList();
    //            res = con.Query<SortFavSupplierByCategoryOutputModel>($"exec User_Favsupplier_Search @userid, @sortBy ", favSupplier).ToList();


    //            if (con.State == System.Data.ConnectionState.Open)
    //            {
    //                con.Close();
    //            }

    //        }
    //        return res;

    //    }




        public async Task<string> UserFavouriteSupplier(UserFavouriteSupplierDataModel userFavouriteSupplier)
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

                    var rowsAffected = con.Execute($"exec  [dbo].[UserSuplierFavMarker] @UserId,@SupplierId ,@IsFavourite", userFavouriteSupplier);

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
    }

    

}


//into UserFavouriteSuppliers(UserId, SupplierId, IsFavourite, CreatedOn) values(@UserId, @SupplierId, @IsFavourite, @CreatedOn)