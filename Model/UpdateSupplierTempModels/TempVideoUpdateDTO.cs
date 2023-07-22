using System;

namespace SourceforqualityAPI.Model.UpdateSupplierTempModels
{
    public class TempVideoUpdateDTO
    {
        public int Id { get; set; }
        public int SupplierProfileId { get; set; }
        public string Description { get; set; }
        public string Link { get; set; }

        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool IsDeleted { get; set; }
    }
}
