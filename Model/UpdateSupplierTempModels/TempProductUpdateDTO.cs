using System;

namespace SourceforqualityAPI.Model.UpdateSupplierTempModels
{
    public class TempProductUpdateDTO
    {
        public int Id { get; set; }
        public int SupplierProfileId { get; set; }
        public string Title { get; set; }
        public int ProductCategoryId { get; set; }
        public string OtherProductCategoryName { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool IsDeleted { get; set; }
    }
}
