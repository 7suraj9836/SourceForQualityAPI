using SourceforqualityAPI.Contracts;
using SourceforqualityAPI.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SourceforqualityAPI.Interfaces
{
    public interface ISupplierSearchService
    {

        Task<List<FoodSupplierSearchOutput>> GetFoodSupplierByFilter(SupplierSearchFilter searchInputDTO);
      

    }
}
