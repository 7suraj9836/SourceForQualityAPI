using System.ComponentModel.DataAnnotations;

namespace SourceforqualityAPI.Model
{
    
    public class AdminConfirmationToSupplier
    {
        public string Email { get; set; }
        public int ApprovalStatus { get; set; }
    }
}
