using SourceforqualityAPI.Contracts;
using SourceforqualityAPI.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SourceforqualityAPI.Interfaces
{
    public interface IDropdown
    {
        Task<List<DropdownResponse>> GetProductCategories();
        Task<List<DropdownResponse>> GetBusinessActivityCategories();
        Task<List<DropdownResponse>> GetCertificationCategories();
        Task<List<DropdownResponse>> GetCountries();
        Task<List<DropdownResponse>> GetFaqPages();
    }
}
