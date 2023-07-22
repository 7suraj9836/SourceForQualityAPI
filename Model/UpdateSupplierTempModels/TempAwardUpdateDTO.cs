using System;

namespace SourceforqualityAPI.Model.UpdateSupplierTempModels
{
    public class TempAwardUpdateDTO
    {
        public int Id { get; set; }
        public int SupplierProfileId { get; set; }
        public string Title { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool IsDeleted { get; set; }
    }

    
}
