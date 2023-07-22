using iTextSharp.text;
using SourceforqualityAPI.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SourceforqualityAPI.Interfaces
{
    public interface IFAQManagementServices
    {

      
        List<FAQManagement> GetFAQ(int pageSize,int pageNumber);
        Task<string> SaveFAQ(FAQManagement faq);
        FAQManagement GetFAQById(int id);
        Task<string> UpdateFAQ(FAQManagement faq);
        public string DeleteFAQ(int Id);
    }
}
