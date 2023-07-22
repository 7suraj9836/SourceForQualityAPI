using SourceforqualityAPI.Contracts;
using SourceforqualityAPI.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SourceforqualityAPI.Interfaces
{
    public interface ISupplierProfileServices
    {
        List<FoodSupplierGetDto> GetFoodSupplier(int pageNumber, int pageSize);
        FoodSupplierOutputDto GetFoodSupplierById(int id, int currentUserID);
        FoodSupplierOutputDto TempGetFoodSupplierById(int id);

        //Task<string> UpdateFoodSupplier(FoodSupplierUpdateInputDTO foodSupplier);
        Task<string> TempUpdateFoodSupplier(FoodSupplierUpdateInputDTO foodSupplier);
        Task<string> UpdateApprovedSupplier(int createdBy);
        //public  Task<string> UpdateSupplierProfile(FoodSupplierProfile foodSupplier);
        Task<string> SaveSupplierProfile(FoodSupplierProfile foodSupplier);
        //Task<string> TempSaveSupplierProfile(FoodSupplierProfile foodSupplier);

        Task<string> UpdateUserFavouriteSupplier(UserFavouriteSupplierDataModel inputModel);
        string DeleteFoodSupplier(int id);        
        string SubscriptionStatusActive(int id);
        string SubscriptionStatusInActive(int id);

    }
}
