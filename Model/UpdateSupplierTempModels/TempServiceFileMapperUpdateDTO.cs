using System;

namespace SourceforqualityAPI.Model.UpdateSupplierTempModels
{
    public class TempServiceFileMapperUpdateDTO
    {
        public int Id { get; set; }
        public int ServiceId { get; set; }
        public int FileId { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool IsDeleted { get; set; }
    }
}
