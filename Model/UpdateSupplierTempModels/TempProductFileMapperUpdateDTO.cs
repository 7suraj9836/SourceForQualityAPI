using System;

namespace SourceforqualityAPI.Model.UpdateSupplierTempModels
{
    public class TempProductFileMapperUpdateDTO
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int FileId { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool IsDeleted { get; set; }
    }
}
