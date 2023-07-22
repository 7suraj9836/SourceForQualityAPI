using SourceforqualityAPI.Model;
using System.Threading.Tasks;

namespace SourceforqualityAPI.Interfaces
{
    public interface IAdminConfirmationToSuppliersServices
    {
        Task<string> AdminConfirmationToSuppliers(AdminConfirmationToSupplier status);
    }
}
