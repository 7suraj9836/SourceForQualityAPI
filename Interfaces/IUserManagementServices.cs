using SourceforqualityAPI.Model;
using System.Collections.Generic;

namespace SourceforqualityAPI.Interfaces
{
    public interface IUserManagementServices
    {
        public List<UserManagement> GetUserManagement(int pageNumber, int pageSize/*, string type*/);
        string DeleteUser(int id);
        string UserAccountStatus(int id, string AccountStatus);
        string UserInActiveStatus(int id);
    }
}
