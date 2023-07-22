using System;

namespace SourceforqualityAPI.Model.UpdateSupplierTempModels
{
    public class TempAwardFileMapperUpdateDTO
    {
        public int Id { get; set; }
        public int AwardId { get; set; }
        public int FileId { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool IsDeleted { get; set; }
    }
}
