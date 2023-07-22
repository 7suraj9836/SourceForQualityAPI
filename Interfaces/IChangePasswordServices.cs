using SourceforqualityAPI.Contracts;
using SourceforqualityAPI.Model;

namespace SourceforqualityAPI.Interfaces
{
    public interface IChangePasswordServices
    {
        string ChangePassword(ChangePasswordDTO contact);
    }
}
