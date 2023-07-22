using System;

namespace SourceforqualityAPI.Model.UpdateSupplierTempModels
{
    public class TempQualificationFileMapperUpdateDTO
    {
        public int Id { get; set; }
        public int QualificationId { get; set; }
        public int FileId { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool IsDeleted { get; set; }
    }
}
