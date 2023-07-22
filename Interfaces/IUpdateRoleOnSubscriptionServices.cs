using SourceforqualityAPI.Contracts;
using System.Threading.Tasks;

namespace SourceforqualityAPI.Interfaces
{
    public interface IUpdateRoleOnSubscriptionServices
    {
         Task<AccountSettingsDTO> UpdateRoleOnSubscription(UpdateRoleOnSubscriptionDTO user);
    }
}
