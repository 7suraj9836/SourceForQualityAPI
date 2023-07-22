using System;

namespace SourceforqualityAPI.Model.UpdateSupplierTempModels
{
    public class TempUploadImageFileMapperUpdateDTO
    {
        public int Id { get; set; }
        public int UploadImageId { get; set; }
        public int FileId { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool IsDeleted { get; set; }
    }
}
