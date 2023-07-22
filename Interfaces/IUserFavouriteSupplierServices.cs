using SourceforqualityAPI.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SourceforqualityAPI.Interfaces
{
    public interface IUserFavouriteSupplierServices
    {
        Task<string> UserFavouriteSupplier(UserFavouriteSupplierDataModel userFavouriteSupplier);

        List<FavouriteSuppliersList> GetAllFavouriteSuppliers(SortFavSupplierByCategory favSupplier);

        //List<SortFavSupplierByCategoryOutputModel> GetAllFavouriteSuppliersBySortCategory(SortFavSupplierByCategory favSupplier);


    }
}
