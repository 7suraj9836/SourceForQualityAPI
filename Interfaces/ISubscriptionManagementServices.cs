using SourceforqualityAPI.Contracts;
using SourceforqualityAPI.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SourceforqualityAPI.Interfaces
{
    public interface ISubscriptionManagementServices
    {
        List<SubscriptionManagement> GetSupplierSubscriptionInfo(int pageSize, int pageNumber);
        Task<string> SaveSubscriptionInfo(SubcriptionManagementSaveDTO faq);
        SubscriptionManagement GetSubscriptionInfoById(int id);
        Task<string> UpdateSubscriptionInfo(SubcriptionManagementUpdateDTO faq);
        public string DeleteSubscriptionInfo(int Id);
    }
}
