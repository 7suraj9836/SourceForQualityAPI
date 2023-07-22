using System;

namespace SourceforqualityAPI.Model.UpdateSupplierTempModels
{
    public class TempQualificationUpdateDTO
    {
        public int Id { get; set; }
        public int SupplierProfileId { get; set; }
        public string Title { get; set; }
        public int CertificationsCategoryId { get; set; }
        public string OtherCertificationsCategoryName { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool IsDeleted { get; set; }
    }
}
